using Inventory.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class ProductExpiryReportController : Controller
    {
        private InventoryModel db = new InventoryModel();

        // GET: ProductExpiryReport
        public ActionResult Index()
        {
            ViewBag.Productdatasource = db.Products.Where(a => a.IsActive == true).ToList();

            return View();
        }



        [HttpPost]
        public string GetData(string sEcho, int iDisplayLength, int iDisplayStart, string sSearch)
        {
            //var today = DateTime.Today;
            //var expiryThreshold = today.AddDays(15);

            //var result = (from c in db.GRNDetail
            //              join p in db.Products on c.ProductCode equals p.ProductCode
            //              orderby c.GRNId
            //              join sply in db.suppliers on c.SupplierID equals sply.SupplierID into supplierr
            //              from aa in supplierr.DefaultIfEmpty()
            //              group c by new
            //              {
            //                  c.GRNNo,
            //                  c.GRNDate,
            //                  c.PONo,
            //                  aa.SupplierName,
            //                  p.ProductCode,
            //                  p.ProductName,
            //                  c.ManufacturingDate,
            //                  c.BatchNo,
            //                  c.ExpiryDate
            //              } into gcs
            //              where gcs.Max(a => a.ExpiryDate) <= expiryThreshold && gcs.Max(a => a.ExpiryDate) >= today
            //              select new
            //              {
            //                  GRNId = gcs.Max(a => a.GRNId),
            //                  GRNNo = gcs.Key.GRNNo,
            //                  GRNDate = gcs.Key.GRNDate,
            //                  SupplierName = gcs.Key.SupplierName,
            //                  PONo = gcs.Key.PONo,
            //                  ReceivedQty = gcs.Sum(a => a.ReceivedQty),
            //                  ExpiryDate = gcs.Max(a => a.ExpiryDate),
            //                  ManufacturingDate = gcs.Key.ManufacturingDate,
            //                  BatchNumber = gcs.Key.BatchNo,
            //                  ProductCode = gcs.Key.ProductCode,
            //                  ProductName = gcs.Key.ProductName
            //              }).ToList();
            var result = (from c in db.GRNDetail
                          join p in db.Products on c.ProductCode equals p.ProductCode // Assuming a relationship between GRNDetail and Product tables based on the ProductId
                          orderby c.GRNId
                          join sply in db.suppliers on c.SupplierID equals sply.SupplierID into supplierr
                          from aa in supplierr.DefaultIfEmpty()
                          group c by new
                          {
                              c.GRNNo,
                              c.GRNDate,
                              c.PONo,
                              aa.SupplierName,
                              p.ProductCode,
                              p.ProductName,
                              c.ManufacturingDate,
                              c.BatchNo
                          } into gcs
                          select new
                          {
                              GRNId = gcs.Max(a => a.GRNId),
                              GRNNo = gcs.Key.GRNNo,
                              GRNDate = gcs.Key.GRNDate,
                              SupplierName = gcs.Key.SupplierName,
                              PONo = gcs.Key.PONo,
                              ReceivedQty = gcs.Sum(a => a.ReceivedQty),
                              ExpiryDate = gcs.Max(a => a.ExpiryDate),
                              ManufacturingDate = gcs.Key.ManufacturingDate,
                              BatchNumber = gcs.Key.BatchNo,
                              ProductCode = gcs.Key.ProductCode,
                              ProductName = gcs.Key.ProductName
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
                 result = (from c in db.GRNDetail
                              join p in db.Products on c.ProductCode equals p.ProductCode // Assuming a relationship between GRNDetail and Product tables based on the ProductId
                              orderby c.GRNId
                              join sply in db.suppliers on c.SupplierID equals sply.SupplierID into supplierr
                              from aa in supplierr.DefaultIfEmpty()
                              group c by new
                              {
                                  c.GRNNo,
                                  c.GRNDate,
                                  c.PONo,
                                  aa.SupplierName,
                                  p.ProductCode,
                                  p.ProductName,
                                  c.ManufacturingDate,
                                  c.BatchNo
                              } into gcs
                              select new
                              {
                                  GRNId = gcs.Max(a => a.GRNId),
                                  GRNNo = gcs.Key.GRNNo,
                                  GRNDate = gcs.Key.GRNDate,
                                  SupplierName = gcs.Key.SupplierName,
                                  PONo = gcs.Key.PONo,
                                  ReceivedQty = gcs.Sum(a => a.ReceivedQty),
                                  ExpiryDate = gcs.Max(a => a.ExpiryDate),
                                  ManufacturingDate = gcs.Key.ManufacturingDate,
                                  BatchNumber = gcs.Key.BatchNo,
                                  ProductCode = gcs.Key.ProductCode,
                                  ProductName = gcs.Key.ProductName
                              }).ToList();


                //result = (from c in db.GRNDetail
                //          join p in db.Products on c.ProductCode equals p.ProductCode
                //          orderby c.GRNId
                //          join sply in db.suppliers on c.SupplierID equals sply.SupplierID into supplierr
                //          from aa in supplierr.DefaultIfEmpty()
                //          group c by new
                //          {
                //              c.GRNNo,
                //              c.GRNDate,
                //              c.PONo,
                //              aa.SupplierName,
                //              p.ProductCode,
                //              p.ProductName,
                //              c.ManufacturingDate,
                //              c.BatchNo,
                //              c.ExpiryDate
                //          } into gcs
                //          where gcs.Max(a => a.ExpiryDate) <= expiryThreshold && gcs.Max(a => a.ExpiryDate) >= today
                //          select new
                //          {
                //              GRNId = gcs.Max(a => a.GRNId),
                //              GRNNo = gcs.Key.GRNNo,
                //              GRNDate = gcs.Key.GRNDate,
                //              SupplierName = gcs.Key.SupplierName,
                //              PONo = gcs.Key.PONo,
                //              ReceivedQty = gcs.Sum(a => a.ReceivedQty),
                //              ExpiryDate = gcs.Max(a => a.ExpiryDate),
                //              ManufacturingDate = gcs.Key.ManufacturingDate,
                //              BatchNumber = gcs.Key.BatchNo,
                //              ProductCode = gcs.Key.ProductCode,
                //              ProductName = gcs.Key.ProductName
                //          }).ToList();
               
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
                result = (from c in db.GRNDetail.Where(a => a.ManufacturingDate >= fromDateValue && a.ExpiryDate <= toDateValue/*.AddDays(1)*/)
                              join p in db.Products on c.ProductCode equals p.ProductCode // Assuming a relationship between GRNDetail and Product tables based on the ProductId
                              orderby c.GRNId
                              join sply in db.suppliers on c.SupplierID equals sply.SupplierID into supplierr
                              from aa in supplierr.DefaultIfEmpty()
                              group c by new
                              {
                                  c.GRNNo,
                                  c.GRNDate,
                                  c.PONo,
                                  aa.SupplierName,
                                  p.ProductCode,
                                  p.ProductName,
                                  c.ManufacturingDate,
                                  c.BatchNo
                              } into gcs
                              select new
                              {
                                  GRNId = gcs.Max(a => a.GRNId),
                                  GRNNo = gcs.Key.GRNNo,
                                  GRNDate = gcs.Key.GRNDate,
                                  SupplierName = gcs.Key.SupplierName,
                                  PONo = gcs.Key.PONo,
                                  ReceivedQty = gcs.Sum(a => a.ReceivedQty),
                                  ExpiryDate = gcs.Max(a => a.ExpiryDate),
                                  ManufacturingDate = gcs.Key.ManufacturingDate,
                                  BatchNumber = gcs.Key.BatchNo,
                                  ProductCode = gcs.Key.ProductCode,
                                  ProductName = gcs.Key.ProductName
                              }).ToList();


                    //result = (from c in db.GRNDetail.Where(a => a.GRNDate >= fromDateValue && a.GRNDate <= toDateValue)
                    //      join p in db.Products on c.ProductCode equals p.ProductCode // Assuming a relationship between GRNDetail and Product tables based on the ProductId
                    //      orderby c.GRNId
                    //      join sply in db.suppliers on c.SupplierID equals sply.SupplierID into supplierr
                    //      from aa in supplierr.DefaultIfEmpty()
                    //      group c by new
                    //      {
                    //          c.GRNNo,
                    //          c.GRNDate,
                    //          c.PONo,
                    //          aa.SupplierName,
                    //          p.ProductCode,
                    //          p.ProductName,
                    //          c.ManufacturingDate,
                    //          c.BatchNo
                    //      } into gcs
                    //      select new
                    //      {
                    //          GRNId = gcs.Max(a => a.GRNId),
                    //          GRNNo = gcs.Key.GRNNo,
                    //          GRNDate = gcs.Key.GRNDate,
                    //          SupplierName = gcs.Key.SupplierName,
                    //          PONo = gcs.Key.PONo,
                    //          ReceivedQty = gcs.Sum(a => a.ReceivedQty),
                    //          ExpiryDate = gcs.Max(a => a.ExpiryDate),
                    //          ManufacturingDate = gcs.Key.ManufacturingDate,
                    //          BatchNumber = gcs.Key.BatchNo,
                    //          ProductCode = gcs.Key.ProductCode,
                    //          ProductName = gcs.Key.ProductName
                    //      }).ToList();



            }
                        result = result.Where(b => (b.ProductName == Product || string.IsNullOrEmpty(Product))).ToList();


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


        public ActionResult SearchData(string FromDate, string ToDate, string Product)
        {
            try
            {
                //ViewBag.FromDate = FromDate;
                //ViewBag.ToDate = ToDate;

                Session["FromDate"] = FromDate;
                Session["ToDate"] = ToDate;
                Session["Product"] = Product;

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