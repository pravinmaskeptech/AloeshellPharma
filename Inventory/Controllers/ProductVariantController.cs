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
    public class ProductsController : Controller
    {
        private InventoryModel db = new InventoryModel();

        public ActionResult Index()
        {
            var categorymaster = new List<Category>(db.Categories);
            var producrMaster = new List<Products>(db.ProductVariant);
            ViewBag.datasource = (from product in producrMaster
                                  where product.IsActive == true
                                  join Categori in categorymaster on product.CategoryId equals Categori.CategoryId into category
                                  from cat in category.DefaultIfEmpty()
                                  orderby product.VariantID descending
                                  select new
                                  {
                                      VariantID = product.VariantID,
                                      ProductCode = product.ProductCode,
                                      StandardWarranty = product.StandardWarranty,
                                      VariantName = product.VariantName,
                                      ModelName = product.ModelName,
                                      ClosingQuantity = product.ClosingQuantity,
                                      Weight = product.Weight,
                                      Size = product.Size,
                                      Discount = product.Discount,
                                      ReorderLevel = product.ReorderLevel,
                                      Note = product.Note,
                                      IsActive = product.IsActive,
                                      SellingPrice = product.SellingPrice,
                                      MaxLevel = product.MaxLevel,
                                      LeadTimePurchase = product.LeadTimePurchase,
                                      LeadTimeSell = product.LeadTimeSell,
                                      OutwardQuantity = product.OutwardQuantity,
                                      InwardQuantity = product.InwardQuantity,
                                      OpeningQuantity = product.OpeningQuantity,
                                      HsnCode = product.HsnCode,
                                      Description = product.Description,
                                      PurchasePrice = product.PurchasePrice,
                                      UOM = product.UOM,
                                      CategoryId = cat == null ? string.Empty : cat.CategoryName
                                  }
                                ).ToList();
            return View();
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Products products = db.ProductVariant.Find(id);
            if (products == null)
            {
                return HttpNotFound();
            }
            return View(products);
        }

        public ActionResult Create()
        {
            ViewBag.ProdDataSource = db.Product.Where(a => a.IsActive == true).ToList();
            ViewBag.CatDataSource = db.Categories.Where(a => a.IsActive == true).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Products products)
        {
            try
            {
                ProductMaster productMaster = db.Product.Where(a => a.ProductCode == products.ProductCode).FirstOrDefault();
                products.IsActive = true;
                products.CreatedBy = User.Identity.Name;
                products.CreatedDate = DateTime.Today;
                products.SerialNoApplicable = true;
                products.Discount = 0;
                products.DiscountIn = "";
                products.ProductClass = "";
                products.ProductType = "";
                products.DamageQty = 0;
                products.HsnCode = productMaster.HSNCode;
                products.IssuedToProductionQty = 0;
                products.ManufactureGRNQty = 0;
                products.BatchNoApplicable = true;
                products.Image = "";
                db.ProductVariant.Add(products);
                db.SaveChanges();
                TempData["Temp"] = "Variant Save Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception Ex)
            {
                ViewBag.ProdDataSource = db.Product.Where(a => a.IsActive == true).ToList();
                ViewBag.CatDataSource = db.Categories.Where(a => a.IsActive == true).ToList();
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return View();
            }
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int id)
        {
            ViewBag.ProdDataSource = db.Product.Where(a => a.IsActive == true).ToList();
            ViewBag.CatDataSource = db.Categories.Where(a => a.IsActive == true).ToList();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Products products = db.ProductVariant.Find(id);
            TempData["ProductClass"] = products.ProductClass;
            if (products == null)
            {
                return HttpNotFound();
            }
            return View(products);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Products products)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    products.UpdatedBy = User.Identity.Name;
                    products.UpdateDate = DateTime.Today;
                    db.Entry(products).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Temp"] = "Product Update Successfully";
                    return RedirectToAction("Index");
                }
                TempData["Msg"] = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(products);

            }
            catch (Exception Ex)
            {
                ViewBag.ProdDataSource = db.Product.Where(a => a.IsActive == true).ToList();
                ViewBag.CatDataSource = db.Categories.Where(a => a.IsActive == true).ToList();
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return View(products);
            }
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
        {
            try
            {
                Products products = db.ProductVariant.Where(a => a.VariantID == id).FirstOrDefault();
                if(products == null)
                {
                    TempData["error"] = "Delete  failed.";
                    return RedirectToAction("Index");
                }
                products.IsActive = false;
                db.SaveChanges();
                TempData["success"] = "Record Deleted Successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["error"] = "Delete  failed.";
                return View();
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
        #region -Check Duplicate Name-
        public ActionResult CheckDuplicateName(string Name, string Mode, string Code)
        {
            try
            {
                if (Mode == "Edit")
                {

                    var result = db.ProductVariant.Where(f => f.VariantName == Name).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.ProductVariant.Where(f => f.VariantName == Name).ToList();
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
        public JsonResult CheckHSN(string hsnCode)
        {
            List<TaxMaster> result = new List<TaxMaster>();
            if (hsnCode != "")
            {
                result = db.TaxMasters.Where(x => x.HSNCode.Equals(hsnCode)).ToList();
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult ChkCategory(string Cat)
        {
            List<Category> result = new List<Category>();
            if (Cat != "")
            {
                result = db.Categories.Where(x => x.CategoryName.Equals(Cat)).ToList();
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetData(string Id)
        {
            try
            {
                var result = db.ProductVariant.Where(a => a.VariantID.Equals(Id)).FirstOrDefault();
                //var result = db.ProductVariant.Where(a => a.ProductCode.Equals(Id)).FirstOrDefault();
                return new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            catch (Exception Ex)
            {

                var msg = Ex.Message.ToString();
                var result = new { error = "error", msg = msg };
                TempData["ErrorMsg"] = "Error";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult bindCategory(string Id)
        {
            var CatId = db.ProductVariant.Where(a => a.ProductCode.Equals(Id)).Select(x => x.CategoryId).Single().ToString();
            int CategoryId = Convert.ToInt32(CatId);
            var result = db.Categories.Where(a => a.CategoryId.Equals(CategoryId)).Select(x => x.CategoryName).Single().ToString();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [ValidateInput(false)]
        public void ExportToExcel(string GridModel)
        {
            ExcelExport exp = new ExcelExport();
            var categorymaster = new List<Category>(db.Categories);
            var producrMaster = new List<Products>(db.ProductVariant);
            var Datasource = (from product in producrMaster
                              join Categori in categorymaster on product.CategoryId equals Categori.CategoryId into category
                              from cat in category.DefaultIfEmpty()
                              orderby product.ProductCode descending
                              select new { ProductCode = product.ProductCode, VariantName = product.VariantName, ModelName = product.ModelName, ClosingQuantity = product.ClosingQuantity, Weight = product.Weight, Size = product.Size, Discount = product.Discount, ReorderLevel = product.ReorderLevel, Note = product.Note, IsActive = product.IsActive, SellingPrice = product.SellingPrice, MaxLevel = product.MaxLevel, LeadTimePurchase = product.LeadTimePurchase, LeadTimeSell = product.LeadTimeSell, OutwardQuantity = product.OutwardQuantity, InwardQuantity = product.InwardQuantity, OpeningQuantity = product.OpeningQuantity, HsnCode = product.HsnCode, CategoryId = cat == null ? string.Empty : cat.CategoryName, DiscountIn = product.DiscountIn }
                                ).ToList();
            GridProperties obj = ConvertGridObject(GridModel);
            obj.Columns[8].DataSource = db.Categories.ToList();
            exp.Export(obj, Datasource, "Categories.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
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
    }
}
