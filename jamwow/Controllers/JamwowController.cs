using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Owin;
using jamwow.Models;

namespace jamwow.Controllers
{
    //[Authorize]
    public class JamwowController : ApiController
    {
        public JamwowController()
        {
        }

        // GET api/Jamwow
        public GetViewModel Get()
        {
            return new GetViewModel() { Hometown = "Phayao" };
        }
    }
}