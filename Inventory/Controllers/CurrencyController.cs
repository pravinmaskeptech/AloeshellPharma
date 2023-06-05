using Inventory.Models;
using Syncfusion.EJ.Export;
using Syncfusion.JavaScript.Models;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Inventory.Controllers
{
    public class CurrencyController : Controller
    {
        private InventoryModel db = new InventoryModel();
        public ActionResult Index()
        {
            try
            {
                ViewBag.datasource = db.Currencies.ToList();
            }
            catch (Exception Ex)
            {
                ViewBag.datasource = null;   
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;                
            }
            return View();
        }
    
        public ActionResult Create()
        {
            return View();
        }
      
        [HttpPost]
        public ActionResult Create(Currency currency)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    currency.IsActive = true;
                    currency.CreatedBy = "Test User";
                    currency.CreatedDate = DateTime.Now;
                    db.Currencies.Add(currency);
                    db.SaveChanges();
                    TempData["Temp"] = "Currency Created Successfully";
                    return RedirectToAction("Index");
                }
                catch (Exception Ex)
                {
                    var msg = Ex.Message.ToString();
                    TempData["Msg"] = msg;
                    return View();
                }
            }
            else
            {
                return View();
            }
        }
     
        public ActionResult Edit(int id)
        {
            ViewBag.Freeze = new List<SelectListItem>
                                                            {
                                                                new SelectListItem{ Text="False", Value = "False" },
                                                                new SelectListItem{ Text="True", Value = "True" }
                                                            };
            Currency currency = db.Currencies.Find(id);
            return View(currency);
        }
      
        [HttpPost]
        public ActionResult Edit(Currency currency)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    currency.UpdatedBy = "test user";
                    currency.UpdatedDate = DateTime.Now;
                    db.Entry(currency).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Temp"] = "Currency Updated Successfully";
                    return RedirectToAction("Index");
                }
                catch (Exception Ex)
                {
                    ViewBag.Freeze = new List<SelectListItem>
                                                            {
                                                                new SelectListItem{ Text="False", Value = "False" },
                                                                new SelectListItem{ Text="True", Value = "True" }
                                                            };

                    var msg = Ex.Message.ToString();
                    TempData["Msg"] = msg;

                    return View(currency);
                }
            }
            else
            {
                ViewBag.Freeze = new List<SelectListItem>
                                                            {
                                                                new SelectListItem{ Text="False", Value = "False" },
                                                                new SelectListItem{ Text="True", Value = "True" }
                                                            };
                return View(currency);
            }
        }
        public ActionResult CheckDuplicateName(string Name, string Mode, int Id)
        {
            try
            {
                if (Mode == "Edit")
                {
                    var result = db.Currencies.Where(x => x.CurrencyName == Name && x.CurrencyID != Id).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.Currencies.Where(x => x.CurrencyName == Name).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
            }
           catch (Exception Ex)
            {                
                var str = Ex.Message.ToString();
                return Json(str, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult DuplicteSymbol(string Name, string Mode, int Id)
        {
            try
            {
                if (Mode == "Edit")
                {
                    var result = db.Currencies.Where(x => x.CurrencySymbol == Name && x.CurrencyID != Id).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.Currencies.Where(x => x.CurrencySymbol == Name).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception Ex)
            {                
                var str = Ex.Message.ToString();
                return Json(str, JsonRequestBehavior.AllowGet);
            }
        }
        [ValidateInput(false)]
        public void ExportToExcel(string GridModel)
        {
            ExcelExport exp = new ExcelExport();
            var DataSource = db.Currencies.ToList();
            GridProperties obj = ConvertGridObject(GridModel);
            exp.Export(obj, DataSource, "Currrency.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
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
