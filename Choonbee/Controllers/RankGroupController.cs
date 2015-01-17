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
    public class RankGroupController : Controller
    {
        private ChoonbeeEdmx db = new ChoonbeeEdmx();

        //
        // GET: /RankGroup/

        public ActionResult Index()
        {
            return View(db.RankGroups.ToList());
        }

        //
        // GET: /RankGroup/Details/5

        public ActionResult Details(int id = 0)
        {
            var rankgroup = db.RankGroups.Find(id);
            ViewBag.TheseRanks = rankgroup.Ranks.ToList();

            if (rankgroup == null)
            {
                return HttpNotFound();
            }
            return View(rankgroup);
        }

        //
        // GET: /RankGroup/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /RankGroup/Create

        [HttpPost]
        public ActionResult Create(RankGroup rankgroup)
        {
            if (ModelState.IsValid)
            {
                db.RankGroups.Add(rankgroup);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(rankgroup);
        }

        //
        // GET: /RankGroup/Edit/5

        public ActionResult Edit(int id = 0)
        {
            var rankgroup = db.RankGroups.Find(id);
            var myRanks = rankgroup.Ranks.ToList();
            List<int> myRankIds = myRanks.Select(s => s.RankId).ToList();

            var allRanks = db.Ranks.Where(r => !myRankIds.Contains(r.RankId)).ToList();
            
            ViewBag.AllAvailableRanks = allRanks.Select(r => new SelectListItem { Value = r.RankId.ToString(), Text = r.RankName }).ToList();
            ViewBag.TheseRanks = rankgroup.Ranks.ToList();

            if (rankgroup == null)
            {
                return HttpNotFound();
            }
            return View(rankgroup);
        }

        //
        // POST: /RankGroup/Edit/5

        [HttpPost]
        public ActionResult Edit(RankGroup rankgroup)
        {
            if (ModelState.IsValid)
            {
                db.Entry(rankgroup).State = EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("Edit", new { id = rankgroup.RankGroupId });
        }

        [HttpPost]
        public ActionResult AddRank(int rankGroupId, int rankId)
        {
            var item = db.RankGroups.Find(rankGroupId);
            item.Ranks.Add(db.Ranks.Find(rankId));
            
            db.Entry(item).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Edit", new { id = rankGroupId });
        }

        [HttpPost]
        public ActionResult RemoveRank(int rankGroupId, int rankId)
        {
            var item = db.RankGroups.Find(rankGroupId);
            item.Ranks.Remove(db.Ranks.Find(rankId));

            db.Entry(item).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Edit", new { id = rankGroupId });
        }

        //
        // GET: /RankGroup/Delete/5

        public ActionResult Delete(int id = 0)
        {
            RankGroup rankgroup = db.RankGroups.Find(id);
            if (rankgroup == null)
            {
                return HttpNotFound();
            }
            return View(rankgroup);
        }

        //
        // POST: /RankGroup/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            RankGroup rankgroup = db.RankGroups.Find(id);
            rankgroup.Ranks.Clear();
            db.RankGroups.Remove(rankgroup);
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