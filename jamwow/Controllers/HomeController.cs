using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace jamwow.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult FacebookDownloader()
        {
            return View();
        }

        public ActionResult YoutubeDownloader()
        {
            return View();
        }

        public ActionResult InstagramDownloader()
        {
            return View();
        }
    }
}
