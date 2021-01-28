using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Choonbee.Models;
using Newtonsoft.Json;

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

            /* */
            IEnumerable<DivisionParticipant> divisionParticipants = null;

            ViewBag.Divisionid = id;
            ViewBag.Heading = division.FriendlyId + " " + division.AdditionalidentifierText + " (" + division.DivisionStatus.Name + ") - " + division.Tournament.Name + " " + division.Tournament.DateHeld;
            ViewBag.TournamentId = division.TournamentId;
            ViewBag.Sparring = false;

            var tournament = division.Tournament;
            ViewBag.AllParticipants = tournament.Participants.Select(r => new SelectListItem { Value = r.ParticipantId.ToString(), Text = r.FirstName + "," + r.LastName }).ToList().OrderBy(z => z.Text);


            if (hasDivisionParticipants)
            {
                ViewBag.Sparring = division.DivisionTypeId == 1;
                divisionParticipants = division.DivisionParticipants.ToList();
                var ret = new LinkedList<Participant>();

                foreach (var divisionParticipant in divisionParticipants)
                {
                    ret.AddLast(divisionParticipant.Participant);
                }

                if (division.DivisionTypeId == 1 || division.DivisionTypeId == 4)
                {
                    return View(ret.OrderBy(x => x, new ParticipantComparer()).ToList());
                }

                return View(ret.ToList());
            }

            else
                return View();
            
        }

        [HttpPost]
        public ActionResult Participants(int id = 0, int AllParticipants = 0)
        {
            var registration = new DivisionParticipant();
            registration.DateEntered = DateTime.Now;
            registration.DivisionId = id;
            registration.ParticipantId = AllParticipants;

            db.DivisionParticipants.Add(registration);
            db.SaveChanges();
            return RedirectToAction("Participants", new { id = id });
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
                return RedirectToAction("SparringScoring", new { id = id });
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
                var winners = dict.OrderByDescending(k => k.Value.Sum).Take(6);
                var count = 1;
                Dictionary<float, LinkedList<Participant>> scoresToParticipants = new Dictionary<float, LinkedList<Participant>>();
                var winnersArea = "<hr><h3>Proposed Winners</h3><table><tr><td><b>Rank</b></td><td><b>Score</b></td><td><b>Full Name(s)</b></td></tr>";
                
                foreach (var participant in winners)
                {
                    if (scoresToParticipants.ContainsKey(participant.Value.Sum) == false)
                        scoresToParticipants.Add(participant.Value.Sum, new LinkedList<Participant>());
                    scoresToParticipants[participant.Value.Sum].AddLast(participant.Key);
                }


                bool hasTie = false;
                foreach (var scoreToParticipants in scoresToParticipants)
                {
                    if(count > 4)
                        break;
                    String names = "";
                    winnersArea += "<tr><td>" + count + "</td><td>" + scoreToParticipants.Key;

                    foreach (var scoreParticipant in scoreToParticipants.Value)
                        names += scoreParticipant.FirstName + " " + scoreParticipant.LastName + ",";
                    names = names.Substring(0, names.Length - 1);

                    if (names.Contains(",") && !hasTie)
                    {
                        hasTie = true;
                        winnersArea += "<td bgcolor='red'>";
                    }
                    else
                        winnersArea += "<td>";

                    winnersArea += names + "</td></tr>";
                    count++;
                }

                /*
                foreach (var participant in winners)
                {
                    //TODO: use stringbuilder
                    winnersArea += "<tr><td>" + participant.Key.FirstName + " " + participant.Key.LastName + "</td><td>" + participant.Value.Sum + "</td><td>" + count + "</td></tr>";
                    count++;
                }
                 * */
                ViewBag.WinnersArea = winnersArea + "</table><h3><a href=\"/Division/CommitWinners/" + id + "\">Commit Winners (ARE YOU SURE?)</a></h3>";
            }
            else
            {
                ViewBag.WinnersArea = "<h3><a href=\"/Division/FormScoring/" + id + "?showWinners=true\">Show Winners <span style='background: red;'>(MUST DO THIS TO COMPLETE DIVISION)</span></a></h3>";
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
            return RedirectToAction("ViewDivisions", "Tournament", new { id = division.TournamentId});
        }


        public ActionResult Stage(int id = 0)
        {
            var division = db.Divisions.Find(id);

            division.DivisionStatus = db.DivisionStatuses.Where(s => s.Name == "Staged").First();
            db.SaveChanges();
            return RedirectToAction("ViewDivisions", "Tournament", new { id = division.TournamentId, DivisionType = Session["DivisionFilter"] });
        }

        public ActionResult Unstage(int id = 0)
        {
            var division = db.Divisions.Find(id);
            division.DivisionStatus = db.DivisionStatuses.Where(s => s.Name == "Open").First();
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

        [HttpGet]
        public ActionResult ManageBracket(int id)
        {
            var division = db.Divisions.Find(id);
            ViewBag.DivisionId = id;
            ViewBag.FriendlyId = division.FriendlyId;
            ViewBag.TournamentId = division.TournamentId;
            var ret = "<ul id=\"sortable\">";

            var divisionParticipants = division.DivisionParticipants;
            var divisionParticipantsCount = divisionParticipants.Count;
            var numByes = divisionParticipantsCount == 1 ? 0 : IsPow2(divisionParticipantsCount) ? 0 : ByLogs(divisionParticipantsCount) - divisionParticipantsCount;
            
            foreach (var participant in divisionParticipants)
            {
                ret += "<li id=\"" + participant.ParticipantId + "\">(" + participant.Participant.School.Name + ") " + participant.Participant.FirstName + " " + participant.Participant.LastName + "</li>";
            }

            for (int i = 0; i < numByes; i++)
            {
                ret += "<li id=\"0\">Bye</li>";
            }

            ret += "</ul>";
            ViewBag.Participants = ret;

            division.DivisionStatusId = 4;
            db.Entry(division).State = EntityState.Modified;
            db.SaveChanges();

            return View();
        }

        private int ByLogs(int n)
        {
            double y = Math.Floor(Math.Log(n, 2));

            return (int)Math.Pow(2, y + 1);
        }

        private bool IsPow2(int x)
        {
            return ((x != 0) && ((x & (~x + 1)) == x));
        }

        [HttpPost, ActionName("ManageBracket")]
        public ActionResult ManageBracket_Post(int id, List<string> participants)
        {
            var division = db.Divisions.Find(id);
            division.DivisionStatusId = 4;
            db.Entry(division).State = EntityState.Modified;
            db.SaveChanges();

            ViewBag.DivisionId = id;
            var count = 1;

            foreach (var participant in participants)
            {
                
                SparringEntry entry = new SparringEntry
                {
                    DivisionId = id,
                    ParticipantId = Convert.ToInt32(participant),
                    Completed = false,
                    Order = count,
                    Round = 1
                };
                db.SparringEntries.Add(entry);
                db.SaveChanges();
                
                count++;
            }
            return RedirectToAction("ViewDivisions", "Tournament", new { id = division.TournamentId });
        }

        public ActionResult SparringScoring(int id)
        {
            var match = getMatch(id);
            var division = db.Divisions.Find(id);
            ViewBag.Heading = division.RankGroup.Name + ", " + division.AgeGroup.Name + ", " + division.Genders;
            ViewBag.Completed = false;

            if (match == null)
            {
                division.DivisionStatusId = 3;
                db.Entry(division).State = EntityState.Modified;
                db.SaveChanges();


                ViewBag.Completed = true;
                var winnersTable = "<table><tr><td>Name</td><td>Rank</td><td>Points</td></tr>";

                var winners = from dw in db.DivisionWinners
                              join p in db.Participants on dw.ParticipantId equals p.ParticipantId
                              where dw.DivisionId == id
                              orderby dw.Rank
                              select new { P = p, Dw = dw };

                foreach (var winner in winners)
                {
                    winnersTable += "<tr><td>" + winner.P.FirstName + " " + winner.P.LastName + "</td><td>" + winner.Dw.Rank + "</td><td>" + winner.Dw.Points + "</td></tr>";
                }
                winnersTable += "</table>";
                ViewBag.WinnersTable = winnersTable;
            }
            else
            {
                ViewBag.Heading += " - Round " + match.Round;
                var playerA = db.Participants.Find(match.PlayerAId);
                var playerB = db.Participants.Find(match.PlayerBId);
                ViewBag.PlayerAName = playerA.FirstName + " " + playerA.LastName;
                ViewBag.PlayerBName = playerB.FirstName + " " + playerB.LastName;
            }
            return View(match);
        }

        private Match getMatch(int DivisionId)
        {
            var division = db.Divisions.Find(DivisionId);
            var entrants = db.SparringEntries.Where(se => se.DivisionId == DivisionId && se.Completed == false).OrderBy(se => se.Order).Take(2);
            var firstRoundSize = db.SparringEntries.Where(se => se.DivisionId == DivisionId && se.Round == 1).Count();
            var maxRounds = Convert.ToInt32(Math.Log(firstRoundSize, 2));
            var entrantCount = entrants.Count();

            Match x = new Match();

            if (entrantCount == 1)
            {
                if (entrants.First().ParticipantId == 0)
                {
                    Response.Write("FAILURE - only have a Bye!");
                    Response.Flush();
                    Response.End();
                }

                InjectWinner(DivisionId, entrants.First().ParticipantId, 1);
                ViewBag.Winner = db.Participants.Find(entrants.First().ParticipantId);
                return null;
            }
            else if (entrantCount == 2 && entrants.First().Round < maxRounds)
            {

                if (entrants.ToList()[0].ParticipantId == 0 || entrants.ToList()[1].ParticipantId == 0)
                {
                    var nonBye = entrants.ToList()[0];
                    var theBye = entrants.ToList()[1];
                    CompleteEntry(theBye);
                    CompleteEntry(nonBye);

                    var newMaxOrder = getNewMaxOrder(DivisionId);

                    SparringEntry entry = new SparringEntry
                    {
                        DivisionId = DivisionId,
                        ParticipantId = nonBye.ParticipantId,
                        Completed = false,
                        Order = newMaxOrder,
                        Round = nonBye.Round + 1
                    };
                    db.SparringEntries.Add(entry);
                    db.SaveChanges();

                    return getMatch(DivisionId);
                }
                else
                {
                    x.PlayerAId = entrants.ToList()[0].ParticipantId;
                    x.PlayerBId = entrants.ToList()[1].ParticipantId;
                    x.Round = entrants.First().Round;
                    x.WinnerId = 0;
                    x.MaxRounds = maxRounds;
                    x.DivisionId = DivisionId;
                }
            }
            else if (entrantCount == 2 && entrants.First().Round == maxRounds)
            {
                var round = entrants.First().Round;
                var participant1id = entrants.ToList()[0].ParticipantId;
                var participant2id = entrants.ToList()[1].ParticipantId;
                var runnerUps = from se in db.SparringEntries
                                where se.Completed &&
                                se.DivisionId == DivisionId &&
                                se.Round == round - 1 &&
                                se.ParticipantId != 0 &&
                                se.ParticipantId != participant1id && se.ParticipantId != participant2id
                                select se;
                if (round == 2 && runnerUps.Count() < 2)
                {
                    var entrant = runnerUps.First();
                    CompleteEntry(entrant);
                    var rank = maxRounds == 1 ? 1 : 3;
                    InjectWinner(DivisionId, entrant.ParticipantId, rank);
                    x.PlayerAId = entrants.ToList()[0].ParticipantId;
                    x.PlayerBId = entrants.ToList()[1].ParticipantId;
                    x.Round = entrants.First().Round;
                    x.WinnerId = 0;
                    x.MaxRounds = maxRounds;
                    x.DivisionId = DivisionId;
                    return x;
                }

                if (round == 1)
                {
                    x.PlayerAId = entrants.ToList()[0].ParticipantId;
                    x.PlayerBId = entrants.ToList()[1].ParticipantId;
                    x.Round = 1;
                    x.WinnerId = 0;
                    x.MaxRounds = maxRounds;
                    x.DivisionId = DivisionId;
                    return x;
                }

                var db2 = new ChoonbeeEdmx();

                if(db2.SparringEntries.Where(se => se.DivisionId == DivisionId && se.Order == 1000 && se.Completed == true).Any())
                {
                    x.PlayerAId = entrants.ToList()[0].ParticipantId;
                    x.PlayerBId = entrants.ToList()[1].ParticipantId;
                    x.Round = entrants.First().Round;
                    x.WinnerId = 0;
                    x.MaxRounds = maxRounds;
                    x.DivisionId = DivisionId;
                }
                else
                {
                    var count = 1000;
                    foreach (var runnerUp in runnerUps)
                    {
                        SparringEntry entry = new SparringEntry
                        {
                            DivisionId = DivisionId,
                            ParticipantId = runnerUp.ParticipantId,
                            Completed = false,
                            Order = count,
                            Round = maxRounds + 1
                        };
                        db2.SparringEntries.Add(entry);
                        db2.SaveChanges();
                        count++;
                    }

                    x.PlayerAId = runnerUps.ToList()[0].ParticipantId;
                    x.PlayerBId = runnerUps.ToList()[1].ParticipantId;
                    x.Round = maxRounds + 1;
                    x.WinnerId = 0;
                    x.MaxRounds = maxRounds;
                    x.DivisionId = DivisionId;
                }
                
            }
            else if (entrantCount == 0)
            {
                return null;
            }
            else
            {
                Response.Write("FAILURE!");
                Response.Flush();
                Response.End();
            }

            return x;
        }

        private void CompleteEntry(SparringEntry entry)
        {
            entry.Completed = true;
            db.Entry(entry).State = EntityState.Modified;
            db.SaveChanges();
        }


        private void saveMatch(Match match)
        {
            

            if (match.Round < match.MaxRounds)
            {
                var entryA = db.SparringEntries.Where(se => se.DivisionId == match.DivisionId && se.ParticipantId == match.PlayerAId && se.Completed == false).First();
                var entryB = db.SparringEntries.Where(se => se.DivisionId == match.DivisionId && se.ParticipantId == match.PlayerBId && se.Completed == false).First();
                CompleteEntry(entryA);
                CompleteEntry(entryB);

                var newMaxOrder = getNewMaxOrder(match.DivisionId);
                var newEntry = new SparringEntry
                {
                    DivisionId = match.DivisionId,
                    Order = newMaxOrder,
                    Round = match.Round + 1,
                    ParticipantId = match.WinnerId,
                    Completed = false,
                };

                db.SparringEntries.Add(newEntry);
                db.SaveChanges();
            }

            else if (match.Round > match.MaxRounds)
            {
                var entryA = db.SparringEntries.Where(se => se.DivisionId == match.DivisionId && se.ParticipantId == match.PlayerAId && se.Completed == false).OrderByDescending(se2 => se2.Order).First();
                var entryB = db.SparringEntries.Where(se => se.DivisionId == match.DivisionId && se.ParticipantId == match.PlayerBId && se.Completed == false).OrderByDescending(se2 => se2.Order).First();
                CompleteEntry(entryA);
                CompleteEntry(entryB);

                if(match.WinnerId == match.PlayerAId)
                {
                    InjectWinner(match.DivisionId, match.PlayerAId, 3);
                    InjectWinner(match.DivisionId, match.PlayerBId, 4);
                }
                else
                {
                    InjectWinner(match.DivisionId, match.PlayerBId, 3);
                    InjectWinner(match.DivisionId, match.PlayerAId, 4);
                }
            }
            else if (match.Round == match.MaxRounds)
            {
                var entryA = db.SparringEntries.Where(se => se.DivisionId == match.DivisionId && se.ParticipantId == match.PlayerAId && se.Completed == false).First();
                var entryB = db.SparringEntries.Where(se => se.DivisionId == match.DivisionId && se.ParticipantId == match.PlayerBId && se.Completed == false).First();
                CompleteEntry(entryA);
                CompleteEntry(entryB);

                if (match.WinnerId == match.PlayerAId)
                {
                    InjectWinner(match.DivisionId, match.PlayerAId, 1);
                    InjectWinner(match.DivisionId, match.PlayerBId, 2);
                }
                else
                {
                    InjectWinner(match.DivisionId, match.PlayerBId, 1);
                    InjectWinner(match.DivisionId, match.PlayerAId, 2);
                }
            }
            else
            {
                Response.Write("FAILURE!");
                Response.Flush();
                Response.End();
            }
            
        }

        private int getNewMaxOrder(int DivisionId)
        {
            var currentMaxOrder = from se in db.SparringEntries
                                  where se.DivisionId == DivisionId
                                  orderby se.Order descending
                                  select se.Order;
            return currentMaxOrder.First() + 1;
        }

        private void InjectWinner(int DivisionId, int ParticipantId, int rank)
        {
            DivisionWinner x = new DivisionWinner
            {
                DivisionId = DivisionId,
                ParticipantId = ParticipantId,
                Rank = rank,
                Points = ChoonbeeHelpers.getPointsByRankOrder(rank)
            };

            db.DivisionWinners.Add(x);
            db.SaveChanges();
        }

        [HttpPost, ActionName("SparringScoring")]
        public ActionResult SparringScoring_Post(Match match)
        {
            saveMatch(match);
            return RedirectToAction("SparringScoring", new { id = match.DivisionId });
        }

        public ActionResult ParticipantsSparring(int id)
        {
            var division = db.Divisions.Find(id);
            var tournament = db.Tournaments.Find(division.TournamentId);
            ViewBag.AllParticipants = tournament.Participants.Select(r => new SelectListItem { Value = r.ParticipantId.ToString(), Text = r.FirstName + "," + r.LastName }).ToList().OrderBy(z => z.Text);

            ViewBag.DivisionHeading = division.FriendlyId + " " + division.AdditionalidentifierText + " " + division.RankGroup.Name + ", " + division.AgeGroup.Name; 
            return View(division);
        }

        public ActionResult GetParticipants(int id)
        {
            var division = db.Divisions.Find(id);
            var participants = from dp in division.DivisionParticipants
                               select new 
                               { 
                                   ParticipantId = dp.ParticipantId,
                                   SchoolName = dp.Participant.School.Name, 
                                   FirstName = dp.Participant.FirstName,
                                   LastName = dp.Participant.LastName,
                                   HeightFeet = dp.Participant.HeightFeet,
                                   HeightInches = dp.Participant.HeightInches,
                                   BeltName = dp.Participant.Rank.RankName,
                                   Gender = dp.Participant.Gender,
                                   Age = dp.Participant.Age
                               };


            return Json(JsonConvert.SerializeObject(participants), JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult CreateDivision(Division thedivision, List<Participant> theparticipantlist)
        {
            db.Divisions.Add(thedivision);
            foreach (var participant in theparticipantlist)
            {
                var entry = new DivisionParticipant
                {
                    DateEntered = DateTime.Now,
                    DivisionId = thedivision.DivisionId,
                    ParticipantId = participant.ParticipantId
                };
                db.DivisionParticipants.Add(entry);
            }
            db.SaveChanges();
            return Json(JsonConvert.SerializeObject(new {DivisionId = thedivision.DivisionId}));
        }

        public ActionResult Create()
        {
            ViewBag.AgeGroupId = new SelectList(db.AgeGroups, "AgeGroupId", "Name");
            ViewBag.RankGroupId = new SelectList(db.RankGroups, "RankGroupId", "Name");
            ViewBag.TournamentId = new SelectList(db.Tournaments, "TournamentId", "Name");
            ViewBag.DivisionTypeId = new SelectList(db.DivisionTypes, "DivisionTypeId", "Name");
            ViewBag.DivisionStatusId = new SelectList(db.DivisionStatuses, "DivisionStatusId", "Name");
            return View();
        }

        //
        // POST: /DivisionController2/Create

        [HttpPost]
        public ActionResult Create(Division division)
        {
            if (ModelState.IsValid)
            {
                db.Divisions.Add(division);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AgeGroupId = new SelectList(db.AgeGroups, "AgeGroupId", "Name", division.AgeGroupId);
            ViewBag.RankGroupId = new SelectList(db.RankGroups, "RankGroupId", "Name", division.RankGroupId);
            ViewBag.TournamentId = new SelectList(db.Tournaments, "TournamentId", "Name", division.TournamentId);
            ViewBag.DivisionTypeId = new SelectList(db.DivisionTypes, "DivisionTypeId", "Name", division.DivisionTypeId);
            ViewBag.DivisionStatusId = new SelectList(db.DivisionStatuses, "DivisionStatusId", "Name", division.DivisionStatusId);
            return View(division);
        }
    }

    
}
