using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Choonbee.Models
{
    public partial class TournamentTeam
    {
        private ChoonbeeEdmx db = new ChoonbeeEdmx();
        
        public int numParticipants
        {
            get
            {
                if (this.Participants != null)
                    return this.Participants.Count;
                else return 0;
            }

            set
            {
                return;
            }
            
        }

    }
}
