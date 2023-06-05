using Inventory.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace Inventory.Controllers
{
    public class SalesPersonController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        InventoryModel db = new InventoryModel();
        public ActionResult Index()
        {
            try
            {
                ViewBag.datasource = (from Cust in db.SalesPersonMasters
                                      where Cust.IsActive == true
                                      orderby Cust.CreatedDate descending
                                      select new { SalesPersonID = Cust.SalesPersonID, MMEID = Cust.MMEID, SalesPersonName = Cust.SalesPersonName, SalesPersonCode = Cust.SalesPersonCode, Password = Cust.Password, Address = Cust.Address, City = Cust.City, Pincode = Cust.Pincode, ContactNo = Cust.ContactNo, Email = Cust.Email }).ToList();
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
        public async Task<ActionResult> Create()
        {
            try
            {
                ViewBag.MMEDataSource =await db.MMEMaster.Where(a => a.IsActive == true).ToListAsync();           
                return View();
            }
            catch (Exception Ex)
            {
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return RedirectToAction("Index");
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }



        [HttpPost]
        public async Task<ActionResult> Create(SalesPersonMaster salesPerson)
        {
            try
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var username =await (from u in context.Users
                                where u.UserName == salesPerson.SalesPersonCode
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
                    int? maxId = await db.SalesPersonMasters.MaxAsync(s => (int?)s.SalesPersonID);
                    if (!maxId.HasValue || maxId == 0)
                    {
                        maxId = 1;
                    }
                    else
                    {
                        maxId = maxId + 1;
                    }
                    salesPerson.MMEID = 2;
                    salesPerson.IsActive = true;
                    salesPerson.CreatedBy = User.Identity.Name;
                    salesPerson.CreatedDate = DateTime.Now;
                    salesPerson.SalesPersonCode = string.Format("PNM{0:D4}", maxId);

                    db.SalesPersonMasters.Add(salesPerson);
                    //db.SaveChanges();
                 
                    //var store = new UserStore<ApplicationUser>(context);
                    //var manager = new ApplicationUserManager(store);
                    //var user = new ApplicationUser { UserName = salesPerson.SalesPersonCode, Email = salesPerson.Email, PhoneNumber = salesPerson.ContactNo };
                    //var adminresult =await manager.CreateAsync(user, salesPerson.Password);
                    
                    //if (adminresult.Succeeded == false)
                    //{
                    //    TempData["error"] = $"{salesPerson.SalesPersonCode} add failed";
                    //    return RedirectToAction("Index");
                    //}

                    //var asprole = new AspNetUserRoles();
                    //asprole.UserId = user.Id;
                    //asprole.RoleId = "SalesPerson";
                    //db.AspNetUserRoles.Add(asprole);

                    db.SaveChanges();
                    TempData["Temp"] = $"{salesPerson.SalesPersonCode} added successfully";
                    return RedirectToAction("Index");

                }
            }
            catch (Exception Ex)
            {
                var SalesPerson = db.SalesPersonMasters.Where(a => a.SalesPersonID == salesPerson.SalesPersonID).ToList();
                if (SalesPerson != null) { ViewBag.SalesPerson = SalesPerson; }
                TempData["error"] = Ex.Message;

                return View(salesPerson);
            }
        }



        [HttpGet]
        public ActionResult Edit(int id)
        {
            try
            {
                ViewBag.MMEDataSource = db.MMEMaster.Where(a => a.IsActive == true).ToList();
                if (id == 0 || id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                SalesPersonMaster supplier = db.SalesPersonMasters.Find(id);
                if ( supplier == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

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
        public ActionResult Edit(SalesPersonMaster suppliers)
        {

            try
            {
                SalesPersonMaster sales = db.SalesPersonMasters.Find(suppliers.SalesPersonID);
                sales.City = suppliers.City;
                sales.Email = suppliers.Email;
                sales.Address = suppliers.Address;
                sales.ContactNo = suppliers.ContactNo;
                sales.Password = suppliers.Password;
                sales.SalesPersonName = suppliers.SalesPersonName;
                sales.Pincode = suppliers.Pincode;
                sales.UpdatedDate = DateTime.Now;
                sales.UpdatedBy = User.Identity.Name;
                sales.MMEID = 2;
                db.Entry(sales).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Temp"] = $"{sales.SalesPersonCode} updated successfully";
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
                    var result = db.SalesPersonMasters.Where(x => x.SalesPersonName == Name && x.SalesPersonID != Id).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.SalesPersonMasters.Where(x => x.SalesPersonName == Name).ToList();
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
                var result = db.SalesPersonMasters.Where(x => x.SalesPersonID == SalesPersonID && x.IsActive == true).FirstOrDefault();
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

        public JsonResult Delete(int ID)
        {
            var x = db.SalesPersonMasters.Where(a => a.SalesPersonID == ID).FirstOrDefault();
            x.IsActive = false;
            db.SaveChanges();
            var result = new { Message = "success" };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult ChkCategory(int Cat)
        {
            List<MMEMaster> result = new List<MMEMaster>();
            if (Cat != 0)
            {
                result = db.MMEMaster.Where(a => a.MMEID == Cat).ToList();
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
