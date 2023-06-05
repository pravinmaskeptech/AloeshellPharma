using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Inventory.Models;

namespace Inventory.Controllers
{
    public class PRNMainsController : Controller
    {
        private InventoryModel db = new InventoryModel();

        public ActionResult PRNApproverIndex() 
        {
            ViewBag.datasource = db.PRNMains.OrderByDescending(a => a.PRNID).Where(a=>a.Status=="Active").OrderByDescending(a=>a.PRNID).ToList();
            return View(db.PRNMains.ToList());
        }
       
        public ActionResult PRNApproverEdit(int? id)
        {
            ViewBag.Dept = db.Departments.OrderBy(a => a.DeptName).ToList();
            ViewBag.products = db.Products.OrderBy(a => a.ProductName).ToList();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PRNMain pRNMain = db.PRNMains.Find(id);
            if (pRNMain == null)
            {
                return HttpNotFound();
            }
            return View(pRNMain);
        }
        public ActionResult Index()
        {
            ViewBag.datasource = db.PRNMains.OrderByDescending(a => a.PRNID).ToList();
            return View(db.PRNMains.ToList());
        }

        public ActionResult Create()
        {
            var prnno = db.BillNumbering.Where(a => a.Type == "PRNNo").Select(a => a.Number).FirstOrDefault();
            ViewBag.PRNNO = "PRN/" + prnno;
            ViewBag.products = db.Products.OrderBy(a => a.ProductName).ToList();
            ViewBag.Dept = db.Departments.OrderBy(a => a.DeptName).ToList();
            return View();
        }

        public ActionResult Edit(int? id)
        {
            ViewBag.Dept = db.Departments.OrderBy(a => a.DeptName).ToList();
            ViewBag.products = db.Products.OrderBy(a => a.ProductName).ToList();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PRNMain pRNMain = db.PRNMains.Find(id);
            if (pRNMain == null)
            {
                return HttpNotFound(); 
            }
            return View(pRNMain);
        }        

        public JsonResult CheckValidProduct(string ProductName)
        {
            try
            {
                var data = db.Products.Where(a => a.ProductName == ProductName).Select(a => a.ProductCode).ToList();
                var result = new { Message = "success", data };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                var result = new { Message = Ex.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult GETPRNData(int ID) 
        {
            try
            {
                var PRN = db.PRNMains.Where(a => a.PRNID == ID).FirstOrDefault();
                var DTL = db.PRNDetails.Where(a => a.PRNID == ID).ToList();
                var result = new { Message = "success", PRN, DTL };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                var result = new { Message = Ex.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

       

        



        public JsonResult Save(List<PRNMain> OrderDetails)
        {
            try
            {
                var prnno = db.BillNumbering.Where(a => a.Type == "PRNNo").FirstOrDefault();
                var PRNNO = "PRN/" + prnno.Number;
                prnno.Number = prnno.Number + 1;             
              
                if (OrderDetails.Count > 0)
                {
                    using (var dbcxtransaction = db.Database.BeginTransaction())
                    {
                        int code = 0;
                        int cnt = 1;
                        foreach (var x in OrderDetails)
                        {
                            if (cnt == 1)
                            {
                                PRNMain main = new PRNMain();
                                main.PRNNo = PRNNO;
                                main.RaisedBy = User.Identity.Name;
                                main.RaisedDate = x.RaisedDate;
                                //  main.CompanyID = Convert.ToInt32(Session["CompanyID"]);
                                main.RequiredDate = x.RequiredDate;
                                main.Department = x.Department;
                                main.Status = "Active";

                                main.CreatedBy = User.Identity.Name;
                                main.CreatedDate = DateTime.Today;
                                db.PRNMains.Add(main);
                                db.SaveChanges();
                                code = db.PRNMains.Max(a => a.PRNID);

                            }

                            cnt = cnt + 1;
                            PRNDetails details = new PRNDetails();
                            details.PRNID = code;
                            details.ProductCode = x.ProductCode;
                            details.PRNNo = PRNNO;
                            details.ProductName = x.ProductName;
                            details.Quantity = x.Quantity;
                            details.CreatedBy = User.Identity.Name;
                            details.CreatedDate = DateTime.Now;                         
                            db.PRNDetails.Add(details);
                        }


                        db.SaveChanges();
                        dbcxtransaction.Commit();
                        var result = new { Message = "success" };
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }


                }
                else
                {
                    var result = new { Message = "No Data " };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ee)
            {
                var result = new { Message = ee.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Update(List<PRNMain> OrderDetails) 
        {
            try
            {               
                if (OrderDetails.Count > 0)
                {
                    using (var dbcxtransaction = db.Database.BeginTransaction())
                    {                    
                        int cnt = 1;
                        foreach (var x in OrderDetails)
                        {
                            if (cnt == 1)
                            {
                                var main = db.PRNMains.Where(a => a.PRNID == x.PRNID).FirstOrDefault();                               
                              
                                main.RaisedDate = x.RaisedDate;                              
                                main.RequiredDate = x.RequiredDate;
                                main.Department = x.Department;
                                main.Status = x.Status;
                                main.DisapproveReason = x.DisapproveReason; 
                                main.UpdatedBy = User.Identity.Name;
                                main.UpdatedDate = DateTime.Today; 
                                
                            }
                            cnt++;
                            if (x.ID == 0)
                            {
                                PRNDetails details = new PRNDetails();
                                details.PRNID = x.PRNID;
                                details.ProductCode = x.ProductCode;
                                details.PRNNo = x.PRNNo;
                                details.ProductName = x.ProductName;
                                details.Quantity = x.Quantity;
                                details.CreatedBy = User.Identity.Name;
                                details.CreatedDate = DateTime.Now;
                                db.PRNDetails.Add(details);
                            }
                        }


                        db.SaveChanges();
                        dbcxtransaction.Commit();
                        var result = new { Message = "success" };
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    var result = new { Message = "No Data " };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ee)
            {
                var result = new { Message = ee.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveApproveStatus(int ID,string Status,string DisapproveReason)
        {
            try
            {
                var PRN = db.PRNMains.Where(a => a.PRNID == ID).FirstOrDefault();
                PRN.Status = Status;
                PRN.DisapproveReason = DisapproveReason;
                db.SaveChanges();
                   var result = new { Message = "success" };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                var result = new { Message = Ex.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }
    }
}

