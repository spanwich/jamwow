using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Net.Http;
using System.Web.Http;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Threading.Tasks;
using jamwow.Models;
using System.IO;
using System.Threading;
using NReco.VideoConverter;

namespace jamwow.Controllers
{
    public class getIgContentController : ApiController
    {
        public getIgContentController()
        {
        }

        public async Task<IHttpActionResult> Get(string hashtag)
        {
            ChromeOptions options = new ChromeOptions();
            // if you like to specify another profile
            //options.AddArguments("headless");
            options.AddArguments("user-data-dir=/root/Downloads/aaa");
            options.AddArguments("start-maximized");
            ChromeDriver driver = new ChromeDriver(options);
            driver.Navigate().GoToUrl("https://www.instagram.com/explore/tags/" + hashtag + "/");

            var linkCollection = driver.FindElementsByTagName("a");

            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            for (int i = 0; i <= 20; i++)
            {
                js.ExecuteScript("window.scrollTo(0,88800);");
                Thread.Sleep(500);
            }

            var webElements = driver.FindElements(By.CssSelector("._6d3hm._mnav9")).ToList();

            List<string> sourceList = new List<string>();

            foreach(var webElement in  webElements)
            {
                sourceList.Add(webElement.GetAttribute("innerHTML"));
            }

            driver.Dispose();

            return Ok(sourceList);
        }
}
}
