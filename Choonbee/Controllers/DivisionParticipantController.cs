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
    public class DivisionParticipantController : Controller
    {
        private ChoonbeeEdmx db = new ChoonbeeEdmx();

        //
        // GET: /DivisionParticipant/

        public ActionResult Index()
        {
            var divisionparticipants = db.DivisionParticipants.Include(d => d.Division).Include(d => d.Participant);
            return View(divisionparticipants.ToList());
        }

        //
        // GET: /DivisionParticipant/Details/5

        public ActionResult Details(int id = 0)
        {
            DivisionParticipant divisionparticipant = db.DivisionParticipants.Find(id);
            if (divisionparticipant == null)
            {
                return HttpNotFound();
            }
            return View(divisionparticipant);
        }

        //
        // GET: /DivisionParticipant/Create

        public ActionResult Create()
        {
            ViewBag.DivisionId = new SelectList(db.Divisions, "DivisionId", "FriendlyId");
            ViewBag.ParticipantId = new SelectList(db.Participants, "ParticipantId", "FirstName");
            return View();
        }

        //
        // POST: /DivisionParticipant/Create

        [HttpPost]
        public ActionResult Create(DivisionParticipant divisionparticipant)
        {
            if (ModelState.IsValid)
            {
                db.DivisionParticipants.Add(divisionparticipant);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.DivisionId = new SelectList(db.Divisions, "DivisionId", "FriendlyId", divisionparticipant.DivisionId);
            ViewBag.ParticipantId = new SelectList(db.Participants, "ParticipantId", "FirstName", divisionparticipant.ParticipantId);
            return View(divisionparticipant);
        }

        //
        // GET: /DivisionParticipant/Edit/5

        public ActionResult Edit(int id = 0)
        {
            DivisionParticipant divisionparticipant = db.DivisionParticipants.Find(id);
            if (divisionparticipant == null)
            {
                return HttpNotFound();
            }
            ViewBag.DivisionId = new SelectList(db.Divisions, "DivisionId", "FriendlyId", divisionparticipant.DivisionId);
            ViewBag.ParticipantId = new SelectList(db.Participants, "ParticipantId", "FirstName", divisionparticipant.ParticipantId);
            return View(divisionparticipant);
        }

        //
        // POST: /DivisionParticipant/Edit/5

        [HttpPost]
        public ActionResult Edit(DivisionParticipant divisionparticipant)
        {
            if (ModelState.IsValid)
            {
                db.Entry(divisionparticipant).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.DivisionId = new SelectList(db.Divisions, "DivisionId", "FriendlyId", divisionparticipant.DivisionId);
            ViewBag.ParticipantId = new SelectList(db.Participants, "ParticipantId", "FirstName", divisionparticipant.ParticipantId);
            return View(divisionparticipant);
        }

        //
        // GET: /DivisionParticipant/Delete/5

        public ActionResult Delete(int id = 0)
        {
            DivisionParticipant divisionparticipant = db.DivisionParticipants.Find(id);
            if (divisionparticipant == null)
            {
                return HttpNotFound();
            }
            return View(divisionparticipant);
        }

        //
        // POST: /DivisionParticipant/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            DivisionParticipant divisionparticipant = db.DivisionParticipants.Find(id);
            db.DivisionParticipants.Remove(divisionparticipant);
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