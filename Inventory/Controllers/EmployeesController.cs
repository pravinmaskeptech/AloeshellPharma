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
    public class EmployeesController : Controller
    {
        private InventoryModel db = new InventoryModel();

        // GET: Employees
        public ActionResult Index()
        {
            try
            {
                var EmployeesMaster = new List<Employee>(db.Employees);
                ViewBag.datasource = (from emp in EmployeesMaster
                                      orderby emp.EmployeeID descending
                                      select new { EmployeeID = emp.EmployeeID, EmployeeName = emp.EmployeeName, DOB = emp.DOB, City = emp.City, Pincode = emp.Pincode, Designation = emp.Designation, Mobile = emp.Mobile, Address = emp.Address, Gender = emp.Gender, BloodGroup = emp.BloodGroup, Phone = emp.Phone, Email = emp.Email, IsActive = emp.IsActive, PicturePath = emp.PicturePath }
                                    ).ToList();
            }
            catch (Exception Ex)
            {
                ViewBag.datasource = null;
                var msg1 = Ex.Message.ToString();
                TempData["Msg"] = msg1;
            }
            return View();
        }

        // GET: Employees/Create
        public ActionResult Create()
        {
            try
            {
                SelectListItem Apositive = new SelectListItem() { Value = "A+", Text = "A+" };
                SelectListItem Anigetive = new SelectListItem() { Value = "A-", Text = "A-" };
                SelectListItem Bpositive = new SelectListItem() { Value = "B+", Text = "B+" };
                SelectListItem Bnigetive = new SelectListItem() { Value = "B-", Text = "B-" };
                SelectListItem Opositive = new SelectListItem() { Value = "O+", Text = "O+" };
                SelectListItem Onigetive = new SelectListItem() { Value = "O-", Text = "O-" };
                SelectListItem ABpositive = new SelectListItem() { Value = "AB+", Text = "AB+" };
                SelectListItem ABnigetive = new SelectListItem() { Value = "AB-", Text = "AB-" };
                List<SelectListItem> BloodGroups = new List<SelectListItem>();
                BloodGroups.Add(Apositive); BloodGroups.Add(Anigetive); BloodGroups.Add(Bpositive); BloodGroups.Add(Bnigetive); BloodGroups.Add(Opositive); BloodGroups.Add(Onigetive); BloodGroups.Add(ABpositive); BloodGroups.Add(ABnigetive);
                ViewBag.BloodGroupDataSource = BloodGroups;

                SelectListItem Male = new SelectListItem() { Value = "Male", Text = "Male" };
                SelectListItem Female = new SelectListItem() { Value = "Female", Text = "Female" };
                List<SelectListItem> Gender = new List<SelectListItem>();
                Gender.Add(Male);
                Gender.Add(Female);
                ViewBag.GenderDataSource = Gender;
                return View();
            }
            catch (Exception Ex)
            {

                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return RedirectToAction("Index");
            }
        }

        // POST: Employees/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Employee employee, HttpPostedFileBase UploadFilePath, string PreviousFile, string BirthDate)
        {
            //if (UploadFilePath != null)
            //{
            //    var fileName = Path.GetFileName(UploadFilePath.FileName);
            //    string path = string.Format("~/Photo/EmployeePic/{0}", fileName);
            //    employee.PicturePath = fileName;
            //    if (System.IO.File.Exists(Server.MapPath(path)))
            //        System.IO.File.Delete(Server.MapPath(path));
            //    UploadFilePath.SaveAs(Server.MapPath(path));
            //}
            //SelectListItem Apositive = new SelectListItem() { Value = "A+", Text = "A+" };
            //SelectListItem Anigetive = new SelectListItem() { Value = "A-", Text = "A-" };
            //SelectListItem Bpositive = new SelectListItem() { Value = "B+", Text = "B+" };
            //SelectListItem Bnigetive = new SelectListItem() { Value = "B-", Text = "B-" };
            //SelectListItem Opositive = new SelectListItem() { Value = "O+", Text = "O+" };
            //SelectListItem Onigetive = new SelectListItem() { Value = "O-", Text = "O-" };
            //SelectListItem ABpositive = new SelectListItem() { Value = "AB+", Text = "AB+" };
            //SelectListItem ABnigetive = new SelectListItem() { Value = "AB-", Text = "AB-" };
            //List<SelectListItem> BloodGroups = new List<SelectListItem>();
            //BloodGroups.Add(Apositive); BloodGroups.Add(Anigetive); BloodGroups.Add(Bpositive); BloodGroups.Add(Bnigetive); BloodGroups.Add(Opositive); BloodGroups.Add(Onigetive); BloodGroups.Add(ABpositive); BloodGroups.Add(ABnigetive);

            //SelectListItem Male = new SelectListItem() { Value = "Male", Text = "Male" };
            //SelectListItem Female = new SelectListItem() { Value = "Female", Text = "Female" };
            //List<SelectListItem> Gender = new List<SelectListItem>();
            //Gender.Add(Male);
            //Gender.Add(Female);

            try
            {
                string[] FromDate = BirthDate.ToString().Split('/');
                employee.DOB = new DateTime(Convert.ToInt32(FromDate[2]), Convert.ToInt32(FromDate[1]), Convert.ToInt32(FromDate[0]));
                var count = db.Employees.Count();
                count = count + 1;
                string Code = count.ToString();
                Code = Code.PadLeft(4, '0');
                employee.EmployeeID = "R" + Code;
                employee.IsActive = true;
                employee.CreatedBy = "";
                employee.CreatedDate = DateTime.Now;
                db.Employees.Add(employee);
                db.SaveChanges();
                TempData["Temp"] = "Employee Save Successfully";
                return RedirectToAction("Index");


            }
            catch (Exception Ex)
            {

                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return View(employee);
            }
        }

        // GET: Employees/Edit/5
        public ActionResult Edit(string id)
        {
            try
            {
                List<Employee> objfiles = db.Employees.Where(x => x.EmployeeID == id).ToList();
                ViewData["FilePath"] = objfiles;
                SelectListItem Apositive = new SelectListItem() { Value = "A+", Text = "A+" };
                SelectListItem Anigetive = new SelectListItem() { Value = "A-", Text = "A-" };
                SelectListItem Bpositive = new SelectListItem() { Value = "B+", Text = "B+" };
                SelectListItem Bnigetive = new SelectListItem() { Value = "B-", Text = "B-" };
                SelectListItem Opositive = new SelectListItem() { Value = "O+", Text = "O+" };
                SelectListItem Onigetive = new SelectListItem() { Value = "O-", Text = "O-" };
                SelectListItem ABpositive = new SelectListItem() { Value = "AB+", Text = "AB+" };
                SelectListItem ABnigetive = new SelectListItem() { Value = "AB-", Text = "AB-" };
                List<SelectListItem> BloodGroups = new List<SelectListItem>();
                BloodGroups.Add(Apositive); BloodGroups.Add(Anigetive); BloodGroups.Add(Bpositive); BloodGroups.Add(Bnigetive); BloodGroups.Add(Opositive); BloodGroups.Add(Onigetive); BloodGroups.Add(ABpositive); BloodGroups.Add(ABnigetive);

                SelectListItem Male = new SelectListItem() { Value = "Male", Text = "Male" };
                SelectListItem Female = new SelectListItem() { Value = "Female", Text = "Female" };
                List<SelectListItem> Gender = new List<SelectListItem>();
                Gender.Add(Male);
                Gender.Add(Female);
                ViewBag.BloodGroupDataSource = BloodGroups;
                ViewBag.GenderDataSource = Gender;
                Employee employee = db.Employees.Find(id);
                return View(employee);
            }
            catch (Exception Ex)
            {
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return RedirectToAction("Index");
            }
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Employee employee, HttpPostedFileBase UploadFilePath, string PreviousFile, string BirthDate)
        {
            SelectListItem Apositive = new SelectListItem() { Value = "A+", Text = "A+" }; SelectListItem Anigetive = new SelectListItem() { Value = "A-", Text = "A-" }; SelectListItem Bpositive = new SelectListItem() { Value = "B+", Text = "B+" }; SelectListItem Bnigetive = new SelectListItem() { Value = "B-", Text = "B-" }; SelectListItem Opositive = new SelectListItem() { Value = "O+", Text = "O+" }; SelectListItem Onigetive = new SelectListItem() { Value = "O-", Text = "O-" }; SelectListItem ABpositive = new SelectListItem() { Value = "AB+", Text = "AB+" }; SelectListItem ABnigetive = new SelectListItem() { Value = "AB-", Text = "AB-" }; List<SelectListItem> BloodGroups = new List<SelectListItem>(); BloodGroups.Add(Apositive); BloodGroups.Add(Anigetive); BloodGroups.Add(Bpositive); BloodGroups.Add(Bnigetive); BloodGroups.Add(Opositive); BloodGroups.Add(Onigetive); BloodGroups.Add(ABpositive); BloodGroups.Add(ABnigetive);
            SelectListItem Male = new SelectListItem() { Value = "Male", Text = "Male" }; SelectListItem Female = new SelectListItem() { Value = "Female", Text = "Female" }; List<SelectListItem> Gender = new List<SelectListItem>(); Gender.Add(Male); Gender.Add(Female);
            try
            {
                
                    if (UploadFilePath != null && UploadFilePath.ContentLength > 0)
                    {
                        var fileName = UploadFilePath.FileName;
                        string path = Server.MapPath("~/Photo/EmployeePic/");
                        employee.PicturePath = fileName;
                        UploadFilePath.SaveAs(path + Path.GetFileName(UploadFilePath.FileName));
                    }
                    else
                    {
                        employee.PicturePath = PreviousFile;
                    }
                    string[] FromDate = BirthDate.ToString().Split('/');
                    employee.DOB = new DateTime(Convert.ToInt32(FromDate[2]), Convert.ToInt32(FromDate[1]), Convert.ToInt32(FromDate[0]));
                    employee.UpdatedBy = User.Identity.Name;
                    employee.UpdateDate = DateTime.Today;
                    db.Entry(employee).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Temp"] = "Employee Update Successfully";
                    return RedirectToAction("Index");
               
            }
            catch (Exception Ex)
            {
                ViewBag.BloodGroupDataSource = BloodGroups;
                ViewBag.GenderDataSource = Gender;
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return View(employee);
            }
        }
        public ActionResult DownloadFiles(string name)
        {
            try
            {
                string Filpath = Server.MapPath("~/Photo/EmployeePic/");
                Filpath = Filpath + name;
                System.IO.FileInfo file = new System.IO.FileInfo(Filpath);
                if (file.Exists)
                {
                    HttpContext.Response.ContentType = "APPLICATION/OCTET-STREAM";
                    String Header = "Attachment; Filename=" + name;
                    HttpContext.Response.AppendHeader("Content-Disposition", Header);
                    System.IO.FileInfo Dfile = new System.IO.FileInfo(HttpContext.Server.MapPath("~/Photo/EmployeePic/" + name + ""));
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

                    var result = db.Employees.Where(f => f.EmployeeName == Name && f.EmployeeID != Code).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.Employees.Where(f => f.EmployeeName == Name).ToList();
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
        #region -Check Duplicate Email-
        public ActionResult CheckDuplicateEmail(string Email, string Mode, string Code)
        {
            try
            {
                if (Mode == "Edit")
                {

                    var result = db.Employees.Where(f => f.Email == Email && f.EmployeeID != Code).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.Employees.Where(f => f.Email == Email).ToList();
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
        #region -get Gender And BloodGroup-
        public JsonResult getGenderAndBloodGroup(string Empid)
        {
            try
            {
                var result = db.Employees.Where(a => a.EmployeeID.Equals(Empid)).FirstOrDefault();
                return Json(result, JsonRequestBehavior.AllowGet);
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
        public JsonResult getAddress(string Id)
        {
            try
            {
                var result = db.Employees.Where(x => x.EmployeeID.Equals(Id)).FirstOrDefault();
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
        #region -Export Excel-
        [ValidateInput(false)]
        public void ExportToExcel(string GridModel)
        {
            ExcelExport exp = new ExcelExport();
            var EmployeesMaster = new List<Employee>(db.Employees);
            var DataSource = (from emp in EmployeesMaster
                              orderby emp.EmployeeID descending
                              select new { EmployeeID = emp.EmployeeID, EmployeeName = emp.EmployeeName, DOB = emp.DOB, City = emp.City, Pincode = emp.Pincode, Designation = emp.Designation, Mobile = emp.Mobile, Address = emp.Address, Gender = emp.Gender, BloodGroup = emp.BloodGroup, Phone = emp.Phone, Email = emp.Email, IsActive = emp.IsActive, PicturePath = emp.PicturePath }
                                ).ToList();

            GridProperties obj = ConvertGridObject(GridModel);
            exp.Export(obj, DataSource, "Employees.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
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
