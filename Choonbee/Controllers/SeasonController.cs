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
    public class SeasonController : Controller
    {
        private ChoonbeeEdmx db = new ChoonbeeEdmx();

        //
        // GET: /Season/

        public ActionResult Index()
        {
            return View(db.Seasons.ToList());
        }

        //
        // GET: /Season/Details/5

        public ActionResult Details(int id = 0)
        {
            Season season = db.Seasons.Find(id);
            if (season == null)
            {
                return HttpNotFound();
            }
            return View(season);
        }

        //
        // GET: /Season/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Season/Create

        [HttpPost]
        public ActionResult Create(Season season)
        {
            if (ModelState.IsValid)
            {
                db.Seasons.Add(season);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(season);
        }

        //
        // GET: /Season/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Season season = db.Seasons.Find(id);
            if (season == null)
            {
                return HttpNotFound();
            }
            return View(season);
        }

        //
        // POST: /Season/Edit/5

        [HttpPost]
        public ActionResult Edit(Season season)
        {
            if (ModelState.IsValid)
            {
                db.Entry(season).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(season);
        }

        //
        // GET: /Season/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Season season = db.Seasons.Find(id);
            if (season == null)
            {
                return HttpNotFound();
            }
            return View(season);
        }

        //
        // POST: /Season/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Season season = db.Seasons.Find(id);
            db.Seasons.Remove(season);
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