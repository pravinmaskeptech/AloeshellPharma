using Inventory.Models;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using System.ServiceModel.Channels;
using Syncfusion.XlsIO;
using Syncfusion.JavaScript.Models;
using System.Web.Script.Serialization;
using Syncfusion.EJ.Export;
using System;
using System.Collections;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;



namespace Inventory.Controllers
{
    public class SOReturnsController : Controller
    {
        InventoryModel db = new InventoryModel();
        // GET: SOReturns
        public ActionResult Create()
        {
            ViewBag.Warehousedatasource = db.Warehouses.ToList();
            ViewBag.PONOdatasource = db.Sales.Where(a => a.DeliveredQty - a.ReturnQty > 0 && a.SerialNoApplicable == false).Select(x => x.InvoiceNo).Distinct().ToList();
            return View();
        }
        public ActionResult Return()
        {
            var setting = db.Settings.Where(x => x.FieldName == "BatchNo" && x.Setting == true).FirstOrDefault();
            if (setting != null)
                ViewBag.BatchNoSetting = setting.FieldName;
            else
                ViewBag.BatchNoSetting = "";

            ViewBag.Productdatasource = db.Products.Where(a => a.IsActive == true && a.SerialNoApplicable == true).ToList();
            return View();
        }

        public ActionResult CreditReturns()
        {
            
            var results1 = from p in db.Sales
                           where p.ReturnQty > 0 /*&& p.CreditDocNo == null*/

                           // join cst in db.Customers on p.CustomerID equals cst.CustomerID into Customers
                           //    group p by p.InvoiceNo into g

                           join Custss in db.orderMain on p.OrderNo equals Custss.OrderNo into customerss
                           from aa in customerss.DefaultIfEmpty()

                           select new { CustomerName = aa.CustomerName, InvoiceNo = p.InvoiceNo, InvoiceDate = p.InvoiceDate, ReturnItems = p.ReturnQty, Amount = (p.AmountPerUnit*p.ReturnQty), InvoiceID = p.SalesId };


            var results = from p in results1
                          group p by p.InvoiceNo into g
                          select new { CustomerName = g.Select(x => x.CustomerName).FirstOrDefault(), InvoiceNo = g.Select(x => x.InvoiceNo).FirstOrDefault(), InvoiceDate = g.Select(x => x.InvoiceDate).FirstOrDefault(), ReturnItems = g.Sum(x => x.ReturnItems), Amount = g.Sum(x => x.Amount), InvoiceID = g.Max(a => a.InvoiceID) };
            ViewBag.datasource = results;

            return View();
        }

        public ActionResult CreateCreditNote(int Id)
        {
            try
            {
                var invno = db.Sales.Where(a => a.SalesId == Id).Select(a => a.InvoiceNo).SingleOrDefault();
                ViewBag.InvNo = invno;
                var Cust = from p in db.Sales
                           where p.InvoiceNo == invno && p.ReturnQty > 0 && p.CreditDocNo == null
                           join c in db.orderMain on p.OrderNo equals c.OrderNo into cust
                           from cs in cust.DefaultIfEmpty()
                           select new { CustomerNm = cs.CustomerName, TotalAmount = p.TotalAmount };
                var cnm = Cust.FirstOrDefault();

                //ViewBag.Custnam = cnm.CustomerNm.ToString();
                ViewBag.Custnam = cnm?.CustomerNm?.ToString() ?? "";
                ViewBag.Total = cnm?.TotalAmount != 0 ? cnm?.TotalAmount.ToString() : "";

            }
            catch (Exception ex) {; }
            return View();
        }

        public ActionResult GetCreditNoteData(string InvNo)
        {
            var results = from p in db.Sales
                          where p.InvoiceNo == InvNo && p.ReturnQty > 0 && p.ReturnQty > 0 && p.CreditDocNo == null
                          join Product in db.Products on p.ProductCode equals Product.ProductCode into products
                          from prd in products.DefaultIfEmpty()   //prd.ProductName
                          select new { ProductCode = p.ProductCode, ProductName = prd.ProductName, ReturnQty = p.ReturnQty, Discount = (p.Discount * p.ReturnQty), TaxPer = p.GSTPercentage, TaxAmount = ((p.CGSTAmount + p.SGSTAmount + p.IGSTAmount) * p.ReturnQty), TotalAmount = (p.AmountPerUnit * p.ReturnQty) };

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveCreditNote(string DocNo, string Customer, string InvoiceNo, decimal Amount)
        {
            int custID = Convert.ToInt32(db.Sales.Where(x => x.InvoiceNo == InvoiceNo).Select(x => x.CustomerID).FirstOrDefault());
            CreditNote Cn = new CreditNote();
            Cn.DocDate = DateTime.Today;
            Cn.DocNo = DocNo;
            Cn.CompanyID = 1;
            Cn.CustomerID = custID;
            Cn.CustomerName = Customer;
            Cn.InvoiceNo = InvoiceNo;
            Cn.Amount = Amount;
            Cn.DocNo = DocNo;
            db.CreditNote.Add(Cn);

            var sl = db.Sales.Where(x => x.InvoiceNo == InvoiceNo && x.ReturnQty > 0 && x.CreditDocNo == null).ToList();
            sl.ForEach(x => x.CreditDocNo = DocNo);


            var billNo = db.BillNumbering.Where(a => a.Type == "CRNote").FirstOrDefault();
            billNo.Number = Convert.ToInt32(billNo.Number) + 1;


            db.SaveChanges();

            var results = true;
            return Json(results, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Index()
        {
            try
            {
                var setting = db.Settings.Where(x => x.FieldName == "BatchNo" && x.Setting == true).FirstOrDefault();
                if (setting != null)
                    ViewBag.BatchNoSetting = setting.FieldName;
                else
                    ViewBag.BatchNoSetting = "";

                var SOReturnsMaster = db.sOReturns.ToList();

                ViewBag.datasource = (from c in SOReturnsMaster
                 join Cust in db.Sales on c.InvoiceNo equals Cust.InvoiceNo into customers
                 from aaa in customers.DefaultIfEmpty()

                 join Custss in db.orderMain on aaa.OrderNo equals Custss.OrderNo into customerss
                 from aa in customerss.DefaultIfEmpty()

                 group c by new
                 {
                     c.SOReturnNo,
                     c.InvoiceNo,
                     c.InvoiceDate,
                     c.Status,
                     aa.CustomerName
                 } into gcs
                 select new
                 {
                     InvoiceNo = gcs.Key.InvoiceNo,
                     InvoiceDate = gcs.Key.InvoiceDate,
                     CustomerName = gcs.Key.CustomerName,
                     SOReturnNo = gcs.Key.SOReturnNo,
                     Status = gcs.Key.Status,
                     ReturnQty = gcs.Sum(a => a.ReturnQty),
                 }).ToList();

                return View();



            }
            catch (Exception qq)
            {
                ;
            }

            return View();

        }

        public JsonResult ShowSOdetails(string InvoiceNo)
        {
            if (InvoiceNo == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                //   int poid = db.POMains.Where(a => a.PurchaseOrderNo == PONO).Select(a => a.PurchaseOrderID).Single();

                //  var result = db.poDetails.Where(a => a.PurchaseOrderID == poid &&  a.ReceivedQty>0).ToList();
                //var result = db.GRNDetail.Where(a => a.PONo == PONO).ToList();

                //return Json(result, JsonRequestBehavior.AllowGet);

                var Productsmaster = new List<Products>(db.Products);
                var Storesmaster = new List<StoreLocations>(db.StoreLocations);
                var Warehousemaster = new List<Warehouse>(db.Warehouses);
                var Salesmaster = new List<Sales>(db.Sales).Where(a => a.InvoiceNo == InvoiceNo).ToList();
                var OrderMainmaster = new List<OrderMain>(db.orderMain).Where(a => a.OrderNo == Salesmaster.Select(aa => aa.OrderNo).FirstOrDefault()).FirstOrDefault();
                var OrderDetailsmasters = new List<OrderDetails>(db.orderDetails);
                var Customersmasters = new List<Customer>(db.Customers);

                var result = (from sales in Salesmaster.Where(a => a.InvoiceNo == InvoiceNo && a.DeliveredQty != a.ReturnQty && a.SerialNoApplicable == false)

                              join podtl in OrderDetailsmasters on sales.OrderNo equals podtl.OrderNo into OrderDetailss
                              from orderDtl in OrderDetailss.Where(a => a.ProductCode == sales.ProductCode && a.OrderNo == sales.OrderNo).DefaultIfEmpty()

                                  //join Customer in Customersmasters on sales.CustomerID equals Customer.CustomerID into Customers
                                  //from Cust in Customers.DefaultIfEmpty()

                              join Product in Productsmaster on sales.ProductCode equals Product.ProductCode into products
                              from prd in products.DefaultIfEmpty()

                              join Warehouse in Warehousemaster on sales.WarehouseID equals Warehouse.WareHouseID into warehouse
                              from Whouse in warehouse.DefaultIfEmpty()

                              join StoreLocation in Storesmaster on sales.StoreLocationId equals StoreLocation.StoreLocationId into storeLoc
                              from store in storeLoc.DefaultIfEmpty()

                              orderby sales.SalesId descending
                              select new { CustomerID = sales.CustomerID, SalesId = sales.SalesId, InvoiceNo = sales.InvoiceNo, InvoiceDate = sales.InvoiceDate, BatchNo = sales.BatchNo, DeliveredQty = sales.DeliveredQty, ProductCode = prd.ProductCode, ProductName = prd == null ? string.Empty : prd.ProductName, WarehouseID = sales.WarehouseID, WareHouseName = Whouse == null ? string.Empty : Whouse.WareHouseName, StoreLocation = store == null ? string.Empty : store.StoreLocation, StoreLocationId = sales.StoreLocationId, ReturnQty = sales.ReturnQty, CustomerName = OrderMainmaster.CustomerName, OrderDetailsID = orderDtl.OrderDetailsID, OrderQty = orderDtl.OrderQty, OrderNo = sales.OrderNo }
                                     ).Distinct().ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }
        public JsonResult ShowGRNDetails(string ProductCode, string BatchNo)
        {
            if (ProductCode == "" || BatchNo == "" || BatchNo == "NaN")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var Productsmaster = new List<Products>(db.Products);
                var Storesmaster = new List<StoreLocations>(db.StoreLocations);
                var Warehousemaster = new List<Warehouse>(db.Warehouses);
                var GRNDetailsMaster = new List<GRNDetails>(db.GRNDetail);
                var result = (from grn in GRNDetailsMaster.Where(a => a.ProductCode == ProductCode && a.SalesQty != 0)

                              join Product in Productsmaster on grn.ProductCode equals Product.ProductCode into products
                              from prd in products.DefaultIfEmpty()

                              join Warehouse in Warehousemaster on grn.WarehouseID equals Warehouse.WareHouseID into warehouse
                              from Whouse in warehouse.DefaultIfEmpty()

                              join StoreLocation in Storesmaster on grn.StoreLocationId equals StoreLocation.StoreLocationId into storeLoc
                              from store in storeLoc.DefaultIfEmpty()

                              orderby grn.GRNId descending
                              select new { GRNId = grn.GRNId, SalesQty = grn.SalesQty, temp = 1, WarehouseID = grn.WarehouseID, StoreLocationId = grn.StoreLocationId, BatchNo = grn.BatchNo, ReceivedQty = grn.ReceivedQty, WareHouseName = Whouse == null ? string.Empty : Whouse.WareHouseName, StoreLocation = store == null ? string.Empty : store.StoreLocation, PurchaseOrderDetailsID = grn.PurchaseOrderDetailsID }
                                     ).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult save(List<SOReturns> Returns)
        {
            var status = false;
            int cnt = 1;
            try
            {
                if (Returns != null)
                {
                    if (Returns.Count > 0)
                    {
                        foreach (var x in Returns)
                        {
                            var prd = db.Products.Where(a => a.ProductCode == x.ProductCode).FirstOrDefault();
                            prd.ClosingQuantity = (Convert.ToDecimal(prd.ClosingQuantity) + Convert.ToDecimal(x.ReturnQty));
                            prd.OutwardQuantity = (Convert.ToDecimal(prd.OutwardQuantity) - Convert.ToDecimal(x.ReturnQty));

                            var sale = db.Sales.Where(a => a.ProductCode == x.ProductCode && a.InvoiceNo == x.InvoiceNo && a.SalesId == x.SalesId).FirstOrDefault();
                            //     sale.ReturnQty = Convert.ToDecimal(sale.ReturnQty) + Convert.ToDecimal(x.ReturnQty);
                            sale.DeliveredQty = Convert.ToDecimal(sale.DeliveredQty) - Convert.ToDecimal(x.ReturnQty);
                            // sale.ReturnReason = x.ReturnReason;
                            if (x.Status == "Replace")
                            {
                                sale.ReplaceQty = Convert.ToDecimal(sale.ReplaceQty) + Convert.ToDecimal(x.ReturnQty);
                                sale.ReplaceReason = x.ReturnReason;
                            }
                            else
                            {
                                sale.ReturnQty = Convert.ToDecimal(sale.ReturnQty) + Convert.ToDecimal(x.ReturnQty);
                                sale.ReturnReason = x.ReturnReason;
                            }

                            //var order = db.orderDetails.Where(a => a.ProductCode == x.ProductCode && a.OrderNo == x.OrderNo).FirstOrDefault();
                            //order.ReturnQty = Convert.ToDecimal(order.ReturnQty) + Convert.ToDecimal(x.ReturnQty);
                            //order.DeliveredQty = Convert.ToDecimal(sale.DeliveredQty) - Convert.ToDecimal(x.ReturnQty);
                            //order.ReturnReason = x.ReturnReason;

                            List<TempSOReturn> tmp = db.tempSOReturn.Where(a => a.ProductCode == x.ProductCode && a.InvoiceNo == x.InvoiceNo).ToList();
                            foreach (var t in tmp)
                            {
                                var grn = db.GRNDetail.Where(a => a.GRNId == t.GrnId && a.ProductCode == t.ProductCode).FirstOrDefault();
                                grn.SalesQty = grn.SalesQty - t.ReturnQty;
                                //grn.ReceivedQty = grn.ReceivedQty + t.ReturnQty;
                            }
                            SOReturns poRtn = new SOReturns();
                            poRtn.ProductCode = x.ProductCode;
                            poRtn.CompanyID = Convert.ToInt32(Session["CompanyID"]);
                            poRtn.ReturnQty = x.ReturnQty;
                            poRtn.WarehouseID = tmp[0].WarehouseId;
                            poRtn.StoreLocationId = tmp[0].StoreLocationId;
                            poRtn.BatchNo = x.BatchNo;
                            poRtn.InvoiceNo = x.InvoiceNo;
                            poRtn.ReturnDate = x.ReturnDate;
                            poRtn.SOReturnNo = x.SOReturnNo;
                            poRtn.InvoiceDate = x.InvoiceDate;
                            poRtn.CustomerId = x.CustomerId;
                            poRtn.ReturnReason = x.ReturnReason;
                            poRtn.CreatedBy = User.Identity.Name;
                            poRtn.CreatedDate = DateTime.Today;
                            poRtn.Status = x.Returnstatus;
                            db.sOReturns.Add(poRtn);
                            foreach (var vp in tmp)
                                db.tempSOReturn.Remove(vp);
                            db.SaveChanges();
                            status = true;
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
        public JsonResult GetAllSODetails(string InvoiceNo, string SONO)
        {
            if (InvoiceNo == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var Productsmaster = new List<Products>(db.Products);
                var Storesmaster = new List<StoreLocations>(db.StoreLocations);
                var Warehousemaster = new List<Warehouse>(db.Warehouses);
                var SOReturnsMaster = new List<SOReturns>(db.sOReturns);
                var result = (from returns in SOReturnsMaster.Where(a => a.InvoiceNo == InvoiceNo && a.SOReturnNo == SONO)

                              join Product in Productsmaster on returns.ProductCode equals Product.ProductCode into products
                              from prd in products.DefaultIfEmpty()

                              join Warehouse in Warehousemaster on returns.WarehouseID equals Warehouse.WareHouseID into warehouse
                              from Whouse in warehouse.DefaultIfEmpty()

                              join StoreLocation in Storesmaster on returns.StoreLocationId equals StoreLocation.StoreLocationId into storeLoc
                              from store in storeLoc.DefaultIfEmpty()

                              orderby returns.SOreturnId descending
                              select new { SOreturnId = returns.SOreturnId, SerialNumber = returns.SerialNumber, InvoiceNo = returns.InvoiceNo, ReturnDate = returns.ReturnDate, BatchNo = returns.BatchNo, ReturnQty = returns.ReturnQty, ReturnReason = returns.ReturnReason, ProductCode = prd == null ? string.Empty : prd.ProductName, Warehouse = Whouse == null ? string.Empty : Whouse.WareHouseName, StoreLocation = store == null ? string.Empty : store.StoreLocation }
                                    ).ToList();

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult getReturnData(string SerialNo, string ProductId)
        {
            if (SerialNo == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                //     select 
                var Productsmaster = new List<Products>(db.Products);
                var Storesmaster = new List<StoreLocations>(db.StoreLocations);
                var Warehousemaster = new List<Warehouse>(db.Warehouses);
                var Suppliersmasters = new List<Suppliers>(db.suppliers);
                var ProductSerialNoMaster = new List<ProductSerialNo>(db.ProductSerialNo);

                var result = (from serialno in ProductSerialNoMaster.Where(a => a.SerialNo == SerialNo && a.ProductCode == ProductId && (a.Status == "Sold" || a.Status == "Sale"))

                              join Product in Productsmaster on serialno.ProductCode equals Product.ProductCode into products
                              from prd in products.DefaultIfEmpty()

                              join Warehouse in Warehousemaster on serialno.WarehouseId equals Warehouse.WareHouseID into warehouse
                              from Whouse in warehouse.DefaultIfEmpty()

                              join StoreLocation in Storesmaster on serialno.StoreLocationId equals StoreLocation.StoreLocationId into storeLoc
                              from store in storeLoc.DefaultIfEmpty()

                              orderby serialno.SerialNoId descending
                              select new { InvoiceNo = serialno.InvoiceNo, SerialNoId = serialno.SerialNoId, PODetailsId = serialno.PODetailsId, ProductCode = serialno.ProductCode, BatchNo = serialno.BatchNo, WarehouseId = serialno.WarehouseId, StoreLocationId = serialno.StoreLocationId, GrnNo = serialno.GrnNo, ProductName = prd == null ? string.Empty : prd.ProductName, WareHouseName = Whouse == null ? string.Empty : Whouse.WareHouseName, StoreLocation = store == null ? string.Empty : store.StoreLocation, PONO = serialno.PONO, SerialNo = serialno.SerialNo, Status = serialno.Status, GrnDate = serialno.GrnDate }
                                     ).Distinct().ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }
        [HttpPost]
        public JsonResult SaveReturnSerialNo(List<SOReturns> ReturnsData)
        {
            var status = false;
            int cnt = 1;
            try
            {

                if (ReturnsData != null)
                {
                    if (ReturnsData.Count > 0)
                    {
                        foreach (var x in ReturnsData)
                        {
                            var prd = db.Products.Where(a => a.ProductCode == x.ProductCode).FirstOrDefault();
                            prd.ClosingQuantity = (Convert.ToDecimal(prd.ClosingQuantity) + Convert.ToDecimal(x.ReturnQty));
                            prd.OutwardQuantity = (Convert.ToDecimal(prd.OutwardQuantity) - Convert.ToDecimal(x.ReturnQty));

                            var serialno = db.ProductSerialNo.Where(a => a.SerialNoId == x.SerialNoId && a.ProductCode == x.ProductCode && a.InvoiceNo == x.InvoiceNo).FirstOrDefault();
                            //  var orderNo = db.Sales.Where(a => a.InvoiceNo == x.InvoiceNo && a.ProductCode == x.ProductCode && a.BatchNo==x.BatchNo ).Select(a => a.OrderNo).Single();
                            var sale = db.Sales.Where(a => a.InvoiceNo == x.InvoiceNo && a.ProductCode == x.ProductCode).FirstOrDefault();
                            sale.DeliveredQty = Convert.ToDecimal(sale.DeliveredQty) - Convert.ToDecimal(x.ReturnQty);
                            if (x.Status == "Replace")
                            {
                                sale.ReplaceQty = Convert.ToDecimal(sale.ReplaceQty) + Convert.ToDecimal(x.ReturnQty);
                                sale.ReplaceReason = x.ReturnReason;
                                serialno.Status = "SO Replace";
                            }
                            else
                            {
                                sale.ReturnQty = Convert.ToDecimal(sale.ReturnQty) + Convert.ToDecimal(x.ReturnQty);
                                sale.ReturnReason = x.ReturnReason;
                                serialno.Status = "SO Return";
                            }

                            //var order = db.orderDetails.Where(a => a.OrderNo == sale.OrderNo && a.ProductCode==x.ProductCode).FirstOrDefault();
                            //order.ReturnQty = Convert.ToDecimal(order.ReturnQty) + Convert.ToDecimal(x.ReturnQty);
                            //order.ReturnReason = x.ReturnReason;
                            //order.DeliveredQty = Convert.ToDecimal(order.DeliveredQty) - Convert.ToDecimal(x.ReturnQty);                           


                            var grn = db.GRNDetail.Where(a => a.PONo == serialno.PONO && a.ProductCode == x.ProductCode && a.BatchNo == x.BatchNo).FirstOrDefault();
                            grn.SalesQty = Convert.ToDecimal(grn.SalesQty) - Convert.ToDecimal(grn.ReturnQty);
                            //grn.ReceivedQty = Convert.ToDecimal(grn.ReceivedQty) + Convert.ToDecimal(grn.ReturnQty);

                            SOReturns poRtn = new SOReturns();
                            poRtn.ReturnDate = x.ReturnDate;
                            poRtn.InvoiceNo = x.InvoiceNo;
                            poRtn.ProductCode = x.ProductCode;
                            poRtn.ReturnQty = 1;
                            poRtn.CustomerId = sale.CustomerID;
                            poRtn.InvoiceDate = sale.InvoiceDate;
                            poRtn.CompanyID = Convert.ToInt32(Session["CompanyID"]);
                            poRtn.BatchNo = x.BatchNo;
                            poRtn.SOReturnNo = x.SOReturnNo;
                            poRtn.WarehouseID = x.WarehouseID;
                            poRtn.StoreLocationId = x.StoreLocationId;
                            poRtn.InvoiceDate = x.InvoiceDate;
                            poRtn.ReturnReason = x.ReturnReason;
                            poRtn.CreatedBy = User.Identity.Name;
                            poRtn.CreatedDate = DateTime.Today;
                            poRtn.SerialNumber = x.SerialNumber;
                            poRtn.Status = x.Status;
                            poRtn.ReturnReason = x.ReturnReason;


                            db.sOReturns.Add(poRtn);
                            db.SaveChanges();
                            status = true;
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

        public JsonResult SaveTempSOReturn(List<SOReturns> GrnData)
        {
            var status = false;
            try
            {
                if (GrnData.Count > 0)
                {
                    foreach (var x in GrnData)
                    {
                        var tmpcnt = db.tempSOReturn.Where(a => a.ProductCode == x.ProductCode && a.GrnId == x.GrnId).Count();
                        if (tmpcnt == 0)
                        {

                            TempSOReturn tmp = new TempSOReturn();
                            tmp.GrnId = x.GrnId;
                            tmp.OrderDetailsID = x.OrderDetailsID;
                            tmp.WarehouseId = x.WarehouseID;
                            tmp.ProductCode = x.ProductCode;
                            tmp.StoreLocationId = x.StoreLocationId;
                            tmp.BatchNo = x.BatchNo;
                            tmp.ReceivedQty = x.ReceivedQty;
                            tmp.ReturnQty = x.ReturnQty;
                            tmp.InvoiceNo = x.InvoiceNo;
                            db.tempSOReturn.Add(tmp);
                            db.SaveChanges();
                            status = true;
                        }
                        else
                        {
                            var grn = db.tempSOReturn.Where(a => a.ProductCode == x.ProductCode && a.GrnId == x.GrnId).FirstOrDefault();
                            grn.GrnId = x.GrnId;
                            grn.OrderDetailsID = x.OrderDetailsID;
                            grn.WarehouseId = x.WarehouseID;
                            grn.ProductCode = x.ProductCode;
                            grn.StoreLocationId = x.StoreLocationId;
                            grn.ReceivedQty = x.ReceivedQty;
                            grn.BatchNo = x.BatchNo;
                            grn.ReturnQty = x.ReturnQty;
                            grn.InvoiceNo = x.InvoiceNo;
                            db.SaveChanges();
                            status = true;

                        }
                    }
                    return new JsonResult { Data = new { status } };
                }
            }

            catch (Exception ee)
            {
                return new JsonResult { Data = new { status = false } };
            }
            return new JsonResult { Data = new { status } };
        }
        public JsonResult getSORturnNo()
        {
            var result = db.sOReturns.Count();
            result = result + 1;
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult getCreditNoteNo()
        {

            var count = db.BillNumbering.Where(a => a.Type == "CRNote").Select(a => a.Number).SingleOrDefault();
            count = count + 1;
            return Json(count, JsonRequestBehavior.AllowGet);
        } 

        public JsonResult PrintCreditNote(string InvoiceNo)
        {
            try
            {

                var srdetails = db.sOReturns.Where(a => a.SOReturnNo == InvoiceNo).FirstOrDefault();
                if (srdetails == null)
                {
                    InvoiceNo = InvoiceNo;
                }
                else
                {
                    InvoiceNo = srdetails.InvoiceNo;
                    if (srdetails.Status == "Replace")
                    {
                        var results = new { Message = "Credit Note Not Genrate For Status Replace" };
                        return Json(results, JsonRequestBehavior.AllowGet);
                    }
                }

                int Count = 1;
                var xxx = db.Sales.Where(a => a.InvoiceNo == InvoiceNo).FirstOrDefault(); ;
                //  var trarmandcondition = xxx.TermsAndConditions;
                var ordermain = db.orderMain.Where(a => a.OrderNo == xxx.OrderNo).FirstOrDefault();
                var OrderNo = xxx.OrderNo;
                var Productsmaster = new List<Products>(db.Products);
                var Storesmaster = new List<StoreLocations>(db.StoreLocations);
                var Warehousemaster = new List<Warehouse>(db.Warehouses);
                var SOReturnsMaster = new List<SOReturns>(db.sOReturns);
                var result11 = (from returns in SOReturnsMaster.Where(a => a.InvoiceNo == InvoiceNo )

                                join Product in Productsmaster on returns.ProductCode equals Product.ProductCode into products
                                from prd in products.DefaultIfEmpty()

                                join Warehouse in Warehousemaster on returns.WarehouseID equals Warehouse.WareHouseID into warehouse
                                from Whouse in warehouse.DefaultIfEmpty()

                                join StoreLocation in Storesmaster on returns.StoreLocationId equals StoreLocation.StoreLocationId into storeLoc
                                from store in storeLoc.DefaultIfEmpty()

                                orderby returns.SOreturnId descending
                                //     select new { SOreturnId = returns.SOreturnId, SerialNumber = returns.SerialNumber, InvoiceNo = returns.InvoiceNo, ReturnDate = returns.ReturnDate, BatchNo = returns.BatchNo, ReturnQty = returns.ReturnQty, ReturnReason = returns.ReturnReason, ProductCode = prd == null ? string.Empty : prd.ProductName, Warehouse = Whouse == null ? string.Empty : Whouse.WareHouseName, StoreLocation = store == null ? string.Empty : store.StoreLocation }
                                select new { InvoiceNo = returns.InvoiceNo, ReturnDate = returns.ReturnDate, ReturnQty = returns.ReturnQty, ProductName = prd == null ? string.Empty : prd.ProductName, ProductCode = prd == null ? string.Empty : prd.ProductCode }
                                    ).ToList();

                var result12 =
    (from c in result11
     group c by new
     {
         c.ProductCode,
         c.ReturnDate,
         c.ProductName,
         c.InvoiceNo

     } into gcs
     select new
     {
         InvoiceNo = gcs.Key.InvoiceNo,
         ProductCode = gcs.Key.ProductCode,
         ProductName = gcs.Key.ProductName,
         ReturnDate = gcs.Key.ReturnDate,
         ReturnQty = gcs.Sum(a => a.ReturnQty)
     }).ToList(); ;




                iTextSharp.text.Font font5 = iTextSharp.text.FontFactory.GetFont(FontFactory.HELVETICA, 9);
                iTextSharp.text.Font font10 = iTextSharp.text.FontFactory.GetFont(FontFactory.HELVETICA, 11);

                DateTime dt = DateTime.Now;
                var aa = dt.ToString("HH");
                var bb = dt.ToString("mm");
                var cc = dt.ToString("ss");

                Document document = new Document(PageSize.A4, 5f, 5f, 5f, 5);
                string path = Server.MapPath("~/Reports/OrderMain/");
                string[] parts = OrderNo.Split('/');
                string numberString = parts[2];
                string filename1 = path + "" + numberString + "_" + aa + bb + cc + ".pdf";
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filename1, FileMode.Create));
                document.Open();
                Session["fileName1"] = filename1;

                PdfPTable table = new PdfPTable(10);
                float[] widths = new float[] { 0.0f, 1.2f, 4f, 2f, 2f, 2f, 2f, 2f, 2f, 0.0f };
                table.SetWidths(widths);
                table.WidthPercentage = 98;

                PdfPCell p4787871 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                p4787871.HorizontalAlignment = 1;
                p4787871.BackgroundColor = BaseColor.BLACK;
                table.AddCell(p4787871);


                PdfPCell p1 = new PdfPCell(new Phrase(new Phrase("Sr. No.", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                p1.HorizontalAlignment = 1;
                p1.BackgroundColor = BaseColor.BLACK;
                table.AddCell(p1);

                PdfPCell p2 = new PdfPCell(new Phrase(new Phrase("Product Name", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                p2.HorizontalAlignment = 1;
                p2.BackgroundColor = BaseColor.BLACK;
                table.AddCell(p2);



                PdfPCell pp4 = new PdfPCell(new Phrase(new Phrase("Return Qty", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                pp4.HorizontalAlignment = 1;
                pp4.BackgroundColor = BaseColor.BLACK;
                table.AddCell(pp4);


                PdfPCell pp45445 = new PdfPCell(new Phrase(new Phrase("Rate", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                pp45445.HorizontalAlignment = 1;
                pp45445.BackgroundColor = BaseColor.BLACK;
                table.AddCell(pp45445);

                PdfPCell pp45445745 = new PdfPCell(new Phrase(new Phrase("Net Amt", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                pp45445745.HorizontalAlignment = 1;
                pp45445745.BackgroundColor = BaseColor.BLACK;
                table.AddCell(pp45445745);

                PdfPCell pp4544545 = new PdfPCell(new Phrase(new Phrase("Disc", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                pp4544545.HorizontalAlignment = 1;
                pp4544545.BackgroundColor = BaseColor.BLACK;
                table.AddCell(pp4544545);

                PdfPCell pp454478545 = new PdfPCell(new Phrase(new Phrase("GST", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                pp454478545.HorizontalAlignment = 1;
                pp454478545.BackgroundColor = BaseColor.BLACK;
                table.AddCell(pp454478545);

                PdfPCell pp44545 = new PdfPCell(new Phrase(new Phrase("Amount", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                pp44545.HorizontalAlignment = 2;
                pp44545.BackgroundColor = BaseColor.BLACK;
                table.AddCell(pp44545);


                PdfPCell p7d21 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                p7d21.HorizontalAlignment = 0;
                table.AddCell(p7d21);

                PdfPTable table1 = new PdfPTable(10);
                float[] width1 = new float[] { 0.0f, 1.2f, 4f, 2f, 2f, 2f, 2f, 2f, 2f, 0.0f };
                table1.SetWidths(width1);
                table1.WidthPercentage = 98;
                table1.DefaultCell.Padding = 0;

                int cnt = 1;
                decimal totAmt = 0;
                foreach (var r in result12)
                {
                    var SalesReport1 = db.Sales.Where(a => a.InvoiceNo == InvoiceNo && a.ProductCode == r.ProductCode).FirstOrDefault();
                    var Ordeerdata = db.orderDetails.Where(a => a.OrderNo == SalesReport1.OrderNo && a.ProductCode == r.ProductCode).FirstOrDefault();
                    var prd = db.Products.Where(a => a.ProductCode == r.ProductCode).FirstOrDefault();
                    var qty = r.ReturnQty;
                    var amt = Ordeerdata.Price;
                    var disc = Ordeerdata.Discount;
                    var gstPer = Ordeerdata.GSTPercentage;

                    var Netamot = Convert.ToDecimal(qty) * Convert.ToDecimal(amt);
                    var nettot = Netamot;
                    decimal discamt = 0;
                    if (disc > 0)
                    {
                        discamt = (Netamot * Convert.ToDecimal(disc)) / 100;
                    }
                    Netamot = Netamot - discamt;
                    var GstAmt = (Netamot * Convert.ToDecimal(gstPer)) / 100;

                    Netamot = Netamot + GstAmt;
                    totAmt = totAmt + Netamot;






                    //var orderdetails=db.orderDetails.Where(a=>a.OrderNo==r.)
                    if (result12.Count > 0)
                    {
                        Paragraph pr931 = new Paragraph();
                        pr931.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell prr931 = new PdfPCell(pr931);
                        prr931.HorizontalAlignment = 1;
                        table1.AddCell(prr931);

                        Paragraph pr31 = new Paragraph();
                        pr31.Add(new Phrase("" + Count + "\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell prr31 = new PdfPCell(pr31);
                        prr31.HorizontalAlignment = 1;
                        prr31.UseAscender = true;
                        table1.AddCell(prr31);

                        Paragraph pr32 = new Paragraph();
                        pr32.Add(new Phrase("" + prd.ProductName + " - " + prd.Size, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell prr32 = new PdfPCell(pr32);
                        prr32.HorizontalAlignment = 0;
                        table1.AddCell(prr32);


                        Paragraph pr34 = new Paragraph();
                        pr34.Add(new Phrase("" + string.Format("{0:0.00 }", r.ReturnQty) + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell prr34 = new PdfPCell(pr34);
                        prr34.HorizontalAlignment = 1;
                        table1.AddCell(prr34);


                        Paragraph pr346 = new Paragraph();
                        pr346.Add(new Phrase("" + string.Format("{0:0.00 }", amt) + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell prr34878 = new PdfPCell(pr346);
                        prr34878.HorizontalAlignment = 1;
                        table1.AddCell(prr34878);

                        Paragraph pr346df = new Paragraph();
                        pr346df.Add(new Phrase("" + string.Format("{0:0.00 }", nettot) + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell prr348478 = new PdfPCell(pr346df);
                        prr348478.HorizontalAlignment = 1;
                        table1.AddCell(prr348478);

                        Paragraph pr345456 = new Paragraph();
                        pr345456.Add(new Phrase("" + string.Format("{0:0.00 }", discamt) + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell prr34866878 = new PdfPCell(pr345456);
                        prr34866878.HorizontalAlignment = 1;
                        table1.AddCell(prr34866878);



                        Paragraph pr34dfd5456 = new Paragraph();
                        pr34dfd5456.Add(new Phrase("" + string.Format("{0:0.00 }", GstAmt) + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell prr348668df78 = new PdfPCell(pr34dfd5456);
                        prr348668df78.HorizontalAlignment = 1;
                        table1.AddCell(prr348668df78);



                        Paragraph pr359894 = new Paragraph();
                        pr359894.Add(new Phrase("" + string.Format("{0:0.00 }", Netamot) + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell prr3sdd4 = new PdfPCell(pr359894);
                        prr3sdd4.HorizontalAlignment = 2;
                        table1.AddCell(prr3sdd4);

                        Paragraph pr36g44 = new Paragraph();
                        pr36g44.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell prr36g44 = new PdfPCell(pr36g44);
                        prr36g44.HorizontalAlignment = 0;
                        prr36g44.Border = Rectangle.RIGHT_BORDER;
                        table1.AddCell(prr36g44);
                        Count++;
                    }
                }
                try
                {

                    var AmtInwords = words(Convert.ToInt32(Math.Round(totAmt)));


                    Paragraph pr931 = new Paragraph();
                    pr931.Add(new Phrase("Amount In Words : " + AmtInwords, FontFactory.GetFont("Arial", 8, Font.BOLD)));
                    PdfPCell prr931 = new PdfPCell(pr931);
                    prr931.HorizontalAlignment = 0;
                    prr931.Colspan = 7;
                    table1.AddCell(prr931);


                    Paragraph pr938781 = new Paragraph();
                    pr938781.Add(new Phrase("TOTAL : ", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell prr954531 = new PdfPCell(pr938781);
                    prr954531.HorizontalAlignment = 1;

                    table1.AddCell(prr954531);


                    Paragraph pr359894 = new Paragraph();
                    pr359894.Add(new Phrase("" + string.Format("{0:0.00 }", Math.Round(totAmt)) + "", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell prr3sdd4 = new PdfPCell(pr359894);
                    prr3sdd4.HorizontalAlignment = 2;
                    table1.AddCell(prr3sdd4);

                    Paragraph pr36g44 = new Paragraph();
                    pr36g44.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                    PdfPCell prr36g44 = new PdfPCell(pr36g44);
                    prr36g44.HorizontalAlignment = 0;
                    prr36g44.Border = Rectangle.RIGHT_BORDER;
                    table1.AddCell(prr36g44);

                }
                catch
                { }

                Paragraph pc1855 = new Paragraph();
                pc1855.Add(new Phrase("", FontFactory.GetFont("Arial", 8, Font.BOLD)));
                PdfPCell pc185 = new PdfPCell(pc1855);
                pc185.HorizontalAlignment = 0;
                pc185.Border = Rectangle.LEFT_BORDER;
                pc185.Colspan = 7;
                table1.AddCell(pc185);



                Paragraph pr285 = new Paragraph();
                pr285.Add(new Phrase(" For Siddhivinayak Distributor \n\n\n\n", FontFactory.GetFont("Arial", 8, Font.BOLD)));
                pr285.Add(new Phrase("                                      Authorised Signatory", FontFactory.GetFont("Arial", 7, Font.BOLD)));
                PdfPCell pc285 = new PdfPCell(pr285);
                pc285.HorizontalAlignment = 1;
                // pc285.Border = Rectangle.LEFT_BORDER;
                pc285.Colspan = 2;
                table1.AddCell(pc285);

                Paragraph pr385 = new Paragraph();
                pr385.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                PdfPCell pc385 = new PdfPCell(pr385);
                pc385.HorizontalAlignment = 1;
                pc385.Border = Rectangle.TOP_BORDER;
                table1.AddCell(pc385);



                //Terms & Condition


                Paragraph pr38525 = new Paragraph();
                //pr38525.Add(new Phrase("Terms And Conditions : \n\n", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                //pr38525.Add(new Phrase("1) Above rates are excluding GST\n", FontFactory.GetFont("Arial", 8, Font.NORMAL)));
                //pr38525.Add(new Phrase("2) Post Dated Cheque (PDC) required at the time of delivery of goods\n", FontFactory.GetFont("Arial", 8, Font.NORMAL)));
                //pr38525.Add(new Phrase("3) Transportation charges - extra as applicable\n", FontFactory.GetFont("Arial", 8, Font.NORMAL)));
                //pr38525.Add(new Phrase("" + trarmandcondition, FontFactory.GetFont("Arial", 8, Font.NORMAL)));
                PdfPCell pc3855478 = new PdfPCell(pr38525);
                pc3855478.HorizontalAlignment = 0;
                //  pc3855478.Border = Rectangle.TOP_BORDER;
                pc3855478.Colspan = 10;
                table1.AddCell(pc3855478);




                PdfPTable table4 = new PdfPTable(2);
                float[] widths5 = new float[] { 8f, 8f };
                table4.SetWidths(widths5);
                table4.WidthPercentage = 98;
                table4.HorizontalAlignment = 1;

                string imageURL = Server.MapPath("/Photo/MicraftLogo.png");
                iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(imageURL);
                jpg.ScaleToFit(70, 100);
                jpg.SpacingBefore = 5f;
                jpg.SpacingAfter = 1f;
                jpg.Alignment = Element.ALIGN_LEFT;

                Paragraph p44 = new Paragraph();
                p44.Add(new Phrase());
                PdfPCell c11144 = new PdfPCell(jpg);
                c11144.Border = Rectangle.NO_BORDER;
                table4.AddCell(c11144);

                Paragraph p4 = new Paragraph();

                p4.Add(new Phrase("Siddhivinayak Distributor", FontFactory.GetFont("Arial", 12, Font.BOLD)));
                p4.Add(new Phrase("\n", FontFactory.GetFont("Arial", 5, Font.BOLD)));
                p4.Add(new Phrase("\nShop no 10, Suyog Navkar Building A, \nNear 7 Loves Chowk, Market Yard Road, Pune 411 037.\nGSTIN: 27ABVPK5495R2Z9\nDL.No: MH-PZ3517351,MH-PZ3517352,", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                PdfPCell c1114 = new PdfPCell(p4);
                c1114.HorizontalAlignment = 0;
                c1114.Border = Rectangle.NO_BORDER;
                table4.AddCell(c1114);



                PdfPTable table5 = new PdfPTable(4);
                float[] width5 = new float[] { 0.5f, 7f, 1f, 5f };
                table5.SetWidths(width5);
                table5.WidthPercentage = 98; ;
                table5.HorizontalAlignment = 1;

                PdfPCell pc2 = new PdfPCell();
                pc2.HorizontalAlignment = 0;
                pc2.Border = Rectangle.NO_BORDER;
                table5.AddCell(pc2);

                Paragraph pr1 = new Paragraph();
                pr1.Add(new Phrase("Customer Name : ", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                pr1.Add(new Phrase(ordermain.CustomerName, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                pr1.Add(new Phrase("\nAddress : ", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                pr1.Add(new Phrase("\n" + ordermain.CustomerAddress, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                PdfPCell pc1 = new PdfPCell(pr1);
                pc1.HorizontalAlignment = 0;
                pc1.FixedHeight = 50f;
                pc1.Border = Rectangle.NO_BORDER;
                table5.AddCell(pc1);


                PdfPCell pc53 = new PdfPCell();
                pc53.HorizontalAlignment = 0;
                pc53.Border = Rectangle.NO_BORDER;
                table5.AddCell(pc53);

                Paragraph pr3 = new Paragraph();
                pr3.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                pr3.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                pr3.Add(new Phrase("\n" + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                PdfPCell pc3 = new PdfPCell(pr3);
                pc3.HorizontalAlignment = 0;
                pc3.Border = Rectangle.NO_BORDER;
                table5.AddCell(pc3);



                PdfPTable table9 = new PdfPTable(1);
                float[] width9 = new float[] { 20f };
                table9.SetWidths(width9);
                table9.WidthPercentage = 98; ;

                Paragraph pr226 = new Paragraph();
                // pr226.Add(new Phrase("Date:" + DateTime.Now.ToString("dd/MM/yyyy") + "\n", FontFactory.GetFont("Arial", 8, Font.BOLD)));

                PdfPCell p79013 = new PdfPCell();
                p79013.HorizontalAlignment = 0;
                table9.AddCell(p79013);


                PdfPTable table3 = new PdfPTable(5);
                float[] widths55 = new float[] { 2f, 8f, 4, 3f, 8f, };
                table3.SetWidths(widths55);
                table3.WidthPercentage = 98;
                table3.HorizontalAlignment = 1;


                Paragraph pr53 = new Paragraph();
                pr53.Add(new Phrase("\n\n   CN No\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                pr53.Add(new Phrase("   Date", FontFactory.GetFont("Arial", 9, Font.NORMAL)));

                PdfPCell c112 = new PdfPCell(pr53);
                c112.Border = Rectangle.TOP_BORDER;
                c112.FixedHeight = 50f;
                c112.HorizontalAlignment = 0;
                table3.AddCell(c112);
                var crdata = db.CreditNote.Where(a => a.InvoiceNo == InvoiceNo).FirstOrDefault();
                if(crdata == null)
                {
                    var results = new { Message = "Make Credit Note First" };
                    return Json(results, JsonRequestBehavior.AllowGet);
                }
                //var dtt = crdata.DocDate.ToString(); ;
                Paragraph pr539 = new Paragraph();
                pr539.Add(new Phrase("\n\n: " + crdata.DocNo + "\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                pr539.Add(new Phrase(": " + crdata.DocDate.ToString("dd/MM/yyyy") + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));

                PdfPCell c1129 = new PdfPCell(pr539);
                c1129.Border = Rectangle.TOP_BORDER;
                c1129.FixedHeight = 50f;
                c1129.HorizontalAlignment = 0;
                table3.AddCell(c1129);

                Paragraph p49 = new Paragraph();
                p49.Add(new Phrase("CREDIT NOTE", FontFactory.GetFont("Arial", 13, Font.BOLD)));
                PdfPCell c1115 = new PdfPCell(p49);
                c1115.HorizontalAlignment = 1;
                c1115.Border = Rectangle.TOP_BORDER;
                table3.AddCell(c1115);



                //New Column Added
                Paragraph pr53545 = new Paragraph();
                pr53545.Add(new Phrase("\n\n   Invoice No\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));


                PdfPCell c11256465 = new PdfPCell(pr53545);
                c11256465.Border = Rectangle.TOP_BORDER;
                c11256465.FixedHeight = 50f;
                c11256465.HorizontalAlignment = 2;
                table3.AddCell(c11256465);


                //var dtt = crdata.DocDate.ToString(); ;
                Paragraph pr53549 = new Paragraph();
                pr53549.Add(new Phrase("\n\n: " + crdata.InvoiceNo + "\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));


                PdfPCell c112219 = new PdfPCell(pr53549);
                c112219.Border = Rectangle.TOP_BORDER;
                c112219.FixedHeight = 50f;
                c112219.HorizontalAlignment = 0;
                table3.AddCell(c112219);





                PdfPTable table6 = new PdfPTable(3);
                float[] width6 = new float[] { 5f, 7f, 4f };
                table6.SetWidths(width6);
                table6.WidthPercentage = 98; ;
                table6.HorizontalAlignment = 1;

                Paragraph pr18 = new Paragraph();
                pr18.Add(new Phrase("_", FontFactory.GetFont("Arial", 1, Font.NORMAL)));

                PdfPCell pc18 = new PdfPCell(pr18);
                pc18.HorizontalAlignment = 0;
                pc18.Border = Rectangle.BOTTOM_BORDER;
                table6.AddCell(pc18);

                Paragraph pr28 = new Paragraph();
                pr28.Add(new Phrase(".", FontFactory.GetFont("Arial", 1, Font.NORMAL)));
                PdfPCell pc28 = new PdfPCell(pr28);
                pc28.HorizontalAlignment = 1;
                pc28.Border = Rectangle.BOTTOM_BORDER;
                table6.AddCell(pc28);

                Paragraph pr38 = new Paragraph();
                pr38.Add(new Phrase("", FontFactory.GetFont("Arial", 1, Font.NORMAL)));
                PdfPCell pc38 = new PdfPCell(pr38);
                pc38.HorizontalAlignment = 1;
                pc38.Border = Rectangle.BOTTOM_BORDER;
                table6.AddCell(pc38);

                Paragraph pr388 = new Paragraph();
                pr388.Add(new Phrase("", FontFactory.GetFont("Arial", 1, Font.NORMAL)));
                PdfPCell pc388 = new PdfPCell(pr388);
                pc388.HorizontalAlignment = 1;
                pc388.Border = Rectangle.BOTTOM_BORDER;
                table6.AddCell(pc388);

                document.Add(table4);
                document.Add(table3);
                document.Add(table6);
                document.Add(table5);
                document.Add(table);
                document.Add(table1);
                // document.Add(table7);

                document.Add(table9);
                document.Close();

                var result = new { Message = "success", FileName = filename1 };
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception EX)
            {
                var result = new { Message = EX.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }
        public FileResult GetReport()
        {
            string FileName = "";
            try
            {
                FileName = Session["fileName1"].ToString();
                byte[] FileBytes = System.IO.File.ReadAllBytes(FileName);
                return File(FileBytes, "application/pdf");
            }
            catch
            {
                byte[] FileBytes = System.IO.File.ReadAllBytes(FileName);
                return File(FileBytes, "application/pdf");
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

        public JsonResult GetData()
        {

            var grndetails = db.Sales.Where(a => a.ReturnQty > 0 && a.CreditDocNo != null);

            return Json(grndetails, JsonRequestBehavior.AllowGet);
        }

        [ValidateInput(false)]
        public void ExportToExcel(string GridModel)
        {
            ExcelExport exp = new ExcelExport();

            var results1 = from p in db.Sales
                           where p.ReturnQty > 0 /*&& p.CreditDocNo == null*/

                           // join cst in db.Customers on p.CustomerID equals cst.CustomerID into Customers
                           //    group p by p.InvoiceNo into g

                           join Custss in db.orderMain on p.OrderNo equals Custss.OrderNo into customerss
                           from aa in customerss.DefaultIfEmpty()

                           select new { CustomerName = aa.CustomerName, InvoiceNo = p.InvoiceNo, InvoiceDate = p.InvoiceDate, ReturnItems = p.ReturnQty, Amount = (p.AmountPerUnit * p.ReturnQty), InvoiceID = p.SalesId };


            var DataSource = from p in results1
                             group p by p.InvoiceNo into g
                             select new { CustomerName = g.Select(x => x.CustomerName).FirstOrDefault(), InvoiceNo = g.Select(x => x.InvoiceNo).FirstOrDefault(), InvoiceDate = g.Select(x => x.InvoiceDate).FirstOrDefault(), ReturnItems = g.Sum(x => x.ReturnItems), Amount = g.Sum(x => x.Amount), InvoiceID = g.Max(a => a.InvoiceID) };


            GridProperties obj = ConvertGridObject(GridModel);



            exp.Export(obj, DataSource, "CreditNote.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
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