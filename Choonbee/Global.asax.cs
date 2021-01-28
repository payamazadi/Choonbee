using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Optimization;
using Choonbee.Models;

namespace Choonbee
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            ModelBinders.Binders.Add(typeof(AgeGroup), new AgeGroupBinder());
            //ModelBinders.Binders.Add(typeof(School), new SchoolBinder());
            ModelBinders.Binders.Add(typeof(List<Participant>), new ParticipantListBinder());
            ModelBinders.Binders.Add(typeof(Division), new DivisionBinder());

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Session_Start(Object sender, EventArgs e)
        {
            Session["DivisionFilter"] = 0;
            Session["ToggleAdults"] = 1;
        }
    }
}