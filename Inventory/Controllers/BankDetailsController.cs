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
    public class BankDetailsController : Controller
    {
        InventoryModel db = new InventoryModel();

        // GET: BankDetails
        public ActionResult Index()
        {
            try
            {
                var CompanyDetails = new List<CompanyDetail>(db.CompanyDetails);
                var BankDetails = new List<BankDetail>(db.BankDetails);
                ViewBag.datasource = (from bank in BankDetails

                                      join Company in CompanyDetails on bank.CompanyID equals Company.CompanyID into CompanyNm
                                      from cmp in CompanyNm.DefaultIfEmpty()
                                      orderby bank.BankID descending
                                      select new { BankID = bank.BankID, BankName = bank.BankName, Branch = bank.Branch, BankAddress = bank.BankAddress, City = bank.City, Pincode = bank.Pincode, AccountHolderName = bank.AccountHolderName, AccountNo = bank.AccountNo, IFSCCode = bank.IFSCCode, CompanyID = cmp == null ? string.Empty : cmp.CompanyName, IsActive=cmp.IsActive }
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
        
        public ActionResult Create()
        {
            try
            {
                ViewBag.CompanyDatasource = db.CompanyDetails.Where(a => a.IsActive == true).ToList();
                return View();
            }
            catch (Exception Ex)
            {
                
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return RedirectToAction("Index");
            }
        }

        // POST: BankDetails/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( BankDetail bankDetail)
        {
            try
            {
                //if (ModelState.IsValid)
                //{
                //    bankDetail.IsActive = true;
                //    bankDetail.CreatedBy = User.Identity.Name;
                //    bankDetail.CreatedDate = DateTime.Today;
                //    db.BankDetails.Add(bankDetail);
                //            db.SaveChanges();
                //            TempData["Temp"] = "Bank Details Save Successfully";
                //            return RedirectToAction("Index");                        
                //}
                //TempData["Msg"]  = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors) .Select(e => e.ErrorMessage));                
                //ViewBag.CompanyDatasource = db.CompanyDetails.Where(a => a.IsActive == true).ToList();
                //return View(bankDetail);
                return View(db.BankDetails.ToList());
            }
            catch (Exception Ex)
            {
                ViewBag.CompanyDatasource = db.CompanyDetails.ToList().Where(a=>a.IsActive==true);              
               var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return View();
            }
        }

        // GET: BankDetails/Edit/5
        public ActionResult Edit(int? id)
        {
            try
            {
                ViewBag.CompanyDatasource = db.CompanyDetails.Where(a => a.IsActive == true).ToList();
                BankDetail bankDetail = db.BankDetails.Find(id);                
                return View(bankDetail);
            }
            catch (Exception Ex)
            {
               
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return RedirectToAction("Index");
            }
        }

        // POST: BankDetails/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit( BankDetail bankDetail)
        {
           try
            {
                if (ModelState.IsValid)
                {
                    bankDetail.UpdateDate = DateTime.Today;
                    bankDetail.UpdatedBy = User.Identity.Name;
                        db.Entry(bankDetail).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["Temp"] = "Bank Details Update Successfully";
                        return RedirectToAction("Index");                   
                }
                TempData["Msg"] = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                ViewBag.CompanyDatasource = db.CompanyDetails.Where(a => a.IsActive == true).ToList();
                return View(bankDetail);

            }
            catch (Exception Ex)
            {
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;              
                ViewBag.CompanyDatasource = db.CompanyDetails.Where(a => a.IsActive == true).ToList();
                return View(bankDetail);
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
        #region -Check Duplicate Bank Name-
        public ActionResult CheckDuplicateName(string Name, string Mode, int Code, int compId)
        {
            try
            {
                if (Mode == "Edit")
                {
                    var result = db.BankDetails.Where(f => f.BankName == Name && f.BankID != Code && f.CompanyID==compId).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.BankDetails.Where(f => f.BankName == Name && f.CompanyID==compId).ToList();
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
        #region -get Company Details-
        public JsonResult getCompanyDetails(int Id)
        {
            try
            {
                var result = db.CompanyDetails.Where(x => x.CompanyID.Equals(Id)).ToList();          
            return new JsonResult { Data = result,  JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            catch (Exception Ex)
            {
                var msg = Ex.Message.ToString();
                var result = new { error = "error", msg = msg };
                TempData["ErrorMsg"] = "Error";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region -get Address-
        public JsonResult getAddress(int bankId)
        {
            try
            {
                var result = db.BankDetails.Where(x => x.BankID.Equals(bankId)).ToList();
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
        #endregion
        #region -Check Duplicate Company Name-
        public JsonResult CheckDuplicateCompanyName(string Name)
        {
            try
            {
                var result = db.CompanyDetails.Where(x => x.CompanyName.Equals(Name)).ToList();
                if (result.Count == 0)
                    return Json("0", JsonRequestBehavior.AllowGet);
                else
                    return Json("1", JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                var msg = Ex.Message.ToString();
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
            var CompanyDetails = new List<CompanyDetail>(db.CompanyDetails);
            var BankDetails = new List<BankDetail>(db.BankDetails);
            var DataSource = (from bank in BankDetails

                                  join Company in CompanyDetails on bank.CompanyID equals Company.CompanyID into CompanyNm
                                  from cmp in CompanyNm.DefaultIfEmpty()
                                  orderby bank.BankID descending
                                  select new { BankID = bank.BankID, BankName = bank.BankName, Branch = bank.Branch, BankAddress = bank.BankAddress, City = bank.City, Pincode = bank.Pincode, AccountHolderName = bank.AccountHolderName, AccountNo = bank.AccountNo, IFSCCode = bank.IFSCCode, CompanyID = cmp == null ? string.Empty : cmp.CompanyName , IsActive = cmp.IsActive }
                                ).ToList();

            GridProperties obj = ConvertGridObject(GridModel);
            exp.Export(obj, DataSource, "BankDetails.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
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
