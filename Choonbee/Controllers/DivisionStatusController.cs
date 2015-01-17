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
    public class DivisionStatusController : Controller
    {
        private ChoonbeeEdmx db = new ChoonbeeEdmx();

        //
        // GET: /DivisionStatus/

        public ActionResult Index()
        {
            return View(db.DivisionStatuses.ToList());
        }

        //
        // GET: /DivisionStatus/Details/5

        public ActionResult Details(int id = 0)
        {
            DivisionStatus divisionstatus = db.DivisionStatuses.Find(id);
            if (divisionstatus == null)
            {
                return HttpNotFound();
            }
            return View(divisionstatus);
        }

        //
        // GET: /DivisionStatus/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /DivisionStatus/Create

        [HttpPost]
        public ActionResult Create(DivisionStatus divisionstatus)
        {
            if (ModelState.IsValid)
            {
                db.DivisionStatuses.Add(divisionstatus);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(divisionstatus);
        }

        //
        // GET: /DivisionStatus/Edit/5

        public ActionResult Edit(int id = 0)
        {
            DivisionStatus divisionstatus = db.DivisionStatuses.Find(id);
            if (divisionstatus == null)
            {
                return HttpNotFound();
            }
            return View(divisionstatus);
        }

        //
        // POST: /DivisionStatus/Edit/5

        [HttpPost]
        public ActionResult Edit(DivisionStatus divisionstatus)
        {
            if (ModelState.IsValid)
            {
                db.Entry(divisionstatus).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(divisionstatus);
        }

        //
        // GET: /DivisionStatus/Delete/5

        public ActionResult Delete(int id = 0)
        {
            DivisionStatus divisionstatus = db.DivisionStatuses.Find(id);
            if (divisionstatus == null)
            {
                return HttpNotFound();
            }
            return View(divisionstatus);
        }

        //
        // POST: /DivisionStatus/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            DivisionStatus divisionstatus = db.DivisionStatuses.Find(id);
            db.DivisionStatuses.Remove(divisionstatus);
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