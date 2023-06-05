using Inventory.Models;
using Syncfusion.EJ.Export;
using Syncfusion.JavaScript.Models;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Http.Results;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Inventory.Controllers
{
    public class ProductController : Controller
    {
        private InventoryModel db = new InventoryModel();

        // GET: Products
        public ActionResult Index()
        {
            var productmaster = new List<ProductMaster>(db.Product);
            var categorymaster = new List<Category>(db.Categories);
            ViewBag.datasource = (from product in productmaster
                                  where product.IsActive == true
                                  join Categori in categorymaster on product.CategoryId equals Categori.CategoryId into category
                                  from cat in category.DefaultIfEmpty()
                                  orderby product.ProductID descending
                                  select new {
                                      ProductID= product.ProductID,ProductCode = product.ProductCode, ProductName = product.ProductName, IsActive = product.IsActive, HSNCode = product.HSNCode, 
                                      CategoryId = cat == null ? string.Empty : cat.CategoryName, GSTPer = product.GSTPer, Description = product.Description }
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
            ProductMaster products = db.Product.Find(id);
            if (products == null)
            {
                return HttpNotFound();
            }
            return View(products);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            ViewBag.HsnDataSource = db.TaxMasters.Where(a => a.IsActive == true).ToList();
            ViewBag.CatDataSource = db.Categories.Where(a => a.IsActive == true).ToList();
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductMaster products)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var count = db.Product.Count();
                    count = count + 1;
                    products.ProductCode = "P" + count.ToString("D4");
                    products.IsActive = true;                
                    db.Product.Add(products);
                    db.SaveChanges();
                    TempData["Temp"] = $"{products.ProductCode} Save Successfully";
                    return RedirectToAction("Index");
                }
                TempData["Msg"] = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(products);
            }
            catch (Exception Ex)
            {
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
            ViewBag.HsnDataSource = db.TaxMasters.Where(a => a.IsActive == true).ToList();
            ViewBag.CatDataSource = db.Categories.Where(a => a.IsActive == true).ToList();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductMaster products = db.Product.Where(a => a.ProductCode == id).FirstOrDefault();
            if (products == null)
            {
                return HttpNotFound();
            }
            return View(products);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProductMaster products)
        {

            try
            {
                ProductMaster productMaster = db.Product.Where(a => a.ProductCode == products.ProductCode).FirstOrDefault();
                productMaster.ProductName = products.ProductName;
                productMaster.HSNCode = products.HSNCode;
                productMaster.CategoryId = products.CategoryId;
                productMaster.Description = products.Description;

                db.Entry(productMaster).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Temp"] = $"{products.ProductCode} updated Successfully";
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

        // GET: Products/Delete/5
        public ActionResult Delete(int? ID)
        {
            try
            {
                ProductMaster products = db.Product.Where(a => a.ProductID == ID).FirstOrDefault();
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

                    var result = db.Product.Where(f => f.ProductName == Name && f.ProductCode != Code).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.Product.Where(f => f.ProductName == Name).ToList();
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
                var result = db.Product.Where(a => a.ProductCode.Equals(Id) && a.IsActive == true).FirstOrDefault();
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
            var CatId = db.Product.Where(a => a.ProductCode.Equals(Id)).Select(x => x.CategoryId).Single().ToString();
            int CategoryId = Convert.ToInt32(CatId);
            var result = db.Categories.Where(a => a.CategoryId.Equals(CategoryId)).Select(x => x.CategoryName).Single().ToString();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [ValidateInput(false)]
        public void ExportToExcel(string GridModel)
        {
            ExcelExport exp = new ExcelExport();
            var categorymaster = new List<Category>(db.Categories);
            var producrMaster = new List<ProductMaster>(db.Product);
            var Datasource = (from product in producrMaster
                              join Categori in categorymaster on product.CategoryId equals Categori.CategoryId into category
                              from cat in category.DefaultIfEmpty()
                              orderby product.ProductCode descending
                              select new { ProductCode = product.ProductCode, ProductName = product.ProductName,  IsActive = product.IsActive, HsnCode = product.HSNCode, CategoryId = cat == null ? string.Empty : cat.CategoryName}
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
