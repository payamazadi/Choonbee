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
    
    public partial class DivisionWinner
    {
        public int DivisionId { get; set; }
        public int ParticipantId { get; set; }
        public int Rank { get; set; }
        public int Points { get; set; }
    
        public virtual Division Division { get; set; }
        public virtual Participant Participant { get; set; }
    }
}
