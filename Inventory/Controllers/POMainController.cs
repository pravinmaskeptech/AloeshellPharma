using Inventory.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebGrease.Activities;

namespace Inventory.Controllers
{
    public class POMainController : Controller
    {
        private InventoryModel db = new InventoryModel();
        public ActionResult Create()
        {
            ViewBag.Warehousedatasource = db.Warehouses.ToList();
            //ViewBag.TermsAndConditions = db.TermsAndConditions.Where(a => a.Orders == "Purchase" && a.IsActive == true).Select(a => a.TermsAndCondition).Single();
            var TandCViewbag = db.TermsAndConditions.Where(a => a.Orders == "Purchase" && a.IsActive == true).Select(a => a.TermsAndCondition);
            if (TandCViewbag.Any())
            {
                ViewBag.TermsAndConditions = TandCViewbag;
            }

            ViewBag.datasource = db.suppliers.Where(a => a.IsActive == true).ToList();
            //ViewBag.Custdatasource = db.Customers.Where(a => a.IsActive == true).ToList();
            ViewBag.Productdatasource = db.Products.Where(a => a.IsActive == true).ToList();
            //var setting = db.Settings.Where(c => c.FieldName == "Approver" && c.Setting == true).FirstOrDefault();
            //if (setting != null)
            //{
            //    ViewBag.ApproverSetting = setting.FieldName;
            //}
            //var setting1 = db.Settings.Where(c => c.FieldName == "SalesOrder" && c.Setting == true).FirstOrDefault();
            //if (setting1 != null)
            //{
            //    ViewBag.SalesOrderSetting = setting1.FieldName;
            //}
            var setting2 = db.Settings.Where(c => c.FieldName == "BarcodeApplocable" && c.Setting == true).FirstOrDefault();
            if (setting2 != null)
            {
                ViewBag.BarcodeApplocable = setting2.FieldName;
            }
            var setting3 = db.Settings.Where(c => c.FieldName == "Warehouse" && c.Setting == true).FirstOrDefault();
            if (setting3 != null)
            {
                ViewBag.Warehouse = setting3.FieldName;
            }
            return View();
        }
        public ActionResult Index()
        {
            var Customersmaster = new List<Customer>(db.Customers);
            var suppliersmaster = new List<Suppliers>(db.suppliers);
            var POMainMaster = new List<POMain>(db.POMains);
            ViewBag.datasource = (from Order in POMainMaster
                                  join Supplier in suppliersmaster on Order.SupplierID equals Supplier.SupplierID into supplier
                                  from sply in supplier.DefaultIfEmpty()

                                  join Customer in Customersmaster on Order.CustomerID equals Customer.CustomerID into customer
                                  from cust in customer.DefaultIfEmpty()


                                  orderby Order.PurchaseOrderID descending
                                  select new { PurchaseOrderID = Order.PurchaseOrderID, PurchaseOrderNo = Order.PurchaseOrderNo, PurchaseOrderDate = Order.PurchaseOrderDate, TermsAndConditions = Order.TermsAndConditions, ExpectedDeliveryDate = Order.ExpectedDeliveryDate, NetAmount = Order.NetAmount, Discount = Order.Discount, IGST = Order.IGST, SGST = Order.SGST, CGST = Order.CGST, TotalAmount = Order.TotalAmount, CustomerID = cust == null ? string.Empty : cust.CustomerName, SupplierID = sply == null ? string.Empty : sply.SupplierName, freeze = Order.freeze, POStatus = Order.POStatus }
                                ).ToList();
            return View();
        }

        public ActionResult GetData()
        {
            var Customersmaster = new List<Customer>(db.Customers);
            var suppliersmaster = new List<Suppliers>(db.suppliers);
            var POMainMaster = new List<POMain>(db.POMains);
            var result = (from Order in POMainMaster
                          join Supplier in suppliersmaster on Order.SupplierID equals Supplier.SupplierID into supplier
                          from sply in supplier.DefaultIfEmpty()

                          join Customer in Customersmaster on Order.CustomerID equals Customer.CustomerID into customer
                          from cust in customer.DefaultIfEmpty()


                          orderby Order.PurchaseOrderID descending
                          select new { PurchaseOrderID = Order.PurchaseOrderID, PurchaseOrderNo = Order.PurchaseOrderNo, PurchaseOrderDate = Order.PurchaseOrderDate, TermsAndConditions = Order.TermsAndConditions, ExpectedDeliveryDate = Order.ExpectedDeliveryDate, NetAmount = Order.NetAmount, Discount = Order.Discount, IGST = Order.IGST, SGST = Order.SGST, CGST = Order.CGST, TotalAmount = Order.TotalAmount, CustomerID = cust == null ? string.Empty : cust.CustomerName, SupplierID = sply == null ? string.Empty : sply.SupplierName, freeze = Order.freeze, POStatus = Order.POStatus }
                                ).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Approve(string Id)
        {
            try
            {
                int POID = Convert.ToInt32(Id);
                var PoMain = db.POMains.Where(x => x.PurchaseOrderID == POID).FirstOrDefault();
                PoMain.POStatus = "Approved";
                db.SaveChanges();
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception EX)
            {
                return Json(EX.Message, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Edit(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.Warehousedatasource = db.Warehouses.ToList();
            var TandCViewbag = db.TermsAndConditions.Where(a => a.Orders == "Purchase" && a.IsActive == true).Select(a => a.TermsAndCondition);
            if (TandCViewbag.Any())
            {
                ViewBag.TermsAndConditions = TandCViewbag;
            }
            ViewBag.datasource = db.suppliers.Where(a => a.IsActive == true).ToList();
            ViewBag.Custdatasource = db.Customers.Where(a => a.IsActive == true).ToList();
            ViewBag.Productdatasource = db.Products.Where(a => a.IsActive == true).ToList();
            //var setting4 = db.Settings.Where(c => c.FieldName == "Approver" && c.Setting == true).FirstOrDefault();
            //if (setting4 != null)
            //{
            //    ViewBag.Approver = setting4.FieldName;
            //}
            var setting2 = db.Settings.Where(c => c.FieldName == "BarcodeApplocable" && c.Setting == true).FirstOrDefault();
            if (setting2 != null)
            {
                ViewBag.BarcodeApplocable = setting2.FieldName;
            }
            var setting3 = db.Settings.Where(c => c.FieldName == "Warehouse" && c.Setting == true).FirstOrDefault();
            if (setting3 != null)
            {
                ViewBag.Warehouse = setting3.FieldName;
            }

            
            POMain order = db.POMains.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }
        public JsonResult getProducts(int SupplierId)
        {
            if (SupplierId != 0)
            {
                var SupplierMaster = new List<Suppliers>(db.suppliers);
                var ProductMaster = new List<Products>(db.Products);
                var ProductSupplierRelation = new List<SupplierProductRelations>(db.SupplierProductRelations);
                var datasource = (from Relation in ProductSupplierRelation

                                  join Product in ProductMaster on Relation.ProductCode equals Product.ProductCode into product1
                                  where (Relation.SupplierId == SupplierId)
                                  from prd in product1.DefaultIfEmpty()

                                  orderby Relation.SupplierProductRelationId descending
                                  select new { ProductCode = prd.ProductCode, ProductName = Relation == null ? string.Empty : prd.ProductName }
                                    ).ToList();
                return new JsonResult { Data = datasource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            return new JsonResult { Data = "", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public JsonResult getProductData(string productId, int supplierId)
        {
            if (productId == "" && supplierId == 0)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                //var ProductName = db.Products.Where(a => a.ProductCode == productId ).Select(a=>a.ProductName).FirstOrDefault();
                //var result = db.SupplierProductRelations.Where(a => a.ProductCode == productId && a.SupplierId == supplierId).FirstOrDefault();
                var ProductMaster = new List<Products>(db.Products);
                var ProductSupplierRelation = new List<SupplierProductRelations>(db.SupplierProductRelations);
                var result = (from Relation in ProductSupplierRelation.Where(a => a.ProductCode == productId && a.SupplierId == supplierId)

                              join Product in ProductMaster on Relation.ProductCode equals Product.ProductCode into product1                                
                                  from prd in product1.DefaultIfEmpty()

                                  orderby Relation.SupplierProductRelationId descending
                                  select new { ProductPrice = Relation.ProductPrice, Discount = Relation.Discount,Tax = Relation.Tax, DiscountIn = Relation.DiscountIn, ProductName = Relation == null ? string.Empty : prd.ProductName }
                                    ).FirstOrDefault();
              
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

      
        public JsonResult isIGST(int spid)
        {
            if (spid == 0)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = db.suppliers.Where(a => a.SupplierID == spid).Select(a => a.IGST).Single();
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
        [HttpPost]
        public JsonResult save(List<POMain> OrderDetails)
        {
            var billNo = db.BillNumbering.Where(a => a.Type == "POMain").FirstOrDefault();
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
                                POMain main = new POMain();
                                main.SupplierID = x.SupplierID;
                                main.PurchaseOrderNo = x.PurchaseOrderNo;
                                main.PurchaseOrderDate = x.PurchaseOrderDate;
                                main.CompanyID = Convert.ToInt32(Session["CompanyID"]);
                                main.ExpectedDeliveryDate = x.ExpectedDeliveryDate;                             
                                main.CustomerID = x.CustomerID;
                                main.ShipVia = "Road";
                                main.ShippingTerms = "Once sold, there is no accept goods";
                                main.NetAmount = Convert.ToDecimal(x.NetAmount);
                                main.IGST = Convert.ToDecimal(x.IGST);
                                main.SGST = Convert.ToDecimal(x.SGST);
                                main.CGST = Convert.ToDecimal(x.CGST);
                                main.TotalAmount = Convert.ToDecimal(x.TotalAmount);
                                main.Discount = Convert.ToDecimal(x.DiscountAmount);

                                main.WarehouseId = x.WarehouseId;
                                main.BarcodeApplicable = x.BarcodeApplicable;
                                main.POStatus = "Active";
                                main.PayAmount = 0;
                                main.freeze = true;
                                main.CreatedBy = User.Identity.Name;
                                main.CreatedDate = DateTime.Today;
                                db.POMains.Add(main);
                                db.SaveChanges();
                                code = db.POMains.Max(a => a.PurchaseOrderID);
                            }
                            catch (Exception ee)
                            {
                                status = false;
                                return new JsonResult { Data = new { status } };
                            }
                        }
                        try
                        {
                            cnt = cnt + 1;
                            PODetails details = new PODetails();
                            details.PurchaseOrderID = code;
                            details.OrderQty = Convert.ToDecimal(x.OrderQty);
                            details.ProductCode = x.ProductCode;
                            details.GSTPercentage = Convert.ToDecimal(x.GSTPercentage);
                            details.Price = Convert.ToDecimal(x.Price);
                            details.NetAmount = Convert.ToDecimal(x.AmountNew);
                            details.SGSTAmount = Convert.ToDecimal(x.SGSTAmount);
                            details.CGSTAmount = Convert.ToDecimal(x.CGSTAmount);
                            details.IGSTAmount = Convert.ToDecimal(x.IGSTAmount);
                            details.TotalAmount = Convert.ToDecimal(x.TotAmount);
                            details.CompanyID = Convert.ToInt32(Session["CompanyID"]);
                            details.HSNCode = x.HSNCode;
                            details.Discount = x.DiscountPercent;

                            if (x.Discount == null)
                            {
                                details.DiscountAmount = 0;
                            }
                            else
                            {
                                details.DiscountAmount = x.Discount;
                            }
                            details.PONO = x.PurchaseOrderNo;
                            details.ReceivedQty = 0;
                            details.IsActive = true;
                            details.DiscountAs = x.DiscountAs;
                            details.CreatedBy = User.Identity.Name;
                            details.CreatedDate = DateTime.Now;
                            details.BarcodeApplicable = x.BarcodeApplicable;
                            status = true;
                            db.poDetails.Add(details);
                            db.SaveChanges();

                            TempData["Temp"] = $"{details.PONO} Saved Successfully";
                            InvoicePrint(x.PurchaseOrderNo);

                        }
                        catch (Exception er)
                        {
                            var a = er.Message;
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
        public JsonResult Update(List<POMain> OrderDetails, string POStatus, string DisapproveReason)
        {
            var status = false;
            int cnt = 1;
            if (OrderDetails.Count > 0)
            {
                try
                {
                    int? CustomerID = 0; string PurchaseOrderNo = ""; DateTime expdt = new DateTime(); DateTime orderdt = new DateTime();
                   decimal NetAmount = 0;
                    decimal? DiscountAmt = 0; decimal? IGST = 0; decimal? SGST = 0; decimal? CGST = 0; decimal? TotalAmount = 0;
                    string TermsAndConditions = "";string WarehouseId = "";  int? SupplierID = 0;
                 

                    //Delete Removed PoDetails
                    var POId = OrderDetails.FirstOrDefault();
                    var OrderDetail = OrderDetails.Select(x => x.PurchaseOrderDetailsID).ToList();
                    var PoDetails = db.poDetails.Where(p => !OrderDetail.Contains(p.PurchaseOrderDetailsID) && p.PurchaseOrderID == POId.PurchaseOrderID).ToList();
                    db.poDetails.RemoveRange(PoDetails);
                    db.SaveChanges();
                    foreach (var x in OrderDetails)
                    {
                        decimal Amount = (Convert.ToDecimal(x.Price) * Convert.ToDecimal(x.OrderQty));
                        //    EmployeeID = x.EmployeeID;
                        CustomerID = x.CustomerID;
                        SupplierID = x.SupplierID;
                        PurchaseOrderNo = x.PurchaseOrderNo;
                        expdt = x.ExpectedDeliveryDate ?? DateTime.MinValue;//Convert.ToDateTime(x.ExpectedDeliveryDate);
                        orderdt = x.PurchaseOrderDate ?? DateTime.MinValue;
                        //Convert.ToDateTime(x.PurchaseOrderDate);                      
                        NetAmount = NetAmount + Amount;
                        DiscountAmt = DiscountAmt + x.DiscountAmount;
                        IGST = IGST + x.IGSTAmount;
                        SGST = SGST + x.SGSTAmount;
                        CGST = CGST + x.CGSTAmount;
                        TermsAndConditions = x.TermsAndConditions;
                        WarehouseId = x.WarehouseId;                       
                        TotalAmount = TotalAmount + x.TotAmount;
                        if (x.PurchaseOrderDetailsID != 0)
                        {
                            try
                            {
                                var order = db.poDetails.Where(a => a.PurchaseOrderDetailsID == x.PurchaseOrderDetailsID).FirstOrDefault();
                                order.ProductCode = x.ProductCode;
                                order.HSNCode = x.HSNCode;
                                order.Price = x.Price;
                                order.GSTPercentage = Convert.ToDecimal(x.GSTPercentage);
                                order.NetAmount = Amount;
                                order.PONO = x.PurchaseOrderNo;
                                order.CGSTAmount = x.CGSTAmount;
                                order.SGSTAmount = x.SGSTAmount;
                                order.IGSTAmount = x.IGSTAmount;
                                order.DiscountAmount = x.DiscountAmount;
                                order.OrderQty = x.OrderQty;
                                order.Discount = x.Discount;
                                order.DiscountAs = x.DiscountAs;
                                order.IsActive = x.IsActive;
                                order.UpdatedBy = User.Identity.Name;
                                order.UpdationDate = DateTime.Today;
                                order.TotalAmount = x.TotAmount;
                                order.BarcodeApplicable = x.BarcodeApplicable;
                                order.IssueToProductionQty = 0;

                                db.SaveChanges();
                                status = true;                                
                            }
                            catch (Exception ex)
                            {
                                status = false;
                                return new JsonResult { Data = new { status } };
                            }
                        }
                        else
                        {
                            try
                            {
                                PODetails details = new PODetails();
                                details.PurchaseOrderID = x.PurchaseOrderID;
                                details.OrderQty = Convert.ToDecimal(x.OrderQty);
                                details.PONO = x.PurchaseOrderNo;
                                details.ProductCode = x.ProductCode;
                                details.Price = Convert.ToDecimal(x.Price);
                                details.GSTPercentage = Convert.ToDecimal(x.GSTPercentage);
                                details.DiscountAmount = Convert.ToDecimal(x.DiscountAmount);
                                details.NetAmount = Convert.ToDecimal(Amount);
                                details.SGSTAmount = Convert.ToDecimal(x.SGSTAmount);
                                details.CGSTAmount = Convert.ToDecimal(x.CGSTAmount);
                                details.IGSTAmount = Convert.ToDecimal(x.IGSTAmount);
                                details.TotalAmount = Convert.ToDecimal(x.TotAmount);
                                details.HSNCode = x.HSNCode;
                                details.ReceivedQty = 0;                                
                                details.IsActive = true;
                                details.DiscountAs = x.DiscountAs;
                                details.BarcodeApplicable = x.BarcodeApplicable;
                                details.Discount = Convert.ToDecimal(x.Discount);
                                details.CreatedBy = User.Identity.Name;
                                details.CreatedDate = DateTime.Now;
                                status = true;
                                db.poDetails.Add(details);
                                db.SaveChanges();
                                status = true;
                            }
                            catch (Exception ee)
                            {
                                status = false;
                                return new JsonResult { Data = new { status } };
                            }
                        }
                    }
                    try
                    {
                        var ordermain = db.POMains.Where(a => a.PurchaseOrderNo == PurchaseOrderNo && a.SupplierID == SupplierID).FirstOrDefault();
                        //ordermain.EmployeeID = EmployeeID;
                        ordermain.CustomerID = CustomerID;
                        ordermain.ExpectedDeliveryDate = expdt;
                        ordermain.PurchaseOrderDate = orderdt;                       
                        ordermain.Discount = DiscountAmt;
                        ordermain.IGST = IGST;
                        ordermain.SGST = SGST;
                        ordermain.CGST = CGST;
                        ordermain.WarehouseId = WarehouseId;
                        ordermain.TotalAmount = TotalAmount;
                        ordermain.ModifiedBy = User.Identity.Name;
                        ordermain.TermsAndConditions = TermsAndConditions;
                        ordermain.ModifiedDate = DateTime.Now;
                        if(POStatus!="")
                            ordermain.POStatus = POStatus;
                        if (POStatus == "Disapprove")
                            ordermain.DisapproveReason = DisapproveReason;
                        db.SaveChanges();
                        status = true;

                    }
                    catch (Exception ex)
                    {
                        ;
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
        public JsonResult getOrderNo()
        {
            var count = db.BillNumbering.Where(a => a.Type == "POMain").Select(a => a.Number).Single();           
            var result = "PONO_" + count;
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAllPODetails(int POID)
        {
            if (POID == 0)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var id = db.POMains.Where(a => a.PurchaseOrderID == POID).FirstOrDefault();
                var Productmaster = new List<Products>(db.Products);
                //var Customersmaster = new List<Customer>(db.Customers);
                var Suppliermaster = new List<Suppliers>(db.suppliers);
                var Warehousesmaster = new List<Warehouse>(db.Warehouses);
                var PODetailsMaster = new List<PODetails>(db.poDetails);
                var result = (from Order in PODetailsMaster
                              where Order.PurchaseOrderID == id.PurchaseOrderID
                              join Product in Productmaster on Order.ProductCode equals Product.ProductCode into product
                              from prd in product.DefaultIfEmpty()

                              //join Customer in Customersmaster on id.CustomerID equals Customer.CustomerID into customer
                              //from cust in customer.DefaultIfEmpty()

                              join Warehouse in Warehousesmaster on id.WarehouseId equals Warehouse.WareHouseID into warehouse1
                              from whouse in warehouse1.DefaultIfEmpty()

                              join Supplier in Suppliermaster on id.SupplierID equals Supplier.SupplierID into supplier
                              from sply in supplier.DefaultIfEmpty()

                              orderby Order.PurchaseOrderDetailsID descending
                              select new { POStatus = id.POStatus, DisapproveReason=id.DisapproveReason, PurchaseOrderDetailsID = Order.PurchaseOrderDetailsID, PurchaseOrderID = Order.PurchaseOrderID, ProductCode = Order.ProductCode, OrderQty = Order.OrderQty, Price = Order.Price, CGSTAmount = Order.CGSTAmount, SGSTAmount = Order.SGSTAmount, IGSTAmount = Order.IGSTAmount, Discount = Order.Discount, TotalAmount = Order.TotalAmount, ReceivedQty = Order.ReceivedQty, ReturnQty = Order.ReturnQty, ProductName = prd == null ? string.Empty : prd.ProductName, WareHouseName = whouse == null ? string.Empty : whouse.WareHouseName, WarehouseId = id.WarehouseId, ExpectedDeliveryDate = id.ExpectedDeliveryDate, PurchaseOrderDate = id.PurchaseOrderDate, GSTPercentage = Order.GSTPercentage, NetAmount = Order.NetAmount, DiscountAs = Order.DiscountAs, SupplierID = id.SupplierID, SupplierName = sply == null ? string.Empty : sply.SupplierName, isIGST = sply.IGST, BarcodeApplicable= Order.BarcodeApplicable }).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetAllPOD(string POID)
        {
            if (POID == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var id = db.POMains.Where(a => a.PurchaseOrderNo == POID).FirstOrDefault();
                var Productmaster = new List<Products>(db.Products);
                var Customersmaster = new List<Customer>(db.Customers);
                var Suppliermaster = new List<Suppliers>(db.suppliers);
                var PODetailsMaster = new List<PODetails>(db.poDetails);
                var result = (from Order in PODetailsMaster
                              where Order.PurchaseOrderID == id.PurchaseOrderID
                              join Product in Productmaster on Order.ProductCode equals Product.ProductCode into product
                              from prd in product.DefaultIfEmpty()

                              //join Customer in Customersmaster on id.CustomerID equals Customer.CustomerID into customer
                              //from cust in customer.DefaultIfEmpty()

                              //join Supplier in Suppliermaster on id.SupplierID equals Supplier.SupplierID into supplier
                              //from sply in supplier.DefaultIfEmpty()

                              orderby Order.PurchaseOrderDetailsID descending
                              select new { PurchaseOrderDetailsID = Order.PurchaseOrderDetailsID, PurchaseOrderID = Order.PurchaseOrderID, ProductCode = Order.ProductCode, OrderQty = Order.OrderQty, Price = Order.Price, CGSTAmount = Order.CGSTAmount, SGSTAmount = Order.SGSTAmount, IGSTAmount = Order.IGSTAmount, Discount = Order.Discount, TotalAmount = Order.TotalAmount, ReceivedQty = Order.ReceivedQty, ReturnQty = Order.ReturnQty, ProductName = prd == null ? string.Empty : prd.ProductName,  ExpectedDeliveryDate = id.ExpectedDeliveryDate, PurchaseOrderDate = id.PurchaseOrderDate, GSTPercentage = Order.GSTPercentage, NetAmount = Order.NetAmount, DiscountAs = Order.DiscountAs, SupplierID = id.SupplierID }).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult InvoicePrint(string PONO)
        {
            ////var PONO = "PO1001";
            var result1 = (from e in db.POMains.Where(a => a.PurchaseOrderNo == PONO)

                           join sply in db.poDetails on e.PurchaseOrderID equals sply.PurchaseOrderID into MainOrder
                           from pd in MainOrder.DefaultIfEmpty()

                               //join Cu in db.VariantMasters on pd.VariantID equals Cu.VariantID into CustomerMasters
                               //from v in CustomerMasters.DefaultIfEmpty()

                           join P in db.Products on pd.ProductCode equals P.ProductCode into ProductMasters
                           from prd in ProductMasters.DefaultIfEmpty()

                           join Pp in db.suppliers on e.SupplierID equals Pp.SupplierID into ProductMasterss
                           from vm in ProductMasterss.DefaultIfEmpty()

                           select new
                           {
                               //vendor Data
                               VendorName = vm.SupplierName,
                               VendorAddress = vm.BillingAddress,
                               VendorCity = vm.BillingCity,

                               VendorPincode = vm.BillingPincode,
                               VendorContactNo = vm.BillingPhone,
                               VendorEmail = vm.BillingEmail,
                               GSTNoEmail = vm.GSTNo,

                               //

                               OrderDate = e.PurchaseOrderDate,
                               PONO = e.PurchaseOrderNo,
                               ExpectedDeliveryDate = e.ExpectedDeliveryDate,
                               DeliveryChallanNo = "",

                               //prodduct

                               ProductName = prd.ProductName,
                               Packing = "",
                               HSNCode = prd.HsnCode,
                               Box = "",
                               Quantity = pd.OrderQty,
                               MRP = "",
                               BasicRate = "",
                               GSTPercent = pd.GSTPercentage,
                               PurchaseRate = pd.Price,
                           }).ToList();



            Document document;
            document = new iTextSharp.text.Document(PageSize.A4, 20f, 20f, 20f, 20f);
            long timeSince1970 = DateTimeOffset.Now.ToUnixTimeSeconds();
            string path = Server.MapPath("~/InvoicePrint/");
            string filename1 = path + User.Identity.Name + timeSince1970 + ".pdf";
            string StikerPrintName = User.Identity.Name + timeSince1970 + ".pdf";
            PdfWriter writer = PdfWriter.GetInstance(document, new System.IO.FileStream(filename1, FileMode.Create));
            document.Open();
            Session["fileName1"] = filename1;

            PdfPTable table1 = new PdfPTable(12);
            float[] width1 = new float[] { 0.1f, 1f, 5f, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f, 2f, 0.1f };
            table1.SetWidths(width1);
            table1.WidthPercentage = 100f;
            table1.DefaultCell.Padding = 1;

            Paragraph Pr1 = new Paragraph();
            Pr1.Add(new Phrase("P  U  R  C  H  A  S  E    O  R  D  E  R", FontFactory.GetFont("Arial", 14, Font.BOLD, BaseColor.WHITE)));
            PdfPCell pc1 = new PdfPCell(Pr1);
            pc1.Colspan = 12;
            pc1.FixedHeight = 25;
            pc1.BackgroundColor = BaseColor.GRAY;
            pc1.HorizontalAlignment = 1;
            table1.AddCell(pc1);

            //string barcode1 = Server.MapPath("~/img/logo.png");
            string barcode1 = Server.MapPath("/Photo/OriginalLogo.png");
            iTextSharp.text.Image barcodejpg1 = iTextSharp.text.Image.GetInstance(barcode1);
            barcodejpg1.ScaleToFit(100, 70);
            barcodejpg1.SpacingBefore = 5f;
            barcodejpg1.SpacingAfter = 1f;
            barcodejpg1.Alignment = Element.ALIGN_LEFT;

            // Second Row
            try
            {
                Paragraph Pr2 = new Paragraph();
                Pr2.Add(new Phrase("Buyer,\n", FontFactory.GetFont("Arial", 8, Font.BOLD)));
                Pr2.Add(new Phrase("Siddhivinayak Distributor,\n", FontFactory.GetFont("Arial", 10, Font.BOLD)));
                Pr2.Add(new Phrase("\n", FontFactory.GetFont("Arial", 6, Font.BOLD)));
                Pr2.Add(new Phrase("Shop no 10, Suyog Navkar Building A,\nNear 7 Loves Chowk, Market Yard Road, Pune 411 037.\n\nGSTIN: 27ABVPK5495R2Z9\n ", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                PdfPCell pc2 = new PdfPCell(Pr2);
                pc2.Colspan = 3;
                pc2.FixedHeight = 120;
                pc2.HorizontalAlignment = 0;
                pc2.Border = Rectangle.LEFT_BORDER;
                table1.AddCell(pc2);
            }
            catch { }
            Paragraph P117585 = new Paragraph();
            P117585.Add(new Phrase("", FontFactory.GetFont("Arial", 11, Font.BOLD)));
            P117585.Add(new Chunk(barcodejpg1, 0, 0, true));
            PdfPCell prr32987456 = new PdfPCell(P117585);
            prr32987456.HorizontalAlignment = 0;
            prr32987456.Colspan = 2;
            prr32987456.Border = Rectangle.NO_BORDER;
            table1.AddCell(prr32987456);
            try
            {
                var Address = result1[0].VendorAddress + ",\n" + result1[0].VendorCity + "," + result1[0].VendorPincode + "\nPhone no: " + result1[0].VendorContactNo + "\nEmail: " + result1[0].VendorEmail;
                Paragraph Pr2 = new Paragraph();
                Pr2.Add(new Phrase("Supplier,\n", FontFactory.GetFont("Arial", 8, Font.BOLD)));
                Pr2.Add(new Phrase(result1[0].VendorName + "\n", FontFactory.GetFont("Arial", 10, Font.BOLD)));
                Pr2.Add(new Phrase("\n", FontFactory.GetFont("Arial", 6, Font.BOLD)));
                Pr2.Add(new Phrase("Address: " + Address, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                Pr2.Add(new Phrase("\n\nGSTIN : " + result1[0].GSTNoEmail, FontFactory.GetFont("Arial", 9, Font.NORMAL)));

                PdfPCell pc2 = new PdfPCell(Pr2);
                pc2.Colspan = 4;
                pc2.HorizontalAlignment = 0;
                table1.AddCell(pc2);
            }
            catch { }
            var XYZ = result1[0].OrderDate.Value.ToString("dd-MM-yyyy");
            string expdate = result1[0].ExpectedDeliveryDate.Value.ToString("dd-MM-yyyy");
            try
            {

                Paragraph Pr2 = new Paragraph();
                Pr2.Add(new Phrase("OR NO   : " + PONO + "\n", FontFactory.GetFont("Arial", 8, Font.BOLD)));
                Pr2.Add(new Phrase("\n", FontFactory.GetFont("Arial", 6, Font.BOLD)));
                Pr2.Add(new Phrase("OR DT   : " + XYZ + "\n", FontFactory.GetFont("Arial", 8, Font.BOLD)));
                Pr2.Add(new Phrase("\n", FontFactory.GetFont("Arial", 6, Font.BOLD)));
                Pr2.Add(new Phrase("SUP DT : " + expdate + "\n", FontFactory.GetFont("Arial", 8, Font.BOLD)));
                Pr2.Add(new Phrase("\n", FontFactory.GetFont("Arial", 6, Font.BOLD)));
                Pr2.Add(new Phrase("TRPT NAME: " + "" + "\n", FontFactory.GetFont("Arial", 8, Font.BOLD)));

                PdfPCell pc2 = new PdfPCell(Pr2);
                pc2.Colspan = 3;
                pc2.HorizontalAlignment = 0;
                table1.AddCell(pc2);
            }
            catch { }


            try
            {

                PdfPCell p1 = new PdfPCell(new Phrase(new Phrase("Sr No", FontFactory.GetFont("Arial", 9, Font.BOLD, BaseColor.WHITE))));
                p1.HorizontalAlignment = 1;
                p1.BackgroundColor = BaseColor.GRAY;
                p1.FixedHeight = 20;
                p1.Colspan = 2;

                //    p1.Border = Rectangle.BOTTOM_BORDER;
                table1.AddCell(p1);

                PdfPCell p2dd = new PdfPCell(new Phrase(new Phrase("Description of Goods", FontFactory.GetFont("Arial", 9, Font.BOLD, BaseColor.WHITE))));
                p2dd.HorizontalAlignment = 1;
                p2dd.BackgroundColor = BaseColor.GRAY;
                //  p2dd.Colspan = 2;
                //      p2dd.Border = Rectangle.BOTTOM_BORDER;
                table1.AddCell(p2dd);

                PdfPCell p2 = new PdfPCell(new Phrase(new Phrase("PACKING", FontFactory.GetFont("Arial", 9, Font.BOLD, BaseColor.WHITE))));
                p2.HorizontalAlignment = 1;
                p2.BackgroundColor = BaseColor.GRAY;
                //        p2.Border = Rectangle.BOTTOM_BORDER;
                table1.AddCell(p2);

                PdfPCell pp5 = new PdfPCell(new Phrase(new Phrase("HSN", FontFactory.GetFont("Arial", 9, Font.BOLD, BaseColor.WHITE))));
                pp5.HorizontalAlignment = 1;
                pp5.BackgroundColor = BaseColor.GRAY;
                //        pp5.Border = Rectangle.BOTTOM_BORDER;
                table1.AddCell(pp5);

                PdfPCell pp55 = new PdfPCell(new Phrase(new Phrase("Box", FontFactory.GetFont("Arial", 9, Font.BOLD, BaseColor.WHITE))));
                pp55.HorizontalAlignment = 1;
                pp55.BackgroundColor = BaseColor.GRAY;
                //        pp55.Border = Rectangle.BOTTOM_BORDER;
                table1.AddCell(pp55);

                PdfPCell p29 = new PdfPCell(new Phrase(new Phrase("Qty", FontFactory.GetFont("Arial", 9, Font.BOLD, BaseColor.WHITE))));
                p29.HorizontalAlignment = 1;
                p29.BackgroundColor = BaseColor.GRAY;
                //         p29.Border = Rectangle.BOTTOM_BORDER;
                //    p29.Colspan = 2;
                table1.AddCell(p29);







                PdfPCell p209 = new PdfPCell(new Phrase(new Phrase("MRP", FontFactory.GetFont("Arial", 9, Font.BOLD, BaseColor.WHITE))));
                p209.HorizontalAlignment = 1;
                p209.BackgroundColor = BaseColor.GRAY;
                //         p29.Border = Rectangle.BOTTOM_BORDER;
                //    p29.Colspan = 2;
                table1.AddCell(p209);



                PdfPCell p298 = new PdfPCell(new Phrase(new Phrase("RATE", FontFactory.GetFont("Arial", 9, Font.BOLD, BaseColor.WHITE))));
                p298.HorizontalAlignment = 1;
                p298.BackgroundColor = BaseColor.GRAY;
                //         p29.Border = Rectangle.BOTTOM_BORDER;
                //    p29.Colspan = 2;
                table1.AddCell(p298);

                PdfPCell p229 = new PdfPCell(new Phrase(new Phrase("GST", FontFactory.GetFont("Arial", 9, Font.BOLD, BaseColor.WHITE))));
                p229.HorizontalAlignment = 1;
                p229.BackgroundColor = BaseColor.GRAY;
                //         p29.Border = Rectangle.BOTTOM_BORDER;
                //    p29.Colspan = 2;
                table1.AddCell(p229);


                PdfPCell pp51 = new PdfPCell(new Phrase(new Phrase("AMOUNT", FontFactory.GetFont("Arial", 9, Font.BOLD, BaseColor.WHITE))));
                pp51.HorizontalAlignment = 2;
                pp51.BackgroundColor = BaseColor.GRAY;
                pp51.Border = Rectangle.BOTTOM_BORDER;

                table1.AddCell(pp51);

                PdfPCell pp5d41 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.BOLD, BaseColor.WHITE))));
                pp5d41.HorizontalAlignment = 2;
                pp5d41.BackgroundColor = BaseColor.GRAY;
                pp5d41.Border = Rectangle.RIGHT_BORDER;

                table1.AddCell(pp5d41);


            }
            catch
            {

            }
            var cntt = 0;
            decimal tottalAmount = 0;
            foreach (var xx in result1)
            {

                cntt++;

                try
                {


                    PdfPCell p1 = new PdfPCell(new Phrase(new Phrase("" + cntt, FontFactory.GetFont("Arial", 9, Font.NORMAL))));
                    p1.HorizontalAlignment = 1;
                    p1.BackgroundColor = BaseColor.WHITE;
                    p1.FixedHeight = 30;
                    p1.Colspan = 2;
                    p1.Border = Rectangle.LEFT_BORDER;
                    table1.AddCell(p1);

                    PdfPCell p2dd = new PdfPCell(new Phrase(new Phrase("" + xx.ProductName, FontFactory.GetFont("Arial", 9, Font.NORMAL))));
                    p2dd.HorizontalAlignment = 0;
                    p2dd.BackgroundColor = BaseColor.WHITE;
                    //  p2dd.Colspan = 2;
                    p2dd.Border = Rectangle.LEFT_BORDER;
                    table1.AddCell(p2dd);


                    PdfPCell pp5 = new PdfPCell(new Phrase(new Phrase("" + xx.Packing, FontFactory.GetFont("Arial", 9, Font.NORMAL))));
                    pp5.HorizontalAlignment = 1;
                    pp5.BackgroundColor = BaseColor.WHITE;
                    pp5.Border = Rectangle.LEFT_BORDER;
                    table1.AddCell(pp5);

                    PdfPCell p2 = new PdfPCell(new Phrase(new Phrase("" + xx.HSNCode, FontFactory.GetFont("Arial", 9, Font.NORMAL))));
                    p2.HorizontalAlignment = 1;
                    p2.BackgroundColor = BaseColor.WHITE;
                    p2.Border = Rectangle.LEFT_BORDER;
                    table1.AddCell(p2);

                    PdfPCell pp545 = new PdfPCell(new Phrase(new Phrase("" + xx.Box, FontFactory.GetFont("Arial", 9, Font.NORMAL))));
                    pp545.HorizontalAlignment = 1;
                    pp545.BackgroundColor = BaseColor.WHITE;
                    pp545.Border = Rectangle.LEFT_BORDER;
                    table1.AddCell(pp545);

                    PdfPCell pp5455 = new PdfPCell(new Phrase(new Phrase("" + xx.Quantity, FontFactory.GetFont("Arial", 9, Font.NORMAL))));
                    pp5455.HorizontalAlignment = 1;
                    pp5455.BackgroundColor = BaseColor.WHITE;
                    pp5455.Border = Rectangle.LEFT_BORDER;
                    table1.AddCell(pp5455);



                    PdfPCell pp55 = new PdfPCell(new Phrase(new Phrase("" + xx.MRP, FontFactory.GetFont("Arial", 9, Font.NORMAL))));
                    pp55.HorizontalAlignment = 1;
                    pp55.BackgroundColor = BaseColor.WHITE;
                    pp55.Border = Rectangle.LEFT_BORDER;
                    table1.AddCell(pp55);

                    PdfPCell p29 = new PdfPCell(new Phrase(new Phrase("" + xx.PurchaseRate, FontFactory.GetFont("Arial", 9, Font.NORMAL))));
                    p29.HorizontalAlignment = 1;
                    p29.BackgroundColor = BaseColor.WHITE;
                    p29.Border = Rectangle.LEFT_BORDER;
                    //    p29.Colspan = 2;


                    table1.AddCell(p29);


                    var totlal = xx.Quantity * xx.PurchaseRate;
                    var gst = (totlal * xx.GSTPercent) / 100;


                    PdfPCell pp855 = new PdfPCell(new Phrase(new Phrase("" + String.Format("{0:0.00}", gst), FontFactory.GetFont("Arial", 9, Font.NORMAL))));
                    pp855.HorizontalAlignment = 1;
                    pp855.BackgroundColor = BaseColor.WHITE;
                    pp855.Border = Rectangle.LEFT_BORDER;
                    table1.AddCell(pp855);

                    var tmtt = totlal + gst;
                    tottalAmount = Convert.ToDecimal(tottalAmount) + Convert.ToDecimal(tmtt);

                    PdfPCell pp51 = new PdfPCell(new Phrase(new Phrase("" + String.Format("{0:0.00}", tmtt), FontFactory.GetFont("Arial", 9, Font.NORMAL))));
                    pp51.HorizontalAlignment = 2;
                    pp51.BackgroundColor = BaseColor.WHITE;
                    pp51.Border = Rectangle.LEFT_BORDER;
                    //   pp51.Colspan = 2;
                    table1.AddCell(pp51);




                    PdfPCell ppzx51 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL))));
                    ppzx51.HorizontalAlignment = 2;
                    ppzx51.BackgroundColor = BaseColor.WHITE;
                    ppzx51.Border = Rectangle.RIGHT_BORDER;
                    //   pp51.Colspan = 2;
                    table1.AddCell(ppzx51);



                }
                catch
                {

                }
                try
                {
                    if (result1.Count == cntt)
                    {
                        var colss = 0;
                        if (cntt == 1) { colss = 260; };
                        if (cntt == 2) { colss = 230; };
                        if (cntt == 3) { colss = 200; };
                        if (cntt == 4) { colss = 170; };
                        if (cntt == 5) { colss = 140; };
                        if (cntt == 6) { colss = 110; };
                        if (cntt == 7) { colss = 80; };
                        if (cntt == 8) { colss = 50; };
                        if (cntt == 9) { colss = 20; };
                        //   if (cntt >8) { colss = 0; };

                        try
                        {


                            PdfPCell p1 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 10, Font.NORMAL))));
                            p1.HorizontalAlignment = 1;
                            p1.BackgroundColor = BaseColor.WHITE;
                            p1.FixedHeight = 30;
                            p1.Colspan = 2;
                            p1.FixedHeight = colss;
                            p1.Border = Rectangle.LEFT_BORDER;
                            table1.AddCell(p1);

                            PdfPCell p2dd = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 10, Font.NORMAL))));
                            p2dd.HorizontalAlignment = 0;
                            p2dd.BackgroundColor = BaseColor.WHITE;
                            //             p2dd.Colspan = 2;
                            p2dd.Border = Rectangle.LEFT_BORDER;
                            table1.AddCell(p2dd);


                            PdfPCell pp5 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 10, Font.NORMAL))));
                            pp5.HorizontalAlignment = 1;
                            pp5.BackgroundColor = BaseColor.WHITE;
                            pp5.Border = Rectangle.LEFT_BORDER;
                            table1.AddCell(pp5);

                            PdfPCell p2 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 10, Font.NORMAL))));
                            p2.HorizontalAlignment = 1;
                            p2.BackgroundColor = BaseColor.WHITE;
                            p2.Border = Rectangle.LEFT_BORDER;
                            table1.AddCell(p2);

                            PdfPCell pp545 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 10, Font.NORMAL))));
                            pp545.HorizontalAlignment = 1;
                            pp545.BackgroundColor = BaseColor.WHITE;
                            pp545.Border = Rectangle.LEFT_BORDER;
                            table1.AddCell(pp545);

                            PdfPCell pp5455 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 10, Font.NORMAL))));
                            pp5455.HorizontalAlignment = 1;
                            pp5455.BackgroundColor = BaseColor.WHITE;
                            pp5455.Border = Rectangle.LEFT_BORDER;
                            table1.AddCell(pp5455);



                            PdfPCell pp55 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 10, Font.NORMAL))));
                            pp55.HorizontalAlignment = 1;
                            pp55.BackgroundColor = BaseColor.WHITE;
                            pp55.Border = Rectangle.LEFT_BORDER;
                            table1.AddCell(pp55);

                            PdfPCell p29 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 10, Font.NORMAL))));
                            p29.HorizontalAlignment = 1;
                            p29.BackgroundColor = BaseColor.WHITE;
                            p29.Border = Rectangle.LEFT_BORDER;
                            //    p29.Colspan = 2;


                            table1.AddCell(p29);


                            PdfPCell pp855 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 10, Font.NORMAL))));
                            pp855.HorizontalAlignment = 1;
                            pp855.BackgroundColor = BaseColor.WHITE;
                            pp855.Border = Rectangle.LEFT_BORDER;
                            table1.AddCell(pp855);



                            PdfPCell pp51 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 10, Font.NORMAL))));
                            pp51.HorizontalAlignment = 2;
                            pp51.BackgroundColor = BaseColor.WHITE;
                            pp51.Border = Rectangle.LEFT_BORDER;
                            //    pp51.Colspan = 2;
                            table1.AddCell(pp51);


                            PdfPCell ppzx51 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 10, Font.NORMAL))));
                            ppzx51.HorizontalAlignment = 2;
                            ppzx51.BackgroundColor = BaseColor.WHITE;
                            ppzx51.Border = Rectangle.RIGHT_BORDER;
                            //   pp51.Colspan = 2;
                            table1.AddCell(ppzx51);

                        }
                        catch
                        {

                        }
                    }
                }
                catch
                {

                }
            }


            try
            {
                Paragraph Pr11 = new Paragraph();
                Pr11.Add(new Phrase("Remark : ", FontFactory.GetFont("Arial", 9, Font.BOLD, BaseColor.WHITE)));
                PdfPCell Pc11 = new PdfPCell(Pr11);
                Pc11.Colspan = 8;
                Pc11.FixedHeight = 30;
                Pc11.HorizontalAlignment = 0;
                Pc11.BackgroundColor = BaseColor.GRAY;
                table1.AddCell(Pc11);

                Paragraph Pr111 = new Paragraph();
                Pr111.Add(new Phrase("Total :                     ", FontFactory.GetFont("Arial", 9, Font.BOLD, BaseColor.WHITE)));
                PdfPCell Pc111 = new PdfPCell(Pr111);
                Pc111.Colspan = 2;
                Pc111.BackgroundColor = BaseColor.GRAY;
                Pc111.HorizontalAlignment = 0;
                Pc111.Border = Rectangle.TOP_BORDER;
                table1.AddCell(Pc111);


                var xm = Math.Round(tottalAmount);

                var xmm = xm.ToString("#.00");
                Paragraph Pr111e = new Paragraph();
                Pr111e.Add(new Phrase("" + xmm, FontFactory.GetFont("Arial", 9, Font.BOLD, BaseColor.WHITE)));
                PdfPCell Pc111s = new PdfPCell(Pr111e);

                Pc111s.BackgroundColor = BaseColor.GRAY;
                Pc111s.HorizontalAlignment = 2;
                Pc111s.Border = Rectangle.TOP_BORDER;
                table1.AddCell(Pc111s);



                PdfPCell Pc1d11s = new PdfPCell();

                Pc1d11s.BackgroundColor = BaseColor.GRAY;
                Pc1d11s.HorizontalAlignment = 0;
                Pc1d11s.Border = Rectangle.RIGHT_BORDER;
                table1.AddCell(Pc1d11s);


            }
            catch { }
            string AmtInwords = words(Convert.ToInt32(tottalAmount));
            try
            {
                Paragraph Pr11 = new Paragraph();
                Pr11.Add(new Phrase("Amount In Words : " + AmtInwords, FontFactory.GetFont("Arial", 8, Font.BOLD)));
                PdfPCell Pc11 = new PdfPCell(Pr11);
                Pc11.Colspan = 8;

                Pc11.HorizontalAlignment = 0;
                table1.AddCell(Pc11);

                Paragraph Pr111 = new Paragraph();
                Pr111.Add(new Phrase("\nGST: As Applicable", FontFactory.GetFont("Arial", 8, Font.BOLD)));
                PdfPCell Pc111 = new PdfPCell(Pr111);
                Pc111.Colspan = 4;

                Pc111.HorizontalAlignment = 0;
                table1.AddCell(Pc111);
            }
            catch { }

            try
            {
                Paragraph Pr11 = new Paragraph();
                Pr11.Add(new Phrase("Terms & Conditions:\n ", FontFactory.GetFont("Arial", 8, Font.BOLD)));
                Pr11.Add(new Phrase("\n1) Subject to PUNE Jurisdiction ", FontFactory.GetFont("Arial", 8, Font.NORMAL)));
                Pr11.Add(new Phrase("\n2) Ensure proper mention of Batch No. Expiry Date & MRP in invoice. ", FontFactory.GetFont("Arial", 8, Font.NORMAL)));
                Pr11.Add(new Phrase("\n3) Kindly supply from single Batch of long expiry.", FontFactory.GetFont("Arial", 8, Font.NORMAL)));
                Pr11.Add(new Phrase("\n4) Kindly ensure that products are stored under the appropriate condition during transportation.", FontFactory.GetFont("Arial", 8, Font.NORMAL)));
                Pr11.Add(new Phrase("\n5) No changes in quantities/item permitted unless confirmed with authorized person(s)", FontFactory.GetFont("Arial", 8, Font.NORMAL)));
                Pr11.Add(new Phrase("\n6) Delivery will not be accepted on last day of month (due to physical stock verification).", FontFactory.GetFont("Arial", 8, Font.NORMAL)));
                Pr11.Add(new Phrase("\n7) This order cancels all pending orders of above manufacturer/division. ", FontFactory.GetFont("Arial", 8, Font.NORMAL)));
                Pr11.Add(new Phrase("\n8) Adjust all pending claim in this order.\n", FontFactory.GetFont("Arial", 8, Font.NORMAL)));
                Pr11.Add(new Phrase("\n                 Expecting co-operation for our success. Thanking you......", FontFactory.GetFont("Arial", 8, Font.NORMAL)));
                PdfPCell Pc11 = new PdfPCell(Pr11);
                Pc11.Colspan = 8;

                Pc11.HorizontalAlignment = 0;
                table1.AddCell(Pc11);

                Paragraph Pr111 = new Paragraph();
                Pr111.Add(new Phrase("For, Siddhivinayak Distributor\n\n\n\n\n\n\n\n\n\n\n\n", FontFactory.GetFont("Arial", 8, Font.BOLD)));
                Pr111.Add(new Phrase("Authorized Signatory", FontFactory.GetFont("Arial", 8, Font.BOLD)));
                PdfPCell Pc111 = new PdfPCell(Pr111);
                Pc111.Colspan = 4;

                Pc111.HorizontalAlignment = 0;
                table1.AddCell(Pc111);
            }
            catch { }

            document.Add(table1);
            document.Close();

            //string FileName = StikerPrintName;

            //string paths = Server.MapPath("~/InvoicePrint/");
            //path = paths + FileName;
            //string ReportURL = path;
            //byte[] FileBytes = System.IO.File.ReadAllBytes(ReportURL);
            //return File(FileBytes, "application/pdf", FileName);
            var result = new { Message = "success", FileName = filename1 };
            return Json(result, JsonRequestBehavior.AllowGet);
            //string FileName = Session["fileName1"].ToString();
            //byte[] FileBytes = System.IO.File.ReadAllBytes(FileName);
            //return File(FileBytes, "application/pdf");
          
        }
        //public JsonResult InvoicePrint(string PONO)
        //{
        //    try
        //    {
        //        int Count = 1;
        //        decimal TotAmt = 0;
        //        string SupplierName = "", OrderDate = "", WAddress = "", WCity = "", WPincode = "", WPhone = "", WEmail = "", WState = "", WCounry = "", WareHouseName = "", IsTax = "Is Applicable", TermsAndConditions = "";
        //        string BillingAddress = "", BillingCity = "", BillingPincode = "", BillingPhone = "", BillingState = "", BillingCountry = "";
        //        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["InventoryModel"].ToString());
        //        string query1 = "select PM.TermsAndConditions,P.ProductCode,P.HsnCode,PD.Price,PD.OrderQty,PD.NetAmount,isnull( PD.DiscountAmount,0) as DiscountAmount ,(PD.NetAmount-isnull(PD.DiscountAmount,0))as TotalAmount, P.ProductName, CONVERT(VARCHAR(10), PM.PurchaseOrderDate, 103)as OrderDate ,W.WareHouseName,W.Address as WAddress,W.City as WCity,W.Pincode as WPincode,W.Phone as WPhone,W.Email as WEmail,W.State as WState,W.Country as WCounry,S.SupplierName , S.BillingAddress,S.BillingCity,S.BillingPincode,S.BillingPhone,S.BillingState,S.BillingCountry from Podetails PD  inner join POMains PM on PM.PurchaseOrderNo = PD.PONO inner join Products P on P.ProductCode = PD.ProductCode inner join Suppliers S on S.SupplierID = PM.SupplierID inner join Warehouses W on W.WareHouseID=PM.WarehouseId where  PD.PONO='" + PONO + "'";
        //        conn.Open();
        //        SqlCommand cmd1 = new SqlCommand(query1, conn);
        //        DataTable dt = new DataTable();
        //        dt.Load(cmd1.ExecuteReader());
        //        conn.Close();
        //        int temp = 1;
        //        if (dt.Rows.Count == 0)
        //        {
        //            var result1 = new { Message = "Record not found", FileName = PONO + ".pdf" };
        //            return Json(result1, JsonRequestBehavior.AllowGet);
        //        }
        //        foreach (DataRow cdr in dt.Rows)
        //        {
        //            if (temp == 1)
        //            {
        //                OrderDate = cdr["OrderDate"].ToString();
        //                //Warehouse
        //                WareHouseName = cdr["WareHouseName"].ToString();
        //                WAddress = cdr["WAddress"].ToString();
        //                WCity = cdr["WCity"].ToString();
        //                WPincode = cdr["WPincode"].ToString();
        //                WPhone = cdr["WPhone"].ToString();
        //                WEmail = cdr["WEmail"].ToString();
        //                WState = cdr["WState"].ToString();
        //                WCounry = cdr["WCounry"].ToString();
        //                //Supplier
        //                SupplierName = cdr["SupplierName"].ToString();
        //                BillingAddress = cdr["BillingAddress"].ToString();
        //                BillingCity = cdr["BillingCity"].ToString();
        //                BillingPincode = cdr["BillingPincode"].ToString();
        //                BillingPhone = cdr["BillingPhone"].ToString();
        //                BillingState = cdr["BillingState"].ToString();
        //                BillingCountry = cdr["BillingCountry"].ToString();
        //                TermsAndConditions = cdr["TermsAndConditions"].ToString();
        //            }
        //            try
        //            {
        //                TotAmt = TotAmt + Convert.ToDecimal(cdr["TotalAmount"].ToString());
        //            }catch
        //            { }
        //            temp++;
        //        }

        //        iTextSharp.text.Font font5 = iTextSharp.text.FontFactory.GetFont(FontFactory.HELVETICA, 9);
        //        iTextSharp.text.Font font10 = iTextSharp.text.FontFactory.GetFont(FontFactory.HELVETICA, 11);

        //        DateTime dt1 = DateTime.Now;
        //        var aa = dt1.ToString("HH");
        //        var bb = dt1.ToString("mm");
        //        var cc = dt1.ToString("ss");

        //        Document document = new Document(PageSize.A4, 20f, 10f, 20f, 10f);
        //        string path = Server.MapPath("~/Reports/OrderMain/");
        //        string filename1 = path + "" + PONO + "_" + aa + bb + cc + ".pdf";
        //        PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filename1, FileMode.Create));
        //        document.Open();
        //        Session["fileName1"] = filename1;

        //        PdfPTable table = new PdfPTable(7);
        //        float[] width1 = new float[] { 5f, 10, 5f, 5f, 5f, 5f, 5f };
        //        table.SetWidths(width1);
        //        table.WidthPercentage = 98;
        //        table.HorizontalAlignment = 1;


        //        string imageURL = Server.MapPath("/Photo/OriginalLogo.png");
        //        iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(imageURL);
        //        jpg.ScaleToFit(100, 150);
        //        jpg.SpacingBefore = 5f;
        //        jpg.SpacingAfter = 1f;
        //        jpg.Alignment = Element.ALIGN_LEFT;


        //        PdfPCell Cell1 = new PdfPCell(jpg);
        //        Cell1.HorizontalAlignment = 0;
        //        Cell1.FixedHeight = 50f;
        //        Cell1.Colspan = 4;
        //        Cell1.Border = Rectangle.NO_BORDER;
        //        Cell1.FixedHeight = 60f;
        //        table.AddCell(Cell1);


        //        Paragraph Para2 = new Paragraph();
        //        Para2.Add(new Phrase("Purchase Order", FontFactory.GetFont("Arial", 15, Font.BOLD)));
        //        Para2.Add(new Phrase("\nPO No\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        Para2.Add(new Phrase("Delivery Date\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        Para2.Add(new Phrase("Date\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        PdfPCell Cell2 = new PdfPCell(Para2);
        //        Cell2.HorizontalAlignment = 0;
        //        Cell2.Colspan = 2;
        //        Cell2.Border = Rectangle.NO_BORDER;
        //        table.AddCell(Cell2);
        //        var date = DateTime.Now.ToString("dd/MM/yyyy").Replace('-', '/');

        //        Paragraph Para3 = new Paragraph();
        //        Para3.Add(new Phrase("\n\n : " + PONO + "\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        Para3.Add(new Phrase(" : " + OrderDate + "\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        Para3.Add(new Phrase(" : " + date + "\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        PdfPCell Cell3 = new PdfPCell(Para3);
        //        Cell3.HorizontalAlignment = 0;
        //        Cell3.Colspan = 2;
        //        Cell3.Border = Rectangle.NO_BORDER;
        //        table.AddCell(Cell3);

        //        // Second Row

        //        Paragraph Para4 = new Paragraph();
        //        Para4.Add(new Phrase("Vendor", FontFactory.GetFont("Arial", 12, Font.BOLD, BaseColor.WHITE)));
        //        PdfPCell Cell4 = new PdfPCell(Para4);
        //        Cell4.BackgroundColor = BaseColor.BLACK;
        //        Cell4.HorizontalAlignment = 0;
        //        Cell4.VerticalAlignment = 1;
        //        Cell4.Colspan = 2;
        //        Cell4.Border = Rectangle.NO_BORDER;
        //        table.AddCell(Cell4);

        //        PdfPCell Cell6 = new PdfPCell();
        //        Cell6.HorizontalAlignment = 0;
        //        Cell6.Border = Rectangle.NO_BORDER;
        //        Cell6.Colspan = 2;
        //        table.AddCell(Cell6);

        //        Paragraph Para5 = new Paragraph();
        //        Para5.Add(new Phrase("Shipped To", FontFactory.GetFont("Arial", 12, Font.BOLD, BaseColor.WHITE)));
        //        PdfPCell Cell5 = new PdfPCell(Para5);
        //        Cell5.HorizontalAlignment = 0;
        //        Cell5.VerticalAlignment = 1;
        //        Cell5.Colspan = 4;
        //        Cell5.BackgroundColor = BaseColor.BLACK;
        //        Cell5.Border = Rectangle.NO_BORDER;
        //        table.AddCell(Cell5);

        //        //Third Row

        //        Paragraph Para7 = new Paragraph();
        //        Para7.Add(new Phrase("" + SupplierName, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        Para7.Add(new Phrase("\n" + BillingAddress, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        Para7.Add(new Phrase("\n" + BillingCity, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        Para7.Add(new Phrase(" - " + BillingPincode, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        Para7.Add(new Phrase("\n" + BillingPhone + "\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        PdfPCell Cell7 = new PdfPCell(Para7);
        //        Cell7.HorizontalAlignment = 0;
        //        Cell7.Colspan = 2;
        //        Cell7.Border = Rectangle.NO_BORDER;
        //        table.AddCell(Cell7);

        //        PdfPCell Cell8 = new PdfPCell();
        //        Cell8.HorizontalAlignment = 0;
        //        Cell8.Border = Rectangle.NO_BORDER;
        //        Cell8.Colspan = 2;
        //        table.AddCell(Cell8);

        //        Paragraph Para9 = new Paragraph();
        //        Para9.Add(new Phrase("" + WareHouseName, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        Para9.Add(new Phrase("\n" + WAddress, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        Para9.Add(new Phrase("\n" + WCity, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        Para9.Add(new Phrase(" - " + WPincode, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        Para9.Add(new Phrase("\n" + WPhone + "\n\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        PdfPCell Cell9 = new PdfPCell(Para9);
        //        Cell9.HorizontalAlignment = 0;
        //        Cell9.Colspan = 4;
        //        Cell9.Border = Rectangle.NO_BORDER;
        //        table.AddCell(Cell9);

        //        //Fourth Row

        //        PdfPCell Cell10 = new PdfPCell(new Phrase(new Phrase("Code", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //        Cell10.HorizontalAlignment = 1;
        //        Cell10.BackgroundColor = BaseColor.BLACK;
        //        table.AddCell(Cell10);

        //        PdfPCell Cell11 = new PdfPCell(new Phrase(new Phrase("Product Name", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //        Cell11.HorizontalAlignment = 1;
        //        Cell11.BackgroundColor = BaseColor.BLACK;
        //        table.AddCell(Cell11);

        //        PdfPCell Cell12 = new PdfPCell(new Phrase(new Phrase("HSN Code", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //        Cell12.HorizontalAlignment = 1;
        //        Cell12.BackgroundColor = BaseColor.BLACK;
        //        table.AddCell(Cell12);

        //        PdfPCell Cell13 = new PdfPCell(new Phrase(new Phrase("Qty", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //        Cell13.HorizontalAlignment = 1;
        //        Cell13.BackgroundColor = BaseColor.BLACK;
        //        table.AddCell(Cell13);

        //        PdfPCell Cell14 = new PdfPCell(new Phrase(new Phrase("Price", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //        Cell14.HorizontalAlignment = 1;
        //        Cell14.BackgroundColor = BaseColor.BLACK;
        //        table.AddCell(Cell14);

        //        PdfPCell Cell15 = new PdfPCell(new Phrase(new Phrase("Discount", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //        Cell15.HorizontalAlignment = 1;
        //        Cell15.BackgroundColor = BaseColor.BLACK;
        //        table.AddCell(Cell15);

        //        PdfPCell Cell16 = new PdfPCell(new Phrase(new Phrase("Amount", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
        //        Cell16.HorizontalAlignment = 1;
        //        Cell16.BackgroundColor = BaseColor.BLACK;
        //        table.AddCell(Cell16);

        //        //Fifth Row


        //        foreach (DataRow r in dt.Rows)
        //        {
        //            PdfPCell Cell17 = new PdfPCell(new Phrase(new Phrase(r["ProductCode"].ToString(), FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //            Cell17.HorizontalAlignment = 0;
        //            table.AddCell(Cell17);

        //            PdfPCell Cell18 = new PdfPCell(new Phrase(new Phrase(r["ProductName"].ToString(), FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //            Cell18.HorizontalAlignment = 0;
        //            table.AddCell(Cell18);

        //            PdfPCell Cell19 = new PdfPCell(new Phrase(new Phrase(r["HsnCode"].ToString(), FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //            Cell19.HorizontalAlignment = 0;
        //            table.AddCell(Cell19);

        //            PdfPCell Cell20 = new PdfPCell(new Phrase(new Phrase(r["OrderQty"].ToString(), FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //            Cell20.HorizontalAlignment = 2;
        //            table.AddCell(Cell20);

        //            PdfPCell Cell21 = new PdfPCell(new Phrase(new Phrase(r["Price"].ToString(), FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //            Cell21.HorizontalAlignment = 2;
        //            table.AddCell(Cell21);

        //            PdfPCell Cell22 = new PdfPCell(new Phrase(new Phrase(r["DiscountAmount"].ToString(), FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //            Cell22.HorizontalAlignment = 2;
        //            table.AddCell(Cell22);

        //            PdfPCell Cell23 = new PdfPCell(new Phrase(new Phrase(r["TotalAmount"].ToString(), FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK))));
        //            Cell23.HorizontalAlignment = 2;
        //            Cell23.FixedHeight = 20f;
        //            table.AddCell(Cell23);
        //        }

        //        // Sixth Row

        //        PdfPCell Cell24002 = new PdfPCell();
        //        Cell24002.Border = Rectangle.LEFT_BORDER;
        //        Cell24002.HorizontalAlignment = 0;
        //        table.AddCell(Cell24002);

        //        Paragraph Para25001 = new Paragraph();
        //        Para25001.Add(new Phrase("Our GST No.\n", FontFactory.GetFont("Arial", 10, Font.BOLD)));
        //        Para25001.Add(new Phrase("Our PAN No.\n", FontFactory.GetFont("Arial", 10, Font.BOLD)));
        //        PdfPCell Cell24 = new PdfPCell(Para25001);
        //        Cell24.Border = Rectangle.NO_BORDER;
        //        Cell24.HorizontalAlignment = 1;
        //        table.AddCell(Cell24);

        //        Paragraph Para250010 = new Paragraph();
        //        Para250010.Add(new Phrase("22AAAAA0000A1Z1\n", FontFactory.GetFont("Arial", 10, Font.BOLD)));
        //        Para250010.Add(new Phrase("BNZPG4398C\n", FontFactory.GetFont("Arial", 10, Font.BOLD)));
        //        PdfPCell Cell24001 = new PdfPCell(Para250010);
        //        Cell24001.Colspan = 3;
        //        Cell24001.Border = Rectangle.NO_BORDER;
        //        Cell24001.HorizontalAlignment = 0;
        //        table.AddCell(Cell24001);


        //        Paragraph Para25 = new Paragraph();
        //        Para25.Add(new Phrase("Total\n\n", FontFactory.GetFont("Arial", 10, Font.BOLD)));
        //        Para25.Add(new Phrase("Taxes\n", FontFactory.GetFont("Arial", 10, Font.NORMAL)));
        //        PdfPCell Cell25 = new PdfPCell(Para25);
        //        Cell25.HorizontalAlignment = 0;
        //        Cell25.Border = Rectangle.NO_BORDER;
        //        table.AddCell(Cell25);

        //        Paragraph Para26 = new Paragraph();
        //        Para26.Add(new Phrase("" + String.Format("{0:0.00}", Math.Round(TotAmt).ToString()), FontFactory.GetFont("Arial", 9, Font.BOLD)));
        //        Para26.Add(new Phrase("\n\n" + IsTax + ".00", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        PdfPCell Cell26 = new PdfPCell(Para26);
        //        Cell26.HorizontalAlignment = 2;
        //        Cell26.Border = Rectangle.RIGHT_BORDER;
        //        table.AddCell(Cell26);
        //        string AmtInwords = words(Convert.ToInt32(TotAmt));
        //        Paragraph Para27 = new Paragraph();
        //        Para27.Add(new Phrase(" Amount In Word  : " + AmtInwords, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        PdfPCell Cell27 = new PdfPCell(Para27);
        //        Cell27.HorizontalAlignment = 0;
        //        Cell27.Colspan = 7;
        //        Cell27.FixedHeight = 20f;
        //        table.AddCell(Cell27);

        //        //Eight Row


        //        Paragraph Para28 = new Paragraph();
        //        Para28.Add(new Phrase("Note And Instuction :  " + TermsAndConditions, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
        //        PdfPCell Cell28 = new PdfPCell(Para28);
        //        Cell28.HorizontalAlignment = 0;
        //        //  Cell27.Border = Rectangle.NO_BORDER;
        //        Cell28.Colspan = 7;
        //        Cell28.FixedHeight = 60f;
        //        table.AddCell(Cell28);


        //        document.Add(table);
        //        document.Close();
        //        var result = new { Message = "success", FileName = filename1 };
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception EX)
        //    {
        //        var result = new { Message = EX.Message };
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }

        //    return Json(null, JsonRequestBehavior.AllowGet);
        //}
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
    }
}
