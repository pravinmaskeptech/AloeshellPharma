using Inventory.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Syncfusion.Olap.MDXQueryParser;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class OrderMainController : Controller
    {
        private InventoryModel db = new InventoryModel();
        // GET: OrderMain
        public ActionResult Create()
        {
            //ViewBag.datasource = db.suppliers.Where(a => a.IsActive == true).ToList();
            ViewBag.Custdatasource = db.Customers.Where(a => a.IsActive == true).ToList();
            ViewBag.Productdatasource = db.Products.Where(a => a.IsActive == true).ToList();
            ViewBag.Empdatasource = db.Employees.Where(a => a.IsActive == true);
            var TandCViewbag = db.TermsAndConditions.Where(a => a.Orders == "Sales" && a.IsActive == true).Select(a => a.TermsAndCondition);
            if (TandCViewbag.Any())
            {
                ViewBag.TermsAndConditions = TandCViewbag;
            }

            var setting1 = db.Settings.Where(c => c.FieldName == "SalesOrder" && c.Setting == true).FirstOrDefault();
            if (setting1 != null)
            {
                ViewBag.SalesOrderSetting = setting1.FieldName;
            }
            var setting2 = db.Settings.Where(c => c.FieldName == "BarcodeApplocable" && c.Setting == true).FirstOrDefault();
            if (setting2 != null)
            {
                ViewBag.BarcodeApplocable = setting2.FieldName;
            }
            
            return View();
        }
        public ActionResult Index()
        {
            var Customermaster = new List<Customer>(db.Customers);
            var OrderMainMaster = new List<OrderMain>(db.orderMain);
            ViewBag.datasource = (from Order in OrderMainMaster.Where(a => a.IsCashCustomer == false && a.OrderNo.StartsWith("SO/"))
                                 
                                  join Customer in Customermaster on Order.CustomerID equals Customer.CustomerID into customer
                                  from cust in customer.DefaultIfEmpty()
                                  orderby Order.OrderID descending
                                  select new { OrderID = Order.OrderID, EmployeeID = Order.EmployeeID, OrderNo = Order.OrderNo, OrderDate = Order.OrderDate, TermsAndConditions = Order.TermsAndConditions, CurrentStatus = Order.CurrentStatus, DeliverTo = Order.DeliverTo, NetAmount = Order.NetAmount, Discount = Order.Discount, Freeze = Order.Freeze, IGST = Order.IGST, SGST = Order.SGST, CGST = Order.CGST, TotalAmount = Order.TotalAmount, CustomerID = cust == null ? string.Empty : cust.CustomerName }
                                ).ToList();
            return View();
        }
        public ActionResult GetData()
        {
            var Customermaster = new List<Customer>(db.Customers);
            var OrderMainMaster = new List<OrderMain>(db.orderMain);
            var result = (from Order in OrderMainMaster
                          where Order.OrderNo.StartsWith("SO/")
                          join Customer in Customermaster on Order.CustomerID equals Customer.CustomerID into customer
                          from cust in customer.DefaultIfEmpty()
                          orderby Order.OrderID descending
                          select new { OrderID = Order.OrderID, EmployeeID = Order.EmployeeID, OrderNo = Order.OrderNo, OrderDate = Order.OrderDate, TermsAndConditions = Order.TermsAndConditions, CurrentStatus = Order.CurrentStatus, DeliverTo = Order.DeliverTo, NetAmount = Order.NetAmount, Discount = Order.Discount, Freeze = Order.Freeze, IGST = Order.IGST, SGST = Order.SGST, CGST = Order.CGST, TotalAmount = Order.TotalAmount, CustomerID = cust == null ? string.Empty : cust.CustomerName }
                                ).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Edit(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.Custdatasource = db.Customers.Where(a => a.IsActive == true).ToList();
            ViewBag.Productdatasource = db.Products.Where(a => a.IsActive == true && a.ProductType == "Finished Good").ToList();
            ViewBag.Empdatasource = db.Employees.Where(a => a.IsActive == true);
            var TandCViewbag = db.TermsAndConditions.Where(a => a.Orders == "Sales" && a.IsActive == true).Select(a => a.TermsAndCondition);
            if (TandCViewbag.Any())
            {
                ViewBag.TermsAndConditions = TandCViewbag;
            }
            //var orderId = db.orderDetails.Where(a => a.OrderID == id && a.DeliveredQty == 0).Count();
            var setting1 = db.Settings.Where(c => c.FieldName == "SalesOrder" && c.Setting == true).FirstOrDefault();
            if (setting1 != null)
            {
                ViewBag.SalesOrderSetting = setting1.FieldName;
            }
            var setting2 = db.Settings.Where(c => c.FieldName == "BarcodeApplocable" && c.Setting == true).FirstOrDefault();
            if (setting2 != null)
            {
                ViewBag.BarcodeApplocable = setting2.FieldName;
            }

            //var setting4 = db.Settings.Where(c => c.FieldName == "Approver" && c.Setting == true).FirstOrDefault();
            //if (setting4 != null)
            //{
            //    ViewBag.Approver = setting4.FieldName;
            //}
            OrderMain order = db.orderMain.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);

        }
        public JsonResult getOrderNo()
        {
            var count = db.BillNumbering.Where(a => a.Type == "SOMain").Select(a => a.Number).Single();         
            var result = string.Format("SO/2023-24/{0:D4}", count);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult getProducts(int CustomerId)
        {
            var datasource = db.Products.Where(a => a.IsActive == true).ToList();

            return new JsonResult { Data = datasource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public JsonResult getProductData(string productId, string CustomerID)
        {
            if (productId == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                dynamic xx;
                decimal? AvailableQty = 0;
                var Product = db.Products.Where(a => a.ProductCode == productId && a.IsActive == true).FirstOrDefault();
                if (Product.SerialNoApplicable == true)
                {

                    xx = (from p in db.GRNDetail.Where(a => a.ProductCode == productId)
                          group p by 1 into g
                          select new
                          {
                              ReceivedQty = g.Sum(x => x.ReceivedQty) - g.Sum(x => x.ReturnQty),
                              SalesQty = db.orderDetails.Where(s => s.ProductCode == productId).Sum(s => s.DeliveredQty) - db.Sales.Where(s => s.ProductCode == productId).Sum(s => s.ReturnQty),
                          }).FirstOrDefault();

                    AvailableQty = xx.ReceivedQty - xx.SalesQty;
                }
                else
                {
                    xx = (from p in db.GRNDetail.Where(a => a.ProductCode == productId)
                          group p by 1 into g
                          select new
                          {
                              ReceivedQty = g.Sum(x => x.ReceivedQty) - g.Sum(x => x.ReturnQty),
                              SalesQty = g.Sum(x => x.SalesQty),
                          }).FirstOrDefault();

                    AvailableQty = xx.ReceivedQty - xx.SalesQty;
                }
                var result = new { Product, AvailableQty };


               

            //if (productId == "")
            //{
            //    return Json(null, JsonRequestBehavior.AllowGet);
            //}
            //else
            //{
            //    var result = db.Products.Where(a => a.ProductCode == productId && a.IsActive == true).FirstOrDefault();
            //    var customer = db.Customers.Where(a => a.CustomerName == CustomerID).FirstOrDefault();
            //    var dataCust = db.CustomerProductRelations.Where(a => a.ProductCode == productId && a.CustomerId == customer.CustomerID).FirstOrDefault();
            //    if(dataCust == null)
            //    {
            //        result.SellingPrice = result.SellingPrice;
            //    }
            //    else
            //    {
            //        result.SellingPrice = dataCust.ProductPrice;
            //    }

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
        public JsonResult isIGST(int Custid)
        {
            if (Custid == 0)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = db.Customers.Where(a => a.CustomerID == Custid).Select(a => a.IGST).Single();
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
        public JsonResult checkCustomerName(string CustName)
        {
            if (CustName == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = db.Customers.Where(a => a.CustomerName == CustName).Count();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetShipToDetails(int Custid)
        {
            try
            {
                if (Custid == 0)
                {
                    return Json(0, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = db.Shippers.Where(a => a.CustomerId == Custid).ToList();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult save(List<OrderMain> OrderDetails)
        {
            var billNo = db.BillNumbering.Where(a => a.Type == "SOMain").FirstOrDefault();
            billNo.Number = Convert.ToInt32(billNo.Number) + 1;
            var status = false;
            int cnt = 1;
            if (OrderDetails.Count > 0)
            {
                try
                {
                    int code = 0;
                    foreach (var x in OrderDetails)
                    {
                        if (cnt == 1)
                        {
                            try
                            {
                                OrderMain main = new OrderMain();
                                main.CustomerID = x.CustomerID;
                                main.EmployeeID = x.EmployeeID;
                                main.OrderNo = x.OrderNo;
                                main.TermsAndConditions = "";
                                main.CurrentStatus = "Approve";
                                main.PayAmount = 0;
                                main.CompanyID = Convert.ToInt32(Session["CompanyID"]);
                                main.OrderDate = DateTime.Now;
                                main.DeliverTo = x.DeliverTo;
                                main.CustomerName = x.DeliverTo;
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
                                main.IsCashCustomer = false;
                                main.Freeze = true;
                                db.orderMain.Add(main);
                                db.SaveChanges();
                                code = db.orderMain.Max(a => a.OrderID);
                            }
                            catch (DbEntityValidationException ee)
                            {
                                status = false;
                                return new JsonResult { Data = new { status } };
                            }
                        }

                        try
                        {
                            var Product = db.Products.Where(p => p.ProductCode == x.ProductCode).Select(p => new { p.UOM }).FirstOrDefault();
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
                            details.OrderNo = x.OrderNo;
                            details.IsActive = true;
                            details.BarcodeApplicable = x.BarcodeApplicable;
                            details.DeliveredQty = 0;
                            details.ReturnQty = 0;
                            details.DiscountAs = x.discIn;
                            details.Discount = Convert.ToDecimal(x.discPer);
                            details.CreatedBy = User.Identity.Name;
                            details.CreatedDate = DateTime.Now;
                            details.CompanyID = Convert.ToInt32(Session["CompanyID"]);

                            details.CustomerId = x.CustomerID;
                            details.UOM = Product.UOM;
                            db.orderDetails.Add(details);
                            db.SaveChanges();
                            status = true;
                        }
                        catch (Exception er)
                        {
                            status = false;
                            return new JsonResult { Data = new { status } };
                        }
                    }

                }
                catch (Exception ee)
                {
                    status = false;
                    return new JsonResult { Data = new { status } };
                }
            }
            return new JsonResult { Data = new { status } };
        }
        public JsonResult Update(List<OrderMain> OrderDetails, string CurrentStatus, string DisapproveReason)
        {
            var Message = "Bom does not exist for ";
            bool status = true;
            if (OrderDetails.Count > 0)
            {
                try
                {
                    foreach (var Z in OrderDetails)
                     {
                        var Prod = db.Products.Where(x => x.ProductCode == Z.ProductCode).Select(x => new { x.ProductClass, x.ClosingQuantity, x.ProductName }).FirstOrDefault();
                        if (Prod.ProductClass == "Manufacture")
                        {
                            var BOM = db.ExplodedBOM.Where(x => x.FinishGoods == Z.ProductCode).Count();
                            if (BOM == 0)
                            {
                                var pomain = db.POMains.Where(x => x.POStatus == "Approve").Select(x => x.PurchaseOrderNo).ToList();
                                var POdetails = db.poDetails.GroupBy(x => x.ProductCode).Select(x => new { OrderQty = x.Sum(a => a.OrderQty), ReceivedQty = x.Sum(a => a.ReceivedQty), PONO = x.Select(a => a.PONO).FirstOrDefault() }).Where(x => pomain.Contains(x.PONO) && (x.OrderQty - x.ReceivedQty) > 0).ToList();
                                decimal? PoOrderQty= POdetails.Sum(x => x.OrderQty);         
                                decimal? ClosingQty=Prod.ClosingQuantity+PoOrderQty;
                                decimal? OrderQty = (ClosingQty - Z.OrderQty);

                                if (OrderQty <= 0)
                                {
                                    status = false;
                                    if (Message == "Bom does not exist for ")
                                        Message =Message+ Prod.ProductName;
                                    else
                                        Message = Message + ", " + Prod.ProductName;
                                }
                            }
                        }
                    }

                        string EmployeeID = ""; int? CustomerID = 0; string OrderNo = ""; DateTime dt = new DateTime();
                        string DeliverTo = ""; decimal NetAmount = 0;
                        decimal? DiscountAmt = 0; decimal? IGST = 0; decimal? SGST = 0; decimal? CGST = 0; decimal? TotalAmount = 0;
                        int code = 0; string TermsAndConditions = "";

                        var POId = OrderDetails.FirstOrDefault();
                        var OrderDetail = OrderDetails.Select(x => x.OrderDetailsID).ToList();
                        var PoDetails = db.orderDetails.Where(p => !OrderDetail.Contains(p.OrderDetailsID) && p.OrderNo == POId.OrderNo).ToList();
                        db.orderDetails.RemoveRange(PoDetails);
                        db.SaveChanges();

                        foreach (var x in OrderDetails)
                        {
                            decimal Amount = (Convert.ToDecimal(x.Price) * Convert.ToDecimal(x.OrderQty));
                            EmployeeID = x.EmployeeID;
                            CustomerID = x.CustomerID;
                            OrderNo = x.OrderNo;
                            dt = Convert.ToDateTime(x.OrderDate);
                            DeliverTo = x.DeliverTo;
                            NetAmount = NetAmount + Amount;
                            DiscountAmt = DiscountAmt + x.discPer;
                            IGST = IGST + x.IGSTAmount;
                            SGST = SGST + x.SGSTAmount;
                            CGST = CGST + x.CGSTAmount;
                            TermsAndConditions = x.TermsAndConditions;
                            //DiscountPer = DiscountPer + x.Discount;
                            TotalAmount = TotalAmount + x.TotAmount;
                            if (x.OrderDetailsID != 0)
                            {
                                try
                                {
                                    var order = db.orderDetails.Where(a => a.OrderDetailsID == x.OrderDetailsID).FirstOrDefault();
                                    order.ProductCode = x.ProductCode;
                                    order.HSNCode = x.HSNCode;
                                    order.Price = x.Price;
                                    order.GSTPercentage = Convert.ToDecimal(x.GSTPercentage);
                                    order.NetAmount = Amount;
                                    order.CGSTAmount = x.CGSTAmount;
                                    order.SGSTAmount = x.SGSTAmount;
                                    order.IGSTAmount = x.IGSTAmount;
                                    order.DiscountAmount = x.discPer;
                                    order.CompanyID = Convert.ToInt32(Session["CompanyID"]);
                                    order.OrderQty = x.OrderQty;
                                    order.Discount = x.Discount;
                                    order.DiscountAs = x.discIn;
                                    order.IsActive = x.isActive;
                                    order.BarcodeApplicable = x.BarcodeApplicable;
                                    order.UpdatedBy = User.Identity.Name;
                                    order.UpdationDate = DateTime.Today;
                                    order.TotalAmount = x.TotAmount;

                                    db.SaveChanges();
                                   
                                }
                                catch (Exception ex)
                                {
                                    return new JsonResult { Data = new { Message = ex.Message, status=false } };
                                }
                            }
                            else
                            {
                                code = db.orderMain.Max(a => a.OrderID);
                                try
                                {
                                    OrderDetails details = new OrderDetails();
                                    details.OrderID = x.OrderMAinID;
                                    details.OrderQty = Convert.ToDecimal(x.OrderQty);
                                    details.ProductCode = x.ProductCode;
                                    details.Price = Convert.ToDecimal(x.Price);
                                    details.GSTPercentage = Convert.ToDecimal(x.GSTPercentage);
                                    details.DiscountAmount = Convert.ToDecimal(x.discPer);
                                    details.NetAmount = Convert.ToDecimal(Amount);
                                    details.SGSTAmount = Convert.ToDecimal(x.SGSTAmount);
                                    details.CGSTAmount = Convert.ToDecimal(x.CGSTAmount);
                                    details.IGSTAmount = Convert.ToDecimal(x.IGSTAmount);
                                    details.TotalAmount = Convert.ToDecimal(x.TotAmount);
                                    details.HSNCode = x.HSNCode;
                                    details.OrderNo = x.OrderNo;
                                    // details.UOM = x.UOM;
                                    details.CustomerId = x.CustomerID;
                                    details.IsActive = true;
                                    details.DeliveredQty = 0;
                                    details.CompanyID = Convert.ToInt32(Session["CompanyID"]);
                                    details.DiscountAs = x.discIn;
                                    details.Discount = Convert.ToDecimal(x.Discount);
                                    details.CreatedBy = User.Identity.Name;
                                    details.CreatedDate = DateTime.Now;
                                    details.BarcodeApplicable = x.BarcodeApplicable;
                                   

                                    db.orderDetails.Add(details);
                                    db.SaveChanges();
                                 

                                }
                                catch (Exception ee)
                                {
                                    return new JsonResult { Data = new { ee.Message, status = false } };
                                }
                            }
                        }
                        try
                        {
                            var ordermain = db.orderMain.Where(a => a.OrderNo == OrderNo && a.CustomerID == CustomerID).FirstOrDefault();
                            ordermain.EmployeeID = EmployeeID;
                            ordermain.CustomerID = CustomerID;
                            ordermain.OrderDate = dt;
                            ordermain.CompanyID = Convert.ToInt32(Session["CompanyID"]);
                            ordermain.DeliverTo = DeliverTo;
                            ordermain.Discount = DiscountAmt;
                            ordermain.IGST = IGST;
                            ordermain.SGST = SGST;
                            ordermain.CGST = CGST;
                            ordermain.TotalAmount = TotalAmount;
                            ordermain.UpdatedBy = User.Identity.Name;
                            ordermain.TermsAndConditions = TermsAndConditions;
                            ordermain.UpdationDate = DateTime.Now;
                            if (CurrentStatus != "")
                                ordermain.CurrentStatus = CurrentStatus;
                            if (CurrentStatus == "Disapprove")
                                ordermain.DisapproveReason = DisapproveReason;

                            //Generate Production Order
                            //if (CurrentStatus == "Approve")
                            //{
                            //    SaveProductionOrder(ordermain.OrderNo);
                            //}

                            db.SaveChanges();
                            Message = "success";
                            status = true;

                        }
                        catch (Exception ex)
                        {
                            ;
                        }                    
                }
                catch (Exception ee)
                {
                    Message = ee.Message;
                    return new JsonResult { Data = new { Message, status = false } };
                }
            }
            return new JsonResult { Data = new { Message, status = status } };
        }

        //PRODUCTION ORDER
        public void SaveProductionOrder(string OrderNo)
        {
            ProductionOrder order = new ProductionOrder();
            var OrderDetails = db.orderDetails.Where(x => x.OrderNo == OrderNo).Select(x => new { x.ProductCode, x.OrderQty }).ToList();
            List<Products> Prod = new List<Products>();
            var count = db.BillNumbering.Where(a => a.Type == "ProductionOrderID").Select(a => a.Number).Single();
            var Code = "PrdOrderId_" + count;
            order.ProductionOrderID = Code;
            order.Date = DateTime.Now;            
            order.CreatedBy = User.Identity.Name;
            order.CreatedDate = DateTime.Now;
            db.ProductionOrder.Add(order);

            foreach (var Temp in OrderDetails)
            {
                var Product = db.Products.Where(x => x.ProductCode == Temp.ProductCode).FirstOrDefault();
                int AvailableQty = Convert.ToInt32(Product.ClosingQuantity);
                int OrderQty = Convert.ToInt32(Temp.OrderQty);
                int RequiredQty = OrderQty - AvailableQty;
                if (RequiredQty > 0)
                {
                    db.StockStatus.Add(new StockStatus
                    {
                        ProductionOrderID = Code,
                        Product = Product.ProductCode,
                        Qty = RequiredQty,
                        CreatedBy = User.Identity.Name,
                        CreatedDate = DateTime.Now,
                    });
                }
            }
            var BillNumber = db.BillNumbering.Where(a => a.Type == "ProductionOrderID").FirstOrDefault();
            BillNumber.Number = BillNumber.Number + 1;
            db.SaveChanges();
            var result = "success";
            //return Json(result, JsonRequestBehavior.AllowGet);           
        }
        public JsonResult checkEmployee(string EmpName)
        {
            if (EmpName == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = db.Employees.Where(a => a.EmployeeName == EmpName).Count();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult checkShipTo(string Name, int CustId)
        {
            if (Name == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = db.Shippers.Where(a => a.CustomerId == CustId && a.Name == Name).Count();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        //Index
        public JsonResult GetAllOrderDetails(string OrderNo)
        {
            if (OrderNo == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                int id = db.orderMain.Where(a => a.OrderNo == OrderNo).Select(a => a.OrderID).Single();

                var Productmaster = new List<Products>(db.Products);
                var OrderDetailsMaster = new List<OrderDetails>(db.orderDetails);
                var result = (from Order in OrderDetailsMaster
                              where Order.OrderID == id
                              join Product in Productmaster on Order.ProductCode equals Product.ProductCode into product
                              from prd in product.DefaultIfEmpty()
                              orderby Order.OrderID descending
                              select new { OrderID = Order.OrderID, OrderQty = Order.OrderQty, Price = Order.Price, CGSTAmount = Order.CGSTAmount, SGSTAmount = Order.SGSTAmount, IGSTAmount = Order.IGSTAmount, DiscountAmount = Order.DiscountAmount, TotalAmount = Order.TotalAmount, DeliveredQty = Order.DeliveredQty, ReturnQty = Order.ReturnQty, ProductCode = prd == null ? string.Empty : prd.ProductName }
                                    ).ToList();

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        // Edit Form
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

                              join Customers in CustomerMaster on data.CustomerID equals Customers.CustomerID into customar
                              from cust in customar.DefaultIfEmpty()

                              join Employee in EmployeeMaster on data.EmployeeID equals Employee.EmployeeID into employee
                              from emp in employee.DefaultIfEmpty()

                              orderby Order.OrderID descending
                              select new { CurrentStatus = data.CurrentStatus, DisapproveReason = data.DisapproveReason, OrderID = Order.OrderID, OrderQty = Order.OrderQty, Price = Order.Price, CGSTAmount = Order.CGSTAmount, SGSTAmount = Order.SGSTAmount, IGSTAmount = Order.IGSTAmount, DiscountAmount = Order.DiscountAmount, TotalAmount = Order.TotalAmount, DeliveredQty = Order.DeliveredQty, ReturnQty = Order.ReturnQty, ProductCode = Order.ProductCode, GSTPercentage = Order.GSTPercentage, Discount = Order.Discount, DiscountAs = Order.DiscountAs, NetAmount = Order.NetAmount, ProductName = prd == null ? string.Empty : prd.ProductName, CustomerName = cust == null ? string.Empty : cust.CustomerName, EmployeeName = emp == null ? string.Empty : emp.EmployeeName, DeliverTo = data.DeliverTo, CustomerID = cust.CustomerID, OrderDate = data.OrderDate, HsnCode = prd.HsnCode, isIGST = cust.IGST, IsActive = Order.IsActive, OrderDetailsID = Order.OrderDetailsID, EmployeeID = emp.EmployeeID, BarcodeApplicable = Order.BarcodeApplicable }
                                    ).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult getAllProductList()
        {
            var result = db.Products.Where(a => a.IsActive == true).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult InvoicePrint(string OrderNo)
        {
            try
            {
                int Count = 1;
                var xxx= db.orderMain.Where(a => a.OrderNo == OrderNo).FirstOrDefault(); ;
                var trarmandcondition = xxx.TermsAndConditions;
                string CustomerAddress = "", CustomerName = "", OrderDate = "";
                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["InventoryModel"].ToString());
                string query1 = "select OM.TermsAndConditions, P.ProductName,OD.OrderQty,OM.DeliverTo as CustomerAddress,Cust.CustomerName,CONVERT(VARCHAR(10), OM.OrderDate, 103)as OrderDate , OD.Price,p.Size from OrderDetails OD  inner join OrderMains OM on OD.OrderNo = OM.OrderNo inner join Products P on P.ProductCode = OD.ProductCode inner join Customers Cust on Cust.CustomerID=OD.CustomerId  where  OM.OrderNo='" + OrderNo + "'  and OM.IsCashCustomer=0";
                conn.Open();
                SqlCommand cmd1 = new SqlCommand(query1, conn);
                DataTable dt1 = new DataTable();
                dt1.Load(cmd1.ExecuteReader());
                conn.Close();

                foreach (DataRow cdr in dt1.Rows)
                {
                    try
                    {
                        trarmandcondition= cdr["TermsAndConditions"].ToString();
                    }
                    catch
                    {

                    }
                    OrderDate = cdr["OrderDate"].ToString();
                    CustomerAddress = cdr["CustomerAddress"].ToString();
                    CustomerName = cdr["CustomerName"].ToString();
                }

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

                PdfPTable table = new PdfPTable(7);
                float[] widths = new float[] { 0.0f, 1.2f, 4f, 2f, 2f, 2f, 0.0f };
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



                PdfPCell pp4 = new PdfPCell(new Phrase(new Phrase("Order Qty", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                pp4.HorizontalAlignment = 1;
                pp4.BackgroundColor = BaseColor.BLACK;
                table.AddCell(pp4);


                PdfPCell pp45445 = new PdfPCell(new Phrase(new Phrase("Rate", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                pp45445.HorizontalAlignment = 1;
                pp45445.BackgroundColor = BaseColor.BLACK;
                table.AddCell(pp45445);

                PdfPCell pp44545 = new PdfPCell(new Phrase(new Phrase("Amount", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                pp44545.HorizontalAlignment = 2;
                pp44545.BackgroundColor = BaseColor.BLACK;
                table.AddCell(pp44545);


                PdfPCell p7d21 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                p7d21.HorizontalAlignment = 0;
                table.AddCell(p7d21);

                PdfPTable table1 = new PdfPTable(7);
                float[] width1 = new float[] { 0.0f, 1.2f, 4f, 2f, 2f, 2f, 0.0f };
                table1.SetWidths(width1);
                table1.WidthPercentage = 98;
                table1.DefaultCell.Padding = 0;

                int cnt = 1;
                decimal totAmt = 0;
                foreach (DataRow r in dt1.Rows)
                {
                    var qty = r["OrderQty"].ToString();
                    var amt = r["Price"].ToString();
                    var Netamot = Convert.ToDecimal(qty) * Convert.ToDecimal(amt);
                    totAmt = totAmt + Netamot;
                    //var orderdetails=db.orderDetails.Where(a=>a.OrderNo==r.)
                    if (dt1.Rows.Count > 0)
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
                        pr32.Add(new Phrase("" + r["ProductName"].ToString() + " - "+r["Size"].ToString(), FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell prr32 = new PdfPCell(pr32);
                        prr32.HorizontalAlignment = 0;
                        table1.AddCell(prr32);


                        Paragraph pr34 = new Paragraph();
                        pr34.Add(new Phrase("" +   string.Format("{0:0.00 }", r["OrderQty"].ToString()) + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell prr34 = new PdfPCell(pr34);
                        prr34.HorizontalAlignment = 1;
                        table1.AddCell(prr34);


                        Paragraph pr346 = new Paragraph();
                        pr346.Add(new Phrase("" +  string.Format("{0:0.00 }", r["Price"].ToString()) + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell prr34878 = new PdfPCell(pr346);
                        prr34878.HorizontalAlignment = 1;
                        table1.AddCell(prr34878);

                        Paragraph pr359894 = new Paragraph();
                        pr359894.Add(new Phrase("" +  string.Format("{0:0.00 }", Netamot) + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
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
                    pr931.Add(new Phrase("Amount In Words : "+ AmtInwords, FontFactory.GetFont("Arial", 8, Font.BOLD)));
                    PdfPCell prr931 = new PdfPCell(pr931);
                    prr931.HorizontalAlignment = 0;
                    prr931.Colspan = 4;
                    table1.AddCell(prr931);


                    Paragraph pr938781 = new Paragraph();
                    pr938781.Add(new Phrase("TOTAL : ", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell prr954531 = new PdfPCell(pr938781);
                    prr954531.HorizontalAlignment = 1;
                   
                    table1.AddCell(prr954531);


                    Paragraph pr359894 = new Paragraph();
                    pr359894.Add(new Phrase("" + string.Format("{0:0.00 }",Math.Round(totAmt)) + "", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell prr3sdd4 = new PdfPCell(pr359894);
                    prr3sdd4.HorizontalAlignment = 2;
                    table1.AddCell(prr3sdd4);

                    Paragraph pr36g44 = new Paragraph();
                    pr36g44.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                    PdfPCell prr36g44 = new PdfPCell(pr36g44);
                    prr36g44.HorizontalAlignment = 0;
                    prr36g44.Border = Rectangle.RIGHT_BORDER;
                    table1.AddCell(prr36g44);

                }catch
                { }

                Paragraph pc1855 = new Paragraph();
                pc1855.Add(new Phrase("", FontFactory.GetFont("Arial", 8, Font.BOLD)));
                PdfPCell pc185 = new PdfPCell(pc1855);
                pc185.HorizontalAlignment = 0;
                pc185.Border = Rectangle.LEFT_BORDER;
                pc185.Colspan = 4;
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
                pr38525.Add(new Phrase("Terms And Conditions : \n\n", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                pr38525.Add(new Phrase("1) Above rates are excluding GST\n", FontFactory.GetFont("Arial", 8, Font.NORMAL)));
                pr38525.Add(new Phrase("2) Post Dated Cheque (PDC) required at the time of delivery of goods\n", FontFactory.GetFont("Arial", 8, Font.NORMAL)));
                pr38525.Add(new Phrase("3) Transportation charges - extra as applicable\n", FontFactory.GetFont("Arial", 8, Font.NORMAL)));
                pr38525.Add(new Phrase(""+trarmandcondition, FontFactory.GetFont("Arial", 8, Font.NORMAL)));
                PdfPCell pc3855478 = new PdfPCell(pr38525);
                pc3855478.HorizontalAlignment = 0;
                //  pc3855478.Border = Rectangle.TOP_BORDER;
                pc3855478.Colspan = 7;
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




                //PdfPTable table7 = new PdfPTable(4);
                //float[] width7 = new float[] { 5.2f, 2f, 0.0f, 0.0f };
                //table7.SetWidths(width7);
                //table7.WidthPercentage = 98;;
                //table7.HorizontalAlignment = 1;

                //Paragraph pr185 = new Paragraph();
                //pr185.Add(new Phrase("        GST NO :  \n\n", FontFactory.GetFont("Arial", 8, Font.BOLD)));
                //PdfPCell pc185 = new PdfPCell();
                //pc185.HorizontalAlignment = 0;
                //pc185.Border = Rectangle.LEFT_BORDER;
                //table7.AddCell(pc185);



                //Paragraph pr285 = new Paragraph();
                //pr285.Add(new Phrase(" For Siddhivinayak Distributor \n\n\n\n", FontFactory.GetFont("Arial", 8, Font.BOLD)));
                //pr285.Add(new Phrase("                                      Authorised Signatory", FontFactory.GetFont("Arial", 7, Font.BOLD)));
                //PdfPCell pc285 = new PdfPCell(pr285);
                //pc285.HorizontalAlignment = 1;
                //pc285.Border = Rectangle.LEFT_BORDER;
                //table7.AddCell(pc285);

                //Paragraph pr385 = new Paragraph();
                //pr385.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                //PdfPCell pc385 = new PdfPCell(pr385);
                //pc385.HorizontalAlignment = 1;
                //pc385.Border = Rectangle.TOP_BORDER;
                //table7.AddCell(pc385);

                //Paragraph pr3855 = new Paragraph();
                //pr3855.Add(new Phrase("sdsd", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                //PdfPCell pc3855 = new PdfPCell(pr3855);
                //pc3855.HorizontalAlignment = 1;
                //pc3855.Border = Rectangle.RIGHT_BORDER;
                //table7.AddCell(pc3855);

                PdfPTable table5 = new PdfPTable(4);
                float[] width5 = new float[] { 0.5f, 7f, 1f, 5f };
                table5.SetWidths(width5);
                table5.WidthPercentage = 98;;
                table5.HorizontalAlignment = 1;

                PdfPCell pc2 = new PdfPCell();
                pc2.HorizontalAlignment = 0;
                pc2.Border = Rectangle.NO_BORDER;
                table5.AddCell(pc2);

                Paragraph pr1 = new Paragraph();
                pr1.Add(new Phrase("Customer Name : ", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                pr1.Add(new Phrase(CustomerName, FontFactory.GetFont("Arial", 9, Font.BOLD)));
                pr1.Add(new Phrase("\n" + CustomerAddress, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
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
                table9.WidthPercentage = 98;;

                Paragraph pr226 = new Paragraph();
                pr226.Add(new Phrase("Date:" + DateTime.Now.ToString("dd/mm/yyyy") + "\n", FontFactory.GetFont("Arial", 8, Font.BOLD)));

                PdfPCell p79013 = new PdfPCell();
                p79013.HorizontalAlignment = 0;
                table9.AddCell(p79013);


                PdfPTable table3 = new PdfPTable(3);
                float[] widths55 = new float[] { 2f, 8f, 10 };
                table3.SetWidths(widths55);
                table3.WidthPercentage = 98;
                table3.HorizontalAlignment = 1;


                Paragraph pr53 = new Paragraph();
                pr53.Add(new Phrase("\n\n   Qtn No\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                pr53.Add(new Phrase("   Date", FontFactory.GetFont("Arial", 9, Font.NORMAL)));

                PdfPCell c112 = new PdfPCell(pr53);
                c112.Border = Rectangle.TOP_BORDER;
                c112.FixedHeight = 50f;
                c112.HorizontalAlignment = 0;
                table3.AddCell(c112);

                Paragraph pr539 = new Paragraph();
                pr539.Add(new Phrase("\n\n: " + OrderNo + "\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                pr539.Add(new Phrase(": " + OrderDate + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));

                PdfPCell c1129 = new PdfPCell(pr539);
                c1129.Border = Rectangle.TOP_BORDER;
                c1129.FixedHeight = 50f;
                c1129.HorizontalAlignment = 0;
                table3.AddCell(c1129);

                Paragraph p49 = new Paragraph();
                p49.Add(new Phrase("Sales Quotation", FontFactory.GetFont("Arial", 13, Font.BOLD)));
                PdfPCell c1115 = new PdfPCell(p49);
                c1115.HorizontalAlignment = 0;
                c1115.Border = Rectangle.TOP_BORDER;
                table3.AddCell(c1115);

                PdfPTable table6 = new PdfPTable(3);
                float[] width6 = new float[] { 5f, 7f, 4f };
                table6.SetWidths(width6);
                table6.WidthPercentage = 98;;
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

    }

}