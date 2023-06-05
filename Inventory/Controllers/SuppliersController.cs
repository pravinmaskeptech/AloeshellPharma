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
    public class SuppliersController : Controller
    {
        private InventoryModel db = new InventoryModel();
        public ActionResult Index()
        {
            ViewBag.VIGST = new List<SelectListItem>
                                                            {
                                                                new SelectListItem{ Text="No", Value = "False" },
                                                                new SelectListItem{ Text="Yes", Value = "True" }
                                                            };
            ViewBag.EmailRegax = "[a-z0-9._%+-]+@[a-z0-9.-]+\\.[a-z]{2,3}$";
            try
            {
                ViewBag.datasource = (from Cust in db.suppliers
                                      join Emp in db.Employees on Cust.EmployeeID equals Emp.EmployeeID into E
                                      from employee in E.DefaultIfEmpty()
                                      orderby Cust.CreatedDate descending
                                      select new { SupplierID = Cust.SupplierID, SupplierName = Cust.SupplierName, BillingCountry = Cust.BillingCountry, BillingState = Cust.BillingState, BillingCity = Cust.BillingCity, EmployeeID = employee.EmployeeName, IsActive = Cust.IsActive }).ToList();
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

        public ActionResult Create()
        {
            try
            {
                ViewBag.VIGST = new List<SelectListItem>
                                                            {
                                                                new SelectListItem{ Text="No", Value = "False" },
                                                                new SelectListItem{ Text="Yes", Value = "True" }
                                                            };
                ViewBag.Employee = db.Employees.Where(x => x.IsActive != false).ToList();
                ViewBag.Curr = db.Currencies.Where(x => x.IsActive != false).ToList();
                ViewBag.Pterms = db.Paymentterms.Where(x => x.IsActive != false).ToList();
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
        public ActionResult Create(Suppliers suppliers)
        {

            try
            {
                suppliers.IsActive = true;
                suppliers.CreatedBy = User.Identity.Name;
                suppliers.CreatedDate = DateTime.Now;
                suppliers.EmployeeID = User.Identity.Name;
                db.suppliers.Add(suppliers);
                db.SaveChanges();
                TempData["Temp"] = "Supplier Created Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception Ex)
            {
                ViewBag.VIGST = new List<SelectListItem>
                                                            {
                                                                new SelectListItem{ Text="No", Value = "False" },
                                                                new SelectListItem{ Text="Yes", Value = "True" }
                                                            };
                ViewBag.Employee = db.Employees.Where(x => x.IsActive != false).ToList();
                ViewBag.Curr = db.Currencies.Where(x => x.IsActive != false).ToList();
                ViewBag.Pterms = db.Paymentterms.Where(x => x.IsActive != false).ToList();
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return View();
            }

        }
        public ActionResult Edit(int id)
        {
            try
            {
                ViewBag.VIGST = new List<SelectListItem>
                                                            {
                                                                new SelectListItem{ Text="No", Value = "False" },
                                                                new SelectListItem{ Text="Yes", Value = "True" }
                                                            };
                ViewBag.Employee = db.Employees.Where(x => x.IsActive != false).ToList();
                ViewBag.Curr = db.Currencies.Where(x => x.IsActive != false).ToList();
                ViewBag.Pterms = db.Paymentterms.Where(x => x.IsActive != false).ToList();

                Suppliers supplier = db.suppliers.Find(id);

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
        public ActionResult Edit(Suppliers suppliers)
        {

            try
            {
                suppliers.EmployeeID = User.Identity.Name;
                suppliers.UpdateDate = DateTime.Now;
                suppliers.UpdatedBy = User.Identity.Name;
                db.Entry(suppliers).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Temp"] = "Supplier Updated Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception Ex)
            {
                ViewBag.VIGST = new List<SelectListItem>
                                                            {
                                                                new SelectListItem{ Text="No", Value = "False" },
                                                                new SelectListItem{ Text="Yes", Value = "True" }
                                                            };
                ViewBag.Employee = db.Employees.Where(x => x.IsActive != false).ToList();
                ViewBag.Curr = db.Currencies.Where(x => x.IsActive != false).ToList();
                ViewBag.Pterms = db.Paymentterms.Where(x => x.IsActive != false).ToList();
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
                    var result = db.suppliers.Where(x => x.SupplierName == Name && x.SupplierID != Id).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.suppliers.Where(x => x.SupplierName == Name).ToList();
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
        #region -Check Duplicate Billing Email-
        public ActionResult CheckDuplicateBillingMail(string Email, string Mode, int Id)
        {
            try
            {
                if (Mode == "Edit")
                {
                    var result = db.suppliers.Where(x => x.BillingEmail == Email && x.SupplierID != Id).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.suppliers.Where(x => x.BillingEmail == Email).ToList();
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
        #endregion

        #region -Check Duplicate Shipping Email-
        public ActionResult CheckDuplicateShippingMail(string Email, string Mode, int Id)
        {
            try
            {
                if (Mode == "Edit")
                {
                    var result = db.suppliers.Where(x => x.ShippingEmail == Email && x.SupplierID != Id).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.suppliers.Where(x => x.ShippingEmail == Email).ToList();
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
        #endregion
        public ActionResult GetSupplier(int SupplierID)
        {
            try
            {
                var result = db.suppliers.Where(x => x.SupplierID == SupplierID).FirstOrDefault();
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

        public ActionResult CheckDuplicateContactPerson(string Name, string Mode, int CustomerID, int ContactPersonID)
        {
            try
            {
                if (Mode == "Edit")
                {
                    var result = db.supplierContacts.Where(x => x.ContactPerson == Name && x.ContactPersonID != ContactPersonID && x.CustomerID == CustomerID).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.supplierContacts.Where(x => x.ContactPerson == Name && x.CustomerID == CustomerID).ToList();
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

        public ActionResult DuplicateContactPersonEmail(string Email, string Mode, int CustomerID, int ContactPersonID)
        {
            try
            {
                if (Mode == "Edit")
                {
                    var result = db.supplierContacts.Where(x => x.Email == Email && x.ContactPersonID != ContactPersonID && x.CustomerID == CustomerID).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.supplierContacts.Where(x => x.Email == Email && x.CustomerID == CustomerID).ToList();
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

        public ActionResult GetContacts(int ID)
        {
            try
            {
                var result = db.supplierContacts.Where(x => x.CustomerID == ID).ToList();
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

        public ActionResult GetOneContact(int ID)
        {
            try
            {
                var result = db.supplierContacts.Where(x => x.ContactPersonID == ID).FirstOrDefault();
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

        public ActionResult SaveContactPerson(int ContactPersonID, int CustomerID, string ContactPerson, string Phone, string Mobile, string Email, bool IsActive)
        {
            try
            {
                if (ContactPersonID == 0)
                {
                    SupplierContacts custContact = new SupplierContacts();
                    custContact.ContactPerson = ContactPerson;
                    custContact.Phone = Phone;
                    custContact.Mobile = Mobile;
                    custContact.Email = Email;
                    custContact.IsActive = true;
                    custContact.CreatedBy = User.Identity.Name;
                    custContact.CreatedDate = DateTime.Now;
                    custContact.CustomerID = CustomerID;

                    db.supplierContacts.Add(custContact);
                    db.SaveChanges();
                }
                else
                {
                    var custContact = db.supplierContacts.Where(x => x.ContactPersonID == ContactPersonID).FirstOrDefault();
                    custContact.ContactPerson = ContactPerson;
                    custContact.Phone = Phone;
                    custContact.Mobile = Mobile;
                    custContact.Email = Email;
                    custContact.IsActive = IsActive;
                    custContact.UpdatedBy = User.Identity.Name;
                    custContact.UpdatedDate = DateTime.Now;
                    custContact.CustomerID = CustomerID;

                    db.SaveChanges();
                }
                var result = "Success";
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

        [ValidateInput(false)]
        public void ExportToExcel(string GridModel)
        {
            ExcelExport exp = new ExcelExport();
            var DataSource = (from Cust in db.suppliers
                              join Emp in db.Employees on Cust.EmployeeID equals Emp.EmployeeID into E
                              from employee in E.DefaultIfEmpty()
                              orderby Cust.CreatedDate descending
                              select new { CustomerID = Cust.SupplierID, CustomerName = Cust.SupplierName, BillingCountry = Cust.BillingCountry, BillingState = Cust.BillingState, BillingCity = Cust.BillingCity, EmployeeID = employee.EmployeeName, IsActive = Cust.IsActive }).ToList();
            GridProperties obj = ConvertGridObject(GridModel);
            exp.Export(obj, DataSource, "Suppliers.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
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
