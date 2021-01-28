using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Choonbee.Models
{
    public partial class Division
    {
        public void UpdateStatus(int DivisionStatusId)
        {
            var db2 = new ChoonbeeEdmx();
            this.DivisionStatusId = DivisionStatusId;
            db2.Entry(this).State = EntityState.Modified;
            db2.SaveChanges();
        }
    }
}