using Inventory.Models;
using Syncfusion.EJ.Export;
using Syncfusion.JavaScript.Models;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Inventory.Controllers
{
    public class LedgerReportController : Controller
    {
        private InventoryModel db = new InventoryModel();

        // GET: Categories
        public ActionResult Index()
        {
            //try
            //{
            //    var FromDate = DateTime.Now;
            //    var ToDate = DateTime.Now;

            //    var ProductCodeList = db.Products.Select(a => a.ProductCode).ToList();
            //    var productStockList = new List<object>();

            //    foreach (var ProductCode in ProductCodeList)
            //    {
            //        var sumOrderQuantity = 0;
            //        var orderDetails = db.orderDetails.Where(a => a.ProductCode == ProductCode && a.CreatedDate >= FromDate && a.CreatedDate <= ToDate).ToList();

            //        if (orderDetails == null || orderDetails.Count == 0)
            //        {
            //            sumOrderQuantity = 0;
            //        }
            //        else
            //        {
            //            sumOrderQuantity = (int)orderDetails.Sum(a => a.OrderQty);
            //        }

            //        var sumReplaceQuantity = 0;
            //        var poReplacement = db.POReplacement.Where(a => a.ProductCode == ProductCode && a.CreatedDate >= FromDate && a.CreatedDate <= ToDate).ToList();

            //        if (poReplacement == null || poReplacement.Count == 0)
            //        {
            //            sumReplaceQuantity = 0;
            //        }
            //        else
            //        {
            //            sumReplaceQuantity = (int)poReplacement.Sum(a => a.ReplacedQty);
            //        }

            //        var ProductName = db.Products.Where(a => a.ProductCode == ProductCode).FirstOrDefault();
            //        if(ProductName == null)
            //        {
            //            TempData["Msg"] = "Product not found.";
            //        }

            //        var productStock = new
            //        {
            //            ProductCode = ProductCode,
            //            ProductName = ProductName,
            //            SumOrderQuantity = sumOrderQuantity,
            //            SumReplaceQuantity = sumReplaceQuantity
            //        };

            //        productStockList.Add(productStock);
            //    }

            //    ViewBag.datasource = productStockList;
            //}


            try
            {
                var FromDate = DateTime.Now;
                var ToDate = DateTime.Now;

                var productStockList = db.Products
                                   .Select(p => new
                                   {
                                       ProductCode = p.ProductCode,
                                       ProductName = p.ProductName,
                                       SumOrderQuantity = db.orderDetails
                                           .Where(od => od.ProductCode == p.ProductCode && od.CreatedDate >= FromDate && od.CreatedDate <= ToDate)
                                           .Sum(od => (int?)od.OrderQty) ?? 0,
                                       SumReplaceQuantity = db.POReplacement
                                           .Where(pr => pr.ProductCode == p.ProductCode && pr.CreatedDate >= FromDate && pr.CreatedDate <= ToDate)
                                           .Sum(pr => (int?)pr.ReplacedQty) ?? 0,
                                       SumReceivedQuantity = db.GRNDetail
                                            .Where(g => g.ProductCode == p.ProductCode)
                                            .Sum(g => (int?)g.ReceivedQty) ?? 0,
                                       ClosingQuantity = db.GRNDetail
                                            .Where(g => g.ProductCode == p.ProductCode)
                                            .Sum(g => ((int?)g.ReceivedQty) - db.orderDetails
                                            .Where(od => od.ProductCode == p.ProductCode)
                                            .Sum(od => (int?)od.OrderQty)) ?? 0
                                   })
                                   .ToList();

                ViewBag.datasource = productStockList;
            }
            catch (FormatException ex)
            {
                // Handle the exception here, for example by logging it or displaying an error message to the user
                ViewBag.datasource = null;
                TempData["Msg"] = "Invalid input format: " + ex.Message;
            }

            return View();

        }

        [ValidateInput(false)]
        public void ExportToExcel(string GridModel)
        {
            ExcelExport exp = new ExcelExport();
            var RecievedQty = db.GRNDetail.Select(p => p.ProductCode).Distinct().ToList();

            var Datasource = new List<object>();
            foreach (var cat in RecievedQty)
            {
                var GRNRecievedQty = db.GRNDetail
                                        .Where(a => a.ProductCode == cat).Select(p => p.ReceivedQty).ToList();
                var RecievedStock = 0;
                foreach (var qty in GRNRecievedQty)
                {
                    RecievedStock += (int)qty.GetValueOrDefault();
                }

                var SaleDeliverdQty = db.orderDetails
                                       .Where(a => a.ProductCode == cat).Select(p => p.DeliveredQty).ToList();

                var DeliverdStock = 0;

                foreach (var qty in SaleDeliverdQty)
                {
                    DeliverdStock += (int)qty.GetValueOrDefault();
                }

                var AvailableStock = RecievedStock - DeliverdStock;

                var prName = db.Products.Where(a => a.ProductCode == cat).Select(p => p.ProductName).FirstOrDefault();
                if (prName == null) {; }

                Datasource.Add(new { ProductCode = cat, ProductName = prName, SerialNo = AvailableStock });

            }
            GridProperties obj = ConvertGridObject(GridModel);
            //obj.Columns[8].DataSource = db.Categories.ToList();
            exp.Export(obj, Datasource, "AvailableStock.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
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
