 using Inventory.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class SalesController : Controller
    {
        private InventoryModel db = new InventoryModel();
        // GET: Sales
        public ActionResult Index()
        {
            var setting = db.Settings.Where(x => x.FieldName == "BatchNo" && x.Setting == true).FirstOrDefault();
            if (setting != null)
                ViewBag.BatchNoSetting = setting.FieldName;
            else
                ViewBag.BatchNoSetting = "";

            var result = from c in db.Sales
                         //where c.OrderDetailsID != 0 
                                 join cust in db.Customers on c.CustomerID equals cust.CustomerID into supplierr
                                 from aa in supplierr.DefaultIfEmpty()
                                 group c by new
                                 {
                                     c.InvoiceNo,
                                     c.InvoiceDate,
                                     c.OrderNo,
                                     aa.CustomerName
                                 } into gcs
                                 select new
                                 {
                                     SalesId = gcs.Max(a => a.SalesId), 
                                     InvoiceNo = gcs.Key.InvoiceNo,
                                     InvoiceDate = gcs.Key.InvoiceDate,
                                     CustomerName = gcs.Key.CustomerName,
                                     OrderNo = gcs.Key.OrderNo,
                                     DeliveredQty = gcs.Sum(a => a.DeliveredQty),
                                     order = gcs.ToList(),
                                 };

            ViewBag.datasource = result.OrderByDescending(a => a.SalesId).ToList();

            return View();
        }
        public JsonResult GetAllSalesDetails(string InvoiceNo)
        {
            if (InvoiceNo == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var Productsmaster = new List<Products>(db.Products);
                var Salesetails = new List<Sales>(db.Sales);
                var result = (from sales in Salesetails.Where(a => a.InvoiceNo == InvoiceNo)

                              join Product in Productsmaster on sales.ProductCode equals Product.ProductCode into products
                              from prd in products.DefaultIfEmpty()

                              orderby sales.SalesId descending
                              select new { InvoiceNo = sales.InvoiceNo, InvoiceDate = sales.InvoiceDate, OrderNo = sales.OrderNo, DeliveredQty = sales.DeliveredQty, BatchNo = sales.BatchNo, ProductCode = prd == null ? string.Empty : prd.ProductName, ReturnQty = sales.ReturnQty, ReturnReason = sales.ReturnReason }
                                    ).ToList();

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Create()
        {
            var setting = db.Settings.Where(x => x.FieldName == "BatchNo" && x.Setting == true).FirstOrDefault();
            if (setting != null)
                ViewBag.BatchNoSetting = setting.FieldName;
            else
                ViewBag.BatchNoSetting = "";
            var result = db.orderDetails.Where(a => a.OrderQty != a.DeliveredQty).Select(x => x.OrderID).ToList();
            ViewBag.OrderMaindatasource = db.orderMain.Where(a => result.Contains(a.OrderID) && a.CurrentStatus== "Approve").ToList();
            return View();
        }

        public JsonResult ShowOrderdetails(string OrderNo)
        {
            if (OrderNo == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var poid = db.orderMain.Where(a => a.OrderNo == OrderNo && a.IsCashCustomer ==false).FirstOrDefault();

                var Productmaster = new List<Products>(db.Products);
                //     var Warehousemaster = new List<Warehouse>(db.Warehouses);
                var Customermaster = new List<Customer>(db.Customers);
                var orderDetails = new List<OrderDetails>(db.orderDetails);
                var stockAllocation = new List<StockAllocation>(db.StockAllocation);

                var result = (from Order in orderDetails.Where(a => a.OrderID == poid.OrderID && a.OrderQty != a.DeliveredQty)

                              join Product in Productmaster on Order.ProductCode equals Product.ProductCode into product
                              from prd in product.DefaultIfEmpty()                           

                              join Customer in Customermaster on poid.CustomerID equals Customer.CustomerID into customer
                              from cust in customer.DefaultIfEmpty()

                              orderby Order.OrderDetailsID descending
                              select new { OrderDetailsID = Order.OrderDetailsID, OrderID = Order.OrderID, HSNCode = Order.HSNCode, ProductCode = Order.ProductCode, OrderNo = Order.OrderNo, GSTPercentage = Order.GSTPercentage, Price = Order.Price, OrderQty = Order.OrderQty, NetAmount = Order.NetAmount, CGSTAmount = Order.CGSTAmount, SGSTAmount = Order.SGSTAmount, IGSTAmount = Order.IGSTAmount, ProductName = prd == null ? string.Empty : prd.ProductName, Discount = Order.Discount, DiscountAmount = Order.DiscountAmount, TotalAmount = Order.TotalAmount, DeliveredQty = Order.DeliveredQty, DiscountAs = Order.DiscountAs, ReturnQty = Order.ReturnQty, ReturnReason = Order.ReturnReason, CustomerId = Order.CustomerId, SerialNoApplicable = prd.SerialNoApplicable, CustomerName = cust == null ? string.Empty : cust.CustomerName, OrderDate = poid.OrderDate, ClosingQuantity = prd.ClosingQuantity }).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

                //int poid = db.orderMain.Where(a => a.OrderNo == OrderNo).Select(a => a.OrderID).Single();
                //var result = db.orderDetails.Where(a => a.OrderID == poid && a.OrderQty != a.DeliveredQty).ToList();
                //return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult getProductName(string ProductCode)
        {
            if (ProductCode == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = db.Products.Where(a => a.ProductCode == ProductCode).FirstOrDefault();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult getInvoiceNo()
        {
            var count = db.BillNumbering.Where(a => a.Type == "NewSales").Select(a => a.Number).Single();          
            var result = string.Format("INV/2023-24/{0:D4}", count);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult getCustomerName(string OrderNo)
        {
            try
            {
                var ID = db.orderMain.Where(a => a.OrderNo == OrderNo).Select(a => a.CustomerID).Single();
                var result = db.Customers.Where(a => a.CustomerID == ID).FirstOrDefault();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }

        }
        public JsonResult save(List<Sales> OrderDetails)
        {
            var billCount = db.BillNumbering.Where(a => a.Type == "NewSales").FirstOrDefault();
            billCount.Number = Convert.ToInt32(billCount.Number) + 1;
            var message = "";
            try
            {   
                if (OrderDetails != null)
                {
                    if (OrderDetails.Count > 0)
                    {
                        foreach (var x in OrderDetails)
                        {
                            var prd = db.Products.Where(a => a.ProductCode == x.ProductCode).FirstOrDefault();
                            List<TempSalesSerialNo> tmpserialNo = db.tempSalesSerialNo.Where(a => a.ProductCode == x.ProductCode).ToList();
                            if (prd.SerialNoApplicable == true)
                            {
                                if (tmpserialNo.Count < x.DeliveredQty)
                                {
                                    message = "Please add serial numbers for " + prd.ProductName + "";
                                    return new JsonResult { Data = new { message = message } };
                                }
                            }  
                        }
                        foreach (var x in OrderDetails)
                        {                          
                            try
                            {
                                var order = db.orderDetails.Where(a => a.OrderDetailsID == x.OrderDetailsID).FirstOrDefault();
                                order.DeliveredQty = (Convert.ToDecimal(order.DeliveredQty) + Convert.ToDecimal(x.DeliveredQty));

                                var prd = db.Products.Where(a => a.ProductCode == x.ProductCode).FirstOrDefault();
                                prd.ClosingQuantity = (Convert.ToDecimal(prd.ClosingQuantity) - Convert.ToDecimal(x.DeliveredQty));
                                prd.OutwardQuantity = (Convert.ToDecimal(prd.OutwardQuantity) + Convert.ToDecimal(x.DeliveredQty));

                                List<TempTable> tmp = db.tempTable.Where(a => a.OrderDetailsID == x.OrderDetailsID).ToList();
                                foreach (var t in tmp)
                                {
                                    var grn = db.GRNDetail.Where(a => a.GRNId == t.GRNId).FirstOrDefault();
                                    grn.SalesQty = grn.SalesQty + t.SalesQty;
                                }

                                List<TempSalesSerialNo> tmpserialNo = db.tempSalesSerialNo.Where(a => a.ProductCode == x.ProductCode).ToList();

                                foreach (var tm in tmpserialNo)
                                {
                                    var prdSerialNo = db.ProductSerialNo.Where(a => a.SerialNoId == tm.SerialNoId).FirstOrDefault();
                                    prdSerialNo.Status = tm.Status;
                                    prdSerialNo.InvoiceNo = x.InvoiceNo;
                                    prdSerialNo.Warrenty = tm.Warrenty;
                                    prdSerialNo.WarrentyDate = tm.WarrentyDate;

                                    var grn = db.GRNDetail.Where(a => a.ProductCode == tm.ProductCode && a.BatchNo == prdSerialNo.BatchNo && a.WarehouseID == tm.WarehouseId && a.StoreLocationId == tm.StoreLocationId).FirstOrDefault();
                                    grn.SalesQty = grn.SalesQty + 1;
                                }

                                var orderno = db.orderDetails.Where(a => a.OrderNo == x.OrderNo).FirstOrDefault();
                               
                                Sales S = new Sales();
                                S.InvoiceNo = x.InvoiceNo;
                                S.InvoiceDate = x.InvoiceDate ?? DateTime.MinValue;
                                S.OrderNo = x.OrderNo;
                                S.ProductCode = x.ProductCode;
                                S.DeliveredQty = x.DeliveredQty;
                                S.CreatedBy = User.Identity.Name;
                                S.BatchNo = x.BatchNo;
                                S.ReturnQty = 0;
                                S.ReplaceQty = 0;
                                S.OrderDetailsID = x.OrderDetailsID;
                                S.SerialNoApplicable = x.SerialNoApplicable;
                                S.CustomerID = x.CustomerID;
                                S.CreatedDate = DateTime.Today;
                                S.CompanyID = Convert.ToInt32(Session["CompanyID"]);
                                //S.SelectSerialFrom = x.SelectSerialFrom;
                                //S.SelectSerialTo = x.SelectSerialTo;
                                S.SODate = orderno.CreatedDate ?? DateTime.MinValue;
                                S.GSTPercentage = x.GSTPercentage;
                                S.BasicRate = x.BasicRate;
                                S.DiscountAs = x.DiscountAs;
                                decimal singalPrice = Convert.ToDecimal(x.BasicRate);
                                string discountAs = x.DiscountAs;
                                decimal Tax = x.GSTPercentage;
                                decimal disc = 0;
                                if (discountAs == "Rupee") { disc = Convert.ToDecimal(x.Discount); S.Discount = disc; } else { disc = (singalPrice * Convert.ToDecimal(x.Discount)) / 100; S.Discount = disc; }
                                var tAmount = singalPrice - disc;
                                var tax = (tAmount * Tax) / 100;
                                if (x.SerialNoApplicable == true) { S.IGSTAmount = tax; } else { var taxAmt = tax / 2; S.CGSTAmount = taxAmt; S.SGSTAmount = taxAmt; }
                                S.AmountPerUnit = tAmount + tax;
                                S.TotalAmount = x.TotalAmount;
                                db.Sales.Add(S);                               
                                db.SaveChanges();
                                message = "success";

                                //List<OrderDetails> pODetails = db.orderDetails.Where(a => a.OrderNo == x.OrderNo).ToList();

                                //bool allSatisfyCondition = true;

                                //foreach (var pomain in pODetails)
                                //{
                                //    if (pomain.DeliveredQty != pomain.OrderQty)
                                //    {
                                //        allSatisfyCondition = false;
                                //        break;
                                //    }
                                //}

                                //if (allSatisfyCondition)
                                //{
                                //    OrderMain pOMain = db.POMains.Where(a => a.PurchaseOrderNo == x.PONo).FirstOrDefault();
                                //    pOMain.POStatus = "Approve";
                                //    db.POMains.AddOrUpdate(pOMain);
                                //    db.SaveChanges();
                                //}

                            }
                            catch (Exception ee)
                            {
                                return new JsonResult { Data = new { message = message } };
                            }

                            var temp = db.tempTable.Where(a => a.OrderDetailsID == x.OrderDetailsID).ToList();
                            foreach (var vp in temp)
                                db.tempTable.Remove(vp);
                            db.SaveChanges();

                            List<TempSalesSerialNo> tmpserial = db.tempSalesSerialNo.Where(a => a.ProductCode == x.ProductCode).ToList();
                            foreach (var tm in tmpserial)
                            {
                                db.tempSalesSerialNo.Remove(tm);
                                db.SaveChanges();
                            }
                            InvoicePrintNew(x.InvoiceNo);
                        }
                        return new JsonResult { Data = new { message = "success" } };
                    }
                }
            }
            catch (Exception ee)
            {
                return new JsonResult { Data = new { message = ee.Message } };
            }
            return new JsonResult { Data = new { message = "success" } };
        }
        public JsonResult ShowGRNDetails(string ProductCode, int OrderDetailsid)
        {
            if (ProductCode == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var id = db.tempTable.Where(a => a.OrderDetailsID == OrderDetailsid).Count();
                if (id == 0)
                {
                    var Productsmaster = new List<Products>(db.Products);
                    var Storesmaster = new List<StoreLocations>(db.StoreLocations);
                    var Warehousemaster = new List<Warehouse>(db.Warehouses);
                    var GRNDetailsMaster = new List<GRNDetails>(db.GRNDetail);
                    var result = (from grn in GRNDetailsMaster.Where(a => a.ProductCode == ProductCode).Where(a => (a.ReceivedQty - a.SalesQty) !=0 )

                                  join Product in Productsmaster on grn.ProductCode equals Product.ProductCode into products
                                  from prd in products.DefaultIfEmpty()

                                  join Warehouse in Warehousemaster on grn.WarehouseID equals Warehouse.WareHouseID into warehouse
                                  from Whouse in warehouse.DefaultIfEmpty()

                                  join StoreLocation in Storesmaster on grn.StoreLocationId equals StoreLocation.StoreLocationId into storeLoc
                                  from store in storeLoc.DefaultIfEmpty()

                                  orderby grn.GRNId descending
                                  select new { GRNId = grn.GRNId, SalesQty = grn.SalesQty, temp = 1, WarehouseID = grn.WarehouseID, StoreLocationId = grn.StoreLocationId, BatchNo = grn.BatchNo, ReceivedQty = grn.ReceivedQty, WareHouseName = Whouse == null ? string.Empty : Whouse.WareHouseName, StoreLocation = store == null ? string.Empty : store.StoreLocation}
                                         ).ToList();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var GRN = new List<GRNDetails>(db.GRNDetail);
                    var Productsmaster = new List<Products>(db.Products);
                    var Storesmaster = new List<StoreLocations>(db.StoreLocations);
                    var Warehousemaster = new List<Warehouse>(db.Warehouses);
                    var TempTableMaster = new List<TempTable>(db.tempTable);
                    var result = (from tmp in TempTableMaster.Where(a => a.ProductCode == ProductCode && a.OrderDetailsID == OrderDetailsid)
                                  join Product in Productsmaster on tmp.ProductCode equals Product.ProductCode into products
                                  from prd in products.DefaultIfEmpty()
                                  join Warehouse in Warehousemaster on tmp.WarehouseID equals Warehouse.WareHouseID into warehouse
                                  from Whouse in warehouse.DefaultIfEmpty()
                                  join StoreLocation in Storesmaster on tmp.StoreLocationId equals StoreLocation.StoreLocationId into storeLoc
                                  from store in storeLoc.DefaultIfEmpty()
                                  join grn in GRN on tmp.GRNId equals grn.GRNId into grn1
                                  from grndetail in grn1.DefaultIfEmpty()
                                  orderby tmp.GRNId descending
                                  select new
                                  {
                                      GRNId = tmp.GRNId,
                                      temp = 0,
                                      SalesQty = tmp.SalesQty,
                                      WarehouseID = tmp.WarehouseID,
                                      StoreLocationId = tmp.StoreLocationId,
                                      BatchNo = tmp.BatchNo,
                                      ReceivedQty = tmp.ReceivedQty,
                                      WareHouseName = Whouse == null ? string.Empty : Whouse.WareHouseName,
                                      //SerialFrom = tmp.SerialFrom,
                                      //SerialTo = tmp.SerialTo,
                                      //SelectSerialFrom = tmp.SelectSerialFrom,
                                      //SelectSerialTo = tmp.SelectSerialTo,
                                      StoreLocation = store == null ? string.Empty : store.StoreLocation
                                  }).ToList();

                    return Json(result, JsonRequestBehavior.AllowGet);
                }
              //  return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult SaveTempDetails(List<Sales> GrnData)
        {
            var status = false;
            int cnt = 1;
            try
            {
                if (GrnData != null)
                {
                    if (GrnData.Count > 0)
                    {
                        foreach (var x in GrnData)
                        {
                            var tmpcnt = db.tempTable.Where(a => a.GRNId == x.GRNId).Count();
                            if (tmpcnt == 0)
                            {
                                try
                                {
                                    int aa = x.GRNId;
                                    TempTable tmp = new TempTable();
                                    tmp.GRNId = x.GRNId;
                                    tmp.OrderDetailsID = x.OrderDetailsID;
                                    tmp.WarehouseID = x.WarehouseID;
                                    tmp.ProductCode = x.ProductCode;
                                    tmp.StoreLocationId = x.StoreLocationId;
                                    tmp.BatchNo = x.BatchNo;
                                    tmp.ReceivedQty = x.ReceivedQty;
                                    tmp.SalesQty = x.SalesQty;
                                    tmp.SelectSerialFrom = x.SelectSerialFrom;
                                    tmp.SelectSerialTo = x.SelectSerialTo;
                                    db.tempTable.Add(tmp);
                                    db.SaveChanges();
                                    status = true;
                                }
                                catch (Exception ee)
                                {
                                    return new JsonResult { Data = new { status = false } };
                                }
                            }
                            else
                            {
                                try
                                {
                                    var grn = db.tempTable.Where(a => a.GRNId == x.GRNId).FirstOrDefault();
                                    grn.GRNId = x.GRNId;
                                    grn.OrderDetailsID = x.OrderDetailsID;
                                    grn.WarehouseID = x.WarehouseID;
                                    grn.ProductCode = x.ProductCode;
                                    grn.StoreLocationId = x.StoreLocationId;
                                    grn.ReceivedQty = x.ReceivedQty;
                                    grn.BatchNo = x.BatchNo;
                                    grn.SalesQty = x.SalesQty;
                                    grn.SelectSerialFrom = x.SelectSerialFrom;
                                    grn.SelectSerialTo = x.SelectSerialTo;
                                    db.SaveChanges();
                                    status = true;
                                }
                                catch (Exception er)
                                {
                                    return new JsonResult { Data = new { status = false } };
                                }
                            }


                        }
                        return new JsonResult { Data = new { status } };
                    }
                }
            }
            catch (Exception ee)
            {
                status = false;
                return new JsonResult { Data = new { status } };
            }
            return new JsonResult { Data = new { status } };
        }
        public JsonResult getSerialNoData(string SerialNo, string ProdCode)
        {


                if (SerialNo == "")
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var Productsmaster = new List<Products>(db.Products);
                    var Storesmaster = new List<StoreLocations>(db.StoreLocations);
                    var Warehousemaster = new List<Warehouse>(db.Warehouses);
                    var Suppliersmasters = new List<Suppliers>(db.suppliers);
                    var ProductSerialNoMaster = new List<ProductSerialNo>(db.ProductSerialNo);
                    var result = (from serialno in ProductSerialNoMaster.Where(a => a.SerialNo == SerialNo && (a.Status == "inward" || a.Status == "Inward" )&& a.ProductCode == ProdCode)

                                  join Product in Productsmaster on serialno.ProductCode equals Product.ProductCode into products
                                  from prd in products.DefaultIfEmpty()

                                  join Warehouse in Warehousemaster on serialno.WarehouseId equals Warehouse.WareHouseID into warehouse
                                  from Whouse in warehouse.DefaultIfEmpty()

                                  join StoreLocation in Storesmaster on serialno.StoreLocationId equals StoreLocation.StoreLocationId into storeLoc
                                  from store in storeLoc.DefaultIfEmpty()

                                  orderby serialno.SerialNoId descending
                                  select new { SerialNoId = serialno.SerialNoId, PODetailsId = serialno.PODetailsId, ProductCode = serialno.ProductCode, BatchNo = serialno.BatchNo, WarehouseId = serialno.WarehouseId, StoreLocationId = serialno.StoreLocationId, GrnNo = serialno.GrnNo, ProductName = prd == null ? string.Empty : prd.ProductName, WareHouseName = Whouse == null ? string.Empty : Whouse.WareHouseName, StoreLocation = store == null ? string.Empty : store.StoreLocation, PONO = serialno.PONO, SerialNo = serialno.SerialNo, Status = serialno.Status, GrnDate = serialno.GrnDate, StandardWarranty = prd.StandardWarranty }
                                         ).Distinct().ToList();
                    return Json(result, JsonRequestBehavior.AllowGet);

                }      
        }
        public JsonResult SaveTempSerialNoDetails(List<ProductSerialNo> TempData,string InvoiceNo) 
        {
            var status = false;
            int cnt = 1;
            try
            {
                if (TempData != null)
                {
                    if (TempData.Count > 0)
                    {
                        foreach (var x in TempData)
                        {
                            var productSRNo = db.ProductSerialNo.Where(a => a.SerialNo == x.SerialNo && a.ProductCode == x.ProductCode).FirstOrDefault();
                          //  if (productSRNo != null)
                                productSRNo.Status = "Sold";
                            

                            var tmpcnt = db.tempSalesSerialNo.Where(a => a.SerialNo == x.SerialNo && a.ProductCode == x.ProductCode).Count();
                            if (tmpcnt == 0)
                            {
                                try
                                {
                                    TempSalesSerialNo tmp = new TempSalesSerialNo();
                                    tmp.ProductCode = x.ProductCode;
                                    tmp.SerialNo = x.SerialNo;
                                    tmp.WarehouseId = x.WarehouseId;
                                    tmp.StoreLocationId = x.StoreLocationId;
                                    tmp.Status = "Sale";
                                    tmp.SerialNoId = x.SerialNoId;
                                    tmp.PODetailsId = x.PODetailsId;
                                    tmp.BatchNo = x.BatchNo;
                                    tmp.PONO = x.PONO;
                                    tmp.GrnNo = x.GrnNo;
                                    tmp.InvoiceNo = InvoiceNo;
                                    tmp.Warrenty = x.Warrenty;
                                    tmp.WarrentyDate = x.WarrentyDate;
                                    db.tempSalesSerialNo.Add(tmp);
                                    db.SaveChanges();
                                    status = true;
                                }
                                catch (Exception ee)
                                {
                                    return new JsonResult { Data = new { status = false } };
                                }
                            }
                            else
                            {
                                try
                                {
                                    var tmp = db.tempSalesSerialNo.Where(a => a.SerialNo == x.SerialNo && a.ProductCode == x.ProductCode).FirstOrDefault();
                                    tmp.ProductCode = x.ProductCode;
                                    tmp.SerialNo = x.SerialNo;
                                    tmp.Warrenty = x.Warrenty;
                                    tmp.WarrentyDate = x.WarrentyDate;
                                    tmp.StoreLocationId = x.StoreLocationId;
                                    tmp.Status = "Sold";
                                    db.SaveChanges();
                                    status = true;
                                }
                                catch (Exception er)
                                {
                                    return new JsonResult { Data = new { status = false } };
                                }
                            }
                        }
                        return new JsonResult { Data = new { status } };
                    }
                }
            }
            catch (Exception ee)
            {
                status = false;
                return new JsonResult { Data = new { status } };
            }
            return new JsonResult { Data = new { status } };
        }
        public JsonResult ShowTempSerialNo(string ProductCode, string InvoiceNo)
        {
            if (ProductCode == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var Productsmaster = new List<Products>(db.Products);
                var Storesmaster = new List<StoreLocations>(db.StoreLocations);
                var Warehousemaster = new List<Warehouse>(db.Warehouses);
                var tempSalesSerialNo = new List<TempSalesSerialNo>(db.tempSalesSerialNo);
                var result = (from tmp in tempSalesSerialNo.Where(a => a.ProductCode == ProductCode && a.InvoiceNo == InvoiceNo)

                              join Product in Productsmaster on tmp.ProductCode equals Product.ProductCode into products
                              from prd in products.DefaultIfEmpty()

                              join Warehouse in Warehousemaster on tmp.WarehouseId equals Warehouse.WareHouseID into warehouse
                              from Whouse in warehouse.DefaultIfEmpty()

                              join StoreLocation in Storesmaster on tmp.StoreLocationId equals StoreLocation.StoreLocationId into storeLoc
                              from store in storeLoc.DefaultIfEmpty()

                              orderby tmp.TempReturnId descending
                              select new { TempReturnId = tmp.TempReturnId, temp = 0, WarehouseID = tmp.WarehouseId, StoreLocationId = tmp.StoreLocationId, BatchNo = tmp.BatchNo, WareHouseName = Whouse == null ? string.Empty : Whouse.WareHouseName, StoreLocation = store == null ? string.Empty : store.StoreLocation, ProductName = prd == null ? string.Empty : prd.ProductName, ProductCode = tmp.ProductCode, PONO = tmp.PONO, GrnNo = tmp.GrnNo, PODetailsId = tmp.PODetailsId, SerialNoId = tmp.SerialNoId, SerialNo = tmp.SerialNo }
                                     ).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        //public JsonResult InvoicePrintNew(string InvoiceNo)
        //{
        //    try
        //    {

        //        var IGSTAmount = 0;
        //        decimal CGSTAmount = 0;
        //        decimal SGSTAmount = 0;
        //        decimal TotalAmount = 0;
        //        var NetAmount = 0;
        //        decimal Discount = 0;
        //        var orderno = InvoiceNo;
        //        //   InvoiceNo = db.Sales.Where(a => a.OrderNo == InvoiceNo).Select(a => a.InvoiceNo).FirstOrDefault();
        //        DateTime dt = DateTime.Now;

        //        var aa = dt.ToString("HH");
        //        var bb = dt.ToString("mm");
        //        var cc = dt.ToString("ss");



        //        Document document = new Document(new Rectangle(288f, 144f), 10, 10, 10, 10);
        //        document.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());

        //        //   Document document = new Document(PageSize.A4.Rotate(), 20f, 10f, 20f, 10f);
        //        string path = Server.MapPath("~/Reports/Invoice/");
        //        string[] parts = InvoiceNo.Split('/');
        //        string numberString = parts[2];
        //        var filename = numberString + ".pdf";
        //        string filename1 = path + "" + numberString + "_" + aa + bb + cc + ".pdf";
        //        PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filename1, FileMode.Create));
        //        document.Open();
        //        document.AddTitle(InvoiceNo); //Statement-1
        //        document.AddHeader("content-disposition", "inline;filename=" + InvoiceNo + ".pdf");
        //        Session["fileName1"] = filename1;
        //        Session["filename"] = filename;

        //        int Count = 1;



        //        var data11 = (from c in db.Sales.Where(a => a.InvoiceNo == InvoiceNo)

        //                      join orderMain in db.orderMain on c.OrderNo equals orderMain.OrderNo into mainordeer
        //                      from om in mainordeer.DefaultIfEmpty()

        //                      join prdd in db.Products on c.ProductCode equals prdd.ProductCode into Products
        //                      from prd in Products.DefaultIfEmpty()

        //                      join GRNDe in db.GRNDetail on c.GRN_ID equals GRNDe.GRNId into GRNDetail
        //                      from grn in GRNDetail.DefaultIfEmpty()

        //                      select new
        //                      {
        //                          OrderNo = om.OrderNo,
        //                          OrderDate = om.OrderDate,

        //                          InvoiceDate = c.InvoiceDate,
        //                          DeliveredQty = c.DeliveredQty,
        //                          Address = om.CustomerAddress,
        //                          ManufacturingDate = grn.ManufacturingDate,
        //                          ExpiryDate = grn.ExpiryDate,
        //                          City = om.CustomerCity,
        //                          Country = "India",
        //                          State = "Maharashtra",
        //                          Pincode = om.CustomerPincode,
        //                          Mobile = om.CustomerMobile,
        //                          GSTNO = om.CustomerGSTNo,
        //                          CustomerName = om.CustomerName,
        //                          BasicRate = c.BasicRate,
        //                          Discount = c.Discount,
        //                          GSTPercentage = c.GSTPercentage,
        //                          IGSTAmount = c.IGSTAmount,
        //                          SGSTAmount = c.SGSTAmount,
        //                          CGSTAmount = c.CGSTAmount,
        //                          TotalAmount = c.TotalAmount,
        //                          BatchNo = c.BatchNo,
        //                          NetAmount = c.BasicRate * c.DeliveredQty,
        //                          ProductName = prd.ProductName,
        //                          ProductCode = prd.ProductCode,
        //                          HsnCode = prd.HsnCode,
        //                          GSTPer = prd.GSTPer,
        //                          Size = prd.Size,


        //                      }).ToList();


        //        ;
        //        var data =
        //        (from c in data11//.Where(a => a.CustomerName != null)
        //         group c by new
        //         {
        //             c.OrderNo,
        //             c.OrderDate,
        //             c.InvoiceDate,
        //             c.DeliveredQty,
        //             c.Address,
        //             c.ManufacturingDate,
        //             //  c.ExpiryDate,
        //             c.City,
        //             c.Country,
        //             c.State,
        //             c.Pincode,
        //             c.Mobile,
        //             c.GSTNO,
        //             c.CustomerName,
        //             c.BasicRate,
        //             c.Discount,
        //             c.GSTPercentage,
        //             c.IGSTAmount,
        //             c.SGSTAmount,
        //             c.CGSTAmount,
        //             c.TotalAmount,
        //             c.BatchNo,
        //             c.NetAmount,
        //             c.ProductName,
        //             c.ProductCode,
        //             c.HsnCode,
        //             c.GSTPer,
        //             c.Size
        //         } into gcs
        //         select new
        //         {

        //             gcs.Key.OrderNo,
        //             gcs.Key.OrderDate,
        //             //gcs.Key.InvoiceDate,
        //             gcs.Key.DeliveredQty,
        //             gcs.Key.Address,
        //             ExpiryDate = gcs.Max(a => a.ExpiryDate),
        //             InvoiceDate = gcs.Max(a => a.InvoiceDate),
        //             gcs.Key.City,
        //             gcs.Key.Country,
        //             gcs.Key.State,
        //             gcs.Key.Pincode,
        //             gcs.Key.Mobile,
        //             gcs.Key.GSTNO,
        //             gcs.Key.CustomerName,
        //             gcs.Key.BasicRate,
        //             gcs.Key.Discount,
        //             gcs.Key.GSTPercentage,
        //             gcs.Key.IGSTAmount,
        //             gcs.Key.SGSTAmount,
        //             gcs.Key.CGSTAmount,
        //             gcs.Key.TotalAmount,
        //             gcs.Key.BatchNo,
        //             gcs.Key.NetAmount,
        //             gcs.Key.ProductName,
        //             gcs.Key.ProductCode,
        //             gcs.Key.HsnCode,
        //             gcs.Key.GSTPer,
        //             gcs.Key.Size

        //         }).ToList();



        //        PdfPTable table0 = new PdfPTable(4);
        //        float[] widths0 = new float[] { 10f, 10f, 10f, 10f };
        //        table0.SetWidths(widths0);
        //        table0.WidthPercentage = 98;
        //        table0.HorizontalAlignment = 1;




        //        Paragraph para1 = new Paragraph();
        //        para1.Add(new Phrase("TAX INVOICE", FontFactory.GetFont("Arial", 12, Font.BOLD)));
        //        para1.Add(new Phrase("\n\n\n", FontFactory.GetFont("Arial", 5, Font.BOLD)));
        //        PdfPCell Cell101 = new PdfPCell(para1);
        //        Cell101.Border = Rectangle.NO_BORDER;
        //        Cell101.Colspan = 4;
        //        Cell101.HorizontalAlignment = 1;
        //        table0.AddCell(Cell101);

        //        try
        //        {
        //            Paragraph para2 = new Paragraph();
        //            para2.Add(new Phrase("Invoice No:", FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //            PdfPCell Cell102 = new PdfPCell(para2);
        //            Cell102.HorizontalAlignment = 0;
        //            table0.AddCell(Cell102);


        //            Paragraph para3 = new Paragraph();
        //            para3.Add(new Phrase("" + InvoiceNo, FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //            PdfPCell Cell103 = new PdfPCell(para3);
        //            Cell103.HorizontalAlignment = 0;
        //            table0.AddCell(Cell103);


        //            Paragraph para4 = new Paragraph();
        //            para4.Add(new Phrase("L.R. No.:", FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //            PdfPCell Cell104 = new PdfPCell(para4);
        //            Cell104.HorizontalAlignment = 0;
        //            table0.AddCell(Cell104);


        //            Paragraph para5 = new Paragraph();
        //            para5.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //            PdfPCell Cell105 = new PdfPCell(para5);
        //            Cell105.HorizontalAlignment = 0;
        //            table0.AddCell(Cell105);

        //        }
        //        catch { }
        //        try
        //        {
        //            Paragraph para2 = new Paragraph();
        //            para2.Add(new Phrase("Invoice Date:", FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //            PdfPCell Cell102 = new PdfPCell(para2);
        //            Cell102.HorizontalAlignment = 0;
        //            table0.AddCell(Cell102);
        //            var XYZ = data[0].InvoiceDate.HasValue ? data[0].InvoiceDate.Value.ToString("dd-MM-yyyy") : "";
        //            //var XYZ = data[0]?.InvoiceDate?.Value.ToString("dd-MM-yyyy") ?? "";


        //            Paragraph para3 = new Paragraph();
        //            para3.Add(new Phrase("" + XYZ, FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //            PdfPCell Cell103 = new PdfPCell(para3);
        //            Cell103.HorizontalAlignment = 0;
        //            table0.AddCell(Cell103);


        //            Paragraph para4 = new Paragraph();
        //            para4.Add(new Phrase("L.R. Date:", FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //            PdfPCell Cell104 = new PdfPCell(para4);
        //            Cell104.HorizontalAlignment = 0;
        //            table0.AddCell(Cell104);


        //            Paragraph para5 = new Paragraph();
        //            para5.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //            PdfPCell Cell105 = new PdfPCell(para5);
        //            Cell105.HorizontalAlignment = 0;
        //            table0.AddCell(Cell105);

        //        }
        //        catch { }
        //        try
        //        {
        //            Paragraph para2 = new Paragraph();
        //            para2.Add(new Phrase("Order No:", FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //            PdfPCell Cell102 = new PdfPCell(para2);
        //            Cell102.HorizontalAlignment = 0;
        //            table0.AddCell(Cell102);


        //            Paragraph para3 = new Paragraph();
        //            para3.Add(new Phrase("" + data[0].OrderNo, FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //            PdfPCell Cell103 = new PdfPCell(para3);
        //            Cell103.HorizontalAlignment = 0;
        //            table0.AddCell(Cell103);


        //            Paragraph para4 = new Paragraph();
        //            para4.Add(new Phrase("Cases:", FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //            PdfPCell Cell104 = new PdfPCell(para4);
        //            Cell104.HorizontalAlignment = 0;
        //            table0.AddCell(Cell104);


        //            Paragraph para5 = new Paragraph();
        //            para5.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //            PdfPCell Cell105 = new PdfPCell(para5);
        //            Cell105.HorizontalAlignment = 0;
        //            table0.AddCell(Cell105);

        //        }
        //        catch { }
        //        try
        //        {
        //            Paragraph para2 = new Paragraph();
        //            para2.Add(new Phrase("Order Date:", FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //            PdfPCell Cell102 = new PdfPCell(para2);
        //            Cell102.HorizontalAlignment = 0;
        //            table0.AddCell(Cell102);


        //            var XYZZ = "";
        //            try
        //            {
        //                XYZZ = data[0].OrderDate.Value.ToString("dd-MM-yyyy");
        //            }
        //            catch
        //            {

        //            }


        //            Paragraph para3 = new Paragraph();
        //            para3.Add(new Phrase("" + XYZZ, FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //            PdfPCell Cell103 = new PdfPCell(para3);
        //            Cell103.HorizontalAlignment = 0;
        //            table0.AddCell(Cell103);


        //            Paragraph para4 = new Paragraph();
        //            para4.Add(new Phrase("Due Date:", FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //            PdfPCell Cell104 = new PdfPCell(para4);
        //            Cell104.HorizontalAlignment = 0;
        //            table0.AddCell(Cell104);


        //            Paragraph para5 = new Paragraph();
        //            para5.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //            PdfPCell Cell105 = new PdfPCell(para5);
        //            Cell105.HorizontalAlignment = 0;
        //            table0.AddCell(Cell105);

        //        }
        //        catch { }
        //        try
        //        {
        //            Paragraph para2 = new Paragraph();
        //            para2.Add(new Phrase("Transport :-", FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //            PdfPCell Cell102 = new PdfPCell(para2);
        //            Cell102.HorizontalAlignment = 0;
        //            Cell102.Colspan = 2;
        //            Cell102.Border = Rectangle.NO_BORDER;
        //            table0.AddCell(Cell102);


        //            Paragraph para3 = new Paragraph();
        //            para3.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //            PdfPCell Cell103 = new PdfPCell(para3);
        //            Cell103.HorizontalAlignment = 0;
        //            Cell103.Colspan = 2;
        //            Cell103.Border = Rectangle.NO_BORDER;
        //            table0.AddCell(Cell103);



        //        }
        //        catch { }
        //        try
        //        {
        //            Paragraph para2 = new Paragraph();
        //            para2.Add(new Phrase("Dispatch date and time :-", FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //            PdfPCell Cell102 = new PdfPCell(para2);
        //            Cell102.HorizontalAlignment = 0;
        //            Cell102.Colspan = 2;
        //            Cell102.Border = Rectangle.NO_BORDER;
        //            table0.AddCell(Cell102);


        //            Paragraph para3 = new Paragraph();
        //            para3.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //            PdfPCell Cell103 = new PdfPCell(para3);
        //            Cell103.HorizontalAlignment = 0;
        //            Cell103.Colspan = 2;
        //            Cell103.Border = Rectangle.NO_BORDER;
        //            table0.AddCell(Cell103);



        //        }
        //        catch { }
        //        try
        //        {
        //            Paragraph para2 = new Paragraph();
        //            para2.Add(new Phrase("Weight :-", FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //            PdfPCell Cell102 = new PdfPCell(para2);
        //            Cell102.HorizontalAlignment = 0;
        //            Cell102.Colspan = 2;
        //            Cell102.Border = Rectangle.NO_BORDER;
        //            table0.AddCell(Cell102);


        //            Paragraph para3 = new Paragraph();
        //            para3.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //            PdfPCell Cell103 = new PdfPCell(para3);
        //            Cell103.HorizontalAlignment = 0;
        //            Cell103.Colspan = 2;
        //            Cell103.Border = Rectangle.NO_BORDER;
        //            table0.AddCell(Cell103);



        //        }
        //        catch { }
        //        try
        //        {
        //            Paragraph para2 = new Paragraph();
        //            para2.Add(new Phrase("Delivery :-", FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //            PdfPCell Cell102 = new PdfPCell(para2);
        //            Cell102.HorizontalAlignment = 0;
        //            Cell102.Colspan = 2;
        //            Cell102.Border = Rectangle.NO_BORDER;
        //            table0.AddCell(Cell102);


        //            Paragraph para3 = new Paragraph();
        //            para3.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //            PdfPCell Cell103 = new PdfPCell(para3);
        //            Cell103.HorizontalAlignment = 0;
        //            Cell103.Colspan = 2;
        //            Cell103.Border = Rectangle.NO_BORDER;
        //            table0.AddCell(Cell103);



        //        }
        //        catch { }
        //        try
        //        {
        //            Paragraph para2 = new Paragraph();
        //            para2.Add(new Phrase("ORDER DATE :-", FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //            PdfPCell Cell102 = new PdfPCell(para2);
        //            Cell102.HorizontalAlignment = 0;
        //            Cell102.Colspan = 2;
        //            Cell102.Border = Rectangle.NO_BORDER;
        //            table0.AddCell(Cell102);


        //            Paragraph para3 = new Paragraph();
        //            para3.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //            PdfPCell Cell103 = new PdfPCell(para3);
        //            Cell103.HorizontalAlignment = 0;
        //            Cell103.Colspan = 2;
        //            Cell103.Border = Rectangle.NO_BORDER;
        //            table0.AddCell(Cell103);

        //        }
        //        catch { }


        //        PdfPTable table1 = new PdfPTable(6);
        //        float[] widths1 = new float[] { 3f, 3f, 4f, 4f, 3f, 3f };
        //        table1.SetWidths(widths1);
        //        table1.WidthPercentage = 98;
        //        table1.HorizontalAlignment = 1;

        //        // string imageURL = Server.MapPath("~/img/logo.png");
        //        string imageURL = Server.MapPath("/Photo/MicraftLogo.png");
        //        iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(imageURL);
        //        jpg.ScaleToFit(70, 100);
        //        jpg.SpacingBefore = 1f;
        //        jpg.SpacingAfter = 1f;
        //        jpg.Alignment = Element.ALIGN_LEFT;

        //        Paragraph P117585 = new Paragraph();
        //        P117585.Add(new Chunk(jpg, 0, 0, true));
        //        P117585.Add(new Phrase("\n", FontFactory.GetFont("Arial", 5, Font.BOLD)));
        //        P117585.Add(new Phrase("Siddhivinayak Distributor", FontFactory.GetFont("Arial", 12, Font.BOLD)));
        //        P117585.Add(new Phrase("\n", FontFactory.GetFont("Arial", 5, Font.BOLD)));
        //        P117585.Add(new Phrase("Shop no 10, Suyog Navkar Building A, \n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        P117585.Add(new Phrase("Near 7 Loves Chowk, Market Yard Road, Pune 411 037.\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        //P117585.Add(new Phrase("Tel. No.: 020-79629339\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        //P117585.Add(new Phrase("D.L. No.: 20B-490622, 21B-490623\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        P117585.Add(new Phrase("GSTIN: 27ABVPK5495R2Z9\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        P117585.Add(new Phrase("DL.No: MH-PZ3517351,MH-PZ3517352,\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        //    P117585.Add(new Phrase("E-Mail : aloeshellpharmaa@gmail.com\n\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));

        //        PdfPCell Cell1 = new PdfPCell(P117585);
        //        Cell1.Colspan = 2;
        //        Cell1.HorizontalAlignment = 0;
        //        table1.AddCell(Cell1);


        //        PdfPCell Cell2 = new PdfPCell(table0);
        //        Cell2.HorizontalAlignment = 0;

        //        Cell2.Colspan = 2;

        //        table1.AddCell(Cell2);
        //        var address = data[0].Address + ", " + data[0].City + ", " + data[0].Pincode + " " + data[0].State + " " + data[0].Country;

        //        Paragraph Para3 = new Paragraph();
        //        Para3.Add(new Phrase("Party Name: " + data[0].CustomerName + "\n\n", FontFactory.GetFont("Arial", 9, Font.BOLD)));

        //        Para3.Add(new Phrase("Address : ", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        Para3.Add(new Phrase("" + address + "\n", FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //        Para3.Add(new Phrase("Mobile No :" + data[0].Mobile + "\n", FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //        Para3.Add(new Phrase("GST NO :" + data[0].GSTNO + "\n", FontFactory.GetFont("Arial", 9, Font.BOLD)));

        //        PdfPCell Cell3 = new PdfPCell(Para3);
        //        Cell3.HorizontalAlignment = 0;
        //        Cell3.Colspan = 2;
        //        table1.AddCell(Cell3);



        //        PdfPTable table2 = new PdfPTable(16);
        //        float[] widths2 = new float[] { 2f, 8f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 4f };
        //        table2.SetWidths(widths2);
        //        table2.WidthPercentage = 98;
        //        table2.HorizontalAlignment = 1;

        //        //3rd Row

        //        PdfPCell Cell8 = new PdfPCell(new Phrase(new Phrase("Sr.No.", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //        Cell8.HorizontalAlignment = 1;
        //        Cell8.BackgroundColor = BaseColor.BLACK;
        //        table2.AddCell(Cell8);

        //        PdfPCell Cell9 = new PdfPCell(new Phrase(new Phrase("Product Name", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //        Cell9.HorizontalAlignment = 1;
        //        Cell9.BackgroundColor = BaseColor.BLACK;
        //        table2.AddCell(Cell9);

        //        PdfPCell Cell10 = new PdfPCell(new Phrase(new Phrase("Pack", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //        Cell10.HorizontalAlignment = 1;
        //        Cell10.BackgroundColor = BaseColor.BLACK;
        //        table2.AddCell(Cell10);

        //        PdfPCell Cell11 = new PdfPCell(new Phrase(new Phrase("Mfr", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //        Cell11.HorizontalAlignment = 1;
        //        Cell11.BackgroundColor = BaseColor.BLACK;
        //        table2.AddCell(Cell11);

        //        PdfPCell Cell12 = new PdfPCell(new Phrase(new Phrase("Batch", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //        Cell12.HorizontalAlignment = 1;
        //        Cell12.BackgroundColor = BaseColor.BLACK;
        //        table2.AddCell(Cell12);

        //        PdfPCell Cell13 = new PdfPCell(new Phrase(new Phrase("Box", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //        Cell13.HorizontalAlignment = 1;
        //        Cell13.BackgroundColor = BaseColor.BLACK;
        //        table2.AddCell(Cell13);


        //        PdfPCell Cel4l11 = new PdfPCell(new Phrase(new Phrase("Tot Qty", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //        Cel4l11.HorizontalAlignment = 1;
        //        Cel4l11.BackgroundColor = BaseColor.BLACK;
        //        table2.AddCell(Cel4l11);

        //        PdfPCell Cells12 = new PdfPCell(new Phrase(new Phrase("Exp Dt", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //        Cells12.HorizontalAlignment = 1;
        //        Cells12.BackgroundColor = BaseColor.BLACK;
        //        table2.AddCell(Cells12);

        //        PdfPCell Cell513 = new PdfPCell(new Phrase(new Phrase("HSN", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //        Cell513.HorizontalAlignment = 1;
        //        Cell513.BackgroundColor = BaseColor.BLACK;
        //        table2.AddCell(Cell513);

        //        PdfPCell Cel7l13 = new PdfPCell(new Phrase(new Phrase("MRP", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //        Cel7l13.HorizontalAlignment = 1;
        //        Cel7l13.BackgroundColor = BaseColor.BLACK;
        //        table2.AddCell(Cel7l13);

        //        PdfPCell Cellsw13 = new PdfPCell(new Phrase(new Phrase("Rate", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //        Cellsw13.HorizontalAlignment = 1;
        //        Cellsw13.BackgroundColor = BaseColor.BLACK;
        //        table2.AddCell(Cellsw13);

        //        PdfPCell Cell1543 = new PdfPCell(new Phrase(new Phrase("SGST", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //        Cell1543.HorizontalAlignment = 1;
        //        Cell1543.BackgroundColor = BaseColor.BLACK;
        //        table2.AddCell(Cell1543);

        //        PdfPCell Cellsa13 = new PdfPCell(new Phrase(new Phrase("Value", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //        Cellsa13.HorizontalAlignment = 1;
        //        Cellsa13.BackgroundColor = BaseColor.BLACK;
        //        table2.AddCell(Cellsa13);

        //        PdfPCell Cell15d43 = new PdfPCell(new Phrase(new Phrase("CGST", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //        Cell15d43.HorizontalAlignment = 1;
        //        Cell15d43.BackgroundColor = BaseColor.BLACK;
        //        table2.AddCell(Cell15d43);

        //        PdfPCell Cells55a13 = new PdfPCell(new Phrase(new Phrase("Value", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //        Cells55a13.HorizontalAlignment = 1;
        //        Cells55a13.BackgroundColor = BaseColor.BLACK;
        //        table2.AddCell(Cells55a13);

        //        PdfPCell Cellsd13 = new PdfPCell(new Phrase(new Phrase("Amount", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //        Cellsd13.HorizontalAlignment = 1;
        //        Cellsd13.BackgroundColor = BaseColor.BLACK;
        //        table2.AddCell(Cellsd13);


        //        //4th Row

        //        foreach (var r in data)
        //        {
        //            if (data.Count > 0)
        //            {
        //                var order = db.orderDetails.Where(a => a.OrderNo == r.OrderNo && a.ProductCode == r.ProductCode).FirstOrDefault();
        //                Paragraph Para14 = new Paragraph();
        //                Para14.Add(new Phrase("" + Count + "\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //                PdfPCell Cell14 = new PdfPCell(Para14);
        //                Cell14.HorizontalAlignment = 1;
        //                Cell14.UseAscender = true;
        //                table2.AddCell(Cell14);


        //                Paragraph Para15 = new Paragraph();
        //                Para15.Add(new Phrase("" + r.ProductName + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //                PdfPCell Cell15 = new PdfPCell(Para15);
        //                Cell15.HorizontalAlignment = 0;
        //                table2.AddCell(Cell15);


        //                //PACK
        //                Paragraph Para16 = new Paragraph();
        //                Para16.Add(new Phrase("" + r.Size + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //                PdfPCell Cell16 = new PdfPCell(Para16);
        //                Cell16.HorizontalAlignment = 1;
        //                table2.AddCell(Cell16);

        //                //MFR


        //                var XYZZ = "";
        //                try
        //                {
        //                    //   XYZZ = r.ManufacturingDate.Value.ToString("dd-MM-yyyy");
        //                }
        //                catch
        //                {

        //                }


        //                Paragraph Para1d6 = new Paragraph();
        //                Para1d6.Add(new Phrase("" + XYZZ, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //                PdfPCell Cells16 = new PdfPCell(Para1d6);
        //                Cells16.HorizontalAlignment = 1;
        //                table2.AddCell(Cells16);

        //                //batck
        //                Paragraph Para16s = new Paragraph();
        //                Para16s.Add(new Phrase("" + r.BatchNo, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //                PdfPCell Celsl16 = new PdfPCell(Para16s);
        //                Celsl16.HorizontalAlignment = 1;
        //                table2.AddCell(Celsl16);


        //                //box
        //                Paragraph Paradd16 = new Paragraph();
        //                Paradd16.Add(new Phrase("Box", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //                PdfPCell Cell1e6 = new PdfPCell(Paradd16);
        //                Cell1e6.HorizontalAlignment = 1;
        //                table2.AddCell(Cell1e6);


        //                Paragraph Parafd16 = new Paragraph();
        //                Parafd16.Add(new Phrase("" + r.DeliveredQty + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //                PdfPCell Cell1df6 = new PdfPCell(Parafd16);
        //                Cell1df6.HorizontalAlignment = 1;
        //                table2.AddCell(Cell1df6);

        //                var XYZZZ = "";
        //                try
        //                {
        //                    XYZZZ = r.ExpiryDate.Value.ToString("dd-MM-yyyy");
        //                }
        //                catch { }


        //                //expdate
        //                Paragraph Para1r6 = new Paragraph();
        //                Para1r6.Add(new Phrase("" + XYZZZ + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //                PdfPCell Para1sr6 = new PdfPCell(Para1r6);
        //                Para1sr6.HorizontalAlignment = 1;
        //                table2.AddCell(Para1sr6);


        //                //hsn

        //                Paragraph Para17 = new Paragraph();
        //                Para17.Add(new Phrase("" + r.HsnCode + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //                PdfPCell Cell17 = new PdfPCell(Para17);
        //                Cell17.HorizontalAlignment = 1;
        //                table2.AddCell(Cell17);



        //                //mrp

        //                Paragraph Para1757 = new Paragraph();
        //                Para1757.Add(new Phrase("" + order.Price, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //                PdfPCell Cellee17 = new PdfPCell(Para1757);
        //                Cellee17.HorizontalAlignment = 1;
        //                table2.AddCell(Cellee17);

        //                Discount = Discount + (Convert.ToDecimal(order.DiscountAmount));

        //                Paragraph Para1sd7 = new Paragraph();
        //                Para1sd7.Add(new Phrase("" + order.Price, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //                PdfPCell Cell1sds7 = new PdfPCell(Para1sd7);
        //                Cell1sds7.HorizontalAlignment = 1;
        //                table2.AddCell(Cell1sds7);


        //                var gst = r.GSTPer / 2;

        //                //SGST
        //                Paragraph Para18 = new Paragraph();
        //                Para18.Add(new Phrase(gst + "%", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //                PdfPCell Cell18 = new PdfPCell(Para18);
        //                Cell18.HorizontalAlignment = 1;
        //                table2.AddCell(Cell18);


        //                // value
        //                Paragraph Paradf18 = new Paragraph();
        //                Paradf18.Add(new Phrase("" + string.Format("{0:0.00 }", order.CGSTAmount) + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //                PdfPCell Parabdf18 = new PdfPCell(Paradf18);
        //                Parabdf18.HorizontalAlignment = 1;
        //                table2.AddCell(Parabdf18);

        //                CGSTAmount = CGSTAmount + (Convert.ToDecimal(order.CGSTAmount));

        //                //CGST
        //                Paragraph Pararer18 = new Paragraph();
        //                Pararer18.Add(new Phrase(gst + "%", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //                PdfPCell Cell18eds = new PdfPCell(Pararer18);
        //                Cell18eds.HorizontalAlignment = 1;
        //                table2.AddCell(Cell18eds);


        //                // value
        //                Paragraph Para18445 = new Paragraph();
        //                Para18445.Add(new Phrase("" + string.Format("{0:0.00 }", order.SGSTAmount) + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //                PdfPCell Cell18787 = new PdfPCell(Para18445);
        //                Cell18787.HorizontalAlignment = 1;
        //                table2.AddCell(Cell18787);

        //                SGSTAmount = SGSTAmount + (Convert.ToDecimal(order.SGSTAmount));

        //                var TotAmt = order.Price * order.DeliveredQty;


        //                Paragraph Parjhja19 = new Paragraph();
        //                Parjhja19.Add(new Phrase("" + string.Format("{0:0.00 }", TotAmt) + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //                PdfPCell Cell19 = new PdfPCell(Parjhja19);
        //                Cell19.HorizontalAlignment = 2;
        //                table2.AddCell(Cell19);

        //                TotalAmount = Convert.ToDecimal(TotalAmount) + Convert.ToDecimal(TotAmt);
        //                TotAmt = 0;
        //                Count++;
        //            }
        //        }



        //        PdfPTable table3 = new PdfPTable(10);
        //        float[] widths3 = new float[] { 3f, 3f, 3f, 3f, 3f, 3f, 3f, 5f, 3f, 3f };
        //        table3.SetWidths(widths3);
        //        table3.WidthPercentage = 98;
        //        table3.HorizontalAlignment = 1;





        //        var GstData =
        //        (from c in db.orderDetails.Where(a => a.OrderNo == orderno)
        //         group c by new
        //         {
        //             c.GSTPercentage,
        //         } into gcs
        //         select new
        //         {
        //             SGSTAmount = gcs.Sum(a => a.SGSTAmount),
        //             CGSTAmount = gcs.Sum(a => a.CGSTAmount),
        //             TotalTAxAmount = gcs.Sum(a => a.CGSTAmount + a.SGSTAmount),
        //             GSTPer = gcs.Key.GSTPercentage
        //         }).ToList();



        //        try
        //        {
        //            PdfPCell Cellsa13121 = new PdfPCell(new Phrase(new Phrase("Class", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.WHITE))));
        //            Cellsa13121.HorizontalAlignment = 1;
        //            Cellsa13121.BackgroundColor = BaseColor.BLACK;
        //            table3.AddCell(Cellsa13121);


        //            PdfPCell Cellsa13122 = new PdfPCell(new Phrase(new Phrase("TOTAL", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.WHITE))));
        //            Cellsa13122.HorizontalAlignment = 1;
        //            Cellsa13122.BackgroundColor = BaseColor.BLACK;
        //            table3.AddCell(Cellsa13122);

        //            PdfPCell Cellsa13123 = new PdfPCell(new Phrase(new Phrase("SCHEME", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.WHITE))));
        //            Cellsa13123.HorizontalAlignment = 1;
        //            Cellsa13123.BackgroundColor = BaseColor.BLACK;
        //            table3.AddCell(Cellsa13123);

        //            PdfPCell Cellsa13124 = new PdfPCell(new Phrase(new Phrase("DISCOUNT", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.WHITE))));
        //            Cellsa13124.HorizontalAlignment = 1;
        //            Cellsa13124.BackgroundColor = BaseColor.BLACK;
        //            table3.AddCell(Cellsa13124);

        //            PdfPCell Cellsa13125 = new PdfPCell(new Phrase(new Phrase("SGST", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.WHITE))));
        //            Cellsa13125.HorizontalAlignment = 1;
        //            Cellsa13125.BackgroundColor = BaseColor.BLACK;
        //            table3.AddCell(Cellsa13125);

        //            PdfPCell Cellsa13126 = new PdfPCell(new Phrase(new Phrase("CGST", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.WHITE))));
        //            Cellsa13126.HorizontalAlignment = 1;
        //            Cellsa13126.BackgroundColor = BaseColor.BLACK;
        //            table3.AddCell(Cellsa13126);

        //            PdfPCell Cellsa13127 = new PdfPCell(new Phrase(new Phrase("TOTAL GST", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.WHITE))));
        //            Cellsa13127.HorizontalAlignment = 1;
        //            Cellsa13127.BackgroundColor = BaseColor.BLACK;
        //            table3.AddCell(Cellsa13127);

        //            PdfPCell Cellsa13128 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //            Cellsa13128.HorizontalAlignment = 1;
        //            Cellsa13128.BackgroundColor = BaseColor.BLACK;
        //            table3.AddCell(Cellsa13128);

        //            PdfPCell Cellsa131200 = new PdfPCell(new Phrase(new Phrase("" + string.Format("{0:0.00 }", TotalAmount), FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //            Cellsa131200.HorizontalAlignment = 2;
        //            Cellsa131200.BackgroundColor = BaseColor.BLACK;
        //            Cellsa131200.Colspan = 2;
        //            table3.AddCell(Cellsa131200);


        //        }
        //        catch { }

        //        var GSt_5 = GstData.Where(a => a.GSTPer == 5).FirstOrDefault();
        //        if (GSt_5 != null)
        //        {
        //            try
        //            {
        //                PdfPCell Cellsa13121 = new PdfPCell(new Phrase(new Phrase("GST: 5.00%", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13121.HorizontalAlignment = 1;
        //                Cellsa13121.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13121.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13121);


        //                PdfPCell Cellsa13122 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13122.HorizontalAlignment = 1;
        //                Cellsa13122.Border = Rectangle.LEFT_BORDER;
        //                Cellsa13122.BackgroundColor = BaseColor.WHITE;
        //                table3.AddCell(Cellsa13122);

        //                PdfPCell Cellsa13123 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13123.HorizontalAlignment = 1;
        //                Cellsa13123.Border = Rectangle.LEFT_BORDER;
        //                Cellsa13123.BackgroundColor = BaseColor.WHITE;
        //                table3.AddCell(Cellsa13123);

        //                PdfPCell Cellsa13124 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13124.HorizontalAlignment = 1;
        //                Cellsa13124.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13124.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13124);

        //                PdfPCell Cellsa13125 = new PdfPCell(new Phrase(new Phrase("" + GSt_5.SGSTAmount, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13125.HorizontalAlignment = 1;
        //                Cellsa13125.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13125.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13125);

        //                PdfPCell Cellsa13126 = new PdfPCell(new Phrase(new Phrase("" + GSt_5.CGSTAmount, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13126.HorizontalAlignment = 1;
        //                Cellsa13126.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13126.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13126);

        //                PdfPCell Cellsa13127 = new PdfPCell(new Phrase(new Phrase("" + GSt_5.TotalTAxAmount, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13127.HorizontalAlignment = 1;
        //                Cellsa13127.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13127.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13127);

        //                PdfPCell Cellsa13128 = new PdfPCell(new Phrase(new Phrase("Total Items :-", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13128.HorizontalAlignment = 1;
        //                Cellsa13128.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13128.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13128);

        //                PdfPCell Cellsa131200 = new PdfPCell(new Phrase(new Phrase("DIS AMT.", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa131200.HorizontalAlignment = 0;
        //                Cellsa131200.BackgroundColor = BaseColor.WHITE;
        //                Cellsa131200.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa131200);

        //                PdfPCell Cellsa131210 = new PdfPCell(new Phrase(new Phrase("" + string.Format("{0:0.00 }", Discount), FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa131210.HorizontalAlignment = 2;
        //                Cellsa131210.BackgroundColor = BaseColor.WHITE;
        //                Cellsa131210.Border = Rectangle.RIGHT_BORDER;
        //                table3.AddCell(Cellsa131210);


        //            }
        //            catch { }
        //        }
        //        else
        //        {
        //            try
        //            {
        //                PdfPCell Cellsa13121 = new PdfPCell(new Phrase(new Phrase("GST: 5.00%", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13121.HorizontalAlignment = 1;
        //                Cellsa13121.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13121.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13121);


        //                PdfPCell Cellsa13122 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13122.HorizontalAlignment = 1;
        //                Cellsa13122.Border = Rectangle.LEFT_BORDER;
        //                Cellsa13122.BackgroundColor = BaseColor.WHITE;
        //                table3.AddCell(Cellsa13122);

        //                PdfPCell Cellsa13123 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13123.HorizontalAlignment = 1;
        //                Cellsa13123.Border = Rectangle.LEFT_BORDER;
        //                Cellsa13123.BackgroundColor = BaseColor.WHITE;
        //                table3.AddCell(Cellsa13123);

        //                PdfPCell Cellsa13124 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13124.HorizontalAlignment = 1;
        //                Cellsa13124.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13124.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13124);

        //                PdfPCell Cellsa13125 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13125.HorizontalAlignment = 1;
        //                Cellsa13125.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13125.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13125);

        //                PdfPCell Cellsa13126 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13126.HorizontalAlignment = 1;
        //                Cellsa13126.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13126.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13126);

        //                PdfPCell Cellsa13127 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13127.HorizontalAlignment = 1;
        //                Cellsa13127.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13127.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13127);

        //                PdfPCell Cellsa13128 = new PdfPCell(new Phrase(new Phrase("Total Items :-", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13128.HorizontalAlignment = 1;
        //                Cellsa13128.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13128.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13128);

        //                PdfPCell Cellsa131200 = new PdfPCell(new Phrase(new Phrase("DIS AMT.", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa131200.HorizontalAlignment = 0;
        //                Cellsa131200.BackgroundColor = BaseColor.WHITE;
        //                Cellsa131200.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa131200);

        //                PdfPCell Cellsa131210 = new PdfPCell(new Phrase(new Phrase("" + string.Format("{0:0.00 }", Discount), FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa131210.HorizontalAlignment = 2;
        //                Cellsa131210.BackgroundColor = BaseColor.WHITE;
        //                Cellsa131210.Border = Rectangle.RIGHT_BORDER;
        //                table3.AddCell(Cellsa131210);


        //            }
        //            catch { }
        //        }





        //        var GSt_12 = GstData.Where(a => a.GSTPer == 12).FirstOrDefault();
        //        if (GSt_12 != null)
        //        {
        //            try
        //            {
        //                PdfPCell Cellsa13121 = new PdfPCell(new Phrase(new Phrase("GST: 12.00%", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13121.HorizontalAlignment = 1;
        //                Cellsa13121.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13121.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13121);


        //                PdfPCell Cellsa13122 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13122.HorizontalAlignment = 1;
        //                Cellsa13122.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13122.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13122);

        //                PdfPCell Cellsa13123 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13123.HorizontalAlignment = 1;
        //                Cellsa13123.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13123.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13123);

        //                PdfPCell Cellsa13124 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13124.HorizontalAlignment = 1;
        //                Cellsa13124.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13124.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13124);

        //                PdfPCell Cellsa13125 = new PdfPCell(new Phrase(new Phrase("" + GSt_12.SGSTAmount, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13125.HorizontalAlignment = 1;
        //                Cellsa13125.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13125.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13125);

        //                PdfPCell Cellsa13126 = new PdfPCell(new Phrase(new Phrase("" + GSt_12.CGSTAmount, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13126.HorizontalAlignment = 1;
        //                Cellsa13126.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13126.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13126);

        //                PdfPCell Cellsa13127 = new PdfPCell(new Phrase(new Phrase("" + GSt_12.TotalTAxAmount, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13127.HorizontalAlignment = 1;
        //                Cellsa13127.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13127.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13127);

        //                PdfPCell Cellsa13128 = new PdfPCell(new Phrase(new Phrase("Total Qty :-", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13128.HorizontalAlignment = 1;
        //                Cellsa13128.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13128.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13128);

        //                PdfPCell Cellsa131200 = new PdfPCell(new Phrase(new Phrase("SGST PAYABLE", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa131200.HorizontalAlignment = 0;
        //                Cellsa131200.BackgroundColor = BaseColor.WHITE;
        //                Cellsa131200.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa131200);

        //                PdfPCell Cellsa131210 = new PdfPCell(new Phrase(new Phrase("" + string.Format("{0:0.00 }", SGSTAmount), FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa131210.HorizontalAlignment = 2;
        //                Cellsa131210.BackgroundColor = BaseColor.WHITE;
        //                Cellsa131210.Border = Rectangle.RIGHT_BORDER;
        //                table3.AddCell(Cellsa131210);


        //            }
        //            catch { }
        //        }
        //        else
        //        {
        //            try
        //            {
        //                PdfPCell Cellsa13121 = new PdfPCell(new Phrase(new Phrase("GST: 12.00%", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13121.HorizontalAlignment = 1;
        //                Cellsa13121.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13121.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13121);


        //                PdfPCell Cellsa13122 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13122.HorizontalAlignment = 1;
        //                Cellsa13122.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13122.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13122);

        //                PdfPCell Cellsa13123 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13123.HorizontalAlignment = 1;
        //                Cellsa13123.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13123.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13123);

        //                PdfPCell Cellsa13124 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13124.HorizontalAlignment = 1;
        //                Cellsa13124.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13124.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13124);

        //                PdfPCell Cellsa13125 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13125.HorizontalAlignment = 1;
        //                Cellsa13125.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13125.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13125);

        //                PdfPCell Cellsa13126 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13126.HorizontalAlignment = 1;
        //                Cellsa13126.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13126.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13126);

        //                PdfPCell Cellsa13127 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13127.HorizontalAlignment = 1;
        //                Cellsa13127.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13127.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13127);

        //                PdfPCell Cellsa13128 = new PdfPCell(new Phrase(new Phrase("Total Qty :-", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13128.HorizontalAlignment = 1;
        //                Cellsa13128.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13128.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13128);

        //                PdfPCell Cellsa131200 = new PdfPCell(new Phrase(new Phrase("SGST PAYABLE", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa131200.HorizontalAlignment = 0;
        //                Cellsa131200.BackgroundColor = BaseColor.WHITE;
        //                Cellsa131200.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa131200);

        //                PdfPCell Cellsa131210 = new PdfPCell(new Phrase(new Phrase("" + string.Format("{0:0.00 }", SGSTAmount), FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa131210.HorizontalAlignment = 2;
        //                Cellsa131210.BackgroundColor = BaseColor.WHITE;
        //                Cellsa131210.Border = Rectangle.RIGHT_BORDER;
        //                table3.AddCell(Cellsa131210);


        //            }
        //            catch { }
        //        }





        //        var GSt_18 = GstData.Where(a => a.GSTPer == 18).FirstOrDefault();
        //        if (GSt_18 != null)
        //        {
        //            try
        //            {
        //                PdfPCell Cellsa13121 = new PdfPCell(new Phrase(new Phrase("GST: 18.00%", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13121.HorizontalAlignment = 1;
        //                Cellsa13121.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13121.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13121);


        //                PdfPCell Cellsa13122 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13122.HorizontalAlignment = 1;
        //                Cellsa13122.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13122.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13122);

        //                PdfPCell Cellsa13123 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13123.HorizontalAlignment = 1;
        //                Cellsa13123.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13123.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13123);

        //                PdfPCell Cellsa13124 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13124.HorizontalAlignment = 1;
        //                Cellsa13124.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13124.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13124);

        //                PdfPCell Cellsa13125 = new PdfPCell(new Phrase(new Phrase("" + GSt_18.CGSTAmount, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13125.HorizontalAlignment = 1;
        //                Cellsa13125.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13125.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13125);

        //                PdfPCell Cellsa13126 = new PdfPCell(new Phrase(new Phrase("" + GSt_18.SGSTAmount, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13126.HorizontalAlignment = 1;
        //                Cellsa13126.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13126.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13126);

        //                PdfPCell Cellsa13127 = new PdfPCell(new Phrase(new Phrase("" + GSt_18.TotalTAxAmount, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13127.HorizontalAlignment = 1;
        //                Cellsa13127.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13127.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13127);

        //                PdfPCell Cellsa13128 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13128.HorizontalAlignment = 1;
        //                Cellsa13128.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13128.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13128);

        //                PdfPCell Cellsa131200 = new PdfPCell(new Phrase(new Phrase("CGST PAYABLE", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa131200.HorizontalAlignment = 0;
        //                Cellsa131200.BackgroundColor = BaseColor.WHITE;
        //                Cellsa131200.Border = Rectangle.LEFT_BORDER;

        //                table3.AddCell(Cellsa131200);

        //                PdfPCell Cellsa131210 = new PdfPCell(new Phrase(new Phrase("" + string.Format("{0:0.00 }", CGSTAmount), FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa131210.HorizontalAlignment = 2;
        //                Cellsa131210.BackgroundColor = BaseColor.WHITE;
        //                Cellsa131210.Border = Rectangle.RIGHT_BORDER;
        //                table3.AddCell(Cellsa131210);


        //            }
        //            catch { }
        //        }
        //        else
        //        {
        //            try
        //            {
        //                PdfPCell Cellsa13121 = new PdfPCell(new Phrase(new Phrase("GST: 18.00%", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13121.HorizontalAlignment = 1;
        //                Cellsa13121.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13121.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13121);


        //                PdfPCell Cellsa13122 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13122.HorizontalAlignment = 1;
        //                Cellsa13122.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13122.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13122);

        //                PdfPCell Cellsa13123 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13123.HorizontalAlignment = 1;
        //                Cellsa13123.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13123.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13123);

        //                PdfPCell Cellsa13124 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13124.HorizontalAlignment = 1;
        //                Cellsa13124.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13124.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13124);

        //                PdfPCell Cellsa13125 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13125.HorizontalAlignment = 1;
        //                Cellsa13125.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13125.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13125);

        //                PdfPCell Cellsa13126 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13126.HorizontalAlignment = 1;
        //                Cellsa13126.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13126.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13126);

        //                PdfPCell Cellsa13127 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13127.HorizontalAlignment = 1;
        //                Cellsa13127.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13127.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13127);

        //                PdfPCell Cellsa13128 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13128.HorizontalAlignment = 1;
        //                Cellsa13128.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13128.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13128);

        //                PdfPCell Cellsa131200 = new PdfPCell(new Phrase(new Phrase("CGST PAYABLE", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa131200.HorizontalAlignment = 0;
        //                Cellsa131200.BackgroundColor = BaseColor.WHITE;
        //                Cellsa131200.Border = Rectangle.LEFT_BORDER;

        //                table3.AddCell(Cellsa131200);

        //                PdfPCell Cellsa131210 = new PdfPCell(new Phrase(new Phrase("" + string.Format("{0:0.00 }", CGSTAmount), FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa131210.HorizontalAlignment = 2;
        //                Cellsa131210.BackgroundColor = BaseColor.WHITE;
        //                Cellsa131210.Border = Rectangle.RIGHT_BORDER;
        //                table3.AddCell(Cellsa131210);


        //            }
        //            catch { }
        //        }


        //        var GSt_28 = GstData.Where(a => a.GSTPer == 28).FirstOrDefault();
        //        if (GSt_28 != null)
        //        {
        //            try
        //            {
        //                PdfPCell Cellsa13121 = new PdfPCell(new Phrase(new Phrase("GST: 28.00%", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13121.HorizontalAlignment = 1;
        //                Cellsa13121.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13121.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13121);


        //                PdfPCell Cellsa13122 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13122.HorizontalAlignment = 1;
        //                Cellsa13122.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13122.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13122);

        //                PdfPCell Cellsa13123 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13123.HorizontalAlignment = 1;
        //                Cellsa13123.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13123.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13123);

        //                PdfPCell Cellsa13124 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13124.HorizontalAlignment = 1;
        //                Cellsa13124.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13124.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13124);

        //                PdfPCell Cellsa13125 = new PdfPCell(new Phrase(new Phrase("" + GSt_28.CGSTAmount, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13125.HorizontalAlignment = 1;
        //                Cellsa13125.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13125.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13125);

        //                PdfPCell Cellsa13126 = new PdfPCell(new Phrase(new Phrase("" + GSt_28.SGSTAmount, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13126.HorizontalAlignment = 1;
        //                Cellsa13126.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13126.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13126);

        //                PdfPCell Cellsa13127 = new PdfPCell(new Phrase(new Phrase("" + GSt_28.TotalTAxAmount, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13127.HorizontalAlignment = 1;
        //                Cellsa13127.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13127.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13127);

        //                PdfPCell Cellsa13128 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13128.HorizontalAlignment = 1;
        //                Cellsa13128.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13128.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13128);

        //                PdfPCell Cellsa131200 = new PdfPCell(new Phrase(new Phrase("ROUND OFF", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa131200.HorizontalAlignment = 0;
        //                Cellsa131200.BackgroundColor = BaseColor.WHITE;
        //                Cellsa131200.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa131200);


        //                var xx = (TotalAmount - Discount) + CGSTAmount + SGSTAmount;
        //                TotalAmount = xx;

        //                PdfPCell Cellsa131210 = new PdfPCell(new Phrase(new Phrase("" + string.Format("{0:0.00 }", xx), FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa131210.HorizontalAlignment = 2;
        //                Cellsa131210.BackgroundColor = BaseColor.WHITE;
        //                Cellsa131210.Border = Rectangle.RIGHT_BORDER;
        //                table3.AddCell(Cellsa131210);


        //            }
        //            catch { }
        //        }
        //        else
        //        {
        //            try
        //            {
        //                PdfPCell Cellsa13121 = new PdfPCell(new Phrase(new Phrase("GST: 28.00%", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13121.HorizontalAlignment = 1;
        //                Cellsa13121.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13121.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13121);


        //                PdfPCell Cellsa13122 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13122.HorizontalAlignment = 1;
        //                Cellsa13122.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13122.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13122);

        //                PdfPCell Cellsa13123 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13123.HorizontalAlignment = 1;
        //                Cellsa13123.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13123.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13123);

        //                PdfPCell Cellsa13124 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13124.HorizontalAlignment = 1;
        //                Cellsa13124.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13124.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13124);

        //                PdfPCell Cellsa13125 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13125.HorizontalAlignment = 1;
        //                Cellsa13125.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13125.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13125);

        //                PdfPCell Cellsa13126 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13126.HorizontalAlignment = 1;
        //                Cellsa13126.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13126.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13126);

        //                PdfPCell Cellsa13127 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13127.HorizontalAlignment = 1;
        //                Cellsa13127.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13127.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13127);

        //                PdfPCell Cellsa13128 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa13128.HorizontalAlignment = 1;
        //                Cellsa13128.BackgroundColor = BaseColor.WHITE;
        //                Cellsa13128.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa13128);

        //                PdfPCell Cellsa131200 = new PdfPCell(new Phrase(new Phrase("ROUND OFF", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa131200.HorizontalAlignment = 0;
        //                Cellsa131200.BackgroundColor = BaseColor.WHITE;
        //                Cellsa131200.Border = Rectangle.LEFT_BORDER;
        //                table3.AddCell(Cellsa131200);


        //                var xx = (TotalAmount - Discount) + CGSTAmount + SGSTAmount;
        //                TotalAmount = xx;

        //                PdfPCell Cellsa131210 = new PdfPCell(new Phrase(new Phrase("" + string.Format("{0:0.00 }", xx), FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //                Cellsa131210.HorizontalAlignment = 2;
        //                Cellsa131210.BackgroundColor = BaseColor.WHITE;
        //                Cellsa131210.Border = Rectangle.RIGHT_BORDER;
        //                table3.AddCell(Cellsa131210);

        //            }
        //            catch { }
        //        }

        //        try
        //        {
        //            PdfPCell Cellsa13121 = new PdfPCell(new Phrase(new Phrase("TOTAL", FontFactory.GetFont("Arial", 9, Font.BOLD, BaseColor.BLACK))));
        //            Cellsa13121.HorizontalAlignment = 0;
        //            Cellsa13121.BackgroundColor = BaseColor.WHITE;
        //            table3.AddCell(Cellsa13121);


        //            PdfPCell Cellsa13122 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //            Cellsa13122.HorizontalAlignment = 1;
        //            Cellsa13122.BackgroundColor = BaseColor.WHITE;
        //            table3.AddCell(Cellsa13122);

        //            PdfPCell Cellsa13123 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //            Cellsa13123.HorizontalAlignment = 1;
        //            Cellsa13123.BackgroundColor = BaseColor.WHITE;
        //            table3.AddCell(Cellsa13123);

        //            PdfPCell Cellsa13124 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //            Cellsa13124.HorizontalAlignment = 1;
        //            Cellsa13124.BackgroundColor = BaseColor.WHITE;
        //            table3.AddCell(Cellsa13124);

        //            PdfPCell Cellsa13125 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //            Cellsa13125.HorizontalAlignment = 1;
        //            Cellsa13125.BackgroundColor = BaseColor.WHITE;
        //            table3.AddCell(Cellsa13125);

        //            PdfPCell Cellsa13126 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //            Cellsa13126.HorizontalAlignment = 1;
        //            Cellsa13126.BackgroundColor = BaseColor.WHITE;
        //            table3.AddCell(Cellsa13126);

        //            PdfPCell Cellsa13127 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //            Cellsa13127.HorizontalAlignment = 1;
        //            Cellsa13127.BackgroundColor = BaseColor.WHITE;
        //            table3.AddCell(Cellsa13127);

        //            PdfPCell Cellsa13128 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //            Cellsa13128.HorizontalAlignment = 1;
        //            Cellsa13128.BackgroundColor = BaseColor.WHITE;
        //            table3.AddCell(Cellsa13128);

        //            PdfPCell Cellsa131200 = new PdfPCell(new Phrase(new Phrase("CR/DR NOTE", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //            Cellsa131200.HorizontalAlignment = 0;
        //            Cellsa131200.BackgroundColor = BaseColor.WHITE;
        //            Cellsa131200.Border = Rectangle.LEFT_BORDER;
        //            table3.AddCell(Cellsa131200);

        //            PdfPCell Cellsa131210 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //            Cellsa131210.HorizontalAlignment = 2;
        //            Cellsa131210.BackgroundColor = BaseColor.WHITE;
        //            Cellsa131210.Border = Rectangle.RIGHT_BORDER;
        //            table3.AddCell(Cellsa131210);


        //        }
        //        catch { }

        //        try
        //        {
        //            var AmtInwords = words(Convert.ToInt32(Math.Round(TotalAmount)));
        //            PdfPCell Cellsa13121 = new PdfPCell(new Phrase(new Phrase("Amount In Word : " + AmtInwords, FontFactory.GetFont("Arial", 9, Font.BOLD, BaseColor.BLACK))));
        //            Cellsa13121.HorizontalAlignment = 0;
        //            Cellsa13121.BackgroundColor = BaseColor.WHITE;
        //            Cellsa13121.Colspan = 8;
        //            table3.AddCell(Cellsa13121);


        //            PdfPCell Cellsa131200 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //            Cellsa131200.HorizontalAlignment = 1;
        //            Cellsa131200.BackgroundColor = BaseColor.WHITE;
        //            Cellsa131200.Border = Rectangle.NO_BORDER;
        //            table3.AddCell(Cellsa131200);

        //            PdfPCell Cellsa1312s00 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //            Cellsa1312s00.HorizontalAlignment = 1;
        //            Cellsa1312s00.BackgroundColor = BaseColor.WHITE;
        //            Cellsa1312s00.Border = Rectangle.RIGHT_BORDER;
        //            table3.AddCell(Cellsa1312s00);


        //        }
        //        catch { }

        //        try
        //        {
        //            PdfPCell Cellsa13121 = new PdfPCell(new Phrase(new Phrase("Remark", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //            Cellsa13121.HorizontalAlignment = 0;
        //            Cellsa13121.BackgroundColor = BaseColor.WHITE;
        //            Cellsa13121.Colspan = 8;
        //            table3.AddCell(Cellsa13121);


        //            PdfPCell Cellsa131200 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //            Cellsa131200.HorizontalAlignment = 1;
        //            Cellsa131200.BackgroundColor = BaseColor.WHITE;

        //            Cellsa131200.Border = Rectangle.NO_BORDER;
        //            table3.AddCell(Cellsa131200);

        //            PdfPCell Cellsa1312s00 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //            Cellsa1312s00.HorizontalAlignment = 1;
        //            Cellsa1312s00.BackgroundColor = BaseColor.WHITE;
        //            Cellsa1312s00.Border = Rectangle.RIGHT_BORDER;
        //            table3.AddCell(Cellsa1312s00);


        //        }
        //        catch { }




        //        //BAnk details

        //        try
        //        {
        //            Paragraph Para5453 = new Paragraph();
        //            Para5453.Add(new Phrase("BANK DETAILS AS :- \n", FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //            Para5453.Add(new Phrase("\n", FontFactory.GetFont("Arial", 5, Font.NORMAL)));
        //            Para5453.Add(new Phrase("Bank Name : Indian Bank\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //            Para5453.Add(new Phrase("Branch : Pune Cantonment branch ,Pune Account No. : 7469753553\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //            Para5453.Add(new Phrase("IFSC Code :IDIB000P087\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //            Para5453.Add(new Phrase("\n", FontFactory.GetFont("Arial", 5, Font.NORMAL)));


        //            Para5453.Add(new Phrase("Terms & Conditions  \n", FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //            Para5453.Add(new Phrase("\n", FontFactory.GetFont("Arial", 5, Font.NORMAL)));


        //            Para5453.Add(new Phrase("***Goods once sold will not be taken back or exchanged.\n", FontFactory.GetFont("Arial", 9, Font.BOLD)));

        //            Para5453.Add(new Phrase("***We are not responsible for any shortage of goods in transit\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));

        //            Para5453.Add(new Phrase("***Bills not paid before due date will attract 24% interest.\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));

        //            Para5453.Add(new Phrase("All disputes subject to PUNE Jurisdiction only.Cheque Bounce Charges Rs.350\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));


        //            PdfPCell Cellsa13ss121 = new PdfPCell(Para5453);
        //            Cellsa13ss121.HorizontalAlignment = 0;
        //            Cellsa13ss121.BackgroundColor = BaseColor.WHITE;
        //            Cellsa13ss121.Colspan = 6;
        //            table3.AddCell(Cellsa13ss121);





        //            Paragraph Para545dsd3 = new Paragraph();
        //            Para545dsd3.Add(new Phrase("FOR Siddhivinayak Distributor LLP\n\n\n\n\n\n\n\n\n", FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //            Para545dsd3.Add(new Phrase("Authorised Signatory", FontFactory.GetFont("Arial", 9, Font.BOLD)));

        //            PdfPCell Cellsa13sss121 = new PdfPCell(Para545dsd3);
        //            Cellsa13sss121.HorizontalAlignment = 1;
        //            Cellsa13sss121.BackgroundColor = BaseColor.WHITE;
        //            Cellsa13sss121.Colspan = 2;
        //            table3.AddCell(Cellsa13sss121);


        //            PdfPCell Cellsa131200 = new PdfPCell(new Phrase(new Phrase("\n\n\n Grand Total : " + string.Format("{0:0.00 }", Math.Round(TotalAmount)), FontFactory.GetFont("Arial", 10, Font.BOLD, BaseColor.BLACK))));
        //            Cellsa131200.HorizontalAlignment = 1;
        //            Cellsa131200.BackgroundColor = BaseColor.WHITE;
        //            Cellsa131200.Colspan = 2;

        //            table3.AddCell(Cellsa131200);


        //        }
        //        catch { }



        //        document.Add(table1);
        //        document.Add(table2);
        //        document.Add(table3);

        //        document.Close();
        //        var result = new { Message = "success", FileName = filename1 };
        //        return Json(result, JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception EX)
        //    {
        //        var result = new { Message = EX.Message };
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }

        //}
        //public JsonResult InvoicePrintNew(string InvoiceNo)
        //{
        //    try
        //    {
        //        DateTime dt = DateTime.Now;
        //        var aa = dt.ToString("HH");
        //        var bb = dt.ToString("mm");
        //        var cc = dt.ToString("ss");
        //        Document document = new Document(PageSize.A4, 20f, 10f, 20f, 10f);
        //        string path = Server.MapPath("~/Reports/Invoice/");
        //        var filename = InvoiceNo + ".pdf";
        //        string filename1 = path + "" + InvoiceNo + "_" + aa + bb + cc + ".pdf";
        //        PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filename1, FileMode.Create));
        //        document.Open();
        //        document.AddTitle(InvoiceNo); //Statement-1
        //        document.AddHeader("content-disposition", "inline;filename=" + InvoiceNo + ".pdf");
        //        Session["fileName1"] = filename1;
        //        Session["filename"] = filename;

        //        int Count = 1;
        //        string CustomerAddress = "", CustomerName = "", OrderNo = "", OrderDate = "", TermsAndConditions = "";
        //        decimal TotalAmount = 0, BasicRate = 0, Discount = 0, GSTPercentage = 0, CGSTAmount = 0, IGSTAmount = 0, SGSTAmount = 0, NetAmount = 0;
        //        string InvoiceDate = "";
        //        decimal DeliveredQty = 0;
        //        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["InventoryModel"].ToString());
        //        string query1 = "select O.TermsAndConditions, S.TotalAmount, S.OrderNo,CONVERT(VARCHAR(10), O.OrderDate, 103) as OrderDate,CONVERT(VARCHAR(10), S.InvoiceDate, 103) as InvoiceDate,S.DeliveredQty,S.BasicRate,S.Discount,S.GSTPercentage,S.CGSTAmount,S.SGSTAmount, S.IGSTAmount,P.ProductName,O.DeliverTo,Cust.CustomerName from Sales S inner join OrderMains O on S.OrderNo = O.OrderNo     inner join Customers Cust on Cust.CustomerID=O.CustomerID inner join products P on S.ProductCode = P.ProductCode where S.InvoiceNo = '" + InvoiceNo + "'";
        //        conn.Open();
        //        SqlCommand cmd1 = new SqlCommand(query1, conn);
        //        DataTable dt1 = new DataTable();
        //        dt1.Load(cmd1.ExecuteReader());
        //        conn.Close();
        //        if (dt1.Rows.Count == 0)
        //        {
        //            var result1 = new { Message = "No data Found....!", FileName = filename1 };
        //            return Json(result1, JsonRequestBehavior.AllowGet);
        //        }
        //        foreach (DataRow cdr in dt1.Rows)
        //        {
        //            OrderNo = cdr["OrderNo"].ToString();
        //            OrderDate = cdr["OrderDate"].ToString();
        //            TermsAndConditions = cdr["TermsAndConditions"].ToString();
        //            InvoiceDate = cdr["InvoiceDate"].ToString();
        //            DeliveredQty = Convert.ToDecimal(cdr["DeliveredQty"].ToString());
        //            CustomerAddress = cdr["DeliverTo"].ToString();
        //            CustomerName = cdr["CustomerName"].ToString();
        //            BasicRate = +Convert.ToDecimal(cdr["BasicRate"].ToString());
        //            Discount = Discount + Convert.ToDecimal(cdr["Discount"].ToString());
        //            GSTPercentage = Convert.ToDecimal(cdr["GSTPercentage"].ToString());
        //            IGSTAmount = IGSTAmount + Convert.ToDecimal(cdr["IGSTAmount"].ToString());
        //            SGSTAmount = SGSTAmount + Convert.ToDecimal(cdr["SGSTAmount"].ToString());
        //            CGSTAmount = CGSTAmount + Convert.ToDecimal(cdr["CGSTAmount"].ToString());
        //            TotalAmount = TotalAmount + (Convert.ToDecimal(cdr["TotalAmount"].ToString()));
        //            NetAmount = NetAmount + (Convert.ToDecimal(cdr["DeliveredQty"].ToString()) * Convert.ToDecimal(cdr["BasicRate"].ToString()));
        //        }

        //        iTextSharp.text.Font font5 = iTextSharp.text.FontFactory.GetFont(FontFactory.HELVETICA, 9);
        //        iTextSharp.text.Font font10 = iTextSharp.text.FontFactory.GetFont(FontFactory.HELVETICA, 11);


        //        PdfPTable table4 = new PdfPTable(6);
        //        float[] widths5 = new float[] { 2f, 4f, 3f, 3f, 3f, 3f };
        //        table4.SetWidths(widths5);
        //        table4.WidthPercentage = 95;
        //        table4.HorizontalAlignment = 1;

        //        string imageURL = Server.MapPath("/Photo/MicraftLogo.png");
        //        iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(imageURL);
        //        jpg.ScaleToFit(100, 150);
        //        jpg.SpacingBefore = 5f;
        //        jpg.SpacingAfter = 1f;
        //        jpg.Alignment = Element.ALIGN_LEFT;

        //        PdfPCell Cell1 = new PdfPCell(jpg);
        //        Cell1.Border = Rectangle.NO_BORDER;
        //        Cell1.Colspan = 3;
        //        Cell1.Border = Rectangle.BOTTOM_BORDER;
        //        table4.AddCell(Cell1);

        //        Paragraph Para2 = new Paragraph();
        //        Para2.Add(new Phrase("\n Office No. 15, 6th Floor, D Wing, \n KK Market Dhankawadi, Pune, Maharashtra 411043\n 9404 77 4367\n", FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //        PdfPCell Cell2 = new PdfPCell(Para2);
        //        Cell2.HorizontalAlignment = 0;
        //        Cell2.Border = Rectangle.NO_BORDER;
        //        Cell2.Colspan = 3;
        //        Cell2.Border = Rectangle.BOTTOM_BORDER;
        //        table4.AddCell(Cell2);


        //        //  2nd Row


        //        Paragraph Para3 = new Paragraph();
        //        Para3.Add(new Phrase("\n\nInvoice No\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        Para3.Add(new Phrase("Date ", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        PdfPCell Cell3 = new PdfPCell(Para3);
        //        Cell3.Border = Rectangle.BOTTOM_BORDER;
        //        Cell3.FixedHeight = 50f;
        //        Cell3.HorizontalAlignment = 0;
        //        table4.AddCell(Cell3);

        //        Paragraph Para4 = new Paragraph();
        //        Para4.Add(new Phrase("\n\n : " + InvoiceNo + "\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        Para4.Add(new Phrase(" : " + InvoiceDate + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        PdfPCell Cell4 = new PdfPCell(Para4);
        //        Cell4.Border = Rectangle.BOTTOM_BORDER;
        //        Cell4.FixedHeight = 50f;
        //        Cell4.HorizontalAlignment = 0;
        //        table4.AddCell(Cell4);

        //        Paragraph Para5 = new Paragraph();
        //        Para5.Add(new Phrase("TAX INVOICE", FontFactory.GetFont("Arial", 11, Font.BOLD)));
        //        PdfPCell Cell5 = new PdfPCell(Para5);
        //        Cell5.HorizontalAlignment = 1;
        //        Cell5.Border = Rectangle.BOTTOM_BORDER;
        //        Cell5.Colspan = 2;
        //        table4.AddCell(Cell5);

        //        Paragraph Para6 = new Paragraph();
        //        Para6.Add(new Phrase("\n\nOrder No\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        Para6.Add(new Phrase("OrderDate", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        PdfPCell Cell6 = new PdfPCell(Para6);
        //        Cell6.Border = Rectangle.BOTTOM_BORDER;
        //        Cell6.FixedHeight = 50f;
        //        Cell6.HorizontalAlignment = 0;
        //        table4.AddCell(Cell6);

        //        Paragraph Para7 = new Paragraph();
        //        Para7.Add(new Phrase("\n\n" + OrderNo + "\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        Para7.Add(new Phrase("" + OrderDate + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        PdfPCell Cell7 = new PdfPCell(Para7);
        //        Cell7.Border = Rectangle.BOTTOM_BORDER;
        //        Cell7.FixedHeight = 50f;
        //        Cell7.HorizontalAlignment = 0;
        //        table4.AddCell(Cell7);


        //        //3rd Row

        //        PdfPCell Cell8 = new PdfPCell(new Phrase(new Phrase("Sr. No.", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //        Cell8.HorizontalAlignment = 1;
        //        Cell8.BackgroundColor = BaseColor.BLACK;
        //        table4.AddCell(Cell8);

        //        PdfPCell Cell9 = new PdfPCell(new Phrase(new Phrase("Product Name", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //        Cell9.HorizontalAlignment = 1;
        //        Cell9.BackgroundColor = BaseColor.BLACK;
        //        table4.AddCell(Cell9);

        //        PdfPCell Cell10 = new PdfPCell(new Phrase(new Phrase("Order Qty", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //        Cell10.HorizontalAlignment = 1;
        //        Cell10.BackgroundColor = BaseColor.BLACK;
        //        table4.AddCell(Cell10);

        //        PdfPCell Cell11 = new PdfPCell(new Phrase(new Phrase("Price", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //        Cell11.HorizontalAlignment = 1;
        //        Cell11.BackgroundColor = BaseColor.BLACK;
        //        table4.AddCell(Cell11);

        //        PdfPCell Cell12 = new PdfPCell(new Phrase(new Phrase("GST", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //        Cell12.HorizontalAlignment = 1;
        //        Cell12.BackgroundColor = BaseColor.BLACK;
        //        table4.AddCell(Cell12);

        //        PdfPCell Cell13 = new PdfPCell(new Phrase(new Phrase("Amount", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //        Cell13.HorizontalAlignment = 1;
        //        Cell13.BackgroundColor = BaseColor.BLACK;
        //        table4.AddCell(Cell13);


        //        //4th Row

        //        foreach (DataRow r in dt1.Rows)
        //        {
        //            if (dt1.Rows.Count > 0)
        //            {

        //                Paragraph Para14 = new Paragraph();
        //                Para14.Add(new Phrase("" + Count + "\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //                PdfPCell Cell14 = new PdfPCell(Para14);
        //                Cell14.HorizontalAlignment = 1;
        //                Cell14.UseAscender = true;
        //                table4.AddCell(Cell14);

        //                Paragraph Para15 = new Paragraph();
        //                Para15.Add(new Phrase("" + r["ProductName"].ToString() + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //                PdfPCell Cell15 = new PdfPCell(Para15);
        //                Cell15.HorizontalAlignment = 0;
        //                table4.AddCell(Cell15);


        //                Paragraph Para16 = new Paragraph();
        //                Para16.Add(new Phrase("" + r["DeliveredQty"].ToString() + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //                PdfPCell Cell16 = new PdfPCell(Para16);
        //                Cell16.HorizontalAlignment = 1;
        //                table4.AddCell(Cell16);

        //                Paragraph Para17 = new Paragraph();
        //                Para17.Add(new Phrase("" + r["BasicRate"].ToString() + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //                PdfPCell Cell17 = new PdfPCell(Para17);
        //                Cell17.HorizontalAlignment = 1;
        //                table4.AddCell(Cell17);

        //                Paragraph Para18 = new Paragraph();
        //                Para18.Add(new Phrase("" + r["GSTPercentage"].ToString() + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //                PdfPCell Cell18 = new PdfPCell(Para18);
        //                Cell18.HorizontalAlignment = 1;
        //                table4.AddCell(Cell18);

        //                decimal TotAmt = Convert.ToDecimal(r["BasicRate"].ToString()) * Convert.ToDecimal(r["DeliveredQty"].ToString());

        //                Paragraph Para19 = new Paragraph();
        //                Para19.Add(new Phrase("" + string.Format("{0:0.00 }", TotAmt) + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //                PdfPCell Cell19 = new PdfPCell(Para19);
        //                Cell19.HorizontalAlignment = 2;
        //                table4.AddCell(Cell19);
        //                TotAmt = 0;

        //                Count++;
        //            }
        //        }

        //        //5th Row



        //        for (int temp = 1; temp <= 7; temp++)
        //        {


        //            Paragraph Para20 = new Paragraph();
        //            Paragraph Para21 = new Paragraph();
        //            var data = "";
        //            var value = "";
        //            if (temp == 1)
        //            {
        //                Para20.Add(new Phrase("\n Our GSTNo    ", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //                Para20.Add(new Phrase("\n PAN No       ", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //                Para21.Add(new Phrase("\n:   22AAAAA0000A1Z1", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //                Para21.Add(new Phrase("\n:   BNZPG0000C", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //                data = "SUB TOTAL";
        //                value = string.Format("{0:0.00}", NetAmount);
        //            }
        //            else
        //            {
        //                Para20.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //                Para21.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //            }
        //            if (temp == 2)
        //            {
        //                data = "DISCOUNT";
        //                value = String.Format("{0:0.00}", Discount);
        //            }
        //            if (temp == 3)
        //            {
        //                data = "IGST";
        //                value = String.Format("{0:0.00}", IGSTAmount);
        //            }
        //            if (temp == 4)
        //            {
        //                data = "CGST";
        //                value = String.Format("{0:0.00}", CGSTAmount);
        //            }
        //            if (temp == 5)
        //            {
        //                data = "SGST";
        //                value = String.Format("{0:0.00}", SGSTAmount);
        //            }
        //            if (temp == 6)
        //            {
        //                data = "TOTAL";
        //                value = String.Format("{0:0.00}", TotalAmount);
        //            }
        //            if (temp == 7)
        //            {
        //                data = "Round Off";
        //                value = Math.Round(TotalAmount) + ".00";
        //            }


        //            PdfPCell Cell20 = new PdfPCell(Para20);
        //            Cell20.HorizontalAlignment = 0;
        //            if (temp == 1)
        //            {
        //                Cell20.Rowspan = 9;
        //            }
        //            Cell20.Border = Rectangle.LEFT_BORDER;
        //            table4.AddCell(Cell20);

        //            PdfPCell Cell21 = new PdfPCell(Para21);
        //            Cell21.HorizontalAlignment = 0;
        //            Cell21.Colspan = 3;
        //            if (temp == 1)
        //            {
        //                Cell21.Rowspan = 9;
        //            }
        //            Cell21.Border = Rectangle.NO_BORDER;
        //            table4.AddCell(Cell21);

        //            Paragraph Para22 = new Paragraph();
        //            Paragraph Para23 = new Paragraph();
        //            if (temp == 1 || temp == 6)
        //            {
        //                Para22.Add(new Phrase("" + data, FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //                Para23.Add(new Phrase("" + value, FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //            }
        //            else
        //            {
        //                Para22.Add(new Phrase("" + data, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //                Para23.Add(new Phrase("" + value, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //            }
        //            PdfPCell Cell22 = new PdfPCell(Para22);
        //            Cell22.HorizontalAlignment = 1;
        //            table4.AddCell(Cell22);

        //            PdfPCell Cell23 = new PdfPCell(Para23);
        //            Cell23.HorizontalAlignment = 2;
        //            table4.AddCell(Cell23);
        //        }


        //        var AmtInwords = words(Convert.ToInt32(TotalAmount));


        //        Paragraph Para24 = new Paragraph();
        //        Para24.Add(new Phrase("Amt in words : ", FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //        Para24.Add(new Phrase("" + AmtInwords + "\n", FontFactory.GetFont("Arial", 8, Font.NORMAL)));
        //        PdfPCell Cell24 = new PdfPCell(Para24);
        //        Cell24.HorizontalAlignment = 0;
        //        Cell24.Colspan = 6;
        //        Cell24.FixedHeight = 15;
        //        table4.AddCell(Cell24);


        //        Paragraph Para25 = new Paragraph();
        //        Para25.Add(new Phrase("Bank Details : Kotak Mahindra Bank Bibvewadi-415263    \nACCOUNT NO :2652124569      IFSC : KKBK0001776.\n\n", FontFactory.GetFont("Arial", 8, Font.NORMAL)));
        //        Para25.Add(new Phrase("Terms And Conditions :" + TermsAndConditions, FontFactory.GetFont("Arial", 8, Font.NORMAL)));

        //        PdfPCell Cell25 = new PdfPCell(Para25);
        //        Cell25.HorizontalAlignment = 0;
        //        Cell25.Colspan = 4;
        //        table4.AddCell(Cell25);


        //        Paragraph Para27 = new Paragraph();
        //        Para27.Add(new Phrase("For Micraft Solutions \n\n\n\n", FontFactory.GetFont("Arial", 8, Font.BOLD)));
        //        Para27.Add(new Phrase("                                      Authorised Signatory", FontFactory.GetFont("Arial", 7, Font.BOLD)));
        //        PdfPCell Cell26 = new PdfPCell(Para27);
        //        Cell26.HorizontalAlignment = 1;
        //        Cell26.Colspan = 2;
        //        table4.AddCell(Cell26);


        //        document.Add(table4);
        //        document.Close();
        //        var result = new { Message = "success", FileName = filename1 };
        //        return Json(result, JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception EX)
        //    {
        //        var result = new { Message = EX.Message };
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }

        //}

        public JsonResult InvoicePrintNew(string InvoiceNo)
        {
            try
            {

                var IGSTAmount = 0;
                decimal CGSTAmount = 0;
                decimal SGSTAmount = 0;
                decimal TotalAmount = 0;
                decimal DisplayAmt = 0;
                decimal? TotAmt = 0;
                var NetAmount = 0;
                decimal Discount = 0;
                var orderno = InvoiceNo;
                //   InvoiceNo = db.Sales.Where(a => a.OrderNo == InvoiceNo).Select(a => a.InvoiceNo).FirstOrDefault();
                DateTime dt = DateTime.Now;

                var aa = dt.ToString("HH");
                var bb = dt.ToString("mm");
                var cc = dt.ToString("ss");



                Document document = new Document(new Rectangle(288f, 144f), 10, 10, 10, 10);
                document.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());

                //   Document document = new Document(PageSize.A4.Rotate(), 20f, 10f, 20f, 10f);
                string path = Server.MapPath("~/Reports/Invoice/");
                string[] parts = InvoiceNo.Split('/');
                string numberString = parts[2];
                var filename = numberString + ".pdf";
                string filename1 = path + "" + numberString + "_" + aa + bb + cc + ".pdf";
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filename1, FileMode.Create));
                document.Open();
                document.AddTitle(InvoiceNo); //Statement-1
                document.AddHeader("content-disposition", "inline;filename=" + InvoiceNo + ".pdf");
                Session["fileName1"] = filename1;
                Session["filename"] = filename;

                int Count = 1;



                var data11 = (from c in db.Sales.Where(a => a.InvoiceNo == InvoiceNo)

                              join orderMain in db.orderMain on c.OrderNo equals orderMain.OrderNo into mainordeer
                              from om in mainordeer.DefaultIfEmpty()

                              join prdd in db.Products on c.ProductCode equals prdd.ProductCode into Products
                              from prd in Products.DefaultIfEmpty()

                              join GRNDe in db.GRNDetail on c.GRN_ID equals GRNDe.GRNId into GRNDetail
                              from grn in GRNDetail.DefaultIfEmpty()

                              select new
                              {
                                  OrderNo = om.OrderNo,
                                  OrderDate = om.OrderDate,

                                  InvoiceDate = c.InvoiceDate,
                                  DeliveredQty = c.DeliveredQty,
                                  Address = om.CustomerAddress,
                                  ManufacturingDate = grn.ManufacturingDate,
                                  ExpiryDate = grn.ExpiryDate,
                                  City = om.CustomerCity,
                                  Country = "India",
                                  State = "Maharashtra",
                                  Pincode = om.CustomerPincode,
                                  Mobile = om.CustomerMobile,
                                  GSTNO = om.CustomerGSTNo,
                                  CustomerName = om.CustomerName,
                                  BasicRate = c.BasicRate,
                                  Discount = c.Discount,
                                  GSTPercentage = c.GSTPercentage,
                                  IGSTAmount = c.IGSTAmount,
                                  SGSTAmount = c.SGSTAmount,
                                  CGSTAmount = c.CGSTAmount,
                                  TotalAmount = c.TotalAmount,
                                  BatchNo = c.BatchNo,
                                  NetAmount = c.BasicRate * c.DeliveredQty,
                                  ProductName = prd.ProductName,
                                  ProductCode = prd.ProductCode,
                                  HsnCode = prd.HsnCode,
                                  GSTPer = prd.GSTPer,
                                  Size = prd.Size,


                              }).ToList();


                ;
                var data =
                (from c in data11.Where(a => a.CustomerName != null)
                 group c by new
                 {
                     c.OrderNo,
                     c.OrderDate,
                     c.InvoiceDate,
                     c.DeliveredQty,
                     c.Address,
                     c.ManufacturingDate,
                     //  c.ExpiryDate,
                     c.City,
                     c.Country,
                     c.State,
                     c.Pincode,
                     c.Mobile,
                     c.GSTNO,
                     c.CustomerName,
                     c.BasicRate,
                     c.Discount,
                     c.GSTPercentage,
                     c.IGSTAmount,
                     c.SGSTAmount,
                     c.CGSTAmount,
                     c.TotalAmount,
                     c.BatchNo,
                     c.NetAmount,
                     c.ProductName,
                     c.ProductCode,
                     c.HsnCode,
                     c.GSTPer,
                     c.Size
                 } into gcs
                 select new
                 {

                     gcs.Key.OrderNo,
                     gcs.Key.OrderDate,
                     gcs.Key.InvoiceDate,
                     gcs.Key.DeliveredQty,
                     gcs.Key.Address,
                     ExpiryDate = gcs.Max(a => a.ExpiryDate),

                     gcs.Key.City,
                     gcs.Key.Country,
                     gcs.Key.State,
                     gcs.Key.Pincode,
                     gcs.Key.Mobile,
                     gcs.Key.GSTNO,
                     gcs.Key.CustomerName,
                     gcs.Key.BasicRate,
                     gcs.Key.Discount,
                     gcs.Key.GSTPercentage,
                     gcs.Key.IGSTAmount,
                     gcs.Key.SGSTAmount,
                     gcs.Key.CGSTAmount,
                     gcs.Key.TotalAmount,
                     gcs.Key.BatchNo,
                     gcs.Key.NetAmount,
                     gcs.Key.ProductName,
                     gcs.Key.ProductCode,
                     gcs.Key.HsnCode,
                     gcs.Key.GSTPer,
                     gcs.Key.Size

                 }).ToList();



                PdfPTable table0 = new PdfPTable(4);
                float[] widths0 = new float[] { 10f, 10f, 10f, 10f };
                table0.SetWidths(widths0);
                table0.WidthPercentage = 98;
                table0.HorizontalAlignment = 1;



                Paragraph para1 = new Paragraph();
                para1.Add(new Phrase("TAX INVOICE", FontFactory.GetFont("Arial", 20, Font.BOLD)));
                para1.Add(new Phrase("\n\n\n", FontFactory.GetFont("Arial", 5, Font.BOLD)));
                PdfPCell Cell101 = new PdfPCell(para1);
                Cell101.SetLeading(10f, 1.2f);
                Cell101.Border = Rectangle.NO_BORDER;
                Cell101.Colspan = 4;
                Cell101.HorizontalAlignment = 1;
                //Cell101.Alignment = Element.ALIGN_CENTER;
                table0.AddCell(Cell101);
                try
                {
                    Paragraph para2 = new Paragraph();
                    para2.Add(new Phrase("Invoice No:", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell Cell102 = new PdfPCell(para2);
                    Cell102.HorizontalAlignment = Element.ALIGN_LEFT;
                    Cell102.VerticalAlignment = Element.ALIGN_MIDDLE;
                    Cell102.PaddingTop = 10f;
                    Cell102.PaddingBottom = 10f;
                    table0.AddCell(Cell102);


                    Paragraph para3 = new Paragraph();
                    para3.Add(new Phrase("" + InvoiceNo, FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell Cell103 = new PdfPCell(para3);
                    Cell103.HorizontalAlignment = Element.ALIGN_LEFT;
                    Cell103.VerticalAlignment = Element.ALIGN_MIDDLE;
                    Cell103.PaddingTop = 10f;
                    Cell103.PaddingBottom = 10f;
                    table0.AddCell(Cell103);


                    Paragraph para4 = new Paragraph();
                    para4.Add(new Phrase("L.R. No.:", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell Cell104 = new PdfPCell(para4);
                    Cell104.HorizontalAlignment = Element.ALIGN_LEFT;
                    Cell104.VerticalAlignment = Element.ALIGN_MIDDLE;
                    Cell104.PaddingTop = 10f;
                    Cell104.PaddingBottom = 10f;
                    table0.AddCell(Cell104);


                    Paragraph para5 = new Paragraph();
                    para5.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell Cell105 = new PdfPCell(para5);
                    Cell105.HorizontalAlignment = Element.ALIGN_LEFT;
                    Cell105.VerticalAlignment = Element.ALIGN_MIDDLE;
                    Cell105.PaddingTop = 10f;
                    Cell105.PaddingBottom = 10f;
                    table0.AddCell(Cell105);

                }
                catch { }
                try
                {
                    Paragraph para2 = new Paragraph();
                    para2.Add(new Phrase("Invoice Date:", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell Cell102 = new PdfPCell(para2);
                    Cell102.HorizontalAlignment = Element.ALIGN_LEFT;
                    Cell102.VerticalAlignment = Element.ALIGN_MIDDLE;
                    Cell102.PaddingTop = 10f;
                    Cell102.PaddingBottom = 10f;
                    table0.AddCell(Cell102);

                    var XYZ = data[0].InvoiceDate.Value.ToString("dd-MM-yyyy");


                    Paragraph para3 = new Paragraph();
                    para3.Add(new Phrase("" + XYZ, FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell Cell103 = new PdfPCell(para3);
                    Cell103.HorizontalAlignment = Element.ALIGN_LEFT;
                    Cell103.VerticalAlignment = Element.ALIGN_MIDDLE;
                    Cell103.PaddingTop = 10f;
                    Cell103.PaddingBottom = 10f;
                    table0.AddCell(Cell103);


                    Paragraph para4 = new Paragraph();
                    para4.Add(new Phrase("L.R. Date:", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell Cell104 = new PdfPCell(para4);
                    Cell104.HorizontalAlignment = Element.ALIGN_LEFT;
                    Cell104.VerticalAlignment = Element.ALIGN_MIDDLE;
                    Cell104.PaddingTop = 10f;
                    Cell104.PaddingBottom = 10f;
                    table0.AddCell(Cell104);


                    Paragraph para5 = new Paragraph();
                    para5.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell Cell105 = new PdfPCell(para5);
                    Cell105.HorizontalAlignment = Element.ALIGN_LEFT;
                    Cell105.VerticalAlignment = Element.ALIGN_MIDDLE;
                    Cell105.PaddingTop = 10f;
                    Cell105.PaddingBottom = 10f;
                    table0.AddCell(Cell105);

                }
                catch { }
                try
                {
                    Paragraph para2 = new Paragraph();
                    para2.Add(new Phrase("Order No:", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell Cell102 = new PdfPCell(para2);
                    Cell102.HorizontalAlignment = Element.ALIGN_LEFT;
                    Cell102.VerticalAlignment = Element.ALIGN_MIDDLE;
                    Cell102.PaddingTop = 10f;
                    Cell102.PaddingBottom = 10f;
                    table0.AddCell(Cell102);


                    Paragraph para3 = new Paragraph();
                    para3.Add(new Phrase("" + data[0].OrderNo, FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell Cell103 = new PdfPCell(para3);
                    Cell103.HorizontalAlignment = Element.ALIGN_LEFT;
                    Cell103.VerticalAlignment = Element.ALIGN_MIDDLE;
                    Cell103.PaddingTop = 10f;
                    Cell103.PaddingBottom = 10f;
                    table0.AddCell(Cell103);


                    Paragraph para4 = new Paragraph();
                    para4.Add(new Phrase("Cases:", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell Cell104 = new PdfPCell(para4);
                    Cell104.HorizontalAlignment = Element.ALIGN_LEFT;
                    Cell104.VerticalAlignment = Element.ALIGN_MIDDLE;
                    Cell104.PaddingTop = 10f;
                    Cell104.PaddingBottom = 10f;
                    table0.AddCell(Cell104);


                    Paragraph para5 = new Paragraph();
                    para5.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell Cell105 = new PdfPCell(para5);
                    Cell105.HorizontalAlignment = 0;
                    table0.AddCell(Cell105);

                }
                catch { }
                try
                {
                    Paragraph para2 = new Paragraph();
                    para2.Add(new Phrase("Order Date:", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell Cell102 = new PdfPCell(para2);
                    Cell102.HorizontalAlignment = Element.ALIGN_LEFT;
                    Cell102.VerticalAlignment = Element.ALIGN_MIDDLE;
                    Cell102.PaddingTop = 10f;
                    Cell102.PaddingBottom = 10f;
                    table0.AddCell(Cell102);


                    var XYZZ = "";
                    try
                    {
                        XYZZ = data[0].OrderDate.Value.ToString("dd-MM-yyyy");
                    }
                    catch
                    {

                    }


                    Paragraph para3 = new Paragraph();
                    para3.Add(new Phrase("" + XYZZ, FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell Cell103 = new PdfPCell(para3);
                    Cell103.HorizontalAlignment = Element.ALIGN_LEFT;
                    Cell103.VerticalAlignment = Element.ALIGN_MIDDLE;
                    Cell103.PaddingTop = 10f;
                    Cell103.PaddingBottom = 10f;
                    table0.AddCell(Cell103);


                    Paragraph para4 = new Paragraph();
                    para4.Add(new Phrase("Due Date:", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell Cell104 = new PdfPCell(para4);
                    Cell104.HorizontalAlignment = Element.ALIGN_LEFT;
                    Cell104.VerticalAlignment = Element.ALIGN_MIDDLE;
                    Cell104.PaddingTop = 10f;
                    Cell104.PaddingBottom = 10f;
                    table0.AddCell(Cell104);


                    Paragraph para5 = new Paragraph();
                    para5.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell Cell105 = new PdfPCell(para5);
                    Cell105.HorizontalAlignment = Element.ALIGN_LEFT;
                    Cell105.VerticalAlignment = Element.ALIGN_MIDDLE;
                    Cell105.PaddingTop = 10f;
                    Cell105.PaddingBottom = 10f;
                    table0.AddCell(Cell105);

                }
                catch { }

                //try
                //{
                //    Paragraph para2 = new Paragraph();
                //    para2.Add(new Phrase("Transport :-", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                //    PdfPCell Cell102 = new PdfPCell(para2);
                //    Cell102.HorizontalAlignment = 0;
                //    Cell102.Colspan = 2;
                //    Cell102.Border = Rectangle.NO_BORDER;
                //    table0.AddCell(Cell102);


                //    Paragraph para3 = new Paragraph();
                //    para3.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                //    PdfPCell Cell103 = new PdfPCell(para3);
                //    Cell103.HorizontalAlignment = 0;
                //    Cell103.Colspan = 2;
                //    Cell103.Border = Rectangle.NO_BORDER;
                //    table0.AddCell(Cell103);



                //}
                //catch { }
                //try
                //{
                //    Paragraph para2 = new Paragraph();
                //    para2.Add(new Phrase("Dispatch date and time :-", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                //    PdfPCell Cell102 = new PdfPCell(para2);
                //    Cell102.HorizontalAlignment = 0;
                //    Cell102.Colspan = 2;
                //    Cell102.Border = Rectangle.NO_BORDER;
                //    table0.AddCell(Cell102);


                //    Paragraph para3 = new Paragraph();
                //    para3.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                //    PdfPCell Cell103 = new PdfPCell(para3);
                //    Cell103.HorizontalAlignment = 0;
                //    Cell103.Colspan = 2;
                //    Cell103.Border = Rectangle.NO_BORDER;
                //    table0.AddCell(Cell103);



                //}
                //catch { }
                //try
                //{
                //    Paragraph para2 = new Paragraph();
                //    para2.Add(new Phrase("Weight :-", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                //    PdfPCell Cell102 = new PdfPCell(para2);
                //    Cell102.HorizontalAlignment = 0;
                //    Cell102.Colspan = 2;
                //    Cell102.Border = Rectangle.NO_BORDER;
                //    table0.AddCell(Cell102);


                //    Paragraph para3 = new Paragraph();
                //    para3.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                //    PdfPCell Cell103 = new PdfPCell(para3);
                //    Cell103.HorizontalAlignment = 0;
                //    Cell103.Colspan = 2;
                //    Cell103.Border = Rectangle.NO_BORDER;
                //    table0.AddCell(Cell103);



                //}
                //catch { }
                //try
                //{
                //    Paragraph para2 = new Paragraph();
                //    para2.Add(new Phrase("Delivery :-", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                //    PdfPCell Cell102 = new PdfPCell(para2);
                //    Cell102.HorizontalAlignment = 0;
                //    Cell102.Colspan = 2;
                //    Cell102.Border = Rectangle.NO_BORDER;
                //    table0.AddCell(Cell102);


                //    Paragraph para3 = new Paragraph();
                //    para3.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                //    PdfPCell Cell103 = new PdfPCell(para3);
                //    Cell103.HorizontalAlignment = 0;
                //    Cell103.Colspan = 2;
                //    Cell103.Border = Rectangle.NO_BORDER;
                //    table0.AddCell(Cell103);



                //}
                //catch { }
                //try
                //{
                //    Paragraph para2 = new Paragraph();
                //    para2.Add(new Phrase("ORDER DATE :-", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                //    PdfPCell Cell102 = new PdfPCell(para2);
                //    Cell102.HorizontalAlignment = 0;
                //    Cell102.Colspan = 2;
                //    Cell102.Border = Rectangle.NO_BORDER;
                //    table0.AddCell(Cell102);


                //    Paragraph para3 = new Paragraph();
                //    para3.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                //    PdfPCell Cell103 = new PdfPCell(para3);
                //    Cell103.HorizontalAlignment = 0;
                //    Cell103.Colspan = 2;
                //    Cell103.Border = Rectangle.NO_BORDER;
                //    table0.AddCell(Cell103);

                //}
                //catch { }


                PdfPTable table1 = new PdfPTable(6);
                float[] widths1 = new float[] { 3f, 3f, 4f, 4f, 3f, 3f };
                table1.SetWidths(widths1);
                table1.WidthPercentage = 98;
                table1.HorizontalAlignment = Element.ALIGN_CENTER;

                string imageURL = Server.MapPath("/Photo/MicraftLogo.png");
                iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(imageURL);
                jpg.ScaleToFit(50, 80);
                jpg.SpacingBefore = 1f;
                jpg.SpacingAfter = 1f;
                jpg.Alignment = Element.ALIGN_CENTER;

                Chunk imageChunk = new Chunk(jpg, 0, 0, true);
                imageChunk.SetAnchor(jpg.Url);

                Paragraph paragraph = new Paragraph();
                paragraph.Alignment = Element.ALIGN_CENTER;
                paragraph.Add(imageChunk);
                paragraph.Add(new Phrase("\n", FontFactory.GetFont("Arial", 5, Font.BOLD)));
                paragraph.Add(new Phrase("Siddhivinayak Distributor", FontFactory.GetFont("Arial", 12, Font.BOLD)));
                paragraph.Add(new Phrase("\n", FontFactory.GetFont("Arial", 5, Font.BOLD)));
                paragraph.Add(new Phrase("Shop no 10, Suyog Navkar Building A, \n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                paragraph.Add(new Phrase("Near 7 Loves Chowk, Market Yard Road, Pune 411 037.\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                paragraph.Add(new Phrase("GSTIN: 27ABVPK5495R2Z9\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                paragraph.Add(new Phrase("DL.No: MH-PZ3517351,MH-PZ3517352\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));

                PdfPCell Cell1 = new PdfPCell();
                Cell1.AddElement(paragraph);
                Cell1.SetLeading(10f, 1.0f);
                Cell1.Colspan = 2;
                Cell1.HorizontalAlignment = Element.ALIGN_CENTER;
                table1.AddCell(Cell1);


                PdfPCell Cell2 = new PdfPCell(table0);
                Cell2.HorizontalAlignment = 0;

                Cell2.Colspan = 2;

                table1.AddCell(Cell2);
                var address = data[0].Address + ", " + data[0].City + ", " + data[0].Pincode + " " + data[0].State + " " + data[0].Country;

                Paragraph Para3 = new Paragraph();
                Para3.Add(new Phrase("Party Name: " + data[0].CustomerName + "\n", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                Para3.Add(new Phrase("Address : ", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                Para3.Add(new Phrase("" + address + "\n", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                Para3.Add(new Phrase("Mobile No :" + data[0].Mobile + "\n", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                Para3.Add(new Phrase("GST NO :" + data[0].GSTNO + "\n", FontFactory.GetFont("Arial", 9, Font.BOLD)));

                PdfPCell Cell3 = new PdfPCell(Para3);
                Cell3.HorizontalAlignment = 0;
                Cell3.Colspan = 2;
                table1.AddCell(Cell3);



                PdfPTable table2 = new PdfPTable(17);
                float[] widths2 = new float[] { 2f, 8f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 4f, 3f, 3f, 3f, 3f, 3f, 4f };
                table2.SetWidths(widths2);
                table2.WidthPercentage = 98;
                table2.HorizontalAlignment = 1;

                //3rd Row

                PdfPCell Cell8 = new PdfPCell(new Phrase(new Phrase("Sr.No.", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                Cell8.HorizontalAlignment = 1;
                Cell8.BackgroundColor = BaseColor.BLACK;
                table2.AddCell(Cell8);

                PdfPCell Cell9 = new PdfPCell(new Phrase(new Phrase("Product Name", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                Cell9.HorizontalAlignment = 1;
                Cell9.BackgroundColor = BaseColor.BLACK;
                table2.AddCell(Cell9);

                PdfPCell Cell10 = new PdfPCell(new Phrase(new Phrase("Pack", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                Cell10.HorizontalAlignment = 1;
                Cell10.BackgroundColor = BaseColor.BLACK;
                table2.AddCell(Cell10);

                PdfPCell Cell11 = new PdfPCell(new Phrase(new Phrase("Mfr", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                Cell11.HorizontalAlignment = 1;
                Cell11.BackgroundColor = BaseColor.BLACK;
                table2.AddCell(Cell11);

                PdfPCell Cell12 = new PdfPCell(new Phrase(new Phrase("Batch", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                Cell12.HorizontalAlignment = 1;
                Cell12.BackgroundColor = BaseColor.BLACK;
                table2.AddCell(Cell12);

                PdfPCell Cell13 = new PdfPCell(new Phrase(new Phrase("Box", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                Cell13.HorizontalAlignment = 1;
                Cell13.BackgroundColor = BaseColor.BLACK;
                table2.AddCell(Cell13);


                PdfPCell Cel4l11 = new PdfPCell(new Phrase(new Phrase("Tot Qty", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                Cel4l11.HorizontalAlignment = 1;
                Cel4l11.BackgroundColor = BaseColor.BLACK;
                table2.AddCell(Cel4l11);

                PdfPCell Cells12 = new PdfPCell(new Phrase(new Phrase("Exp Dt", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                Cells12.HorizontalAlignment = 1;
                Cells12.BackgroundColor = BaseColor.BLACK;
                table2.AddCell(Cells12);

                PdfPCell Cell513 = new PdfPCell(new Phrase(new Phrase("HSN", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                Cell513.HorizontalAlignment = 1;
                Cell513.BackgroundColor = BaseColor.BLACK;
                table2.AddCell(Cell513);

                PdfPCell Cel7l13 = new PdfPCell(new Phrase(new Phrase("MRP", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                Cel7l13.HorizontalAlignment = 1;
                Cel7l13.BackgroundColor = BaseColor.BLACK;
                table2.AddCell(Cel7l13);

                PdfPCell Cellsw13 = new PdfPCell(new Phrase(new Phrase("Rate", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                Cellsw13.HorizontalAlignment = 1;
                Cellsw13.BackgroundColor = BaseColor.BLACK;
                table2.AddCell(Cellsw13);

                PdfPCell Cellsd123 = new PdfPCell(new Phrase(new Phrase("Taxable", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                Cellsd123.HorizontalAlignment = 1;
                Cellsd123.BackgroundColor = BaseColor.BLACK;
                table2.AddCell(Cellsd123);

                PdfPCell Cell1543 = new PdfPCell(new Phrase(new Phrase("SGST", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                Cell1543.HorizontalAlignment = 1;
                Cell1543.BackgroundColor = BaseColor.BLACK;
                table2.AddCell(Cell1543);

                PdfPCell Cellsa13 = new PdfPCell(new Phrase(new Phrase("Value", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                Cellsa13.HorizontalAlignment = 1;
                Cellsa13.BackgroundColor = BaseColor.BLACK;
                table2.AddCell(Cellsa13);

                PdfPCell Cell15d43 = new PdfPCell(new Phrase(new Phrase("CGST", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                Cell15d43.HorizontalAlignment = 1;
                Cell15d43.BackgroundColor = BaseColor.BLACK;
                table2.AddCell(Cell15d43);

                PdfPCell Cells55a13 = new PdfPCell(new Phrase(new Phrase("Value", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                Cells55a13.HorizontalAlignment = 1;
                Cells55a13.BackgroundColor = BaseColor.BLACK;
                table2.AddCell(Cells55a13);

                PdfPCell Cellsd13 = new PdfPCell(new Phrase(new Phrase("Amount", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                Cellsd13.HorizontalAlignment = 1;
                Cellsd13.BackgroundColor = BaseColor.BLACK;
                table2.AddCell(Cellsd13);


                //4th Row

                foreach (var r in data)
                {
                    if (data.Count > 0)
                    {
                        var order = db.orderDetails.Where(a => a.OrderNo == r.OrderNo && a.ProductCode == r.ProductCode).FirstOrDefault();
                        Paragraph Para14 = new Paragraph();
                        Para14.Add(new Phrase("" + Count + "\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell Cell14 = new PdfPCell(Para14);
                        Cell14.HorizontalAlignment = 1;
                        Cell14.UseAscender = true;
                        table2.AddCell(Cell14);


                        Paragraph Para15 = new Paragraph();
                        Para15.Add(new Phrase("" + r.ProductName + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell Cell15 = new PdfPCell(Para15);
                        Cell15.HorizontalAlignment = 0;
                        table2.AddCell(Cell15);


                        //PACK
                        Paragraph Para16 = new Paragraph();
                        Para16.Add(new Phrase("" + r.Size + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell Cell16 = new PdfPCell(Para16);
                        Cell16.HorizontalAlignment = 1;
                        table2.AddCell(Cell16);

                        //MFR


                        var XYZZ = "";
                        try
                        {
                            //   XYZZ = r.ManufacturingDate.Value.ToString("dd-MM-yyyy");
                        }
                        catch
                        {

                        }


                        Paragraph Para1d6 = new Paragraph();
                        Para1d6.Add(new Phrase("" + XYZZ, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell Cells16 = new PdfPCell(Para1d6);
                        Cells16.HorizontalAlignment = 1;
                        table2.AddCell(Cells16);

                        //batck
                        Paragraph Para16s = new Paragraph();
                        Para16s.Add(new Phrase("" + r.BatchNo, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell Celsl16 = new PdfPCell(Para16s);
                        Celsl16.HorizontalAlignment = 1;
                        table2.AddCell(Celsl16);


                        //box
                        Paragraph Paradd16 = new Paragraph();
                        Paradd16.Add(new Phrase("Box", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell Cell1e6 = new PdfPCell(Paradd16);
                        Cell1e6.HorizontalAlignment = 1;
                        table2.AddCell(Cell1e6);


                        Paragraph Parafd16 = new Paragraph();
                        Parafd16.Add(new Phrase("" + r.DeliveredQty + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell Cell1df6 = new PdfPCell(Parafd16);
                        Cell1df6.HorizontalAlignment = 1;
                        table2.AddCell(Cell1df6);

                        var XYZZZ = "";
                        try
                        {
                            XYZZZ = r.ExpiryDate.Value.ToString("dd-MM-yyyy");
                        }
                        catch { }


                        //expdate
                        Paragraph Para1r6 = new Paragraph();
                        Para1r6.Add(new Phrase("" + XYZZZ + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell Para1sr6 = new PdfPCell(Para1r6);
                        Para1sr6.HorizontalAlignment = 1;
                        table2.AddCell(Para1sr6);


                        //hsn

                        Paragraph Para17 = new Paragraph();
                        Para17.Add(new Phrase("" + r.HsnCode + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell Cell17 = new PdfPCell(Para17);
                        Cell17.HorizontalAlignment = 1;
                        table2.AddCell(Cell17);



                        //mrp

                        Paragraph Para1757 = new Paragraph();
                        Para1757.Add(new Phrase("" + order.Price, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell Cellee17 = new PdfPCell(Para1757);
                        Cellee17.HorizontalAlignment = 1;
                        table2.AddCell(Cellee17);

                        Discount = Discount + (Convert.ToDecimal(order.DiscountAmount));

                        Paragraph Para1sd7 = new Paragraph();
                        Para1sd7.Add(new Phrase("" + order.Price, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell Cell1sds7 = new PdfPCell(Para1sd7);
                        Cell1sds7.HorizontalAlignment = 1;
                        table2.AddCell(Cell1sds7);

                        TotAmt = order.Price * order.DeliveredQty;

                        Paragraph Parjhja19 = new Paragraph();
                        Parjhja19.Add(new Phrase("" + string.Format("{0:0.00 }", TotAmt) + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell Cell19 = new PdfPCell(Parjhja19);
                        Cell19.HorizontalAlignment = 2;
                        table2.AddCell(Cell19);


                        var gst = r.GSTPer / 2;

                        //SGST
                        Paragraph Para18 = new Paragraph();
                        Para18.Add(new Phrase(gst + "%", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell Cell18 = new PdfPCell(Para18);
                        Cell18.HorizontalAlignment = 1;
                        table2.AddCell(Cell18);


                        // value
                        Paragraph Paradf18 = new Paragraph();
                        Paradf18.Add(new Phrase("" + string.Format("{0:0.00 }", order.CGSTAmount) + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell Parabdf18 = new PdfPCell(Paradf18);
                        Parabdf18.HorizontalAlignment = 1;
                        table2.AddCell(Parabdf18);

                        CGSTAmount = CGSTAmount + (Convert.ToDecimal(order.CGSTAmount));

                        //CGST
                        Paragraph Pararer18 = new Paragraph();
                        Pararer18.Add(new Phrase(gst + "%", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell Cell18eds = new PdfPCell(Pararer18);
                        Cell18eds.HorizontalAlignment = 1;
                        table2.AddCell(Cell18eds);


                        // value
                        Paragraph Para18445 = new Paragraph();
                        Para18445.Add(new Phrase("" + string.Format("{0:0.00 }", order.SGSTAmount) + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell Cell18787 = new PdfPCell(Para18445);
                        Cell18787.HorizontalAlignment = 1;
                        table2.AddCell(Cell18787);

                        SGSTAmount = SGSTAmount + (Convert.ToDecimal(order.SGSTAmount));

                        //var TotAmt = order.Price * order.DeliveredQty;


                        var TotAmt1 = TotAmt + order.SGSTAmount + order.CGSTAmount + order.IGSTAmount;
                        Paragraph Parjhjaq19 = new Paragraph();
                        Parjhjaq19.Add(new Phrase("" + string.Format("{0:0.00 }", TotAmt1) + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell Cellq19 = new PdfPCell(Parjhjaq19);
                        Cellq19.HorizontalAlignment = 2;
                        table2.AddCell(Cellq19);

                        TotalAmount = Convert.ToDecimal(TotalAmount) + Convert.ToDecimal(TotAmt);
                        DisplayAmt = Convert.ToDecimal(DisplayAmt) + Convert.ToDecimal(TotAmt1);
                        //TotAmt = 0;
                        Count++;
                    }
                }



                PdfPTable table3 = new PdfPTable(10);
                float[] widths3 = new float[] { 3f, 3f, 3f, 3f, 3f, 3f, 3f, 5f, 3f, 3f };
                table3.SetWidths(widths3);
                table3.WidthPercentage = 98;
                table3.HorizontalAlignment = 1;





                var GstData =
                (from c in db.orderDetails.Where(a => a.OrderNo == orderno)
                 group c by new
                 {
                     c.GSTPercentage,
                 } into gcs
                 select new
                 {
                     SGSTAmount = gcs.Sum(a => a.SGSTAmount),
                     CGSTAmount = gcs.Sum(a => a.CGSTAmount),
                     TotalTAxAmount = gcs.Sum(a => a.CGSTAmount + a.SGSTAmount),
                     GSTPer = gcs.Key.GSTPercentage
                 }).ToList();



                try
                {
                    PdfPCell Cellsa13121 = new PdfPCell(new Phrase(new Phrase("Class", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.WHITE))));
                    Cellsa13121.HorizontalAlignment = 1;
                    Cellsa13121.BackgroundColor = BaseColor.BLACK;
                    table3.AddCell(Cellsa13121);


                    PdfPCell Cellsa13122 = new PdfPCell(new Phrase(new Phrase("TOTAL", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.WHITE))));
                    Cellsa13122.HorizontalAlignment = 1;
                    Cellsa13122.BackgroundColor = BaseColor.BLACK;
                    table3.AddCell(Cellsa13122);

                    PdfPCell Cellsa13123 = new PdfPCell(new Phrase(new Phrase("SCHEME", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.WHITE))));
                    Cellsa13123.HorizontalAlignment = 1;
                    Cellsa13123.BackgroundColor = BaseColor.BLACK;
                    table3.AddCell(Cellsa13123);

                    PdfPCell Cellsa13124 = new PdfPCell(new Phrase(new Phrase("DISCOUNT", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.WHITE))));
                    Cellsa13124.HorizontalAlignment = 1;
                    Cellsa13124.BackgroundColor = BaseColor.BLACK;
                    table3.AddCell(Cellsa13124);

                    PdfPCell Cellsa13125 = new PdfPCell(new Phrase(new Phrase("SGST", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.WHITE))));
                    Cellsa13125.HorizontalAlignment = 1;
                    Cellsa13125.BackgroundColor = BaseColor.BLACK;
                    table3.AddCell(Cellsa13125);

                    PdfPCell Cellsa13126 = new PdfPCell(new Phrase(new Phrase("CGST", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.WHITE))));
                    Cellsa13126.HorizontalAlignment = 1;
                    Cellsa13126.BackgroundColor = BaseColor.BLACK;
                    table3.AddCell(Cellsa13126);

                    PdfPCell Cellsa13127 = new PdfPCell(new Phrase(new Phrase("TOTAL GST", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.WHITE))));
                    Cellsa13127.HorizontalAlignment = 1;
                    Cellsa13127.BackgroundColor = BaseColor.BLACK;
                    table3.AddCell(Cellsa13127);

                    PdfPCell Cellsa13128 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                    Cellsa13128.HorizontalAlignment = 1;
                    Cellsa13128.BackgroundColor = BaseColor.BLACK;
                    table3.AddCell(Cellsa13128);

                    PdfPCell Cellsa131200 = new PdfPCell(new Phrase(new Phrase("" + string.Format("{0:0.00 }", DisplayAmt), FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                    Cellsa131200.HorizontalAlignment = 2;
                    Cellsa131200.BackgroundColor = BaseColor.BLACK;
                    Cellsa131200.Colspan = 2;
                    table3.AddCell(Cellsa131200);


                }
                catch { }

                var GSt_5 = GstData.Where(a => a.GSTPer == 5).FirstOrDefault();
                if (GSt_5 != null)
                {
                    try
                    {
                        PdfPCell Cellsa13121 = new PdfPCell(new Phrase(new Phrase("GST: 5.00%", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13121.HorizontalAlignment = 1;
                        Cellsa13121.BackgroundColor = BaseColor.WHITE;
                        Cellsa13121.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13121);


                        PdfPCell Cellsa13122 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13122.HorizontalAlignment = 1;
                        Cellsa13122.Border = Rectangle.LEFT_BORDER;
                        Cellsa13122.BackgroundColor = BaseColor.WHITE;
                        table3.AddCell(Cellsa13122);

                        PdfPCell Cellsa13123 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13123.HorizontalAlignment = 1;
                        Cellsa13123.Border = Rectangle.LEFT_BORDER;
                        Cellsa13123.BackgroundColor = BaseColor.WHITE;
                        table3.AddCell(Cellsa13123);

                        PdfPCell Cellsa13124 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13124.HorizontalAlignment = 1;
                        Cellsa13124.BackgroundColor = BaseColor.WHITE;
                        Cellsa13124.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13124);

                        PdfPCell Cellsa13125 = new PdfPCell(new Phrase(new Phrase("" + GSt_5.SGSTAmount, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13125.HorizontalAlignment = 1;
                        Cellsa13125.BackgroundColor = BaseColor.WHITE;
                        Cellsa13125.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13125);

                        PdfPCell Cellsa13126 = new PdfPCell(new Phrase(new Phrase("" + GSt_5.CGSTAmount, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13126.HorizontalAlignment = 1;
                        Cellsa13126.BackgroundColor = BaseColor.WHITE;
                        Cellsa13126.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13126);

                        PdfPCell Cellsa13127 = new PdfPCell(new Phrase(new Phrase("" + GSt_5.TotalTAxAmount, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13127.HorizontalAlignment = 1;
                        Cellsa13127.BackgroundColor = BaseColor.WHITE;
                        Cellsa13127.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13127);

                        PdfPCell Cellsa13128 = new PdfPCell(new Phrase(new Phrase("Total Items :-", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13128.HorizontalAlignment = 1;
                        Cellsa13128.BackgroundColor = BaseColor.WHITE;
                        Cellsa13128.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13128);

                        PdfPCell Cellsa131200 = new PdfPCell(new Phrase(new Phrase("DIS AMT.", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa131200.HorizontalAlignment = 0;
                        Cellsa131200.BackgroundColor = BaseColor.WHITE;
                        Cellsa131200.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa131200);

                        PdfPCell Cellsa131210 = new PdfPCell(new Phrase(new Phrase("" + string.Format("{0:0.00 }", Discount), FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa131210.HorizontalAlignment = 2;
                        Cellsa131210.BackgroundColor = BaseColor.WHITE;
                        Cellsa131210.Border = Rectangle.RIGHT_BORDER;
                        table3.AddCell(Cellsa131210);


                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        PdfPCell Cellsa13121 = new PdfPCell(new Phrase(new Phrase("GST: 5.00%", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13121.HorizontalAlignment = 1;
                        Cellsa13121.BackgroundColor = BaseColor.WHITE;
                        Cellsa13121.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13121);


                        PdfPCell Cellsa13122 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13122.HorizontalAlignment = 1;
                        Cellsa13122.Border = Rectangle.LEFT_BORDER;
                        Cellsa13122.BackgroundColor = BaseColor.WHITE;
                        table3.AddCell(Cellsa13122);

                        PdfPCell Cellsa13123 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13123.HorizontalAlignment = 1;
                        Cellsa13123.Border = Rectangle.LEFT_BORDER;
                        Cellsa13123.BackgroundColor = BaseColor.WHITE;
                        table3.AddCell(Cellsa13123);

                        PdfPCell Cellsa13124 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13124.HorizontalAlignment = 1;
                        Cellsa13124.BackgroundColor = BaseColor.WHITE;
                        Cellsa13124.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13124);

                        PdfPCell Cellsa13125 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13125.HorizontalAlignment = 1;
                        Cellsa13125.BackgroundColor = BaseColor.WHITE;
                        Cellsa13125.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13125);

                        PdfPCell Cellsa13126 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13126.HorizontalAlignment = 1;
                        Cellsa13126.BackgroundColor = BaseColor.WHITE;
                        Cellsa13126.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13126);

                        PdfPCell Cellsa13127 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13127.HorizontalAlignment = 1;
                        Cellsa13127.BackgroundColor = BaseColor.WHITE;
                        Cellsa13127.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13127);

                        PdfPCell Cellsa13128 = new PdfPCell(new Phrase(new Phrase("Total Items :-", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13128.HorizontalAlignment = 1;
                        Cellsa13128.BackgroundColor = BaseColor.WHITE;
                        Cellsa13128.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13128);

                        PdfPCell Cellsa131200 = new PdfPCell(new Phrase(new Phrase("DIS AMT.", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa131200.HorizontalAlignment = 0;
                        Cellsa131200.BackgroundColor = BaseColor.WHITE;
                        Cellsa131200.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa131200);

                        PdfPCell Cellsa131210 = new PdfPCell(new Phrase(new Phrase("" + string.Format("{0:0.00 }", Discount), FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa131210.HorizontalAlignment = 2;
                        Cellsa131210.BackgroundColor = BaseColor.WHITE;
                        Cellsa131210.Border = Rectangle.RIGHT_BORDER;
                        table3.AddCell(Cellsa131210);


                    }
                    catch { }
                }





                var GSt_12 = GstData.Where(a => a.GSTPer == 12).FirstOrDefault();
                if (GSt_12 != null)
                {
                    try
                    {
                        PdfPCell Cellsa13121 = new PdfPCell(new Phrase(new Phrase("GST: 12.00%", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13121.HorizontalAlignment = 1;
                        Cellsa13121.BackgroundColor = BaseColor.WHITE;
                        Cellsa13121.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13121);


                        PdfPCell Cellsa13122 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13122.HorizontalAlignment = 1;
                        Cellsa13122.BackgroundColor = BaseColor.WHITE;
                        Cellsa13122.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13122);

                        PdfPCell Cellsa13123 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13123.HorizontalAlignment = 1;
                        Cellsa13123.BackgroundColor = BaseColor.WHITE;
                        Cellsa13123.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13123);

                        PdfPCell Cellsa13124 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13124.HorizontalAlignment = 1;
                        Cellsa13124.BackgroundColor = BaseColor.WHITE;
                        Cellsa13124.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13124);

                        PdfPCell Cellsa13125 = new PdfPCell(new Phrase(new Phrase("" + GSt_12.SGSTAmount, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13125.HorizontalAlignment = 1;
                        Cellsa13125.BackgroundColor = BaseColor.WHITE;
                        Cellsa13125.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13125);

                        PdfPCell Cellsa13126 = new PdfPCell(new Phrase(new Phrase("" + GSt_12.CGSTAmount, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13126.HorizontalAlignment = 1;
                        Cellsa13126.BackgroundColor = BaseColor.WHITE;
                        Cellsa13126.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13126);

                        PdfPCell Cellsa13127 = new PdfPCell(new Phrase(new Phrase("" + GSt_12.TotalTAxAmount, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13127.HorizontalAlignment = 1;
                        Cellsa13127.BackgroundColor = BaseColor.WHITE;
                        Cellsa13127.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13127);

                        PdfPCell Cellsa13128 = new PdfPCell(new Phrase(new Phrase("Total Qty :-", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13128.HorizontalAlignment = 1;
                        Cellsa13128.BackgroundColor = BaseColor.WHITE;
                        Cellsa13128.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13128);

                        PdfPCell Cellsa131200 = new PdfPCell(new Phrase(new Phrase("SGST PAYABLE", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa131200.HorizontalAlignment = 0;
                        Cellsa131200.BackgroundColor = BaseColor.WHITE;
                        Cellsa131200.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa131200);

                        PdfPCell Cellsa131210 = new PdfPCell(new Phrase(new Phrase("" + string.Format("{0:0.00 }", SGSTAmount), FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa131210.HorizontalAlignment = 2;
                        Cellsa131210.BackgroundColor = BaseColor.WHITE;
                        Cellsa131210.Border = Rectangle.RIGHT_BORDER;
                        table3.AddCell(Cellsa131210);


                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        PdfPCell Cellsa13121 = new PdfPCell(new Phrase(new Phrase("GST: 12.00%", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13121.HorizontalAlignment = 1;
                        Cellsa13121.BackgroundColor = BaseColor.WHITE;
                        Cellsa13121.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13121);


                        PdfPCell Cellsa13122 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13122.HorizontalAlignment = 1;
                        Cellsa13122.BackgroundColor = BaseColor.WHITE;
                        Cellsa13122.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13122);

                        PdfPCell Cellsa13123 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13123.HorizontalAlignment = 1;
                        Cellsa13123.BackgroundColor = BaseColor.WHITE;
                        Cellsa13123.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13123);

                        PdfPCell Cellsa13124 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13124.HorizontalAlignment = 1;
                        Cellsa13124.BackgroundColor = BaseColor.WHITE;
                        Cellsa13124.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13124);

                        PdfPCell Cellsa13125 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13125.HorizontalAlignment = 1;
                        Cellsa13125.BackgroundColor = BaseColor.WHITE;
                        Cellsa13125.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13125);

                        PdfPCell Cellsa13126 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13126.HorizontalAlignment = 1;
                        Cellsa13126.BackgroundColor = BaseColor.WHITE;
                        Cellsa13126.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13126);

                        PdfPCell Cellsa13127 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13127.HorizontalAlignment = 1;
                        Cellsa13127.BackgroundColor = BaseColor.WHITE;
                        Cellsa13127.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13127);

                        PdfPCell Cellsa13128 = new PdfPCell(new Phrase(new Phrase("Total Qty :-", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13128.HorizontalAlignment = 1;
                        Cellsa13128.BackgroundColor = BaseColor.WHITE;
                        Cellsa13128.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13128);

                        PdfPCell Cellsa131200 = new PdfPCell(new Phrase(new Phrase("SGST PAYABLE", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa131200.HorizontalAlignment = 0;
                        Cellsa131200.BackgroundColor = BaseColor.WHITE;
                        Cellsa131200.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa131200);

                        PdfPCell Cellsa131210 = new PdfPCell(new Phrase(new Phrase("" + string.Format("{0:0.00 }", SGSTAmount), FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa131210.HorizontalAlignment = 2;
                        Cellsa131210.BackgroundColor = BaseColor.WHITE;
                        Cellsa131210.Border = Rectangle.RIGHT_BORDER;
                        table3.AddCell(Cellsa131210);


                    }
                    catch { }
                }





                var GSt_18 = GstData.Where(a => a.GSTPer == 18).FirstOrDefault();
                if (GSt_18 != null)
                {
                    try
                    {
                        PdfPCell Cellsa13121 = new PdfPCell(new Phrase(new Phrase("GST: 18.00%", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13121.HorizontalAlignment = 1;
                        Cellsa13121.BackgroundColor = BaseColor.WHITE;
                        Cellsa13121.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13121);


                        PdfPCell Cellsa13122 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13122.HorizontalAlignment = 1;
                        Cellsa13122.BackgroundColor = BaseColor.WHITE;
                        Cellsa13122.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13122);

                        PdfPCell Cellsa13123 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13123.HorizontalAlignment = 1;
                        Cellsa13123.BackgroundColor = BaseColor.WHITE;
                        Cellsa13123.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13123);

                        PdfPCell Cellsa13124 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13124.HorizontalAlignment = 1;
                        Cellsa13124.BackgroundColor = BaseColor.WHITE;
                        Cellsa13124.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13124);

                        PdfPCell Cellsa13125 = new PdfPCell(new Phrase(new Phrase("" + GSt_18.CGSTAmount, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13125.HorizontalAlignment = 1;
                        Cellsa13125.BackgroundColor = BaseColor.WHITE;
                        Cellsa13125.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13125);

                        PdfPCell Cellsa13126 = new PdfPCell(new Phrase(new Phrase("" + GSt_18.SGSTAmount, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13126.HorizontalAlignment = 1;
                        Cellsa13126.BackgroundColor = BaseColor.WHITE;
                        Cellsa13126.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13126);

                        PdfPCell Cellsa13127 = new PdfPCell(new Phrase(new Phrase("" + GSt_18.TotalTAxAmount, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13127.HorizontalAlignment = 1;
                        Cellsa13127.BackgroundColor = BaseColor.WHITE;
                        Cellsa13127.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13127);

                        PdfPCell Cellsa13128 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13128.HorizontalAlignment = 1;
                        Cellsa13128.BackgroundColor = BaseColor.WHITE;
                        Cellsa13128.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13128);

                        PdfPCell Cellsa131200 = new PdfPCell(new Phrase(new Phrase("CGST PAYABLE", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa131200.HorizontalAlignment = 0;
                        Cellsa131200.BackgroundColor = BaseColor.WHITE;
                        Cellsa131200.Border = Rectangle.LEFT_BORDER;

                        table3.AddCell(Cellsa131200);

                        PdfPCell Cellsa131210 = new PdfPCell(new Phrase(new Phrase("" + string.Format("{0:0.00 }", CGSTAmount), FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa131210.HorizontalAlignment = 2;
                        Cellsa131210.BackgroundColor = BaseColor.WHITE;
                        Cellsa131210.Border = Rectangle.RIGHT_BORDER;
                        table3.AddCell(Cellsa131210);


                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        PdfPCell Cellsa13121 = new PdfPCell(new Phrase(new Phrase("GST: 18.00%", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13121.HorizontalAlignment = 1;
                        Cellsa13121.BackgroundColor = BaseColor.WHITE;
                        Cellsa13121.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13121);


                        PdfPCell Cellsa13122 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13122.HorizontalAlignment = 1;
                        Cellsa13122.BackgroundColor = BaseColor.WHITE;
                        Cellsa13122.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13122);

                        PdfPCell Cellsa13123 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13123.HorizontalAlignment = 1;
                        Cellsa13123.BackgroundColor = BaseColor.WHITE;
                        Cellsa13123.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13123);

                        PdfPCell Cellsa13124 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13124.HorizontalAlignment = 1;
                        Cellsa13124.BackgroundColor = BaseColor.WHITE;
                        Cellsa13124.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13124);

                        PdfPCell Cellsa13125 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13125.HorizontalAlignment = 1;
                        Cellsa13125.BackgroundColor = BaseColor.WHITE;
                        Cellsa13125.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13125);

                        PdfPCell Cellsa13126 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13126.HorizontalAlignment = 1;
                        Cellsa13126.BackgroundColor = BaseColor.WHITE;
                        Cellsa13126.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13126);

                        PdfPCell Cellsa13127 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13127.HorizontalAlignment = 1;
                        Cellsa13127.BackgroundColor = BaseColor.WHITE;
                        Cellsa13127.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13127);

                        PdfPCell Cellsa13128 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13128.HorizontalAlignment = 1;
                        Cellsa13128.BackgroundColor = BaseColor.WHITE;
                        Cellsa13128.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13128);

                        PdfPCell Cellsa131200 = new PdfPCell(new Phrase(new Phrase("CGST PAYABLE", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa131200.HorizontalAlignment = 0;
                        Cellsa131200.BackgroundColor = BaseColor.WHITE;
                        Cellsa131200.Border = Rectangle.LEFT_BORDER;

                        table3.AddCell(Cellsa131200);

                        PdfPCell Cellsa131210 = new PdfPCell(new Phrase(new Phrase("" + string.Format("{0:0.00 }", CGSTAmount), FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa131210.HorizontalAlignment = 2;
                        Cellsa131210.BackgroundColor = BaseColor.WHITE;
                        Cellsa131210.Border = Rectangle.RIGHT_BORDER;
                        table3.AddCell(Cellsa131210);


                    }
                    catch { }
                }


                var GSt_28 = GstData.Where(a => a.GSTPer == 28).FirstOrDefault();
                if (GSt_28 != null)
                {
                    try
                    {
                        PdfPCell Cellsa13121 = new PdfPCell(new Phrase(new Phrase("GST: 28.00%", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13121.HorizontalAlignment = 1;
                        Cellsa13121.BackgroundColor = BaseColor.WHITE;
                        Cellsa13121.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13121);


                        PdfPCell Cellsa13122 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13122.HorizontalAlignment = 1;
                        Cellsa13122.BackgroundColor = BaseColor.WHITE;
                        Cellsa13122.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13122);

                        PdfPCell Cellsa13123 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13123.HorizontalAlignment = 1;
                        Cellsa13123.BackgroundColor = BaseColor.WHITE;
                        Cellsa13123.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13123);

                        PdfPCell Cellsa13124 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13124.HorizontalAlignment = 1;
                        Cellsa13124.BackgroundColor = BaseColor.WHITE;
                        Cellsa13124.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13124);

                        PdfPCell Cellsa13125 = new PdfPCell(new Phrase(new Phrase("" + GSt_28.CGSTAmount, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13125.HorizontalAlignment = 1;
                        Cellsa13125.BackgroundColor = BaseColor.WHITE;
                        Cellsa13125.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13125);

                        PdfPCell Cellsa13126 = new PdfPCell(new Phrase(new Phrase("" + GSt_28.SGSTAmount, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13126.HorizontalAlignment = 1;
                        Cellsa13126.BackgroundColor = BaseColor.WHITE;
                        Cellsa13126.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13126);

                        PdfPCell Cellsa13127 = new PdfPCell(new Phrase(new Phrase("" + GSt_28.TotalTAxAmount, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13127.HorizontalAlignment = 1;
                        Cellsa13127.BackgroundColor = BaseColor.WHITE;
                        Cellsa13127.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13127);

                        PdfPCell Cellsa13128 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13128.HorizontalAlignment = 1;
                        Cellsa13128.BackgroundColor = BaseColor.WHITE;
                        Cellsa13128.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13128);

                        PdfPCell Cellsa131200 = new PdfPCell(new Phrase(new Phrase("ROUND OFF", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa131200.HorizontalAlignment = 0;
                        Cellsa131200.BackgroundColor = BaseColor.WHITE;
                        Cellsa131200.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa131200);


                        var xx = (TotalAmount - Discount) + CGSTAmount + SGSTAmount;
                        TotalAmount = xx;

                        PdfPCell Cellsa131210 = new PdfPCell(new Phrase(new Phrase("" + string.Format("{0:0.00 }", xx), FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa131210.HorizontalAlignment = 2;
                        Cellsa131210.BackgroundColor = BaseColor.WHITE;
                        Cellsa131210.Border = Rectangle.RIGHT_BORDER;
                        table3.AddCell(Cellsa131210);


                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        PdfPCell Cellsa13121 = new PdfPCell(new Phrase(new Phrase("GST: 28.00%", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13121.HorizontalAlignment = 1;
                        Cellsa13121.BackgroundColor = BaseColor.WHITE;
                        Cellsa13121.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13121);


                        PdfPCell Cellsa13122 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13122.HorizontalAlignment = 1;
                        Cellsa13122.BackgroundColor = BaseColor.WHITE;
                        Cellsa13122.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13122);

                        PdfPCell Cellsa13123 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13123.HorizontalAlignment = 1;
                        Cellsa13123.BackgroundColor = BaseColor.WHITE;
                        Cellsa13123.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13123);

                        PdfPCell Cellsa13124 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13124.HorizontalAlignment = 1;
                        Cellsa13124.BackgroundColor = BaseColor.WHITE;
                        Cellsa13124.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13124);

                        PdfPCell Cellsa13125 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13125.HorizontalAlignment = 1;
                        Cellsa13125.BackgroundColor = BaseColor.WHITE;
                        Cellsa13125.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13125);

                        PdfPCell Cellsa13126 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13126.HorizontalAlignment = 1;
                        Cellsa13126.BackgroundColor = BaseColor.WHITE;
                        Cellsa13126.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13126);

                        PdfPCell Cellsa13127 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13127.HorizontalAlignment = 1;
                        Cellsa13127.BackgroundColor = BaseColor.WHITE;
                        Cellsa13127.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13127);

                        PdfPCell Cellsa13128 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa13128.HorizontalAlignment = 1;
                        Cellsa13128.BackgroundColor = BaseColor.WHITE;
                        Cellsa13128.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa13128);

                        PdfPCell Cellsa131200 = new PdfPCell(new Phrase(new Phrase("ROUND OFF", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa131200.HorizontalAlignment = 0;
                        Cellsa131200.BackgroundColor = BaseColor.WHITE;
                        Cellsa131200.Border = Rectangle.LEFT_BORDER;
                        table3.AddCell(Cellsa131200);


                        var xx = (TotalAmount - Discount) + CGSTAmount + SGSTAmount;
                        TotalAmount = xx;

                        PdfPCell Cellsa131210 = new PdfPCell(new Phrase(new Phrase("" + string.Format("{0:0.00 }", xx), FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                        Cellsa131210.HorizontalAlignment = 2;
                        Cellsa131210.BackgroundColor = BaseColor.WHITE;
                        Cellsa131210.Border = Rectangle.RIGHT_BORDER;
                        table3.AddCell(Cellsa131210);

                    }
                    catch { }
                }

                try
                {
                    PdfPCell Cellsa13121 = new PdfPCell(new Phrase(new Phrase("TOTAL", FontFactory.GetFont("Arial", 9, Font.BOLD, BaseColor.BLACK))));
                    Cellsa13121.HorizontalAlignment = 0;
                    Cellsa13121.BackgroundColor = BaseColor.WHITE;
                    table3.AddCell(Cellsa13121);


                    PdfPCell Cellsa13122 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                    Cellsa13122.HorizontalAlignment = 1;
                    Cellsa13122.BackgroundColor = BaseColor.WHITE;
                    table3.AddCell(Cellsa13122);

                    PdfPCell Cellsa13123 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                    Cellsa13123.HorizontalAlignment = 1;
                    Cellsa13123.BackgroundColor = BaseColor.WHITE;
                    table3.AddCell(Cellsa13123);

                    PdfPCell Cellsa13124 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                    Cellsa13124.HorizontalAlignment = 1;
                    Cellsa13124.BackgroundColor = BaseColor.WHITE;
                    table3.AddCell(Cellsa13124);

                    PdfPCell Cellsa13125 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                    Cellsa13125.HorizontalAlignment = 1;
                    Cellsa13125.BackgroundColor = BaseColor.WHITE;
                    table3.AddCell(Cellsa13125);

                    PdfPCell Cellsa13126 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                    Cellsa13126.HorizontalAlignment = 1;
                    Cellsa13126.BackgroundColor = BaseColor.WHITE;
                    table3.AddCell(Cellsa13126);

                    PdfPCell Cellsa13127 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                    Cellsa13127.HorizontalAlignment = 1;
                    Cellsa13127.BackgroundColor = BaseColor.WHITE;
                    table3.AddCell(Cellsa13127);

                    PdfPCell Cellsa13128 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                    Cellsa13128.HorizontalAlignment = 1;
                    Cellsa13128.BackgroundColor = BaseColor.WHITE;
                    table3.AddCell(Cellsa13128);

                    PdfPCell Cellsa131200 = new PdfPCell(new Phrase(new Phrase("CR/DR NOTE", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                    Cellsa131200.HorizontalAlignment = 0;
                    Cellsa131200.BackgroundColor = BaseColor.WHITE;
                    Cellsa131200.Border = Rectangle.LEFT_BORDER;
                    table3.AddCell(Cellsa131200);

                    PdfPCell Cellsa131210 = new PdfPCell(new Phrase(new Phrase("0.00", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                    Cellsa131210.HorizontalAlignment = 2;
                    Cellsa131210.BackgroundColor = BaseColor.WHITE;
                    Cellsa131210.Border = Rectangle.RIGHT_BORDER;
                    table3.AddCell(Cellsa131210);


                }
                catch { }

                try
                {
                    var AmtInwords = words(Convert.ToInt32(Math.Round(TotalAmount)));
                    PdfPCell Cellsa13121 = new PdfPCell(new Phrase(new Phrase("Amount In Word : " + AmtInwords, FontFactory.GetFont("Arial", 9, Font.BOLD, BaseColor.BLACK))));
                    Cellsa13121.HorizontalAlignment = 0;
                    Cellsa13121.BackgroundColor = BaseColor.WHITE;
                    Cellsa13121.Colspan = 8;
                    table3.AddCell(Cellsa13121);


                    PdfPCell Cellsa131200 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                    Cellsa131200.HorizontalAlignment = 1;
                    Cellsa131200.BackgroundColor = BaseColor.WHITE;
                    Cellsa131200.Border = Rectangle.NO_BORDER;
                    table3.AddCell(Cellsa131200);

                    PdfPCell Cellsa1312s00 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                    Cellsa1312s00.HorizontalAlignment = 1;
                    Cellsa1312s00.BackgroundColor = BaseColor.WHITE;
                    Cellsa1312s00.Border = Rectangle.RIGHT_BORDER;
                    table3.AddCell(Cellsa1312s00);


                }
                catch { }

                try
                {
                    PdfPCell Cellsa13121 = new PdfPCell(new Phrase(new Phrase("Remark", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                    Cellsa13121.HorizontalAlignment = 0;
                    Cellsa13121.BackgroundColor = BaseColor.WHITE;
                    Cellsa13121.Colspan = 8;
                    table3.AddCell(Cellsa13121);


                    PdfPCell Cellsa131200 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                    Cellsa131200.HorizontalAlignment = 1;
                    Cellsa131200.BackgroundColor = BaseColor.WHITE;

                    Cellsa131200.Border = Rectangle.NO_BORDER;
                    table3.AddCell(Cellsa131200);

                    PdfPCell Cellsa1312s00 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                    Cellsa1312s00.HorizontalAlignment = 1;
                    Cellsa1312s00.BackgroundColor = BaseColor.WHITE;
                    Cellsa1312s00.Border = Rectangle.RIGHT_BORDER;
                    table3.AddCell(Cellsa1312s00);


                }
                catch { }




                //BAnk details

                try
                {
                    Paragraph Para5453 = new Paragraph();
                    Para5453.Add(new Phrase("BANK DETAILS AS :- \n", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    Para5453.Add(new Phrase("\n", FontFactory.GetFont("Arial", 5, Font.NORMAL)));
                    Para5453.Add(new Phrase("Bank Name : Indian Bank\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                    Para5453.Add(new Phrase("Branch : Pune Cantonment branch ,Pune Account No. : 7469753553\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                    Para5453.Add(new Phrase("IFSC Code :IDIB000P087\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                    Para5453.Add(new Phrase("\n", FontFactory.GetFont("Arial", 5, Font.NORMAL)));


                    Para5453.Add(new Phrase("Terms & Conditions  \n", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    Para5453.Add(new Phrase("\n", FontFactory.GetFont("Arial", 5, Font.NORMAL)));


                    Para5453.Add(new Phrase("***Goods once sold will not be taken back or exchanged.\n", FontFactory.GetFont("Arial", 9, Font.BOLD)));

                    Para5453.Add(new Phrase("***We are not responsible for any shortage of goods in transit\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));

                    Para5453.Add(new Phrase("***Bills not paid before due date will attract 24% interest.\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));

                    Para5453.Add(new Phrase("All disputes subject to PUNE Jurisdiction only.Cheque Bounce Charges Rs.350\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));


                    PdfPCell Cellsa13ss121 = new PdfPCell(Para5453);
                    Cellsa13ss121.HorizontalAlignment = 0;
                    Cellsa13ss121.BackgroundColor = BaseColor.WHITE;
                    Cellsa13ss121.Colspan = 6;
                    table3.AddCell(Cellsa13ss121);





                    Paragraph Para545dsd3 = new Paragraph();
                    Para545dsd3.Add(new Phrase("FOR Siddhivinayak Distributor\n\n\n\n\n\n\n\n\n", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    Para545dsd3.Add(new Phrase("Authorised Signatory", FontFactory.GetFont("Arial", 9, Font.BOLD)));

                    PdfPCell Cellsa13sss121 = new PdfPCell(Para545dsd3);
                    Cellsa13sss121.HorizontalAlignment = 1;
                    Cellsa13sss121.BackgroundColor = BaseColor.WHITE;
                    Cellsa13sss121.Colspan = 2;
                    table3.AddCell(Cellsa13sss121);


                    PdfPCell Cellsa131200 = new PdfPCell(new Phrase(new Phrase("\n\n\n Grand Total : " + string.Format("{0:0.00 }", Math.Round(TotalAmount)), FontFactory.GetFont("Arial", 10, Font.BOLD, BaseColor.BLACK))));
                    Cellsa131200.HorizontalAlignment = 1;
                    Cellsa131200.BackgroundColor = BaseColor.WHITE;
                    Cellsa131200.Colspan = 2;

                    table3.AddCell(Cellsa131200);


                }
                catch { }



                document.Add(table1);
                document.Add(table2);
                document.Add(table3);

                document.Close();
                var result = new { Message = "success", FileName = filename1 };
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception EX)
            {
                var result = new { Message = EX.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }
        public string words(int numbers)
        {
            int number = numbers;

            if (number == 0) return "Zero";
            if (number == -2147483648) return "Minus Two Hundred and Fourteen Crore Seventy Four Lakh Eighty Three Thousand Six Hundred and Forty Eight";
            int[] num = new int[4];
            int first = 0;
            int u, h, t;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (number < 0)
            {
                sb.Append("Minus ");
                number = -number;
            }
            string[] words0 = {"" ,"One ", "Two ", "Three ", "Four ",
         "Five " ,"Six ", "Seven ", "Eight ", "Nine "};
            string[] words1 = {"Ten ", "Eleven ", "Twelve ", "Thirteen ", "Fourteen ",
        "Fifteen ","Sixteen ","Seventeen ","Eighteen ", "Nineteen "};
            string[] words2 = {"Twenty ", "Thirty ", "Forty ", "Fifty ", "Sixty ",
         "Seventy ","Eighty ", "Ninety "};
            string[] words3 = { "Thousand ", "Lakh ", "Crore " };
            num[0] = number % 1000; // units
            num[1] = number / 1000;
            num[2] = number / 100000;
            num[1] = num[1] - 100 * num[2]; // thousands
            num[3] = number / 10000000; // crores
            num[2] = num[2] - 100 * num[3]; // lakhs
            for (int i = 3; i > 0; i--)
            {
                if (num[i] != 0)
                {
                    first = i;
                    break;
                }
            }
            for (int i = first; i >= 0; i--)
            {
                if (num[i] == 0) continue;
                u = num[i] % 10; // ones
                t = num[i] / 10;
                h = num[i] / 100; // hundreds
                t = t - 10 * h; // tens
                if (h > 0) sb.Append(words0[h] + "Hundred ");
                if (u > 0 || t > 0)
                {
                    if (h > 0 || i == 0) sb.Append("and ");
                    if (t == 0)
                        sb.Append(words0[u]);
                    else if (t == 1)
                        sb.Append(words1[u]);
                    else
                        sb.Append(words2[t - 2] + words0[u]);
                }
                if (i != 0) sb.Append(words3[i - 1]);
            }
            return sb.ToString().TrimEnd();
        }
        public FileResult GetReport()
        {
            var filename1 = Session["filename"].ToString();
            string FileName = "";
            try
            {
                FileName = Session["fileName1"].ToString();
                byte[] FileBytes = System.IO.File.ReadAllBytes(FileName);
                return File(FileName, "application/pdf");
            }
            catch
            {
                byte[] FileBytes = System.IO.File.ReadAllBytes(FileName);
                return File(FileBytes, "application/pdf");
            }
        }
    }
}