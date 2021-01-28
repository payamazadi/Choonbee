using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Helpers;
using Newtonsoft.Json;

namespace Choonbee.Models
{
    public class AgeGroupBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            HttpRequestBase request = controllerContext.HttpContext.Request;
            var temp = JsonConvert.DeserializeObject<AgeGroup>(request.Form.Get("theagegroup"));
            return temp;
        }
    }

    //public class SchoolBinder : DefaultModelBinder
    //{
    //    public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
    //    {
    //        HttpRequestBase request = controllerContext.HttpContext.Request;
    //        var temp = JsonConvert.DeserializeObject<School>(request.Form.Get("theschool"));
    //        return temp;
    //    }
    //}

    public class ParticipantListBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            HttpRequestBase request = controllerContext.HttpContext.Request;
            var temp = JsonConvert.DeserializeObject<List<Participant>>(request.Form.Get("theparticipantlist"));
            return temp;
        }
    }

    public class DivisionBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            HttpRequestBase request = controllerContext.HttpContext.Request;
            var temp = JsonConvert.DeserializeObject<Division>(request.Form.Get("thedivision"));
            return temp;
        }
    }
}