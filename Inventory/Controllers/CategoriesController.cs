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
    public class CategoriesController : Controller
    {
        private InventoryModel db = new InventoryModel();

        // GET: Categories
        public ActionResult Index()
        {
            try
            {
                var CategoryMaster = new List<Category>(db.Categories);
                ViewBag.datasource = (from cat in CategoryMaster
                                      orderby cat.CategoryId descending
                                      select new { CategoryId=cat.CategoryId, CategoryName = cat.CategoryName, Description = cat.Description, IsActive = cat.IsActive }
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

        // GET: Categories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // GET: Categories/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( Category category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    category.IsActive = true;
                    category.CreatedBy = User.Identity.Name;
                    category.CreatedDate = DateTime.Today;
                    db.Categories.Add(category);
                    db.SaveChanges();
                    TempData["Temp"] = "Category Save Successfully";
                    return RedirectToAction("Index");
                }
                TempData["Msg"] = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(category);
            }
            catch (Exception Ex)
            {

                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return View();
            }
        }

        // GET: Categories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit( Category category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    category.UpdatedBy = User.Identity.Name;
                    category.UpdateDate = DateTime.Today;
                    db.Entry(category).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Temp"] = "Category Update Successfully";
                    return RedirectToAction("Index");
                }
                TempData["Msg"] = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(category);

            }
            catch (Exception Ex)
            {
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return View(category);
            }
        }
        #region -Export Excel-
        [ValidateInput(false)]
        public void ExportToExcel(string GridModel)
        {
            ExcelExport exp = new ExcelExport();
            var CategoryMaster = new List<Category>(db.Categories);
            var DataSource = (from cat in CategoryMaster
                                  orderby cat.CategoryId descending
                                  select new { CategoryId = cat.CategoryId, CategoryName = cat.CategoryName, Description = cat.Description, IsActive = cat.IsActive }
                                ).ToList();

            GridProperties obj = ConvertGridObject(GridModel);
            exp.Export(obj, DataSource, "Category .xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
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

        // GET: Categories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Category category = db.Categories.Find(id);
            db.Categories.Remove(category);
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
        public ActionResult CheckDuplicateName(string Name, string Mode, int Code)
        {
            try
            {
                if (Mode == "Edit")
                {
                    var result = db.Categories.Where(f => f.CategoryName == Name && f.CategoryId != Code).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.Categories.Where(f => f.CategoryName == Name ).ToList();
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
    }
}
