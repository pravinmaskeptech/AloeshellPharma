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
    public class CompanyDetailsController : Controller
    {
        private InventoryModel db = new InventoryModel();

        // GET: CompanyDetails
        public ActionResult Index()
        {
            try
            {
                var CompanyMaster = new List<CompanyDetail>(db.CompanyDetails);
                ViewBag.datasource = (from comp in CompanyMaster
                                      orderby comp.CompanyID descending
                                      select new { CompanyID = comp.CompanyID, CompanyName = comp.CompanyName, City = comp.City, Pincode = comp.Pincode, Address = comp.Address, Phone = comp.Phone, Email = comp.Email, IsActive = comp.IsActive, LogoPath = comp.LogoPath, PanNo = comp.PanNo, TanNo = comp.TanNo }
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

        // GET: CompanyDetails/Create
        public ActionResult Create()
        {
            try
            {
                SelectListItem F1 = new SelectListItem() { Value = "January to December", Text = "January to December" };
                SelectListItem F2 = new SelectListItem() { Value = "April to March", Text = "April to March" };
                List<SelectListItem> Fiscal = new List<SelectListItem>();
                Fiscal.Add(F1);
                Fiscal.Add(F2);
                ViewBag.FinancialDataSource = Fiscal;
                return View();
            }

            catch (Exception Ex)
            {
               
                 var  msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return RedirectToAction("Index");
            }
        }

        // POST: CompanyDetails/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CompanyDetail companyDetail, HttpPostedFileBase UploadFilePath)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (UploadFilePath != null)
                {
                    var fileName = Path.GetFileName(UploadFilePath.FileName);
                    string path = string.Format("~/Photo/CompanyPic/{0}", fileName);
                    companyDetail.LogoPath = fileName;
                    if (System.IO.File.Exists(Server.MapPath(path)))
                        System.IO.File.Delete(Server.MapPath(path));
                    UploadFilePath.SaveAs(Server.MapPath(path));
                }                
                            companyDetail.IsActive = true;
                            companyDetail.CreatedBy = User.Identity.Name;
                            companyDetail.CreatedDate = DateTime.Today;

                            db.CompanyDetails.Add(companyDetail);
                            db.SaveChanges();
                            TempData["Temp"] = "Company Details Save Successfully";
                            return RedirectToAction("Index");
                        
                }
                SelectListItem F1 = new SelectListItem() { Value = "January to December", Text = "January to December" };
                SelectListItem F2 = new SelectListItem() { Value = "April to March", Text = "April to March" };
                List<SelectListItem> Fiscal = new List<SelectListItem>();
                Fiscal.Add(F1);
                Fiscal.Add(F2);
                ViewBag.FinancialDataSource = Fiscal;
                TempData["Msg"] = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                ViewBag.CompanyDatasource = db.CompanyDetails.Where(a => a.IsActive == true).ToList();
                return View(companyDetail);
            }
            catch (Exception Ex)
            {
                SelectListItem F1 = new SelectListItem() { Value = "January to December", Text = "January to December" };
                SelectListItem F2 = new SelectListItem() { Value = "April to March", Text = "April to March" };
                List<SelectListItem> Fiscal = new List<SelectListItem>();
                Fiscal.Add(F1);
                Fiscal.Add(F2);
                ViewBag.FinancialDataSource = Fiscal;
            
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return View();
            }
        }

        // GET: CompanyDetails/Edit/5
        public ActionResult Edit(int? id)
        {
            try
            {
                List<CompanyDetail> objfiles = db.CompanyDetails.Where(x => x.CompanyID == id).ToList();
                ViewData["FilePath"] = objfiles;
                SelectListItem F1 = new SelectListItem() { Value = "January to December", Text = "January to December" };
                SelectListItem F2 = new SelectListItem() { Value = "April to March", Text = "April to March" };
                List<SelectListItem> Fiscal = new List<SelectListItem>();
                Fiscal.Add(F1);
                Fiscal.Add(F2);
                ViewBag.FinancialDataSource = Fiscal;
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                CompanyDetail companyDetail = db.CompanyDetails.Find(id);
                if (companyDetail == null)
                {
                    return HttpNotFound();
                }
                return View(companyDetail);
            }
            catch (Exception Ex)
            {
               
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return RedirectToAction("Index");
            }
        }

        // POST: CompanyDetails/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CompanyDetail companyDetail, HttpPostedFileBase UploadFilePath, string PreviousFile)
        {
            try
            {
                if(ModelState.IsValid)
                {                
                        if (UploadFilePath != null && UploadFilePath.ContentLength > 0)
                        {
                            var fileName = UploadFilePath.FileName;
                            string path = Server.MapPath("~/Photo/CompanyPic/");
                            companyDetail.LogoPath = fileName;
                            UploadFilePath.SaveAs(path + Path.GetFileName(UploadFilePath.FileName));
                        }
                        else
                        {
                            companyDetail.LogoPath = PreviousFile;
                        }
                        companyDetail.UpdatedBy = User.Identity.Name;
                        companyDetail.UpdateDate = DateTime.Today;
                        db.Entry(companyDetail).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["Temp"] = "Company Details Update Successfully";
                        return RedirectToAction("Index");                   
            }
                SelectListItem F1 = new SelectListItem() { Value = "January to December", Text = "January to December" };
                SelectListItem F2 = new SelectListItem() { Value = "April to March", Text = "April to March" };
                List<SelectListItem> Fiscal = new List<SelectListItem>();
                Fiscal.Add(F1);
                Fiscal.Add(F2);
                ViewBag.FinancialDataSource = Fiscal;
                TempData["Msg"] = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                ViewBag.CompanyDatasource = db.CompanyDetails.Where(a => a.IsActive == true).ToList();
                return View(companyDetail);
            }
            catch (Exception Ex)
            {
                SelectListItem F1 = new SelectListItem() { Value = "January to December", Text = "January to December" };
                SelectListItem F2 = new SelectListItem() { Value = "April to March", Text = "April to March" };
                List<SelectListItem> Fiscal = new List<SelectListItem>();
                Fiscal.Add(F1);
                Fiscal.Add(F2);
                ViewBag.FinancialDataSource = Fiscal;
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return View(companyDetail);
            }
        }
        #region -Get Fiscal Year-
        public JsonResult getFiscalYear(int Id)
        {
            try
            {
                var result = db.CompanyDetails.Where(a => a.CompanyID.Equals(Id)).FirstOrDefault();
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
        #region -Check Duplicate Email-
        public ActionResult CheckDuplicateEmail(string Email, string Mode, int Code)
        {
            try
            {
                if (Mode == "Edit")
                {

                    var result = db.CompanyDetails.Where(f => f.Email == Email && f.CompanyID != Code).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.CompanyDetails.Where(f => f.Email == Email).ToList();
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
        public ActionResult CheckDuplicateName(string Name, string Mode, int Code)
        {
            try
            {
                if (Mode == "Edit")
                {

                    var result = db.CompanyDetails.Where(f => f.CompanyName == Name && f.CompanyID != Code).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.CompanyDetails.Where(f => f.CompanyName == Name).ToList();
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

        #region -Export Excel-
        [ValidateInput(false)]
        public void ExportToExcel(string GridModel)
        {
            ExcelExport exp = new ExcelExport();
            var EmployeesMaster = new List<Employee>(db.Employees);
            var CompanyMaster = new List<CompanyDetail>(db.CompanyDetails);
            var DataSource = (from comp in CompanyMaster
                                  orderby comp.CompanyID descending
                                  select new { CompanyID = comp.CompanyID, CompanyName = comp.CompanyName, City = comp.City, Pincode = comp.Pincode, Address = comp.Address, Phone = comp.Phone, Email = comp.Email, IsActive = comp.IsActive, LogoPath = comp.LogoPath, PanNo = comp.PanNo, TanNo = comp.TanNo }
                                ).ToList();

            GridProperties obj = ConvertGridObject(GridModel);
            exp.Export(obj, DataSource, "CompanyDetails.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
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
        public ActionResult DownloadFiles(string name)
        {
            try
            {
                string Filpath = Server.MapPath("~/Photo/CompanyPic/");
                Filpath = Filpath + name;
                System.IO.FileInfo file = new System.IO.FileInfo(Filpath);
                if (file.Exists)
                {
                    HttpContext.Response.ContentType = "APPLICATION/OCTET-STREAM";
                    String Header = "Attachment; Filename=" + name;
                    HttpContext.Response.AppendHeader("Content-Disposition", Header);
                    System.IO.FileInfo Dfile = new System.IO.FileInfo(HttpContext.Server.MapPath("~/Photo/CompanyPic/" + name + ""));
                    HttpContext.Response.WriteFile(Dfile.FullName);
                    HttpContext.Response.End();
                }
            }
            catch (Exception Ex)
            {
                SelectListItem Apositive = new SelectListItem() { Value = "A+", Text = "A+" }; SelectListItem Anigetive = new SelectListItem() { Value = "A-", Text = "A-" }; SelectListItem Bpositive = new SelectListItem() { Value = "B+", Text = "B+" }; SelectListItem Bnigetive = new SelectListItem() { Value = "B-", Text = "B-" }; SelectListItem Opositive = new SelectListItem() { Value = "O+", Text = "O+" }; SelectListItem Onigetive = new SelectListItem() { Value = "O-", Text = "O-" }; SelectListItem ABpositive = new SelectListItem() { Value = "AB+", Text = "AB+" }; SelectListItem ABnigetive = new SelectListItem() { Value = "AB-", Text = "AB-" }; List<SelectListItem> BloodGroups = new List<SelectListItem>(); BloodGroups.Add(Apositive); BloodGroups.Add(Anigetive); BloodGroups.Add(Bpositive); BloodGroups.Add(Bnigetive); BloodGroups.Add(Opositive); BloodGroups.Add(Onigetive); BloodGroups.Add(ABpositive); BloodGroups.Add(ABnigetive);
                SelectListItem Male = new SelectListItem() { Value = "Male", Text = "Male" }; SelectListItem Female = new SelectListItem() { Value = "Female", Text = "Female" }; List<SelectListItem> Gender = new List<SelectListItem>(); Gender.Add(Male); Gender.Add(Female);
                ViewBag.BloodGroupDataSource = BloodGroups;
                ViewBag.GenderDataSource = Gender;
                TempData["Msg"] = "Image not found";
                return View();
            }
            return View();
        }

    }
}
  
