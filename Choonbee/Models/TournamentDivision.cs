using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Choonbee.Models
{
    public class TournamentDivision
    {
        public string FriendlyId { get; set; }
        public string RankGroupName { get; set; }
        public AgeGroup AgeGroup { get; set; }
        public string Genders { get; set; }
        public string DivisionStatusName { get; set; }
        public int DivisionId { get; set; }
        public int Order { get; set; }
        public int ParticipantCount { get; set; }
        public DivisionType DivisionType { get; set; }
        public int ParentDivisionId { get; set; }
    }
}