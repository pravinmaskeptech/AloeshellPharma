using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Inventory.Models;
using Syncfusion.EJ.Export;
using Syncfusion.JavaScript.Models;
using Syncfusion.XlsIO;
using System.Web.Script.Serialization;
using System.Collections;
using System.Reflection;

namespace Inventory.Controllers
{
    public class StoreLocationsController : Controller
    {
        private InventoryModel db = new InventoryModel();

        // GET: StoreLocations
        public ActionResult Index()
        {
           // var Storelocatin=new List<StoreLocations>(db.StoreLocations.ToList());          

                                ViewBag.datasource = (from StoreLocation in db.StoreLocations
                                      join Warehous in db.Warehouses on StoreLocation.WarehouseId equals Warehous.WareHouseID into E
                                      from Store in E.DefaultIfEmpty()
                                      orderby StoreLocation.CreatedDate descending
                                                      select new { StoreLocationId = StoreLocation.StoreLocationId, WarehouseId = Store.WareHouseName, StoreLocation = StoreLocation.StoreLocation, Description = StoreLocation.Description, IsActive = StoreLocation.IsActive }).ToList();                
            return View();
        }
        
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StoreLocations storeLocations = db.StoreLocations.Find(id);
            if (storeLocations == null)
            {
                return HttpNotFound();
            }
            return View(storeLocations);
        }
     
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.Warehouse = db.Warehouses.ToList();
            return View();
        }
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(StoreLocations storeLocations)
        {
            try
            {
                storeLocations.CreatedBy = User.Identity.Name;
                storeLocations.CreatedDate = DateTime.Now;
                storeLocations.IsActive = true;
                if (ModelState.IsValid)
                {                    
                    db.StoreLocations.Add(storeLocations);
                    db.SaveChanges();
                    TempData["Temp"] = "Store Location Inserted Successfully";
                    return RedirectToAction("Index");
                }
                TempData["Msg"] = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                ViewBag.Warehouse = db.Warehouses.ToList();
                return View(storeLocations);
            }
            catch (Exception EX)
            {

                return View(storeLocations);
            }
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StoreLocations storeLocations = db.StoreLocations.Find(id);
            ViewBag.Warehouse = db.Warehouses.ToList();
            if (storeLocations == null)
            {
                return HttpNotFound();
            }
            return View(storeLocations);
        }
     
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(StoreLocations storeLocations)
        {
            storeLocations.ModifiedBy = User.Identity.Name;
            storeLocations.ModifiedDate = DateTime.Now;
            if (ModelState.IsValid)
            {                
                db.Entry(storeLocations).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Temp"] = "Store Location Updated Successfully";
                return RedirectToAction("Index");
            }
            ViewBag.Warehouse = db.Warehouses.ToList();
            TempData["Msg"] = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return View(storeLocations);
        }

        public ActionResult CheckDuplicateName(string Name, string Mode, int Code)
        {
            try
            {
                if (Mode == "Edit")
                {
                    var result = db.StoreLocations.Where(f => f.StoreLocation == Name && f.StoreLocationId != Code).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.StoreLocations.Where(f => f.StoreLocation == Name).ToList();
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
        [HttpGet]
        public ActionResult GetData(int Id)
        {
            var result = db.StoreLocations.Where(x => x.StoreLocationId == Id).Select(x => new { x.Description }).FirstOrDefault();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [ValidateInput(false)]
        public void ExportToExcel(string GridModel)
        {
            ExcelExport exp = new ExcelExport();
            
            var DataSource = (from StoreLocation in db.StoreLocations
                              join Warehous in db.Warehouses on StoreLocation.WarehouseId equals Warehous.WareHouseID into E
                              from Store in E.DefaultIfEmpty()
                              orderby StoreLocation.CreatedDate descending
                              select new { StoreLocationId = StoreLocation.StoreLocationId, WarehouseId = Store.WareHouseName, StoreLocation = StoreLocation.StoreLocation, Description = StoreLocation.Description, IsActive = StoreLocation.IsActive }).ToList();                

            GridProperties obj = ConvertGridObject(GridModel);
            exp.Export(obj, DataSource, "StoreLocation.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
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
