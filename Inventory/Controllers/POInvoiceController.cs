using Inventory.Models;
using iTextSharp.text.pdf;
using Microsoft.Ajax.Utilities;
using Syncfusion.PMML;
using Syncfusion.Windows.Shared;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class POInvoiceController : Controller
    {
        InventoryModel db = new InventoryModel();
        // GET: POInvoice
        public ActionResult Index()
        {
            var dataSource = (from invoice in db.POInvoices
                              join supplier in db.suppliers on invoice.SupplierId equals supplier.SupplierID
                              orderby invoice.DocumentDate descending
                              select new
                              {
                                  DocunentNo = invoice.DocunentNo,
                                  InvoiceDate = invoice.InvoiceDate,
                                  DocumentDate = invoice.DocumentDate,
                                  InvoiceNo = invoice.InvoiceNo,
                                  PoDate = invoice.PoDate,
                                  BasicAmount = invoice.BasicAmount,
                                  DiscAmount = invoice.DiscAmount,
                                  TransportAmount = invoice.TransportAmount,
                                  TaxAmount = invoice.TaxAmount,
                                  TotalAmount = invoice.TotalAmount,
                                  SupplierName = supplier.SupplierName,
                              }).ToList();

            ViewBag.datasource = dataSource;


            //ViewBag.datasource = db.POInvoices.OrderByDescending(a=>a.DocunentNo).ToList();
            return View();
        }
        public ActionResult Invoice()
        {

            ViewBag.SPLYdatasource = db.suppliers.Where(a => a.IsActive == true);
            return View();
        }

        // GET: Categories/Edit/5
        [HttpGet]
        public ActionResult Edit(string id)
        {
            var supplier = db.POInvoices.Where(a => a.DocunentNo == id).FirstOrDefault();
            if (supplier == null)
            {
                return View();
            }

            ViewBag.SPLYdatasource = db.suppliers.Where(a => a.SupplierID == supplier.SupplierId).ToList();
            ViewBag.datasource = db.POInvoices.ToList();
            if (id == null || id == "")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            POInvoice category = db.POInvoices.Where(a => a.DocunentNo == id).FirstOrDefault();
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }
        public JsonResult GetSupplierName(int SupplierId)
        {
            if (SupplierId == null || SupplierId == 0)
            {
                return new JsonResult { Data = new { status = false } };
            }
            Suppliers category = db.suppliers.Where(a => a.SupplierID == SupplierId).FirstOrDefault();
            if (category == null)
            {
                return new JsonResult { Data = new { status = false } };
            }
            return Json(category, JsonRequestBehavior.AllowGet);
        }
        public JsonResult getData(string InvoiceNo)
        {
            if (InvoiceNo == null || InvoiceNo == "")
            {
                return new JsonResult { Data = new { status = false } };
            }
            POInvoice category = db.POInvoices.Where(a => a.InvoiceNo == InvoiceNo).FirstOrDefault();
            if (category == null)
            {
                return new JsonResult { Data = new { status = false } };
            }
            return Json(category, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult getDocument(string DocunentNo)
        {

            ViewBag.datasource = db.POInvoices.ToList();
            if (DocunentNo == null || DocunentNo == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            POInvoice category = db.POInvoices.Where(a => a.DocunentNo == DocunentNo).FirstOrDefault();
            if (category == null)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            return Json(category, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult Edit(POInvoice pOInvoice)
        {


            var status = false;
            var po = db.GRNDetail.FirstOrDefault(a => a.InvoiceNo == pOInvoice.InvoiceNo);
            if (po == null)
            {
                return new JsonResult { Data = new { status } };
            }

            //DateTime? d = Convert.ToDateTime(pOInvoice.PaymentDate, CultureInfo.InvariantCulture);
            //DateTime? e = Convert.ToDateTime(pOInvoice.DocumentDate, CultureInfo.InvariantCulture);
            //DateTime? f = Convert.ToDateTime(pOInvoice.PoDate, CultureInfo.InvariantCulture);
            //DateTime? g = Convert.ToDateTime(pOInvoice.InvoiceDate, CultureInfo.InvariantCulture);


            try
            {
                POInvoice inv = db.POInvoices.Where(a => a.DocunentNo == pOInvoice.DocunentNo).FirstOrDefault();
                inv.DocunentNo = pOInvoice.DocunentNo;
                inv.DocumentDate = pOInvoice.DocumentDate;
                inv.InvoiceNo = pOInvoice.InvoiceNo;
                inv.InvoiceDate = pOInvoice.InvoiceDate;
                inv.PoNo = "";
                inv.PoDate = pOInvoice.PoDate;
                inv.BasicAmount = po.BasicAmount;
                inv.DiscAmount = po.DiscAmount;
                inv.TaxAmount = po.Tax;
                inv.TransportAmount = po.TransportAmount;
                inv.TotalAmount = pOInvoice.TotalAmount;
                inv.SupplierId = pOInvoice.SupplierId;
                //inv.PayAmount = 0;
                inv.CompanyID = Convert.ToInt32(Session["CompanyID"]);
                inv.BalanceAmount = pOInvoice.TotalAmount - pOInvoice.PayAmount;
                inv.ReferenceNo = pOInvoice.ReferenceNo;
                inv.Reason = pOInvoice.Reason;
                //inv.PaymentDate = d; // Assign the parsedDate value directly to DateTime property
                inv.PaymentMode = pOInvoice.PaymentMode;

                db.Entry(inv).State = EntityState.Modified;
                db.SaveChanges();
                status = true;
                return new JsonResult { Data = new { status } };
            }
            catch (Exception Ex)
            {
                var msg = Ex.Message.ToString();
                TempData["Msg"] = msg;
                return new JsonResult { Data = new { status } };
            }
        }
        public JsonResult getInvoiceNo()
        {
            try
            {
                var Tcount = db.POInvoices.Count();
                Tcount = Tcount + 1;
                string result = "Doc_" + Tcount;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ee)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult ShowTotalAmount(string PoNo)
        {
            var TotalAmount = db.poDetails.Where(a => a.PONO == PoNo).Sum(a => a.TotalAmount);
            var payAmount = db.POMains.Where(a => a.PurchaseOrderNo == PoNo).FirstOrDefault();
            var result = TotalAmount - payAmount.PayAmount;
            decimal? gst = 0;
            if (payAmount.IGST == 0)
            {
                gst = payAmount.SGST + payAmount.CGST;
            }
            else
            {
                gst = payAmount.IGST;
            }


            var list = new List<object>();
            list.Add(result);
            list.Add(payAmount.NetAmount);
            list.Add(payAmount.Discount);
            list.Add(gst);
            list.Add(payAmount.TotalAmount);

            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveInvoiceDetails(POInvoice pOInvoice)
        {
            var status = false;
            var po = db.GRNDetail.FirstOrDefault(a => a.InvoiceNo == pOInvoice.InvoiceNo);
            if (po == null)
            {
                return new JsonResult { Data = new { status } };
            }



            try
            {
                POInvoice inv = new POInvoice();
                inv.DocunentNo = pOInvoice.DocunentNo;
                inv.DocumentDate = pOInvoice.DocumentDate;
                inv.InvoiceNo = pOInvoice.InvoiceNo;
                inv.InvoiceDate = pOInvoice.InvoiceDate;
                inv.PoNo = "";
                inv.PoDate = pOInvoice.PoDate;
                inv.BasicAmount = po.BasicAmount;
                inv.DiscAmount = po.DiscAmount;
                inv.TaxAmount = po.TaxAmount;
                inv.TransportAmount = po.TransportAmount;
                inv.TotalAmount = pOInvoice.TotalAmount;
                inv.SupplierId = pOInvoice.SupplierId;
                inv.PayAmount = 0;
                inv.CompanyID = Convert.ToInt32(Session["CompanyID"]);
                inv.BalanceAmount = pOInvoice.TotalAmount - pOInvoice.PayAmount;
                inv.ReferenceNo = pOInvoice.ReferenceNo;
                //inv.PaymentDate = d; // Assign the parsedDate value directly to DateTime property
                inv.PaymentMode = pOInvoice.PaymentMode;
                inv.Reason = pOInvoice.Reason;

                db.POInvoices.Add(inv);
                db.SaveChanges();
                status = true;
                return new JsonResult { Data = new { status } };
            }
            catch (Exception ee)
            {
                status = false;
                return new JsonResult { Data = new { status } };
            }
        }
        public JsonResult getInvoice(int SupplierID)
        {
            var invoiceList = db.GRNDetail.Where(x => x.SupplierID == SupplierID && x.InvoiceNo != null)
                                           .Select(a => a.InvoiceNo)
                                           .Distinct()
                                           .ToList();

            if (invoiceList.Count == 0)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }

            List<string> FilterInvoiceList = new List<string>();
            foreach (var invoice in invoiceList)
            {
                var filteredInvoices = db.POInvoices.Where(a => a.InvoiceNo == invoice).FirstOrDefault();
                if (filteredInvoices == null)
                {
                    FilterInvoiceList.Add(invoice);
                }

            }

            return Json(FilterInvoiceList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CheckValidSupplier(string SupplierName)
        {
            var result = db.suppliers.Where(a => a.IsActive == true && a.SupplierName == SupplierName).Count();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult bindSupplier(string Id)
        {
            var CatId = db.POInvoices.Where(a => a.DocunentNo.Equals(Id)).Select(x => x.SupplierId).Single().ToString();
            if (CatId == null) { return Json("", JsonRequestBehavior.AllowGet); }
            int CategoryId;
            if (int.TryParse(CatId, out CategoryId))
            {
                CategoryId = Convert.ToInt32(CatId);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }

            var result = db.suppliers.Where(a => a.SupplierID.Equals(CategoryId)).Select(x => x.SupplierName).Single().ToString();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult checkSupplier(string supplier)
        {
            var result = 0;
            if (supplier != "")
            {
                result = db.suppliers.Where(x => x.SupplierName.Equals(supplier)).Count();
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult bindPONO(string Id)
        {
            var CatId = db.POInvoices.Where(a => a.DocunentNo.Equals(Id)).Select(x => x.PoNo).Single().ToString();
            if (CatId == null) { return Json("", JsonRequestBehavior.AllowGet); }

            var result = CatId;
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult checkPONO(string supplier)
        {
            var result = 0;
            if (supplier != "")
            {
                result = db.POInvoices.Where(x => x.PoNo.Equals(supplier)).Count();
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult getInvoiceDetails(string Id)
        {
            if (Id == null || Id == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = db.POInvoices.Where(a => a.DocunentNo == Id).FirstOrDefault();

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult ShowAmount(string InvoiceNo)
        {
            GRNDetails result = null;
            if (!string.IsNullOrEmpty(InvoiceNo))
            {
                result = db.GRNDetail.Where(x => x.InvoiceNo == InvoiceNo).FirstOrDefault();
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        

        public JsonResult ShowDetails(string InvoiceNo)

        {


            //string invoiceNo = "A000001";

            var result = (from gd in db.GRNDetail
                         where gd.InvoiceNo == InvoiceNo
                         group gd by gd.GRNNo into g
                         select new
                         {
                             GRNNo = g.Key,
                             TotalAmount = g.Sum(x => x.AmountPerItem * x.ReceivedQty)
                         }).ToList();
            //List<GRNDetails> result = new List<GRNDetails>();

            //var GRN = db.GRNDetail.Where(a => a.InvoiceNo == InvoiceNo).Select(a => a.GRNNo).Distinct().ToList();




            //foreach (var GRNNO in GRN)
            //{
            //    var totalQty = 0m;
            //    var grn = db.GRNDetail.Where(a => a.GRNNo == GRNNO).ToList();
            //    foreach (var grnNo in grn)
            //    {
            //        var qty = (grnNo.AmountPerItem * grnNo.ReceivedQty) ?? 0m;
            //        totalQty += qty;
            //    }

            //    GRNDetails grnDetails = new GRNDetails
            //    {
            //        GRNNo = GRNNO,
            //        TotalAmount = totalQty
            //    };

            //    result.Add(grnDetails);

            //}

            return Json(result, JsonRequestBehavior.AllowGet);
        }


    }
}