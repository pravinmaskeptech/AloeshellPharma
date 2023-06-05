﻿using Inventory.Models;
using Syncfusion.JavaScript;
using Syncfusion.JavaScript.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Inventory.Controllers
{
    public class SalesReportController : Controller
    {
        private InventoryModel db = new InventoryModel();

        // GET: SalesReport
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
  where o.IsCashCustomer == true
  group new { sl.InvoiceNo, sl.InvoiceDate, o.OrderNo, o.OrderID, o.CurrentStatus, o.DeliverTo, o.NetAmount, o.Discount, o.Freeze, o.IGST, o.SGST, o.CGST, o.TotalAmount, o.CustomerName }
  by new { o.OrderNo, o.OrderID, o.CurrentStatus, o.DeliverTo, o.NetAmount, o.Discount, o.Freeze, o.IGST, o.SGST, o.CGST, o.TotalAmount, o.CustomerName } into groupedData
  orderby groupedData.Key.OrderID ascending
  select new
  {
      groupedData.Key.OrderNo,
      groupedData.Key.OrderID,
      groupedData.Key.CurrentStatus,
      groupedData.Key.DeliverTo,
      groupedData.Key.NetAmount,
      groupedData.Key.Discount,
      groupedData.Key.Freeze,
      groupedData.Key.IGST,
      groupedData.Key.SGST,
      groupedData.Key.CGST,
      groupedData.Key.TotalAmount,
      groupedData.Key.CustomerName,
      InvoiceNo = groupedData.Max(x => x.InvoiceNo),
      InvoiceDate = groupedData.Max(x => x.InvoiceDate)
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
   where o.IsCashCustomer == true
   group new { sl.InvoiceNo, sl.InvoiceDate, o.OrderNo, o.OrderID, o.CurrentStatus, o.DeliverTo, o.NetAmount, o.Discount, o.Freeze, o.IGST, o.SGST, o.CGST, o.TotalAmount, o.CustomerName }
   by new { o.OrderNo, o.OrderID, o.CurrentStatus, o.DeliverTo, o.NetAmount, o.Discount, o.Freeze, o.IGST, o.SGST, o.CGST, o.TotalAmount, o.CustomerName } into groupedData
   orderby groupedData.Key.OrderID ascending
   select new
   {
       groupedData.Key.OrderNo,
       groupedData.Key.OrderID,
       groupedData.Key.CurrentStatus,
       groupedData.Key.DeliverTo,
       groupedData.Key.NetAmount,
       groupedData.Key.Discount,
       groupedData.Key.Freeze,
       groupedData.Key.IGST,
       groupedData.Key.SGST,
       groupedData.Key.CGST,
       groupedData.Key.TotalAmount,
       groupedData.Key.CustomerName,
       InvoiceNo = groupedData.Max(x => x.InvoiceNo),
       InvoiceDate = groupedData.Max(x => x.InvoiceDate)
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
  where o.IsCashCustomer == true
  group new { sl.InvoiceNo, sl.InvoiceDate, o.OrderNo, o.OrderID, o.CurrentStatus, o.DeliverTo, o.NetAmount, o.Discount, o.Freeze, o.IGST, o.SGST, o.CGST, o.TotalAmount, o.CustomerName }
  by new { o.OrderNo, o.OrderID, o.CurrentStatus, o.DeliverTo, o.NetAmount, o.Discount, o.Freeze, o.IGST, o.SGST, o.CGST, o.TotalAmount, o.CustomerName } into groupedData
  orderby groupedData.Key.OrderID ascending
  select new
  {
      groupedData.Key.OrderNo,
      groupedData.Key.OrderID,
      groupedData.Key.CurrentStatus,
      groupedData.Key.DeliverTo,
      groupedData.Key.NetAmount,
      groupedData.Key.Discount,
      groupedData.Key.Freeze,
      groupedData.Key.IGST,
      groupedData.Key.SGST,
      groupedData.Key.CGST,
      groupedData.Key.TotalAmount,
      groupedData.Key.CustomerName,
      InvoiceNo = groupedData.Max(x => x.InvoiceNo),
      InvoiceDate = groupedData.Max(x => x.InvoiceDate)
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

        private GridProperties ConvertGridObject(string gridProperty)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            IEnumerable div = (IEnumerable)serializer.Deserialize(gridProperty, typeof(IEnumerable));
            GridProperties gridProp = new GridProperties();
            foreach (KeyValuePair<string, object> ds in div)
            {
                var property = gridProp.GetType().GetProperty(ds.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (property != null)
                {
                    Type type = property.PropertyType;
                    string serialize = serializer.Serialize(ds.Value);
                    object value = serializer.Deserialize(serialize, type);
                    property.SetValue(gridProp, value, null);
                }
            }
            return gridProp;
        }


    }
}