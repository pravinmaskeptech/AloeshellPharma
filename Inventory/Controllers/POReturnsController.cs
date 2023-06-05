using Inventory.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class POReturnsController : Controller
    {
        InventoryModel db = new InventoryModel();
        // GET: POReturns
        public ActionResult Create()
        {
            var setting = db.Settings.Where(x => x.FieldName == "BatchNo" && x.Setting == true).FirstOrDefault();
            if (setting != null)
                ViewBag.BatchNoSetting = setting.FieldName;
            else
                ViewBag.BatchNoSetting = "";
            ViewBag.Warehousedatasource = db.Warehouses.ToList();
            ViewBag.PONOdatasource = db.GRNDetail.Where(a => a.ReceivedQty > 0).Select(x => x.GRNNo).Distinct().ToList();
            return View();
        }
        public ActionResult POReturn()
        {
            var setting = db.Settings.Where(x => x.FieldName == "BatchNo" && x.Setting == true).FirstOrDefault();
            if (setting != null)
                ViewBag.BatchNoSetting = setting.FieldName;
            else
                ViewBag.BatchNoSetting = "";
            ViewBag.Productdatasource = db.Products.Where(a => a.IsActive == true && a.SerialNoApplicable == true).ToList();

            ViewBag.GRNNOdatasource = db.GRNDetail.Where(a => a.BatchNo == null && a.SerialNoApplicable == false && a.SalesQty == 0).Select(a => a.GRNNo).Distinct().ToList();
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
        public ActionResult Index()
        {
            var setting = db.Settings.Where(x => x.FieldName == "BatchNo" && x.Setting == true).FirstOrDefault();
            if (setting != null)
                ViewBag.BatchNoSetting = setting.FieldName;
            else
                ViewBag.BatchNoSetting = "";

            var POReturnsMaster = db.pOReturns.ToList();
            ViewBag.datasource = (from c in POReturnsMaster
                                  join sply in db.suppliers on c.SupplierID equals sply.SupplierID into Suppliers
                                  from aa in Suppliers.DefaultIfEmpty()
                                  group c by new
                                  {
                                      c.GrnNo,
                                      aa.SupplierName
                                  } into gcs
                                  select new
                                  {
                                      GrnNo = gcs.Key.GrnNo,
                                      SupplierName = gcs.Key.SupplierName,
                                      ReturnQty = gcs.Sum(a => a.ReturnQty),
                                      ReturnBy = gcs.Any(a => a.SerialNumber != null) ? "Serial Number" : "Batch Number"
                                  }).ToList();
            return View();
        }

        public JsonResult ShowPOdetails(string GRNNO)
        {
            if (GRNNO == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                try
                {
                    var Productsmaster = new List<Products>(db.Products);
                    var Storesmaster = new List<StoreLocations>(db.StoreLocations);
                    var Warehousemaster = new List<Warehouse>(db.Warehouses);
                    var GRNDetailmaster = new List<GRNDetails>(db.GRNDetail);
                    var POMainmaster = new List<POMain>(db.POMains);
                    var PODetailsmasters = new List<PODetails>(db.poDetails);
                    var Suppliersmasters = new List<Suppliers>(db.suppliers);


                    var result = (from grndtl in GRNDetailmaster.Where(a => a.GRNNo == GRNNO && a.SerialNoApplicable != true)

                                  join podtl in PODetailsmasters on grndtl.PurchaseOrderDetailsID equals podtl.PurchaseOrderDetailsID into POdetails
                                  from podetails in POdetails.DefaultIfEmpty()

                                  join Suppliers in Suppliersmasters on grndtl.SupplierID equals Suppliers.SupplierID into Supplier
                                  from sply in Supplier.DefaultIfEmpty()

                                  join podtl in POMainmaster on podetails.PONO equals podtl.PurchaseOrderNo into POmains
                                  from poMain in POmains.DefaultIfEmpty()

                                  join Product in Productsmaster on grndtl.ProductCode equals Product.ProductCode into products
                                  from prd in products.DefaultIfEmpty()

                                  join Warehouse in Warehousemaster on grndtl.WarehouseID equals Warehouse.WareHouseID into warehouse
                                  from Whouse in warehouse.DefaultIfEmpty()

                                  join StoreLocation in Storesmaster on grndtl.StoreLocationId equals StoreLocation.StoreLocationId into storeLoc
                                  from store in storeLoc.DefaultIfEmpty()

                                  orderby grndtl.GRNId descending
                                  select new { GRNId = grndtl.GRNId, GRNNo = grndtl.GRNNo, GRNDate = grndtl.GRNDate, BatchNo = grndtl.BatchNo, ReceivedQty = grndtl.ReceivedQty, ManufacturingDate = grndtl.ManufacturingDate, ExpiryDate = grndtl.ExpiryDate, ProductCode = prd.ProductCode, ProductName = prd == null ? string.Empty : prd.ProductName, WarehouseID = grndtl.WarehouseID, WareHouseName = Whouse == null ? string.Empty : Whouse.WareHouseName, StoreLocation = store == null ? string.Empty : store.StoreLocation, StoreLocationId = grndtl.StoreLocationId, OrderQty = podetails.OrderQty, Price = podetails.Price, GSTPercentage = podetails.GSTPercentage, Discount = podetails.Discount, DiscountAs = podetails.DiscountAs, SupplierID = poMain.SupplierID, PurchaseOrderDetailsID = grndtl.PurchaseOrderDetailsID, ReturnQty = grndtl.ReturnQty, SupplierName = sply == null ? string.Empty : sply.SupplierName, SalesQty = grndtl.SalesQty, DamageQty = grndtl.DamageQty, ReplaceQty = grndtl.ReplaceQty }
                                         ).Distinct().ToList();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ee)
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
        }
        public JsonResult save(List<POReturns> Poreturns)
        {
            var status = false;
            int cnt = 1;
            try
            {
                if (Poreturns != null)
                {
                    if (Poreturns.Count > 0)
                    {
                        foreach (var x in Poreturns)
                        {
                            var prd = db.Products.Where(a => a.ProductCode == x.ProductCode).FirstOrDefault();
                            prd.ClosingQuantity = (Convert.ToDecimal(prd.ClosingQuantity) - Convert.ToDecimal(x.ReturnQty));
                            prd.InwardQuantity = (Convert.ToDecimal(prd.InwardQuantity) - Convert.ToDecimal(x.ReturnQty));
                            if (x.Status == "Replace")
                            {
                                var grn = db.GRNDetail.Where(a => a.GRNId == x.GRNId).FirstOrDefault();
                                grn.ReplaceQty = Convert.ToDecimal(grn.ReplaceQty) + Convert.ToDecimal(x.ReturnQty);
                                grn.ReceivedQty = Convert.ToDecimal(grn.ReceivedQty) - Convert.ToDecimal(x.ReturnQty);
                                grn.ReplaceReason = x.Status;
                                db.GRNDetail.AddOrUpdate(grn);
                            
                            }
                            else
                            {
                                var grn = db.GRNDetail.Where(a => a.GRNId == x.GRNId).FirstOrDefault();
                                grn.ReturnQty = Convert.ToDecimal(grn.ReturnQty) + Convert.ToDecimal(x.ReturnQty);
                                grn.ReceivedQty = Convert.ToDecimal(grn.ReceivedQty) - Convert.ToDecimal(x.ReturnQty);
                                grn.ReturnReason = x.Status;
                                db.GRNDetail.AddOrUpdate(grn);
                             
                            }

                            var order = db.poDetails.Where(a => a.PurchaseOrderDetailsID == x.PurchaseOrderDetailsID).FirstOrDefault();
                            order.ReturnQty = Convert.ToDecimal(order.ReturnQty) + Convert.ToDecimal(x.ReturnQty);
                            order.ReturnReason = x.ReturnReason;
                            //order.ReceivedQty = Convert.ToDecimal(order.ReceivedQty) - Convert.ToDecimal(x.ReturnQty);

                            POReturns poRtn = new POReturns();
                            poRtn.ReturnDate = x.ReturnDate;
                            poRtn.GrnNo = x.GrnNo;
                            poRtn.ProductCode = x.ProductCode;
                            poRtn.ReturnQty = x.ReturnQty;
                            poRtn.WarehouseID = x.WarehouseID;
                            poRtn.StoreLocationId = x.StoreLocationId;
                            poRtn.BatchNo = x.BatchNo;
                            poRtn.ReturnQty = x.ReturnQty;
                            poRtn.POReturnNo = x.POReturnNo;
                            poRtn.Status = x.Status;
                            poRtn.CompanyID = Convert.ToInt32(Session["CompanyID"]);
                            poRtn.ReturnReason = x.ReturnReason;
                            poRtn.SupplierID = x.SupplierID;
                            poRtn.CreatedBy = User.Identity.Name;
                            poRtn.CreatedDate = DateTime.Today;
                            db.pOReturns.Add(poRtn);
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
        public JsonResult getReturnData(string SerialNo, string ProductId)
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
                var POMainmasters = new List<POMain>(db.POMains);
                var ProductSerialNoMaster = new List<ProductSerialNo>(db.ProductSerialNo);

                var result = (from serialno in ProductSerialNoMaster.Where(a => a.SerialNo == SerialNo && a.ProductCode == ProductId && a.Status == "inward")

                              join Product in Productsmaster on serialno.ProductCode equals Product.ProductCode into products
                              from prd in products.DefaultIfEmpty()

                              join Warehouse in Warehousemaster on serialno.WarehouseId equals Warehouse.WareHouseID into warehouse
                              from Whouse in warehouse.DefaultIfEmpty()

                              join StoreLocation in Storesmaster on serialno.StoreLocationId equals StoreLocation.StoreLocationId into storeLoc
                              from store in storeLoc.DefaultIfEmpty()

                              join poMain in POMainmasters on serialno.PONO equals poMain.PurchaseOrderNo into POMAINS
                              from po in POMAINS.DefaultIfEmpty()

                              orderby serialno.SerialNoId descending
                              select new { SerialNoId = serialno.SerialNoId, PODetailsId = serialno.PODetailsId, ProductCode = serialno.ProductCode, BatchNo = serialno.BatchNo, WarehouseId = serialno.WarehouseId, StoreLocationId = serialno.StoreLocationId, GrnNo = serialno.GrnNo, ProductName = prd == null ? string.Empty : prd.ProductName, WareHouseName = Whouse == null ? string.Empty : Whouse.WareHouseName, StoreLocation = store == null ? string.Empty : store.StoreLocation, PONO = serialno.PONO, SerialNo = serialno.SerialNo, Status = serialno.Status, GrnDate = serialno.GrnDate, SupplierID = po.SupplierID }
                                     ).Distinct().ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult SaveReturnSerialNo(List<POReturns> ReturnOrder)
        {
            var status = false;
            int cnt = 1;
            try
            {
                if (ReturnOrder != null)
                {
                    if (ReturnOrder.Count > 0)
                    {
                        foreach (var x in ReturnOrder)
                        {

                            var prd = db.Products.Where(a => a.ProductCode == x.ProductCode).FirstOrDefault();
                            prd.ClosingQuantity = (Convert.ToDecimal(prd.ClosingQuantity) - Convert.ToDecimal(x.ReturnQty));
                            prd.InwardQuantity = (Convert.ToDecimal(prd.InwardQuantity) - Convert.ToDecimal(x.ReturnQty));

                            var SerialNo = db.ProductSerialNo.Where(a => a.SerialNoId == x.SerialNoId && a.ProductCode == x.ProductCode).FirstOrDefault();
                            if (x.Status == "Replace")
                            {
                                SerialNo.Status = "PO Replace";
                            }
                            else
                            {
                                SerialNo.Status = "PO Returned";
                            }

                            var order = db.poDetails.Where(a => a.PurchaseOrderDetailsID == x.PurchaseOrderDetailsID).FirstOrDefault();
                            order.ReturnQty = Convert.ToDecimal(order.ReturnQty) + Convert.ToDecimal(x.ReturnQty);
                            order.ReturnReason = x.ReturnReason;
                            //order.ReceivedQty = Convert.ToDecimal(order.ReceivedQty) - Convert.ToDecimal(x.ReturnQty);

                            var grn = db.GRNDetail.Where(a => a.GRNNo == x.GrnNo && a.WarehouseID == x.WarehouseID && a.StoreLocationId == x.StoreLocationId && a.ProductCode == x.ProductCode).FirstOrDefault();
                            if (x.Status == "Replace")
                            {
                                grn.ReplaceQty = Convert.ToDecimal(grn.ReplaceQty) + Convert.ToDecimal(x.ReturnQty);
                                grn.ReplaceReason = x.ReturnReason;
                                //db.GRNDetail.AddOrUpdate(grn);
                            }
                            else
                            {
                                grn.ReturnQty = Convert.ToDecimal(grn.ReturnQty) + Convert.ToDecimal(x.ReturnQty);
                                grn.ReturnReason = x.ReturnReason;
                                //db.GRNDetail.AddOrUpdate(grn);
                            }
                            grn.ReceivedQty = Convert.ToDecimal(grn.ReceivedQty) - Convert.ToDecimal(x.ReturnQty);

                            POReturns poRtn = new POReturns();
                            poRtn.ReturnDate = x.ReturnDate;
                            poRtn.GrnNo = x.GrnNo;
                            poRtn.ProductCode = x.ProductCode;
                            poRtn.ReturnQty = 1;
                            poRtn.POReturnNo = x.POReturnNo;
                            poRtn.Status = x.Status;
                            poRtn.WarehouseID = x.WarehouseID;
                            poRtn.StoreLocationId = x.StoreLocationId;
                            poRtn.BatchNo = x.BatchNo;
                            poRtn.ReturnReason = x.ReturnReason;
                            poRtn.CreatedBy = User.Identity.Name;
                            poRtn.CreatedDate = DateTime.Today;
                            poRtn.SerialNumber = x.SerialNo;
                            poRtn.SupplierID = x.SupplierID;
                            poRtn.CompanyID = Convert.ToInt32(Session["CompanyID"]);

                            db.pOReturns.Add(poRtn);
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
        public JsonResult GetAllPODetails(string GRNNo)
        {
            if (GRNNo == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var Productsmaster = new List<Products>(db.Products);
                var Storesmaster = new List<StoreLocations>(db.StoreLocations);
                var Warehousemaster = new List<Warehouse>(db.Warehouses);
                var POReturnsMaster = new List<POReturns>(db.pOReturns);
                var result = (from returns in POReturnsMaster.Where(a => a.GrnNo == GRNNo)

                              join Product in Productsmaster on returns.ProductCode equals Product.ProductCode into products
                              from prd in products.DefaultIfEmpty()

                              join Warehouse in Warehousemaster on returns.WarehouseID equals Warehouse.WareHouseID into warehouse
                              from Whouse in warehouse.DefaultIfEmpty()

                              join StoreLocation in Storesmaster on returns.StoreLocationId equals StoreLocation.StoreLocationId into storeLoc
                              from store in storeLoc.DefaultIfEmpty()

                              orderby returns.POreturnId descending
                              select new { POreturnId = returns.POreturnId, SerialNumber = returns.SerialNumber, GrnNo = returns.GrnNo, ReturnDate = returns.ReturnDate, BatchNo = returns.BatchNo, ReturnQty = returns.ReturnQty, ReturnReason = returns.ReturnReason, ProductCode = prd == null ? string.Empty : prd.ProductName, Warehouse = Whouse == null ? string.Empty : Whouse.WareHouseName, StoreLocation = store == null ? string.Empty : store.StoreLocation, Status = returns.Status }
                                    ).ToList();

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult getPORturnNo()
        {
            var result = db.pOReturns.Count();
            result = result + 1;
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult ShowGRNdetails(string GRNNO)
        {
            if (GRNNO == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                //int poid = db.POMains.Where(a => a.PurchaseOrderNo == PONO).Select(a => a.PurchaseOrderID).Single();
                //var result = db.poDetails.Where(a => a.PurchaseOrderID == poid && a.OrderQty != a.ReceivedQty).ToList();
                var poid = db.GRNDetail.Where(a => a.GRNNo == GRNNO).FirstOrDefault();

                var Productmaster = new List<Products>(db.Products);
                var Warehousemaster = new List<Warehouse>(db.Warehouses);
                var Suppliermaster = new List<Suppliers>(db.suppliers);
                var GRNDetailsMaster = new List<GRNDetails>(db.GRNDetail);
                var Storesmaster = new List<StoreLocations>(db.StoreLocations);
                var PODetailsmasters = new List<PODetails>(db.poDetails);
                var result = (from grndtl in GRNDetailsMaster.Where(a => a.GRNNo == GRNNO && a.SerialNoApplicable == false)

                              join Product in Productmaster on grndtl.ProductCode equals Product.ProductCode into product
                              from prd in product.DefaultIfEmpty()


                              join warehouse in Warehousemaster on poid.WarehouseID equals warehouse.WareHouseID into Warehousee
                              from whouse in Warehousee.DefaultIfEmpty()

                              join Supplier in Suppliermaster on poid.SupplierID equals Supplier.SupplierID into supplier
                              from sply in supplier.DefaultIfEmpty()

                              join StoreLocation in Storesmaster on grndtl.StoreLocationId equals StoreLocation.StoreLocationId into storeLoc
                              from store in storeLoc.DefaultIfEmpty()

                              join podtl in PODetailsmasters on grndtl.PurchaseOrderDetailsID equals podtl.PurchaseOrderDetailsID into POdetails
                              from podetails in POdetails.Where(a => a.ProductCode == grndtl.ProductCode).DefaultIfEmpty()

                              orderby grndtl.PurchaseOrderDetailsID descending
                              select new { GRNId = grndtl.GRNId, GRNNo = grndtl.GRNNo, GRNDate = grndtl.GRNDate, BatchNo = grndtl.BatchNo, ReceivedQty = grndtl.ReceivedQty, ManufacturingDate = grndtl.ManufacturingDate, ExpiryDate = grndtl.ExpiryDate, ProductCode = prd.ProductCode, ProductName = prd == null ? string.Empty : prd.ProductName, WarehouseID = grndtl.WarehouseID, WareHouseName = whouse == null ? string.Empty : whouse.WareHouseName, StoreLocation = store == null ? string.Empty : store.StoreLocation, StoreLocationId = grndtl.StoreLocationId, OrderQty = podetails.OrderQty, Price = grndtl.BasicRate, GSTPercentage = grndtl.Tax, Discount = grndtl.Discount, DiscountAs = grndtl.DiscountAs, SupplierID = grndtl.SupplierID, PurchaseOrderDetailsID = grndtl.PurchaseOrderDetailsID, ReturnQty = grndtl.ReturnQty, SupplierName = sply == null ? string.Empty : sply.SupplierName, SalesQty = grndtl.SalesQty, DamageQty = grndtl.DamageQty, ReplaceQty = grndtl.ReplaceQty }).ToList();

                return Json(result, JsonRequestBehavior.AllowGet);


            }
        }
        public JsonResult getBillNo()
        {
            var count = db.pOReturns.Count();
            count = count + 1;
            var result = count;
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
