using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Choonbee.Models;
using Choonbee.App_Start;
using Newtonsoft.Json;

namespace Choonbee.Controllers
{
    public class ParticipantController : Controller
    {
        private ChoonbeeEdmx db = new ChoonbeeEdmx();

        //
        // GET: /Participant/

        public ActionResult Index()
        {
            var participants = db.Participants.Include(p => p.School).Include(p => p.Rank);
            return View(participants.ToList());
        }

        //
        // GET: /Participant/Details/5

        public ActionResult Details(int id = 0)
        {
            Participant participant = db.Participants.Find(id);
            if (participant == null)
            {
                return HttpNotFound();
            }
            return View(participant);
        }

        //
        // GET: /Participant/Create
        [HttpGet]
        public ActionResult Create(int? tournamentId)
        {
            ViewBag.SchoolId = new SelectList(db.Schools, "SchoolId", "Name").OrderBy(s => s.Text);
            ViewBag.RankId = new SelectList(db.Ranks, "RankId", "RankName");
            ViewBag.TournamentId = tournamentId.GetValueOrDefault();
            return View();
        }
        

        //
        // POST: /Participant/Create

        [HttpPost]
        public ActionResult Create(Participant participant, int tournamentId)
        {
            if (ModelState.IsValid)
            {
                participant.DateEntered = DateTime.Now;
                participant.DateModified = DateTime.Now;
                participant.Gender = participant.Gender.ToUpper();
                db.Participants.Add(participant);
                var a = db.GetValidationErrors();
                db.SaveChanges();

                if (tournamentId != 0)
                {
                    var tournament = db.Tournaments.Find(tournamentId);
                    if (tournament != null)
                    {
                        tournament.Participants.Add(participant);
                        db.Entry(tournament).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                //return RedirectToAction("Details", "Tournament", new {id = tournamentId});
                return RedirectToAction("ManageParticipantDivision", "Tournament", new { id = tournamentId, participantid = participant.ParticipantId });
            }

            ViewBag.SchoolId = new SelectList(db.Schools, "SchoolId", "Name", participant.SchoolId);
            ViewBag.RankId = new SelectList(db.Ranks, "RankId", "RankName", participant.RankId);
            return View(participant);
        }

        //
        // GET: /Participant/Edit/5

        public ActionResult Edit(int id = 0, string ret = "")
        {
            Participant participant = db.Participants.Find(id);
            if (participant == null)
            {
                return HttpNotFound();
            }
            ViewBag.SchoolId = new SelectList(db.Schools, "SchoolId", "Name", participant.SchoolId);
            ViewBag.RankId = new SelectList(db.Ranks, "RankId", "RankName", participant.RankId);
            ViewBag.Ret = ret;
            return View(participant);
        }

        //
        // POST: /Participant/Edit/5

        [HttpPost]
        public ActionResult Edit(Participant participant, string ret)
        {
            if (ModelState.IsValid)
            {
                participant.Gender = participant.Gender.ToUpper();
                db.Entry(participant).State = EntityState.Modified;
                var p = db.GetValidationErrors();
                db.SaveChanges();

                if (ret.Equals(String.Empty) == false)
                    return Redirect(ret);
                return RedirectToAction("Index");
            }
            ViewBag.SchoolId = new SelectList(db.Schools, "SchoolId", "Name", participant.SchoolId);
            ViewBag.RankId = new SelectList(db.Ranks, "RankId", "RankName", participant.RankId);

            return View(participant);
        }

        //
        // GET: /Participant/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Participant participant = db.Participants.Find(id);
            if (participant == null)
            {
                return HttpNotFound();
            }
            return View(participant);
        }

        //
        // POST: /Participant/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Participant participant = db.Participants.Find(id);
            db.Participants.Remove(participant);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        public ActionResult GetParticipant(int id)
        {
            var participant = from p in db.Participants
                              where p.ParticipantId == id
                              select new
                              {
                                  ParticipantId = p.ParticipantId,
                                  SchoolName = p.School.Name,
                                  FirstName = p.FirstName,
                                  LastName = p.LastName,
                                  HeightFeet = p.HeightFeet,
                                  HeightInches = p.HeightInches,
                                  BeltName = p.Rank.RankName,
                                  Gender = p.Gender,
                                  Age = p.Age
                              };

            return Json(JsonConvert.SerializeObject(participant), JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}