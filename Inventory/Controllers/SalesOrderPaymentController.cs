using Inventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class SalesOrderPaymentController : Controller
    {
        InventoryModel db = new InventoryModel();
        // GET: SalesOrderPayment
        public ActionResult Index()
        {
            var CustomersDetails = new List<Customer>(db.Customers);
            var salesOrderPayment = new List<SalesOrderPayment>(db.SalesOrderPayment);
            ViewBag.datasource = (from order in salesOrderPayment

                                  join Customer in CustomersDetails on order.CustomerId equals Customer.CustomerID into customer
                                  from cust in customer.DefaultIfEmpty()
                                  orderby order.ID descending
                                  select new { DocNo = order.DocNo, DocDate = order.DocDate, InvoiceNo = order.InvoiceNo, InvoiceDate = order.InvoiceDate, Amount = order.Amount, PaymentType = order.PaymentType, PaymentMode = order.PaymentMode, ReferenceNo = order.ReferenceNo, Date = order.Date, CustomerName = cust == null ? string.Empty : cust.CustomerName, InvoiceAmount = order.InvoiceAmount, SoNo = order.SoNo, AdvanceAmount = order.AdvanceAmount }
                                ).ToList();
            return View();
        }
        public ActionResult Payment()
        {
        //    ViewBag.Invoicedatasource = db.POInvoices.ToList();
            ViewBag.Custdatasource = db.Customers.Where(a => a.IsActive == true);
            return View();
        }
        public JsonResult getPaymentNo()
        {
            var count = db.SalesOrderPayment.Count();
            count = count + 1;
            var result = "Pay_" + count;
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult ShowInvoiceDetails(int CustomerId)
        {
            // var result = db.Sales.Where(a =>  TotalAmount != a.PayAmount && a.CustomerID == CustomerId && a.Status != "Complite").ToList();
            try
            {
                var result = (from c in db.Sales.Where(a => a.TotalAmount != a.PayAmount && a.CustomerID == CustomerId && a.Status != "Complite").ToList()
                              join sply in db.orderMain on c.OrderNo equals sply.OrderNo into main
                              from aa in main.Where(a => a.TotalAmount != a.PayAmount && a.CustomerID == CustomerId && a.Status != "Complite").DefaultIfEmpty()
                              group c by new
                              {
                                  c.InvoiceDate,
                                  c.InvoiceNo,
                                  aa.PayAmount,
                                  c.OrderNo,
                              } into gcs
                              select new
                              {
                                  InvoiceDate = gcs.Key.InvoiceDate,
                                  OrderNo = gcs.Key.OrderNo,
                                  InvoiceNo = gcs.Key.InvoiceNo,
                                  PayAmount = gcs.Key.PayAmount,
                                  InvoiceAmount = (gcs.Sum(a => a.TotalAmount)),
                              }).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }catch(Exception ee)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult save(List<SalesOrderPayment> Invoice)
        {
            var status = false;
            int cnt = 1;
            try
            {
                if (Invoice != null)
                {
                    if (Invoice.Count > 0)
                    {
                        foreach (var x in Invoice)
                        {
                            int count = 1;
                            if (count == 1)
                            {
                                var customer = db.Customers.Where(a => a.CustomerID == x.CustomerId).FirstOrDefault();
                                customer.AdvanceAmount = Convert.ToDecimal(x.hfAdvanceamt);
                            }
                            count = count + 1;
                            if (x.balanceAmount == x.InvoiceAmount)
                            {
                                var Salesinv = db.orderMain.Where(a => a.OrderNo == x.SoNo).FirstOrDefault();
                                Salesinv.PayAmount = Salesinv.PayAmount + x.balanceAmount;
                                Salesinv.Status = "Complite";
                            }
                            else
                            {
                                var poinv = db.orderMain.Where(a => a.OrderNo == x.SoNo).FirstOrDefault();
                                poinv.PayAmount = poinv.PayAmount + x.balanceAmount;
                            }

                            SalesOrderPayment inv = new SalesOrderPayment();
                            inv.DocNo = x.DocNo;
                            inv.DocDate = x.DocDate;
                            inv.CustomerId = x.CustomerId;
                            inv.InvoiceNo = x.InvoiceNo;
                            inv.InvoiceDate = x.InvoiceDate;
                            inv.Amount = x.Amount;
                            inv.PaymentType = x.PaymentType;
                            inv.PaymentMode = x.PaymentMode;
                            inv.ReferenceNo = x.ReferenceNo;
                            inv.Date = x.Date;
                            inv.SoNo = x.SoNo;
                            inv.Remarks = x.Remarks;
                            inv.PaymentType = x.PaymentType;
                            //  inv.SupplierID = x.SupplierID;
                            inv.InvoiceAmount = x.InvoiceAmount;
                            db.SalesOrderPayment.Add(inv);
                            db.SaveChanges();
                            status = true;
                        }
                        return new JsonResult { Data = new { status } };
                    }
                }
            }
            catch (Exception ee)
            {
                status = false;
                return new JsonResult { Data = new { status } };
            }
            return new JsonResult { Data = new { status } };
        }
        public JsonResult getAdvanceAmount(int CustomerId)
        {
            var result = db.Customers.Where(a => a.CustomerID == CustomerId).Select(a => a.AdvanceAmount).Single();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AddAdvanceAmount(int CustomerId, string DocNo, decimal Amount, DateTime DocDate, string PaymentMode, string ReferenceNo, string PaymentType, DateTime Date)
        {
            try
            {
                var result = db.Customers.Where(a => a.CustomerID == CustomerId).FirstOrDefault();
                result.AdvanceAmount = result.AdvanceAmount + Amount;

                SalesOrderPayment inv = new SalesOrderPayment();
                inv.DocNo = DocNo;
                inv.DocDate = DocDate;
                inv.CustomerId = CustomerId;
                inv.AdvanceAmount = Amount;
                inv.PaymentType = PaymentType;
                inv.PaymentMode = PaymentMode;
                inv.ReferenceNo = ReferenceNo;
                inv.Date = Date;
                inv.InvoiceNo = "NA";
                inv.InvoiceDate = null;
                inv.Remarks = "Advance Amount";
                inv.InvoiceAmount = 0;
                inv.SoNo = "NA";               
                db.SalesOrderPayment.Add(inv);
                db.SaveChanges();
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ee)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }

        }
        public JsonResult CheckValidCustomer(string CustomerName) 
        {
            var result = db.Customers.Where(a => a.IsActive == true && a.CustomerName == CustomerName).Count();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
