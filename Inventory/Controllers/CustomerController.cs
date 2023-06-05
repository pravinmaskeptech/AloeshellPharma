using Inventory.Models;
using Microsoft.Owin.BuilderProperties;
using Syncfusion.EJ.Export;
using Syncfusion.JavaScript.Models;
using Syncfusion.UI.Xaml.Maps;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Inventory.Controllers
{
    public class CustomerController : Controller
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
                ViewBag.datasource = (from Cust in db.Customers
                                      join Emp in db.Employees on Cust.EmployeeID equals Emp.EmployeeID into E
                                      from employee in E.DefaultIfEmpty()
                                      orderby Cust.CreatedDate descending
                                      select new { CustomerID = Cust.CustomerID, CustomerName = Cust.CustomerName, BillingCountry = Cust.BillingCountry, BillingState = Cust.BillingState, BillingCity = Cust.BillingCity, EmployeeID = employee.EmployeeName, IsActive = Cust.IsActive }).ToList();
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

        //
        // POST: /Customer/Create
        [HttpPost]
        public ActionResult Create(Customer customer)
        {

            try
            {
                customer.IsActive = true;
                customer.CreatedBy = User.Identity.Name;
                customer.CreatedDate = DateTime.Now;
                customer.EmployeeID = User.Identity.Name;
                db.Customers.Add(customer);
                db.SaveChanges();

                Customer ct = db.Customers.Where(a => a.BillingPhone == customer.BillingPhone).FirstOrDefault();

                Shipper shipperDtl = new Shipper();

                shipperDtl.ContactPerson = customer.CustomerName;
                shipperDtl.ContactNo = customer.ShippingPhone;
                shipperDtl.Email = customer.ShippingEmail;
                shipperDtl.Pincode = customer.ShippingPincode;
                shipperDtl.City = customer.ShippingCity;
                shipperDtl.Address = customer.ShippingAddress;
                shipperDtl.CustomerId = Convert.ToInt32(ct.CustomerID);
                shipperDtl.Freeze = true;
                shipperDtl.Country = customer.ShippingCountry;
                shipperDtl.State = customer.ShippingState;
                shipperDtl.Name = customer.CustomerName;
                shipperDtl.CreatedBy = User.Identity.Name;
                shipperDtl.CreatedDate = DateTime.Now;
                shipperDtl.PlaceOfSupply = customer.ShippingCity;
                shipperDtl.UpdatedBy = "";
                shipperDtl.Customer = "";
                shipperDtl.PlaceOfSupply = customer.ShippingCity;

                db.Shippers.Add(shipperDtl);
                db.SaveChanges();
                // TODO: Add insert logic here
                TempData["Temp"] = "Customer Updated Successfully";
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

            //else
            //{
            //    ViewBag.VIGST = new List<SelectListItem>
            //                                                {
            //                                                    new SelectListItem{ Text="False", Value = "False" },
            //                                                    new SelectListItem{ Text="True", Value = "True" }
            //                                                };
            //    ViewBag.Employee = db.Employees.Where(x => x.IsActive != false).ToList();
            //    ViewBag.Curr = db.Currencies.Where(x => x.IsActive != false).ToList();
            //    ViewBag.Pterms = db.Paymentterms.Where(x => x.IsActive != false).ToList();
            //    return View(customer);
            //}
        }
        public ActionResult Edit(int id)
        {
            try
            {
                ViewBag.VIGST = new List<SelectListItem>
                                                            {
                                                                new SelectListItem { Text = "No", Value = "False" } , 
                                                                new SelectListItem { Text = "Yes", Value = "True" }
                                                            };
                ViewBag.Employee = db.Employees.Where(x => x.IsActive != false).ToList();
                ViewBag.Curr = db.Currencies.Where(x => x.IsActive != false).ToList();
                ViewBag.Pterms = db.Paymentterms.Where(x => x.IsActive != false).ToList();

                Customer customer = db.Customers.Find(id);

                return View(customer);
            }
            catch (Exception Ex)
            {
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return RedirectToAction("Index");
            }
        }

        //
        // POST: /Customer/Edit/5
        [HttpPost]
        public ActionResult Edit(Customer customer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    customer.UpdateDate = DateTime.Now;
                    customer.UpdatedBy = User.Identity.Name;
                    customer.EmployeeID = User.Identity.Name;
                    db.Entry(customer).State = EntityState.Modified;
                    db.SaveChanges();

                    Shipper shipperDtl = db.Shippers.Where(a => a.ContactNo == customer.ShippingPhone).FirstOrDefault();

                    shipperDtl.ContactPerson = customer.CustomerName;
                    shipperDtl.ContactNo = customer.ShippingPhone;
                    shipperDtl.Email = customer.ShippingEmail;
                    shipperDtl.Pincode = customer.ShippingPincode;
                    shipperDtl.City = customer.ShippingCity;
                    shipperDtl.Address = customer.ShippingAddress;
                    shipperDtl.Freeze = true;
                    shipperDtl.Country = customer.ShippingCountry;
                    shipperDtl.State = customer.ShippingState;
                    shipperDtl.Name = customer.CustomerName;
                    shipperDtl.UpdatedBy = User.Identity.Name;
                    shipperDtl.PlaceOfSupply = customer.BillingCity;

                    db.Shippers.AddOrUpdate(shipperDtl);
                    db.SaveChanges();
                    TempData["Temp"] = "Customer Updated Successfully";
                    return RedirectToAction("Index");
                }
                catch (Exception Ex)
                {
                    ViewBag.VIGST = new List<SelectListItem>
                                                            {
                                                                new SelectListItem { Text = "No", Value = "False" } , new SelectListItem { Text = "Yes", Value = "True" }
                                                            };
                    ViewBag.Employee = db.Employees.Where(x => x.IsActive != false).ToList();
                    ViewBag.Curr = db.Currencies.Where(x => x.IsActive != false).ToList();
                    ViewBag.Pterms = db.Paymentterms.Where(x => x.IsActive != false).ToList();
                    var msg = Ex.Message.ToString();
                    TempData["Msg"] = msg;
                    return View(customer);
                }
            }
            else
            {
                ViewBag.VIGST = new List<SelectListItem>
                                                            {
                                                                new SelectListItem{ Text="No", Value = "False" },
                                                                new SelectListItem{ Text="Yes", Value = "True" }
                                                            };
                ViewBag.Employee = db.Employees.Where(x => x.IsActive != false).ToList();
                ViewBag.Curr = db.Currencies.Where(x => x.IsActive != false).ToList();
                ViewBag.Pterms = db.Paymentterms.Where(x => x.IsActive != false).ToList();
                return View(customer);
            }
        }

        public ActionResult CheckDuplicateName(string Name, string Mode, int Id)
        {
            try
            {
                if (Mode == "Edit")
                {
                    var result = db.Customers.Where(x => x.CustomerName == Name && x.CustomerID != Id).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.Customers.Where(x => x.CustomerName == Name).ToList();
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

        public ActionResult CheckDuplicateContactPerson(string Name, string Mode, int CustomerID, int ContactPersonID)
        {
            try
            {
                if (Mode == "Edit")
                {
                    var result = db.CustomerContacts.Where(x => x.ContactPerson == Name && x.ContactPersonID != ContactPersonID && x.CustomerID == CustomerID).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.CustomerContacts.Where(x => x.ContactPerson == Name && x.CustomerID == CustomerID).ToList();
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
                    var result = db.CustomerContacts.Where(x => x.Email == Email && x.ContactPersonID != ContactPersonID && x.CustomerID == CustomerID).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.CustomerContacts.Where(x => x.Email == Email && x.CustomerID == CustomerID).ToList();
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
        public ActionResult CheckDuplicateBillingMail(string Email, string Mode, int Id)
        {
            try
            {
                if (Mode == "Edit")
                {
                    var result = db.Customers.Where(x => x.BillingEmail == Email && x.CustomerID != Id).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.Customers.Where(x => x.BillingEmail == Email).ToList();
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
                    var result = db.Customers.Where(x => x.ShippingEmail == Email && x.CustomerID != Id).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.Customers.Where(x => x.ShippingEmail == Email).ToList();
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
        public ActionResult GetCustomer(int CustomerID)
        {
            try
            {
                var result = db.Customers.Where(x => x.CustomerID == CustomerID).FirstOrDefault();
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

        public ActionResult GetContacts(int ID)
        {
            try
            {
                var result = db.CustomerContacts.Where(x => x.CustomerID == ID).ToList();
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
                var result = db.CustomerContacts.Where(x => x.ContactPersonID == ID).FirstOrDefault();
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
                    CustomerContacts custContact = new CustomerContacts();
                    custContact.ContactPerson = ContactPerson;
                    custContact.Phone = Phone;
                    custContact.Mobile = Mobile;
                    custContact.Email = Email;
                    custContact.IsActive = true;
                    custContact.CreatedBy = "Test User";
                    custContact.CreatedDate = DateTime.Now;
                    custContact.CustomerID = CustomerID;

                    db.CustomerContacts.Add(custContact);
                    db.SaveChanges();
                }
                else
                {
                    var custContact = db.CustomerContacts.Where(x => x.ContactPersonID == ContactPersonID).FirstOrDefault();
                    custContact.ContactPerson = ContactPerson;
                    custContact.Phone = Phone;
                    custContact.Mobile = Mobile;
                    custContact.Email = Email;
                    custContact.IsActive = IsActive;
                    custContact.UpdatedBy = "Test User";
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
            var DataSource = (from Cust in db.Customers
                              join Emp in db.Employees on Cust.EmployeeID equals Emp.EmployeeID into E
                              from employee in E.DefaultIfEmpty()
                              orderby Cust.CreatedDate descending
                              select new { CustomerID = Cust.CustomerID, CustomerName = Cust.CustomerName, BillingCountry = Cust.BillingCountry, BillingState = Cust.BillingState, BillingCity = Cust.BillingCity, EmployeeID = employee.EmployeeName, IsActive = Cust.IsActive }).ToList();
            GridProperties obj = ConvertGridObject(GridModel);
            exp.Export(obj, DataSource, "Customer.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
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

        public ActionResult SaveShipperContactPerson(int ContactPersonID, int CustomerID, string ContactPerson, string Phone, string Email, bool IsActive, string Address, string City, string Pincode, string Name, string Country, string State)
        {
            try
            {
                if (ContactPersonID == 0)
                {
                    try
                    {
                        Shipper shipperDtl = new Shipper();

                        shipperDtl.ContactPerson = ContactPerson;
                        shipperDtl.ContactNo = Phone;
                        shipperDtl.Email = Email;
                        shipperDtl.Pincode = Pincode;
                        shipperDtl.City = City;
                        shipperDtl.Address = Address;
                        shipperDtl.CustomerId = Convert.ToInt32(CustomerID);
                        shipperDtl.Freeze = true;
                        shipperDtl.Country = Country;
                        shipperDtl.State = State;
                        shipperDtl.Name = Name;
                        shipperDtl.CreatedBy = User.Identity.Name;
                        shipperDtl.CreatedDate = DateTime.Now;
                        shipperDtl.UpdatedBy = "";
                        shipperDtl.Customer = "";
                        shipperDtl.PlaceOfSupply = Address;

                        db.Shippers.Add(shipperDtl);
                        db.SaveChanges();
                    }
                    catch (Exception ee)
                    {
                        ;
                    }

                }
                else
                {
                    var shipperDtl = db.Shippers.Where(x => x.ShipperId == ContactPersonID).FirstOrDefault();
                    shipperDtl.ContactPerson = ContactPerson;
                    shipperDtl.ContactNo = Phone;
                    shipperDtl.Email = Email;
                    shipperDtl.Pincode = Pincode;
                    shipperDtl.City = City;
                    shipperDtl.Address = Address;
                    shipperDtl.Freeze = Convert.ToBoolean(IsActive);
                    shipperDtl.CustomerId = Convert.ToInt32(CustomerID);
                    shipperDtl.UpdatedBy = User.Identity.Name;
                    shipperDtl.UpdationDate = DateTime.Now;
                    shipperDtl.Country = Country;
                    shipperDtl.State = State;
                    shipperDtl.Name = Name;
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

        public ActionResult GetShipperDetails(int ID)
        {
            try
            {
                var result = db.Shippers.Where(x => x.CustomerId == ID).ToList();
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
        public ActionResult GetSingleShipperDetails(int ID)
        {
            try
            {
                var result = db.Shippers.Where(x => x.ShipperId == ID).FirstOrDefault();
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
        public ActionResult CheckDuplicateShipperName(string Name, string Mode, int CustomerID, int ShipperID)
        {
            try
            {
                if (Mode == "Edit")
                {
                    var result = db.Shippers.Where(x => x.Name == Name && x.ShipperId != ShipperID && x.CustomerId == CustomerID).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.Shippers.Where(x => x.Name == Name && x.CustomerId == CustomerID).ToList();
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
    }
}
