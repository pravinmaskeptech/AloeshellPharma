using Inventory.Models;
using Syncfusion.EJ.Export;
using Syncfusion.JavaScript.Models;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
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

        // GET: Products
        public ActionResult Index()
        {
            var categorymaster = new List<Category>(db.Categories);
            var producrMaster = new List<Products>(db.Products);
            ViewBag.datasource = (from product in producrMaster.OrderBy(a=>a.ProductName)
                                  where product.IsActive == true
                                  join Categori in categorymaster on product.CategoryId equals Categori.CategoryId into category
                                  from cat in category.DefaultIfEmpty()
                                  orderby product.ProductCode descending
                                  select new { ProductType= product.ProductType, ProductCode = product.ProductCode,PurchasePrice = product.PurchasePrice,Prefix = product.Prefix,CurrentSerialNo = product.CurrentSerialNo, StandardWarranty = product.StandardWarranty, ProductName = product.ProductName, ModelName = product.ModelName, ClosingQuantity = product.ClosingQuantity, Weight = product.Weight, Size = product.Size, Discount = product.Discount, ReorderLevel = product.ReorderLevel, Note = product.Note, IsActive = product.IsActive, SellingPrice = product.SellingPrice, MaxLevel = product.MaxLevel, LeadTimePurchase = product.LeadTimePurchase, LeadTimeSell = product.LeadTimeSell, OutwardQuantity = product.OutwardQuantity, InwardQuantity = product.InwardQuantity, OpeningQuantity = product.OpeningQuantity, HsnCode = product.HsnCode, CategoryId = cat == null ? string.Empty : cat.CategoryName, DiscountIn = product.DiscountIn, SerialNoApplicable = product.SerialNoApplicable }
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
            Products products = db.Products.Find(id);
            if (products == null)
            {
                return HttpNotFound();
            }
            return View(products);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            ViewBag.datasource = db.suppliers.Where(a => a.IsActive == true).ToList();
            ViewBag.HsnDataSource = db.TaxMasters.Where(a => a.IsActive == true).ToList();
            ViewBag.CatDataSource = db.Categories.Where(a => a.IsActive == true).ToList();
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Products products)
        {
            try
            {
                var spname = db.suppliers.Where(a => a.SupplierID == products.SupplierId).FirstOrDefault();
                if(spname == null)
                {
                    ViewBag.datasource = db.suppliers.Where(a => a.IsActive == true).ToList();
                    ViewBag.HsnDataSource = db.TaxMasters.Where(a => a.IsActive == true).ToList();
                    ViewBag.CatDataSource = db.Categories.Where(a => a.IsActive == true).ToList();
                    var msg = "Select Supplier";
                    TempData["Msg"] = msg;
                    return View();
                }
                var Categoryname = db.Categories.Where(a => a.CategoryId == products.CategoryId).FirstOrDefault();
                if (Categoryname == null)
                {
                    ViewBag.datasource = db.suppliers.Where(a => a.IsActive == true).ToList();
                    ViewBag.HsnDataSource = db.TaxMasters.Where(a => a.IsActive == true).ToList();
                    ViewBag.CatDataSource = db.Categories.Where(a => a.IsActive == true).ToList();
                    var msg = "Select Category";
                    TempData["Msg"] = msg;
                    return View();
                }

                var count = db.Products.Count();
                count = count + 1;
                products.ProductCode = "P00" + count;

                products.ManufactureGRNQty = 0;
                products.AllocatedQty = 0;
                products.DamageQty = 0;
                products.IssuedToProductionQty = 0;
                products.Discount = 0;
                products.DiscountIn = "0";
                products.ModelName = "0";
                products.ProductClass = "Brought Out";
                products.ProductType = spname.SupplierName;
                products.StandardWarranty = 0;
                products.IsActive = true;
                products.CreatedBy = User.Identity.Name;
                products.CreatedDate = DateTime.Today;
                db.Products.Add(products);

                SupplierProductRelations SPRelations = new SupplierProductRelations();
                SPRelations.CreatedBy = User.Identity.Name;
                SPRelations.CreatedDate = DateTime.Today;
                SPRelations.SupplierId = products.SupplierId;
                SPRelations.Tax = products.GSTPer;
                SPRelations.Delivery = 0;
                SPRelations.Discount = products.Discount;
                SPRelations.DiscountIn = products.DiscountIn;
                SPRelations.ProductCode = products.ProductCode;
                SPRelations.ProductPrice = products.PurchasePrice;
                SPRelations.IsActive = products.IsActive;
                SPRelations.UpdatedBy = "";
                SPRelations.IsPrefered = false;
                db.SupplierProductRelations.Add(SPRelations);

                db.SaveChanges();
                TempData["Temp"] = "Product Save Successfully";
                return RedirectToAction("Index");

                //TempData["Msg"] = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                //return View(products);
            }
            catch (Exception Ex)
            {
                ViewBag.datasource = db.suppliers.Where(a => a.IsActive == true).ToList();
                ViewBag.HsnDataSource = db.TaxMasters.Where(a => a.IsActive == true).ToList();
                ViewBag.CatDataSource = db.Categories.Where(a => a.IsActive == true).ToList();
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return View();
            }
        }

        // GET: Products/Edit/5
        public ActionResult Edit(string id)
        {
            ViewBag.datasource = db.suppliers.Where(a => a.IsActive == true).ToList();
            ViewBag.HsnDataSource = db.TaxMasters.Where(a => a.IsActive == true).ToList();
            ViewBag.CatDataSource = db.Categories.Where(a => a.IsActive == true).ToList();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Products products = db.Products.Find(id);
            TempData["ProductClass"] = products.ProductClass;
            if (products == null)
            {
                return HttpNotFound();
            }
            var sp = db.suppliers.Where(a => a.SupplierID == products.SupplierId).FirstOrDefault();
            if (sp != null)
                products.str_Supplier = sp.SupplierName;

            var cat = db.Categories.Where(a => a.CategoryId == products.CategoryId).FirstOrDefault();
            if (cat != null)
                products.str_category = cat.CategoryName;

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
                var spname = db.suppliers.Where(a => a.SupplierID == products.SupplierId).FirstOrDefault();
                if (spname == null)
                {
                    TempData["Temp"] = "Select Supplier";
                    return View();
                }

                products.ManufactureGRNQty = 0;
                products.AllocatedQty = 0;
                products.DamageQty = 0;
                products.IssuedToProductionQty = 0;
                products.Discount = 0;
                products.DiscountIn = "0";
                products.ModelName = "0";
                products.ProductClass = "Brought Out";
                products.ProductType = spname.SupplierName; 
                products.StandardWarranty = 0;
                products.UpdatedBy = User.Identity.Name;
                products.UpdateDate = DateTime.Today;
                db.Entry(products).State = EntityState.Modified;



                SupplierProductRelations SPRelations = db.SupplierProductRelations.Where(a=>a.SupplierId == products.SupplierId && a.ProductCode == products.ProductCode).FirstOrDefault();
                if (SPRelations == null)
                {
                    SPRelations = new SupplierProductRelations();
                    SPRelations.CreatedBy = User.Identity.Name;
                    SPRelations.CreatedDate = DateTime.Today;
                    SPRelations.SupplierId = products.SupplierId;
                    SPRelations.Tax = products.GSTPer;
                    SPRelations.Delivery = 0;
                    SPRelations.Discount = products.Discount;
                    SPRelations.DiscountIn = products.DiscountIn;
                    SPRelations.ProductCode = products.ProductCode;
                    SPRelations.ProductPrice = products.PurchasePrice;
                    SPRelations.IsActive = products.IsActive;
                    SPRelations.UpdatedBy = "";
                    SPRelations.IsPrefered = false;
                    db.SupplierProductRelations.Add( SPRelations );

                }
                else
                {
                    SPRelations.CreatedBy = User.Identity.Name;
                    SPRelations.CreatedDate = DateTime.Today;
                    SPRelations.SupplierId = products.SupplierId;
                    SPRelations.Tax = products.GSTPer;
                    SPRelations.Delivery = 0;
                    SPRelations.Discount = products.Discount;
                    SPRelations.DiscountIn = products.DiscountIn;
                    SPRelations.ProductCode = products.ProductCode;
                    SPRelations.ProductPrice = products.PurchasePrice;
                    SPRelations.IsActive = products.IsActive;
                    SPRelations.UpdatedBy = "";
                    SPRelations.IsPrefered = false;
                    db.Entry(SPRelations).State = EntityState.Modified;

                }

                db.SaveChanges();
                TempData["Temp"] = "Product Update Successfully";
                return RedirectToAction("Index");


            }
            catch (Exception Ex)
            {
                ViewBag.HsnDataSource = db.TaxMasters.Where(a => a.IsActive == true).ToList();
                ViewBag.CatDataSource = db.Categories.Where(a => a.IsActive == true).ToList();
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return View(products);
            }
        }

        public JsonResult checkSupplier(string supplier)
        {
            var result = 0;
            if (supplier != "")
            {
                result = db.suppliers.Where(x => x.SupplierName.Equals(supplier)).Count();
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Products products = db.Products.Find(id);
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
            Products products = db.Products.Find(id);
            db.Products.Remove(products);
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
        #region -Check Duplicate Name-
        public ActionResult CheckDuplicateName(string Name, string Mode, string Code)
        {
            try
            {
                if (Mode == "Edit")
                {

                    var result = db.Products.Where(f => f.ProductName == Name && f.ProductCode != Code).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.Products.Where(f => f.ProductName == Name).ToList();
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
                var result = db.Products.Where(a => a.ProductCode.Equals(Id)).FirstOrDefault();
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

        public JsonResult bindSupplier(string Id)
        {
            var CatId = db.Products.Where(a => a.ProductCode.Equals(Id)).Select(x => x.SupplierId).Single().ToString();
            if (CatId == null) { return Json("", JsonRequestBehavior.AllowGet); }
            int CategoryId ;
            if (int.TryParse(CatId, out CategoryId))
            {
                CategoryId = Convert.ToInt32(CatId);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            
            var result = db.suppliers.Where(a => a.SupplierID.Equals(CategoryId)).Select(x => x.SupplierName);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult bindCategory(string Id)
        {
            //var CatId = db.Products.Where(a => a.ProductCode.Equals(Id)).Select(x => x.CategoryId).Single().ToString();
            //int CategoryId = Convert.ToInt32(CatId);

            var CatId = db.Products.Where(a => a.ProductCode.Equals(Id)).Select(x => x.CategoryId).Single().ToString();
            if (CatId == null) { return Json("", JsonRequestBehavior.AllowGet); }
            int CategoryId;
            if (int.TryParse(CatId, out CategoryId))
            {
                CategoryId = Convert.ToInt32(CatId);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }


            var result = db.Categories.Where(a => a.CategoryId.Equals(CategoryId)).Select(x => x.CategoryName).Single().ToString();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [ValidateInput(false)]
        public void ExportToExcel(string GridModel)
        {
            ExcelExport exp = new ExcelExport();
            var categorymaster = new List<Category>(db.Categories);
            var producrMaster = new List<Products>(db.Products);
            var Datasource = (from product in producrMaster
                              join Categori in categorymaster on product.CategoryId equals Categori.CategoryId into category
                              from cat in category.DefaultIfEmpty()
                              orderby product.ProductCode descending
                              select new { ProductCode = product.ProductCode, ProductName = product.ProductName, ModelName = product.ModelName, ClosingQuantity = product.ClosingQuantity, Weight = product.Weight, Size = product.Size, Discount = product.Discount, ReorderLevel = product.ReorderLevel, Note = product.Note, IsActive = product.IsActive, SellingPrice = product.SellingPrice, MaxLevel = product.MaxLevel, LeadTimePurchase = product.LeadTimePurchase, LeadTimeSell = product.LeadTimeSell, OutwardQuantity = product.OutwardQuantity, InwardQuantity = product.InwardQuantity, OpeningQuantity = product.OpeningQuantity, HsnCode = product.HsnCode, CategoryId = cat == null ? string.Empty : cat.CategoryName, DiscountIn = product.DiscountIn, SerialNoApplicable = product.SerialNoApplicable }
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
