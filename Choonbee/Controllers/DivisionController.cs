using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Choonbee.Models;

namespace Choonbee.Controllers
{
    public class DivisionController : Controller
    {
        private ChoonbeeEdmx db = new ChoonbeeEdmx();
        //
        // GET: /Division/

        public ActionResult Index()
        {
            
            return View();
        }


        public ActionResult Participants(int id = 0)
        {
            var division = db.Divisions.Find(id);
            var hasDivisionParticipants = division.DivisionParticipants != null && division.DivisionParticipants.Count > 0;
            IEnumerable<DivisionParticipant> divisionParticipants = null;

            ViewBag.Divisionid = id;
            ViewBag.Heading = division.FriendlyId + " " + division.AdditionalidentifierText + " (" + division.DivisionStatus.Name + ") - " + division.Tournament.Name + " " + division.Tournament.DateHeld;
            ViewBag.TournamentId = division.TournamentId;

            if (hasDivisionParticipants)
            {
                divisionParticipants = division.DivisionParticipants.ToList();
                var ret = new LinkedList<Participant>();

                foreach (var divisionParticipant in divisionParticipants)
                {
                    ret.AddLast(divisionParticipant.Participant);
                }

                return View(ret);
            }

            else
                return View();
            
        }

        public ActionResult AddParticipant(int id = 0)
        {
            var division = db.Divisions.Find(id);
            var allEligibleParticipants = division.Tournament.Participants.Where(p => p.Age >= division.AgeGroup.MinAge && p.Age <= division.AgeGroup.MaxAge && (division.Genders == "E" || p.Gender == division.Genders) && division.RankGroup.Ranks.Contains(p.Rank)).ToList();
            var allAlreadyRegistered = division.DivisionParticipants.Select(p => p.ParticipantId);
            allEligibleParticipants = allEligibleParticipants.Where(p => !allAlreadyRegistered.Contains(p.ParticipantId)).ToList();
            
            ViewBag.AllParticipants = allEligibleParticipants.Select(r => new SelectListItem { Value = r.ParticipantId.ToString(), Text = r.FirstName + "," + r.LastName }).ToList();
            ViewBag.TournamentId = division.TournamentId;
            ViewBag.Divisionid = id;
            return View();
        }

        [HttpPost]
        public ActionResult AddParticipant(int id = 0, int AllParticipants = 0)
        {
            var registration = new DivisionParticipant();
            registration.DateEntered = DateTime.Now;
            registration.DivisionId = id;
            registration.ParticipantId = AllParticipants;

            db.DivisionParticipants.Add(registration);
            db.SaveChanges();

            return RedirectToAction("Participants", new { id = id });
        }

        public ActionResult RemoveParticipant(int id = 0, int participantId = 0)
        {
            var divisionRegistration = db.Divisions.Find(id).DivisionParticipants.Where(p => p.DivisionId == id && p.ParticipantId == participantId).First();
            db.DivisionParticipants.Remove(divisionRegistration);
            db.SaveChanges();
            return RedirectToAction("Participants", new { id = id });
        }

        public ActionResult Score(int id = 0)
        {
            var division = db.Divisions.Find(id);
            division.DivisionStatus = db.DivisionStatuses.Where(s => s.Name == "In Progress").First();
            db.SaveChanges();

            //TODO: oh god please fix this
            if (division.DivisionType.DivisionTypeId == 2 || division.DivisionType.DivisionTypeId == 3)
                return RedirectToAction("FormScoring", new { id = id });
            else
                return RedirectToAction("Create", "DivisionWinner", new { id = id });
        }

        public ActionResult FormScoring(int id = 0, bool showWinners = false)
        {
            /*attempt 1: only divisionparticipants
            var participants = (from d1 in db.Divisions
                                join d2 in db.DivisionParticipants on d1.DivisionId equals d2.DivisionId
                                join p1 in db.Participants on d2.ParticipantId equals p1.ParticipantId
                                select p1);
            */

            /*attempt 2: only returns results that has entries in DP and DFS
            var participants = (from dp in db.DivisionParticipants
                                join s1 in db.DivisionFormScores on dp.DivisionId equals s1.DivisionId
                                    into merging
                                from score in merging.DefaultIfEmpty(new DivisionFormScore { ParticipantId = 0, DivisionId = 0, Sum = 0})
                                
                                select new { score, dp });
             */

            /*attempt 3*/
            var thedict = getDivisionParticipantsAndScores(id);
            var dict = thedict.OrderByDescending(k => k.Key.ParticipantId);

            ViewBag.Participants = dict;
            ViewBag.Division = db.Divisions.Find(id);
            ViewBag.ParticipantCount = dict.Count();
            
            //ViewBag.WinnersArea
            if (showWinners)
            {
                //TODO: need to sort by score here!!!
                var winners = dict.OrderByDescending(k => k.Value.Sum).Take(4);
                var count = 1;
                var winnersArea = "<hr><h3>Proposed Winners</h3><table><tr><td><b>Full Name</b></td><td><b>Score</b></td><td><b>Rank</b></td></tr>";
                foreach (var participant in winners)
                {
                    //TODO: use stringbuilder
                    winnersArea += "<tr><td>" + participant.Key.FirstName + " " + participant.Key.LastName + "</td><td>" + participant.Value.Sum + "</td><td>" + count + "</td></tr>";
                    count++;
                }
                ViewBag.WinnersArea = winnersArea + "</table><h3><a href=\"/Division/CommitWinners/" + id + "\">Commit Winners (ARE YOU SURE?)</a></h3>";
            }
            else
            {
                ViewBag.WinnersArea = "<h3><a href=\"/Division/FormScoring/" + id + "?showWinners=true\">Show Winners</a></h3>";
            }
            return View();
        }

        private Dictionary<Participant, DivisionFormScore> getDivisionParticipantsAndScores(int id)
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

        //TODO: truncate DivisionWinners first?..
        public ActionResult CommitWinners(int id = 0)
        {
            var thedict = getDivisionParticipantsAndScores(id);
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
            return RedirectToAction("ViewDivisions", "Tournament", new { id = division.TournamentId });
        }


        public ActionResult Stage(int id = 0)
        {
            var division = db.Divisions.Find(id);
            division.DivisionStatus = db.DivisionStatuses.Where(s => s.Name == "Staged").First();
            db.SaveChanges();
            return RedirectToAction("ViewDivisions", "Tournament", new { id = division.TournamentId });
        }

        public ActionResult Close(int id = 0)
        {
            var division = db.Divisions.Find(id);
            division.DivisionStatus = db.DivisionStatuses.Where(s => s.Name == "Completed").First();
            db.SaveChanges();
            return RedirectToAction("ViewDivisions", "Tournament", new { id = division.TournamentId });
        }

        public ActionResult Split(int id)
        {
            var orgDivision = db.Divisions.Find(id);
            if (orgDivision != null && orgDivision.DivisionParticipants.Count >= 6)
            {
                var newDivision = new Division()
                {
                    AgeGroup = orgDivision.AgeGroup,
                    AdditionalidentifierText = orgDivision.AdditionalidentifierText,
                    RankGroup = orgDivision.RankGroup,
                    DateEntered = DateTime.Now,
                    DivisionStatus = orgDivision.DivisionStatus,
                    DivisionType = orgDivision.DivisionType,
                    FriendlyId = orgDivision.FriendlyId,
                    Genders = orgDivision.Genders,
                    HadTieBreaker = orgDivision.HadTieBreaker,
                    ParentDivisionId = orgDivision.ParentDivisionId,
                    Order = orgDivision.Order,
                    TournamentId = orgDivision.TournamentId
                };
                db.Divisions.Add(newDivision);
                db.SaveChanges();

                var orgDivisionParticipantsCount = orgDivision.DivisionParticipants.Count();
                var startIndex = (int) orgDivisionParticipantsCount / 2;
                var participantsToRemove = new List<DivisionParticipant>();
                for (var i = startIndex; i < orgDivisionParticipantsCount; i++)
                {
                    var participant = new DivisionParticipant() { DivisionId = newDivision.DivisionId, ParticipantId = orgDivision.DivisionParticipants.ToList()[i].ParticipantId };
                    //orgDivision.DivisionParticipants.ToList()[i];
                    newDivision.DivisionParticipants.Add(participant);
                    participantsToRemove.Add(orgDivision.DivisionParticipants.ToList()[i]);
                }
                //db.Entry(newDivision).State = EntityState.Modified;
                db.SaveChanges();

                foreach(var p in participantsToRemove)
                {
                    orgDivision.DivisionParticipants.Remove(p);
                }
                //db.Entry(orgDivision).State = EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("ViewDivisions", "Tournament", new { id = orgDivision.TournamentId });
        }
    }
}
