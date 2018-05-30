using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using System.Threading.Tasks;
using System.IO;
using YoutubeExtractor;
using NReco.VideoConverter;

namespace jamwow.Controllers
{
    public class YoutubeController : ApiController
    {
        public YoutubeController()
        {
        }

        public HttpResponseMessage Get(string link, string size)
        {
            /*
             * Get the available video formats.
             * We'll work with them in the video and audio download examples.
             */
            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link);

            //Create a stream for the file
            Stream stream = null;

            var fileName = AppDomain.CurrentDomain.BaseDirectory + "temp\\" + link.Split('=')[1] + DateTime.Now.ToFileTime();

            try
            {

                VideoInfo video = (size == "MP3")
                    ? videoInfos.First(info => info.Resolution <= 360)
                    : videoInfos.First(info => info.VideoType == VideoType.Mp4 && info.Resolution <= 720);

                /*
                 * If the video has a decrypted signature, decipher it
                 */
                if (video.RequiresDecryption)
                {
                    DownloadUrlResolver.DecryptDownloadUrl(video);
                }

                //This controls how many bytes to read at a time and send to the client
                int bytesToRead = 40960;

                // Buffer to read bytes in chunk size specified above
                byte[] buffer = new Byte[bytesToRead];

                // The number of bytes read
                //Create a WebRequest to get the file
                HttpWebRequest fileReq = (HttpWebRequest)HttpWebRequest.Create(video.DownloadUrl);

                //Create a response for this request
                HttpWebResponse fileResp = (HttpWebResponse)fileReq.GetResponse();

                if (fileReq.ContentLength > 0)
                    fileResp.ContentLength = fileReq.ContentLength;

                if (size == "MP3")
                {
                    WebClient client = new WebClient();
                    string address = video.DownloadUrl;
                    // Save the file to desktop for debugging
                    var loadedFileName = Path.Combine(fileName + ".mp4");
                    client.DownloadFile(address, loadedFileName);

                    var ffmpeg = new NReco.VideoConverter.FFMpegConverter();

                    ffmpeg.ConvertMedia(fileName + ".mp4"
                      , fileName + ".mp3", "mp3");

                    ////Get the Stream returned from the response
                    stream = File.OpenRead(fileName + ".mp3");
                }
                else
                {
                    //Get the Stream returned from the response
                    stream = fileResp.GetResponseStream();
                }

                // prepare the response to the client. resp is the client Response
                var resp = HttpContext.Current.Response;

                if (size == "MP3")
                {
                    resp.ContentType = "audio/mp3";
                    //Name the file 
                    resp.AddHeader("Content-Disposition", "attachment; filename=\"" + link.Split('=')[1] + ".mp3" + "\"");
                    resp.AddHeader("Content-Length", stream.Length.ToString());
                }
                else
                {
                    resp.ContentType = "video/mp4";
                    //Name the file 
                    resp.AddHeader("Content-Disposition", "attachment; filename=\"" + link.Split('=')[1] + ".mp4" + "\"");
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
                resp.Clear();
                resp.Redirect("http://" + HttpContext.Current.Request.Url.Authority + "/Home/YoutubeDownloader");
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
                if (File.Exists(fileName + ".mp4"))
                {
                    File.Delete(fileName + ".mp4");
                }
                if (File.Exists(fileName + ".mp3"))
                {
                    File.Delete(fileName + ".mp3");
                }
            }
            //Return complete video
            //Here i need to redirect to a aspx page.
            //var response = Request.CreateResponse(HttpStatusCode.Moved);
            //response.Headers.Location = new Uri("http://" + HttpContext.Current.Request.Url.Authority + "/Home/FacebookDownloader");

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            return response;

            //HttpResponseMessage fullResponse = Request.CreateResponse(HttpStatusCode.OK);
            ////fullResponse.Content = new StreamContent(stream);
            ////fullResponse.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4"); ;
            //return fullResponse;
        }
    }
}