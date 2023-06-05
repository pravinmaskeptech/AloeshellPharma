using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Inventory.Models;

namespace Inventory.Controllers
{
    public class TermsAndConditionsController : Controller
    {
        private InventoryModel db = new InventoryModel();

        // GET: TermsAndConditions
        public ActionResult Index()
        {
            var termsConditions = new List<TermsAndConditions>(db.TermsAndConditions);
            ViewBag.datasource = (from term in termsConditions
                                  orderby term.TermsId descending
                                  select new { TermsId = term.TermsId, Orders = term.Orders, TermsAndCondition = term.TermsAndCondition, IsActive = term.IsActive }
                                ).ToList();
            return View();
        }

        // GET: TermsAndConditions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TermsAndConditions termsAndConditions = db.TermsAndConditions.Find(id);
            if (termsAndConditions == null)
            {
                return HttpNotFound();
            }
            return View(termsAndConditions);
        }

        // GET: TermsAndConditions/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TermsAndConditions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TermsId,Orders,TermsAndCondition")] TermsAndConditions termsAndConditions)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    termsAndConditions.CreatedBy = User.Identity.Name;
                    termsAndConditions.CreatedDate = DateTime.Today;
                    termsAndConditions.IsActive = true;
                    db.TermsAndConditions.Add(termsAndConditions);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (Exception Ex)
            {
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return View();
            }
            return View(termsAndConditions);
        }

        // GET: TermsAndConditions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TermsAndConditions termsAndConditions = db.TermsAndConditions.Find(id);
            if (termsAndConditions == null)
            {
                return HttpNotFound();
            }
            return View(termsAndConditions);
        }

        // POST: TermsAndConditions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TermsId,Orders,TermsAndCondition,CreatedBy,CreatedDate,UpdatedBy,UpdateDate,IsActive")] TermsAndConditions termsAndConditions)
        {
            if (ModelState.IsValid)
            {
                db.Entry(termsAndConditions).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(termsAndConditions);
        }
        
        public JsonResult getTerms(int termsId)
        {
            try
            {
                var result = db.TermsAndConditions.Where(x => x.TermsId.Equals(termsId)).Select(a => a.TermsAndCondition).Single();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                var msg = Ex.Message.ToString();
                var result = new { error = "error", msg = msg };
                TempData["ErrorMsg"] = "Error";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
