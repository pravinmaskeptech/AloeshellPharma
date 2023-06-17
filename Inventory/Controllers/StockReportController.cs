using Inventory.Models;
using Syncfusion.EJ.Export;
using Syncfusion.JavaScript.DataSources;
using Syncfusion.JavaScript;
using Syncfusion.JavaScript.DataVisualization.DiagramEnums;
using Syncfusion.JavaScript.Models;
using Syncfusion.Windows.Shared;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Text;
using Syncfusion.PMML;
using Microsoft.Ajax.Utilities;

namespace Inventory.Controllers
{
    public class StockReportController : Controller
    {
        private InventoryModel db = new InventoryModel();

        // GET: Categories
        public ActionResult Index()
        {
            ViewBag.Productdatasource = db.Products.Where(a => a.IsActive == true).ToList();
            //ViewBag.Categorydatasource = db.Products.Where(a => a.IsActive == true).ToList();
            ViewBag.Categorydatasource = db.Categories.Where(a => a.IsActive == true).ToList();

            return View();
        }



        [HttpPost]
        public string GetData(string sEcho, int iDisplayLength, int iDisplayStart, string sSearch)
        {

            var xx1 = db.GRNDetail
   .GroupBy(l => l.ProductCode)
   .Select(cl => new
   {
       ProductCode = cl.Key,
       ReceivedQty = cl.Sum(c => c.ReceivedQty) - cl.Sum(c => c.ReturnQty),
       SalesQty = db.orderDetails.Where(s => s.ProductCode == cl.Key).Sum(s => s.DeliveredQty) - db.Sales.Where(s => s.ProductCode == cl.Key).Sum(s => s.ReturnQty),
       AvailableQty = (cl.Sum(c => c.ReceivedQty) - cl.Sum(c => c.ReturnQty)) - (db.orderDetails.Where(s => s.ProductCode == cl.Key).Sum(s => s.DeliveredQty) - db.Sales.Where(s => s.ProductCode == cl.Key).Sum(s => s.ReturnQty))
   })
   .ToList();

            var xxFiltered1 = xx1.Where(x => db.Products.Any(p => p.ProductCode == x.ProductCode && p.SerialNoApplicable == true)).ToList();


            var xx2 = db.GRNDetail
    .GroupBy(l => l.ProductCode)
    .Select(cl => new
    {
        ProductCode = cl.FirstOrDefault().ProductCode,
        ReceivedQty = cl.Sum(c => c.ReceivedQty) - cl.Sum(c => c.ReturnQty),
        SalesQty = cl.Sum(c => c.SalesQty),
        AvailableQty = (cl.Sum(c => c.ReceivedQty) - cl.Sum(c => c.ReturnQty)) - cl.Sum(a => a.SalesQty),
    })
    .ToList();

            var xxFiltered2 = xx2.Where(x => db.Products.Any(p => p.ProductCode == x.ProductCode && p.SerialNoApplicable == false)).ToList();

            var xx = xxFiltered1.Concat(xxFiltered2).ToList();



            var products = db.Products.ToList();
            var categories = db.Categories.ToList();

            var result = from product in products.Where(a => a.ProductName == "sgahdfafddadf")
                         join grn in xx on product.ProductCode equals grn.ProductCode into supplier
                         from grn in supplier.DefaultIfEmpty()
                         join category in categories on product.CategoryId equals category.CategoryId

                         where !string.IsNullOrEmpty(grn?.ProductCode)

                         select new
                         {
                             product.ProductName,
                             CategoryName = category.CategoryName,

                             grn?.ProductCode,
                             grn?.ReceivedQty,
                             grn?.SalesQty,
                             grn?.AvailableQty,
                         };

            // ViewBag.datasource = result.ToList();
            string Product = "";
            string Category = "";

            var FromDate = Session["FromDate"];
            var ToDate = Session["ToDate"];
            try
            {
                Product = Session["Product"]?.ToString();
                Category = Session["Category"]?.ToString();


            }
            catch
            {

            }

            Session["FromDate"] = "";
            Session["ToDate"] = "";
            Session["Product"] = "";
            Session["Category"] = "";


            if (FromDate == "" || ToDate == "" || ToDate == null || FromDate == null)
            {

                xx1 = db.GRNDetail
    .GroupBy(l => l.ProductCode)
    .Select(cl => new
    {
        ProductCode = cl.Key,
        ReceivedQty = cl.Sum(c => c.ReceivedQty) - cl.Sum(c => c.ReturnQty),
        SalesQty = db.orderDetails.Where(s => s.ProductCode == cl.Key).Sum(s => s.DeliveredQty) - db.Sales.Where(s => s.ProductCode == cl.Key).Sum(s => s.ReturnQty),
        AvailableQty = (cl.Sum(c => c.ReceivedQty) - cl.Sum(c => c.ReturnQty)) - (db.orderDetails.Where(s => s.ProductCode == cl.Key).Sum(s => s.DeliveredQty) - db.Sales.Where(s => s.ProductCode == cl.Key).Sum(s => s.ReturnQty)),
    })
    .ToList();
                xxFiltered1 = xx1.Where(x => db.Products.Any(p => p.ProductCode == x.ProductCode && p.SerialNoApplicable == true)).ToList();


                xx2 = db.GRNDetail
       .GroupBy(l => l.ProductCode)
       .Select(cl => new
       {
           ProductCode = cl.FirstOrDefault().ProductCode,
           ReceivedQty = cl.Sum(c => c.ReceivedQty) - cl.Sum(c => c.ReturnQty),
           SalesQty = cl.Sum(c => c.SalesQty),
           AvailableQty = (cl.Sum(c => c.ReceivedQty) - cl.Sum(c => c.ReturnQty)) - cl.Sum(a => a.SalesQty),
       })
    .ToList();

                xxFiltered2 = xx2.Where(x => db.Products.Any(p => p.ProductCode == x.ProductCode && p.SerialNoApplicable == false)).ToList();

                xx = xxFiltered1.Concat(xxFiltered2).ToList();

                products = db.Products.ToList();
                categories = db.Categories.ToList();

                result = (from product in products
                          join grn in xx on product.ProductCode equals grn.ProductCode into supplier
                          from grn in supplier.DefaultIfEmpty()
                          join category in categories on product.CategoryId equals category.CategoryId

                          where !string.IsNullOrEmpty(grn?.ProductCode)

                          select new
                          {
                              product.ProductName,
                              CategoryName = category.CategoryName,

                              grn?.ProductCode,
                              grn?.ReceivedQty,
                              grn?.SalesQty,
                              grn?.AvailableQty
                          }).ToList();


            }
            else
            {
                DateTime fromDateValue = new DateTime();
                DateTime toDateValue = new DateTime();

                try
                {
                    fromDateValue = DateTime.ParseExact(FromDate.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }
                catch { }

                try
                {
                    toDateValue = DateTime.ParseExact(ToDate.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }
                catch { }

                xx1 = db.GRNDetail
                    .Where(a => a.GRNDate >= fromDateValue && a.GRNDate <= toDateValue)
                    .GroupBy(l => l.ProductCode)
                    .Select(cl => new
                    {
                        ProductCode = cl.Key,
                        ReceivedQty = cl.Sum(c => c.ReceivedQty) - cl.Sum(c => c.ReturnQty),
                        SalesQty = db.orderDetails.Where(s => s.ProductCode == cl.Key && s.CreatedDate >= fromDateValue && s.CreatedDate <= toDateValue).Sum(s => s.DeliveredQty) - db.Sales.Where(s => s.ProductCode == cl.Key && s.CreatedDate >= fromDateValue && s.CreatedDate <= toDateValue).Sum(s => s.ReturnQty),
                        AvailableQty = (db.GRNDetail.Where(s => s.ProductCode == cl.Key).Sum(c => c.ReceivedQty - c.ReturnQty) - (db.orderDetails.Where(s => s.ProductCode == cl.Key).Sum(s => s.DeliveredQty) - db.Sales.Where(s => s.ProductCode == cl.Key).Sum(s => s.ReturnQty)))
                        - (cl.Sum(c => c.ReceivedQty) - cl.Sum(c => c.ReturnQty) - (db.orderDetails.Where(s => s.ProductCode == cl.Key && s.CreatedDate >= fromDateValue && s.CreatedDate <= toDateValue).Sum(s => s.DeliveredQty) - db.Sales.Where(s => s.ProductCode == cl.Key && s.CreatedDate >= fromDateValue && s.CreatedDate <= toDateValue).Sum(s => s.ReturnQty)))
                    })
                    .ToList();

               



                xxFiltered1 = xx1.Where(x => db.Products.Any(p => p.ProductCode == x.ProductCode && p.SerialNoApplicable == true)).ToList();


                xx2 = db.GRNDetail
                    .Where(a => a.GRNDate >= fromDateValue && a.GRNDate <= toDateValue)
                    .GroupBy(l => l.ProductCode)
                    .Select(cl => new
                    {
                        ProductCode = cl.FirstOrDefault().ProductCode,
                        ReceivedQty = cl.Sum(c => c.ReceivedQty) - cl.Sum(c => c.ReturnQty),
                        SalesQty = cl.Sum(c => c.SalesQty),
                        AvailableQty = (db.GRNDetail.Where(s => s.ProductCode == cl.Key).Sum(c => c.ReceivedQty - c.ReturnQty) - (db.orderDetails.Where(s => s.ProductCode == cl.Key).Sum(s => s.DeliveredQty) - db.Sales.Where(s => s.ProductCode == cl.Key).Sum(s => s.ReturnQty))) - ((cl.Sum(c => c.ReceivedQty) - cl.Sum(c => c.ReturnQty)) - cl.Sum(a => a.SalesQty))
                    })
                    .ToList();


                //            xx2 = db.GRNDetail.Where(a => a.GRNDate >= fromDateValue && a.GRNDate <= toDateValue)
                //   .GroupBy(l => l.ProductCode)
                //   .Select(cl => new
                //   {
                //       ProductCode = cl.FirstOrDefault().ProductCode,
                //       ReceivedQty = cl.Sum(c => c.ReceivedQty) - cl.Sum(c => c.ReturnQty),
                //       SalesQty = cl.Sum(c => c.SalesQty),
                //       AvailableQty = (db.GRNDetail.Where(s => s.ProductCode == cl.Key).Sum(c => c.ReceivedQty - c.ReturnQty) - (db.orderDetails.Where(s => s.ProductCode == cl.Key).Sum(s => s.DeliveredQty) - db.Sales.Where(s => s.ProductCode == cl.Key).Sum(s => s.ReturnQty))) - ((cl.Sum(c => c.ReceivedQty) - cl.Sum(c => c.ReturnQty)) - cl.Sum(a => a.SalesQty)),
                //   })
                //.ToList();

                xxFiltered2 = xx2.Where(x => db.Products.Any(p => p.ProductCode == x.ProductCode && p.SerialNoApplicable == false)).ToList();

                xx = xxFiltered1.Concat(xxFiltered2).ToList();

                result = (from product in products
                          join grn in xx on product.ProductCode equals grn.ProductCode into supplier
                          from grn in supplier.DefaultIfEmpty()
                          join category in categories on product.CategoryId equals category.CategoryId

                          where !string.IsNullOrEmpty(grn?.ProductCode)

                          select new
                          {
                              product.ProductName,
                              CategoryName = category.CategoryName,
                              grn?.ProductCode,
                              grn?.ReceivedQty,
                              grn?.SalesQty,
                              grn?.AvailableQty
                          }).ToList();



            }

            //result = result.Where(b =>b.ProductName == Product || Product == "" || Product == null).ToList();
            result = result.Where(b => (b.ProductName == Product || string.IsNullOrEmpty(Product)) && (b.CategoryName == Category || string.IsNullOrEmpty(Category))).ToList();

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


        public ActionResult SearchData(string FromDate, string ToDate, string Product, string Category)
        {
            try
            {


                Session["FromDate"] = FromDate;
                Session["ToDate"] = ToDate;
                Session["Product"] = Product;
                Session["Category"] = Category;

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
