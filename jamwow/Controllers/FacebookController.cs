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
using HtmlAgilityPack;
using jamwow.Models;
using NReco.VideoConverter;
using System.Text;

namespace jamwow.Controllers
{
    public class FacebookController : ApiController
    {
        public FacebookController()
        {
        }

        public HttpResponseMessage Get(string link)
        {
            string downloadUrl = "";

            //Create a stream for the file
            Stream stream = null;

            var fileName = link.Split('/').Last(element => !string.IsNullOrEmpty(element));
            //    var fileName = "";
            try
            {
                BrowserSession b = new BrowserSession();
                b.Get("https://fbdown.net/");
                b.FormElements["URLz"] = link;
                string resp = b.Post("https://fbdown.net/download.php");

                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(resp);

                downloadUrl = System.Web.HttpUtility.HtmlDecode(
                    htmlDocument.DocumentNode.SelectNodes(
                        "//div[@id='result']/div[2]/div[2]/a"
                        ).LastOrDefault().Attributes["href"].Value);
            }
            catch(Exception e)
            {
                //Return complete video
                HttpResponseMessage errorResponse = Request.CreateResponse(HttpStatusCode.OK);
                errorResponse.Content = new StringContent(e.StackTrace);
                //fullResponse.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4"); ;
                return errorResponse;
            }

            #region oldcode
            //        ChromeOptions options = new ChromeOptions();
            //        // if you like to specify another profile
            //        //options.AddArguments("headless");
            //        options.AddArguments("user-data-dir=/root/Downloads/aaa");
            //        options.AddArguments("start-maximized");
            //        ChromeDriver driver = new ChromeDriver(options);
            //        Thread.Sleep(1000);
            //        driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 30);
            //        driver.Manage().Timeouts().PageLoad = new TimeSpan(0, 0, 10);
            //        driver.Navigate().GoToUrl("https://fbdown.net/");
            //        Thread.Sleep(5000);
            //        try
            //        {
            //            var urlZ = driver.FindElement(By.Name("URLz"));
            //            urlZ.SendKeys(link);
            //            urlZ.Submit();


            //            while (!new WebDriverWait(driver, new TimeSpan(0, 0, 2)).Until(
            //d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete")))
            //            {
            //                Thread.Sleep(500);
            //            }

            //            downloadUrl = driver.FindElement(By.XPath("//div[@id='result']/div[2]/div[2]/a")).GetAttribute("href");
            //        }
            //        catch (Exception e)
            //        {

            //        }
            //        finally
            //        {
            //            driver.Dispose();
            //        }
            #endregion

            if (string.IsNullOrEmpty(downloadUrl))
            {
                //Return Error video
                HttpResponseMessage errorResponse = Request.CreateResponse(HttpStatusCode.OK);
                errorResponse.Content = new StringContent("Cannot parse download link.");
                //fullResponse.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4"); ;
                return errorResponse;
            }

            try
            {

                //This controls how many bytes to read at a time and send to the client
                int bytesToRead = 40960;

                // Buffer to read bytes in chunk size specified above
                byte[] buffer = new Byte[bytesToRead];

                // The number of bytes read
                //Create a WebRequest to get the file
                HttpWebRequest fileReq = (HttpWebRequest)HttpWebRequest.Create(downloadUrl);

                //Create a response for this request
                HttpWebResponse fileResp = (HttpWebResponse)fileReq.GetResponse();

                if (fileReq.ContentLength > 0)
                    fileResp.ContentLength = fileReq.ContentLength;

                //Get the Stream returned from the response
                stream = fileResp.GetResponseStream();

                // prepare the response to the client. resp is the client Response
                var resp = HttpContext.Current.Response;

                //Indicate the type of data being sent
                //resp.ContentType = "application/octet-stream";
                resp.ContentType = "video/mp4";

                //Name the file 
                resp.AddHeader("Content-Disposition", "attachment; filename=\"" + fileName + ".mp4" + "\"");
                //            resp.AddHeader("Content-Length", stream.Length.ToString());

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
                //Return complete video
                HttpResponseMessage errorResponse = Request.CreateResponse(HttpStatusCode.OK);
                errorResponse.Content = new StringContent(e.StackTrace);
                //fullResponse.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4"); ;
                return errorResponse;
            }
            finally
            {
                if (stream != null)
                {
                    //Close the input stream
                    stream.Close();
                }
            }

            ////Return complete video
            //HttpResponseMessage fullResponse = Request.CreateResponse(HttpStatusCode.OK);
            ////fullResponse.Content = new StreamContent(stream);
            ////fullResponse.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4"); ;
            //return fullResponse;
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent("Download Complete");
            return response;
        }

        #region oldcode
        //public HttpResponseMessage Get(string link)
        //{
        //    List<string> cleanUrl = new List<string>();

        //    ChromeOptions options = new ChromeOptions();
        //    // if you like to specify another profile
        //    //options.AddArguments("headless");
        //    options.AddArguments("user-data-dir=/root/Downloads/aaa");
        //    options.AddArguments("start-maximized");
        //    ChromeDriver driver = new ChromeDriver(options);
        //    driver.Navigate().GoToUrl(link);
        //    Thread.Sleep(100);
        //    //        new WebDriverWait(driver, new TimeSpan(0,0,30)).Until(
        //    //d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
        //    try
        //    {
        //        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
        //        string title = (string)js.ExecuteScript("return document.title");
        //        String scriptToExecute = "var entries = JSON.stringify(window.performance.getEntries()); return entries;";

        //        String netData = js.ExecuteScript(scriptToExecute).ToString().Replace("[]", "\"[]\"");

        //        List<JSPerfModel> listNetworkData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<JSPerfModel>>(netData);

        //        var listUrl = listNetworkData.Where(p => p.name.Contains("mp4")).Select(p => p.name).Distinct();

        //        List<string> listClean = new List<string>();

        //        foreach (string url in listUrl)
        //        {
        //            int index = url.LastIndexOf("&bytestart");
        //            if (index > 0)
        //            {
        //                var temp = url.Substring(0, index);
        //                if (!listUrl.Contains(temp))
        //                {
        //                    listClean.Add(temp); // or index + 1 to keep slash
        //                }
        //            }
        //        }
        //        var q = listClean.GroupBy(c => c)
        //          .Where(e => e.Count() > 2)
        //          .Select(g => g.Key)
        //          .ToList();

        //        cleanUrl = q.ToList();

        //        //var name = Newtonsoft.Json.JsonConvert.DeserializeObject<JSPerfModel>(s);
        //    }
        //    catch (Exception e)
        //    {

        //    }
        //    finally
        //    {
        //        driver.Dispose();
        //    }
        //    //Create a stream for the file
        //    Stream stream = null;

        //    //This controls how many bytes to read at a time and send to the client
        //    int bytesToRead = 40960;

        //    // Buffer to read bytes in chunk size specified above
        //    byte[] buffer = new Byte[bytesToRead];

        //    var videoName = link.Split('/').Last(element => !string.IsNullOrEmpty(element));
        //    var fileName = "";
        //    // The number of bytes read
        //    try
        //    {
        //        ////Create a WebRequest to get the file
        //        //HttpWebRequest fileReq = (HttpWebRequest)HttpWebRequest.Create(link);
        //        //fileReq.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:44.0) Gecko/20100101 Firefox/44.0";

        //        ////Create a response for this request
        //        //HttpWebResponse fileResp = (HttpWebResponse)fileReq.GetResponse();

        //        //if (fileReq.ContentLength > 0)
        //        //    fileResp.ContentLength = fileReq.ContentLength;

        //        var resp = HttpContext.Current.Response;

        //        if (cleanUrl.Count() > 1)
        //        {
        //            for (int i = 0; i < 2; i++)
        //            {
        //                WebClient client = new WebClient();
        //                string address = cleanUrl[i];
        //                // Save the file to desktop for debugging
        //                fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "temp\\" + videoName + "_" + i + ".mp4");
        //                client.DownloadFile(address, fileName);
        //            }


        //            var ffmpeg = new NReco.VideoConverter.FFMpegConverter();

        //            ffmpeg.ConvertMedia(
        //              new[] {
        //                new FFMpegInput(AppDomain.CurrentDomain.BaseDirectory + "temp\\" + videoName + "_0.mp4"),
        //                new FFMpegInput(AppDomain.CurrentDomain.BaseDirectory + "temp\\" + videoName + "_1.mp4")
        //              }, AppDomain.CurrentDomain.BaseDirectory + "temp\\" + videoName + ".mp4", null,
        //              new ConvertSettings()
        //              {
        //                  AudioCodec = "copy", // or audio codec for re-encoding 
        //                  VideoCodec = "copy", // or video codec for re-encoding
        //                  CustomOutputArgs = " -map 0:v:0 -map 1:a:0 "
        //              });

        //            ////Get the Stream returned from the response
        //            stream = File.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "temp\\" + videoName + ".mp4");

        //            // prepare the response to the client. resp is the client Response
        //            resp = HttpContext.Current.Response;

        //            //Indicate the type of data being sent
        //            //resp.ContentType = "application/octet-stream";
        //            resp.ContentType = "video/mp4";

        //            //Name the file 
        //            resp.AddHeader("Content-Disposition", "attachment; filename=\"" + videoName + ".mp4" + "\"");
        //            resp.AddHeader("Content-Length", stream.Length.ToString());
        //        }
        //        else
        //        {
        //            //Create a WebRequest to get the file
        //            HttpWebRequest fileReq = (HttpWebRequest)HttpWebRequest.Create(cleanUrl[0]);

        //            //Create a response for this request
        //            HttpWebResponse fileResp = (HttpWebResponse)fileReq.GetResponse();

        //            if (fileReq.ContentLength > 0)
        //                fileResp.ContentLength = fileReq.ContentLength;

        //            //Get the Stream returned from the response
        //            stream = fileResp.GetResponseStream();

        //            // prepare the response to the client. resp is the client Response
        //            resp = HttpContext.Current.Response;

        //            //Indicate the type of data being sent
        //            //resp.ContentType = "application/octet-stream";
        //            resp.ContentType = "video/mp4";

        //            //Name the file 
        //            resp.AddHeader("Content-Disposition", "attachment; filename=\"" + videoName + ".mp4" + "\"");
        //            resp.AddHeader("Content-Length", fileResp.ContentLength.ToString());
        //        }

        //        int length;
        //        do
        //        {
        //            // Verify that the client is connected.
        //            if (resp.IsClientConnected)
        //            {
        //                // Read data into the buffer.
        //                length = stream.Read(buffer, 0, bytesToRead);

        //                // and write it out to the response's output stream
        //                resp.OutputStream.Write(buffer, 0, length);

        //                // Flush the data
        //                resp.Flush();

        //                //Clear the buffer
        //                buffer = new Byte[bytesToRead];
        //            }
        //            else
        //            {
        //                // cancel the download if client has disconnected
        //                length = -1;
        //            }
        //        } while (length > 0); //Repeat until no data is read
        //    }
        //    catch (Exception e)
        //    {
        //        //Return complete video
        //        HttpResponseMessage errorResponse = Request.CreateResponse(HttpStatusCode.OK);
        //        errorResponse.Content = new StringContent(e.StackTrace);
        //        //fullResponse.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4"); ;
        //        return errorResponse;
        //    }
        //    finally
        //    {

        //        if (stream != null)
        //        {
        //            //Close the input stream
        //            stream.Close();
        //        }
        //    }

        //    //wait for file release
        //    Thread.Sleep(1000);

        //    for (int i = 0; i < 2; i++)
        //    {
        //        if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "temp\\" + videoName + "_" + i + ".mp4"))
        //        {
        //            File.Delete(AppDomain.CurrentDomain.BaseDirectory + "temp\\" + videoName + "_" + i + ".mp4");
        //        }
        //    }

        //    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "temp\\" + videoName + ".mp4"))
        //    {
        //        File.Delete(AppDomain.CurrentDomain.BaseDirectory + "temp\\" + videoName + ".mp4");
        //    }

        //    //Return complete video
        //    HttpResponseMessage fullResponse = Request.CreateResponse(HttpStatusCode.OK);
        //    //fullResponse.Content = new StreamContent(stream);
        //    //fullResponse.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4"); ;
        //    return fullResponse;
        //}
        #endregion
    }

    public class BrowserSession
    {
        private bool _isPost;
        private HtmlDocument _htmlDoc;

        /// <summary>
        /// System.Net.CookieCollection. Provides a collection container for instances of Cookie class 
        /// </summary>
        public CookieCollection Cookies { get; set; }

        /// <summary>
        /// Provide a key-value-pair collection of form elements 
        /// </summary>
        public FormElementCollection FormElements { get; set; }

        /// <summary>
        /// Makes a HTTP GET request to the given URL
        /// </summary>
        public string Get(string url)
        {
            _isPost = false;
            CreateWebRequestObject().Load(url);
            return _htmlDoc.DocumentNode.InnerHtml;
        }

        /// <summary>
        /// Makes a HTTP POST request to the given URL
        /// </summary>
        public string Post(string url)
        {
            _isPost = true;
            CreateWebRequestObject().Load(url, "POST");
            return _htmlDoc.DocumentNode.InnerHtml;
        }

        /// <summary>
        /// Creates the HtmlWeb object and initializes all event handlers. 
        /// </summary>
        private HtmlWeb CreateWebRequestObject()
        {
            HtmlWeb web = new HtmlWeb();
            web.UseCookies = true;
            web.PreRequest = new HtmlWeb.PreRequestHandler(OnPreRequest);
            web.PostResponse = new HtmlWeb.PostResponseHandler(OnAfterResponse);
            web.PreHandleDocument = new HtmlWeb.PreHandleDocumentHandler(OnPreHandleDocument);
            return web;
        }

        /// <summary>
        /// Event handler for HtmlWeb.PreRequestHandler. Occurs before an HTTP request is executed.
        /// </summary>
        protected bool OnPreRequest(HttpWebRequest request)
        {
            AddCookiesTo(request);               // Add cookies that were saved from previous requests
            if (_isPost) AddPostDataTo(request); // We only need to add post data on a POST request
            return true;
        }

        /// <summary>
        /// Event handler for HtmlWeb.PostResponseHandler. Occurs after a HTTP response is received
        /// </summary>
        protected void OnAfterResponse(HttpWebRequest request, HttpWebResponse response)
        {
            SaveCookiesFrom(response); // Save cookies for subsequent requests
        }

        /// <summary>
        /// Event handler for HtmlWeb.PreHandleDocumentHandler. Occurs before a HTML document is handled
        /// </summary>
        protected void OnPreHandleDocument(HtmlDocument document)
        {
            SaveHtmlDocument(document);
        }

        /// <summary>
        /// Assembles the Post data and attaches to the request object
        /// </summary>
        private void AddPostDataTo(HttpWebRequest request)
        {
            string payload = FormElements.AssemblePostPayload();
            byte[] buff = Encoding.UTF8.GetBytes(payload.ToCharArray());
            request.ContentLength = buff.Length;
            request.ContentType = "application/x-www-form-urlencoded";
            System.IO.Stream reqStream = request.GetRequestStream();
            reqStream.Write(buff, 0, buff.Length);
        }

        /// <summary>
        /// Add cookies to the request object
        /// </summary>
        private void AddCookiesTo(HttpWebRequest request)
        {
            if (Cookies != null && Cookies.Count > 0)
            {
                request.CookieContainer.Add(Cookies);
            }
        }

        /// <summary>
        /// Saves cookies from the response object to the local CookieCollection object
        /// </summary>
        private void SaveCookiesFrom(HttpWebResponse response)
        {
            if (response.Cookies.Count > 0)
            {
                if (Cookies == null) Cookies = new CookieCollection();
                Cookies.Add(response.Cookies);
            }
        }

        /// <summary>
        /// Saves the form elements collection by parsing the HTML document
        /// </summary>
        private void SaveHtmlDocument(HtmlDocument document)
        {
            _htmlDoc = document;
            FormElements = new FormElementCollection(_htmlDoc);
        }
    }
    /// <summary>
    /// Represents a combined list and collection of Form Elements.
    /// </summary>
    public class FormElementCollection : Dictionary<string, string>
    {
        /// <summary>
        /// Constructor. Parses the HtmlDocument to get all form input elements. 
        /// </summary>
        public FormElementCollection(HtmlDocument htmlDoc)
        {
            var inputs = htmlDoc.DocumentNode.Descendants("input");
            foreach (var element in inputs)
            {
                string name = element.GetAttributeValue("name", "undefined");
                string value = element.GetAttributeValue("value", "");
                if (!name.Equals("undefined")) Add(name, value);
            }
        }

        /// <summary>
        /// Assembles all form elements and values to POST. Also html encodes the values.  
        /// </summary>
        public string AssemblePostPayload()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var element in this)
            {
                string value = System.Web.HttpUtility.UrlEncode(element.Value);
                sb.Append("&" + element.Key + "=" + value);
            }
            return sb.ToString().Substring(1);
        }
    }
}
