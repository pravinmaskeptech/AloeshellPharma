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
    public class DiscountStructureController : Controller
    {
        private InventoryModel db = new InventoryModel();

        // GET: Products
        public ActionResult Index()
        {
            ViewBag.ProductSrc = db.Products.Where(a => a.IsActive == true).ToList();
            var Master = new List<DiscountStructure>(db.DiscountStructure);
            ViewBag.datasource = (from master in Master

                                  orderby master.CreatedDate descending
                                  select new { DiscountStructureID = master.DiscountStructureID, ProductName = master.ProductName, DoctorRefCodePt = master.DoctorRefCodePt, NutraRefCodePt = master.NutraRefCodePt, MRPoints = master.MRPoints, CustDiscount = master.CustDiscount, MRP = master.MRP, EcomDiscount = master.EcomDiscount }
                                ).ToList();
            return View();
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DiscountStructure products = db.DiscountStructure.Find(id);
            if (products == null)
            {
                return HttpNotFound();
            }
            return View(products);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            ViewBag.ProductSrc = db.Products.Where(a => a.IsActive == true).ToList();
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DiscountStructure products)
        {
            try
            {

                products.CreatedBy = User.Identity.Name;
                products.CreatedDate = DateTime.Today;
                db.DiscountStructure.Add(products);             
                db.SaveChanges();
                TempData["Temp"] = "Discount Structure Save Successfully";
                return RedirectToAction("Index");

      
            }
            catch (Exception Ex)
            {
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return View();
            }
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int id)
        {
            ViewBag.ProductSrc = db.Products.Where(a => a.IsActive == true).ToList();
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DiscountStructure products = db.DiscountStructure.Find(id);
            if (products == null)
            {
                return HttpNotFound();
            }
            //Products prd = db.Products.Where(a=>a.ProductName == products.ProductName).FirstOrDefault();
            //products.ProductName = prd.ProductCode;
            return View(products);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DiscountStructure products)
        {
            try
            {
               
                products.UpdatedBy = User.Identity.Name;
                products.UpdatedDate = DateTime.Today;
                db.Entry(products).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Temp"] = "Discount Structure Update Successfully";
                return RedirectToAction("Index");


            }
            catch (Exception Ex)
            {
               
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return View(products);
            }
        }

        public JsonResult DeleteRecord(int ID)
        {
            var x = db.DiscountStructure.Where(a => a.DiscountStructureID == ID).FirstOrDefault();
            db.DiscountStructure.Remove(x);
            db.SaveChanges();
            var result = new { Message = "success" };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DiscountStructure products = db.DiscountStructure.Find(id);
            if (products == null)
            {
                return HttpNotFound();
            }
            return View(products);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DiscountStructure products = db.DiscountStructure.Find(id);
            db.DiscountStructure.Remove(products);
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
       
       
       
        //[ValidateInput(false)]
        //public void ExportToExcel(string GridModel)
        //{
        //    ExcelExport exp = new ExcelExport();
        //    var categorymaster = new List<Category>(db.Categories);
        //    var producrMaster = new List<Products>(db.Products);
        //    var Datasource = (from product in producrMaster
        //                      join Categori in categorymaster on product.CategoryId equals Categori.CategoryId into category
        //                      from cat in category.DefaultIfEmpty()
        //                      orderby product.ProductCode descending
        //                      select new { ProductCode = product.ProductCode, ProductName = product.ProductName, ModelName = product.ModelName, ClosingQuantity = product.ClosingQuantity, Weight = product.Weight, Size = product.Size, Discount = product.Discount, ReorderLevel = product.ReorderLevel, Note = product.Note, IsActive = product.IsActive, SellingPrice = product.SellingPrice, MaxLevel = product.MaxLevel, LeadTimePurchase = product.LeadTimePurchase, LeadTimeSell = product.LeadTimeSell, OutwardQuantity = product.OutwardQuantity, InwardQuantity = product.InwardQuantity, OpeningQuantity = product.OpeningQuantity, HsnCode = product.HsnCode, CategoryId = cat == null ? string.Empty : cat.CategoryName, DiscountIn = product.DiscountIn, SerialNoApplicable = product.SerialNoApplicable }
        //                        ).ToList();
        //    GridProperties obj = ConvertGridObject(GridModel);
        //    obj.Columns[8].DataSource = db.Categories.ToList();
        //    exp.Export(obj, Datasource, "Categories.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        //}
        //private GridProperties ConvertGridObject(string gridProperty)
        //{
        //    JavaScriptSerializer serializer = new JavaScriptSerializer();
        //    IEnumerable div = (IEnumerable)serializer.Deserialize(gridProperty, typeof(IEnumerable));
        //    GridProperties gridProp = new GridProperties();
        //    foreach (KeyValuePair<string, object> ds in div)
        //    {
        //        var property = gridProp.GetType().GetProperty(ds.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        //        if (property != null)
        //        {
        //            Type type = property.PropertyType;
        //            string serialize = serializer.Serialize(ds.Value);
        //            object value = serializer.Deserialize(serialize, type);
        //            property.SetValue(gridProp, value, null);
        //        }
        //    }
        //    return gridProp;
        //}
    }
}
