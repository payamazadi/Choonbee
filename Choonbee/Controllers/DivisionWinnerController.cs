using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Choonbee.Models;

namespace Choonbee.Controllers
{
    public class DivisionWinnerController : Controller
    {
        private ChoonbeeEdmx db = new ChoonbeeEdmx();

        //
        // GET: /DivisionWinner/

        public ActionResult Index()
        {
            var divisionwinners = db.DivisionWinners.Include(d => d.Division).Include(d => d.Participant);
            return View(divisionwinners.ToList());
        }

        //
        // GET: /DivisionWinner/Details/5

        public ActionResult Details(int id = 0)
        {
            DivisionWinner divisionwinner = db.DivisionWinners.Find(id);
            if (divisionwinner == null)
            {
                return HttpNotFound();
            }
            return View(divisionwinner);
        }

        //
        // GET: /DivisionWinner/Create

        public ActionResult Create(int? id, string message = "")
        {
            if (id != null)
            {
                //TOODO: ONCE TOP 4 POSITIONS FILLED, CLOSE DIVISION!
                var divisions = db.Divisions.Where(d => d.DivisionId == id).ToList();
                var division = divisions.First();
                ViewBag.DivisionId = new SelectList(divisions, "DivisionId", "FriendlyId", division.DivisionId);
                ViewBag.Division = division;
                var divisionParticipants = db.DivisionParticipants.Where(d => d.DivisionId == id).ToList();
                var scoredParticipants = (from p1 in db.DivisionWinners.ToList().Where(dw => dw.DivisionId == id)
                                          join p2 in divisionParticipants on p1.ParticipantId equals p2.ParticipantId
                                          select p2).Select(p3 => p3.Participant).ToList();
                var divisionParticipants2 = divisionParticipants.Where(d2 => !scoredParticipants.Contains(d2.Participant));

                //db.Ranks.Where(r => !myRankIds.Contains(r.RankId)).ToList();


                var participants = divisionParticipants2.ToList().Select(p => p.Participant).Select(p2 => new SelectListItem { Value = p2.ParticipantId.ToString(), Text = p2.FirstName + " " + p2.LastName }).ToList();
                //.Select(r => new SelectListItem { Value = r.ParticipantId.ToString(), Text = r.FirstName + "," + r.LastName }).ToList();
                
                ViewBag.ParticipantId = participants;
                ViewBag.HasCount = participants.ToList().Count != 0;
                
                ViewBag.RetLink = "/Tournament/ViewDivisions/" + division.TournamentId;
                ViewBag.TheDivisionId = id;
                if (message.Equals("") == false)
                    ViewBag.Message = message;
            }
            else
            {
                var divisions = db.Divisions.ToList();
                var participants = db.DivisionParticipants.ToList().Select(p => new SelectListItem { Value = p.ParticipantId.ToString(), Text = p.Participant.FirstName + " " + p.Participant.LastName }).ToList();

                //Duplicate lines of code from if condition.
                ViewBag.ParticipantId = participants;
                ViewBag.HasCount = participants.ToList().Count != 0;

                ViewBag.Divisionid = new SelectList(divisions, "DivisionId", "FriendlyId");
                
                ViewBag.RetLink = "/DivisionWinner";
            }
            return View();
        }

        //
        // POST: /DivisionWinner/Create

        [HttpPost]
        public ActionResult Create(DivisionWinner divisionwinner)
        {
            if (ModelState.IsValid)
            {
                var points = getPoints(divisionwinner.Rank);
                divisionwinner.Points = points;
                db.DivisionWinners.Add(divisionwinner);
                db.SaveChanges();
                //return RedirectToAction("Index");
                return RedirectToAction("Create", new { id = divisionwinner.DivisionId, message = "Added." });
            }

            ViewBag.DivisionId = new SelectList(db.Divisions, "DivisionId", "FriendlyId", divisionwinner.DivisionId);
            ViewBag.ParticipantId = new SelectList(db.Participants, "ParticipantId", "FirstName", divisionwinner.ParticipantId);
            return RedirectToAction("Create", new { id = divisionwinner.DivisionId });
        }

        private int getPoints(int rank)
        {
            return ChoonbeeHelpers.getPointsByRankOrder(rank);
        }

        //
        // GET: /DivisionWinner/Edit/5

        public ActionResult Edit(int id = 0, int participant = 0)
        {
            DivisionWinner divisionwinner = db.DivisionWinners.Where(dw => dw.DivisionId == id && dw.ParticipantId == participant).First();
            if (divisionwinner == null)
            {
                return HttpNotFound();
            }
            ViewBag.DivisionId = new SelectList(db.Divisions, "DivisionId", "FriendlyId", divisionwinner.DivisionId);
            ViewBag.ParticipantId = new SelectList(db.Participants, "ParticipantId", "FirstName", divisionwinner.ParticipantId);
            return View(divisionwinner);
        }

        //
        // POST: /DivisionWinner/Edit/5

        [HttpPost]
        public ActionResult Edit(DivisionWinner divisionwinner)
        {
            divisionwinner.Participant = db.Participants.Find(divisionwinner.ParticipantId);
            
            //@TODO: WHY!?!?!
            //if (ModelState.IsValid)
            //{
                
                
                
                db.Entry(divisionwinner).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            //}
            
            /*
            ViewBag.DivisionId = new SelectList(db.Divisions, "DivisionId", "FriendlyId", divisionwinner.DivisionId);
            ViewBag.ParticipantId = new SelectList(db.Participants, "ParticipantId", "FirstName", divisionwinner.ParticipantId);
            return View(divisionwinner);
             */
        }

        //
        // GET: /DivisionWinner/Delete/5

        public ActionResult Delete(int id = 0, int participant = 0)
        {
            DivisionWinner divisionwinner = db.DivisionWinners.Where(dw => dw.DivisionId == id && dw.ParticipantId == participant).First();
            db.DivisionWinners.Remove(divisionwinner);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //
        // POST: /DivisionWinner/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            DivisionWinner divisionwinner = db.DivisionWinners.Find(id);
            db.DivisionWinners.Remove(divisionwinner);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}