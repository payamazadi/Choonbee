using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Choonbee.Models;

namespace Choonbee.Controllers
{
    public static class ChoonbeeHelpers
    {
        private static ChoonbeeEdmx db = new ChoonbeeEdmx();

        public static Tournament getLatestTournament()
        {
            return db.Tournaments/*.Where(m => DateTime.Today <= m.DateHeld)*/.OrderBy(m => m.DateHeld).First();
        }

        public static string getLatestTournamentLinkHref()
        {
            return "/Tournament/Details/" + getLatestTournament().TournamentId;
        }

        public static ExpandoObject ToExpando(this object anonymousObject)
        {
            IDictionary<string, object> anonymousDictionary = new RouteValueDictionary(anonymousObject);
            IDictionary<string, object> expando = new ExpandoObject();
            foreach (var item in anonymousDictionary)
                expando.Add(item);
            return (ExpandoObject)expando;
        }

        public static int getPointsByRankOrder(int rank)
        {
            switch (rank)
            {
                case 1:
                    return 5;
                case 2:
                    return 4;
                case 3:
                    return 3;
                case 4:
                    return 1;
                default:
                    return 0;
            }
        }

        public static Dictionary<Participant, DivisionFormScore> getDivisionParticipantsAndScores(int id)
        {
            var participants = (from divisionParticipant in db.DivisionParticipants
                                from divisionPlayerScore in db.DivisionFormScores.Where(
                                     score => score.DivisionId == divisionParticipant.DivisionId
                                         && score.ParticipantId == divisionParticipant.ParticipantId).DefaultIfEmpty()
                                where divisionParticipant.DivisionId == id
                                select new { divisionParticipant = divisionParticipant, divisionPlayerScore }
                               );

            var merged = (from score in participants
                          join participant in db.Participants on score.divisionParticipant.ParticipantId equals participant.ParticipantId
                          select new { score, participant }
                          );

            var dictunsorted = new Dictionary<Participant, DivisionFormScore>();
            foreach (var item in merged)
            {
                var score = item.score.divisionPlayerScore;
                if (score == null)
                    score = new DivisionFormScore { JudgeOneScore = 0, JudgeTwoScore = 0, JudgeThreeScore = 0, Sum = 0 };
                dictunsorted.Add(item.participant, score);
            }

            return dictunsorted;
        }

        public static void CommitWinners(int id)
        {
            var thedict = ChoonbeeHelpers.getDivisionParticipantsAndScores(id);
            var dict = thedict.OrderByDescending(k => k.Value.Sum).Take(4);
            var division = db.Divisions.Find(id);

            var count = 1;
            foreach (var participantscore in dict)
            {
                var item = new DivisionWinner
                {
                    Division = division,
                    Participant = participantscore.Key,
                    Rank = count,
                    Points = ChoonbeeHelpers.getPointsByRankOrder(count)
                };
                count++;

                db.DivisionWinners.Add(item);
                db.SaveChanges();
            }

            division.DivisionStatus = db.DivisionStatuses.Where(s => s.Name == "Completed").First();
            db.SaveChanges();
        }

        public static string PrintWinnersTable(Dictionary<Participant, DivisionFormScore> dict, int id)
        {
            var winners = dict.OrderByDescending(k => k.Value.Sum).Take(4);
            var count = 1;
            var winnersArea = "<hr><h3>Proposed Winners</h3><table><tr><td><b>Full Name</b></td><td><b>Score</b></td><td><b>Rank</b></td></tr>";
            foreach (var participant in winners)
            {
                //TODO: use stringbuilder
                winnersArea += "<tr><td>" + participant.Key.FirstName + " " + participant.Key.LastName + "</td><td>" + participant.Value.Sum + "</td><td>" + count + "</td></tr>";
                count++;
            }
            return winnersArea + "</table><h3><a href=\"/Division/CommitWinners/" + id + "\">Commit Winners (ARE YOU SURE?)</a></h3>";
        }
    }
}
