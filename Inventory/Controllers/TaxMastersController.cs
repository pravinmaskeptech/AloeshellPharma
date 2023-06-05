using Inventory.Models;
using Syncfusion.EJ.Export;
using Syncfusion.JavaScript.Models;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Inventory.Controllers
{
    public class TaxMastersController : Controller
    {
        private InventoryModel db = new InventoryModel();

        // GET: TaxMasters
        public ActionResult Index()
        {
            try
            {             
                var taxMaster = new List<TaxMaster>(db.TaxMasters);
                ViewBag.datasource = (from tax in taxMaster
                                      orderby tax.HSNCode descending
                                      select new { HSNCode = tax.HSNCode, TaxPercent = tax.TaxPercent, IsActive = tax.IsActive }
                                    ).ToList();
                return View();
            }
            catch (Exception Ex)
            {
                ViewBag.datasource = null;
                var msg1 = Ex.Message.ToString();
                TempData["Msg"] = msg1;
            }
            return View();
        }
        public ActionResult Create()
        {
            return View();
        }

        // POST: TaxMasters/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( TaxMaster taxMaster)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    taxMaster.IsActive = true;
                    taxMaster.CreatedBy = User.Identity.Name;
                    taxMaster.CreatedDate = DateTime.Today;
                    db.TaxMasters.Add(taxMaster);
                    db.SaveChanges();
                    TempData["Temp"] = "Tax Details Save Successfully";
                    return RedirectToAction("Index");
                }
                TempData["Msg"] = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));               
                return View(taxMaster);
            }
            catch (Exception Ex)            {
               
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return View();
            }
        }

        // GET: TaxMasters/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TaxMaster taxMaster = db.TaxMasters.Find(id);
            if (taxMaster == null)
            {
                return HttpNotFound();
            }
            return View(taxMaster);
        }

        // POST: TaxMasters/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit( TaxMaster taxMaster)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    taxMaster.UpdatedBy = User.Identity.Name;
                    taxMaster.UpdateDate = DateTime.Today;
                    db.Entry(taxMaster).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Temp"] = "Tax Details Update Successfully";
                    return RedirectToAction("Index");
                }
                TempData["Msg"] = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));               
                return View(taxMaster);

            }
            catch (Exception Ex)
            {
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;              
                return View(taxMaster);
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
        #region -Check Duplicate HSN Code-
        public ActionResult CheckDuplicateName( string Mode, string Code)
        {
            try
            {
                if (Mode == "Edit")
                {
                    var result = db.TaxMasters.Where(f => f.HSNCode == Code ).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.TaxMasters.Where(f => f.HSNCode == Code ).ToList();
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
        #region -Export Excel-
        [ValidateInput(false)]
        public void ExportToExcel(string GridModel)
        {
            ExcelExport exp = new ExcelExport();
            var taxMaster = new List<TaxMaster>(db.TaxMasters);
            var DataSource = (from tax in taxMaster
                                  orderby tax.HSNCode descending
                                  select new { HSNCode = tax.HSNCode, TaxPercent = tax.TaxPercent, IsActive = tax.IsActive }
                                ).ToList();

            GridProperties obj = ConvertGridObject(GridModel);
            exp.Export(obj, DataSource, "taxMasters.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }
        private GridProperties ConvertGridObject(string gridProperty)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            IEnumerable div = (IEnumerable)serializer.Deserialize(gridProperty, typeof(IEnumerable));
            GridProperties gridProp = new GridProperties();
            foreach (KeyValuePair<string, object> ds in div)
            {
                var property = gridProp.GetType().GetProperty(ds.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (property != null)
                {
                    Type type = property.PropertyType;
                    string serialize = serializer.Serialize(ds.Value);
                    object value = serializer.Deserialize(serialize, type);
                    property.SetValue(gridProp, value, null);
                }
            }
            return gridProp;
        }
        #endregion
    }
}
