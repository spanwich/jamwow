using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.IO;
using System.Web;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Threading;
using OpenQA.Selenium.Support.UI;
using jamwow.Models;
using NReco.VideoConverter;

namespace jamwow.Controllers
{
    public class FacebookController : ApiController
    {
        public FacebookController()
        {
        }

        public HttpResponseMessage Get(string link)
        {
            List<string> cleanUrl = new List<string>();

            ChromeOptions options = new ChromeOptions();
            // if you like to specify another profile
            //options.AddArguments("headless");
            options.AddArguments("user-data-dir=/root/Downloads/aaa");
            options.AddArguments("start-maximized");
            ChromeDriver driver = new ChromeDriver(options);
            driver.Navigate().GoToUrl(link);
            Thread.Sleep(100);
            //        new WebDriverWait(driver, new TimeSpan(0,0,30)).Until(
            //d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
            try
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                string title = (string)js.ExecuteScript("return document.title");
                String scriptToExecute = "var entries = JSON.stringify(window.performance.getEntries()); return entries;";

                String netData = js.ExecuteScript(scriptToExecute).ToString().Replace("[]", "\"[]\"");

                List<JSPerfModel> listNetworkData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<JSPerfModel>>(netData);

                var listUrl = listNetworkData.Where(p => p.name.Contains("mp4")).Select(p => p.name).Distinct();

                List<string> listClean = new List<string>();

                foreach (string url in listUrl)
                {
                    int index = url.LastIndexOf("&bytestart");
                    if (index > 0)
                    {
                        var temp = url.Substring(0, index);
                        if (!listUrl.Contains(temp))
                        {
                            listClean.Add(temp); // or index + 1 to keep slash
                        }
                    }
                }
                var q = listClean.GroupBy(c => c)
                  .Where(e => e.Count() > 2)
                  .Select(g => g.Key)
                  .ToList();

                cleanUrl = q.ToList();

                //var name = Newtonsoft.Json.JsonConvert.DeserializeObject<JSPerfModel>(s);
            }
            catch (Exception e)
            {

            }
            finally
            {
                driver.Dispose();
            }
            //Create a stream for the file
            Stream stream = null;

            //This controls how many bytes to read at a time and send to the client
            int bytesToRead = 40960;

            // Buffer to read bytes in chunk size specified above
            byte[] buffer = new Byte[bytesToRead];

            var videoName = link.Split('/').Last(element => !string.IsNullOrEmpty(element));
            var fileName = "";
            // The number of bytes read
            try
            {
                ////Create a WebRequest to get the file
                //HttpWebRequest fileReq = (HttpWebRequest)HttpWebRequest.Create(link);
                //fileReq.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:44.0) Gecko/20100101 Firefox/44.0";

                ////Create a response for this request
                //HttpWebResponse fileResp = (HttpWebResponse)fileReq.GetResponse();

                //if (fileReq.ContentLength > 0)
                //    fileResp.ContentLength = fileReq.ContentLength;

                var resp = HttpContext.Current.Response;

                if (cleanUrl.Count() > 1)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        WebClient client = new WebClient();
                        string address = cleanUrl[i];
                        // Save the file to desktop for debugging
                        fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "temp\\" + videoName + "_" + i + ".mp4");
                        client.DownloadFile(address, fileName);
                    }


                    var ffmpeg = new NReco.VideoConverter.FFMpegConverter();

                    ffmpeg.ConvertMedia(
                      new[] {
                        new FFMpegInput(AppDomain.CurrentDomain.BaseDirectory + "temp\\" + videoName + "_0.mp4"),
                        new FFMpegInput(AppDomain.CurrentDomain.BaseDirectory + "temp\\" + videoName + "_1.mp4")
                      }, AppDomain.CurrentDomain.BaseDirectory + "temp\\" + videoName + ".mp4", null,
                      new ConvertSettings()
                      {
                          AudioCodec = "copy", // or audio codec for re-encoding 
                          VideoCodec = "copy", // or video codec for re-encoding
                          CustomOutputArgs = " -map 0:v:0 -map 1:a:0 "
                      });

                    ////Get the Stream returned from the response
                    stream = File.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "temp\\" + videoName + ".mp4");

                    // prepare the response to the client. resp is the client Response
                    resp = HttpContext.Current.Response;

                    //Indicate the type of data being sent
                    //resp.ContentType = "application/octet-stream";
                    resp.ContentType = "video/mp4";

                    //Name the file 
                    resp.AddHeader("Content-Disposition", "attachment; filename=\"" + videoName + ".mp4" + "\"");
                    resp.AddHeader("Content-Length", stream.Length.ToString());
                }
                else
                {
                    //Create a WebRequest to get the file
                    HttpWebRequest fileReq = (HttpWebRequest)HttpWebRequest.Create(cleanUrl[0]);

                    //Create a response for this request
                    HttpWebResponse fileResp = (HttpWebResponse)fileReq.GetResponse();

                    if (fileReq.ContentLength > 0)
                        fileResp.ContentLength = fileReq.ContentLength;

                    //Get the Stream returned from the response
                    stream = fileResp.GetResponseStream();

                    // prepare the response to the client. resp is the client Response
                    resp = HttpContext.Current.Response;

                    //Indicate the type of data being sent
                    //resp.ContentType = "application/octet-stream";
                    resp.ContentType = "video/mp4";

                    //Name the file 
                    resp.AddHeader("Content-Disposition", "attachment; filename=\"" + videoName + ".mp4" + "\"");
                    resp.AddHeader("Content-Length", fileResp.ContentLength.ToString());
                }

                int length;
                do
                {
                    // Verify that the client is connected.
                    if (resp.IsClientConnected)
                    {
                        // Read data into the buffer.
                        length = stream.Read(buffer, 0, bytesToRead);

                        // and write it out to the response's output stream
                        resp.OutputStream.Write(buffer, 0, length);

                        // Flush the data
                        resp.Flush();

                        //Clear the buffer
                        buffer = new Byte[bytesToRead];
                    }
                    else
                    {
                        // cancel the download if client has disconnected
                        length = -1;
                    }
                } while (length > 0); //Repeat until no data is read
            }
            catch (Exception e)
            {

            }
            finally
            {
                
                if (stream != null)
                {
                    //Close the input stream
                    stream.Close();
                }
            }

            //wait for file release
            Thread.Sleep(1000);

            for (int i = 0; i < 2; i++)
            {
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "temp\\" + videoName + "_" + i + ".mp4"))
                {
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "temp\\" + videoName + "_" + i + ".mp4");
                }
            }

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "temp\\" + videoName + ".mp4"))
            {
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + "temp\\" + videoName + ".mp4");
            }

            //Return complete video
            HttpResponseMessage fullResponse = Request.CreateResponse(HttpStatusCode.OK);
            //fullResponse.Content = new StreamContent(stream);
            //fullResponse.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4"); ;
            return fullResponse;
        }
    }
}
