using Inventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class DamageController : Controller
    {
        InventoryModel db = new InventoryModel();
        // GET: Damage
        public ActionResult Index()
        {
            var setting = db.Settings.Where(x => x.FieldName == "BatchNo" && x.Setting == true).FirstOrDefault();
            if (setting != null)
            {
                ViewBag.BatchNoSetting = setting.FieldName;
            }
            else
            {
                ViewBag.BatchNoSetting = "";
            }
            var SerialNo = db.Settings.Where(x => x.FieldName == "SerialNo" && x.Setting == true).FirstOrDefault();
            if (SerialNo != null)
            {
                ViewBag.SerialNoSetting = setting.FieldName;
            }
            else
            {
                ViewBag.BatchNoSetting = "";
            }
            var DamageMaster = new List<Damage>(db.damage);
            ViewBag.datasource = (from c in DamageMaster                                      

                                  join sply in db.suppliers on c.SupplierId equals sply.SupplierID into Suppliers
                                  from aa in Suppliers.DefaultIfEmpty()

                                  join prd in db.Products on c.ProductCode equals prd.ProductCode into Product
                                  from pp in Product.DefaultIfEmpty()

                                  group c by new
                                  {
                                      //    c.ReturnDate, ProductCode
                                      c.DamageNo,
                                      c.ProductCode,
                                      c.GRNId,
                                      aa.SupplierName,
                                      c.PONO,
                                      pp.ProductName
                                  } into gcs
                                  select new
                                  {
                                      //      ReturnDate = gcs.Key.ReturnDate,
                                      ProductCode = gcs.Key.ProductName,
                                      DamageNo = gcs.Key.DamageNo,
                                      GRNId = gcs.Key.GRNId,
                                      PONO = gcs.Key.PONO,
                                      SupplierName = gcs.Key.SupplierName,
                                      ReturnQty = gcs.Sum(a => a.DamageQty),
                                  }).ToList();
            return View();
        }
        public ActionResult DamageByBatchNo()
        {
            ViewBag.Warehousedatasource = db.Warehouses.ToList();
            ViewBag.Productdatasource = db.Products.Where(a => a.IsActive == true && a.SerialNoApplicable==false).ToList();
            return View();
        }
        public JsonResult getDamageNo()
        {
            var result = db.damage.Count();
            result = result + 1;
                return Json(result, JsonRequestBehavior.AllowGet);            
        }
        public ActionResult DamageBySerialNo()
        {
            ViewBag.Productdatasource = db.Products.Where(a => a.SerialNoApplicable == true && a.IsActive == true);
            return View();
        }
        public JsonResult bindStoreLocation(string ProductId,string WarehouseId)
        {
            if (ProductId == "" || WarehouseId == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = db.StoreLocations.Where(a => a.WarehouseId == WarehouseId && a.IsActive == true).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }
        public JsonResult getSerialNoData(string SerialNo ,string ProductId)
        {
            if (SerialNo == "" )
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {           
                var Productsmaster = new List<Products>(db.Products);
                var Storesmaster = new List<StoreLocations>(db.StoreLocations);
                var Warehousemaster = new List<Warehouse>(db.Warehouses);
                var ProductSerialNo = new List<ProductSerialNo>(db.ProductSerialNo);
                var result = (from serial in ProductSerialNo.Where(a => a.SerialNo == SerialNo && a.Status =="inward" && a.ProductCode==ProductId)

                              join Product in Productsmaster on serial.ProductCode equals Product.ProductCode into products
                              from prd in products.DefaultIfEmpty()

                              join Warehouse in Warehousemaster on serial.WarehouseId equals Warehouse.WareHouseID into warehouse
                              from Whouse in warehouse.DefaultIfEmpty()

                              join StoreLocation in Storesmaster on serial.StoreLocationId equals StoreLocation.StoreLocationId into storeLoc
                              from store in storeLoc.DefaultIfEmpty()

                              orderby serial.SerialNoId ascending
                              select new { SerialNoId = serial.SerialNoId, PODetailsId = serial.PODetailsId, BatchNo = serial.BatchNo, GrnNo = serial.GrnNo, PONO = serial.PONO, SerialNo = serial.SerialNo, Status = serial.Status, ProductName = prd == null ? string.Empty : prd.ProductName, WareHouseName = Whouse == null ? string.Empty : Whouse.WareHouseName, StoreLocation = store == null ? string.Empty : store.StoreLocation, ReturnQty = serial.ReturnQty , ProductCode = serial.ProductCode, StoreLocationId = serial.StoreLocationId, WarehouseId = serial.WarehouseId }
                                     ).ToList();

                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }
        
        public JsonResult getBatchNo(string ProductId, string WarehouseId ,int LocationId)
        {
            if (ProductId == "" || WarehouseId == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }   
            else
            {
                var result = db.GRNDetail.Where(a => a.WarehouseID == WarehouseId && a.ProductCode== ProductId && a.StoreLocationId== LocationId).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }
        public JsonResult getAvailableQty(string ProductId, string WarehouseId, int LocationId,string batchNo)
        {
            if (ProductId == "" || WarehouseId == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = db.GRNDetail.Where(a => a.WarehouseID == WarehouseId && a.ProductCode == ProductId && a.StoreLocationId == LocationId && a.BatchNo==batchNo).Sum(a=>a.ReceivedQty-a.SalesQty);
                if(result<1)
                {
                    result = 0;
                }
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult getGrnData(string ProductId, string WarehouseId, int LocationId, string batchNo)
        {
            if (ProductId == "" || WarehouseId == "")
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = db.GRNDetail.Where(a => a.WarehouseID == WarehouseId && a.ProductCode == ProductId && a.StoreLocationId == LocationId && a.BatchNo == batchNo && a.ReceivedQty !=a.DamageQty).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult save(List<Damage> damage)
        {
            var status = false;
            int cnt = 1;
            try
            {
                if (damage != null)
                {
                    if (damage.Count > 0)
                    {
                        foreach (var x in damage)
                        {
                            var prd = db.Products.Where(a => a.ProductCode == x.ProductCode).FirstOrDefault();
                            prd.ClosingQuantity = (Convert.ToDecimal(prd.ClosingQuantity) - Convert.ToDecimal(x.DamageQty));
                            prd.InwardQuantity = (Convert.ToDecimal(prd.InwardQuantity) - Convert.ToDecimal(x.DamageQty));
                            prd.DamageQty= (Convert.ToDecimal(prd.DamageQty) + Convert.ToDecimal(x.DamageQty));

                            //var grn = db.GRNDetail.Where(a => a.GRNId == x.GRNId).FirstOrDefault();
                            //grn.ReturnQty = Convert.ToDecimal(grn.ReturnQty) + Convert.ToDecimal(x.DamageQty);
                            //grn.ReceivedQty = Convert.ToDecimal(grn.ReceivedQty) - Convert.ToDecimal(x.DamageQty);
                            //grn.ReturnReason = x.DamageReason;

                            //var order = db.poDetails.Where(a => a.PurchaseOrderDetailsID == x.PODetailsId).FirstOrDefault();
                            //order.ReturnQty = Convert.ToDecimal(order.ReturnQty) + Convert.ToDecimal(x.DamageQty);
                            //order.ReturnReason = x.DamageReason;
                            //order.ReceivedQty = Convert.ToDecimal(order.ReceivedQty) - Convert.ToDecimal(x.DamageQty);
                            List<TempDamage> tmp = db.TempDamage.Where(a => a.ProductCode == x.ProductCode && a.WarehouseId==x.WarehouseId && a.StoreLocationId==x.StoreLocationId && a.BatchNo==x.BatchNo).ToList();
                            
                            foreach (var t in tmp)
                            {
                                var result = db.GRNDetail.Where(a => a.BatchNo == x.BatchNo && a.ProductCode == x.ProductCode && a.WarehouseID == x.WarehouseId && a.StoreLocationId == x.StoreLocationId).FirstOrDefault();
                                if (result!=null)
                                {
                                    result.DamageQty = (Convert.ToDecimal(result.DamageQty) + Convert.ToDecimal(t.DamageQty));
                                    result.ReceivedQty = Convert.ToDecimal(result.ReceivedQty) - Convert.ToDecimal(t.DamageQty);
                                    Damage poRtn = new Damage();
                                    poRtn.PODetailsId = t.PODetailsId;
                                    poRtn.GRNId = t.GRNId;
                                    poRtn.ProductCode = t.ProductCode;
                                    poRtn.DamageQty = t.DamageQty;
                                    poRtn.WarehouseId = t.WarehouseId;
                                    poRtn.SupplierId = t.SupplierId;
                                    poRtn.StoreLocationId = t.StoreLocationId;
                                    poRtn.BatchNo = t.BatchNo;
                                    poRtn.DamageNo = t.DamageNo;
                                    db.damage.Add(poRtn);
                                }
                            }
                            
                            status = true;                            
                            foreach (var vp in tmp)
                                db.TempDamage.Remove(vp);
                            db.SaveChanges();
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
        public JsonResult SaveDamageTempData(List<Damage> damage)
        {
            var status = false;
            int cnt = 1;
            try
            {
                if (damage != null)
                {
                    if (damage.Count > 0)
                    {
                        foreach (var x in damage)
                        {
                            //var prd = db.Products.Where(a => a.ProductCode == x.ProductCode).FirstOrDefault();
                            //prd.ClosingQuantity = (Convert.ToDecimal(prd.ClosingQuantity) - Convert.ToDecimal(x.DamageQty));
                            //prd.InwardQuantity = (Convert.ToDecimal(prd.InwardQuantity) - Convert.ToDecimal(x.DamageQty));
                            //prd.DamageQty = (Convert.ToDecimal(prd.DamageQty) - Convert.ToDecimal(x.DamageQty));

                            //var grn = db.GRNDetail.Where(a => a.GRNId == x.GRNId).FirstOrDefault();
                            //grn.ReturnQty = Convert.ToDecimal(grn.ReturnQty) + Convert.ToDecimal(x.DamageQty);
                            //grn.ReceivedQty = Convert.ToDecimal(grn.ReceivedQty) - Convert.ToDecimal(x.DamageQty);
                            //grn.ReturnReason = x.DamageReason;

                            //var order = db.poDetails.Where(a => a.PurchaseOrderDetailsID == x.PODetailsId).FirstOrDefault();
                            //order.ReturnQty = Convert.ToDecimal(order.ReturnQty) + Convert.ToDecimal(x.DamageQty);
                            //order.ReturnReason = x.DamageReason;
                            //order.ReceivedQty = Convert.ToDecimal(order.ReceivedQty) - Convert.ToDecimal(x.DamageQty);

                            TempDamage poRtn = new TempDamage();                            
                            poRtn.ProductCode = x.ProductCode;
                            poRtn.DamageQty = x.DamageQty;
                            poRtn.SupplierId = x.SupplierId;
                            poRtn.WarehouseId = x.WarehouseId;
                            poRtn.StoreLocationId = x.StoreLocationId;
                            poRtn.BatchNo = x.BatchNo;
                            poRtn.DamageQty = x.DamageQty;
                            poRtn.DamageNo = x.DamageNo;
                            poRtn.GRNId = x.GRNId;
                            poRtn.PODetailsId = x.PODetailsId;
                            poRtn.DamageNo = x.DamageNo;
                            db.TempDamage.Add(poRtn);
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
        public JsonResult SaveSerialNo(List<Damage> damage)
        {
            var status = false;
            int cnt = 1;
            try
            {
                if (damage != null)
                {
                    if (damage.Count > 0)
                    {
                        foreach (var x in damage)
                        {
                            var prd = db.Products.Where(a => a.ProductCode == x.ProductCode).FirstOrDefault();
                            prd.ClosingQuantity = (Convert.ToDecimal(prd.ClosingQuantity) - Convert.ToDecimal(x.DamageQty));
                            prd.InwardQuantity = (Convert.ToDecimal(prd.InwardQuantity) - Convert.ToDecimal(x.DamageQty));
                            prd.DamageQty = (Convert.ToDecimal(prd.DamageQty) + Convert.ToDecimal(x.DamageQty));

                            var serial = db.ProductSerialNo.Where(a => a.SerialNoId == x.SerialNoId).FirstOrDefault();
                            serial.Status = "Damage";
                           

                            var grn = db.GRNDetail.Where(a => a.GRNNo == x.GRNNo && a.ProductCode==x.ProductCode).FirstOrDefault();
                            grn.DamageQty = Convert.ToDecimal(grn.DamageQty) + Convert.ToDecimal(x.DamageQty);
                            grn.ReceivedQty = Convert.ToDecimal(grn.ReceivedQty) - Convert.ToDecimal(x.DamageQty);

                            //var order = db.poDetails.Where(a => a.PurchaseOrderDetailsID == x.PODetailsId).FirstOrDefault();
                            //order.ReturnQty = Convert.ToDecimal(order.ReturnQty) + Convert.ToDecimal(x.DamageQty);
                            //order.ReturnReason = x.DamageReason;
                            //order.ReceivedQty = Convert.ToDecimal(order.ReceivedQty) - Convert.ToDecimal(x.DamageQty);

                            Damage poRtn = new Damage();
                                    poRtn.PODetailsId = x.PODetailsId;
                                    //poRtn.GRNId = x.GRNId;
                                    poRtn.ProductCode = x.ProductCode;
                                    poRtn.DamageQty = x.DamageQty;
                                    poRtn.SerialNo = x.SerialNo;
                                    poRtn.WarehouseId = x.WarehouseId;
                                    poRtn.SupplierId = grn.SupplierID;
                                    poRtn.StoreLocationId = x.StoreLocationId;
                                    poRtn.BatchNo = x.BatchNo;
                                    poRtn.DamageNo = x.DamageNo;
                                     poRtn.DamageDate = x.DamageDate;
                                    db.damage.Add(poRtn);
                          

                        }
                        db.SaveChanges();
                        status = true;
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
    }
}