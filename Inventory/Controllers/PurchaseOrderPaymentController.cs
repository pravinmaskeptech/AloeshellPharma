using Inventory.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class PurchaseOrderPaymentController : Controller
    {
        InventoryModel db = new InventoryModel();
        // GET: PurchaseOrderPayment
        public ActionResult Index()
        {
            var SuppliersDetails = new List<Suppliers>(db.suppliers);
            var purchaseOrderPayment = new List<PurchaseOrderPayment>(db.purchaseOrderPayment);
            ViewBag.datasource = (from order in purchaseOrderPayment

                                  join Supplier in SuppliersDetails on order.SupplierID equals Supplier.SupplierID into supplier 
                                  from sply in supplier.DefaultIfEmpty()
                                  orderby order.ID descending
                                  select new { DocNo = order.DocNo, DocDate = order.DocDate, InvoiceNo = order.InvoiceNo, InvoiceDate = order.InvoiceDate, Amount = order.Amount, PaymentType = order.PaymentType, PaymentMode = order.PaymentMode, ReferenceNo = order.ReferenceNo, Date = order.Date, SupplierID = sply == null ? string.Empty : sply.SupplierName, InvoiceAmount = order.InvoiceAmount, PoNo = order.PoNo, AdvanceAmount = order.AdvanceAmount  }
                                ).ToList();
            return View();
        }
        public ActionResult Payment()
        {
            ViewBag.Invoicedatasource = db.POInvoices.ToList();
            ViewBag.SPLYdatasource = db.suppliers.Where(a => a.IsActive == true);
            return View();
        }
        public JsonResult getPaymentNo()
        {
            var count = db.purchaseOrderPayment.Count();
            count = count + 1;
            var result = "Pay_" + count;
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult ShowInvoiceDetails( int SupplierId)
        {
            var result = db.POInvoices.Where(a => a.TotalAmount != a.PayAmount && a.SupplierId == SupplierId && a.Status != "Complite").ToList();
            return Json(result,JsonRequestBehavior.AllowGet);
        }
        public JsonResult save(List<PurchaseOrderPayment> Invoice)
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
                            if(count==1)
                            {
                                var supplier = db.suppliers.Where(a => a.SupplierID == x.SupplierID).FirstOrDefault();
                                supplier.AdvanceAmount = Convert.ToDecimal(x.hfAdvanceamt);
                            }
                            count = count + 1;
                            if (x.balanceAmount == x.InvoiceAmount)
                            {
                                var poinv = db.POInvoices.Where(a => a.InvoiceNo == x.InvoiceNo).FirstOrDefault();
                                poinv.PayAmount = poinv.PayAmount + x.balanceAmount;
                                poinv.Status ="Complite";
                                db.POInvoices.AddOrUpdate(poinv);
                            }else
                            {
                                var poinv = db.POInvoices.Where(a => a.InvoiceNo == x.InvoiceNo).FirstOrDefault();
                                poinv.PayAmount = poinv.PayAmount + x.balanceAmount;
                                db.POInvoices.AddOrUpdate(poinv);
                            }
                           
                            PurchaseOrderPayment inv = new PurchaseOrderPayment();
                            inv.DocNo = x.DocNo;
                            inv.DocDate = x.DocDate;
                            inv.SupplierID = x.SupplierID;
                            inv.InvoiceNo = x.InvoiceNo;
                            inv.InvoiceDate = x.InvoiceDate;
                            inv.Amount = x.Amount;
                            inv.CompanyID = Convert.ToInt32(Session["CompanyID"]);
                            inv.PaymentType = x.PaymentType;
                            inv.PaymentMode = x.PaymentMode;
                            inv.ReferenceNo = x.ReferenceNo;
                            inv.AdvanceAmount = x.AdvanceAmount;
                            inv.Date = x.Date;
                            inv.PoNo = x.PoNo;
                            inv.Remarks = x.Remarks;
                            inv.PaymentType = x.PaymentType;
                          //  inv.SupplierID = x.SupplierID;
                            inv.InvoiceAmount = x.InvoiceAmount;                            
                            db.purchaseOrderPayment.Add(inv);
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
        public JsonResult getAdvanceAmount(int SupplierId)
        {
            var result = db.suppliers.Where( a=>a.SupplierID == SupplierId).Select(a=>a.AdvanceAmount).Single();            
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AddAdvanceAmount(int SupplierId, string DocNo, decimal Amount, DateTime DocDate, string PaymentMode, string ReferenceNo, string PaymentType, DateTime Date)
        {
            try
            {
                var result = db.suppliers.Where(a => a.SupplierID == SupplierId).FirstOrDefault();
                result.AdvanceAmount = result.AdvanceAmount + Amount;

                PurchaseOrderPayment inv = new PurchaseOrderPayment();
                inv.DocNo = DocNo;
                inv.DocDate = DocDate;
                inv.SupplierID = SupplierId;
                inv.AdvanceAmount = Amount;
                inv.PaymentType = PaymentType;
                inv.PaymentMode = PaymentMode;
                inv.ReferenceNo = ReferenceNo;
                inv.Date = Date;
                inv.CompanyID = Convert.ToInt32(Session["CompanyID"]);
                inv.InvoiceNo = "NA";
                inv.InvoiceDate = null;
                inv.Remarks = "Advance Amount";
                inv.InvoiceAmount = 0;
                inv.PoNo = "NA";
                db.purchaseOrderPayment.Add(inv);
                db.SaveChanges();
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch(Exception ee)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
          
        }

        public JsonResult CheckValidSupplier( string SupplierName)
        {
            var result = db.suppliers.Where(a => a.IsActive == true && a.SupplierName == SupplierName).Count();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}