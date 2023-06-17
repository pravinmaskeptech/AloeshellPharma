﻿using Inventory.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class DCSaleReportController : Controller
    {
        private InventoryModel db = new InventoryModel();

        // GET: DCSaleReport
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public string GetData(string sEcho, int iDisplayLength, int iDisplayStart, string sSearch)
        {
            try

            {
               var result = (
    from o in db.orderMain
    join s in db.Sales on o.OrderNo equals s.OrderNo into salesData
    from sl in salesData.DefaultIfEmpty()
    where  o.IsDCSale == true
    group new { sl.InvoiceNo, sl.InvoiceDate, o.OrderNo, o.OrderID, o.CurrentStatus, o.DeliverTo, o.NetAmount, o.Discount, o.Freeze, o.IGST, o.SGST, o.CGST, o.TotalAmount, o.CustomerName, o.CustomerGSTNo, o.CustomerCity, sl.ReturnQty, sl.AmountPerUnit }
    by new { o.OrderNo, o.OrderID }
    into groupedData
    orderby groupedData.Key.OrderID ascending
    select new
    {
        groupedData.Key.OrderNo,
        groupedData.Key.OrderID,
        CurrentStatus = groupedData.Select(x => x.CurrentStatus).FirstOrDefault(),
        DeliverTo = groupedData.Select(x => x.DeliverTo).FirstOrDefault(),
        NetAmount = groupedData.Select(x => x.NetAmount).FirstOrDefault(),
        Discount = groupedData.Select(x => x.Discount).FirstOrDefault(),
        Freeze = groupedData.Select(x => x.Freeze).FirstOrDefault(),
        IGST = groupedData.Select(x => x.IGST).FirstOrDefault(),
        SGST = groupedData.Select(x => x.SGST).FirstOrDefault(),
        CGST = groupedData.Select(x => x.CGST).FirstOrDefault(),
        TotalAmount = groupedData.Select(x => x.TotalAmount).FirstOrDefault(),
        CustomerName = groupedData.Select(x => x.CustomerName).FirstOrDefault(),
        CustomerGSTNo = groupedData.Select(x => x.CustomerGSTNo).FirstOrDefault(),
        CustomerCity = groupedData.Select(x => x.CustomerCity).FirstOrDefault(),
        InvoiceNo = groupedData.Max(x => x.InvoiceNo),
        InvoiceDate = groupedData.Max(x => x.InvoiceDate),
        ReturnQty = groupedData.Sum(x => x.ReturnQty),
        ReturnAmount = groupedData.Any(x => x.ReturnQty != null && x.ReturnQty != 0)
            ? groupedData.Sum(x => x.ReturnQty * x.AmountPerUnit)
            : 0,

        Amount = groupedData.Any(x => x.ReturnQty != null && x.ReturnQty != 0)
            ? groupedData.Sum(x => x.TotalAmount) - groupedData.Sum(x => x.ReturnQty * x.AmountPerUnit)
            : 0,

    }
).ToList();


                var FromDate = Session["FromDate"];
                var ToDate = Session["ToDate"];
                if (FromDate == "" || ToDate == "" || ToDate == null || FromDate == null)
                {
                    result = (
     from o in db.orderMain
     join s in db.Sales on o.OrderNo equals s.OrderNo into salesData
     from sl in salesData.DefaultIfEmpty()
     where o.IsDCSale == true
     group new { sl.InvoiceNo, sl.InvoiceDate, o.OrderNo, o.OrderID, o.CurrentStatus, o.DeliverTo, o.NetAmount, o.Discount, o.Freeze, o.IGST, o.SGST, o.CGST, o.TotalAmount, o.CustomerName, o.CustomerGSTNo, o.CustomerCity, sl.ReturnQty, sl.AmountPerUnit }
     by new { o.OrderNo, o.OrderID }
     into groupedData
     orderby groupedData.Key.OrderID ascending
     select new
     {
         groupedData.Key.OrderNo,
         groupedData.Key.OrderID,
         CurrentStatus = groupedData.Select(x => x.CurrentStatus).FirstOrDefault(),
         DeliverTo = groupedData.Select(x => x.DeliverTo).FirstOrDefault(),
         NetAmount = groupedData.Select(x => x.NetAmount).FirstOrDefault(),
         Discount = groupedData.Select(x => x.Discount).FirstOrDefault(),
         Freeze = groupedData.Select(x => x.Freeze).FirstOrDefault(),
         IGST = groupedData.Select(x => x.IGST).FirstOrDefault(),
         SGST = groupedData.Select(x => x.SGST).FirstOrDefault(),
         CGST = groupedData.Select(x => x.CGST).FirstOrDefault(),
         TotalAmount = groupedData.Select(x => x.TotalAmount).FirstOrDefault(),
         CustomerName = groupedData.Select(x => x.CustomerName).FirstOrDefault(),
         CustomerGSTNo = groupedData.Select(x => x.CustomerGSTNo).FirstOrDefault(),
         CustomerCity = groupedData.Select(x => x.CustomerCity).FirstOrDefault(),
         InvoiceNo = groupedData.Max(x => x.InvoiceNo),
         InvoiceDate = groupedData.Max(x => x.InvoiceDate),
         ReturnQty = groupedData.Sum(x => x.ReturnQty),
         ReturnAmount = groupedData.Any(x => x.ReturnQty != null && x.ReturnQty != 0)
             ? groupedData.Sum(x => x.ReturnQty * x.AmountPerUnit)
             : 0,

         Amount = groupedData.Any(x => x.ReturnQty != null && x.ReturnQty != 0)
             ? groupedData.Sum(x => x.TotalAmount) - groupedData.Sum(x => x.ReturnQty * x.AmountPerUnit)
             : 0,

        
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
                    Session["FromDate"] = "";
                    Session["ToDate"] = "";

                    result = (
        from o in db.orderMain.Where(a => a.OrderDate >= fromDateValue && a.OrderDate <= toDateValue).DefaultIfEmpty()
        join s in db.Sales on o.OrderNo equals s.OrderNo into salesData
    from sl in salesData.DefaultIfEmpty()
        where o.IsDCSale == true
        group new { sl.InvoiceNo, sl.InvoiceDate, o.OrderNo, o.OrderID, o.CurrentStatus, o.DeliverTo, o.NetAmount, o.Discount, o.Freeze, o.IGST, o.SGST, o.CGST, o.TotalAmount, o.CustomerName, o.CustomerGSTNo, o.CustomerCity, sl.ReturnQty, sl.AmountPerUnit }
    by new { o.OrderNo, o.OrderID }
    into groupedData
    orderby groupedData.Key.OrderID ascending
    select new
    {
        groupedData.Key.OrderNo,
        groupedData.Key.OrderID,
        CurrentStatus = groupedData.Select(x => x.CurrentStatus).FirstOrDefault(),
        DeliverTo = groupedData.Select(x => x.DeliverTo).FirstOrDefault(),
        NetAmount = groupedData.Select(x => x.NetAmount).FirstOrDefault(),
        Discount = groupedData.Select(x => x.Discount).FirstOrDefault(),
        Freeze = groupedData.Select(x => x.Freeze).FirstOrDefault(),
        IGST = groupedData.Select(x => x.IGST).FirstOrDefault(),
        SGST = groupedData.Select(x => x.SGST).FirstOrDefault(),
        CGST = groupedData.Select(x => x.CGST).FirstOrDefault(),
        TotalAmount = groupedData.Select(x => x.TotalAmount).FirstOrDefault(),
        CustomerName = groupedData.Select(x => x.CustomerName).FirstOrDefault(),
        CustomerGSTNo = groupedData.Select(x => x.CustomerGSTNo).FirstOrDefault(),
        CustomerCity = groupedData.Select(x => x.CustomerCity).FirstOrDefault(),
        InvoiceNo = groupedData.Max(x => x.InvoiceNo),
        InvoiceDate = groupedData.Max(x => x.InvoiceDate),
        ReturnQty = groupedData.Sum(x => x.ReturnQty),
        ReturnAmount = groupedData.Any(x => x.ReturnQty != null && x.ReturnQty != 0)
            ? groupedData.Sum(x => x.ReturnQty * x.AmountPerUnit)
            : 0,

        Amount = groupedData.Any(x => x.ReturnQty != null && x.ReturnQty != 0)
            ? groupedData.Sum(x => x.TotalAmount) - groupedData.Sum(x => x.ReturnQty * x.AmountPerUnit)
            : 0,

        
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
            catch (Exception ex)
            {
                return ex.Message;

            }
        }


        public ActionResult SearchData(string FromDate, string ToDate)
        {
            try
            {
               
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