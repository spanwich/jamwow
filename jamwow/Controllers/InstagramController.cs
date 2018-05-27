using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Net.Http;
using System.Web.Http;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using jamwow.Models;
using System.IO;
using System.Threading;
using NReco.VideoConverter;

namespace jamwow.Controllers
{
    public class InstagramController : ApiController
    {
        public InstagramController()
        {
        }

        public HttpResponseMessage Get(string link)
        {
            ChromeOptions options = new ChromeOptions();
            // if you like to specify another profile
            //options.AddArguments("headless");
            options.AddArguments("user-data-dir=/root/Downloads/aaa");
            options.AddArguments("start-maximized");
            ChromeDriver driver = new ChromeDriver(options);
            driver.Navigate().GoToUrl("https://www.instagram.com" + link);

            string videoSource = "";
            IWebElement webElement;

            webElement = driver.FindElementByClassName("_l6uaz");
            videoSource = webElement.GetAttribute("src");

            //Create a stream for the file
            Stream stream = null;

            driver.Dispose();

            //This controls how many bytes to read at a time and send to the client
            int bytesToRead = 40960;

            // Buffer to read bytes in chunk size specified above
            byte[] buffer = new Byte[bytesToRead];

            var fileName = link.Split('/').Last(element => !string.IsNullOrEmpty(element)).Split('?').FirstOrDefault();

            // The number of bytes read
            try
            {
                //Create a WebRequest to get the file
                HttpWebRequest fileReq = (HttpWebRequest)HttpWebRequest.Create(videoSource);
                fileReq.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:44.0) Gecko/20100101 Firefox/44.0";

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
                resp.AddHeader("Content-Length", fileResp.ContentLength.ToString());

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

            //Return complete video
            HttpResponseMessage fullResponse = Request.CreateResponse(HttpStatusCode.OK);
            //fullResponse.Content = new StreamContent(stream);
            //fullResponse.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4"); ;
            return fullResponse;
        }
    }
}
