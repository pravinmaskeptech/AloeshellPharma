using Inventory.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class GRNDetailsController : Controller
    {
        InventoryModel db = new InventoryModel();
        // GET: GRNDetails
        public ActionResult Create()
        {
            //db.TempSerialNo.RemoveRange(db.TempSerialNo);
            //db.SaveChanges();
            ViewBag.Warehousedatasource = db.Warehouses.ToList();
            var result = db.poDetails.Where(a => a.OrderQty != a.ReceivedQty).Select(x => x.PurchaseOrderID).ToList();
            ViewBag.PONOdatasource = db.POMains.Where(a => result.Contains(a.PurchaseOrderID)).ToList();

            var setting = db.Settings.Where(x => x.FieldName == "BatchNo" && x.Setting == true).FirstOrDefault();
            if (setting != null)
                ViewBag.BatchNoSetting = setting.FieldName;
            else
                ViewBag.BatchNoSetting = "";

            setting = db.Settings.Where(x => x.FieldName == "ManufactureDate" && x.Setting == true).FirstOrDefault();
            if (setting != null)
                ViewBag.ManufactureDate = setting.FieldName;
            else
                ViewBag.ManufactureDate = "";

            setting = db.Settings.Where(x => x.FieldName == "ExpiryDate" && x.Setting == true).FirstOrDefault();
            if (setting != null)
                ViewBag.ExpiryDate = setting.FieldName;
            else
                ViewBag.ExpiryDate = "";

            return View();
        }
        public ActionResult ManufactureGRN()
        {


            //var Prod = (from stock in db.IssueToProductionDetails.Where(x => x.IsManufactureGRN != true)

            //            orderby stock.ID descending
            //            select new { MRNNO = stock.MRNNO } into p
            //            group p by new { p.MRNNO } into g
            //            select new
            //            {
            //                MRNNO = g.Select(x => x.MRNNO).FirstOrDefault(),
            //            }).ToList();


            var data = (from stock in db.IssueToProductionDetails.Where(x => x.ManufactureGRNQty != x.IssueToProductionQty)

                        orderby stock.ID descending
                        select new { MRNNO = stock.MRNNO } into p
                        group p by new { p.MRNNO } into g
                        select new
                        {
                            MRNNO = g.Select(x => x.MRNNO).FirstOrDefault(),
                        }).ToList();

            var data1 = data.Select(a => a.MRNNO).ToList();
            ViewBag.MRNNO = db.MRNMain.Where(a => data1.Contains(a.MRNNo)).ToList();


            // ViewBag.MRNNO = db.IssueToProductionDetails.Where(a=>a.MRNNO=="jkfsdjkhhj").ToList();
            ViewBag.Warehousedatasource = db.Warehouses.ToList();
            ViewBag.vBatchNo = db.Settings.Where(a => a.FieldName == "BatchNo").Select(a => a.Setting).Single().ToString();
            ViewBag.vSerialNo = db.Settings.Where(a => a.FieldName == "SerialNo").Select(a => a.Setting).Single().ToString();
            ViewBag.vExpiryDate = db.Settings.Where(a => a.FieldName == "ExpiryDate").Select(a => a.Setting).Single().ToString();
            ViewBag.vManufactureDate = db.Settings.Where(a => a.FieldName == "ManufactureDate").Select(a => a.Setting).Single().ToString();
            return View();
        }

        //public JsonResult GetAllGRNDetails()
        //{



        //    return Json(result, JsonRequestBehavior.AllowGet);

        //}
        public ActionResult Index()
        {
            var setting = db.Settings.Where(x => x.FieldName == "BatchNo" && x.Setting == true).FirstOrDefault();
            if (setting != null)
                ViewBag.BatchNoSetting = setting.FieldName;
            else
                ViewBag.BatchNoSetting = "";

            setting = db.Settings.Where(x => x.FieldName == "ManufactureDate" && x.Setting == true).FirstOrDefault();
            if (setting != null)
                ViewBag.ManufactureDate = setting.FieldName;
            else
                ViewBag.ManufactureDate = "";

            setting = db.Settings.Where(x => x.FieldName == "ExpiryDate" && x.Setting == true).FirstOrDefault();
            if (setting != null)
                ViewBag.ExpiryDate = setting.FieldName;
            else
                ViewBag.ExpiryDate = "";


            var result = (from c in db.GRNDetail
                          orderby (c.GRNId)
                          join sply in db.suppliers on c.SupplierID equals sply.SupplierID into supplierr
                          from aa in supplierr.DefaultIfEmpty()
                          group c by new
                          {
                              c.GRNNo,
                              c.GRNDate,
                              c.PONo,
                              aa.SupplierName
                          } into gcs

                          select new
                          {
                              GRNId = gcs.Max(a => a.GRNId),
                              GRNNo = gcs.Key.GRNNo,
                              GRNDate = gcs.Key.GRNDate,
                              SupplierName = gcs.Key.SupplierName,
                              PONo = gcs.Key.PONo,
                              ReceivedQty = gcs.Sum(a => a.ReceivedQty),
                              //order = gcs.ToList(),
                          }).ToList();

            ViewBag.datasource = result.OrderByDescending(a => a.GRNId).ToList();
            return View();
        }
        public JsonResult GetAllGRNDetails(string GRNNO)
        {
            if (GRNNO == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                //int id = db.GRNDetail.Where(a => a.GRNNo == GRNNO).Select(a => a.OrderID).Single();

                var Productsmaster = new List<Products>(db.Products);
                var Storesmaster = new List<StoreLocations>(db.StoreLocations);
                var Warehousemaster = new List<Warehouse>(db.Warehouses);
                var GRNDetailsMaster = new List<GRNDetails>(db.GRNDetail);
                var result = (from grn in GRNDetailsMaster.Where(a => a.GRNNo == GRNNO)

                              join Product in Productsmaster on grn.ProductCode equals Product.ProductCode into products
                              from prd in products.DefaultIfEmpty()

                              join Warehouse in Warehousemaster on grn.WarehouseID equals Warehouse.WareHouseID into warehouse
                              from Whouse in warehouse.DefaultIfEmpty()

                              join StoreLocation in Storesmaster on grn.StoreLocationId equals StoreLocation.StoreLocationId into storeLoc
                              from store in storeLoc.DefaultIfEmpty()

                              orderby grn.GRNId ascending
                              select new
                              {
                                  GRNId = grn.GRNId,
                                  GRNDate = grn.GRNDate,

                                  GRNNo = grn == null ? string.Empty : grn.GRNNo,

                                  PONo = grn == null ? string.Empty : grn.PONo,
                                  BatchNo = grn == null ? string.Empty : grn.BatchNo,
                                  ReceivedQty = grn == null ? 0 : grn.ReceivedQty,
                                  ManufacturingDate = grn.ManufacturingDate,
                                  ExpiryDate = grn.ExpiryDate,
                                  ProductCode = prd == null ? string.Empty : prd.ProductName,
                                  WarehouseID = Whouse == null ? string.Empty : Whouse.WareHouseName,
                                  StoreLocationId = store == null ? string.Empty : store.StoreLocation,
                                  ReturnReason = grn == null ? string.Empty : grn.ReturnReason,
                                  ReplaceQty = grn == null ? 0 : grn.ReplaceQty,
                                  DamageQty = grn == null ? 0 : grn.DamageQty,
                                  ReturnQty = grn == null ? 0 : grn.ReturnQty,
                                  SerialFrom = grn == null ? string.Empty : grn.SerialFrom,
                                  SerialTo = grn == null ? string.Empty : grn.SerialTo,
                                  BasicRate = grn.BasicRate,
                                  Tax = grn.Tax,
                                  Discount = grn.Discount,
                              }
                              //select new { GRNId = grn.GRNId, GRNNo = grn.GRNNo, GRNDate = grn.GRNDate, PONo = grn.PONo, BatchNo = grn.BatchNo, ReceivedQty = grn.ReceivedQty, ManufacturingDate = grn.ManufacturingDate, ExpiryDate = grn.ExpiryDate, ProductCode = prd == null ? string.Empty : prd.ProductName, WarehouseID = Whouse == null ? string.Empty : Whouse.WareHouseName, StoreLocationId = store == null ? string.Empty : store.StoreLocation, ReturnQty = grn.ReturnQty, ReplaceQty = grn.ReplaceQty, DamageQty = grn.DamageQty }
                                     ).ToList();

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult ShowPOdetails(string PONO)
        {
            if (PONO == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                //int poid = db.POMains.Where(a => a.PurchaseOrderNo == PONO).Select(a => a.PurchaseOrderID).Single();
                //var result = db.poDetails.Where(a => a.PurchaseOrderID == poid && a.OrderQty != a.ReceivedQty).ToList();
                try
                {
                    var poid = db.POMains.Where(a => a.PurchaseOrderNo == PONO).FirstOrDefault();

                    var Productmaster = new List<Products>(db.Products);
                    var Warehousemaster = new List<Warehouse>(db.Warehouses);
                    var Suppliermaster = new List<Suppliers>(db.suppliers);
                    var PODetailsMaster = new List<PODetails>(db.poDetails);
                    var result = (from Order in PODetailsMaster.Where(a => a.PONO == PONO)

                                  join Product in Productmaster on Order.ProductCode equals Product.ProductCode into product
                                  from prd in product.DefaultIfEmpty()

                                  join warehouse in Warehousemaster on poid.WarehouseId equals warehouse.WareHouseID into Warehousee
                                  from whouse in Warehousee.DefaultIfEmpty()

                                  join Supplier in Suppliermaster on poid.SupplierID equals Supplier.SupplierID into supplier
                                  from sply in supplier.DefaultIfEmpty()

                                  orderby Order.PurchaseOrderDetailsID descending
                                  select new
                                  {
                                      PurchaseOrderDetailsID = Order.PurchaseOrderDetailsID,
                                      PurchaseOrderID = Order.PurchaseOrderID,
                                      PONO = Order == null ? string.Empty : Order.PONO,
                                      ProductCode = Order == null ? string.Empty : Order.ProductCode,
                                      HSNCode = Order == null ? string.Empty : Order.HSNCode,
                                      GSTPercentage = Order == null ? 0 : Order.GSTPercentage,
                                      Price = Order == null ? 0 : Order.Price,
                                      OrderQty = Order == null ? 0 : Order.OrderQty,
                                      NetAmount = Order == null ? 0 : Order.NetAmount,
                                      CGSTAmount = Order == null ? 0 : Order.CGSTAmount,
                                      SGSTAmount = Order == null ? 0 : Order.SGSTAmount,
                                      IGSTAmount = Order == null ? 0 : Order.IGSTAmount,

                                      Discount = Order == null ? 0 : Order.Discount,
                                      DiscountAmount = Order == null ? 0 : Order.DiscountAmount,
                                      TotalAmount = Order == null ? 0 : Order.TotalAmount,
                                      ReceivedQty = Order == null ? 0 : Order.ReceivedQty,
                                      DiscountAs = Order == null ? string.Empty : Order.DiscountAs,


                                      ReturnQty = Order == null ? 0 : Order.ReturnQty,
                                      ReturnReason = Order == null ? string.Empty : Order.ReturnReason,
                                      VariantName = prd == null ? string.Empty : prd.ProductName,

                                      WareHouseName = whouse == null ? string.Empty : whouse.WareHouseName,
                                      WareHouseID = poid.WarehouseId,
                                      SerialNoApplicable = prd.SerialNoApplicable,
                                      SupplierName = sply == null ? string.Empty : sply.SupplierName,
                                      SupplierID = poid.SupplierID,
                                      PurchaseOrderDate = poid.PurchaseOrderDate,
                                      Prefix = prd.Prefix,
                                      CurrentSerialNo = prd.CurrentSerialNo,
                                  }).ToList();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                catch (Exception Ex)
                {
                    var result = Ex.InnerException.InnerException.Message;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

            }
        }
        public JsonResult checkValidSerialNo(string SerialNo, string ProdCode)
        {
            var result = db.ProductSerialNo.Where(a => a.ProductCode == ProdCode && a.SerialNo == SerialNo).Count();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult getBillNo()
        {
            var count = db.BillNumbering.Where(a => a.Type == "NewGRN").Select(a => a.Number).Single();
            var result = "GRNNo_" + count;
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult getPONO()
        {
            var result1 = db.poDetails.Where(a => a.OrderQty != a.ReceivedQty).Select(x => x.PurchaseOrderID).ToList();
            var result = db.POMains.Where(a => result1.Contains(a.PurchaseOrderID)).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult getStoreLocation(string WarehouseID)
        {
            var result = db.StoreLocations.Where(a => a.WarehouseId == WarehouseID && a.IsActive == true).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult getWarehouse()
        {
            var result = db.Warehouses.ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DownloadAttachment()
        {

            var fileNM = Session["FileNM"].ToString(); ;
            Session["FileNM"] = "";
            string FilePath = Server.MapPath("~/Photo/NotepadFiles/" + fileNM + "");
            byte[] fileBytes = System.IO.File.ReadAllBytes(FilePath);
            System.IO.File.Delete(FilePath);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileNM + ".txt");
        }

        public JsonResult save(List<GRNDetails> OrderDetails, string strGRNDate)
        {


            DateTime DTFrom = DateTime.ParseExact(strGRNDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var message = "";
            var billNo = db.BillNumbering.Where(a => a.Type == "NewGRN").FirstOrDefault();
            billNo.Number = Convert.ToInt32(billNo.Number) + 1;
            try
            {
                if (OrderDetails != null)
                {
                    if (OrderDetails.Count > 0)
                    {
                        foreach (var x in OrderDetails)
                        {
                            var prefixx = "";
                            var prd = db.Products.Where(a => a.ProductCode == x.ProductCode).FirstOrDefault();
                            if (prd.SerialNoApplicable == true)
                            {
                                prd.CurrentSerialNo = Convert.ToInt32(x.SerialTo);
                                db.Products.AddOrUpdate(prd);
                                db.SaveChanges();
                                prefixx = prd.Prefix;




                                string conn = System.Configuration.ConfigurationManager.ConnectionStrings["InventoryModel"].ConnectionString;
                                SqlConnection cnn = new SqlConnection(conn);
                                SqlCommand cmd = new SqlCommand();
                                cmd.Connection = cnn;
                                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                                cmd.CommandText = "Sp_GenrateSerialNo";
                                cmd.Parameters.AddWithValue("@ProductCode", x.ProductCode);
                                cmd.Parameters.AddWithValue("@ToSerialNo", x.SerialTo);
                                cmd.Parameters.AddWithValue("@FromSeriallNo", x.SerialFrom);
                                cmd.Parameters.AddWithValue("@BatchNo", x.BatchNo);
                                cmd.Parameters.AddWithValue("@GRNNO", x.GRNNo);
                                cmd.Parameters.AddWithValue("@PONO", x.PONo);
                                cmd.Parameters.AddWithValue("@CreatedBy", User.Identity.Name);
                                cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Today);
                                cmd.Parameters.AddWithValue("@PODTLID", x.PurchaseOrderDetailsID);
                                cmd.Parameters.AddWithValue("@WarehouseId", x.WarehouseID);
                                cmd.Parameters.AddWithValue("@StoreLocationId", x.StoreLocationId);
                                cmd.Parameters.AddWithValue("@GRNDate", DTFrom);

                                cmd.Parameters.AddWithValue("@Prefix", prefixx);

                                cnn.Open();

                                DataTable dt = new DataTable();
                                dt.Load(cmd.ExecuteReader());
                                cnn.Close();
                                cmd.Dispose();

                                List<ProductSerialNo> Quot = new List<ProductSerialNo>();

                                string FilePath = Server.MapPath("~/Photo/NotepadFiles/" + x.GRNNo + ".txt");
                                System.IO.File.Delete(FilePath);
                                var count = 0;
                                foreach (DataRow dr in dt.Rows)
                                {
                                    System.IO.File.AppendAllText(FilePath, dr["SerialNo"] + "," + dr["ErrorMessage"] + Environment.NewLine);
                                    count = count + 1;
                                }

                                if (count > 1)
                                {
                                    Session["FileNM"] = x.GRNNo + ".txt";
                                    message = "duplicateFound";
                                    return new JsonResult { Data = new { message = message } };
                                }
                            }

                            //if (prd.SerialNoApplicable == true)
                            //{
                            //    var SerialNoCount = db.TempSerialNo.Where(t => t.PODetailsId == x.PurchaseOrderDetailsID && t.ProductCode == x.ProductCode).Count();
                            //    if (SerialNoCount < x.ReceivedQty)
                            //    {
                            //        message = "Please add serial numbers for " + prd.ProductName + "";
                            //        return new JsonResult { Data = new { message = message } };
                            //    }
                            //}
                            var podtl = db.poDetails.Where(a => a.PurchaseOrderDetailsID == x.PurchaseOrderDetailsID && a.ProductCode == x.ProductCode).FirstOrDefault();
                            podtl.ReceivedQty = (Convert.ToDecimal(podtl.ReceivedQty) + Convert.ToDecimal(x.ReceivedQty));

                            prd.ClosingQuantity = (Convert.ToDecimal(prd.ClosingQuantity) + Convert.ToDecimal(x.ReceivedQty));
                            prd.InwardQuantity = (Convert.ToDecimal(prd.InwardQuantity) + Convert.ToDecimal(x.ReceivedQty));
                            prd.PurchasePrice = x.PurchasePrice;
                            prd.IssuedToProductionQty = 0;
                            prd.ManufactureGRNQty = 0;

                            GRNDetails grn = new GRNDetails();
                            grn.GRNNo = x.GRNNo;
                            grn.GRNDate = DTFrom;
                            grn.PONo = x.PONo;
                            grn.ProductCode = x.ProductCode;
                            grn.ReceivedQty = Convert.ToDecimal(x.ReceivedQty);
                            grn.StoreLocationId = x.StoreLocationId;
                            grn.WarehouseID = x.WarehouseID;
                            grn.BatchNo = x.BatchNo;
                            grn.CompanyID = Convert.ToInt32(Session["CompanyID"]);
                            grn.ExpiryDate = x.ExpiryDate;
                            grn.ManufacturingDate = x.ManufacturingDate;
                            grn.SalesQty = 0;
                            grn.PurchaseOrderDetailsID = x.PurchaseOrderDetailsID;
                            grn.SupplierID = x.SupplierID;
                            grn.ReturnQty = 0;
                            grn.DamageQty = 0;
                            grn.ReplaceQty = 0;
                            //grn.TotalAmount = x.TotalAmount;
                            grn.SerialFrom = x.SerialFrom;
                            grn.SerialTo = x.SerialTo;

                            grn.CreatedBy = User.Identity.Name;
                            grn.SerialNoApplicable = x.SerialNoApplicable;
                            grn.CreatedDate = DateTime.Today;
                            //
                            grn.Tax = x.Tax;
                            grn.BasicRate = x.PurchasePrice;
                            grn.DiscountAs = x.DiscountAs;
                            grn.PODate = x.PODate;
                            decimal singalPrice = Convert.ToDecimal(x.PurchasePrice);
                            string discountAs = x.DiscountAs;
                            decimal? Tax = x.Tax;
                            decimal disc = 0;
                            if (discountAs == "Rupee") { disc = Convert.ToDecimal(x.Discount); grn.Discount = disc; } else { disc = (singalPrice * Convert.ToDecimal(x.Discount)) / 100; grn.Discount = disc; }
                            var tAmount = singalPrice - disc;
                            var tax = (tAmount * Tax) / 100;

                            var supplierPincode = db.suppliers.Where(a => a.SupplierID == x.SupplierID).FirstOrDefault();
                            if(supplierPincode == null)
                            {
                                message = "Invalid Supplier";
                                return new JsonResult { Data = new { message = message } };
                            }
                            if (supplierPincode.BillingState == "Maharashtra")
                            {
                                var taxAmt = tax / 2;
                                grn.CGST = taxAmt;
                                grn.SGST = taxAmt; 
                            }
                            else
                            {
                                grn.IGST = tax;
                            }
                            grn.AmountPerItem = tAmount + tax;

                            try
                            {
                                db.GRNDetail.Add(grn);
                                db.SaveChanges();

                                List<PODetails> pODetails = db.poDetails.Where(a => a.PONO == x.PONo).ToList();

                                bool allSatisfyCondition = true;

                                foreach (var pomain in pODetails)
                                {
                                    if (pomain.ReceivedQty != pomain.OrderQty)
                                    {
                                        allSatisfyCondition = false;
                                        break;
                                    }
                                }

                                if (allSatisfyCondition)
                                {
                                    POMain pOMain = db.POMains.Where(a => a.PurchaseOrderNo == x.PONo).FirstOrDefault();
                                    pOMain.POStatus = "Approve";
                                    db.POMains.AddOrUpdate(pOMain);
                                    db.SaveChanges();
                                }


                            }
                            catch (DbEntityValidationException ex)
                            {
                                var result = db.ProductSerialNo

                                ;
                            }
                            ///delete from product serial no
                            var temp = db.TempSerialNo.Where(a => a.PODetailsId == x.PurchaseOrderDetailsID && a.ProductCode == x.ProductCode).ToList();
                            foreach (var vp in temp)
                                db.TempSerialNo.Remove(vp);
                            db.SaveChanges();
                            message = "success";
                            InvoicePrint(x.GRNNo);
                        }
                        return new JsonResult { Data = new { message = message } };
                    }
                }
            }
            catch (Exception ee)
            {
                return new JsonResult { Data = new { message = ee.Message } };
            }
            return new JsonResult { Data = new { message = message } };
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
                    var result = (from grn in GRNDetailsMaster.Where(a => a.ProductCode == ProductCode).Where(a => a.ReceivedQty - a.SalesQty != 0)

                                  join Product in Productsmaster on grn.ProductCode equals Product.ProductCode into products
                                  from prd in products.DefaultIfEmpty()

                                  join Warehouse in Warehousemaster on grn.WarehouseID equals Warehouse.WareHouseID into warehouse
                                  from Whouse in warehouse.DefaultIfEmpty()

                                  join StoreLocation in Storesmaster on grn.StoreLocationId equals StoreLocation.StoreLocationId into storeLoc
                                  from store in storeLoc.DefaultIfEmpty()

                                  orderby grn.GRNId descending
                                  select new { GRNId = grn.GRNId, SalesQty = grn.SalesQty, temp = 1, WarehouseID = grn.WarehouseID, StoreLocationId = grn.StoreLocationId, BatchNo = grn.BatchNo, ReceivedQty = grn.ReceivedQty, WareHouseName = Whouse == null ? string.Empty : Whouse.WareHouseName, StoreLocation = store == null ? string.Empty : store.StoreLocation }
                                         ).ToList();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
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

                                  orderby tmp.GRNId descending
                                  select new { GRNId = tmp.GRNId, temp = 0, SalesQty = tmp.SalesQty, WarehouseID = tmp.WarehouseID, StoreLocationId = tmp.StoreLocationId, BatchNo = tmp.BatchNo, ReceivedQty = tmp.ReceivedQty, WareHouseName = Whouse == null ? string.Empty : Whouse.WareHouseName, StoreLocation = store == null ? string.Empty : store.StoreLocation }
                                         ).ToList();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult SaveSerialNoDetails(List<TempSerialNo> Data)
        {
            var status = false;
            int cnt = 1;
            try
            {
                if (Data != null)
                {
                    if (Data.Count > 0)
                    {
                        //Delete removed serial numbers
                        var DefaultSerialNo = Data.FirstOrDefault();
                        var SerialNo = Data.Select(x => x.SerialNo).ToList();
                        var TempSerialNo = db.TempSerialNo.Where(x => !SerialNo.Contains(x.SerialNo) && x.ProductCode == DefaultSerialNo.ProductCode && x.PODetailsId == DefaultSerialNo.PODetailsId).ToList();
                        db.TempSerialNo.RemoveRange(TempSerialNo);
                        db.SaveChanges();

                        foreach (var x in Data)
                        {
                            var tmpcnt = db.TempSerialNo.Where(a => a.SerialNo == x.SerialNo && a.PODetailsId == x.PODetailsId && a.ProductCode == x.ProductCode).Count();
                            if (tmpcnt == 0)
                            {
                                TempSerialNo serials = new TempSerialNo();
                                serials.GrnNo = x.GrnNo;
                                serials.PODetailsId = x.PODetailsId;
                                serials.PONO = x.PONO;
                                serials.GrnDate = Convert.ToDateTime(x.GrnDate);
                                //  serials.GrnDate = x.GrnDate;
                                serials.ProductCode = x.ProductCode;

                                serials.StoreLocationId = x.StoreLocationId;
                                serials.WarehouseId = x.WarehouseId;
                                serials.BatchNo = x.BatchNo;
                                serials.SerialNo = x.SerialNo;
                                //serials.SerialTo = x.SerialTo;
                                //serials.SerialFrom = x.SerialFrom;
                                //serials.EndCustomerID = x.EndCustomerID;
                                serials.Status = "inward";

                                db.TempSerialNo.Add(serials);
                                db.SaveChanges();
                                status = true;
                            }
                            else
                            {
                                status = true;
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
        public JsonResult getTempData(string ProductCode, int OrderDtlId)
        {
            try
            {
                var result = db.TempSerialNo.Where(a => a.PODetailsId == OrderDtlId).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception EX)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult getManufactureBillNo()
        {
            var count = db.BillNumbering.Where(a => a.Type == "ManufactureGRN").Select(a => a.Number).Single();
            count = count + 1;
            var result = "MFG_" + count;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getProducts(string MRNNO)
        {
            // var ProductionOrder = db.ProductionOrder.Where(a => a.MRNNO == MRNNO).Select(a => a.OrderNo).ToList();

            var data = (from stock in db.IssueToProductionDetails.Where(x => x.MRNNO == MRNNO)
                        orderby stock.ID descending
                        select new { Product = stock.Product, MainProductCode = stock.MainProductCode } into p
                        group p by new { p.Product, p.MainProductCode } into g
                        select new
                        {
                            Product = g.Select(x => x.Product).FirstOrDefault(),
                            MainProductCode = g.Select(x => x.MainProductCode).FirstOrDefault(),
                        }).ToList();




            //var bomMaster = new List<BOM>(db.Bom);
            //var producrMaster = new List<Products>(db.Products);
            //var data = (from bom in bomMaster 
            //            join Products in producrMaster on bom.ProductId equals Products.ProductCode into PRODUCT
            //            from prd in PRODUCT.DefaultIfEmpty()
            //            orderby bom.ProductId descending
            //            select new { ProductCode = prd.ProductCode, ProductName = prd.ProductName } 
            //                    ).Distinct().ToList();            
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public JsonResult getSerialNo(string ProductName)
        {
            var result = db.Products.Where(a => a.ProductName == ProductName && a.IsActive == true).FirstOrDefault();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveManufactureData(List<GRNDetails> OrderDetails)
        {
            var bill = db.BillNumbering.Where(a => a.Type == "ManufactureGRN").FirstOrDefault();
            bill.Number = bill.Number + 1;

            try
            {
                if (OrderDetails != null)
                {
                    if (OrderDetails.Count > 0)
                    {
                        using (var dbcxtransaction = db.Database.BeginTransaction())
                        {
                            foreach (var x in OrderDetails)
                            {
                                List<ProductSerialNo> productSerialNo = new List<ProductSerialNo>();
                                try
                                {
                                    List<TempSerialNo> tmp = db.TempSerialNo.Where(a => a.BatchNo == x.BatchNo && a.ProductCode == x.ProductCode && a.GrnNo == x.GRNNo).ToList();

                                    foreach (var t in tmp)
                                    {
                                        var serialno = new ProductSerialNo();
                                        serialno.PODetailsId = t.PODetailsId;

                                        serialno.ProductCode = t.ProductCode;
                                        serialno.BatchNo = t.BatchNo;
                                        serialno.WarehouseId = t.WarehouseId;
                                        serialno.StoreLocationId = t.StoreLocationId;
                                        serialno.GrnNo = t.GrnNo;
                                        serialno.CompanyID = Convert.ToInt32(Session["CompanyID"]);
                                        serialno.GrnDate = t.GrnDate;
                                        serialno.SerialNo = t.SerialNo;
                                        //serialno.SerialFrom = t.SerialFrom;
                                        //serialno.SerialTo = t.SerialTo;
                                        //serialno.EndCustomerID = t.EndCustomerID;
                                        serialno.Status = "inward";
                                        productSerialNo.Add(serialno);
                                    }



                                }
                                catch (Exception Ex)
                                {
                                    ;
                                }
                                db.ProductSerialNo.AddRange(productSerialNo);

                                var prd = db.Products.Where(a => a.ProductCode == x.ProductCode).FirstOrDefault();
                                prd.InwardQuantity = prd.InwardQuantity + x.ReceivedQty;
                                prd.ClosingQuantity = prd.ClosingQuantity + x.ReceivedQty;

                                var isbomAvailable = db.Bom.Where(a => a.ProductId == x.ProductCode).Count();
                                var ProdQty = prd.ProductCode;

                                if (isbomAvailable > 0)
                                {
                                    List<BOM> bom = db.Bom.Where(a => a.ProductId == x.ProductCode).ToList();
                                    foreach (var b in bom)
                                    {
                                        var product = db.Products.Where(a => a.ProductCode == b.ComponentId).FirstOrDefault();
                                        var closingbal = product.IssuedToProductionQty;
                                        var outwardbal = product.OutwardQuantity;
                                        var bomqty = b.Quantity;
                                        var Tqty = x.ReceivedQty * bomqty;
                                        if (Tqty <= closingbal)
                                        {
                                            product.IssuedToProductionQty = product.IssuedToProductionQty - Convert.ToInt32(Tqty);
                                            product.ManufactureGRNQty = product.ManufactureGRNQty + Convert.ToInt32(Tqty);
                                        }

                                        var IssueToProduction = db.IssueToProductionDetails.Where(a => a.MRNNO == x.MRNNO && b.ComponentId == a.ProdCode).FirstOrDefault();
                                        IssueToProduction.ManufactureGRNQty = IssueToProduction.ManufactureGRNQty + Convert.ToInt32(Tqty);

                                    }

                                }
                                else
                                {
                                    return new JsonResult { Data = new { message = "something going wrong..." } };
                                }

                                GRNDetails grn = new GRNDetails();
                                grn.GRNNo = x.GRNNo;
                                grn.PONo = x.GRNNo;
                                grn.GRNDate = x.GRNDate;
                                grn.PONo = "0";
                                grn.ProductCode = x.ProductCode;
                                grn.ReceivedQty = Convert.ToDecimal(x.ReceivedQty);
                                grn.StoreLocationId = x.StoreLocationId;
                                grn.WarehouseID = x.WarehouseID;
                                grn.BatchNo = x.BatchNo;
                                grn.CompanyID = Convert.ToInt32(Session["CompanyID"]);
                                grn.ExpiryDate = x.ExpiryDate;
                                grn.ManufacturingDate = x.ManufacturingDate;
                                grn.SalesQty = 0;
                                grn.PurchaseOrderDetailsID = x.PurchaseOrderDetailsID;
                                grn.SupplierID = x.SupplierID;
                                grn.ReturnQty = 0;
                                grn.DamageQty = 0;
                                grn.ReplaceQty = 0;
                                grn.SerialNoApplicable = x.SerialNoApplicable;
                                grn.CreatedBy = User.Identity.Name;
                                grn.CreatedDate = DateTime.Today;

                                try
                                {
                                    db.GRNDetail.Add(grn);
                                    db.SaveChanges();
                                    TempData["Msg"] = "successfully saved.... ";
                                    var temp = db.TempSerialNo.Where(a => a.ProductCode == x.ProductCode && a.BatchNo == x.BatchNo && a.GrnNo == x.BatchNo).ToList();
                                    foreach (var vp in temp)
                                        db.TempSerialNo.Remove(vp);

                                    //return Json(Data, JsonRequestBehavior.AllowGet);
                                }
                                catch (DbEntityValidationException ex)
                                {
                                    ;
                                }
                            }
                            db.SaveChanges();
                            dbcxtransaction.Commit();

                            var result = new { Message = "success" };
                            return Json(result, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                var result = new { Message = ee.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            var result1 = new { Message = "" };
            return Json(result1, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CheckAvailability(string ProductCode, Decimal Qty)
        {
            var result = true;
            var isbomAvailable = db.Bom.Where(a => a.ProductId == ProductCode).Count();
            if (isbomAvailable > 0)
            {
                List<BOM> bom = db.Bom.Where(a => a.ProductId == ProductCode).ToList();
                var temp = true;
                List<SelectListItem> data = new List<SelectListItem>();
                SelectListItem lst = new SelectListItem();
                foreach (var b in bom)
                {
                    var product = db.Products.Where(a => a.ProductCode == b.ComponentId).FirstOrDefault();
                    var closingbal = product.IssuedToProductionQty;
                    var bomqty = b.Quantity;
                    var Tqty = Qty * bomqty;
                    var Count = 1;
                    if (Tqty > closingbal)
                    {
                        TempData["Msg"] = product.ProductName + " Quantity Not Available";
                        Count = Count + 1;
                        lst = new SelectListItem() { Text = product.ProductName, Value = product.IssuedToProductionQty.ToString(), };
                        data.Add(lst);
                        temp = false;
                    }
                }
                if (temp == false)
                {
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    TempData["Msg"] = "";
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveManufactureSerialNo(List<TempSerialNo> Data)
        {
            var status = false;
            int cnt = 1;
            try
            {
                if (Data != null)
                {
                    if (Data.Count > 0)
                    {
                        //Delete removed serial numbers
                        var DefaultSerialNo = Data.FirstOrDefault();
                        var SerialNo = Data.Select(x => x.SerialNo).ToList();
                        var TempSerialNo = db.TempSerialNo.Where(x => !SerialNo.Contains(x.SerialNo) && x.ProductCode == DefaultSerialNo.ProductCode && x.PODetailsId == DefaultSerialNo.PODetailsId).ToList();
                        db.TempSerialNo.RemoveRange(TempSerialNo);
                        db.SaveChanges();

                        foreach (var x in Data)
                        {
                            var tmpcnt = db.TempSerialNo.Where(a => a.SerialNo == x.SerialNo && a.ProductCode == x.ProductCode).Count();
                            if (tmpcnt == 0)
                            {
                                TempSerialNo serials = new TempSerialNo();
                                serials.GrnNo = x.GrnNo;
                                serials.PODetailsId = x.PODetailsId;
                                serials.PONO = x.PONO;
                                serials.GrnDate = Convert.ToDateTime(x.GrnDate);
                                //  serials.GrnDate = x.GrnDate;
                                serials.ProductCode = x.ProductCode;

                                serials.StoreLocationId = x.StoreLocationId;
                                serials.WarehouseId = x.WarehouseId;
                                serials.BatchNo = x.BatchNo;
                                serials.SerialNo = x.SerialNo;
                                serials.Status = "inward";

                                db.TempSerialNo.Add(serials);
                                db.SaveChanges();
                                status = true;
                            }
                            else
                            {
                                status = false;
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
        public JsonResult InvoicePrint(string GRNNo)
        {
            string PONo = db.GRNDetail.Where(a => a.GRNNo == GRNNo).Select(a => a.PONo).FirstOrDefault();
            var grn = db.GRNDetail.Where(a => a.GRNNo == GRNNo).FirstOrDefault();
            var spname = db.suppliers.Where(a => a.SupplierID == grn.SupplierID).FirstOrDefault();
            string GRNDate = $"{grn.GRNDate:dd/MM/yyyy}";
            string SupplierName = spname.SupplierName;
            try
            {
                int Count = 1;
                string OrderNo = "", OrderDate = "", ContactNo = "";
                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["InventoryModel"].ToString());
                string query1 = "SELECT SP.BillingPhone, PRD.ProductName, POD.OrderQty, POD.ReceivedQty, (POD.OrderQty - POD.ReceivedQty) AS BalanceQty, CONVERT(VARCHAR(10), POM.PurchaseOrderDate, 103) AS OrderDate, POM.PurchaseOrderNo AS OrderNo, POM.PurchaseOrderID AS ChallanNo, CONVERT(VARCHAR(10), GETDATE(), 103) AS ChallanDate FROM GRNDetails GRN INNER JOIN PODetails POD ON GRN.PurchaseOrderDetailsID = POD.PurchaseOrderDetailsID INNER JOIN Products PRD ON GRN.ProductCode = PRD.ProductCode INNER JOIN POMains POM ON GRN.PONo = POM.PurchaseOrderNo INNER JOIN Suppliers SP ON GRN.SupplierID = SP.SupplierID  WHERE GRN.PONo = '" + PONo + "' AND GRN.GRNNo = '" + GRNNo + "'";
                //string query1 = "SELECT SP.BillingPhone, PRD.ProductName, POD.OrderQty, POD.ReceivedQty, (POD.OrderQty - POD.ReceivedQty) AS BalanceQty, CONVERT(VARCHAR(10), POM.PurchaseOrderDate, 103) AS OrderDate, POM.PurchaseOrderNo AS OrderNo, POM.PurchaseOrderID AS ChallanNo, CONVERT(VARCHAR(10), GETDATE(), 103) AS ChallanDate FROM PODetails POD INNER JOIN Products PRD ON POD.ProductCode = PRD.ProductCode INNER JOIN POMains POM ON POM.PurchaseOrderNo = POD.PONO INNER JOIN Suppliers SP ON POM.SupplierID = SP.SupplierID INNER JOIN GRNDetails GRN ON  GRN.PONo = POD.PurchaseOrderNo WHERE POD.PONO = '" + PONo + "' AND GRN.GRNNo = '" + GRNNo + "'";
                conn.Open();
                SqlCommand cmd1 = new SqlCommand(query1, conn);
                DataTable dt1 = new DataTable();
                dt1.Load(cmd1.ExecuteReader());
                conn.Close();

                foreach (DataRow cdr in dt1.Rows)
                {
                    OrderNo = cdr["OrderNo"].ToString();
                    OrderDate = cdr["OrderDate"].ToString();
                    ContactNo = cdr["BillingPhone"].ToString();
                }

                iTextSharp.text.Font font5 = iTextSharp.text.FontFactory.GetFont(FontFactory.HELVETICA, 9);
                iTextSharp.text.Font font10 = iTextSharp.text.FontFactory.GetFont(FontFactory.HELVETICA, 11);

                DateTime dt = DateTime.Now;
                var aa = dt.ToString("HH");
                var bb = dt.ToString("mm");
                var cc = dt.ToString("ss");

                Document document = new Document(PageSize.A4, 20f, 10f, 20f, 10f);
                string path = Server.MapPath("~/Reports/Invoice/");
                var filename = GRNNo + ".pdf";
                string filename1 = path + "" + GRNNo + "_" + aa + bb + cc + ".pdf";
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filename1, FileMode.Create));
                document.Open();
                Session["fileName1"] = filename1;
                Session["filename"] = filename;

                PdfPTable table = new PdfPTable(6);
                float[] widths = new float[] { 1.2f, 4f, 2f, 2f, 2f, 0.0f };
                table.SetWidths(widths);
                table.WidthPercentage = 96;

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

                PdfPCell pp5d = new PdfPCell(new Phrase(new Phrase("Received Qty", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                pp5d.HorizontalAlignment = 1;
                pp5d.BackgroundColor = BaseColor.BLACK;
                table.AddCell(pp5d);

                PdfPCell pp5 = new PdfPCell(new Phrase(new Phrase("Balance Qty", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                pp5.HorizontalAlignment = 1;
                pp5.BackgroundColor = BaseColor.BLACK;
                table.AddCell(pp5);

                PdfPCell p7d21 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
                p7d21.HorizontalAlignment = 0;
                table.AddCell(p7d21);

                PdfPTable table1 = new PdfPTable(7);
                float[] width1 = new float[] { 0.0f, 1.2f, 4f, 2f, 2f, 2f, 0.0f };
                table1.SetWidths(width1);
                table1.WidthPercentage = 96;
                table1.DefaultCell.Padding = 0;

                foreach (DataRow r in dt1.Rows)
                {
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
                        pr32.Add(new Phrase("" + r["ProductName"].ToString() + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell prr32 = new PdfPCell(pr32);
                        prr32.HorizontalAlignment = 0;
                        table1.AddCell(prr32);


                        Paragraph pr34 = new Paragraph();
                        pr34.Add(new Phrase("" + r["OrderQty"].ToString() + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell prr34 = new PdfPCell(pr34);
                        prr34.HorizontalAlignment = 1;
                        table1.AddCell(prr34);

                        Paragraph pr35 = new Paragraph();
                        pr35.Add(new Phrase("" + r["ReceivedQty"].ToString() + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell prr35 = new PdfPCell(pr35);
                        prr35.HorizontalAlignment = 1;
                        table1.AddCell(prr35);


                        Paragraph pr36g4 = new Paragraph();
                        pr36g4.Add(new Phrase("" + r["BalanceQty"].ToString() + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell prr36g4 = new PdfPCell(pr36g4);
                        prr36g4.HorizontalAlignment = 1;
                        table1.AddCell(prr36g4);

                        Paragraph pr36g44 = new Paragraph();
                        pr36g44.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell prr36g44 = new PdfPCell(pr36g44);
                        prr36g44.HorizontalAlignment = 0;
                        prr36g44.Border = Rectangle.RIGHT_BORDER;
                        table1.AddCell(prr36g44);
                        Count++;
                    }
                }


                PdfPTable table4 = new PdfPTable(2);
                float[] widths5 = new float[] { 10f, 10f };
                table4.SetWidths(widths5);
                table4.WidthPercentage = 95;
                table4.HorizontalAlignment = 1;

                string imageURL = Server.MapPath("/Photo/MicraftLogo.png");
                iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(imageURL);
                //Resize image depend upon your need
                jpg.ScaleToFit(100, 150);
                //Give space before image
                jpg.SpacingBefore = 5f;
                //Give some space after the image
                jpg.SpacingAfter = 1f;
                jpg.Alignment = Element.ALIGN_LEFT;

                Paragraph p44 = new Paragraph();
                p44.Add(new Phrase());
                PdfPCell c11144 = new PdfPCell(jpg);
                c11144.Border = Rectangle.NO_BORDER;
                table4.AddCell(c11144);

                Paragraph p4 = new Paragraph();
                p4.Add(new Phrase(" Siddhivinayak Distributor", FontFactory.GetFont("Arial", 14, Font.BOLD)));
                p4.Add(new Phrase(" "));
                p4.Add(new Phrase("\n Shop no 10, Suyog Navkar Building A, \n Near 7 Loves Chowk, Market Yard Road, Pune 411 037.\n GSTIN: 27ABVPK5495R2Z9\n", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                PdfPCell c1114 = new PdfPCell(p4);
                c1114.HorizontalAlignment = 0;
                c1114.Border = Rectangle.NO_BORDER;
                table4.AddCell(c1114);


                PdfPTable table6 = new PdfPTable(3);
                float[] width6 = new float[] { 5f, 7f, 4f };
                table6.SetWidths(width6);
                table6.WidthPercentage = 96;
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


                PdfPTable table8 = new PdfPTable(3);
                float[] width8 = new float[] { 5f, 7f, 4f };
                table8.SetWidths(width8);
                table8.WidthPercentage = 96;
                table8.HorizontalAlignment = 1;

                Paragraph pr183 = new Paragraph();
                pr183.Add(new Phrase("_", FontFactory.GetFont("Arial", 0, Font.NORMAL)));
                PdfPCell pc1833 = new PdfPCell(pr183);
                pc1833.HorizontalAlignment = 0;
                pc1833.Border = Rectangle.BOTTOM_BORDER;
                table8.AddCell(pc1833);

                Paragraph pr283 = new Paragraph();
                pr283.Add(new Phrase(".", FontFactory.GetFont("Arial", 0, Font.NORMAL)));
                PdfPCell pc283 = new PdfPCell(pr283);
                pc283.HorizontalAlignment = 1;
                pc283.Border = Rectangle.BOTTOM_BORDER;
                table8.AddCell(pc283);

                Paragraph pr383 = new Paragraph();
                pr383.Add(new Phrase("", FontFactory.GetFont("Arial", 0, Font.NORMAL)));
                PdfPCell pc383 = new PdfPCell(pr383);
                pc383.HorizontalAlignment = 1;
                pc383.Border = Rectangle.BOTTOM_BORDER;
                table8.AddCell(pc383);

                Paragraph pr3883 = new Paragraph();
                pr3883.Add(new Phrase("", FontFactory.GetFont("Arial", 0, Font.NORMAL)));
                PdfPCell pc3883 = new PdfPCell(pr3883);
                pc3883.HorizontalAlignment = 1;
                pc3883.Border = Rectangle.BOTTOM_BORDER;
                table8.AddCell(pc3883);

                PdfPTable table7 = new PdfPTable(4);
                float[] width7 = new float[] { 7.2f, 4f, 0.0f, 0.0f };
                table7.SetWidths(width7);
                table7.WidthPercentage = 96;
                table7.HorizontalAlignment = 1;

                Paragraph pr185 = new Paragraph();
                pr185.Add(new Phrase("        GST NO :  \n\n", FontFactory.GetFont("Arial", 8, Font.BOLD)));
                PdfPCell pc185 = new PdfPCell();
                pc185.HorizontalAlignment = 0;
                pc185.Border = Rectangle.LEFT_BORDER;
                table7.AddCell(pc185);


                Paragraph pr285 = new Paragraph();
                pr285.Add(new Phrase("For Siddhivinayak Distributor \n\n\n\n", FontFactory.GetFont("Arial", 8, Font.BOLD)));
                pr285.Add(new Phrase("                                      Authorised Signatory", FontFactory.GetFont("Arial", 7, Font.BOLD)));
                PdfPCell pc285 = new PdfPCell(pr285);
                pc285.HorizontalAlignment = 1;
                pc285.Border = Rectangle.LEFT_BORDER;
                table7.AddCell(pc285);

                Paragraph pr385 = new Paragraph();
                pr385.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                PdfPCell pc385 = new PdfPCell(pr385);
                pc385.HorizontalAlignment = 1;
                pc385.Border = Rectangle.TOP_BORDER;
                table7.AddCell(pc385);

                Paragraph pr3855 = new Paragraph();
                pr3855.Add(new Phrase("sdsd", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                PdfPCell pc3855 = new PdfPCell(pr3855);
                pc3855.HorizontalAlignment = 1;
                pc3855.Border = Rectangle.RIGHT_BORDER;
                table7.AddCell(pc3855);

                PdfPTable table5 = new PdfPTable(2);
                float[] width5 = new float[] { 1.2f, 7f };
                table5.SetWidths(width5);
                table5.WidthPercentage = 95;
                table5.HorizontalAlignment = 1;

                Paragraph pr101 = new Paragraph();
                pr101.Add(new Phrase("    Supplier Name\n", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                pr101.Add(new Phrase("    Contact No", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                PdfPCell pc2 = new PdfPCell(pr101);
                pc2.HorizontalAlignment = 0;
                pc2.Border = Rectangle.NO_BORDER;
                table5.AddCell(pc2);

                Paragraph pr1 = new Paragraph();
                pr1.Add(new Phrase(":   " + SupplierName + "\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                pr1.Add(new Phrase(":   " + ContactNo, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                PdfPCell pc1 = new PdfPCell(pr1);
                pc1.HorizontalAlignment = 0;
                pc1.FixedHeight = 50f;
                pc1.Border = Rectangle.NO_BORDER;
                table5.AddCell(pc1);

                PdfPTable table9 = new PdfPTable(1);
                float[] width9 = new float[] { 20f };
                table9.SetWidths(width9);
                table9.WidthPercentage = 96;

                PdfPCell p79013 = new PdfPCell();
                p79013.HorizontalAlignment = 0;
                table9.AddCell(p79013);


                PdfPTable table3 = new PdfPTable(4);
                float[] widths3 = new float[] { 4f, 6f, 10f, 10f };
                table3.SetWidths(widths3);
                table3.WidthPercentage = 95;
                table3.HorizontalAlignment = 1;


                Paragraph pr53 = new Paragraph();
                pr53.Add(new Phrase("\n\n    GRN No\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                pr53.Add(new Phrase("    GRN Date ", FontFactory.GetFont("Arial", 9, Font.NORMAL)));

                PdfPCell c112 = new PdfPCell(pr53);
                c112.Border = Rectangle.TOP_BORDER;
                c112.FixedHeight = 50f;
                c112.HorizontalAlignment = 0;
                table3.AddCell(c112);

                Paragraph pr539 = new Paragraph();
                pr539.Add(new Phrase("\n\n  :  " + GRNNo + "\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                pr539.Add(new Phrase("  :  " + GRNDate + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));

                PdfPCell c1129 = new PdfPCell(pr539);
                c1129.Border = Rectangle.TOP_BORDER;
                c1129.FixedHeight = 50f;
                c1129.HorizontalAlignment = 0;
                table3.AddCell(c1129);

                Paragraph p49 = new Paragraph();
                p49.Add(new Phrase("      GOODS RECEIVED NOTE", FontFactory.GetFont("Arial", 11, Font.BOLD)));
                PdfPCell c1115 = new PdfPCell(p49);
                c1115.HorizontalAlignment = 0;
                c1115.Border = Rectangle.TOP_BORDER;
                table3.AddCell(c1115);

                Paragraph pr5397 = new Paragraph();
                pr5397.Add(new Phrase("\n\n      Order No       :  " + OrderNo + "\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                pr5397.Add(new Phrase("      OrderDate     :  " + OrderDate + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));


                PdfPCell c11297 = new PdfPCell(pr5397);
                c11297.Border = Rectangle.TOP_BORDER;
                c11297.FixedHeight = 50f;
                c11297.HorizontalAlignment = 0;
                table3.AddCell(c11297);

                document.Add(table4);
                document.Add(table3);
                document.Add(table6);
                document.Add(table5);
                document.Add(table);
                document.Add(table1);
                document.Add(table7);
                document.Add(table8);
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

        public JsonResult SaveInvoiceDetails(GRNDetails grn)
        {
            List<GRNDetails> grnData = db.GRNDetail.Where(a => a.GRNNo == grn.GRNNo).ToList();
            if (grnData.Count == 0 || grnData == null)
            {
                return new JsonResult { Data = new { status = false } };
            }
            foreach (var data in grnData)
            {
                data.TAmount = grn.TAmount;
                data.BasicAmount = grn.BasicAmount;
                data.DiscAmount = grn.DiscAmount;
                data.InvoiceNo = grn.InvoiceNo;
                data.InvoiceDate = grn.InvoiceDate;
                data.TransportAmount = grn.TransportAmount;
                data.TaxAmount = grn.TaxAmount;
                db.GRNDetail.AddOrUpdate(data);
                db.SaveChanges();
            }
            var result = new { Message = "success" };
            return new JsonResult { Data = new { status = true } };
        }

        public JsonResult ShowPOInvoiceData(string GRNNo)
        {
            GRNDetails grnData = db.GRNDetail.Where(a => a.GRNNo == GRNNo).FirstOrDefault();
            if (grnData == null)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
           
            return Json(grnData, JsonRequestBehavior.AllowGet);
        }
    }

}

