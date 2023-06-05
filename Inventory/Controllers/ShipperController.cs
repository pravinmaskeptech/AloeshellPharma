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
    public class ShipperController : Controller
    {
        InventoryModel db = new InventoryModel();
        public ActionResult Index()
        {           
            try
            {
                ViewBag.datasource = (from shipper in db.Shippers
                                      join customer in db.Customers on shipper.CustomerId equals customer.CustomerID into cust
                                      from c in cust.DefaultIfEmpty()
                                      orderby shipper.CreatedDate descending
                                      select new { ShipperId = shipper.ShipperId, Name = shipper.Name, Customer = c.CustomerName, ContactPerson = shipper.ContactPerson, Country = shipper.Country, State = shipper.State, City = shipper.City, Freeze=shipper.Freeze }
                                   ).ToList();
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
            ViewBag.Customer = db.Customers.Where(x => x.IsActive == true).ToList();            
            return View();
        }
        
        [HttpPost]
        public ActionResult Create(Shipper shipper)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    shipper.Freeze = false;
                    shipper.CreatedBy = User.Identity.Name;
                    shipper.CreatedDate = DateTime.Now;
                    db.Shippers.Add(shipper);
                    db.SaveChanges();
                    TempData["Temp"] = "Shipper Created Successfully";
                    return RedirectToAction("Index");
                }
                catch (Exception Ex)
                {
                    ViewBag.Customer = db.Customers.Where(x => x.IsActive == true).ToList();
                    var msg = Ex.Message.ToString();
                    TempData["Msg"] = msg;
                    return View();
                }
            }
            else
            {
                ViewBag.Customer = db.Customers.Where(x => x.IsActive == true).ToList();
                return View(shipper);
            }
        }
        
        public ActionResult Edit(int id)
        {
            ViewBag.Freeze = new List<SelectListItem>
                                                            {
                                                                new SelectListItem{ Text="False", Value = "False" },
                                                                new SelectListItem{ Text="True", Value = "True" }
                                                            };
            ViewBag.Customer = db.Customers.Where(x => x.IsActive == true).ToList();
            var shipper = db.Shippers.Find(id);
            return View(shipper);
        }
        
        [HttpPost]
        public ActionResult Edit(Shipper shipper)
        {
            if (ModelState.IsValid)
            {
                try
                {                   
                    shipper.UpdatedBy = User.Identity.Name;
                    shipper.UpdationDate = DateTime.Now;
                    db.Entry(shipper).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Temp"] = "Shipper Updated Successfully";
                    return RedirectToAction("Index");
                }
                catch (Exception Ex)
                {
                    ViewBag.Freeze = new List<SelectListItem>
                                                            {
                                                                new SelectListItem{ Text="False", Value = "False" },
                                                                new SelectListItem{ Text="True", Value = "True" }
                                                            };
                    ViewBag.Customer = db.Customers.Where(x => x.IsActive == true).ToList();
                    var msg = Ex.Message.ToString();
                    TempData["Msg"] = msg;
                    return View();
                }
            }
            else
            {
                ViewBag.Freeze = new List<SelectListItem>
                                                            {
                                                                new SelectListItem{ Text="False", Value = "False" },
                                                                new SelectListItem{ Text="True", Value = "True" }
                                                            };
                ViewBag.Customer = db.Customers.Where(x => x.IsActive == true).ToList();
                return View(shipper);
            }
        }

        public ActionResult GetData(string Id)
        {            
            try
            {
                int ShipperId = Convert.ToInt32(Id);
                var shipper = db.Shippers.Where(x => x.ShipperId == ShipperId).Select(x => new { x.CustomerId, x.Freeze, x.Address}).FirstOrDefault();
                var customer = db.Customers.Where(x => x.CustomerID == shipper.CustomerId).Select(x => new { x.CustomerName }).FirstOrDefault();
                var result = new { CustomerName = customer.CustomerName, Address = shipper.Address, Freeze = shipper.Freeze };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception EX)
            {
                var result = EX.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult CheckDuplicateName(string Name, string Mode, int Id)
        {
            try
            {
                if (Mode == "Edit")
                {
                    var result = db.Shippers.Where(x => x.Name == Name && x.ShipperId != Id).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.Shippers.Where(x => x.Name == Name).ToList();
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

        public ActionResult CheckDuplicateMail(string Email, string Mode, int Id)
        {
            try
            {
                if (Mode == "Edit")
                {
                    var result = db.Shippers.Where(x => x.Email == Email && x.ShipperId != Id).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.Shippers.Where(x => x.Email == Email).ToList();
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
            var DataSource = (from shipper in db.Shippers
                              join customer in db.Customers on shipper.CustomerId equals customer.CustomerID into cust
                              from c in cust.DefaultIfEmpty()
                              orderby shipper.CreatedDate descending
                              select new { ShipperId = shipper.ShipperId, Name = shipper.Name, Customer = c.CustomerName, ContactPerson = shipper.ContactPerson, Country = shipper.Country, State = shipper.State, City = shipper.City, Freeze = shipper.Freeze }
                                   ).ToList();
            GridProperties obj = ConvertGridObject(GridModel);
            exp.Export(obj, DataSource, "Shipper.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
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

        public ActionResult CheckEmployee(string Customer)
        {
            var result = db.Customers.Where(x => x.CustomerName == Customer).Count();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
