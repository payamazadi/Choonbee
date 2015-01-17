using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Choonbee.Models;

namespace Choonbee.Controllers
{
    public class TournamentController : Controller
    {
        private ChoonbeeEdmx db = new ChoonbeeEdmx();

        //
        // GET: /Tournament/

        public ActionResult Index()
        {
            return View(db.Tournaments.ToList());
        }

        //
        // GET: /Tournament/Details/5

        public ActionResult Details(int id = 0)
        {
            Tournament tournament = db.Tournaments.Find(id);
            if (tournament == null)
            {
                return HttpNotFound();
            }
            ViewBag.TournamentId = id;
            return View(tournament);
        }

        //
        // GET: /Tournament/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Tournament/Create

        [HttpPost]
        public ActionResult Create(Tournament tournament)
        {
            if (ModelState.IsValid)
            {
                tournament.DateEntered = DateTime.Now;
                db.Tournaments.Add(tournament);
                db.SaveChanges();
                return RedirectToAction("Details", new { id = tournament.TournamentId });
            }

            return View(tournament);
        }

        //
        // GET: /Tournament/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Tournament tournament = db.Tournaments.Find(id);
            if (tournament == null)
            {
                return HttpNotFound();
            }
            return View(tournament);
        }

        //
        // POST: /Tournament/Edit/5

        [HttpPost]
        public ActionResult Edit(Tournament tournament)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tournament).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tournament);
        }

        //
        // GET: /Tournament/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Tournament tournament = db.Tournaments.Find(id);
            if (tournament == null)
            {
                return HttpNotFound();
            }
            return View(tournament);
        }

        //
        // POST: /Tournament/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Tournament tournament = db.Tournaments.Find(id);
            db.Tournaments.Remove(tournament);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult Populate(int id = 0)
        {
            var list = new LinkedList<DivisionDefault>();
            foreach (var defaultDivision in db.DivisionDefaults)
            {
                list.AddLast(defaultDivision);
            }

            foreach (var defaultDivision in list)
            {
                Division a = new Division();
                a.AgeGroup = defaultDivision.AgeGroup;
                a.AdditionalidentifierText = "";
                a.RankGroup = defaultDivision.RankGroup;
                a.DateEntered = DateTime.Now;
                a.DivisionStatus = db.DivisionStatuses.Where(ds => ds.Name == "Open").First();
                a.DivisionType = defaultDivision.DivisionType;
                a.FriendlyId = defaultDivision.FriendlyId;
                a.Genders = defaultDivision.Genders;
                a.HadTieBreaker = false;
                a.ParentDivisionId = 0;
                a.Order = defaultDivision.Order;
                a.Tournament = db.Tournaments.Find(id);
                db.Divisions.Add(a);
                var qq = db.GetValidationErrors();
                db.SaveChanges();
            }

            return RedirectToAction("Details", new { id = id });
        }

        public ActionResult ViewDivisions(int id = 0)
        {
            var tournament = db.Tournaments.Find(id);
            ViewBag.Heading = tournament.Name + " " + tournament.DateHeld + " - Divisions";

            return View(tournament.Divisions.ToList().OrderByDescending(t => t.DivisionStatus.Name).ThenBy(t2 => t2.Order));
        }

        public ActionResult AddExistingParticipant(int id = 0)
        {
            var tournament = db.Tournaments.Find(id);
            ViewBag.AllParticipants = db.Participants.ToList().OrderBy(r => r.FirstName).Select(r => new SelectListItem { Value = r.ParticipantId.ToString(), Text = r.FirstName + " " + r.LastName }).ToList();
            ViewBag.Heading = tournament.Name;
            ViewBag.TournamentId = id;
            return View(tournament);
        }

        [HttpPost]
        public ActionResult AddExistingParticipant(int id = 0, int AllParticipants = 0)
        {
            db.Tournaments.Find(id).Participants.Add(db.Participants.Find(AllParticipants));
            db.SaveChanges();
            return RedirectToAction("Details", new { id = id });
        }

        public ActionResult RemoveParticipant(int id = 0, int participantId = 0)
        {
            db.Tournaments.Find(id).Participants.Remove(db.Participants.Find(participantId));
            db.SaveChanges();
            return RedirectToAction("Details", new { id = id });
        }

        //TODO: clearly the mapping from division to participants is wrong, otherwise I'd get a list of all Participants from division..
        public ActionResult ManageParticipantDivision(int id = 0, int participantId = 0)
        {
            //show currently registered divisions
            //get and display all divisions this person is qualified for in this tournament
            var participant = db.Participants.Find(participantId);
            var tournament = db.Tournaments.Find(id);

            var allDivisions = tournament.Divisions.Where(d => d.AgeGroup.MinAge <= participant.Age && d.AgeGroup.MaxAge >= participant.Age && (d.Genders == "E" || d.Genders == participant.Gender) && d.RankGroup.Ranks.Contains(participant.Rank)).ToList();
            
            var myDivisionRegistrations = db.DivisionParticipants.Where(d => d.ParticipantId == participantId);
            
            var myDivisions = new LinkedList<Division>();
            var allDivisionsPruned = new LinkedList<Division>();
            
            //TODO: wow what a horrid way to do this.
            foreach (var registration in myDivisionRegistrations)
            {
                myDivisions.AddLast(allDivisions.Where(d => d.DivisionId == registration.DivisionId).First());
            }
            
            foreach (var division in allDivisions)
            {
                if (myDivisions.Contains(division) == false)
                    allDivisionsPruned.AddLast(division);
            }

            ViewBag.MyDivisions = myDivisions;
            ViewBag.AllQualifiedDivisions = allDivisionsPruned;
            ViewBag.ParticipantName = participant.FirstName + " " + participant.LastName;
            ViewBag.TournamentName = tournament.Name;
            ViewBag.TournamentId = id;
            ViewBag.ParticipantId = participantId;
            return View();
        }

        public ActionResult AddParticipantDivision(int id = 0, int participantId = 0, int divisionId = 0)
        {
            var division = db.Divisions.Find(divisionId);
            var participant = db.Participants.Find(participantId);

            var newRegistration = new DivisionParticipant();
            newRegistration.DateEntered = DateTime.Now;
            newRegistration.Division = division;
            newRegistration.Participant = participant;
            db.DivisionParticipants.Add(newRegistration);
            db.SaveChanges();

            return RedirectToAction("ManageParticipantDivision", new { id = id, participantId = participantId });
        }

        public ActionResult RemoveParticipantDivision(int id = 0, int participantId = 0, int divisionId = 0)
        {
            var registration = db.DivisionParticipants.Where(r => r.DivisionId == divisionId && r.ParticipantId == participantId).First();
            db.DivisionParticipants.Remove(registration);

            db.SaveChanges();

            return RedirectToAction("ManageParticipantDivision", new { id = id, participantId = participantId });
        }

        public ActionResult ManageTeams(int id = 0)
        {
            //list teams. links to show teams, with dropdown of participants, not already on that team.
            var tournaments = db.Tournaments.Find(id);
            var teams = (from team in db.TournamentTeams
                         join school in db.Schools on team.SchoolId equals school.SchoolId
                         select new { school=school, team=team }).ToList().OrderBy(t => t.school.Name).ToList();
            
            ViewBag.TournamentId = id;

            Dictionary<TournamentTeam, School> temp = new Dictionary<TournamentTeam, School>();
            foreach (var item in teams)
                temp.Add(item.team, item.school);

            return View(temp);
        }

        [HttpGet]
        public ActionResult AddTeam(int id = 0)
        {
            ViewBag.SchoolId = new SelectList(db.Schools, "SchoolId", "Name");
            ViewBag.TournamentId = id;
            return View();
        }

        [HttpPost]
        public ActionResult AddTeam(TournamentTeam a)
        {
            if (ModelState.IsValid)
            {
                db.TournamentTeams.Add(a);
                db.SaveChanges();
            }
            return RedirectToAction("ManageTeams", new { id = a.TournamentId });
        }

        //TODO: BARF!!!!!!!!
        public ActionResult AddTeamParticipant(int id = 0, int teamId = 0)
        {
            var tournament = db.Tournaments.Find(id);
            var team = db.TournamentTeams.Find(teamId);

            //get all participants whos school matches the team's, that are registered for this tournament.
            //who aren't already on a team?
            //var participants = db.Participants.Where(p => p.SchoolId == team.SchoolId).ToList();
            /*
            var participants = from p1 in db.Participants
                               join p2 in tournament.Participants on p1.ParticipantId equals p2.ParticipantId
                               select p1;
            */
            var participants = tournament.Participants.ToList();

            //subtract out participants already on this team
            var existingParticipants = team.Participants.Select(p => p.ParticipantId).ToList();
            //get all participants from this school on a team in this tournament
            var allSchoolTeams = db.TournamentTeams.Where(tt => team.SchoolId == team.SchoolId).ToList();

            List<int> participatingParticipants = new List<int>();
            foreach (var schoolTeams in allSchoolTeams)
            {
                participatingParticipants.AddRange(schoolTeams.Participants.Select(p => p.ParticipantId));
            }

            var participantsPruned = participants.Where(p => !existingParticipants.Contains(p.ParticipantId));
            participantsPruned = participantsPruned.Where(p => !participatingParticipants.Contains(p.ParticipantId));

            ViewBag.AllParticipants = participantsPruned.ToList().OrderBy(r => r.ParticipantId).Select(r => new SelectListItem { Value = r.ParticipantId.ToString(), Text = r.FirstName + " " + r.LastName }).ToList();
            ViewBag.TournamentId = id;

            return View();
        }

        [HttpPost]
        public ActionResult AddTeamParticipant(int id = 0, int teamId = 0, int AllParticipants = 0)
        {
            db.TournamentTeams.Find(teamId).Participants.Add(db.Participants.Find(AllParticipants));
            db.SaveChanges();
            ViewBag.TournamentId = id;
            return RedirectToAction("AddTeamParticipant", new {id = id, teamId = teamId});
        }

        public ActionResult ManageTeamParticipants(int id = 0, int teamId = 0)
        {
            var participants = db.TournamentTeams.Find(teamId).Participants.ToList();
            
            ViewBag.Participants = participants;
            ViewBag.TournamentId = id;
            ViewBag.TeamId = teamId;
            
            return View();
        }

        public ActionResult DumpRegistration(int id = 0)
        {
            var query = from p in db.Participants
                        join dp in db.DivisionParticipants on p.ParticipantId equals dp.ParticipantId
                        join d in db.Divisions on dp.DivisionId equals d.DivisionId
                        where d.TournamentId == id
                        select new { Player = p, Division = d };

            var dict = new Dictionary<Division, LinkedList<Participant>>();
            foreach (var item in query)
            {
                if(dict.Keys.Contains(item.Division)==false)
                    dict[item.Division] = new LinkedList<Participant>();
                dict[item.Division].AddLast(item.Player);
            }

            ViewBag.TournamentDivisions = dict;

            return View(); // RedirectToAction("Details", "Tournament", new { id = id });
        }

        public ActionResult TeamScores(int id = 0)
        {
            var query = from dw in db.DivisionWinners
                        from tt in db.TournamentTeams
                        from p in tt.Participants
                        where tt.TournamentId == id && dw.ParticipantId == p.ParticipantId
                        select new { Team = tt, Participant = p, Score = dw.Points };

            var teamsAdded = new List<int>();
            var teamScoresResult = new Dictionary<int, TeamScore>();
            foreach (var item in query.ToList())
            {
                TeamScore team;
                if (!teamScoresResult.Keys.Contains(item.Team.TeamId))
                {
                    team = new TeamScore()
                    {
                        TeamId = item.Team.TeamId,
                        TeamName = item.Team.Name,
                        SchoolId = item.Team.School.SchoolId,
                        SchoolName = item.Team.School.Name,
                        Score = 0
                    };
                }
                else
                {
                    team = teamScoresResult[item.Team.TeamId];
                }

                if (team.Participants.Keys.Contains(item.Participant.ParticipantId))
                {
                    var pt = team.Participants[item.Participant.ParticipantId];
                    team.Participants[item.Participant.ParticipantId] = new ParticipantScore { ParticipantId = pt.ParticipantId, Name = pt.Name, Score = pt.Score + item.Score };
                }
                else
                {
                    team.Participants.Add(item.Participant.ParticipantId, new ParticipantScore { ParticipantId = item.Participant.ParticipantId, Name = item.Participant.FirstName + " " + item.Participant.LastName, Score = item.Score });
                }

                team.Score = team.Score + item.Score;
                teamScoresResult[item.Team.TeamId] = team;
            }

            List<TeamScore> result = teamScoresResult.Values.OrderBy(ts => ts.Score).ThenBy(ts => ts.SchoolName).ThenBy(ts => ts.TeamName).ToList();
            Response.AddHeader("Refresh", "300");
            return View(result);
        }
    }
}