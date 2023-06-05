using Inventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class SOReplacementController : Controller
    {
        InventoryModel db = new InventoryModel();
        public ActionResult Index()
        {
            var test = (from Sale in db.Sales
                        where Sale.ReplaceQty > 0
                        join OrderMmain in db.orderMain on Sale.OrderNo equals OrderMmain.OrderNo into SO
                        from so in SO.DefaultIfEmpty()
                        join Prod in db.Products on Sale.ProductCode equals Prod.ProductCode into p
                        from Product in p.DefaultIfEmpty()
                        orderby Sale.SalesId descending
                        select new { SalesId = Sale.SalesId, InvoiceNo = Sale.InvoiceNo, InvoiceDate = Sale.InvoiceDate, OrderNo = Sale.OrderNo, OrderDate = Sale.SODate, ProductName = Product.ProductName }
                                    ).ToList();
            ViewBag.datasource = (from Sale in db.Sales
                                  where Sale.ReplaceQty > 0
                                  join OrderMmain in db.orderMain on Sale.OrderNo equals OrderMmain.OrderNo into SO
                                  from so in SO.DefaultIfEmpty()
                                  join Prod in db.Products on Sale.ProductCode equals Prod.ProductCode into p
                                  from Product in p.DefaultIfEmpty()
                                  orderby Sale.SalesId descending
                                  select new { SalesId = Sale.SalesId, InvoiceNo = Sale.InvoiceNo, InvoiceDate = Sale.InvoiceDate, OrderNo = Sale.OrderNo, OrderDate = Sale.SODate, ProductName = Product.ProductName }
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

            var Sale = db.Sales.Where(x => x.SalesId == Id).Select(x => new { x.InvoiceNo, x.OrderNo, x.CustomerID }).FirstOrDefault();
            ViewBag.InvoiceNo = Sale.InvoiceNo;
            ViewBag.SONO = Sale.OrderNo;
            var customername = db.orderMain.Where(a => a.OrderNo == Sale.OrderNo).FirstOrDefault();

            //     var Customer = db.Customers.Where(x => x.CustomerID == Sale.CustomerID).Select(x => new { x.CustomerName}).FirstOrDefault();
            ViewBag.Customer = customername.CustomerName;
            ViewBag.CustomerId = customername.CustomerID;
            var count = db.BillNumbering.Where(x => x.Type == "NewInv").FirstOrDefault();
            var Number = count.Number;
            ViewBag.NewInvoiceNo = "NewInv_" + Number;

            return View();
        }

        [HttpPost]
        public ActionResult Edit()
        {
            return View();
        }

        public ActionResult GetData(string InvoiceNo)
        {
            //var result = (from Sale in db.Sales
            //              where Sale.ReplaceQty > 0 && Sale.InvoiceNo == InvoiceNo
            //              join orderdetails in db.orderDetails on Sale.OrderDetailsID equals orderdetails.OrderDetailsID into order
            //              from OrderD in order.DefaultIfEmpty()

            //              join product in db.Products on Sale.ProductCode equals product.ProductCode into p
            //              from prod in p.DefaultIfEmpty()
            //              select new { OrderDetailsID = OrderD.OrderDetailsID, SalesId = Sale.SalesId, orderQty = OrderD.OrderQty, InvoiceNo = Sale.InvoiceNo, OrderNo = Sale.OrderNo, ProductName = prod.ProductName, ProductCode = Sale.ProductCode, CustomerID = Sale.CustomerID, BatchNo = Sale.BatchNo, ReplaceQty = Sale.ReplaceQty, GSTPercentage = OrderD.GSTPercentage, Price = OrderD.Price, Discount = OrderD.Discount, DiscountAs = OrderD.DiscountAs, SerialNoApplicable = Sale.SerialNoApplicable, ClosingQuantity = prod.ClosingQuantity }).ToList();

            var result = (from Sale in db.Sales
                          where Sale.ReplaceQty > 0 && Sale.InvoiceNo == InvoiceNo

                          join orderdetails in db.orderDetails on Sale.OrderNo equals orderdetails.OrderNo into order
                          from OrderD in order.Where(a => a.ProductCode == Sale.ProductCode).DefaultIfEmpty()

                          join product in db.Products on Sale.ProductCode equals product.ProductCode into p
                          from prod in p.DefaultIfEmpty()
                          select new
                          {
                              OrderDetailsID = OrderD.OrderDetailsID,
                              SalesId = Sale.SalesId,
                              orderQty = OrderD.OrderQty,
                              InvoiceNo = Sale.InvoiceNo,
                              OrderNo = Sale.OrderNo,
                              ProductName = prod.ProductName,
                              ProductCode = Sale.ProductCode,
                              CustomerID = Sale.CustomerID,
                              //   CustomerID = Sale == null ? 0 : Sale.CustomerID,
                              BatchNo = Sale.BatchNo,
                              ReplaceQty = Sale.ReplaceQty,
                              GSTPercentage = OrderD.GSTPercentage,
                              Price = OrderD.Price,
                              Discount = OrderD.Discount,
                              DiscountAs = OrderD.DiscountAs,
                              SerialNoApplicable = Sale.SerialNoApplicable,
                              ClosingQuantity = prod.ClosingQuantity
                          }).ToList();

            

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult save(List<Sales> OrderDetails)
        {
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
                                var order = db.orderDetails.Where(a => a.OrderDetailsID == x.OrderDetailsID).Select(a => new { a.OrderID}).FirstOrDefault();
                                var orderMain = db.orderMain.Where(a => a.OrderID == order.OrderID).Select(a => new { a.OrderDate}).FirstOrDefault();
                                //order.DeliveredQty = (Convert.ToDecimal(order.DeliveredQty) + Convert.ToDecimal(x.DeliveredQty));

                                var prd = db.Products.Where(a => a.ProductCode == x.ProductCode).FirstOrDefault();
                                prd.ClosingQuantity = (Convert.ToDecimal(prd.ClosingQuantity) - Convert.ToDecimal(x.DeliveredQty));
                                prd.OutwardQuantity = (Convert.ToDecimal(prd.OutwardQuantity) + Convert.ToDecimal(x.DeliveredQty));

                                List<TempTable> tmp = db.tempTable.Where(a => a.OrderDetailsID == x.OrderDetailsID).ToList();
                                foreach (var t in tmp)
                                {
                                    var grn = db.GRNDetail.Where(a => a.GRNId == t.GRNId).FirstOrDefault();
                                    grn.SalesQty = grn.SalesQty + t.SalesQty;
                                }


                                List<TempSalesSerialNo> tmpserialNo = db.tempSalesSerialNo.Where(a => a.ProductCode == x.ProductCode && a.InvoiceNo==x.InvoiceNo).ToList();

                                var OldSales = db.Sales.Where(s => s.SalesId == x.SalesId).FirstOrDefault();

                                Sales S = new Sales();
                                S.InvoiceNo = x.InvoiceNo;
                                S.InvoiceDate = x.InvoiceDate;
                                S.OrderDetailsID = x.OrderDetailsID;
                                S.OrderNo = x.OrderNo;
                                S.SODate = Convert.ToDateTime(orderMain.OrderDate);
                                S.ProductCode = x.ProductCode;
                                S.DeliveredQty = x.DeliveredQty;
                                S.CreatedBy = User.Identity.Name;
                                S.BatchNo = x.BatchNo;                               
                                S.ReturnQty = 0;
                                S.SerialNoApplicable = x.SerialNoApplicable;
                                S.CustomerID = x.CustomerID;
                                S.CreatedDate = DateTime.Today;

                                S.BasicRate = OldSales.BasicRate;
                                S.DiscountAs = OldSales.DiscountAs;
                                S.Discount = OldSales.Discount;
                                S.GSTPercentage = OldSales.GSTPercentage;
                                S.CGSTAmount = OldSales.CGSTAmount;
                                S.IGSTAmount = OldSales.IGSTAmount;
                                S.SGSTAmount = OldSales.SGSTAmount;
                                S.AmountPerUnit = OldSales.AmountPerUnit;
                                S.CompanyID = Convert.ToInt32(Session["CompanyID"]);

                                db.Sales.Add(S);

                                SOReplacement soreplacment = new SOReplacement();
                                
                                soreplacment.ReplacementQty = OldSales.ReplaceQty;
                                int ReplaceQty = Convert.ToInt32(OldSales.ReplaceQty);
                                ReplaceQty = ReplaceQty - Convert.ToInt32(x.DeliveredQty);
                                OldSales.ReplaceQty = ReplaceQty;

                                
                                soreplacment.InvNo = OldSales.InvoiceNo;
                                soreplacment.InvDate = OldSales.InvoiceDate ?? DateTime.MinValue; ;
                                soreplacment.OrderNo = OldSales.OrderNo;
                                soreplacment.OrderDate = OldSales.SODate;
                                soreplacment.ReplacementQty = x.DeliveredQty;
                                soreplacment.NewInvNo = x.InvoiceNo;
                                soreplacment.CompanyID = Convert.ToInt32(Session["CompanyID"]);
                                soreplacment.NewInvDate = x.InvoiceDate ?? DateTime.MinValue; ;
                                soreplacment.ProductCode = x.ProductCode;
                                soreplacment.IsActive = true;
                                soreplacment.CreatedBy = User.Identity.Name;
                                soreplacment.CreatedDate = DateTime.Now;
                                db.SOReplacement.Add(soreplacment);

                                db.SaveChanges();
                                message = "success";


                                foreach (var tm in tmpserialNo.Where(a=>a.InvoiceNo==x.InvoiceNo && a.ProductCode==x.ProductCode))
                                {
                                    var prdSerialNo = db.ProductSerialNo.Where(a => a.SerialNoId == tm.SerialNoId).FirstOrDefault();
                                    prdSerialNo.Status = tm.Status;
                                    prdSerialNo.InvoiceNo = tm.InvoiceNo;




                                    var grn = db.GRNDetail.Where(a => a.ProductCode == tm.ProductCode && a.BatchNo == prdSerialNo.BatchNo && a.WarehouseID == tm.WarehouseId && a.StoreLocationId == tm.StoreLocationId).FirstOrDefault();
                                    grn.SalesQty = grn.SalesQty + 1;


                                    var returns= db.sOReturns.Where(a => a.InvoiceNo == x.InvoiceNo && a.ProductCode == x.ProductCode).FirstOrDefault();
                                    if (returns != null) {
                                        returns.ReplaceWithSRNo = tm.SerialNo;
                                    }
                                    
                                }

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
                            if (message == "success")
                            {
                                var BillNumbers = db.BillNumbering.Where(b=> b.Type == "NewInv").FirstOrDefault();
                                int number = Convert.ToInt32(BillNumbers.Number) + 1;
                                BillNumbers.Number = number;
                                db.SaveChanges();
                            }
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
	}
}