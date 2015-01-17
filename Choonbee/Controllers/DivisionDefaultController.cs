using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Choonbee.Models;

namespace Choonbee.Controllers
{
    public class DivisionDefaultController : Controller
    {
        private ChoonbeeEdmx db = new ChoonbeeEdmx();

        //
        // GET: /DivisionDefault/

        public ActionResult Index()
        {
            var divisiondefaults = db.DivisionDefaults.Include(d => d.AgeGroup).Include(d => d.RankGroup).Include(d => d.DivisionType);
            return View(divisiondefaults.ToList());
        }

        //
        // GET: /DivisionDefault/Details/5

        public ActionResult Details(int id = 0)
        {
            DivisionDefault divisiondefault = db.DivisionDefaults.Find(id);
            if (divisiondefault == null)
            {
                return HttpNotFound();
            }
            return View(divisiondefault);
        }

        //
        // GET: /DivisionDefault/Create

        public ActionResult Create()
        {
            ViewBag.AgeGroupId = new SelectList(db.AgeGroups, "AgeGroupId", "Name");
            ViewBag.RankGroupId = new SelectList(db.RankGroups, "RankGroupId", "Name");
            ViewBag.DivisionTypeId = new SelectList(db.DivisionTypes, "DivisionTypeId", "Name");
            return View();
        }

        //
        // POST: /DivisionDefault/Create

        [HttpPost]
        public ActionResult Create(DivisionDefault divisiondefault)
        {
            if (ModelState.IsValid)
            {
                // @TODO: for some reason MySQL not saving the time on date correctly.
                divisiondefault.DateEntered = DateTime.Now;
                db.DivisionDefaults.Add(divisiondefault);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AgeGroupId = new SelectList(db.AgeGroups, "AgeGroupId", "Name", divisiondefault.AgeGroupId);
            ViewBag.RankGroupId = new SelectList(db.RankGroups, "RankGroupId", "Name", divisiondefault.RankGroupId);
            ViewBag.DivisionTypeId = new SelectList(db.DivisionTypes, "DivisionTypeId", "Name", divisiondefault.DivisionTypeId);
            return View(divisiondefault);
        }

        //
        // GET: /DivisionDefault/Edit/5

        public ActionResult Edit(int id = 0)
        {
            DivisionDefault divisiondefault = db.DivisionDefaults.Find(id);
            if (divisiondefault == null)
            {
                return HttpNotFound();
            }
            ViewBag.AgeGroupId = new SelectList(db.AgeGroups, "AgeGroupId", "Name", divisiondefault.AgeGroupId);
            ViewBag.RankGroupId = new SelectList(db.RankGroups, "RankGroupId", "Name", divisiondefault.RankGroupId);
            ViewBag.DivisionTypeId = new SelectList(db.DivisionTypes, "DivisionTypeId", "Name", divisiondefault.DivisionTypeId);
            return View(divisiondefault);
        }

        //
        // POST: /DivisionDefault/Edit/5

        [HttpPost]
        public ActionResult Edit(DivisionDefault divisiondefault)
        {
            if (ModelState.IsValid)
            {
                db.Entry(divisiondefault).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AgeGroupId = new SelectList(db.AgeGroups, "AgeGroupId", "Name", divisiondefault.AgeGroupId);
            ViewBag.RankGroupId = new SelectList(db.RankGroups, "RankGroupId", "Name", divisiondefault.RankGroupId);
            ViewBag.DivisionTypeId = new SelectList(db.DivisionTypes, "DivisionTypeId", "Name", divisiondefault.DivisionTypeId);
            return View(divisiondefault);
        }

        //
        // GET: /DivisionDefault/Delete/5

        public ActionResult Delete(int id = 0)
        {
            DivisionDefault divisiondefault = db.DivisionDefaults.Find(id);
            if (divisiondefault == null)
            {
                return HttpNotFound();
            }
            return View(divisiondefault);
        }

        //
        // POST: /DivisionDefault/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            DivisionDefault divisiondefault = db.DivisionDefaults.Find(id);
            db.DivisionDefaults.Remove(divisiondefault);
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