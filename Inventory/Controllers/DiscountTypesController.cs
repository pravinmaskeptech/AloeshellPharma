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
    public class DiscountTypesController : Controller
    {
        private InventoryModel db = new InventoryModel();

        // GET: DiscountTypes
        public ActionResult Index()
        {
            var discountMaster = new List<DiscountTypes>(db.DiscountTypes);
            ViewBag.datasource = (from disc in discountMaster
                                  orderby disc.DiscountId descending
                                  select new { DiscountId = disc.DiscountId, DiscountType = disc.DiscountType, Description = disc.Description, IsActive = disc.IsActive }
                                ).ToList();
            return View();
        }

        // GET: DiscountTypes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DiscountTypes discountTypes = db.DiscountTypes.Find(id);
            if (discountTypes == null)
            {
                return HttpNotFound();
            }
            return View(discountTypes);
        }

        // GET: DiscountTypes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DiscountTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( DiscountTypes discountTypes)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    discountTypes.IsActive = true;
                    discountTypes.CreatedBy = User.Identity.Name;
                    discountTypes.CreatedDate = DateTime.Today;
                    db.DiscountTypes.Add(discountTypes);
                    db.SaveChanges();
                    TempData["Temp"] = "Discount Type Save Successfully";
                    return RedirectToAction("Index");
                }
                TempData["Msg"] = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(discountTypes);
            }
            catch (Exception Ex)
            {

                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return View();
            }
        }

        // GET: DiscountTypes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DiscountTypes discountTypes = db.DiscountTypes.Find(id);
            if (discountTypes == null)
            {
                return HttpNotFound();
            }
            return View(discountTypes);
        }

        // POST: DiscountTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "DiscountId,DiscountType,Description,CreatedBy,CreatedDate,UpdatedBy,UpdateDate,IsActive")] DiscountTypes discountTypes)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    discountTypes.UpdatedBy = User.Identity.Name;
                    discountTypes.UpdateDate = DateTime.Today;
                    db.Entry(discountTypes).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Temp"] = "discount Types Update Successfully";
                    return RedirectToAction("Index");
                }
                TempData["Msg"] = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(discountTypes);

            }
            catch (Exception Ex)
            {
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return View(discountTypes);
            }
        }
        #region -Check Duplicate Discount Type-
        public ActionResult CheckDuplicateName(string Name, string Mode, int Code)
        {
            try
            {
                if (Mode == "Edit")
                {
                    var result = db.DiscountTypes.Where(f => f.DiscountType == Name && f.DiscountId != Code).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.DiscountTypes.Where(f => f.DiscountType == Name).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception Ex)
            {
                var str = Ex.InnerException.Message.ToString();
                if (str == null)
                    str = Ex.Message.ToString();
                return Json(str, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        // GET: DiscountTypes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DiscountTypes discountTypes = db.DiscountTypes.Find(id);
            if (discountTypes == null)
            {
                return HttpNotFound();
            }
            return View(discountTypes);
        }

        // POST: DiscountTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DiscountTypes discountTypes = db.DiscountTypes.Find(id);
            db.DiscountTypes.Remove(discountTypes);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        #region -get Description-
        public JsonResult getDescription(int Id)
        {
            try
            {
                var result = db.DiscountTypes.Where(x => x.DiscountId ==  Id).Select(a => a.Description).Single();
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
        #endregion
    }
}
