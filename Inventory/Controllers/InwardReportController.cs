using Inventory.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class InwardReportController : Controller
    {
        private InventoryModel db = new InventoryModel();

        // GET: InwardReport
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public string GetData(string sEcho, int iDisplayLength, int iDisplayStart, string sSearch)
        {
   //         var result = (
   //    from o in db.POMains
   //    join s in db.GRNDetail on o.PurchaseOrderNo equals s.PONo into salesData
   //    from sl in salesData.DefaultIfEmpty()
   //    join sm in db.suppliers on o.SupplierID equals sm.SupplierID
   //    join pd in db.poDetails on o.PurchaseOrderNo equals pd.PONO into orderDetailsData
   //    from od in orderDetailsData.DefaultIfEmpty()
   //    where !string.IsNullOrEmpty(sl.GRNNo) // Filter out empty GRN numbers
   //    group new { sm.SupplierName, sl.GRNNo, sl.GRNDate, od.OrderQty, sl.ReceivedQty }
   //    by sl.GRNNo into groupedData // Group by GRNNo
   //    orderby groupedData.Key ascending
   //    select new
   //    {
   //        GRNNo = groupedData.Key,
   //        PoNo = groupedData.Key,

   //        SupplierName = groupedData.FirstOrDefault().SupplierName,
   //        GRNDate = groupedData.FirstOrDefault().GRNDate,
   //        OrderQty = groupedData.Sum(x => x.OrderQty),
   //        ReceivedQty = groupedData.Sum(x => x.ReceivedQty),
   //        NetQty = groupedData.Sum(x => x.OrderQty) - groupedData.Sum(x => x.ReceivedQty)
   //    }
   //).ToList();
            var result = (
                from o in db.POMains
                join s in db.GRNDetail on o.PurchaseOrderNo equals s.PONo into salesData
                from sl in salesData.DefaultIfEmpty()
                join sm in db.suppliers on o.SupplierID equals sm.SupplierID
                join pd in db.poDetails on o.PurchaseOrderNo equals pd.PONO into orderDetailsData
                from od in orderDetailsData.DefaultIfEmpty()
                where !string.IsNullOrEmpty(sl.GRNNo) 
                group new { sm.SupplierName, sl.GRNNo, sl.GRNDate, od.OrderQty, sl.ReceivedQty, od.PONO }
                by od.PONO into groupedData 
                orderby groupedData.Key ascending
                select new
                {
                    PoNo = groupedData.Key,
                    SupplierName = groupedData.FirstOrDefault().SupplierName,
                    GRNNo = groupedData.FirstOrDefault().GRNNo,
                    GRNDate = groupedData.FirstOrDefault().GRNDate,
                    OrderQty = groupedData.Sum(x => x.OrderQty),
                    ReceivedQty = groupedData.Sum(x => x.ReceivedQty),
                    NetQty = groupedData.Sum(x => x.OrderQty) - groupedData.Sum(x => x.ReceivedQty)
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
                 join pd in db.poDetails on o.PurchaseOrderNo equals pd.PONO into orderDetailsData
                 from od in orderDetailsData.DefaultIfEmpty()
                 where !string.IsNullOrEmpty(sl.GRNNo) 
                 group new { sm.SupplierName, sl.GRNNo, sl.GRNDate, od.OrderQty, sl.ReceivedQty, od.PONO }
                 by od.PONO into groupedData 
                 orderby groupedData.Key ascending
                 select new
                 {
                     PoNo = groupedData.Key,
                     SupplierName = groupedData.FirstOrDefault().SupplierName,
                     GRNNo = groupedData.FirstOrDefault().GRNNo,
                     GRNDate = groupedData.FirstOrDefault().GRNDate,
                     OrderQty = groupedData.Sum(x => x.OrderQty),
                     ReceivedQty = groupedData.Sum(x => x.ReceivedQty),
                     NetQty = groupedData.Sum(x => x.OrderQty) - groupedData.Sum(x => x.ReceivedQty)
                 }
             ).ToList();

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
                join pd in db.poDetails on o.PurchaseOrderNo equals pd.PONO into orderDetailsData
                from od in orderDetailsData.DefaultIfEmpty()
                where !string.IsNullOrEmpty(sl.GRNNo) 
                group new { sm.SupplierName, sl.GRNNo, sl.GRNDate, od.OrderQty, sl.ReceivedQty, od.PONO }
                by od.PONO into groupedData 
                orderby groupedData.Key ascending
                select new
                {
                    PoNo = groupedData.Key,
                    SupplierName = groupedData.FirstOrDefault().SupplierName,
                    GRNNo = groupedData.FirstOrDefault().GRNNo,
                    GRNDate = groupedData.FirstOrDefault().GRNDate,
                    OrderQty = groupedData.Sum(x => x.OrderQty),
                    ReceivedQty = groupedData.Sum(x => x.ReceivedQty),
                    NetQty = groupedData.Sum(x => x.OrderQty) - groupedData.Sum(x => x.ReceivedQty)
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