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
    public class DivisionFormScoreController : Controller
    {
        private ChoonbeeEdmx db = new ChoonbeeEdmx();

        //
        // GET: /DivisionFormScore/

        public ActionResult Index()
        {
            var divisionformscores = db.DivisionFormScores.Include(d => d.Division).Include(d => d.Participant);
            return View(divisionformscores.ToList());
        }

        //
        // GET: /DivisionFormScore/Details/5

        public ActionResult Details(int id = 0)
        {
            DivisionFormScore divisionformscore = db.DivisionFormScores.Find(id);
            if (divisionformscore == null)
            {
                return HttpNotFound();
            }
            return View(divisionformscore);
        }

        //
        // GET: /DivisionFormScore/Create

        public ActionResult Create(int? id, int? participantId)
        {
            //check for existence. if so, route to edit.
            var exists = db.DivisionFormScores.Where(dfs => dfs.DivisionId == id && dfs.ParticipantId == participantId);
            if(exists.Count() > 0)
                return RedirectToAction("Edit", new {id = id, participantId = participantId});
            ViewBag.DivisionId = new SelectList(db.Divisions, "DivisionId", "FriendlyId", db.Divisions.Find(id).DivisionId);
            ViewBag.TheDivisionid = id;
            ViewBag.ParticipantId = new SelectList(db.Participants, "ParticipantId", "FirstName");
            return View();
        }

        //
        // POST: /DivisionFormScore/Create

        [HttpPost]
        public ActionResult Create(DivisionFormScore divisionformscore)
        {
            if (ModelState.IsValid)
            {
                divisionformscore.DateEntered = DateTime.Now;
                divisionformscore.Sum = divisionformscore.JudgeOneScore + divisionformscore.JudgeTwoScore + divisionformscore.JudgeThreeScore;
                db.DivisionFormScores.Add(divisionformscore);
                db.SaveChanges();
                return RedirectToAction("FormScoring", "Division", new { id = divisionformscore.DivisionId });
            }

            ViewBag.DivisionId = new SelectList(db.Divisions, "DivisionId", "FriendlyId", divisionformscore.DivisionId);
            ViewBag.ParticipantId = new SelectList(db.Participants, "ParticipantId", "FirstName", divisionformscore.ParticipantId);
            return View(divisionformscore);
        }

        //
        // GET: /DivisionFormScore/Edit/5

        public ActionResult Edit(int id = 0, int? participantId = 0)
        {
            var thedfs = db.DivisionFormScores.Where(dfs => dfs.DivisionId == id && dfs.ParticipantId == participantId);
            if (thedfs == null)
            {
                return HttpNotFound();
            }
            var divisionformscore = thedfs.First();
            ViewBag.DivisionId = new SelectList(db.Divisions, "DivisionId", "FriendlyId", divisionformscore.DivisionId);
            ViewBag.ParticipantId = new SelectList(db.Participants, "ParticipantId", "FirstName", divisionformscore.ParticipantId);
            return View(divisionformscore);
        }

        //
        // POST: /DivisionFormScore/Edit/5

        [HttpPost]
        public ActionResult Edit(DivisionFormScore divisionformscore)
        {
            if (ModelState.IsValid)
            {
                divisionformscore.DateEntered = DateTime.Now;
                divisionformscore.Sum = divisionformscore.JudgeOneScore + divisionformscore.JudgeTwoScore + divisionformscore.JudgeThreeScore;
                db.Entry(divisionformscore).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("FormScoring", "Division", new { id = divisionformscore.DivisionId });
            }
            ViewBag.DivisionId = new SelectList(db.Divisions, "DivisionId", "FriendlyId", divisionformscore.DivisionId);
            ViewBag.ParticipantId = new SelectList(db.Participants, "ParticipantId", "FirstName", divisionformscore.ParticipantId);
            return View(divisionformscore);
        }

        //
        // GET: /DivisionFormScore/Delete/5

        public ActionResult Delete(int id = 0)
        {
            DivisionFormScore divisionformscore = db.DivisionFormScores.Find(id);
            if (divisionformscore == null)
            {
                return HttpNotFound();
            }
            return View(divisionformscore);
        }

        //
        // POST: /DivisionFormScore/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            DivisionFormScore divisionformscore = db.DivisionFormScores.Find(id);
            db.DivisionFormScores.Remove(divisionformscore);
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