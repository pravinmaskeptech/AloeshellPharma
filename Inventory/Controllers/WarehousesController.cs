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
    public class WarehousesController : Controller
    {
        private InventoryModel db = new InventoryModel();

        // GET: Warehouses
        public ActionResult Index()
        {

            var WarehousesMaster = new List<Warehouse>(db.Warehouses);
            ViewBag.datasource = (from cat in WarehousesMaster
                                  orderby cat.WareHouseID descending
                                  select new { WareHouseID = cat.WareHouseID, WareHouseName = cat.WareHouseName, Address = cat.Address, City = cat.City, Pincode = cat.Pincode, Email = cat.Email, Phone = cat.Phone, State = cat.State, Country = cat.Country, IsPrimary = cat.IsPrimary }
                                ).ToList();
            return View();          
            
        }
        
        // GET: Warehouses/Create
        public ActionResult Create()
        {
            try
            {
                return View();
            }

            catch (Exception Ex)
            {

                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return RedirectToAction("Index");
            }
        }

        // POST: Warehouses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Warehouse warehouse)
        {
            try
            {
                if (ModelState.IsValid)
                {                   
                            var count = db.Warehouses.Count();
                            count = count + 1;
                            string Code = count.ToString();
                            Code = Code.PadLeft(4, '0');
                            warehouse.WareHouseID = "W" + Code;
                            db.Warehouses.Add(warehouse);
                            db.SaveChanges();
                            TempData["Temp"] = "Warehouse Save Successfully";
                            return RedirectToAction("Index");                       
                }

                TempData["Msg"] = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(warehouse);
            }
            catch (Exception Ex)               
            {
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return View();
            }

        }

        // GET: Warehouses/Edit/5
        public ActionResult Edit(string id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Warehouse warehouse = db.Warehouses.Find(id);
                if (warehouse == null)
                {
                    return HttpNotFound();
                }
                return View(warehouse);
            }
            catch (Exception Ex)
            {

                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return RedirectToAction("Index");
            }
        }

        // POST: Warehouses/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit( Warehouse warehouse)
        {
            try
            {
                if (ModelState.IsValid)
                {                    
                            db.Entry(warehouse).State = EntityState.Modified;
                            db.SaveChanges();
                            TempData["Temp"] = "Warehouse Update Successfully";
                            return RedirectToAction("Index");                                            
                }
                TempData["Msg"] = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(warehouse);
            }
            catch (Exception Ex)
            {
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return View(warehouse);
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
        #region -Check Duplicate Email-
        public ActionResult CheckDuplicateEmail(string Email, string Mode, string Code)
        {
            try
            {
                if (Mode == "Edit")
                {

                    var result = db.Warehouses.Where(f => f.Email == Email && f.WareHouseID != Code).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.Warehouses.Where(f => f.Email == Email).ToList();
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
        #region -Check Duplicate Name-
        public ActionResult CheckDuplicateName(string Name, string Mode, string Code)
        {
            try
            {
                if (Mode == "Edit")
                {

                    var result = db.Warehouses.Where(f => f.WareHouseName == Name && f.WareHouseID != Code).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.Warehouses.Where(f => f.WareHouseName == Name).ToList();
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
        #region -Get All Data-
        public JsonResult GetAllData(string Id)
        {
            try
            {
                var result = db.Warehouses.Where(a => a.WareHouseID.Equals(Id)).FirstOrDefault();
                return new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            catch (Exception Ex)
            {

                  var  msg = Ex.Message.ToString();
                var result = new { error = "error", msg = msg };
                TempData["ErrorMsg"] = "Error";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region -Export Excel-
        [ValidateInput(false)]
        public void ExportToExcel(string GridModel)
        {
            ExcelExport exp = new ExcelExport();
            var WarehouseMaster = new List<Warehouse>(db.Warehouses);
            var DataSource = (from house in WarehouseMaster
                                  orderby house.WareHouseID descending
                                  select new { WareHouseID = house.WareHouseID, WareHouseName = house.WareHouseName, City = house.City, Pincode = house.Pincode, Address = house.Address, Phone = house.Phone, Email = house.Email, State = house.State, Country = house.Country, PanNo = house.IsPrimary, IsPrimary = house.IsPrimary }
                                ).ToList();

            GridProperties obj = ConvertGridObject(GridModel);
            exp.Export(obj, DataSource, "Warehouse.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
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
