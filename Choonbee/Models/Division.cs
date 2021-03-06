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
    
    public partial class Division
    {
        public Division()
        {
            this.DivisionFormScores = new HashSet<DivisionFormScore>();
            this.DivisionFormTieHistories = new HashSet<DivisionFormTieHistory>();
            this.DivisionFormTieHistories1 = new HashSet<DivisionFormTieHistory>();
            this.DivisionWinners = new HashSet<DivisionWinner>();
            this.DivisionParticipants = new HashSet<DivisionParticipant>();
            this.SparringEntries = new HashSet<SparringEntry>();
        }
    
        public int DivisionId { get; set; }
        public int TournamentId { get; set; }
        public int ParentDivisionId { get; set; }
        public string FriendlyId { get; set; }
        public int RankGroupId { get; set; }
        public int AgeGroupId { get; set; }
        public int DivisionTypeId { get; set; }
        public string Genders { get; set; }
        public string AdditionalidentifierText { get; set; }
        public bool HadTieBreaker { get; set; }
        public int DivisionStatusId { get; set; }
        public System.DateTime DateEntered { get; set; }
        public int Order { get; set; }
        public Nullable<int> NumSplits { get; set; }
    
        public virtual AgeGroup AgeGroup { get; set; }
        public virtual ICollection<DivisionFormScore> DivisionFormScores { get; set; }
        public virtual ICollection<DivisionFormTieHistory> DivisionFormTieHistories { get; set; }
        public virtual ICollection<DivisionFormTieHistory> DivisionFormTieHistories1 { get; set; }
        public virtual RankGroup RankGroup { get; set; }
        public virtual Tournament Tournament { get; set; }
        public virtual DivisionType DivisionType { get; set; }
        public virtual DivisionStatus DivisionStatus { get; set; }
        public virtual ICollection<DivisionWinner> DivisionWinners { get; set; }
        public virtual ICollection<DivisionParticipant> DivisionParticipants { get; set; }
        public virtual ICollection<SparringEntry> SparringEntries { get; set; }
    }
}
