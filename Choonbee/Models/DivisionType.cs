//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Choonbee.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class DivisionType
    {
        public DivisionType()
        {
            this.DivisionDefaults = new HashSet<DivisionDefault>();
            this.Divisions = new HashSet<Division>();
        }
    
        public int DivisionTypeId { get; set; }
        public string Name { get; set; }
    
        public virtual ICollection<DivisionDefault> DivisionDefaults { get; set; }
        public virtual ICollection<Division> Divisions { get; set; }
    }
}
