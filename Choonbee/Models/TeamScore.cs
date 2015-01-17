using System.Collections.Generic;

namespace Choonbee.Models
{
    public class TeamScore
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public int Score { get; set; }
        public int SchoolId { get; set; }
        public string SchoolName { get; set; }
        public Dictionary<int, ParticipantScore> Participants { get; set; }

        public TeamScore()
        {
            Participants = new Dictionary<int, ParticipantScore>();
        }
    }
}