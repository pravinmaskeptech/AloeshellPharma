using Inventory.Models;
using Syncfusion.Windows.Shared;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class PurchaseReportController : Controller
    {
        private InventoryModel db = new InventoryModel();

        // GET: PurchaseReport

        public ActionResult Index()
        {

            return View();
        }
        [HttpPost]
        public string GetData(string sEcho, int iDisplayLength, int iDisplayStart, string sSearch)
        {

            //          var  result = (
            //  from o in db.POMains
            //  join s in db.GRNDetail on o.PurchaseOrderNo equals s.PONo into salesData
            //  from sl in salesData.DefaultIfEmpty()
            //  where o.IsActive == true
            //  group new { sl.PONo, sl.PODate, o.Supplier, o.NetAmount, o.Discount, o.IGST, o.SGST, o.CGST, o.TotalAmount, o.SupplierID }
            //  by new { o.PurchaseOrderNo, o.PurchaseOrderDate, o.Discount, o.NetAmount,o.IGST, o.SGST, o.CGST, o.TotalAmount, o.Supplier } into groupedData
            //  orderby groupedData.Key.PurchaseOrderNo ascending
            //  select new
            //  {
            //      groupedData.Key.PurchaseOrderNo,

            //      groupedData.Key.PurchaseOrderDate,
            //      groupedData.Key.Supplier,
            //      groupedData.Key.NetAmount,
            //      groupedData.Key.Discount,

            //      groupedData.Key.IGST,
            //      groupedData.Key.SGST,
            //      groupedData.Key.CGST,
            //      groupedData.Key.TotalAmount,

            //  }
            //).ToList();
            var result = (
    from o in db.POMains
    join s in db.GRNDetail on o.PurchaseOrderNo equals s.PONo into salesData
    from sl in salesData.DefaultIfEmpty()
    join sm in db.suppliers on o.SupplierID equals sm.SupplierID

    group new { sm.SupplierName,sl.PONo, sl.PODate, o.NetAmount, o.Discount, o.IGST, o.SGST, o.CGST, o.TotalAmount }
    by new {sm.SupplierName, o.PurchaseOrderNo, o.PurchaseOrderDate, o.Discount, o.NetAmount, o.IGST, o.SGST, o.CGST, o.TotalAmount } into groupedData

    orderby groupedData.Key.PurchaseOrderNo ascending
    select new
    {
        groupedData.Key.PurchaseOrderNo,
        groupedData.Key.PurchaseOrderDate,
        groupedData.Key.SupplierName,
        groupedData.Key.NetAmount,
        groupedData.Key.Discount,
        groupedData.Key.IGST,
        groupedData.Key.SGST,
        groupedData.Key.CGST,
        groupedData.Key.TotalAmount
    }
).ToList();



            var FromDate = Session["FromDate"];
            var ToDate = Session["ToDate"];
            if (FromDate == "" || ToDate == "" || ToDate == null || FromDate == null)
            {
                Session["FromDate"] = "";
                Session["ToDate"] = "";
                result = (
   from o in db.POMains
   join s in db.GRNDetail on o.PurchaseOrderNo equals s.PONo into salesData
   from sl in salesData.DefaultIfEmpty()
   join sm in db.suppliers on o.SupplierID equals sm.SupplierID

   group new { sm.SupplierName, sl.PONo, sl.PODate, o.NetAmount, o.Discount, o.IGST, o.SGST, o.CGST, o.TotalAmount }
   by new { sm.SupplierName, o.PurchaseOrderNo, o.PurchaseOrderDate, o.Discount, o.NetAmount, o.IGST, o.SGST, o.CGST, o.TotalAmount } into groupedData

   orderby groupedData.Key.PurchaseOrderNo ascending
   select new
   {
       groupedData.Key.PurchaseOrderNo,
       groupedData.Key.PurchaseOrderDate,
       groupedData.Key.SupplierName,

       //groupedData.Key.Supplier,
       groupedData.Key.NetAmount,
       groupedData.Key.Discount,
       groupedData.Key.IGST,
       groupedData.Key.SGST,
       groupedData.Key.CGST,
       groupedData.Key.TotalAmount
   }
).ToList();
                //                 result = (
                // from o in db.POMains
                // join s in db.GRNDetail on o.PurchaseOrderNo equals s.PONo into salesData
                // from sl in salesData.DefaultIfEmpty()
                // where o.IsActive == true
                // group new { sl.PONo, sl.PODate, o.Supplier, o.NetAmount, o.Discount, o.IGST, o.SGST, o.CGST, o.TotalAmount, o.SupplierID }
                // by new { o.PurchaseOrderNo, o.PurchaseOrderDate, o.Discount, o.NetAmount, o.IGST, o.SGST, o.CGST, o.TotalAmount, o.Supplier } into groupedData
                // orderby groupedData.Key.PurchaseOrderNo ascending
                // select new
                // {
                //     groupedData.Key.PurchaseOrderNo,

                //     groupedData.Key.PurchaseOrderDate,
                //     groupedData.Key.Supplier,
                //     groupedData.Key.NetAmount,
                //     groupedData.Key.Discount,

                //     groupedData.Key.IGST,
                //     groupedData.Key.SGST,
                //     groupedData.Key.CGST,
                //     groupedData.Key.TotalAmount,

                // }
                //).ToList();
               

            }
            else
            {
                DateTime fromDateValue = new DateTime();
                DateTime toDateValue = new DateTime();
                try
                { fromDateValue = DateTime.ParseExact(FromDate.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture); }
                catch { }

                try
                { toDateValue = DateTime.ParseExact(ToDate.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture); }
                catch { }
                result = (
   from o in db.POMains.Where(a => a.PurchaseOrderDate >= fromDateValue && a.PurchaseOrderDate <= toDateValue)
   join s in db.GRNDetail on o.PurchaseOrderNo equals s.PONo into salesData
   from sl in salesData.DefaultIfEmpty()
   join sm in db.suppliers on o.SupplierID equals sm.SupplierID

   group new { sm.SupplierName,sl.PONo, sl.PODate, o.NetAmount, o.Discount, o.IGST, o.SGST, o.CGST, o.TotalAmount }
   by new { sm.SupplierName,o.PurchaseOrderNo, o.PurchaseOrderDate, o.Discount, o.NetAmount, o.IGST, o.SGST, o.CGST, o.TotalAmount } into groupedData

   orderby groupedData.Key.PurchaseOrderNo ascending
   select new
   {
       groupedData.Key.PurchaseOrderNo,
       groupedData.Key.PurchaseOrderDate,
       groupedData.Key.SupplierName,
       groupedData.Key.NetAmount,
       groupedData.Key.Discount,
       groupedData.Key.IGST,
       groupedData.Key.SGST,
       groupedData.Key.CGST,
       groupedData.Key.TotalAmount
   }
).ToList();
               

            }

            int totalRecords = result.Count();


            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"sEcho\":");
            sb.Append(sEcho);
            sb.Append(",");
            sb.Append("\"iTotalRecords\":");
            sb.Append(totalRecords);
            sb.Append(",");
            sb.Append("\"iTotalDisplayRecords\":");
            sb.Append(totalRecords);
            sb.Append(",");
            sb.Append("\"aaData\":");
            sb.Append(Newtonsoft.Json.JsonConvert.SerializeObject(result));
            sb.Append("}");
            return sb.ToString();
        }


        [HttpGet]



        public JsonResult SearchData(string fromDate, string toDate)
        {
            Session["FromDate"] = fromDate;
            Session["ToDate"] = toDate;


            

            var result = new { Message = "success" };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}