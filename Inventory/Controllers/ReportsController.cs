using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class ReportsController : Controller
    {
        //public ActionResult Index()
        //{
        //    return View();
        //}
        public ActionResult rptSupplierWiseStock()
        {
            ViewBag.Supplier = "1";
            return View();
        }


        public ActionResult rptCustomerwiseSales()
        {
            return View();
        }


        public ActionResult rptDebitNote()
        {
            return View();
        }
        public ActionResult rptCreditNote() 
        {
            return View();
        }

        public ActionResult rptPendingPRNApproval() 
        {
            return View();
        }       


        public ActionResult rptDeliveryChallan()
        {
            return View();
        }
        public ActionResult rptExpiryReport()
        {
            return View();
        }
        public ActionResult rptInvoice()
        {
            ViewBag.InvNo = "Inv_1";
            return View();
        }
        public ActionResult rptItemLedger()
        {
            return View();
        }
        public ActionResult rptOverStockLevel()
        {
            return View();
        }
        public ActionResult rptPendingPO()
        {
            return View();
        }
        public ActionResult rptPurchaseOrder()
        {
            return View();
        }
        public ActionResult rptReorderStocklevel()
        {
            return View();
        }

        public ActionResult rptStockSummary()
        {
            return View();
        }
    }
}