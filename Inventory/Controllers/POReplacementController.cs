using Inventory.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class POReplacementController : Controller
    {
        InventoryModel db = new InventoryModel();
        public ActionResult Index()
        {
            

            //ViewBag.datasource = (from GRN in db.GRNDetail
            //                      where GRN.ReplaceQty > 0
            //                      //join Prod in db.Products on GRN.ProductCode equals Prod.ProductCode into p
            //                      //from Product in p.DefaultIfEmpty()

            //                      orderby GRN.GRNNo descending
            //                      select new { GRNId = GRN.GRNId, GRN = GRN.GRNNo, GRNDate = GRN.GRNDate, PONO = GRN.PONo, PODate = GRN.PODate/*, ProductName = Product.ProductName*/ }
            //                        ).ToList();

            ViewBag.datasource = (from GRN in db.GRNDetail
                                  where GRN.ReplaceQty > 0
                              
                                  orderby GRN.GRNNo descending
                                  group GRN by GRN.GRNNo into g
                                  select new { GRNId = g.FirstOrDefault().GRNId, GRN = g.Key, GRNDate = g.FirstOrDefault().GRNDate, PONO = g.FirstOrDefault().PONo, PODate = g.FirstOrDefault().PODate }
                     ).ToList();



            return View();
        }

        public ActionResult Edit(int Id)
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

            var GRN = db.GRNDetail.Where(x => x.GRNId == Id).Select(x => new { x.GRNNo, x.PONo, x.SupplierID }).FirstOrDefault();            
            ViewBag.PONO = GRN.PONo;
            ViewBag.OldGRNNo = GRN.GRNNo;
            var supplier = db.suppliers.Where(x => x.SupplierID == GRN.SupplierID).Select(x => new { x.SupplierName }).FirstOrDefault();
            ViewBag.Vendor = supplier.SupplierName;
            ViewBag.SupplierId = GRN.SupplierID;
            var count = db.BillNumbering.Where(x => x.Type == "NewGRN").FirstOrDefault();
            var Number = count.Number;
            var PO = db.POMains.Where(x => x.PurchaseOrderNo == GRN.PONo).Select(x => new{ x.WarehouseId}).FirstOrDefault();
            var Warehouse = db.Warehouses.Where(x => x.WareHouseID == PO.WarehouseId).Select(x => new { x.WareHouseName }).FirstOrDefault();
            ViewBag.WarehouseId = PO.WarehouseId;
            ViewBag.WareHouse = Warehouse.WareHouseName;
            ViewBag.GRNNo = "NEWGRNNo_" + Number;            
            return View();
        }

        public ActionResult GetData(string GRNNO)
        {
            var result = (from GRN in db.GRNDetail
                          where GRN.GRNNo == GRNNO && GRN.ReplaceQty>0
                          join poDetail in db.poDetails on GRN.PurchaseOrderDetailsID equals poDetail.PurchaseOrderDetailsID into poD
                          from poDetails in poD.DefaultIfEmpty()

                          join prod in db.Products on GRN.ProductCode equals prod.ProductCode into p
                          from product in p.DefaultIfEmpty()

                          orderby GRN.GRNId descending
                          select new {CompanyID = GRN.CompanyID, GRNId = GRN.GRNId, ProductCode = product.ProductCode, ProductName = product.ProductName, OrderQty = poDetails.OrderQty, ReplaceQty = GRN.ReplaceQty, GSTPercentage = poDetails.GSTPercentage, Price = poDetails.Price, Discount = poDetails.Discount, DiscountAs = poDetails.DiscountAs, SerialNoApplicable = GRN.SerialNoApplicable, PurchaseOrderDetailsID = GRN.PurchaseOrderDetailsID, ManuFacturingDate = GRN.ManufacturingDate, ExpiryDate= GRN.ExpiryDate, BatchNo = GRN.BatchNo }).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Edit(GRNDetails grnDetails)
        {
            return View();
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
        public ActionResult DuplicateSerialNo(string SerialNo, string ProductCode)
        {
            var flag = true;
            var TempSerial = db.TempSerialNo.Where(x => x.SerialNo == SerialNo && x.ProductCode == ProductCode).Count();
            if (TempSerial > 0)
                flag = false;
            var ProductSerial = db.ProductSerialNo.Where(x=>x.SerialNo==SerialNo && x.ProductCode==ProductCode).Count();
            if (ProductSerial > 0)
                flag = false;
            return Json(flag, JsonRequestBehavior.AllowGet);
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

        public JsonResult save(List<GRNDetails> OrderDetails, int NewQtyForSerialNo)
        {
            var message = "";
            int cnt = 1;
            try
            {
                if (OrderDetails != null)
                {
                    if (OrderDetails.Count > 0)
                    {
                        foreach (var x in OrderDetails)
                        {
                            var prd = db.Products.Where(a => a.ProductCode == x.ProductCode).FirstOrDefault();
                            if(NewQtyForSerialNo != 0)
                            {
                                prd.CurrentSerialNo = NewQtyForSerialNo;
                                db.Products.AddOrUpdate(prd);
                            }
                           
                            if (prd.SerialNoApplicable == true)
                            {
                                var SerialNoCount = db.TempSerialNo.Where(t => t.PODetailsId == x.PurchaseOrderDetailsID && t.ProductCode == x.ProductCode).Count();
                                if (SerialNoCount < x.ReceivedQty)
                                {
                                    message = "Please add serial numbers for " + prd.ProductName + "";
                                    return new JsonResult { Data = new { message = message } };
                                }
                            }
                            var podtl = db.poDetails.Where(a => a.PurchaseOrderDetailsID == x.PurchaseOrderDetailsID).Select(a => new { a.PurchaseOrderID}).FirstOrDefault();
                            var poMain = db.POMains.Where(a => a.PurchaseOrderID == podtl.PurchaseOrderID).Select(a => new { a.PurchaseOrderDate }).FirstOrDefault();
                            //podtl.ReceivedQty = (Convert.ToDecimal(podtl.ReceivedQty) + Convert.ToDecimal(x.ReceivedQty));

                            prd.ClosingQuantity = (Convert.ToDecimal(prd.ClosingQuantity) + Convert.ToDecimal(x.ReceivedQty));
                            prd.InwardQuantity = (Convert.ToDecimal(prd.InwardQuantity) + Convert.ToDecimal(x.ReceivedQty));
                            prd.PurchasePrice = x.PurchasePrice;

                            List<TempSerialNo> tmp = db.TempSerialNo.Where(a => a.PODetailsId == x.PurchaseOrderDetailsID).ToList();
                            List<ProductSerialNo> productSerialNo = new List<ProductSerialNo>();
                            foreach (var t in tmp)
                            {
                                var serialno = new ProductSerialNo();
                                serialno.PODetailsId = t.PODetailsId;
                                serialno.PONO = t.PONO;
                                serialno.ProductCode = t.ProductCode;
                                serialno.BatchNo = t.BatchNo;
                                serialno.WarehouseId = t.WarehouseId;
                                serialno.StoreLocationId = t.StoreLocationId;
                                serialno.GrnNo = t.GrnNo;
                                serialno.GrnDate = t.GrnDate;
                                serialno.SerialNo = t.SerialNo;
                                serialno.Status = "inward";
                                productSerialNo.Add(serialno);
                            }
                            db.ProductSerialNo.AddRange(productSerialNo);
                           
                            var OldGRNDetails = db.GRNDetail.Where(o => o.GRNId == x.GRNId).FirstOrDefault();
                            if(x.ReceivedQty == x.ReplaceQty)
                            {
                                OldGRNDetails.CompanyID = 1;
                                db.GRNDetail.AddOrUpdate(OldGRNDetails);
                            }

                            GRNDetails grn = new GRNDetails();
                            grn.GRNNo = x.GRNNo;
                            grn.GRNDate = x.GRNDate;
                            grn.PODate = poMain.PurchaseOrderDate;
                            grn.PONo = x.PONo;
                            grn.ProductCode = x.ProductCode;
                            grn.ReceivedQty = Convert.ToDecimal(x.ReceivedQty);
                            grn.ReplaceQty = 0;
                            grn.CompanyID = 0;
                            grn.StoreLocationId = x.StoreLocationId;
                            grn.WarehouseID = x.WarehouseID;
                            grn.BatchNo = x.BatchNo;
                            grn.ExpiryDate = x.ExpiryDate;
                            grn.ManufacturingDate = x.ManufacturingDate;
                            grn.SalesQty = 0;
                            grn.PurchaseOrderDetailsID = x.PurchaseOrderDetailsID;
                            grn.SupplierID = x.SupplierID;
                            grn.ReturnQty = 0;
                            grn.DamageQty = 0;
                            grn.CreatedBy = User.Identity.Name;
                            grn.SerialNoApplicable = x.SerialNoApplicable;
                            grn.CreatedDate = DateTime.Today;

                            grn.DiscountAs = OldGRNDetails.DiscountAs;
                            grn.BasicRate = OldGRNDetails.BasicRate;
                            grn.Discount = OldGRNDetails.Discount;
                            grn.IGST = OldGRNDetails.IGST;
                            grn.CGST = OldGRNDetails.CGST;
                            grn.SGST = OldGRNDetails.SGST;
                            grn.Tax = OldGRNDetails.Tax;
                            grn.AmountPerItem = OldGRNDetails.AmountPerItem;

                            db.GRNDetail.Add(grn);

                            //MInus replace Qauntity from Old GRN
                            POReplacement replacement = new POReplacement();
                            
                            replacement.ReplacementQty = OldGRNDetails.ReplaceQty;
                            int ReplaceQty =Convert.ToInt32(OldGRNDetails.ReplaceQty);
                            ReplaceQty = ReplaceQty-Convert.ToInt32(x.ReceivedQty);
                            OldGRNDetails.ReplaceQty = ReplaceQty;
                            //if (OldGRNDetails.ReplaceQty < 1)
                            //    OldGRNDetails.ReplaceReason = "Replaced";

                            
                            replacement.GRNNo = OldGRNDetails.GRNNo;
                            replacement.NewGRNNo = x.GRNNo;
                            replacement.NewGRNDate =  Convert.ToDateTime(x.GRNDate);
                            replacement.GRNDate = Convert.ToDateTime(OldGRNDetails.GRNDate);
                            replacement.PONO = x.PONo;
                            replacement.ProductCode = x.ProductCode;
                            replacement.ReplacedQty = x.ReceivedQty;
                            replacement.CreatedBy = User.Identity.Name;
                            replacement.CreatedDate = DateTime.Now;
                            replacement.IsActive = true;
                            //if (x.ReceivedQty == ReplaceQty)
                            //{
                            //    replacement.CompanyID = 1;
                            //}
                           
                            db.POReplacement.Add(replacement);

                            db.SaveChanges();
                            var temp = db.TempSerialNo.Where(a => a.PODetailsId == x.PurchaseOrderDetailsID).ToList();
                            foreach (var vp in temp)
                                db.TempSerialNo.Remove(vp);
                            db.SaveChanges();
                            message = "success";
                        }
                        if (message == "success")
                        {
                            var BillNumbers = db.BillNumbering.Where(x => x.Type == "NewGRN").FirstOrDefault();
                            int number = Convert.ToInt32(BillNumbers.Number)+1;
                            BillNumbers.Number = number;
                            db.SaveChanges();
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

        public JsonResult GetDataForCompID()
        {
            try
            {
                var result = db.POReplacement.ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception EX)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult getCurrentSerialNoFromPrd(string NewGRNNO)
        {
            
            var grnNo = db.GRNDetail.Where(a=>a.GRNNo == NewGRNNO).FirstOrDefault();
            if (grnNo == null)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            var result = db.Products.Where(a => a.ProductCode == grnNo.ProductCode).FirstOrDefault();
            if (result == null)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDataForEdit(string GRNNO)
        {
            List<object> result = new List<object>();
            var GRNNoList = db.POReplacement.Where(a => a.GRNNo == GRNNO).ToList();
            if (GRNNoList.Count == 0 || GRNNoList == null)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                
                    var query = (from GRN in db.GRNDetail
                                 where GRN.GRNNo == GRNNO
                                 join poDetail in db.poDetails on GRN.PurchaseOrderDetailsID equals poDetail.PurchaseOrderDetailsID into poD
                                 from poDetails in poD.DefaultIfEmpty()
                                 join prod in db.Products on GRN.ProductCode equals prod.ProductCode into p
                                 from product in p.DefaultIfEmpty()
                                 orderby GRN.GRNId descending
                                 select new
                                 {
                                     GRNId = GRN.GRNId,
                                     ProductCode = product.ProductCode,
                                     ProductName = product.ProductName,
                                     OrderQty = poDetails.OrderQty,
                                     ReplaceQty = GRN.ReplaceQty,
                                     GSTPercentage = poDetails.GSTPercentage,
                                     Price = poDetails.Price,
                                     Discount = poDetails.Discount,
                                     DiscountAs = poDetails.DiscountAs,
                                     SerialNoApplicable = GRN.SerialNoApplicable,
                                     PurchaseOrderDetailsID = GRN.PurchaseOrderDetailsID,
                                     ManuFacturingDate = GRN.ManufacturingDate,
                                     ExpiryDate = GRN.ExpiryDate,
                                     BatchNo = GRN.BatchNo
                                 }).ToList();

                    result.AddRange(query);
        
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }


    }
}