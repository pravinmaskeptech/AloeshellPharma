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


            //            var result = (
            //    from o in db.POMains
            //    join s in db.GRNDetail on o.PurchaseOrderNo equals s.PONo into salesData
            //    from sl in salesData.DefaultIfEmpty()
            //    join sm in db.suppliers on o.SupplierID equals sm.SupplierID

            //    group new { sm.SupplierName,sl.PONo, sl.PODate, o.NetAmount, o.Discount, o.IGST, o.SGST, o.CGST, o.TotalAmount }
            //    by new {sm.SupplierName, o.PurchaseOrderNo, o.PurchaseOrderDate, o.Discount, o.NetAmount, o.IGST, o.SGST, o.CGST, o.TotalAmount } into groupedData

            //    orderby groupedData.Key.PurchaseOrderNo ascending
            //    select new
            //    {
            //        groupedData.Key.PurchaseOrderNo,
            //        groupedData.Key.PurchaseOrderDate,
            //        groupedData.Key.SupplierName,
            //        groupedData.Key.NetAmount,
            //        groupedData.Key.Discount,
            //        groupedData.Key.IGST,
            //        groupedData.Key.SGST,
            //        groupedData.Key.CGST,
            //        groupedData.Key.TotalAmount
            //    }
            //).ToList();
            var result = (
    from o in db.POMains
    join s in db.GRNDetail on o.PurchaseOrderNo equals s.PONo into salesData
    from sl in salesData.DefaultIfEmpty()
    join sm in db.suppliers on o.SupplierID equals sm.SupplierID
    group new { sm.SupplierName, sl.PONo, sl.PODate, o.NetAmount, o.Discount, o.IGST, o.SGST, o.CGST, o.TotalAmount, sl.ReturnQty, sl.AmountPerItem }
by new { sm.SupplierName, o.PurchaseOrderNo, o.PurchaseOrderDate, sm.GSTNo, sm.BillingCity }
into groupedData
    orderby groupedData.Key.PurchaseOrderNo ascending
    select new
    {
        groupedData.Key.PurchaseOrderNo,
        groupedData.Key.PurchaseOrderDate,
        groupedData.Key.SupplierName,
        groupedData.Key.GSTNo,
        groupedData.Key.BillingCity,
        NetAmount = groupedData.Sum(x => x.NetAmount),
        Discount = groupedData.Sum(x => x.Discount),
        IGST = groupedData.Sum(x => x.IGST),
        SGST = groupedData.Sum(x => x.SGST),
        CGST = groupedData.Sum(x => x.CGST),
        TotalAmount = groupedData.Sum(x => x.TotalAmount),
        ReturnQty = groupedData.Sum(x => x.ReturnQty),
        Amount = groupedData.Any(x => x.ReturnQty != null && x.ReturnQty != 0)
            ? groupedData.Sum(x => x.TotalAmount - (x.ReturnQty * x.AmountPerItem))
            : 0,
        ReturnAmount = groupedData.Any(x => x.ReturnQty != null && x.ReturnQty != 0)
            ? groupedData.Sum(x => x.ReturnQty * x.AmountPerItem)
            : 0
    }
).ToList();

   //         var result = (
   //           from o in db.POMains
   //           join s in db.GRNDetail on o.PurchaseOrderNo equals s.PONo into salesData
   //           from sl in salesData.DefaultIfEmpty()
   //           join sm in db.suppliers on o.SupplierID equals sm.SupplierID

   //           group new { sm.SupplierName, sl.PONo, sl.PODate, o.NetAmount, o.Discount, o.IGST, o.SGST, o.CGST, o.TotalAmount, sl.ReturnQty, sl.AmountPerItem }
   //by new { sm.SupplierName, o.PurchaseOrderNo, o.PurchaseOrderDate, o.Discount, o.NetAmount, o.IGST, o.SGST, o.CGST, o.TotalAmount, sm.GSTNo, sm.BillingCity, sl.ReturnQty, sl.AmountPerItem } into groupedData

   //           orderby groupedData.Key.PurchaseOrderNo ascending
   //           select new
   //           {
   //               groupedData.Key.PurchaseOrderNo,
   //               groupedData.Key.PurchaseOrderDate,
   //               groupedData.Key.SupplierName,
   //               groupedData.Key.GSTNo,
   //               groupedData.Key.BillingCity,

   //               groupedData.Key.NetAmount,
   //               groupedData.Key.Discount,
   //               groupedData.Key.IGST,
   //               groupedData.Key.SGST,
   //               groupedData.Key.CGST,
   //               groupedData.Key.TotalAmount,
   //               groupedData.Key.ReturnQty,
   //               Amount = groupedData.Any(x => x.ReturnQty != null && x.ReturnQty != 0)
   //         ? groupedData.Sum(x => x.TotalAmount) - groupedData.Sum(x => x.ReturnQty * x.AmountPerItem)
   //         : 0,
   //               ReturnAmount = groupedData.Any(x => x.ReturnQty != null && x.ReturnQty != 0)
   //         ? groupedData.Sum(x => x.ReturnQty * x.AmountPerItem)
   //         : 0
   //               //Amount = groupedData.Sum(x => x.ReturnQty * x.AmountPerItem),
   //               //ReturnAmount = groupedData.Sum(x => x.ReturnQty * x.AmountPerItem)
   //           }
   //        ).ToList();
//            var result = (
//    from o in db.POMains
//    join s in db.GRNDetail on o.PurchaseOrderNo equals s.PONo into salesData
//    from sl in salesData.DefaultIfEmpty()
//    join sm in db.suppliers on o.SupplierID equals sm.SupplierID

//    group new { sm.SupplierName, sl.PONo, sl.PODate, o.NetAmount, o.Discount, o.IGST, o.SGST, o.CGST, o.TotalAmount }
//    by new { sm.SupplierName, o.PurchaseOrderNo, o.PurchaseOrderDate, o.Discount, o.NetAmount, o.IGST, o.SGST, o.CGST, o.TotalAmount, sm.GSTNo, sm.BillingCity,sl.ReturnQty } into groupedData

//    orderby groupedData.Key.PurchaseOrderNo ascending
//    select new
//    {
//        groupedData.Key.PurchaseOrderNo,
//        groupedData.Key.PurchaseOrderDate,
//        groupedData.Key.SupplierName,
//         groupedData.Key.GSTNo,
//        groupedData.Key.BillingCity,
//        groupedData.Key.NetAmount,
//        groupedData.Key.Discount,
//        groupedData.Key.IGST,
//        groupedData.Key.SGST,
//        groupedData.Key.CGST,
//        groupedData.Key.TotalAmount,
//        groupedData.Key.ReturnQty
//    }
//).ToList();


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
    group new { sm.SupplierName, sl.PONo, sl.PODate, o.NetAmount, o.Discount, o.IGST, o.SGST, o.CGST, o.TotalAmount, sl.ReturnQty, sl.AmountPerItem }
by new { sm.SupplierName, o.PurchaseOrderNo, o.PurchaseOrderDate,  sm.GSTNo, sm.BillingCity }
into groupedData
    orderby groupedData.Key.PurchaseOrderNo ascending
    select new
    {
        groupedData.Key.PurchaseOrderNo,
        groupedData.Key.PurchaseOrderDate,
        groupedData.Key.SupplierName,
        groupedData.Key.GSTNo,
        groupedData.Key.BillingCity,
        NetAmount = groupedData.Sum(x => x.NetAmount),
        Discount = groupedData.Sum(x => x.Discount),
        IGST = groupedData.Sum(x => x.IGST),
        SGST = groupedData.Sum(x => x.SGST),
        CGST = groupedData.Sum(x => x.CGST),
        TotalAmount = groupedData.Sum(x => x.TotalAmount),
        ReturnQty = groupedData.Sum(x => x.ReturnQty),
        Amount = groupedData.Any(x => x.ReturnQty != null && x.ReturnQty != 0)
            ? groupedData.Sum(x => x.TotalAmount - (x.ReturnQty * x.AmountPerItem))
            : 0,
        ReturnAmount = groupedData.Any(x => x.ReturnQty != null && x.ReturnQty != 0)
            ? groupedData.Sum(x => x.ReturnQty * x.AmountPerItem)
            : 0
    }
).ToList();

//                result = (
//  from o in db.POMains
//  join s in db.GRNDetail on o.PurchaseOrderNo equals s.PONo into salesData
//  from sl in salesData.DefaultIfEmpty()
//  join sm in db.suppliers on o.SupplierID equals sm.SupplierID

//  group new { sm.SupplierName, sl.PONo, sl.PODate, o.NetAmount, o.Discount, o.IGST, o.SGST, o.CGST, o.TotalAmount, sl.ReturnQty, sl.AmountPerItem }
//   by new { sm.SupplierName, o.PurchaseOrderNo, o.PurchaseOrderDate, o.Discount, o.NetAmount, o.IGST, o.SGST, o.CGST, o.TotalAmount, sm.GSTNo, sm.BillingCity, sl.ReturnQty, sl.AmountPerItem } into groupedData

//  orderby groupedData.Key.PurchaseOrderNo ascending
//  select new
//  {
//      groupedData.Key.PurchaseOrderNo,
//      groupedData.Key.PurchaseOrderDate,
//      groupedData.Key.SupplierName,
//      groupedData.Key.GSTNo,
//      groupedData.Key.BillingCity,

//      groupedData.Key.NetAmount,
//      groupedData.Key.Discount,
//      groupedData.Key.IGST,
//      groupedData.Key.SGST,
//      groupedData.Key.CGST,
//      groupedData.Key.TotalAmount,
//      groupedData.Key.ReturnQty,
//      Amount = groupedData.Any(x => x.ReturnQty != null && x.ReturnQty != 0)
//            ? groupedData.Sum(x => x.TotalAmount) - groupedData.Sum(x => x.ReturnQty * x.AmountPerItem)
//            : 0,
//      ReturnAmount = groupedData.Any(x => x.ReturnQty != null && x.ReturnQty != 0)
//            ? groupedData.Sum(x => x.ReturnQty * x.AmountPerItem)
//            : 0
//      //Amount = groupedData.Sum(x => x.ReturnQty * x.AmountPerItem),
//      //ReturnAmount = groupedData.Sum(x => x.ReturnQty * x.AmountPerItem)
//  }
//).ToList();
                //                result = (
//   from o in db.POMains
//   join s in db.GRNDetail on o.PurchaseOrderNo equals s.PONo into salesData
//   from sl in salesData.DefaultIfEmpty()
//   join sm in db.suppliers on o.SupplierID equals sm.SupplierID

//   group new { sm.SupplierName, sl.PONo, sl.PODate, o.NetAmount, o.Discount, o.IGST, o.SGST, o.CGST, o.TotalAmount }
//   by new { sm.SupplierName, o.PurchaseOrderNo, o.PurchaseOrderDate, o.Discount, o.NetAmount, o.IGST, o.SGST, o.CGST, o.TotalAmount } into groupedData

//   orderby groupedData.Key.PurchaseOrderNo ascending
//   select new
//   {
//       groupedData.Key.PurchaseOrderNo,
//       groupedData.Key.PurchaseOrderDate,
//       groupedData.Key.SupplierName,

//       //groupedData.Key.Supplier,
//       groupedData.Key.NetAmount,
//       groupedData.Key.Discount,
//       groupedData.Key.IGST,
//       groupedData.Key.SGST,
//       groupedData.Key.CGST,
//       groupedData.Key.TotalAmount
//   }
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
    from o in db.POMains
    join s in db.GRNDetail on o.PurchaseOrderNo equals s.PONo into salesData
    from sl in salesData.DefaultIfEmpty()
    join sm in db.suppliers on o.SupplierID equals sm.SupplierID
    where o.PurchaseOrderDate >= fromDateValue && o.PurchaseOrderDate <= toDateValue
    group new { sm.SupplierName, sl.PONo, sl.PODate, o.NetAmount, o.Discount, o.IGST, o.SGST, o.CGST, o.TotalAmount, sl.ReturnQty, sl.AmountPerItem }
by new { sm.SupplierName, o.PurchaseOrderNo, o.PurchaseOrderDate, sm.GSTNo, sm.BillingCity }
into groupedData
    orderby groupedData.Key.PurchaseOrderNo ascending
    select new
    {
        groupedData.Key.PurchaseOrderNo,
        groupedData.Key.PurchaseOrderDate,
        groupedData.Key.SupplierName,
        groupedData.Key.GSTNo,
        groupedData.Key.BillingCity,
        NetAmount = groupedData.Sum(x => x.NetAmount),
        Discount = groupedData.Sum(x => x.Discount),
        IGST = groupedData.Sum(x => x.IGST),
        SGST = groupedData.Sum(x => x.SGST),
        CGST = groupedData.Sum(x => x.CGST),
        TotalAmount = groupedData.Sum(x => x.TotalAmount),
        ReturnQty = groupedData.Sum(x => x.ReturnQty),
        Amount = groupedData.Any(x => x.ReturnQty != null && x.ReturnQty != 0)
            ? groupedData.Sum(x => x.TotalAmount - (x.ReturnQty * x.AmountPerItem))
            : 0,
        ReturnAmount = groupedData.Any(x => x.ReturnQty != null && x.ReturnQty != 0)
            ? groupedData.Sum(x => x.ReturnQty * x.AmountPerItem)
            : 0
    }
).ToList();

//                result = (
//   from o in db.POMains.Where(a => a.PurchaseOrderDate >= fromDateValue && a.PurchaseOrderDate <= toDateValue)
//   join s in db.GRNDetail on o.PurchaseOrderNo equals s.PONo into salesData
//    from sl in salesData.DefaultIfEmpty()
//    join sm in db.suppliers on o.SupplierID equals sm.SupplierID

//    group new { sm.SupplierName, sl.PONo, sl.PODate, o.NetAmount, o.Discount, o.IGST, o.SGST, o.CGST, o.TotalAmount ,sl.ReturnQty,sl.AmountPerItem}
//    by new { sm.SupplierName, o.PurchaseOrderNo, o.PurchaseOrderDate, o.Discount, o.NetAmount, o.IGST, o.SGST, o.CGST, o.TotalAmount, sm.GSTNo, sm.BillingCity, sl.ReturnQty,sl.AmountPerItem } into groupedData

//    orderby groupedData.Key.PurchaseOrderNo ascending
//    select new
//    {
//        groupedData.Key.PurchaseOrderNo,
//        groupedData.Key.PurchaseOrderDate,
//        groupedData.Key.SupplierName,
//        groupedData.Key.GSTNo,
//        groupedData.Key.BillingCity,
        
//        groupedData.Key.NetAmount,
//        groupedData.Key.Discount,
//        groupedData.Key.IGST,
//        groupedData.Key.SGST,
//        groupedData.Key.CGST,
//        groupedData.Key.TotalAmount,
//        groupedData.Key.ReturnQty,
//        Amount = groupedData.Any(x => x.ReturnQty != null && x.ReturnQty != 0)
//            ? groupedData.Sum(x => x.TotalAmount) - groupedData.Sum(x => x.ReturnQty * x.AmountPerItem)
//            : 0,
//        ReturnAmount = groupedData.Any(x => x.ReturnQty != null && x.ReturnQty != 0)
//            ? groupedData.Sum(x => x.ReturnQty * x.AmountPerItem)
//            : 0
//        // Amount = groupedData.Sum(x => x.ReturnQty * x.AmountPerItem),
//        //ReturnAmount = groupedData.Sum(x => x.ReturnQty * x.AmountPerItem)
//    }
//).ToList();
//                result = (
//   from o in db.POMains.Where(a => a.PurchaseOrderDate >= fromDateValue && a.PurchaseOrderDate <= toDateValue)
//   join s in db.GRNDetail on o.PurchaseOrderNo equals s.PONo into salesData
//   from sl in salesData.DefaultIfEmpty()
//   join sm in db.suppliers on o.SupplierID equals sm.SupplierID

//   group new { sm.SupplierName,sl.PONo, sl.PODate, o.NetAmount, o.Discount, o.IGST, o.SGST, o.CGST, o.TotalAmount }
//   by new { sm.SupplierName,o.PurchaseOrderNo, o.PurchaseOrderDate, o.Discount, o.NetAmount, o.IGST, o.SGST, o.CGST, o.TotalAmount } into groupedData

//   orderby groupedData.Key.PurchaseOrderNo ascending
//   select new
//   {
//       groupedData.Key.PurchaseOrderNo,
//       groupedData.Key.PurchaseOrderDate,
//       groupedData.Key.SupplierName,
//       groupedData.Key.NetAmount,
//       groupedData.Key.Discount,
//       groupedData.Key.IGST,
//       groupedData.Key.SGST,
//       groupedData.Key.CGST,
//       groupedData.Key.TotalAmount
//   }
//).ToList();
               

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