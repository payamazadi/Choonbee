using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Choonbee.Models;

namespace Choonbee.Controllers
{
    public class HomeController : Controller
    {
        private ChoonbeeEdmx db = new ChoonbeeEdmx();

        //
        // GET: /Home/

        public ActionResult Index()
        {
            ViewBag.LatestTournamentLinkHref = ChoonbeeHelpers.getLatestTournamentLinkHref();
            return View();
        }

    }
}
