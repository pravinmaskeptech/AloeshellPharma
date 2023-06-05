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
    public class ReturnReasonController : Controller
    {
        private InventoryModel db = new InventoryModel();
        //
        // GET: /ReturnReason/
        public ActionResult Index()
        {
            ViewBag.datasource = db.ReturnReason.ToList();
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(ReturnReason Returnreason)
        {
            try
            {
                var result = db.ReturnReason.Where(x => x.Reason == Returnreason.Reason).ToList();
                if (result.Count == 0)
                {
                    Returnreason.CreatedBy = User.Identity.Name;
                    Returnreason.CreatedDate = DateTime.Now;
                    Returnreason.IsActive = true;
                    db.ReturnReason.Add(Returnreason);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewData["Message"] = "Reason already exist";
                    return View(Returnreason);
                }
            }
            catch (Exception EX)
            {
                ViewData["Message"] = EX.Message;
                return View(Returnreason);
            }            
        }

        public ActionResult Edit(int Id)
        {
            ViewBag.IsActive = new List<SelectListItem>
                                                            {
                                                                new SelectListItem{ Text="False", Value = "False" },
                                                                new SelectListItem{ Text="True", Value = "True" }
                                                            };
            var returnreason = db.ReturnReason.Find(Id);
            return View(returnreason);
        }

        [HttpPost]
        public ActionResult Edit(ReturnReason Returnreason)
        {
            try
            {
                var result = db.ReturnReason.Where(x => x.Reason == Returnreason.Reason && x.ReturnId!=Returnreason.ReturnId).ToList();
                if (result.Count == 0)
                {
                    Returnreason.UpdatedBy = User.Identity.Name;
                    Returnreason.UpdatedDate = DateTime.Now;                  
                    db.Entry(Returnreason).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.IsActive = new List<SelectListItem>
                                                            {
                                                                new SelectListItem{ Text="False", Value = "False" },
                                                                new SelectListItem{ Text="True", Value = "True" }
                                                            };
                    ViewData["Message"] = "Reason already exist";
                    return View(Returnreason);
                }
            }
            catch (Exception EX)
            {
                ViewData["Message"] = EX.Message;
                return View(Returnreason);
            }             
        }

        public ActionResult CheckDuplicateName(string Reason, string Mode, int Id)
        {
            try
            {
                if (Mode == "Edit")
                {
                    var result = db.ReturnReason.Where(x=>x.Reason==Reason && x.ReturnId!=Id).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.ReturnReason.Where(x => x.Reason == Reason).ToList();
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
            var DataSource = db.ReturnReason.ToList();
            GridProperties obj = ConvertGridObject(GridModel);
            exp.Export(obj, DataSource, "ReturnReason.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
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

        public ActionResult GetData(int Id)
        {
            var result = db.ReturnReason.Where(x => x.ReturnId == Id).FirstOrDefault();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
	}
}
    
