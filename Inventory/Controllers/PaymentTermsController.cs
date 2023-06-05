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
    public class PaymentTermsController : Controller
    {
        private InventoryModel db = new InventoryModel();
        public ActionResult Index()
        {
            try
            {
                ViewBag.datasource = db.Paymentterms.ToList();
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
        public ActionResult Create(Paymentterms paymentterms)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    paymentterms.IsActive = true;
                    paymentterms.CreatedBy = "Test User";
                    paymentterms.CreatedDate = DateTime.Now;
                    db.Paymentterms.Add(paymentterms);
                    db.SaveChanges();
                    TempData["Temp"] = "payment terms Created Successfully";
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
                return View(paymentterms);
            }
        }
     
        public ActionResult Edit(int id)
        {
            ViewBag.Freeze = new List<SelectListItem>
                                                            {
                                                                new SelectListItem{ Text="False", Value = "False" },
                                                                new SelectListItem{ Text="True", Value = "True" }
                                                            };
            Paymentterms paymentterms = db.Paymentterms.Find(id);
            return View(paymentterms);
        }
     
        [HttpPost]
        public ActionResult Edit(Paymentterms paymentterms)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    paymentterms.UpdatedBy = "test user";
                    paymentterms.UpdatedDate = DateTime.Now;
                    db.Entry(paymentterms).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Temp"] = "Payment terms Updated Successfully";
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

                    return View(paymentterms);
                }
            }
            else
            {
                ViewBag.Freeze = new List<SelectListItem>
                                                            {
                                                                new SelectListItem{ Text="False", Value = "False" },
                                                                new SelectListItem{ Text="True", Value = "True" }
                                                            };
                return View(paymentterms);
            }
        }
        public ActionResult CheckDuplicateName(string Name, string Mode, int Id)
        {
            try
            {
                if (Mode == "Edit")
                {
                    var result = db.Paymentterms.Where(x => x.PaymentTerm == Name && x.ID != Id).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.Paymentterms.Where(x => x.PaymentTerm == Name).ToList();
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
            var DataSource = db.Paymentterms.ToList();
            GridProperties obj = ConvertGridObject(GridModel);
            exp.Export(obj, DataSource, "PaymentTerms.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
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
