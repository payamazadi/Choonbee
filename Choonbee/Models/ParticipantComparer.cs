using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Choonbee.Models
{
    public class ParticipantComparer : IComparer<Participant>
    {
        public int Compare(Participant x, Participant y)
        {
            return getHeight(x.HeightFeet, x.HeightInches) - getHeight(y.HeightFeet, y.HeightInches);
        }

        public int getHeight(int feet, int inches){
            return 12 * feet + inches;
        }
    }
}