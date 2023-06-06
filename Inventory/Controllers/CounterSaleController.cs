using Inventory.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Ajax.Utilities;
using Syncfusion.JavaScript;
using Syncfusion.Olap.MDXQueryParser;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class CounterSaleController : Controller
    {
        // GET: CounterSale
        private InventoryModel db = new InventoryModel();
        public ActionResult Index()
        {
            //ViewBag.datasource = (from Order in db.orderMain.Where(a=>a.IsCashCustomer==true)

            //                      join Saless in db.Sales on Order.OrderNo equals Saless.OrderNo into SalesData  
            //                      from sl in SalesData.DefaultIfEmpty()

            //                      orderby Order.OrderID descending
            //                      select new { sl.InvoiceNo,sl.InvoiceDate, OrderID = Order.OrderID,CurrentStatus = Order.CurrentStatus, DeliverTo = Order.DeliverTo, NetAmount = Order.NetAmount, Discount = Order.Discount, Freeze = Order.Freeze, IGST = Order.IGST, SGST = Order.SGST, CGST = Order.CGST, TotalAmount = Order.TotalAmount, CustomerName=Order.CustomerName }
            //                    ).ToList();


            //            var data = (
            //    from Order in db.orderMain.Where(a => a.IsCashCustomer == true)
            //    join Saless in db.Sales on Order.OrderNo equals Saless.OrderNo into SalesData
            //    from sl in SalesData.DefaultIfEmpty()
            //    orderby Order.OrderID descending
            //    group new { sl.InvoiceNo, sl.InvoiceDate, Order.OrderID, Order.CurrentStatus, Order.DeliverTo, Order.NetAmount, Order.Discount, Order.Freeze, Order.IGST, Order.SGST, Order.CGST, Order.TotalAmount, Order.CustomerName }
            //    by new { Order.OrderID, Order.CurrentStatus, Order.DeliverTo, Order.NetAmount, Order.Discount, Order.Freeze, Order.IGST, Order.SGST, Order.CGST, Order.TotalAmount, Order.CustomerName } into groupedData
            //    select new
            //    {

            //        groupedData.Key.OrderID,
            //        groupedData.Key.CurrentStatus,
            //        groupedData.Key.DeliverTo,
            //        groupedData.Key.NetAmount,
            //        groupedData.Key.Discount,
            //        groupedData.Key.Freeze,
            //        groupedData.Key.IGST,
            //        groupedData.Key.SGST,
            //        groupedData.Key.CGST,
            //        groupedData.Key.TotalAmount,
            //        groupedData.Key.CustomerName,
            //        //Count = groupedData.Count(),
            //        InvoiceNo = groupedData.Max(x => x.InvoiceNo),
            //        InvoiceDate = groupedData.Max(x => x.InvoiceDate),
            //        //TotalDiscount = groupedData.Sum(x => x.Discount)
            //    }
            //).ToList();

            var data = (
    from o in db.orderMain
    where o.IsCashCustomer == true
    join s in db.Sales on o.OrderNo equals s.OrderNo into salesData
    from sl in salesData.DefaultIfEmpty()
    where sl.InvoiceNo.StartsWith("INV/")
    group new { sl.InvoiceNo, sl.InvoiceDate, o.OrderNo, o.OrderID, o.CurrentStatus, o.DeliverTo, o.NetAmount, o.Discount, o.Freeze, o.IGST, o.SGST, o.CGST, o.TotalAmount, o.CustomerName } by new { o.OrderNo, o.OrderID, o.CurrentStatus, o.DeliverTo, o.NetAmount, o.Discount, o.Freeze, o.IGST, o.SGST, o.CGST, o.TotalAmount, o.CustomerName } into groupedData
    orderby groupedData.Key.OrderID descending
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



            ViewBag.datasource = data.OrderByDescending(a => a.InvoiceNo).ToList();
            return View();
        }

        public JsonResult getSerialNoData(string SerialNo, string ProdCode, long tempSRNO)
        {
            if (SerialNo == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var data = db.ProductSerialNo.Where(a => a.SerialNo == SerialNo && (a.Status == "inward" || a.Status == "SO Return") && a.ProductCode == ProdCode).FirstOrDefault();
                if (data != null)
                {
                    data.tempSRNO = tempSRNO;
                    db.SaveChanges();
                    var result = new { Message = "success", data };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = new { Message = "Serial No Not Found" };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public JsonResult ViewScanned(string ProdCode, long tempSRNO)
        {

            var data = db.ProductSerialNo.Where(a => a.ProductCode == ProdCode && a.tempSRNO == tempSRNO && (a.Status == "inward" || a.Status == "SO Return")).ToList();

            var result = new { Message = "success", data };
            return Json(result, JsonRequestBehavior.AllowGet);

        }


        public JsonResult ViewScannedEdit(string ProdCode, long tempSRNO)
        {

            var data = db.ProductSerialNo.Where(a => a.ProductCode == ProdCode && a.tempSRNO == tempSRNO).ToList();

            var result = new { Message = "success", data };
            return Json(result, JsonRequestBehavior.AllowGet);

        }
        public JsonResult RemoveSerialNO(string ProdCode, long tempSRNO, string SerialNo)
        {

            var data = db.ProductSerialNo.Where(a => a.ProductCode == ProdCode && a.tempSRNO == tempSRNO && a.SerialNo == SerialNo && (a.Status == "inward" || a.Status == "SO Return")).FirstOrDefault();
            if (data != null)
            {
                data.tempSRNO = null;
                db.SaveChanges();
            }

            var result = new { Message = "success", data };
            return Json(result, JsonRequestBehavior.AllowGet);


        }
        public JsonResult RemoveSerialNOForEdit(string ProdCode, long tempSRNO, string SerialNo)
        {

            var data = db.ProductSerialNo.Where(a => a.ProductCode == ProdCode && a.tempSRNO == tempSRNO && a.SerialNo == SerialNo && (a.Status == "Sold" || a.Status == "Sale")).FirstOrDefault();
            if (data != null)
            {
                data.tempSRNO = null;
                data.Status = "inward";
                data.UpdatedBy = null;
                data.UpdateDate = null;
                data.InvoiceNo = null;
                db.SaveChanges();
            }

            var result = new { Message = "success", data };
            return Json(result, JsonRequestBehavior.AllowGet);


        }

        //public JsonResult RemoveSerialNO(string ProdCode, long tempSRNO)
        //{

        //    var data = db.ProductSerialNo.Where(a => a.ProductCode == ProdCode && a.tempSRNO == tempSRNO).FirstOrDefault();
        //    if (data != null)
        //    {
        //        data.tempSRNO = null;
        //        db.SaveChanges();
        //    }
        //    var result = new { Message = "success", data };
        //    return Json(result, JsonRequestBehavior.AllowGet);


        //}

        public ActionResult Create()
        {
            ViewBag.Empdatasource = db.Employees.Where(a => a.IsActive == true).OrderBy(a => a.EmployeeName).ToList();
            ViewBag.Products = db.Products.Where(a => a.IsActive == true).OrderBy(a => a.ProductName).ToList();
            ViewBag.SRNO = DateTime.Now.Ticks;
            return View();
        }
        public ActionResult Edit(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.Empdatasource = db.Employees.Where(a => a.IsActive == true).OrderBy(a => a.EmployeeName).ToList();
            ViewBag.Products = db.Products.Where(a => a.IsActive == true).OrderBy(a => a.ProductName).ToList();


            OrderMain order = db.orderMain.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);

        }

        public JsonResult getProductData(string productId)
        {
            if (productId == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var xx = (from p in db.GRNDetail.Where(a => a.ProductCode == productId)
                          group p by 1 into g
                          select new
                          {
                              ReceivedQty = g.Sum(x => x.ReceivedQty),
                              SalesQty = g.Sum(x => x.SalesQty)
                          }).FirstOrDefault();
                var Product = db.Products.Where(a => a.ProductCode == productId && a.IsActive == true).FirstOrDefault();
                var AvailableQty = xx.ReceivedQty - xx.SalesQty;
                var result = new { Product, AvailableQty };


                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult getTax(string ProductId)
        {
            if (ProductId == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var hsn = db.Products.Where(a => a.ProductCode == ProductId).Select(a => a.HsnCode).Single();
                var result = db.TaxMasters.Where(a => a.HSNCode == hsn).FirstOrDefault();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult checkProduct(string PRD)
        {
            if (PRD == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = db.Products.Where(a => a.ProductName == PRD).Count();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult save(List<OrderMain> OrderDetails, long tempSRNO, string DoctorCode)
        {
            var doctordtl = db.DoctorMasterModels.Where(a => a.DoctorCode == DoctorCode).FirstOrDefault();
            DataTable dt = new DataTable();
            dt.Rows.Clear();


            dt.Columns.Add("ProductCode");
            dt.Columns.Add("EndCustomerID");
            dt.Columns.Add("BarcodeApplicable");
            dt.Columns.Add("Price");
            dt.Columns.Add("DiscountAmt");
            dt.Columns.Add("ProductName");
            dt.Columns.Add("CreatedDate");
            dt.Columns.Add("Final");
            dt.Columns.Add("ProductSrNo");
            dt.Columns.Add("PromoCode");
            dt.Columns.Add("DoctorID");
            dt.Columns.Add("Points");
            dt.Columns.Add("InvoiceNo");

            var isSrno = false;

            var batchNo = "";
            var GRNID = 0;
            var count = db.BillNumbering.Where(a => a.Type == "NewSales").FirstOrDefault();

            var Invoiceno = string.Format("INV/2023-24/{0:D4}", count.Number);
            count.Number = count.Number + 1;

            var billNo = db.BillNumbering.Where(a => a.Type == "SOMain").FirstOrDefault();
            var OrderNo = string.Format("SO/2023-24/{0:D4}", billNo.Number);
            billNo.Number = Convert.ToInt32(billNo.Number) + 1;


            var status = false;
            int cnt = 1;
            try
            {
                using (var dbTransaction = db.Database.BeginTransaction())
                {
                    if (OrderDetails.Count > 0)
                    {
                        int code = 0;

                        //  string consString = ConfigurationManager.ConnectionStrings["EcommerceModell"].ConnectionString;
                        foreach (var x in OrderDetails)
                        {
                            var Customerdata = db.Customers.Where(a => a.CustomerName == x.CustomerName).FirstOrDefault();
                            if (cnt == 1)
                            {
                                try
                                {
                                    OrderMain main = new OrderMain();
                                    main.IsCashCustomer = true;



                                    main.CustomerName = x.CustomerName;
                                    if (Customerdata != null)
                                    {
                                        main.CustomerAddress = Customerdata.ShippingAddress;
                                        main.CustomerCity = Customerdata.ShippingCity;
                                        main.CustomerPincode = Customerdata.ShippingPincode;
                                        main.CustomerMobile = Customerdata.ShippingPhone;
                                        main.CustomerGSTNo = Customerdata.GSTNo;
                                        main.CustomerID = Customerdata.CustomerID;
                                    }
                                    else
                                    {
                                        main.CustomerID = 0;
                                        main.CustomerAddress = x.CustomerAddress;
                                    }
                                    main.OrderNo = OrderNo;

                                    main.EmployeeID = "R0007";
                                    main.TermsAndConditions = "";
                                    main.CurrentStatus = "Approve";
                                    main.PayAmount = 0;
                                    main.CompanyID = Convert.ToInt32(Session["CompanyID"]);
                                    main.OrderDate = x.OrderDate;
                                    main.DeliverTo = x.CustomerName;
                                    main.VehicleNo = x.VehicleNo;
                                    main.DispatchDate = x.DispatchDate;
                                    main.Transport = x.Transport;
                                    main.Delivery = x.Delivery;
                                    main.NetAmount = Convert.ToDecimal(x.NetAmount);
                                    main.IGST = Convert.ToDecimal(x.IGST);
                                    main.SGST = Convert.ToDecimal(x.SGST);
                                    main.CGST = Convert.ToDecimal(x.CGST);
                                    main.TotalAmount = Convert.ToDecimal(x.TotalAmount);
                                    main.Discount = Convert.ToDecimal(x.Discount);
                                    main.CreatedBy = User.Identity.Name;
                                    main.CreatedDate = DateTime.Today;
                                    main.TermsAndConditions = x.TermsAndConditions;
                                    main.BarcodeApplicable = x.BarcodeApplicable;
                                    main.Freeze = true;
                                    db.orderMain.Add(main);
                                    db.SaveChanges();
                                    code = db.orderMain.Max(a => a.OrderID);
                                }
                                catch (Exception ee)
                                {
                                    status = false;
                                    return new JsonResult { Data = new { status, ee } };
                                }
                            }

                            var Product = db.Products.Where(p => p.ProductCode == x.ProductCode).FirstOrDefault();

                            var srdata = Product.SerialNoApplicable;
                            if (srdata == true)
                            {
                                var prdsrno = db.ProductSerialNo.Where(a => a.tempSRNO == tempSRNO && a.ProductCode == x.ProductCode).ToList();
                                if (prdsrno.Count == 0)
                                {
                                    var result = new { Message = "Serial No Not Added For " + Product.ProductName };
                                    return Json(result, JsonRequestBehavior.AllowGet);
                                }

                                if (prdsrno.Count != x.OrderQty)
                                {
                                    var result = new { Message = "Order Qty and Serial No Count Missmatch..." };
                                    return Json(result, JsonRequestBehavior.AllowGet);
                                }
                                foreach (var aa in prdsrno)
                                {
                                    var dataa = db.ProductSerialNo.Where(a => a.SerialNoId == aa.SerialNoId).FirstOrDefault();
                                    dataa.DoctorCode = DoctorCode;
                                    dataa.Status = "Sold";
                                    dataa.InvoiceNo = Invoiceno;
                                    dataa.UpdatedBy = User.Identity.Name;
                                    dataa.UpdateDate = DateTime.Today;

                                    //db.ProductSerialNo.AddOrUpdate(dataa);

                                    if (Customerdata != null)
                                    {
                                        dataa.EndCustomerID = Customerdata.CustomerID;
                                    }

                                    var grn = db.GRNDetail.Where(a => a.GRNNo == aa.GrnNo && a.ProductCode == x.ProductCode).FirstOrDefault();
                                    batchNo = grn.BatchNo;
                                    GRNID = grn.GRNId;
                                    grn.SalesQty = grn.SalesQty + 1;
                                }
                            }
                            else
                            {

                                List<TempTable> tmp = db.tempTable.Where(a => a.tempSRNO == tempSRNO && a.ProductCode == x.ProductCode).ToList();
                                foreach (var t in tmp)
                                {
                                    var grn = db.GRNDetail.Where(a => a.GRNId == t.GRNId).FirstOrDefault();
                                    batchNo = grn.BatchNo;
                                    GRNID = grn.GRNId;
                                    grn.SalesQty = grn.SalesQty + t.SalesQty;
                                }

                            }

                            Product.ClosingQuantity = (Convert.ToDecimal(Product.ClosingQuantity) - Convert.ToDecimal(x.OrderQty));
                            Product.OutwardQuantity = (Convert.ToDecimal(Product.OutwardQuantity) + Convert.ToDecimal(x.OrderQty));

                            cnt = cnt + 1;
                            OrderDetails details = new OrderDetails();
                            var bom = db.Bom.Where(a => a.ProductId == x.ProductCode).Count();
                            if (bom != 0)
                            {
                                details.BomApplicable = true;
                            }
                            else
                            {
                                details.BomApplicable = false;
                            }

                            details.OrderNo = OrderNo;
                            details.OrderID = code;
                            details.OrderQty = Convert.ToDecimal(x.OrderQty);
                            details.ProductCode = x.ProductCode;
                            details.GSTPercentage = Convert.ToDecimal(x.GSTPercentage);
                            details.Price = Convert.ToDecimal(x.Price);
                            details.DiscountAmount = Convert.ToDecimal(x.Discount);
                            details.NetAmount = Convert.ToDecimal(x.AmountNew);
                            details.SGSTAmount = Convert.ToDecimal(x.SGSTAmount);
                            details.CGSTAmount = Convert.ToDecimal(x.CGSTAmount);
                            details.IGSTAmount = Convert.ToDecimal(x.IGSTAmount);
                            details.TotalAmount = Convert.ToDecimal(x.TotAmount);
                            details.HSNCode = x.HSNCode;

                            details.IsActive = true;
                            details.BarcodeApplicable = x.BarcodeApplicable;
                            details.DeliveredQty = 0;
                            details.ReturnQty = 0;
                            details.DiscountAs = x.discIn;
                            details.Discount = Convert.ToDecimal(x.discPer);
                            details.CreatedBy = User.Identity.Name;
                            details.CreatedDate = DateTime.Now;
                            details.CompanyID = Convert.ToInt32(Session["CompanyID"]);
                            details.DeliveredQty = x.OrderQty;
                            details.CustomerId = x.CustomerID;
                            details.UOM = Product.UOM;
                            details.tempSRNO = tempSRNO;

                            details.GRN_ID = GRNID;
                            db.orderDetails.Add(details);



                            Sales S = new Sales();
                            S.InvoiceNo = Invoiceno;
                            S.InvoiceDate = x.OrderDate;
                            S.OrderNo = OrderNo;
                            S.ProductCode = x.ProductCode;
                            S.DeliveredQty = x.OrderQty;
                            S.CreatedBy = User.Identity.Name;
                            //  S.BatchNo = x.BatchNo;
                            S.ReturnQty = 0;
                            S.ReplaceQty = 0;
                            //        S.OrderDetailsID = x.OrderDetailsID;
                            S.SerialNoApplicable = Product.SerialNoApplicable;
                            S.CustomerID = x.CustomerID;
                            S.CreatedDate = DateTime.Today;
                            S.CompanyID = Convert.ToInt32(Session["CompanyID"]);
                            S.SelectSerialFrom = "";
                            S.SelectSerialTo = "";
                            S.SODate = Convert.ToDateTime(x.OrderDate);
                            S.GSTPercentage = Convert.ToDecimal(Product.GSTPer);
                            S.BasicRate = Convert.ToDecimal(x.Price);
                            S.DiscountAs = x.discIn;
                            S.BatchNo = batchNo;
                            S.GRN_ID = GRNID;


                            S.SGSTAmount = Convert.ToDecimal(x.SGSTAmount);
                            S.CGSTAmount = Convert.ToDecimal(x.CGSTAmount);
                            S.IGSTAmount = Convert.ToDecimal(x.IGSTAmount);
                            S.TotalAmount = Convert.ToDecimal(x.TotAmount);

                            decimal singalPrice = Convert.ToDecimal(x.Price);
                            string discountAs = x.discIn;
                            decimal Tax = Convert.ToDecimal(Product.GSTPer);
                            decimal disc = 0;
                            if (discountAs == "Rupee") { disc = Convert.ToDecimal(x.Discount); S.Discount = disc; } else { disc = (singalPrice * Convert.ToDecimal(x.Discount)) / 100; S.Discount = disc; }
                            var tAmount = singalPrice - disc;
                            var tax = (tAmount * Tax) / 100;
                            if (Product.SerialNoApplicable == true) { S.IGSTAmount = tax; } else { var taxAmt = tax / 2; S.CGSTAmount = taxAmt; S.SGSTAmount = taxAmt; }
                            S.AmountPerUnit = tAmount + tax;
                            //   S.TotalAmount = Convert.ToDecimal(x.TotAmount);
                            db.Sales.Add(S);

                            if (doctordtl != null)
                            {


                                var Productsr = db.ProductSerialNo.Where(a => a.tempSRNO == tempSRNO && a.ProductCode == x.ProductCode).ToList();

                                foreach (var b in Productsr)
                                {
                                    isSrno = true;
                                    var discoPoint = (Product.MRP * 25) / 100;

                                    int integerValue = (int)discoPoint;


                                    var disccc = (Product.MRP * x.discPer) / 100;
                                    disccc = Product.MRP - disccc;
                                    dt.Rows.Add(b.ProductCode, x.CustomerID, 1,
                                                  Product.MRP, disccc, Product.ProductName, DateTime.Today, "Yes",
                                                  b.SerialNo, doctordtl.DoctorCode, doctordtl.DoctorID,
                                                 integerValue, Invoiceno
                                                  );

                                }
                            }
                        }

                        string Messagess = "";
                        Messagess = "SP Success";

                        if (DoctorCode != "" && isSrno == true)
                        {

                            string consStrings = ConfigurationManager.ConnectionStrings["EcommerceModell"].ConnectionString;
                            using (SqlConnection con = new SqlConnection(consStrings))
                            {
                                using (SqlCommand cmd = new SqlCommand("SP_InsertPoints"))
                                {
                                    // DateTime DRSDt1 = DateTime.ParseExact(DRSDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                                    SqlDataReader reader;
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Connection = con;

                                    cmd.Parameters.AddWithValue("@tblInsertPoints", dt);

                                    cmd.CommandTimeout = 240; //in seconds
                                    con.Open();
                                    Messagess = cmd.ExecuteScalar().ToString();

                                }
                            }
                        }


                        if (Messagess == "SP Success")
                        {

                            db.SaveChanges();
                        }
                        else
                        {
                            var result = new { Message = Messagess };
                            return Json(result, JsonRequestBehavior.AllowGet);
                        }
                        dbTransaction.Commit();
                        var result1 = new { Message = "success" };
                        return Json(result1, JsonRequestBehavior.AllowGet);

                    }
                }

            }
            catch (Exception ee)
            {
                var result = new { Message = ee.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            return new JsonResult { Data = new { status } };
        }

        public JsonResult Update(List<OrderMain> OrderDetails, long tempSRNO, string DoctorCode, int OrderID, string OrderNo)
        {
            var doctordtl = db.DoctorMasterModels.Where(a => a.DoctorCode == DoctorCode).FirstOrDefault();
            DataTable dt = new DataTable();
            dt.Rows.Clear();


            dt.Columns.Add("ProductCode");
            dt.Columns.Add("EndCustomerID");
            dt.Columns.Add("BarcodeApplicable");
            dt.Columns.Add("Price");
            dt.Columns.Add("DiscountAmt");
            dt.Columns.Add("ProductName");
            dt.Columns.Add("CreatedDate");
            dt.Columns.Add("Final");
            dt.Columns.Add("ProductSrNo");
            dt.Columns.Add("PromoCode");
            dt.Columns.Add("DoctorID");
            dt.Columns.Add("Points");
            dt.Columns.Add("InvoiceNo");

            var isSrno = false;

            var batchNo = "";
            var GRNID = 0;
            var count = db.BillNumbering.Where(a => a.Type == "NewSales").FirstOrDefault();

            var Invoicedata = db.Sales.Where(a => a.OrderNo == OrderNo).FirstOrDefault();
            var Invoiceno = Invoicedata.InvoiceNo;

            //var billNo = db.BillNumbering.Where(a => a.Type == "SOMain").FirstOrDefault();
            //var OrderNo = string.Format("SO/2023-24/{0:D4}", billNo.Number);
            //billNo.Number = Convert.ToInt32(billNo.Number) + 1;


            var status = false;
            int cnt = 1;
            try
            {
                using (var dbTransaction = db.Database.BeginTransaction())
                {

                    if (OrderDetails.Count > 0)
                    {
                        int code = 0;

                        //  string consString = ConfigurationManager.ConnectionStrings["EcommerceModell"].ConnectionString;
                        var editeddata = OrderDetails.Where(a => a.OrderDtlID != 0).ToList();
                        foreach (var x in editeddata)
                        {
                            var Product = db.Products.Where(p => p.ProductCode == x.ProductCode).FirstOrDefault();
                            var Customerdata = db.Customers.Where(a => a.CustomerName == x.CustomerName).FirstOrDefault();
                            OrderDetails details = new OrderDetails();
                            details.OrderNo = OrderNo;
                            details.OrderID = OrderID;
                            details.OrderDetailsID = Convert.ToInt32(x.OrderDtlID) > 0 ? x.OrderDtlID : 0;
                            details.OrderQty = Convert.ToDecimal(x.OrderQty);
                            details.DeliveredQty = Convert.ToDecimal(x.OrderQty);
                            details.ProductCode = x.ProductCode;
                            details.TotalAmount = Convert.ToDecimal(x.TotAmount);
                            details.HSNCode = Product.HsnCode;
                            details.GSTPercentage = Convert.ToDecimal(x.GSTPercentage);
                            details.Price = Convert.ToDecimal(x.Price);
                            details.DiscountAmount = Convert.ToDecimal(x.Discount);
                            details.NetAmount = Convert.ToDecimal(x.AmountNew);
                            details.SGSTAmount = Convert.ToDecimal(x.SGSTAmount);
                            details.CGSTAmount = Convert.ToDecimal(x.CGSTAmount);
                            details.IGSTAmount = Convert.ToDecimal(x.IGSTAmount);
                            details.TotalAmount = Convert.ToDecimal(x.TotAmount);
                            details.Discount = Convert.ToDecimal(x.discPer);
                            details.CreatedBy = User.Identity.Name;
                            details.tempSRNO = x.tempSRNO;
                            db.Entry(details).State = EntityState.Modified;
                            db.SaveChanges();

                            var productSerialNo = db.ProductSerialNo.Where(s => s.ProductCode == Product.ProductCode && s.tempSRNO == tempSRNO && (s.Status != "Sold" || s.Status != "Sale")).ToList();
                            foreach (var item in productSerialNo)
                            {
                                item.InvoiceNo = Invoiceno;
                                item.Status = "Sold";
                                item.UpdateDate = DateTime.Today;
                                item.UpdatedBy = User.Identity.Name;
                                db.ProductSerialNo.AddOrUpdate(item);
                            }
                            if (Product.SerialNoApplicable == false)

                            {
                                List<TempTable> tmp = db.tempTable.Where(a => a.tempSRNO == tempSRNO && a.ProductCode == x.ProductCode).ToList();
                                foreach (var t in tmp)
                                {
                                    var grn = db.GRNDetail.Where(a => a.GRNId == t.GRNId).FirstOrDefault();
                                    batchNo = grn.BatchNo;
                                    GRNID = grn.GRNId;
                                    grn.SalesQty = grn.SalesQty + t.SalesQty;
                                    db.GRNDetail.AddOrUpdate(grn);
                                }
                            }



                            OrderMain main = db.orderMain.AsNoTracking().Where(a => a.OrderID == OrderID).FirstOrDefault();
                            main.EmployeeID = "R0007";
                            main.TermsAndConditions = "";
                            main.CurrentStatus = "Approve";
                            main.PayAmount = 0;
                            main.CompanyID = Convert.ToInt32(Session["CompanyID"]);
                            main.OrderDate = x.OrderDate;
                            main.DeliverTo = x.CustomerName;

                            main.NetAmount = Convert.ToDecimal(x.NetAmount);
                            main.IGST = Convert.ToDecimal(x.IGST);
                            main.SGST = Convert.ToDecimal(x.SGST);
                            main.CGST = Convert.ToDecimal(x.CGST);
                            main.TotalAmount = Convert.ToDecimal(x.TotalAmount);
                            main.Discount = Convert.ToDecimal(x.Discount);
                            main.UpdatedBy = User.Identity.Name;
                            main.UpdationDate = DateTime.Today;

                            main.BarcodeApplicable = x.BarcodeApplicable;

                            db.orderMain.AddOrUpdate(main);



                            var salesValue = db.Sales.AsNoTracking().Where(a => a.OrderNo == OrderNo && a.ProductCode == Product.ProductCode && a.InvoiceNo == Invoiceno).FirstOrDefault();
                            var salesid = salesValue.SalesId;
                            //Sales S = new Sales();
                            salesValue.InvoiceNo = Invoiceno;
                            salesValue.OrderNo = OrderNo;
                            salesValue.ProductCode = x.ProductCode;
                            salesValue.InvoiceDate = Convert.ToDateTime(x.OrderDate);
                            salesValue.TotalAmount = Convert.ToDecimal(x.TotAmount);
                            salesValue.DeliveredQty = Convert.ToDecimal(x.OrderQty);
                            //salesValue.SalesId = Convert.ToInt32(salesid);
                            db.Sales.AddOrUpdate(salesValue);





                            Product.ClosingQuantity = (Convert.ToDecimal(Product.ClosingQuantity) - Convert.ToDecimal(x.OrderQty));
                            Product.OutwardQuantity = (Convert.ToDecimal(Product.OutwardQuantity) + Convert.ToDecimal(x.OrderQty));

                            db.SaveChanges();


                        }

                        var adddData = OrderDetails.Where(x => x.OrderDtlID == 0).ToList();
                        foreach (var x in adddData)
                        {
                            var Product = db.Products.Where(p => p.ProductCode == x.ProductCode).FirstOrDefault();
                            var Customerdata = db.Customers.Where(a => a.CustomerName == x.CustomerName).FirstOrDefault();
                            if (Product.SerialNoApplicable == false)

                            {
                                List<TempTable> tmp = db.tempTable.Where(a => a.tempSRNO == tempSRNO && a.ProductCode == x.ProductCode).ToList();
                                foreach (var t in tmp)
                                {
                                    var grn = db.GRNDetail.Where(a => a.GRNId == t.GRNId).FirstOrDefault();
                                    batchNo = grn.BatchNo;
                                    GRNID = grn.GRNId;
                                    grn.SalesQty = grn.SalesQty + t.SalesQty;
                                    db.GRNDetail.AddOrUpdate(grn);
                                }
                            }

                            OrderDetails details = new OrderDetails();

                            details.OrderNo = OrderNo;
                            details.OrderID = OrderID;
                            details.OrderQty = Convert.ToDecimal(x.OrderQty);
                            details.ProductCode = x.ProductCode;
                            details.GSTPercentage = Convert.ToDecimal(x.GSTPercentage);
                            details.Price = Convert.ToDecimal(x.Price);
                            details.DiscountAmount = Convert.ToDecimal(x.Discount);
                            details.NetAmount = Convert.ToDecimal(x.AmountNew);
                            details.SGSTAmount = Convert.ToDecimal(x.SGSTAmount);
                            details.CGSTAmount = Convert.ToDecimal(x.CGSTAmount);
                            details.IGSTAmount = Convert.ToDecimal(x.IGSTAmount);
                            details.TotalAmount = Convert.ToDecimal(x.TotAmount);
                            details.HSNCode = Product.HsnCode;
                            details.CreatedBy = User.Identity.Name;
                            details.CreatedDate = DateTime.Now;
                            details.CompanyID = Convert.ToInt32(Session["CompanyID"]);
                            details.DeliveredQty = x.OrderQty;
                            details.CustomerId = x.CustomerID;
                            details.Discount = Convert.ToDecimal(x.Discount);
                            details.UOM = Product.UOM;
                            details.tempSRNO = tempSRNO;
                            details.GRN_ID = GRNID;
                            db.orderDetails.Add(details);
                            db.SaveChanges();


                            OrderMain main = db.orderMain.AsNoTracking().Where(a => a.OrderID == OrderID).FirstOrDefault();
                            main.EmployeeID = "R0007";
                            main.TermsAndConditions = "";
                            main.CurrentStatus = "Approve";
                            main.PayAmount = 0;
                            main.CompanyID = Convert.ToInt32(Session["CompanyID"]);
                            main.OrderDate = x.OrderDate;
                            main.DeliverTo = x.CustomerName;

                            main.NetAmount = Convert.ToDecimal(x.NetAmount);
                            main.IGST = Convert.ToDecimal(x.IGST);
                            main.SGST = Convert.ToDecimal(x.SGST);
                            main.CGST = Convert.ToDecimal(x.CGST);
                            main.TotalAmount = Convert.ToDecimal(x.TotalAmount);
                            main.Discount = Convert.ToDecimal(x.Discount);
                            main.UpdatedBy = User.Identity.Name;
                            main.UpdationDate = DateTime.Today;

                            main.BarcodeApplicable = x.BarcodeApplicable;

                            db.orderMain.AddOrUpdate(main);



                            Sales S = new Sales();
                            S.InvoiceNo = Invoiceno;
                            S.InvoiceDate = x.OrderDate;
                            S.OrderNo = OrderNo;
                            S.ProductCode = x.ProductCode;
                            S.DeliveredQty = x.OrderQty;
                            S.CreatedBy = User.Identity.Name;
                            //  S.BatchNo = x.BatchNo;
                            S.ReturnQty = 0;
                            S.ReplaceQty = 0;
                            //        S.OrderDetailsID = x.OrderDetailsID;
                            S.SerialNoApplicable = Product.SerialNoApplicable;
                            S.CustomerID = x.CustomerID;
                            S.CreatedDate = DateTime.Today;
                            S.CompanyID = Convert.ToInt32(Session["CompanyID"]);
                            S.SelectSerialFrom = "";
                            S.SelectSerialTo = "";
                            S.SODate = Convert.ToDateTime(x.OrderDate);
                            S.GSTPercentage = Convert.ToDecimal(Product.GSTPer);
                            S.BasicRate = Convert.ToDecimal(x.Price);
                            S.DiscountAs = x.discIn;
                            S.BatchNo = batchNo;
                            S.GRN_ID = GRNID;


                            S.SGSTAmount = Convert.ToDecimal(x.SGSTAmount);
                            S.CGSTAmount = Convert.ToDecimal(x.CGSTAmount);
                            S.IGSTAmount = Convert.ToDecimal(x.IGSTAmount);
                            S.TotalAmount = Convert.ToDecimal(x.TotAmount);




                            decimal singalPrice = Convert.ToDecimal(x.Price);
                            string discountAs = x.discIn;
                            decimal Tax = Convert.ToDecimal(Product.GSTPer);
                            decimal disc = 0;
                            if (discountAs == "Rupee") { disc = Convert.ToDecimal(x.Discount); S.Discount = disc; } else { disc = (singalPrice * Convert.ToDecimal(x.Discount)) / 100; S.Discount = disc; }
                            var tAmount = singalPrice - disc;
                            var tax = (tAmount * Tax) / 100;
                            if (Product.SerialNoApplicable == true) { S.IGSTAmount = tax; } else { var taxAmt = tax / 2; S.CGSTAmount = taxAmt; S.SGSTAmount = taxAmt; }
                            S.AmountPerUnit = tAmount + tax;
                            //   S.TotalAmount = Convert.ToDecimal(x.TotAmount);
                            db.Sales.Add(S);

                        }
                    }



                    string Messagess = "";
                    Messagess = "SP Success";


                    if (Messagess == "SP Success")
                    {

                        db.SaveChanges();
                        dbTransaction.Commit();
                    }
                    else
                    {
                        var result = new { Message = Messagess };
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }

                    var result1 = new { Message = "success" };
                    return Json(result1, JsonRequestBehavior.AllowGet);

                }



            }
            catch (Exception ee)
            {
                var result = new { Message = ee.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }


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
        public JsonResult InvoicePrint(string InvoiceNo)
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
                InvoiceNo = db.Sales.Where(a => a.OrderNo == InvoiceNo).Select(a => a.InvoiceNo).FirstOrDefault();
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
                var filename = InvoiceNo + ".pdf";
                string filename1 = path + "" + numberString + "_" + aa + bb + cc + ".pdf";
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filename1, FileMode.Create));
                document.Open();
                document.AddTitle(InvoiceNo); //Statement-1
                document.AddHeader("content-disposition", "inline;filename=" + InvoiceNo + ".pdf");
                Session["fileName1"] = filename1;
                Session["filename"] = filename;

                int Count = 1;

                var dattta = db.Sales.Where(a => a.InvoiceNo == InvoiceNo).ToList();

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
                                  Transport = om.Transport ?? string.Empty,
                                  DispatchDate = om.DispatchDate ,
                                  Delivery = om.Delivery ?? string.Empty,
                                  VehicleNo = om.VehicleNo ?? string.Empty,
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
                                  MFR = prd.MFR,
                                  MRP = prd.MRP
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
                     c.Delivery,
                     c.DispatchDate,
                     c.Transport,
                     c.VehicleNo,
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
                     c.Size,
                     c.MFR,
                     c.MRP
                 } into gcs
                 select new
                 {

                     gcs.Key.OrderNo,
                     gcs.Key.OrderDate,
                     gcs.Key.InvoiceDate,
                     gcs.Key.DeliveredQty,
                     gcs.Key.Address,
                     gcs.Key.Transport,
                     gcs.Key.VehicleNo,
                     gcs.Key.Delivery,
                     ExpiryDate = gcs.Max(a => a.ExpiryDate),
                     DispatchDate = gcs.Max(a => a.DispatchDate),
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
                     gcs.Key.Size,
                     gcs.Key.MFR,
                     gcs.Key.MRP

                 }).ToList();



                PdfPTable table0 = new PdfPTable(4);
                float[] widths0 = new float[] { 10f, 10f, 10f, 10f };
                table0.SetWidths(widths0);
                table0.WidthPercentage = 98;
                table0.HorizontalAlignment = 1;
                table0.HorizontalAlignment = Element.ALIGN_CENTER;



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
                    Cell102.VerticalAlignment = Element.ALIGN_MIDDLE;  // Center the content vertically
                                                                       // Cell102.SetLeading(10f, 1.0f);
                    Cell102.PaddingTop = 10f;  // Increase the top padding
                    Cell102.PaddingBottom = 10f;  // Increase the bottom padding
                    table0.AddCell(Cell102);


                    Paragraph para3 = new Paragraph();
                    para3.Add(new Phrase("" + InvoiceNo, FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell Cell103 = new PdfPCell(para3);
                    Cell103.HorizontalAlignment = Element.ALIGN_LEFT;
                    Cell103.VerticalAlignment = Element.ALIGN_MIDDLE;  // Center the content vertically
                                                                       // Cell103.SetLeading(10f, 1.0f);
                    Cell103.PaddingTop = 10f;  // Increase the top padding
                    Cell103.PaddingBottom = 10f;  // Increase the bottom padding
                    table0.AddCell(Cell103);


                    Paragraph para4 = new Paragraph();
                    para4.Add(new Phrase("Transport.:", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell Cell104 = new PdfPCell(para4);
                    Cell104.HorizontalAlignment = Element.ALIGN_LEFT;
                    Cell104.VerticalAlignment = Element.ALIGN_MIDDLE;  // Center the content vertically
                                                                       // Cell104.SetLeading(10f, 1.0f);
                    Cell104.PaddingTop = 10f;  // Increase the top padding
                    Cell104.PaddingBottom = 10f;  // Increase the bottom padding
                    table0.AddCell(Cell104);


                    Paragraph para5 = new Paragraph();
                    para5.Add(new Phrase("" + data[0].Transport, FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell Cell105 = new PdfPCell(para5);
                    Cell105.HorizontalAlignment = Element.ALIGN_LEFT;
                    Cell105.VerticalAlignment = Element.ALIGN_MIDDLE;  // Center the content vertically
                    Cell105.PaddingTop = 10f;  // Increase the top padding
                    Cell105.PaddingBottom = 10f;  // Increase the bottom padding
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
                    Cell102.PaddingTop = 10f;  // Increase the top padding
                    Cell102.PaddingBottom = 10f;
                    //Cell102.SetLeading(10f, 1.0f);
                    table0.AddCell(Cell102);

                    var XYZ = data[0].InvoiceDate.HasValue ? data[0].InvoiceDate.Value.ToString("dd-MM-yyyy") : string.Empty;



                    Paragraph para3 = new Paragraph();
                    para3.Add(new Phrase("" + XYZ, FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell Cell103 = new PdfPCell(para3);
                    Cell103.HorizontalAlignment = Element.ALIGN_LEFT;
                    Cell103.VerticalAlignment = Element.ALIGN_MIDDLE;
                    Cell103.PaddingTop = 10f;  // Increase the top padding
                    Cell103.PaddingBottom = 10f;
                    table0.AddCell(Cell103);

                    var XYZ1 = data[0].DispatchDate.HasValue ? data[0].DispatchDate.Value.ToString("dd-MM-yyyy") : string.Empty;


                    Paragraph para4 = new Paragraph();
                    para4.Add(new Phrase("Dispatch Date:", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell Cell104 = new PdfPCell(para4);
                    Cell104.HorizontalAlignment = Element.ALIGN_LEFT;
                    Cell104.VerticalAlignment = Element.ALIGN_MIDDLE;
                    Cell104.PaddingTop = 10f;  // Increase the top padding
                    Cell104.PaddingBottom = 10f;
                    table0.AddCell(Cell104);


                    Paragraph para5 = new Paragraph();
                    para5.Add(new Phrase("" + XYZ1, FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell Cell105 = new PdfPCell(para5);
                    Cell105.HorizontalAlignment = Element.ALIGN_LEFT;
                    Cell105.VerticalAlignment = Element.ALIGN_MIDDLE;
                    Cell105.PaddingTop = 10f;  // Increase the top padding
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
                    Cell102.PaddingTop = 10f;  // Increase the top padding
                    Cell102.PaddingBottom = 10f;
                    table0.AddCell(Cell102);


                    Paragraph para3 = new Paragraph();
                    para3.Add(new Phrase("" + data[0].OrderNo, FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell Cell103 = new PdfPCell(para3);
                    Cell103.HorizontalAlignment = Element.ALIGN_LEFT;
                    Cell103.VerticalAlignment = Element.ALIGN_MIDDLE;
                    Cell103.PaddingTop = 10f;  // Increase the top padding
                    Cell103.PaddingBottom = 10f;
                    table0.AddCell(Cell103);


                    Paragraph para4 = new Paragraph();
                    para4.Add(new Phrase("Delivery:", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    para4.SpacingAfter = 10f;
                    PdfPCell Cell104 = new PdfPCell(para4);
                    Cell104.HorizontalAlignment = Element.ALIGN_LEFT;

                    Cell104.VerticalAlignment = Element.ALIGN_MIDDLE;
                    Cell104.PaddingTop = 10f;  // Increase the top padding
                    Cell104.PaddingBottom = 10f;
                    table0.AddCell(Cell104);


                    Paragraph para5 = new Paragraph();
                    para5.Add(new Phrase("" + data[0].Delivery, FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell Cell105 = new PdfPCell(para5);
                    Cell105.HorizontalAlignment = Element.ALIGN_LEFT;
                    Cell105.VerticalAlignment = Element.ALIGN_MIDDLE;
                    Cell105.PaddingTop = 10f;  // Increase the top padding
                    Cell105.PaddingBottom = 10f;
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
                    Cell102.PaddingTop = 10f;  // Increase the top padding
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
                    Cell103.PaddingTop = 10f;  // Increase the top padding
                    Cell103.PaddingBottom = 10f;
                    table0.AddCell(Cell103);


                    Paragraph para4 = new Paragraph();
                    para4.Add(new Phrase("Vehicle No:", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell Cell104 = new PdfPCell(para4);
                    Cell104.VerticalAlignment = Element.ALIGN_MIDDLE;
                    Cell104.PaddingTop = 10f;  // Increase the top padding
                    Cell104.PaddingBottom = 10f;
                    Cell104.HorizontalAlignment = Element.ALIGN_LEFT;
                    //  Cell104.SetLeading(10f, 1.0f);
                    table0.AddCell(Cell104);


                    Paragraph para5 = new Paragraph();
                    para5.Add(new Phrase("" + data[0].VehicleNo, FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell Cell105 = new PdfPCell(para5);
                    Cell105.VerticalAlignment = Element.ALIGN_MIDDLE;
                    Cell105.PaddingTop = 10f;  // Increase the top padding
                    Cell105.PaddingBottom = 10f;
                    Cell105.HorizontalAlignment = Element.ALIGN_LEFT;
                    Cell105.SetLeading(10f, 1.0f);
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
                ////try
                ////{
                ////    Paragraph para2 = new Paragraph();
                ////    para2.Add(new Phrase("Weight :-", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                ////    PdfPCell Cell102 = new PdfPCell(para2);
                ////    Cell102.HorizontalAlignment = 0;
                ////    Cell102.Colspan = 2;
                ////    Cell102.Border = Rectangle.NO_BORDER;
                ////    table0.AddCell(Cell102);


                ////    Paragraph para3 = new Paragraph();
                ////    para3.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                ////    PdfPCell Cell103 = new PdfPCell(para3);
                ////    Cell103.HorizontalAlignment = 0;
                ////    Cell103.Colspan = 2;
                ////    Cell103.Border = Rectangle.NO_BORDER;
                ////    table0.AddCell(Cell103);



                ////}
                ////catch { }
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
                //    //Paragraph para2 = new Paragraph();
                //    //para2.Add(new Phrase("ORDER DATE :-", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                //    //PdfPCell Cell102 = new PdfPCell(para2);
                //    //Cell102.HorizontalAlignment = 0;
                //    //Cell102.Colspan = 2;
                //    //Cell102.Border = Rectangle.NO_BORDER;
                //    //table0.AddCell(Cell102);


                //    Paragraph para3 = new Paragraph();
                //    para3.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                //    PdfPCell Cell103 = new PdfPCell(para3);
                //    Cell103.HorizontalAlignment = 0;
                //    Cell103.Colspan = 2;
                //    Cell103.Border = Rectangle.NO_BORDER;
                //    table0.AddCell(Cell103);

                //}
                //catch { } 


                //PdfPTable table1 = new PdfPTable(6);
                //float[] widths1 = new float[] {  3f, 3f, 4f, 4f, 3f, 3f };
                //table1.SetWidths(widths1);
                //table1.WidthPercentage = 98;
                //table1.HorizontalAlignment = 1;

                //// string imageURL = Server.MapPath("~/img/logo.png");
                //string imageURL = Server.MapPath("/Photo/MicraftLogo.png");
                //iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(imageURL);
                //jpg.ScaleToFit(50, 80);
                //jpg.SpacingBefore = 1f;
                //jpg.SpacingAfter = 1f;
                //jpg.Alignment = Element.ALIGN_CENTER;

                //Paragraph P117585 = new Paragraph();
                //P117585.Add(new Chunk(jpg, 0, 0, true));
                //P117585.Add(new Phrase("\n", FontFactory.GetFont("Arial", 5, Font.BOLD)));
                //P117585.Add(new Phrase("Siddhivinayak Distributor", FontFactory.GetFont("Arial", 12, Font.BOLD)));
                //P117585.Add(new Phrase("\n", FontFactory.GetFont("Arial", 5, Font.BOLD)));
                //P117585.Add(new Phrase("Shop no 10, Suyog Navkar Building A, \n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                //P117585.Add(new Phrase("Near 7 Loves Chowk, Market Yard Road, Pune 411 037.\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                ////P117585.Add(new Phrase("Tel. No.: 020-79629339\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                ////P117585.Add(new Phrase("D.L. No.: 20B-490622, 21B-490623\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                //P117585.Add(new Phrase("GSTIN: 27ABVPK5495R2Z9\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                //P117585.Add(new Phrase("DL.No: MH-PZ3517351,MH-PZ3517352\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                ////    P117585.Add(new Phrase("E-Mail : aloeshellpharmaa@gmail.com\n\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));


                //PdfPCell Cell1 = new PdfPCell(P117585);
                //Cell1.SetLeading(10f, 1.0f);
                //Cell1.Colspan = 2;
                //Cell1.HorizontalAlignment = 0;
                //table1.AddCell(Cell1);

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
                //Para3.Add(new Phrase("Mobile No :" + data[0].Mobile + "\n", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                //Para3.Add(new Phrase("GST NO :" + data[0].GSTNO + "\n", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                if (data[0].Mobile != null)
                {
                    Para3.Add(new Phrase("Mobile No: " + data[0].Mobile + "\n", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                }

                if (data[0].GSTNO != null)
                {
                    Para3.Add(new Phrase("GST NO: " + data[0].GSTNO + "\n", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                }



                PdfPCell Cell3 = new PdfPCell(Para3);
                Cell3.HorizontalAlignment = 0;
                Cell3.SetLeading(10f, 1.2f);
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
                Cell9.Colspan = 2;
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
                        Cell15.Colspan = 2;
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
                        Para1d6.Add(new Phrase("" + r.MFR, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell Cells16 = new PdfPCell(Para1d6);
                        Cells16.HorizontalAlignment = 1;
                        table2.AddCell(Cells16);

                        //batck
                        Paragraph Para16s = new Paragraph();
                        Para16s.Add(new Phrase("" + r.BatchNo, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell Celsl16 = new PdfPCell(Para16s);
                        Celsl16.HorizontalAlignment = 1;
                        table2.AddCell(Celsl16);





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
                        Para1757.Add(new Phrase("" + r.MRP, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
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
                     TaxableAmount = gcs.Sum(a=>a.Price) * gcs.Sum(a => a.DeliveredQty),
                     SGSTAmount = gcs.Sum(a => a.SGSTAmount),
                     CGSTAmount = gcs.Sum(a => a.CGSTAmount),
                     TotalTAxAmount = gcs.Sum(a => a.CGSTAmount + a.SGSTAmount),
                     GSTPer = gcs.Key.GSTPercentage
                 }).ToList();



                try
                {
                    PdfPCell Cellsa13121 = new PdfPCell(new Phrase(new Phrase("CLASS", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.WHITE))));
                    Cellsa13121.HorizontalAlignment = 1;
                    Cellsa13121.BackgroundColor = BaseColor.BLACK;
                    table3.AddCell(Cellsa13121);


                    PdfPCell Cellsa13122 = new PdfPCell(new Phrase(new Phrase("TAXABLE", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.WHITE))));
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


                        PdfPCell Cellsa13122 = new PdfPCell(new Phrase(new Phrase("" + GSt_5.TaxableAmount, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
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


                        PdfPCell Cellsa13122 = new PdfPCell(new Phrase(new Phrase("" + GSt_12.TaxableAmount, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
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


                        PdfPCell Cellsa13122 = new PdfPCell(new Phrase(new Phrase("" + GSt_18.TaxableAmount, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
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


                        PdfPCell Cellsa13122 = new PdfPCell(new Phrase(new Phrase("" + GSt_28.TaxableAmount, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
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


                        var floatNumber = xx;
                        var xyyz = Convert.ToDecimal(floatNumber - Math.Truncate(floatNumber));
                        var signn = "";
                        decimal minval = 0.50m;
                        if (xyyz > minval)
                        {
                            signn = "+";
                        }
                        else
                        {
                            signn = "-";
                        }

                        PdfPCell Cellsa131210 = new PdfPCell(new Phrase(new Phrase(signn + "" + string.Format("{0:0.00 }", xyyz), FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
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
                        var floatNumber = xx;
                        var xyyz = Convert.ToDecimal(floatNumber - Math.Truncate(floatNumber));
                        var signn = "";
                        decimal minval = 0.50m;
                        if (xyyz > minval)
                        {
                            signn = "+";
                        }
                        else
                        {
                            signn = "-";
                        }


                        PdfPCell Cellsa131210 = new PdfPCell(new Phrase(new Phrase(signn + "" + string.Format("{0:0.00 }", xyyz), FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
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

                    PdfPCell Cellsa131200 = new PdfPCell(new Phrase(new Phrase("TOTAL", FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                    Cellsa131200.HorizontalAlignment = 0;
                    Cellsa131200.BackgroundColor = BaseColor.WHITE;
                    Cellsa131200.Border = Rectangle.LEFT_BORDER;
                    table3.AddCell(Cellsa131200);

                    PdfPCell Cellsa131210 = new PdfPCell(new Phrase(new Phrase(string.Format("{0:0.00 }", TotalAmount), FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
                    Cellsa131210.HorizontalAlignment = 2;
                    Cellsa131210.BackgroundColor = BaseColor.WHITE;
                    Cellsa131210.Border = Rectangle.RIGHT_BORDER;
                    table3.AddCell(Cellsa131210);


                }
                catch { }

                try
                {
                    var AmtInwords = words(Convert.ToInt32(Math.Round(TotalAmount)));
                    PdfPCell Cellsa13121 = new PdfPCell(new Phrase(new Phrase("Amount In Word : " + AmtInwords + " only", FontFactory.GetFont("Arial", 9, Font.BOLD, BaseColor.BLACK))));
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

        public JsonResult ShowGRNDetails(string ProductCode)
        {
            if (ProductCode == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                //var id = db.tempTable.Where(a => a.OrderDetailsID == OrderDetailsid).Count();
                //if (id == 0)
                //{
                var Productsmaster = new List<Products>(db.Products);
                var Storesmaster = new List<StoreLocations>(db.StoreLocations);
                var Warehousemaster = new List<Warehouse>(db.Warehouses);
                var GRNDetailsMaster = new List<GRNDetails>(db.GRNDetail);
                var result = (from grn in GRNDetailsMaster.Where(a => a.ProductCode == ProductCode).Where(a => (a.ReceivedQty - a.SalesQty) != 0)

                              join Product in Productsmaster on grn.ProductCode equals Product.ProductCode into products
                              from prd in products.DefaultIfEmpty()

                              join Warehouse in Warehousemaster on grn.WarehouseID equals Warehouse.WareHouseID into warehouse
                              from Whouse in warehouse.DefaultIfEmpty()

                              join StoreLocation in Storesmaster on grn.StoreLocationId equals StoreLocation.StoreLocationId into storeLoc
                              from store in storeLoc.DefaultIfEmpty()

                              orderby grn.GRNId descending
                              select new { GRNId = grn.GRNId, SerialNoApplicable = prd.SerialNoApplicable, SalesQty = grn.SalesQty, temp = 1, WarehouseID = grn.WarehouseID, StoreLocationId = grn.StoreLocationId, BatchNo = grn.BatchNo, ReceivedQty = grn.ReceivedQty, WareHouseName = Whouse == null ? string.Empty : Whouse.WareHouseName, StoreLocation = store == null ? string.Empty : store.StoreLocation, SerialFrom = grn.SerialFrom, SerialTo = grn.SerialTo }
                                     ).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    var GRN = new List<GRNDetails>(db.GRNDetail);
                //    var Productsmaster = new List<Products>(db.Products);
                //    var Storesmaster = new List<StoreLocations>(db.StoreLocations);
                //    var Warehousemaster = new List<Warehouse>(db.Warehouses);
                //    var TempTableMaster = new List<TempTable>(db.tempTable);
                //    var result = (from tmp in TempTableMaster.Where(a => a.ProductCode == ProductCode && a.OrderDetailsID == OrderDetailsid)
                //                  join Product in Productsmaster on tmp.ProductCode equals Product.ProductCode into products
                //                  from prd in products.DefaultIfEmpty()
                //                  join Warehouse in Warehousemaster on tmp.WarehouseID equals Warehouse.WareHouseID into warehouse
                //                  from Whouse in warehouse.DefaultIfEmpty()
                //                  join StoreLocation in Storesmaster on tmp.StoreLocationId equals StoreLocation.StoreLocationId into storeLoc
                //                  from store in storeLoc.DefaultIfEmpty()
                //                  join grn in GRN on tmp.GRNId equals grn.GRNId into grn1
                //                  from grndetail in grn1.DefaultIfEmpty()
                //                  orderby tmp.GRNId descending
                //                  select new
                //                  {
                //                      GRNId = tmp.GRNId,
                //                      temp = 0,
                //                      SalesQty = tmp.SalesQty,
                //                      WarehouseID = tmp.WarehouseID,
                //                      StoreLocationId = tmp.StoreLocationId,
                //                      BatchNo = tmp.BatchNo,
                //                      ReceivedQty = tmp.ReceivedQty,
                //                      WareHouseName = Whouse == null ? string.Empty : Whouse.WareHouseName,
                //                      SerialFrom = tmp.SerialFrom,
                //                      SerialTo = tmp.SerialTo,
                //                      SelectSerialFrom = tmp.SelectSerialFrom,
                //                      SelectSerialTo = tmp.SelectSerialTo,
                //                      StoreLocation = store == null ? string.Empty : store.StoreLocation
                //                  }).ToList();

                //    return Json(result, JsonRequestBehavior.AllowGet);
                //}
                //  return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SaveTempDetails(List<Sales> GrnData, long tempSRNO)
        {
            var status = false;
            int cnt = 1;
            try
            {
                if (GrnData != null)
                {
                    if (GrnData.Count > 0)
                    {
                        foreach (var x in GrnData.Where(a => a.SalesQty != 0).ToList())
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
                                    tmp.tempSRNO = tempSRNO;
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
                                    grn.tempSRNO = tempSRNO;

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

        public JsonResult GetCustomerData(string CustomerType)
        {
            if (CustomerType == "Retailer")
            {
                var CustomerData = db.Customers.Where(a => a.CustomerName != "Cash Customer").OrderBy(a => a.CustomerName).ToList();
                var result = new { CustomerData };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var CustomerData = db.Customers.Where(a => a.CustomerName == "Cash Customer").OrderBy(a => a.CustomerName).ToList();
                var result = new { CustomerData };
                return Json(result, JsonRequestBehavior.AllowGet);
            }


        }
        public JsonResult ApplyCode(string DoctorCode)
        {
            var doctor = db.DoctorMasterModels.Where(a => a.DoctorCode == DoctorCode).FirstOrDefault();
            if (doctor == null)
            {
                var result = new { Message = "Doctor Code Not Found..." };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = new { Message = "success" };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }


        ///Edit Codr
        ///
        public JsonResult EditGetAllOrderDetails(int OrderId)
        {
            if (OrderId == 0)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var data = db.orderMain.Where(a => a.OrderID == OrderId).FirstOrDefault();
                var CustomerMaster = new List<Customer>(db.Customers);
                var EmployeeMaster = new List<Employee>(db.Employees);
                var Productmaster = new List<Products>(db.Products);
                var OrderDetailsMaster = new List<OrderDetails>(db.orderDetails);
                var result = (from Order in OrderDetailsMaster
                              where Order.OrderID == OrderId

                              join Product in Productmaster on Order.ProductCode equals Product.ProductCode into product
                              from prd in product.DefaultIfEmpty()


                              join Employee in EmployeeMaster on data.EmployeeID equals Employee.EmployeeID into employee
                              from emp in employee.DefaultIfEmpty()

                              orderby Order.OrderID descending
                              select new {CustomerAddress = data.CustomerAddress, Transport= data.Transport, Delivery = data.Delivery, VehicleNo = data.VehicleNo, DispatchDate = data.DispatchDate, OrderNo = data.OrderNo, CurrentStatus = data.CurrentStatus, DisapproveReason = data.DisapproveReason, OrderID = Order.OrderID, OrderQty = Order.OrderQty, Price = Order.Price, CGSTAmount = Order.CGSTAmount, SGSTAmount = Order.SGSTAmount, IGSTAmount = Order.IGSTAmount, DiscountAmount = Order.DiscountAmount, TotalAmount = Order.TotalAmount, DeliveredQty = Order.DeliveredQty, ReturnQty = Order.ReturnQty, ProductCode = Order.ProductCode, GSTPercentage = Order.GSTPercentage, Discount = Order.Discount, DiscountAs = Order.DiscountAs, NetAmount = Order.NetAmount, ProductName = prd == null ? string.Empty : prd.ProductName, CustomerName = data.CustomerName, EmployeeName = emp == null ? string.Empty : emp.EmployeeName, DeliverTo = data.DeliverTo, CustomerID = data.CustomerID, OrderDate = data.OrderDate, HsnCode = prd.HsnCode, isIGST = false, IsActive = Order.IsActive, OrderDetailsID = Order.OrderDetailsID, EmployeeID = emp.EmployeeID, BarcodeApplicable = Order.BarcodeApplicable, tempSRNONew = Order.tempSRNO.ToString(), SerialNoApplicable = prd.SerialNoApplicable }
                                    ).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetSoldSerialNo(int id)
        {
            var inv = db.orderMain.Where(a => a.OrderID == id).FirstOrDefault();
            if (inv == null)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            var sale = db.Sales.Where(a => a.OrderNo == inv.OrderNo).ToList();
            if (sale == null || sale.Count <= 0)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            var result = new List<object>();

            foreach (var productCode in sale)
            {
                var srNoList = db.ProductSerialNo.Where(a => a.InvoiceNo == productCode.InvoiceNo && a.ProductCode == productCode.ProductCode && (a.Status == "Sold" || a.Status == "Sale")).Select(a => a.SerialNo).ToList();
                if (srNoList == null)
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                var ProductName = db.Products.Where(a => a.ProductCode == productCode.ProductCode).FirstOrDefault();
                if (ProductName == null)
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                var item = new
                {
                    ProductCode = ProductName.ProductName,
                    SerialNumbers = srNoList
                };

                result.Add(item);
            }

            ViewBag.SerialNoResult = result;

            return Json(result, JsonRequestBehavior.AllowGet);
        }


    }
}