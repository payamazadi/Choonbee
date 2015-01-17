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
    public class AgeGroupController : Controller
    {
        private ChoonbeeEdmx db = new ChoonbeeEdmx();

        //
        // GET: /AgeGroup/

        public ActionResult Index()
        {
            return View(db.AgeGroups.ToList());
        }

        //
        // GET: /AgeGroup/Details/5

        public ActionResult Details(int id = 0)
        {
            AgeGroup agegroup = db.AgeGroups.Find(id);
            if (agegroup == null)
            {
                return HttpNotFound();
            }
            return View(agegroup);
        }

        //
        // GET: /AgeGroup/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /AgeGroup/Create

        [HttpPost]
        public ActionResult Create(AgeGroup agegroup)
        {
            if (ModelState.IsValid)
            {
                db.AgeGroups.Add(agegroup);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(agegroup);
        }

        //
        // GET: /AgeGroup/Edit/5

        public ActionResult Edit(int id = 0)
        {
            AgeGroup agegroup = db.AgeGroups.Find(id);
            if (agegroup == null)
            {
                return HttpNotFound();
            }
            return View(agegroup);
        }

        //
        // POST: /AgeGroup/Edit/5

        [HttpPost]
        public ActionResult Edit(AgeGroup agegroup)
        {
            if (ModelState.IsValid)
            {
                db.Entry(agegroup).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(agegroup);
        }

        //
        // GET: /AgeGroup/Delete/5

        public ActionResult Delete(int id = 0)
        {
            AgeGroup agegroup = db.AgeGroups.Find(id);
            if (agegroup == null)
            {
                return HttpNotFound();
            }
            return View(agegroup);
        }

        //
        // POST: /AgeGroup/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            AgeGroup agegroup = db.AgeGroups.Find(id);
            db.AgeGroups.Remove(agegroup);
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