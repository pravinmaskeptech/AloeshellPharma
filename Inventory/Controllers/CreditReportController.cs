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
    public class CreditReportController : Controller
    {
        private InventoryModel db = new InventoryModel();

        // GET: CreditReport
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public string GetData(string sEcho, int iDisplayLength, int iDisplayStart, string sSearch)
        {
            DateTime currentDate = DateTime.Now;

            //var result = (from psn in db.ProductSerialNo
            //              join cn in db.CreditNote on psn.InvoiceNo equals cn.InvoiceNo
            //              join prd in db.Products on psn.ProductCode equals prd.ProductCode
            //              where psn.Status == "SO Return"
            //              group new { psn, cn, prd } by new
            //              {
            //                  cn.DocNo,
            //                  CreditNoteDate = cn.DocDate,
            //                  psn.ProductCode,
            //                  prd.ProductName,
            //                  cn.InvoiceNo,



            //                  cn.CustomerName,
            //                  cn.Amount
            //              } into g
            //              orderby g.Key.DocNo
            //              select new
            //              {
            //                  CreditNoteNo = g.Key.DocNo,
            //                  CreditNoteDate = g.Key.CreditNoteDate,
            //                  g.Key.ProductCode,
            //                  g.Key.ProductName,
            //                  g.Key.InvoiceNo,
            //                  Qty = g.Count(),
            //                  g.Key.CustomerName,
            //                   g.Key.Amount

            //              }).ToList();


            var result = (from psn in db.ProductSerialNo
                          join cn in db.CreditNote on psn.InvoiceNo equals cn.InvoiceNo
                          join prd in db.Products on psn.ProductCode equals prd.ProductCode
                          join st in db.Sales on psn.InvoiceNo equals st.InvoiceNo // Join with SalesTable
                          where psn.Status == "SO Return"
                          group new { psn, cn, prd, st } by new
                          {
                              cn.DocNo,
                              CreditNoteDate = cn.DocDate,
                              psn.ProductCode,
                              prd.ProductName,
                              cn.InvoiceNo,
                              cn.CustomerName,
                              cn.Amount,
                              cn.ID
                          } into g
                          orderby g.Key.ID
                          select new
                          {
                              CreditNoteNo = g.Key.DocNo,
                              CreditNoteDate = g.Key.CreditNoteDate,
                              g.Key.ProductCode,
                              g.Key.ProductName,
                              g.Key.InvoiceNo,
                              Qty = g.Count(),
                              g.Key.CustomerName,
                              g.Key.Amount,
                              InvoiceDate = g.Select(x => x.st.InvoiceDate).FirstOrDefault() // Get the InvoiceDate from SalesTable
                          }).ToList();


            string Product = "";

            var FromDate = Session["FromDate"];
            var ToDate = Session["ToDate"];
            try
            {
                //    Product = Session["Product"].ToString();
                //    Category = Session["Category"].ToString();
                Product = Session["Product"]?.ToString();
            }
            catch
            {

            }

            Session["FromDate"] = "";
            Session["ToDate"] = "";
            Session["Product"] = "";


            if (FromDate == "" || ToDate == "" || ToDate == null || FromDate == null)
            {
                 result = (from psn in db.ProductSerialNo
                              join cn in db.CreditNote on psn.InvoiceNo equals cn.InvoiceNo
                              join prd in db.Products on psn.ProductCode equals prd.ProductCode
                              join st in db.Sales on psn.InvoiceNo equals st.InvoiceNo // Join with SalesTable
                              where psn.Status == "SO Return"
                              group new { psn, cn, prd, st } by new
                              {
                                  cn.DocNo,
                                  CreditNoteDate = cn.DocDate,
                                  psn.ProductCode,
                                  prd.ProductName,
                                  cn.InvoiceNo,
                                  cn.CustomerName,
                                  cn.Amount,
                                  cn.ID
                              } into g
                              orderby g.Key.ID
                           select new
                              {
                                  CreditNoteNo = g.Key.DocNo,
                                  CreditNoteDate = g.Key.CreditNoteDate,
                                  g.Key.ProductCode,
                                  g.Key.ProductName,
                                  g.Key.InvoiceNo,
                                  Qty = g.Count(),
                                  g.Key.CustomerName,
                                  g.Key.Amount,
                                  InvoiceDate = g.Select(x => x.st.InvoiceDate).FirstOrDefault() // Get the InvoiceDate from SalesTable
                              }).ToList();

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

                 result = (from psn in db.ProductSerialNo
                              join cn in db.CreditNote on psn.InvoiceNo equals cn.InvoiceNo
                              join prd in db.Products on psn.ProductCode equals prd.ProductCode
                              join st in db.Sales on psn.InvoiceNo equals st.InvoiceNo // Join with SalesTable
                              where psn.Status == "SO Return"
                              group new { psn, cn, prd, st } by new
                              {
                                  cn.DocNo,
                                  CreditNoteDate = cn.DocDate,
                                  psn.ProductCode,
                                  prd.ProductName,
                                  cn.InvoiceNo,
                                  cn.CustomerName,
                                  cn.Amount,
                                  cn.ID
                              } into g
                              orderby g.Key.ID
                           select new
                              {
                                  CreditNoteNo = g.Key.DocNo,
                                  CreditNoteDate = g.Key.CreditNoteDate,
                                  g.Key.ProductCode,
                                  g.Key.ProductName,
                                  g.Key.InvoiceNo,
                                  Qty = g.Count(),
                                  g.Key.CustomerName,
                                  g.Key.Amount,
                                  InvoiceDate = g.Select(x => x.st.InvoiceDate).FirstOrDefault() // Get the InvoiceDate from SalesTable
                              }).ToList();

            }
            //result = result.Where(b => (b.ProductName == Product || string.IsNullOrEmpty(Product))).ToList();


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

//        public string GetData(string sEcho, int iDisplayLength, int iDisplayStart, string sSearch)
//        {
//            try

//            {
//                var result = (
//    from o in db.orderMain
//    join s in db.Sales on o.OrderNo equals s.OrderNo into salesData
//    from sl in salesData.DefaultIfEmpty()
//    where o.IsCashCustomer == true
//    group new { sl.InvoiceNo, sl.InvoiceDate, o.OrderNo, o.OrderID, o.CurrentStatus, o.DeliverTo, o.NetAmount, o.Discount, o.Freeze, o.IGST, o.SGST, o.CGST, o.TotalAmount, o.CustomerName, o.CustomerGSTNo, o.CustomerCity, sl.ReturnQty, sl.AmountPerUnit }
//    by new { o.OrderNo, o.OrderID }
//    into groupedData
//    orderby groupedData.Key.OrderID ascending
//    select new
//    {
//        groupedData.Key.OrderNo,
//        groupedData.Key.OrderID,
//        CurrentStatus = groupedData.Select(x => x.CurrentStatus).FirstOrDefault(),
//        DeliverTo = groupedData.Select(x => x.DeliverTo).FirstOrDefault(),
//        NetAmount = groupedData.Select(x => x.NetAmount).FirstOrDefault(),
//        Discount = groupedData.Select(x => x.Discount).FirstOrDefault(),
//        Freeze = groupedData.Select(x => x.Freeze).FirstOrDefault(),
//        IGST = groupedData.Select(x => x.IGST).FirstOrDefault(),
//        SGST = groupedData.Select(x => x.SGST).FirstOrDefault(),
//        CGST = groupedData.Select(x => x.CGST).FirstOrDefault(),
//        TotalAmount = groupedData.Select(x => x.TotalAmount).FirstOrDefault(),
//        CustomerName = groupedData.Select(x => x.CustomerName).FirstOrDefault(),
//        CustomerGSTNo = groupedData.Select(x => x.CustomerGSTNo).FirstOrDefault(),
//        CustomerCity = groupedData.Select(x => x.CustomerCity).FirstOrDefault(),
//        InvoiceNo = groupedData.Max(x => x.InvoiceNo),
//        InvoiceDate = groupedData.Max(x => x.InvoiceDate),
//        ReturnQty = groupedData.Sum(x => x.ReturnQty),
//        ReturnAmount = groupedData.Any(x => x.ReturnQty != null && x.ReturnQty != 0)
//            ? groupedData.Sum(x => x.ReturnQty * x.AmountPerUnit)
//            : 0,

//        Amount = groupedData.Any(x => x.ReturnQty != null && x.ReturnQty != 0)
//            ? groupedData.Sum(x => x.TotalAmount) - groupedData.Sum(x => x.ReturnQty * x.AmountPerUnit)
//            : 0,

//        //ReturnAmount = groupedData.Sum(x => x.ReturnQty * x.AmountPerUnit),
//        //Amount = groupedData.Sum(x => x.TotalAmount) - groupedData.Sum(x => x.ReturnQty * x.AmountPerUnit)

//        //Amount = groupedData.Sum(x => x.TotalAmount - (x.ReturnQty * x.AmountPerUnit))
//    }
//).ToList();

                



//                var FromDate = Session["FromDate"];
//                var ToDate = Session["ToDate"];
//                if (FromDate == "" || ToDate == "" || ToDate == null || FromDate == null)
//                {
//                    result = (
//   from o in db.orderMain
//   join s in db.Sales on o.OrderNo equals s.OrderNo into salesData
//   from sl in salesData.DefaultIfEmpty()
//   where o.IsCashCustomer == true
//   group new { sl.InvoiceNo, sl.InvoiceDate, o.OrderNo, o.OrderID, o.CurrentStatus, o.DeliverTo, o.NetAmount, o.Discount, o.Freeze, o.IGST, o.SGST, o.CGST, o.TotalAmount, o.CustomerName, o.CustomerGSTNo, o.CustomerCity, sl.ReturnQty, sl.AmountPerUnit }
//   by new { o.OrderNo, o.OrderID }
//   into groupedData
//   orderby groupedData.Key.OrderID ascending
//   select new
//   {
//       groupedData.Key.OrderNo,
//       groupedData.Key.OrderID,
//       CurrentStatus = groupedData.Select(x => x.CurrentStatus).FirstOrDefault(),
//       DeliverTo = groupedData.Select(x => x.DeliverTo).FirstOrDefault(),
//       NetAmount = groupedData.Select(x => x.NetAmount).FirstOrDefault(),
//       Discount = groupedData.Select(x => x.Discount).FirstOrDefault(),
//       Freeze = groupedData.Select(x => x.Freeze).FirstOrDefault(),
//       IGST = groupedData.Select(x => x.IGST).FirstOrDefault(),
//       SGST = groupedData.Select(x => x.SGST).FirstOrDefault(),
//       CGST = groupedData.Select(x => x.CGST).FirstOrDefault(),
//       TotalAmount = groupedData.Select(x => x.TotalAmount).FirstOrDefault(),
//       CustomerName = groupedData.Select(x => x.CustomerName).FirstOrDefault(),
//       CustomerGSTNo = groupedData.Select(x => x.CustomerGSTNo).FirstOrDefault(),
//       CustomerCity = groupedData.Select(x => x.CustomerCity).FirstOrDefault(),
//       InvoiceNo = groupedData.Max(x => x.InvoiceNo),
//       InvoiceDate = groupedData.Max(x => x.InvoiceDate),
//       ReturnQty = groupedData.Sum(x => x.ReturnQty),
//       ReturnAmount = groupedData.Any(x => x.ReturnQty != null && x.ReturnQty != 0)
//           ? groupedData.Sum(x => x.ReturnQty * x.AmountPerUnit)
//           : 0,
//       //ReturnAmount = groupedData.Sum(x => x.ReturnQty * x.AmountPerUnit),
//       //Amount = groupedData.Sum(x => x.TotalAmount) - groupedData.Sum(x => x.ReturnQty * x.AmountPerUnit)
//       Amount = groupedData.Any(x => x.ReturnQty != null && x.ReturnQty != 0)
//           ? groupedData.Sum(x => x.TotalAmount) - groupedData.Sum(x => x.ReturnQty * x.AmountPerUnit)
//           : 0

//       //ReturnAmount = groupedData.Sum(x => x.TotalAmount - (x.ReturnQty * x.AmountPerUnit))
//   }
//).ToList();

//                }
//                else
//                {
//                    DateTime fromDateValue = new DateTime();
//                    DateTime toDateValue = new DateTime();
//                    try
//                    { fromDateValue = DateTime.ParseExact(FromDate.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture); }
//                    catch { }

//                    try
//                    { toDateValue = DateTime.ParseExact(ToDate.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture); }
//                    catch { }
//                    Session["FromDate"] = "";
//                    Session["ToDate"] = "";

//                    result = (
//       from o in db.orderMain.Where(a => a.OrderDate >= fromDateValue && a.OrderDate <= toDateValue).DefaultIfEmpty()
//       join s in db.Sales on o.OrderNo equals s.OrderNo into salesData
//       from sl in salesData.DefaultIfEmpty()
//       where o.IsCashCustomer == true
//       group new { sl.InvoiceNo, sl.InvoiceDate, o.OrderNo, o.OrderID, o.CurrentStatus, o.DeliverTo, o.NetAmount, o.Discount, o.Freeze, o.IGST, o.SGST, o.CGST, o.TotalAmount, o.CustomerName, o.CustomerGSTNo, o.CustomerCity, sl.ReturnQty, sl.AmountPerUnit }
//        by new { o.OrderNo, o.OrderID }
//   into groupedData
//       orderby groupedData.Key.OrderID ascending
//       select new
//       {
//           groupedData.Key.OrderNo,
//           groupedData.Key.OrderID,
//           CurrentStatus = groupedData.Select(x => x.CurrentStatus).FirstOrDefault(),
//           DeliverTo = groupedData.Select(x => x.DeliverTo).FirstOrDefault(),
//           NetAmount = groupedData.Select(x => x.NetAmount).FirstOrDefault(),
//           Discount = groupedData.Select(x => x.Discount).FirstOrDefault(),
//           Freeze = groupedData.Select(x => x.Freeze).FirstOrDefault(),
//           IGST = groupedData.Select(x => x.IGST).FirstOrDefault(),
//           SGST = groupedData.Select(x => x.SGST).FirstOrDefault(),
//           CGST = groupedData.Select(x => x.CGST).FirstOrDefault(),
//           TotalAmount = groupedData.Select(x => x.TotalAmount).FirstOrDefault(),
//           CustomerName = groupedData.Select(x => x.CustomerName).FirstOrDefault(),
//           CustomerGSTNo = groupedData.Select(x => x.CustomerGSTNo).FirstOrDefault(),
//           CustomerCity = groupedData.Select(x => x.CustomerCity).FirstOrDefault(),
//           InvoiceNo = groupedData.Max(x => x.InvoiceNo),
//           InvoiceDate = groupedData.Max(x => x.InvoiceDate),
//           ReturnQty = groupedData.Sum(x => x.ReturnQty),
//           ReturnAmount = groupedData.Any(x => x.ReturnQty != null && x.ReturnQty != 0)
//                ? groupedData.Sum(x => x.ReturnQty * x.AmountPerUnit)
//                : 0,
//           Amount = groupedData.Any(x => x.ReturnQty != null && x.ReturnQty != 0)
//                ? groupedData.Sum(x => x.TotalAmount) - groupedData.Sum(x => x.ReturnQty * x.AmountPerUnit)
//                : 0

//           //ReturnAmount = groupedData.Sum(x => x.ReturnQty * x.AmountPerUnit),
//           //Amount = groupedData.Sum(x => x.TotalAmount) - groupedData.Sum(x => x.ReturnQty * x.AmountPerUnit)

//       }
//).ToList();




//                }

//                int totalRecords = result.Count();

//                StringBuilder sb = new StringBuilder();
//                sb.Append("{");
//                sb.Append("\"sEcho\":");
//                sb.Append(sEcho);
//                sb.Append(",");
//                sb.Append("\"iTotalRecords\":");
//                sb.Append(totalRecords);
//                sb.Append(",");
//                sb.Append("\"iTotalDisplayRecords\":");
//                sb.Append(totalRecords);
//                sb.Append(",");
//                sb.Append("\"aaData\":");
//                sb.Append(Newtonsoft.Json.JsonConvert.SerializeObject(result));
//                sb.Append("}");
//                return sb.ToString();
//            }
//            catch (Exception ex)
//            {
//                return ex.Message;

//            }
//        }


        public ActionResult SearchData(string FromDate, string ToDate)
        {
            try
            {
                //var dtFrom = DateTime.ParseExact(FromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                //var dtTo = DateTime.ParseExact(ToDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                //var dtTo = dtTo1.AddDays(1); 

                Session["FromDate"] = FromDate;
                Session["ToDate"] = ToDate;

                var result = new { Message = "success" };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception EX)
            {
                var result = new { Message = EX.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }





    }
}