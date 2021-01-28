using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Choonbee.Models
{
    public class Match
    {
        /*
         * divisionid
         * person 1
         * person 2
         * winner
         * round
         * maxRounds
         */
        public int DivisionId { get; set; }
        public int PlayerAId { get; set; }
        public int PlayerBId { get; set; }
        public int WinnerId { get; set; }
        public int Round { get; set; }
        public int MaxRounds { get; set; }
    }
}