using Inventory.Models;
using Syncfusion.EJ.Export;
using Syncfusion.JavaScript.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using Syncfusion.XlsIO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Inventory.Controllers
{
    public class MMEController : Controller
    {
        private InventoryModel db = new InventoryModel();

        // GET: Products
        public ActionResult Index()
        {
            ViewBag.datasource = (from mme in db.MMEMaster
                                  where mme.IsActive == true
                                  orderby mme.MMEID descending
                                  select new
                                  {
                                      MMEID = mme.MMEID,
                                      ContactNo = mme.ContactNo,
                                      Pincode = mme.Pincode,
                                      Password = mme.Password,
                                      City = mme.City,
                                      MMECode = mme.MMECode,
                                      MMEName = mme.MMEName,
                                      Address = mme.Address,
                                      Email = mme.Email,
                                  }
                                ).ToList();

            return View();
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MMEMaster mme = db.MMEMaster.Find(id);
            if (mme == null)
            {
                return HttpNotFound();
            }
            return View(mme);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(MMEMaster mme)
        {

            try
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var username = await (from u in context.Users
                                      where u.UserName == mme.MMECode
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
                    int? maxId = await db.MMEMaster.MaxAsync(s => (int?)s.MMEID);
                    if (!maxId.HasValue || maxId == 0)
                    {
                        maxId = 1;
                    }
                    else
                    {
                        maxId = maxId + 1;
                    }

                    mme.IsActive = true;
                    mme.CreatedBy = User.Identity.Name;
                    mme.CreatedDate = DateTime.Now;
                    mme.MMECode = string.Format("MME{0:D4}", maxId);
                    db.MMEMaster.Add(mme);
                    //db.SaveChanges();

                    var store = new UserStore<ApplicationUser>(context);
                    var manager = new ApplicationUserManager(store);
                    var user = new ApplicationUser { UserName = mme.MMECode, Email = mme.Email, PhoneNumber = mme.ContactNo };
                    var adminresult = await manager.CreateAsync(user, mme.Password);
                   
                    if (adminresult.Succeeded == false)
                    {
                        TempData["Temp"] = $"{mme.MMECode} add failed";
                        return RedirectToAction("Index");
                    }
                    var asprole = new AspNetUserRoles();
                    asprole.UserId = user.Id;
                    asprole.RoleId = "MME";
                    db.AspNetUserRoles.Add(asprole);

                    await db.SaveChangesAsync();
                    TempData["Temp"] = $"{mme.MMECode} added successfully";
                    return RedirectToAction("Index");

                }
            }
            catch (Exception Ex)
            {
                var SalesPerson = db.MMEMaster.Where(a => a.MMEID == mme.MMEID).ToList();
                if (SalesPerson != null) { ViewBag.SalesPerson = SalesPerson; }
                TempData["error"] = Ex.Message;

                return View(mme);
            }
        }

               
        public ActionResult Edit(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MMEMaster mme = db.MMEMaster.Where(a => a.MMEID == id).FirstOrDefault();
            if (mme == null)
            {
                return HttpNotFound();
            }
            return View(mme);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MMEMaster mme)
        {

            try
            {
                MMEMaster mmemaster = db.MMEMaster.Where(a => a.MMECode == mme.MMECode).FirstOrDefault();
                mmemaster.MMEName = mme.MMEName;
                mmemaster.MMECode = mme.MMECode;
                mmemaster.Pincode = mme.Pincode;
                mmemaster.Email = mme.Email;
                mmemaster.City = mme.City;
                mmemaster.Address = mme.Address;
                mmemaster.ContactNo = mme.ContactNo;
                mmemaster.Password = mme.Password;

                db.Entry(mmemaster).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Temp"] = $"{mmemaster.MMECode} updated successfully";
                return RedirectToAction("Index");
            }
            catch (Exception Ex)
            {
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return View(mme);
            }
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int? ID)
        {
            try
            {
                MMEMaster mme = db.MMEMaster.Where(a => a.MMEID == ID).FirstOrDefault();
                mme.IsActive = false;
                db.SaveChanges();
                TempData["success"] = "Record Deleted Successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["error"] = "Delete  failed.";
                return View();
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

        public ActionResult CheckDuplicateName(string Name, string Mode, string Code)
        {
            try
            {
                if (Mode == "Edit")
                {

                    var result = db.MMEMaster.Where(f => f.MMEName == Name && f.MMECode != Code).ToList();
                    if (result.Count == 0)
                        return Json("0", JsonRequestBehavior.AllowGet);
                    else
                        return Json("1", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.MMEMaster.Where(f => f.MMEName == Name).ToList();
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

        public JsonResult GetData(string Id)
        {
            try
            {
                var result = db.MMEMaster.Where(a => a.MMECode.Equals(Id) && a.IsActive == true).FirstOrDefault();
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
     
        [ValidateInput(false)]
        public void ExportToExcel(string GridModel)
        {
            ExcelExport exp = new ExcelExport();
           
            var Datasource = (from mme in db.MMEMaster
                              where mme.IsActive == true
                              orderby mme.MMECode descending
                              select new { MMECode = mme.MMECode, MMEName = mme.MMEName, Password = mme.Password, Pincode = mme.Pincode, ContactNo = mme.ContactNo, Address = mme.Address, Email = mme.Email, }
                                ).ToList();
            GridProperties obj = ConvertGridObject(GridModel);
            obj.Columns[8].DataSource = db.Categories.ToList();
            exp.Export(obj, Datasource, "Categories.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
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