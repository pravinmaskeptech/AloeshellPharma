using Inventory.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Syncfusion.EJ.Export;
using Syncfusion.JavaScript.Models;
using Syncfusion.UI.Xaml.Maps;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Inventory.Controllers
{
    public class DoctorMasterController : Controller
    {
        InventoryModel db = new InventoryModel();
        public ActionResult Index()
        {

            try
            {
                ViewBag.datasource = (from Cust in db.DoctorMasterModels.Where(a => a.IsActive == true)
                                      orderby Cust.CreatedDate descending
                                      select new { SalesPersonName = Cust.SalesPersonName, DoctorDropdownRegister = Cust.DoctorDropdownRegister,Type = Cust.Type, DoctorID = Cust.DoctorID, FirmName = Cust.FirmName, DoctorName = Cust.DoctorName, DoctorCode = Cust.DoctorCode, Password = Cust.Password, Address = Cust.Address, City = Cust.City, Pincode = Cust.Pincode, ContactNo = Cust.ContactNo, Email = Cust.Email, IsActive = Cust.IsActive,RegisterUnder = Cust.RegisterUnder }).ToList();
            }
            catch (Exception Ex)
            {
                ViewBag.datasource = null;
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
            }
            return View();
        }

        public ActionResult Details(int id)
        {
            return View();
        }
        [HttpGet]
        public ActionResult Create()
        {
            try
            {
                ViewBag.Doctor = db.DoctorMasterModels.Where(a=>a.IsActive == true).ToList();
                ViewBag.Sales = db.SalesPersonMasters.Where(a => a.IsActive == true).ToList();
                return View();
            }
            catch (Exception Ex)
            {
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<ActionResult> Create(DoctorMasterModel salesPerson)
        {

            try
            {
                ViewBag.Doctor = db.DoctorMasterModels.Where(a => a.IsActive == true).ToList();
                ViewBag.Sales = db.SalesPersonMasters.Where(a => a.IsActive == true).ToList();

                ApplicationDbContext context = new ApplicationDbContext();
                var username = await (from u in context.Users
                                      where u.UserName == salesPerson.ContactNo
                                      select new
                                      {
                                          Username = u.UserName,
                                          Email = u.Email
                                      }).FirstOrDefaultAsync();


                if (username != null)
                {
                    TempData["error"] = username.Username + " already taken please enter different username!";
                    return View();
                }
                else
                {
                    DoctorMasterModel doctor = new DoctorMasterModel();

                    doctor.DoctorName = salesPerson.DoctorName;
                    doctor.FirmName = salesPerson.FirmName;
                    doctor.Address = salesPerson.Address;
                    doctor.Email = salesPerson.Email;
                    doctor.City = salesPerson.City;
                    doctor.ContactNo = salesPerson.ContactNo;
                    doctor.Password = salesPerson.Password;
                    doctor.Pincode = salesPerson.Pincode;
                    doctor.Type = salesPerson.Type;
                    doctor.RegisterUnder = salesPerson.RegisterUnder;
                    doctor.UpdatedBy = salesPerson.UpdatedBy;
                    doctor.UpdatedDate = salesPerson.UpdatedDate;

                    if (salesPerson.RegisterUnder == "SalesPerson")
                    {

                        var PersonID = Convert.ToInt32(salesPerson.SalesPersonName);
                        if(PersonID == null || PersonID == 0) {
                            TempData["error"] =  " Select " + salesPerson.RegisterUnder + "Reference";
                            return View();
                        }
                        SalesPersonMaster name = db.SalesPersonMasters.Where(a => a.SalesPersonID == PersonID).FirstOrDefault();
                        doctor.SalesPersonName = name.SalesPersonName;
                        doctor.DoctorDropdownRegister = "";
                    }
                    else
                    {

                        var PersonID1 = Convert.ToInt32(salesPerson.DoctorDropdownRegister);
                        if (PersonID1 == null || PersonID1 == 0)
                        {
                            TempData["error"] = " Select " + salesPerson.RegisterUnder + "Reference";
                            return View();
                        }
                        DoctorMasterModel name1 = db.DoctorMasterModels.Where(a => a.DoctorID == PersonID1).FirstOrDefault();
                        doctor.DoctorDropdownRegister = name1.DoctorName;
                        doctor.SalesPersonName = "";
                    }

                    var doctorcode = Convert.ToInt16(salesPerson.DoctorCode);
                    if (salesPerson.Type == "NutraAgent")
                    {
                        doctor.DoctorCode = string.Format("PNA{0:D3}", doctorcode);
                    }
                    if (salesPerson.Type == "Doctor")
                    {
                        doctor.DoctorCode = string.Format("PND{0:D3}", doctorcode);
                    }

                    doctor.IsActive = true;
                    doctor.CreatedBy = User.Identity.Name;
                    doctor.CreatedDate = DateTime.Now;
                    //doctor.DoctorDropdownRegister = doctor.DoctorDropdownRegister;
                    db.DoctorMasterModels.Add(doctor);

                    //var store = new UserStore<ApplicationUser>(context);
                    //var manager = new ApplicationUserManager(store);
                    //var user = new ApplicationUser { UserName = salesPerson.ContactNo, Email = salesPerson.Email, PhoneNumber = salesPerson.ContactNo };
                    //var adminresult = await manager.CreateAsync(user, salesPerson.Password);

                    //if (adminresult.Succeeded == false)
                    //{
                    //    TempData["error"] = $"{salesPerson.DoctorCode} add failed";
                    //    return RedirectToAction("Index");
                    //}

                    //var asprole = new AspNetUserRoles();
                    //asprole.UserId = user.Id;
                    //if (salesPerson.Type == "NutraAgent")
                    //{
                    //    asprole.RoleId = "NutraAgent";
                    //}
                    //if (salesPerson.Type == "Doctor")
                    //{
                    //    asprole.RoleId = "Doctor";
                    //}
                    //db.AspNetUserRoles.Add(asprole);
                  

                    db.SaveChanges();

                    TempData["Temp"] = $"{doctor.DoctorCode} Registered Successfully";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception Ex)
            {
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return View();
            }

        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            try
            {
                if (id == null || id == 0)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                ViewBag.Doctor = db.DoctorMasterModels.Where(a => a.IsActive == true).ToList();
                ViewBag.Sales = db.SalesPersonMasters.Where(a => a.IsActive == true).ToList();
                DoctorMasterModel supplier = db.DoctorMasterModels.Find(id);

                return View(supplier);
            }
            catch (Exception Ex)
            {
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult Edit(DoctorMasterModel suppliers)
        {

            try
            {
                suppliers.UpdatedDate = DateTime.Now;
                suppliers.UpdatedBy = "Admin";
                db.Entry(suppliers).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Temp"] = $"{suppliers.DoctorCode} Updated Successfully"; 
                return RedirectToAction("Index");
            }
            catch (Exception Ex)
            {
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return View(suppliers);
            }

        }
        public ActionResult CheckDuplicateName(string Name, string Mode, int Id)
        {
            try
            {
                if (Mode == "Edit")
                {
                    var result = db.DoctorMasterModels.Where(x => x.DoctorName == Name && x.DoctorID != Id).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.DoctorMasterModels.Where(x => x.DoctorName == Name).ToList();
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

        public ActionResult CheckDuplicateCode(string DoctorCode, string Mode, string Type)
        {
            try
            {
                var doctorcode = Convert.ToInt16(DoctorCode);
                if (Type == "NutraAgent")
                {
                    DoctorCode = string.Format("PNA{0:D3}", doctorcode);
                }
                if (Type == "Doctor")
                {
                    DoctorCode = string.Format("PND{0:D3}", doctorcode);
                }

                var result = db.DoctorMasterModels.Where(x => x.DoctorCode == DoctorCode).ToList();
                if (result.Count == 0)
                    return Json("0", JsonRequestBehavior.AllowGet);
                else
                    return Json("1", JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                var str = Ex.Message.ToString();
                return Json(str, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult CheckDuplicateFirmName(string FirmName, string Mode, int Id)
        {
            try
            {
                if (Mode == "Edit")
                {
                    var result = db.DoctorMasterModels.Where(x => x.FirmName == FirmName && x.DoctorID != Id).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.DoctorMasterModels.Where(x => x.FirmName == FirmName).ToList();
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


        public ActionResult GetSalesPerson(int SalesPersonID)
        {
            try
            {
                var result = db.DoctorMasterModels.Where(x => x.DoctorID == SalesPersonID).FirstOrDefault();
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


        public JsonResult DeleteRecord(int ID)
        {
            var x = db.DoctorMasterModels.Where(a => a.DoctorID == ID).FirstOrDefault();
            x.IsActive = false;
            db.SaveChanges();
            var result = new { Message = "success" };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SetDoctorId(string DoctorID)
        {
            try
            {
                if (DoctorID == "")
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.DoctorMasterModels.Where(a => a.DoctorID == Convert.ToInt32(DoctorID)).FirstOrDefault();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception Ex)
            {
                TempData["Msg"] = Ex.Message.ToString();
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult SetSalesPersonId(string SalesPersonID)
        {
            try
            {
                if (SalesPersonID == "")
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    int salesID = Convert.ToInt16(SalesPersonID);
                    var result = db.SalesPersonMasters.Where(a => a.SalesPersonID == salesID).FirstOrDefault();
                    return Json(result.SalesPersonID, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception Ex)
            {
                TempData["Msg"] = Ex.Message.ToString();
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        [ValidateInput(false)]
        public void ExportToExcel(string GridModel)
        {
            ExcelExport exp = new ExcelExport();
            var EmployeesMaster = new List<Employee>(db.Employees);
            var DataSource = (from Cust in db.DoctorMasterModels.Where(a => a.IsActive == true)
                              orderby Cust.CreatedDate descending
                              select new { SalesPersonName = Cust.SalesPersonName, DoctorDropdownRegister = Cust.DoctorDropdownRegister, Type = Cust.Type, DoctorID = Cust.DoctorID, FirmName = Cust.FirmName, DoctorName = Cust.DoctorName, DoctorCode = Cust.DoctorCode, Password = Cust.Password, Address = Cust.Address, City = Cust.City, Pincode = Cust.Pincode, ContactNo = Cust.ContactNo, Email = Cust.Email, IsActive = Cust.IsActive, RegisterUnder = Cust.RegisterUnder }).ToList();

            GridProperties obj = ConvertGridObject(GridModel);
            exp.Export(obj, DataSource, "Doctor.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
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
