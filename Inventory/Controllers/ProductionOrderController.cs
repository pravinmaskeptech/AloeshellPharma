using Inventory.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Syncfusion.EJ.Export;
using Syncfusion.JavaScript.Models;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
namespace Inventory.Controllers
{
    public class ProductionOrderController : Controller
    {
        InventoryModel db = new InventoryModel();       

        public ActionResult GenerateProdOrderList()
        {
            ViewBag.datasource = (from prodorder in db.ProductionOrder.Where(x=>x.Status==null)
                                  join
                                      order in db.orderMain on prodorder.OrderNo equals order.OrderNo into Om
                                  from Ordermain in Om.DefaultIfEmpty()
                                  join cust in db.Customers on Ordermain.CustomerID equals cust.CustomerID into c
                                  from customer in c.DefaultIfEmpty()
                                  orderby prodorder.ProductionOrderID descending
                                  select new { ProductionOrderID = prodorder.ProductionOrderID, Status=prodorder.Status, CustomerName = customer.CustomerName, Date = prodorder.Date, RequiredDate = prodorder.RequiredDate }).ToList();
            return View();
        }

        public ActionResult PendingIssueToProductionList() 
        {
            var MRND = db.MRNDetails.Where(a=>a.PoGenerated==true).GroupBy(x => x.MRNNo).Select(x => x.Key).ToList();
            // ViewBag.datasource = db.POMains.Where(x => MRND.Contains(x.MRNNO)).GroupBy(A=>A.MRNNO)
            //   var Prod = (from stock in db.poDetails.Where(x => MRND.Contains(x.MRNNO) &&(x.OrderQty-x.ReceivedQty)>0)
            var Prod = (from stock in db.poDetails.Where(x => MRND.Contains(x.MRNNO))
                             
                              orderby stock.SalesOrderNo descending
                              select new { MRNNO = stock.MRNNO, SalesOrderNo = stock.SalesOrderNo, OrderQty = stock == null ? 0 : stock.OrderQty, IssueToProductionQty = stock == null ? 0 : stock.IssueToProductionQty, } into p 
                              group p by new { p.MRNNO } into g 
                              select new
                              {
                                  MRNNO = g.Select(x => x.MRNNO).FirstOrDefault(),

                                  SalesOrderNo = g.Select(x => x.SalesOrderNo).FirstOrDefault(),
                                  IssueToProductionQty = g.Sum(x => x.IssueToProductionQty),
                                  OrderQty = g.Sum(x => x.OrderQty),
                                  //ClosingQuantity = g.Sum(x => x.ClosingQuantity),
                                  //Component = g.Select(x => x.Component).FirstOrDefault()
                              }
                             
                            ).ToList();
            ViewBag.datasource = Prod.Where(a => a.OrderQty != a.IssueToProductionQty).ToList();

            return View();
        }

        public ActionResult IssuedProductionList() 
        {
            var MRND = db.MRNDetails.Where(a => a.PoGenerated == true).GroupBy(x => x.MRNNo).Select(x => x.Key).ToList();
            // ViewBag.datasource = db.POMains.Where(x => MRND.Contains(x.MRNNO)).GroupBy(A=>A.MRNNO)
            //   var Prod = (from stock in db.poDetails.Where(x => MRND.Contains(x.MRNNO) &&(x.OrderQty-x.ReceivedQty)>0)

            ViewBag.datasource = db.IssueToProduction.OrderByDescending(a => a.ID).ToList();

            return View();
        }




        public ActionResult IssueToProduction(string id)  
        {
            ViewBag.MRNNO = id;
            var OrderDetails = db.ProductionOrder.Where(a => a.MRNNO == id).ToList();
            var OrderNo = "";
            var count = 1;
            foreach(var x in OrderDetails)
            {
                if(OrderDetails.Count==1)
                {
                    OrderNo = x.OrderNo;
                }
                else
                {
                    OrderNo = OrderNo + "" + x.OrderNo + ",";
                }
               
            }
            ViewBag.OrderNo = OrderNo;
           var Order=  db.BillNumbering.Where(a => a.Type == "IssueToProduction").Select(a => a.Number).SingleOrDefault();
            ViewBag.IssueToProduction = "Issue_No_" + Order;

            return View();

            
        }

        [ValidateInput(false)]
        public void GenerateProdOrderExportToExcel(string GridModel)
        {
            ExcelExport exp = new ExcelExport();
            var DataSource = (from prodorder in db.ProductionOrder.Where(x => x.Status == null)
                              join
                                  order in db.orderMain on prodorder.OrderNo equals order.OrderNo into Om
                              from Ordermain in Om.DefaultIfEmpty()
                              join cust in db.Customers on Ordermain.CustomerID equals cust.CustomerID into c
                              from customer in c.DefaultIfEmpty()
                              orderby prodorder.ProductionOrderID descending
                              select new { ProductionOrderID = prodorder.ProductionOrderID, Status = prodorder.Status, CustomerName = customer.CustomerName, Date = prodorder.Date, RequiredDate = prodorder.RequiredDate }).ToList();
            GridProperties obj = ConvertGridObject(GridModel);
            exp.Export(obj, DataSource, "ProductionOrder.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }

        public ActionResult GenerateProdOrder(string id)
        {
            ViewBag.Id = id;
            return View();
        }

        public JsonResult SaveIssueToProductions(List<IssueToProductionDetails> orderItem)
        {
            var billNo = db.BillNumbering.Where(a => a.Type == "IssueToProduction").FirstOrDefault();
            billNo.Number = Convert.ToInt32(billNo.Number) + 1;
            var status = false;
            int cnt = 1;
          
                try
                {
                if (orderItem.Count > 0)
                {
                    int code = 0;                   
                    using (var dbcxtransaction = db.Database.BeginTransaction())
                    {
                        foreach (var x in orderItem)
                        {
                            if (cnt == 1)
                            {
                                try
                                {
                                    IssueToProduction main = new IssueToProduction();
                                    main.IssueToProductionNo = x.IssueToProductionNo;
                                    main.SalesOderNo = x.SalesOderNo;
                                    main.MRNNO = x.MRNNO;

                                    main.CreatedBy = User.Identity.Name;
                                    main.CreatedDate = DateTime.Today;

                                    db.IssueToProduction.Add(main);
                                    db.SaveChanges();
                                    code = db.IssueToProduction.Max(a => a.ID);
                                }
                                catch (Exception ee)
                                {
                                    var result2 = new { Message = ee.Message };
                                    return Json(result2, JsonRequestBehavior.AllowGet);
                                }

                            }
                            try
                            {
                                cnt = cnt + 1;
                                IssueToProductionDetails details = new IssueToProductionDetails();
                                details.IssueToProductionID = code;
                                details.Product = x.Product;
                                details.ProductQty = x.ProductQty;
                                details.ProductComponent = x.ProductComponent;
                                details.ComponentQty = x.ComponentQty;
                                details.GRNQty = x.GRNQty;
                                details.IssuedQty = x.IssuedQty;
                                details.IssueToProductionQty = x.IssueToProductionQty;
                                details.ProdCode = x.ProdCode;
                                details.MainProductCode = x.MainProductCode;
                                details.IssueToProductionNo = x.IssueToProductionNo;
                                details.MRNNO = x.MRNNO;
                                details.SalesOderNo = x.SalesOderNo;
                                details.CreatedBy = User.Identity.Name;
                                details.ManufactureGRNQty = 0;
                                details.CreatedDate = DateTime.Today;
                                db.IssueToProductionDetails.Add(details);                              

                                var PODetails = db.poDetails.Where(a => a.MRNNO == x.MRNNO && a.ProductCode == x.ProdCode).FirstOrDefault();
                                PODetails.IssueToProductionQty = PODetails.IssueToProductionQty + x.IssueToProductionQty;
                                
                                var Products = db.Products.Where(a => a.ProductCode == x.ProdCode).FirstOrDefault();
                                Products.IssuedToProductionQty = Products.IssuedToProductionQty + x.IssueToProductionQty;
                                Products.ClosingQuantity = Products.ClosingQuantity- x.IssueToProductionQty; ;
                                Products.OutwardQuantity = Products.OutwardQuantity + x.IssueToProductionQty; ;

                            }
                            catch (Exception er)
                            {
                               
                                var result1 = new { Message = er.Message };
                                return Json(result1, JsonRequestBehavior.AllowGet);
                            }

                        }
                        db.SaveChanges();
                        dbcxtransaction.Commit();

                        var result = new { Message = "success" };
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
                }
                catch (Exception er) 
                {
                var result = new { Message = er.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            
            return new JsonResult { Data = new { status } };
        }
        public ActionResult GetData(string OrderNo)
        {
            try
            {
                var oRDER = db.ProductionOrder.Where(x => x.ProductionOrderID == OrderNo).Select(x => new { x.OrderNo }).FirstOrDefault();

                var stockStatus = (from stock in db.StockStatus.Where(x => x.ProductionOrderID == OrderNo)
                                   join StAllocation in db.StockAllocation.Where(x => x.SONO == oRDER.OrderNo) on stock.Product equals StAllocation.Item into ST
                                   from stockAllocation in ST.DefaultIfEmpty()
                                   join product in db.Products on stock.Product equals product.ProductCode into Product
                                   from products in Product.DefaultIfEmpty()
                                   select new { ProductionOrderID = stock.ProductionOrderID,
                                       Product = products == null ? string.Empty : products.ProductName,
                                       OrderQty = stock == null ? 0 : stock.OrderQty,
                                       AllocatedQty = stockAllocation == null ? 0 : stockAllocation.Quantity,
                                       RequiredQty = stockAllocation == null ? 0 : stockAllocation.Shortage,
                                      

                                   }).ToList();

                //var stockStatus = (from stock in db.StockStatus.Where(x => x.ProductionOrderID == OrderNo)
                //                   join p in db.ProductionOrder on stock.ProductionOrderID equals p.ProductionOrderID into pod
                //                   from production in pod.DefaultIfEmpty()
                //                   join product in db.Products on stock.Product equals product.ProductCode into Product
                //                   from products in Product.DefaultIfEmpty()
                //                   join StAllocation in db.StockAllocation.Where(x => x.SONO != oRDER.OrderNo) on stock.Product equals StAllocation.Item into ST
                //                   from stockAllocation in ST.DefaultIfEmpty()
                //                   orderby stock.ProductionOrderID descending
                //                   select new { ProductionOrderID = stock.ProductionOrderID, AvailableQty = products.ClosingQuantity, Product = products.ProductName, Qty = stock.Qty, OrderQty = stock.OrderQty, AllocatedQty = stockAllocation == null ? 0 : stockAllocation.Quantity } into p
                //                   group p by new { p.Product } into g
                //                   select new {
                //                       ProductionOrderID = g.Select(x => x.ProductionOrderID).FirstOrDefault(),
                //                       AvailableQty = g.Select(x => x.AvailableQty).FirstOrDefault(),
                //                       Product = g.Select(x => x.Product).FirstOrDefault(),
                //                       Qty = g.Select(x => x.Qty).FirstOrDefault(),
                //                       OrderQty = g.Select(x => x.OrderQty).FirstOrDefault(),
                //                       AllocatedQty = g.Sum(x => x.AllocatedQty),
                //                   }
                //                   ).ToList();

                //var stockStatus = (from stock in db.StockStatus.Where(x => x.ProductionOrderID == OrderNo)
                //                   join p in db.ProductionOrder on stock.ProductionOrderID equals p.ProductionOrderID into pod
                //                   from production in pod.DefaultIfEmpty()
                //                   join product in db.Products on stock.Product equals product.ProductCode into Product
                //                   from products in Product.DefaultIfEmpty()
                //                   join StAllocation in db.StockAllocation.Where(x => x.SONO == oRDER.OrderNo) on stock.Product equals StAllocation.Item into ST
                //                   from stockAllocation in ST.DefaultIfEmpty()
                //                   orderby stock.ProductionOrderID descending
                //                   select new { ProductionOrderID = stock.ProductionOrderID, AvailableQty = products.ClosingQuantity, Product = products.ProductName, Qty = stock.Qty, OrderQty = stock.OrderQty, AllocatedQty = stockAllocation == null ? 0 : stockAllocation.Quantity } into p
                //                   group p by new { p.Product } into g
                //                   select new
                //                   {
                //                       ProductionOrderID = g.Select(x => x.ProductionOrderID).FirstOrDefault(),
                //                       AvailableQty = g.Select(x => x.AvailableQty).FirstOrDefault(),
                //                       Product = g.Select(x => x.Product).FirstOrDefault(),
                //                       Qty = g.Select(x => x.Qty).FirstOrDefault(),
                //                       OrderQty = g.Select(x => x.OrderQty).FirstOrDefault(),
                //                       AllocatedQty = g.Sum(x => x.AllocatedQty),
                //                   }
                //                   ).ToList();

                var Prod = db.ProductionOrder.Where(x => x.ProductionOrderID == OrderNo).FirstOrDefault();
                var result = new { Message = "success", stockStatus = stockStatus, Prod = Prod };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception EX)
            {
                var result = new { Message = EX.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SaveProdOrder(string OrderNo, DateTime RequiredDate)
        {
            try
            {
                var Productionorder = db.ProductionOrder.Where(x => x.ProductionOrderID == OrderNo).FirstOrDefault();                
                Productionorder.RequiredDate = RequiredDate;
                Productionorder.UpdatedBy = User.Identity.Name;
                Productionorder.UpdatedDate = DateTime.Now;
                db.SaveChanges();
                var result = "success";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception EX)
            {
                var result = EX.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ProductionOrderList()
        {
            ViewBag.datasource = (from prodorder in db.ProductionOrder.Where(x=>x.RequiredDate!=null)
                                  join
                                      order in db.orderMain on prodorder.OrderNo equals order.OrderNo into Om
                                  from Ordermain in Om.DefaultIfEmpty()
                                  join cust in db.Customers on Ordermain.CustomerID equals cust.CustomerID into c
                                  from customer in c.DefaultIfEmpty()
                                  orderby prodorder.ProductionOrderID descending
                                  select new { ProductionOrderID = prodorder.ProductionOrderID, Status = prodorder.Status, CustomerName = customer.CustomerName, Date = prodorder.Date, RequiredDate = prodorder.RequiredDate }).ToList();
            return View();
        }

        public ActionResult GetStatusProdOrder()
        {
            try
            {
                var Prod = (from prodorder in db.ProductionOrder.Where(x => x.RequiredDate != null)
                            join
                                order in db.orderMain on prodorder.OrderNo equals order.OrderNo into Om
                            from Ordermain in Om.DefaultIfEmpty()
                            join cust in db.Customers on Ordermain.CustomerID equals cust.CustomerID into c
                            from customer in c.DefaultIfEmpty()
                            orderby prodorder.ProductionOrderID descending
                            select new { ProductionOrderID = prodorder.ProductionOrderID, Status = prodorder.Status, CustomerName = customer.CustomerName, Date = prodorder.Date, RequiredDate = prodorder.RequiredDate }).ToList();
                var result = new { Message = "success", Prod = Prod };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception EX)
            {
                var result = new { Message = EX.Message};
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [ValidateInput(false)]
        public void ProdOrderExportToExcel(string GridModel)
        {
            ExcelExport exp = new ExcelExport();
            var DataSource = (from prodorder in db.ProductionOrder.Where(x => x.RequiredDate != null)
                              join
                                  order in db.orderMain on prodorder.OrderNo equals order.OrderNo into Om
                              from Ordermain in Om.DefaultIfEmpty()
                              join cust in db.Customers on Ordermain.CustomerID equals cust.CustomerID into c
                              from customer in c.DefaultIfEmpty()
                              orderby prodorder.ProductionOrderID descending
                              select new { ProductionOrderID = prodorder.ProductionOrderID, Status = prodorder.Status, CustomerName = customer.CustomerName, Date = prodorder.Date, RequiredDate = prodorder.RequiredDate }).ToList();
            GridProperties obj = ConvertGridObject(GridModel);
            exp.Export(obj, DataSource, "ProductionOrder.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }      
        public ActionResult GetOrderDetails(string OrderNo)
        {
            try
            {
                var Prod = (from stock in db.StockStatus
                            join product in db.Products on stock.Product equals product.ProductCode into Product
                            from products in Product.DefaultIfEmpty()
                            orderby stock.ProductionOrderID descending
                            select new { ProductionOrderID = stock.ProductionOrderID, Product = products.ProductName, Qty = stock.Qty }).ToList();
                var result = new { Message = "success", Prod = Prod };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception EX)
            {
                var result = new { Message = EX.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Edit(string id)
        {
            ViewBag.Id = id;            
            return View();
        }
        

        public ActionResult Approve(string OrderNo, string Status, string Reason, DateTime RequiredDate)
        {
            try
            {
                var Productionorder = db.ProductionOrder.Where(x => x.ProductionOrderID == OrderNo).FirstOrDefault();
                Productionorder.Status = Status;
                Productionorder.Reason = Reason;
                Productionorder.RequiredDate = RequiredDate;
                Productionorder.UpdatedBy = User.Identity.Name;
                Productionorder.UpdatedDate = DateTime.Now;
                db.SaveChanges();
                var result = "success";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception EX)
            {
                var result = EX.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ProdDeptList()
        {
            ViewBag.datasource = db.ProductionOrder.Where(x=>x.Status=="Approve").ToList();
            return View();
        }

        //GENERATE MRN
        public ActionResult GenerateMRN(string id)
        {
            var count = db.BillNumbering.Where(a => a.Type == "MRNNo").Select(a => a.Number).Single();
            ViewBag.Code = "MRNNo_" + count;
            ViewBag.Id = id;
            return View();
        }
        public ActionResult GetApprovedProd(string OrderNo)
        {
            try
            {
                string[] Temp = OrderNo.Split(',');

                var Prod1 = (from stock in db.StockStatus.Where(x => Temp.Contains(x.ProductionOrderID))
                            join ExBOM in db.ExplodedBOM on stock.Product equals ExBOM.FinishGoods into exBOM
                            from bom in exBOM.DefaultIfEmpty()
                            join prod1 in db.Products on bom.ComponentId equals prod1.ProductCode into p1
                            from product1 in p1.DefaultIfEmpty()
                            join prod in db.Products on bom.ComponentId equals prod.ProductCode into p
                            from product in p.DefaultIfEmpty()
                            orderby stock.Product descending
                            select new { product = product.ProductName, Qty = stock.Qty, ItemReq = product1 == null ? product.ProductName : product1.ProductName, ReqQty = bom == null ? stock.Qty : bom.Quantity, Component = bom == null ? "NO" : "YES" }).ToList();

                var Prod = (from stock in db.StockStatus.Where(x => Temp.Contains(x.ProductionOrderID))
                            join ExBOM in db.ExplodedBOM on stock.Product equals ExBOM.FinishGoods into exBOM
                            from bom in exBOM.DefaultIfEmpty()
                            join prod1 in db.Products on bom.ComponentId equals prod1.ProductCode into p1
                            from product1 in p1.DefaultIfEmpty()
                            join prod in db.Products on bom.ComponentId equals prod.ProductCode into p
                            from product in p.DefaultIfEmpty()
                            orderby stock.Product descending
                            select new {product = product.ProductName, Qty = stock.Qty, ItemReq = product1 == null ? product.ProductName : product1.ProductName, ReqQty = bom == null ? stock.Qty : bom.Quantity, Component = bom == null ? "NO" : "YES", ClosingQuantity = product == null ? 0 : product.ClosingQuantity, } into p
                            group p by new {p.ItemReq } into g
                            select new { product = g.Select(x => x.product).FirstOrDefault(), Qty = g.Sum(x => x.Qty), ItemReq = g.Select(x => x.ItemReq).FirstOrDefault(),                                
                                ReqQty = g.Sum(x => x.ReqQty),
                                ClosingQuantity = g.Sum(x => x.ClosingQuantity),
                                Component = g.Select(x => x.Component).FirstOrDefault() }
                            ).ToList();
                            
                var result = new { Message = "success", Prod = Prod, Date=DateTime.Now };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception EX)
            {
                var result = new { Message = EX.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SaveMRN(string OrderNo, string MRNNO)
        {

            try
            {
                string[] Temp = OrderNo.Split(',');

                var Prod = (from stock in db.StockStatus.Where(x => Temp.Contains(x.ProductionOrderID))
                            join ExBOM in db.ExplodedBOM on stock.Product equals ExBOM.FinishGoods into exBOM
                            from bom in exBOM.DefaultIfEmpty()

                             
                            join prPrds in db.Products on bom.ProductId equals prPrds.ProductCode into ProductCodes
                            from prd in ProductCodes.DefaultIfEmpty() 


                            orderby stock.Product descending
                            select new { ProductionOrderNo= stock.ProductionOrderID, product = stock.Product, Qty = stock.Qty, OrderQty= stock.OrderQty, ItemReq = bom == null ? stock.Product : bom.ComponentId, ReqQty = bom == null ? stock.Qty : bom.Quantity, Component = bom == null ? "NO" : "YES" , ClosingQuantity = prd == null ? 0 : prd.ClosingQuantity, } into p
                            group p by new { p.product, p.ItemReq } into g
                            select new { ProductionOrderNo= g.Select(x => x.ProductionOrderNo).FirstOrDefault(), product = g.Select(x => x.product).FirstOrDefault(), Qty = g.Sum(x => x.Qty), OrderQty = g.Sum(x => x.OrderQty), ItemReq = g.Select(x => x.ItemReq).FirstOrDefault(), ReqQty = g.Sum(x => x.ReqQty), ClosingQuantity = g.Sum(x => x.ClosingQuantity), Component = g.Select(x => x.Component).FirstOrDefault() }
                            ).ToList();

                MRNMain MRN = new MRNMain();
                MRN.MRNNo = MRNNO;
                MRN.MRNDate = DateTime.Now;
                //MRN.ProductionOrderId = OrderNo;
                MRN.CreatedBy = User.Identity.Name;
                MRN.CreatedDate = DateTime.Now;

                db.MRNMain.Add(MRN);

                foreach (var p in Prod)
                {
                    int AvailableQty = Convert.ToInt32(p.Qty);
                    int OrderQty = Convert.ToInt32(p.ReqQty);
                    decimal? RequiredQty = 0;
                    if (p.Component == "YES")
                        RequiredQty = OrderQty * AvailableQty;
                    else
                        RequiredQty = OrderQty;

                    var ClosingQuantity = p.ClosingQuantity;

                    //if (p.product == p.ItemReq)
                    //    RequiredQty = p.OrderQty;

                    db.MRNDetails.Add(new MRNDetails
                    {
                        MRNNo=MRNNO,
                        OrderedProduct=p.product,
                        OrderedQty= p.Qty,
                        RequiredItems=p.ItemReq,
                        RequiredQty = RequiredQty,
                        CurrentAvailableStock = Convert.ToInt32( ClosingQuantity),
                        PoGenerated =false,
                        CreatedBy=User.Identity.Name,
                        CreatedDate=DateTime.Now,
                        ProductionOrderNo=p.ProductionOrderNo
                    });
                }                                              

                var BillNumber = db.BillNumbering.Where(a => a.Type == "MRNNo").FirstOrDefault();
                BillNumber.Number = BillNumber.Number + 1;

                db.SaveChanges();

                for (int i = 0; i < Temp.Count(); i++)
                {
                    OrderNo = Temp[i];
                    var ProdOrder = db.ProductionOrder.Where(x => x.ProductionOrderID == OrderNo).FirstOrDefault();
                    ProdOrder.Status = "MRN Generated";
                    ProdOrder.UpdatedBy = User.Identity.Name;
                    ProdOrder.UpdatedDate = DateTime.Now;
                    ProdOrder.MRNNO = MRNNO;
                    db.SaveChanges();
                }  
                var result = "success";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception EX)
            {
                var result = new { Message = EX.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        //SHORTAGE
        public ActionResult Shortage()
        {
            //var MRND = db.MRNDetails.Where(x => x.PoGenerated !=true).GroupBy(x => x.MRNNo).Select(x => x.Key).ToList();
            //ViewBag.datasource = (from MRN in db.MRNMain.Where(x => MRND.Contains(x.MRNNo))
            //                      join POrder in db.ProductionOrder on MRN.MRNNo equals POrder.MRNNO into order
            //                      from ProdOrder in order.DefaultIfEmpty()
            //                      orderby MRN.MRNNo descending
            //                      select new { MRNNo = MRN.MRNNo, ProductionOrderId = ProdOrder.ProductionOrderID, MRNDate = MRN.MRNDate }).ToList(); //db.MRNMain.Where(x => x.Status != "PO Generated").ToList();
            var MRND = db.MRNDetails.Where(a=>a.PoGenerated !=true).GroupBy(x => x.MRNNo).Select(x => x.Key).ToList();
            ViewBag.datasource = db.MRNMain.Where(x => MRND.Contains(x.MRNNo)).ToList();
            return View();
        }

        public ActionResult GetStatusShortage()
        {
            try
            {

                var MRND = db.MRNDetails.Where(a=>a.PoGenerated==true).GroupBy(x => x.MRNNo).Select(x => x.Key).ToList();
                var Prod =  db.MRNMain.Where(x => MRND.Contains(x.MRNNo)).ToList();

               
                var result = new { Message = "success", Prod = Prod };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception EX)
            {
                var result = new { Message = EX.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        [ValidateInput(false)]
        public void MRNDetailsExcelExport(string GridModel)
        {
            ExcelExport exp = new ExcelExport();
            var MRND = db.MRNDetails.Where(x => x.PoGenerated != true).GroupBy(x => x.MRNNo).Select(x => x.Key).ToList();
            var DataSource = db.MRNMain.Where(x => MRND.Contains(x.MRNNo)).ToList(); //db.MRNMain.Where(x => x.Status != "PO Generated").ToList();
            GridProperties obj = ConvertGridObject(GridModel);
            exp.Export(obj, DataSource, "Shortage.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
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

        public ActionResult CheckAvailability(string id)
        {
            ViewBag.MRNNO = id;
            return View();
        }
        

             public ActionResult GetIssueToProduction(string MRNNO) 
        {
            try
            {
                // join StAllocation in db.StockAllocation.Where(x => x.SONO == oRDER.OrderNo) on stock.Product equals StAllocation.Item into ST        
                var Prod = (from pod in db.poDetails.Where(x => x.MRNNO == MRNNO) 

                            join prod in db.Products on pod.ProductCode equals prod.ProductCode into p
                            from product in p.DefaultIfEmpty()


                            join MRNDetails in db.MRNDetails.Where(x => x.MRNNo == MRNNO) on pod.ProductCode equals MRNDetails.RequiredItems into ST
                            from mrn in ST.DefaultIfEmpty()

                            join prods in db.Products on mrn.OrderedProduct equals prods.ProductCode into pp
                            from mainprd in pp.DefaultIfEmpty()


                            orderby pod.PurchaseOrderID descending
                            select new { PurchaseOrderID = pod.PurchaseOrderID, ProductCode = pod.ProductCode, ProductName = product.ProductName, MAinProduct  = mainprd.ProductName, MainProductCode = mrn.OrderedProduct, RequiredQty = mrn.RequiredQty, ReceivedQty = pod == null ? 0 : pod.ReceivedQty, OrderQty = pod == null ? 0 : pod.OrderQty, IssueToProductionQty = pod == null ? 0 : pod.IssueToProductionQty, MainOrderQty = mrn == null ? 0 : mrn.OrderedQty, } 
                           
                           ).ToList();

                var result = new { Message = "success", Prod = Prod };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception EX)
            {
                var result = new { Message = EX.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetAvailability(string MRNNO)
        {
            try
            {
                var POmain = db.POMains.Where(x => x.POStatus == "Approve").Select(x => x.PurchaseOrderNo).ToList();
                //var Prod = (from MRN in db.MRNDetails.Where(x => x.MRNNo == MRNNO && x.PoGenerated != true)
                //            join prod in db.Products on MRN.RequiredItems equals prod.ProductCode into p
                //            from product in p.DefaultIfEmpty()
                //            join StockAllocatn in db.StockAllocation.GroupBy(x=>x.Item).Select(x=>new { Quantity= x.Sum(b=>b.Quantity), Item =x.Key}) on product.ProductCode equals StockAllocatn.Item into sta
                //            from stockAllocation in sta.DefaultIfEmpty()
                //            join PODetail in db.poDetails.Where(x => (x.OrderQty - x.ReceivedQty) > 0 && POmain.Contains(x.PONO)).GroupBy(x => x.ProductCode).Select(x => new { OrderQty = x.Sum(b => b.OrderQty), ReceivedQty = x.Sum(b => b.ReceivedQty), ProductCode = x.Key }) on MRN.RequiredItems equals PODetail.ProductCode into POD
                //            from PoDetails in POD.DefaultIfEmpty()
                //            orderby MRN.ID descending
                //            select new {ID=MRN.ID, ProductCode = MRN.RequiredItems, ProductName = product.ProductName, RequiredQty = MRN.RequiredQty, AvailableQty = product.ClosingQuantity, AllocatedStock= stockAllocation.Quantity, OrderQty =PoDetails==null?0: PoDetails.OrderQty, ReceivedQty =PoDetails==null?0: PoDetails.ReceivedQty } into p
                //            group p by p.ProductCode into g
                //            select new { ID = g.Select(x => x.ID).FirstOrDefault(), AllocatedStock= g.Select(x => x.AllocatedStock).FirstOrDefault(), ProductName = g.Select(x => x.ProductName).FirstOrDefault(), RequiredQty = g.Sum(x => x.RequiredQty), AvailableQty = g.Sum(x => x.AvailableQty), OrderQty = g.Sum(x => x.OrderQty), RecievedQty = g.Sum(x => x.ReceivedQty) }
                //            ).ToList();



                // Change by hanu
                //var Prod = (from MRN in db.MRNDetails.Where(x => x.MRNNo == MRNNO && x.PoGenerated != true)
                //            join prod in db.Products on MRN.RequiredItems equals prod.ProductCode into p
                //            from product in p.DefaultIfEmpty()
                //            join StockAllocatn in db.StockAllocation.GroupBy(x => x.Item).Select(x => new { Quantity = x.Sum(b => b.Quantity), Item = x.Key }) on product.ProductCode equals StockAllocatn.Item into sta
                //            from stockAllocation in sta.DefaultIfEmpty()
                //            join PODetail in db.poDetails.Where(x => (x.OrderQty - x.ReceivedQty) > 0 && POmain.Contains(x.PONO)).GroupBy(x => x.ProductCode).Select(x => new { OrderQty = x.Sum(b => b.OrderQty), ReceivedQty = x.Sum(b => b.ReceivedQty), ProductCode = x.Key }) on MRN.RequiredItems equals PODetail.ProductCode into POD
                //            from PoDetails in POD.DefaultIfEmpty()
                //            orderby MRN.ID descending
                //            select new { ID = MRN.ID, ProductCode = MRN.RequiredItems, ProductName = product.ProductName, RequiredQty = MRN.RequiredQty, AllocatedStock = stockAllocation.Quantity, OrderQty = PoDetails == null ? 0 : PoDetails.OrderQty, ReceivedQty = PoDetails == null ? 0 : PoDetails.ReceivedQty } into p
                //            group p by p.ProductCode into g
                //            select new { ID = g.Select(x => x.ID).FirstOrDefault(), AllocatedStock = g.Select(x => x.AllocatedStock).FirstOrDefault(), ProductName = g.Select(x => x.ProductName).FirstOrDefault(), RequiredQty = g.Sum(x => x.RequiredQty), OrderQty = g.Sum(x => x.OrderQty), RecievedQty = g.Sum(x => x.ReceivedQty) }
                //            ).ToList();


                var Prod = (from MRN in db.MRNDetails.Where(x => x.MRNNo == MRNNO )
                            join prod in db.Products on MRN.RequiredItems equals prod.ProductCode into p
                            from product in p.DefaultIfEmpty()
                            join StockAllocatn in db.StockAllocation.GroupBy(x => x.Item).Select(x => new { Quantity = x.Sum(b => b.Quantity), Item = x.Key }) on product.ProductCode equals StockAllocatn.Item into sta
                            from stockAllocation in sta.DefaultIfEmpty()
                            join PODetail in db.poDetails.Where(x => (x.OrderQty - x.ReceivedQty) > 0 && POmain.Contains(x.PONO)).GroupBy(x => x.ProductCode).Select(x => new { OrderQty = x.Sum(b => b.OrderQty), ReceivedQty = x.Sum(b => b.ReceivedQty), ProductCode = x.Key }) on MRN.RequiredItems equals PODetail.ProductCode into POD
                            from PoDetails in POD.DefaultIfEmpty()
                            orderby MRN.ID descending
                            select new { ID = MRN.ID, ProductCode = MRN.RequiredItems, ProductName = product.ProductName, RequiredQty = MRN.RequiredQty, AllocatedStock = stockAllocation.Quantity, OrderQty = PoDetails == null ? 0 : PoDetails.OrderQty, ReceivedQty = PoDetails == null ? 0 : PoDetails.ReceivedQty, PoGenerated=MRN.PoGenerated } into p
                            group p by p.ProductCode into g
                            select new { ID = g.Select(x => x.ID).FirstOrDefault(), AllocatedStock = g.Select(x => x.AllocatedStock).FirstOrDefault(), ProductName = g.Select(x => x.ProductName).FirstOrDefault(), RequiredQty = g.Sum(x => x.RequiredQty), OrderQty = g.Sum(x => x.OrderQty), RecievedQty = g.Sum(x => x.ReceivedQty), PoGenerated = g.Select(x => x.PoGenerated).FirstOrDefault() } 
                           ).ToList();

                var result = new { Message = "success", Prod = Prod };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception EX)
            {
                var result = new { Message = EX.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GeneratePO(string MRNNO,string MainMRNNo)
        {
            try
            {
                string cnnString = System.Configuration.ConfigurationManager.ConnectionStrings["InventoryModel"].ConnectionString;
                SqlConnection cnn = new SqlConnection(cnnString);
                SqlCommand cmd = new SqlCommand();
                try
                {
                    cmd.Connection = cnn;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "SP_AutoGeneratedPO";
                    cmd.Parameters.AddWithValue("@MRNNO", MRNNO);
                    cmd.Parameters.AddWithValue("@MainMRNNo", MainMRNNo);
                    cmd.Parameters.AddWithValue("@CreatedBy", User.Identity.Name);
                    cnn.Open();
                    string result = cmd.ExecuteScalar().ToString();
                    cnn.Close();

                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                catch (Exception EX)
                {
                    var result=EX.Message;
                    return Json(result, JsonRequestBehavior.AllowGet);
                    cnn.Close();
                }
            }
            catch (Exception EX)
            {
                var result = EX.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult NewProductionOrder()
        {
            var BillNumbering = db.BillNumbering.Where(x => x.Type == "ProductionOrderID").FirstOrDefault();
            if (BillNumbering == null)
            {
                ViewBag.ProdOrderNo = 1;
            }
            else
            {
                int? BillNo=BillNumbering.Number + 1;
                ViewBag.ProdOrderNo = "PrdOrderId_" + BillNo.ToString();
            }
            return View();
        }

        public ActionResult FillProduct()
        {
            try
            {
                var Product = db.Products.Select(x => new { x.ProductCode, x.ProductName }).ToList();
                var result = new { Message = "success", Product = Product };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception EX)
            {
                var result = new { Message = EX.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Create(ProductionOrder productionOrder)
        {
            using (var dx = db.Database.BeginTransaction())
            {
                try
                {
                    productionOrder.Date = DateTime.Now;
                    productionOrder.CreatedBy = User.Identity.Name;
                    productionOrder.CreatedDate = DateTime.Now;

                    db.ProductionOrder.Add(productionOrder);
                    db.SaveChanges();

                    foreach (var stock in productionOrder.StockStatus)
                    {
                        db.StockStatus.Add(new StockStatus
                        {
                            ProductionOrderID = productionOrder.ProductionOrderID,
                            Product=stock.Product,
                            OrderQty=stock.OrderQty,
                            Qty=stock.OrderQty,
                            CreatedBy=User.Identity.Name,
                            CreatedDate = DateTime.Now
                        });
                    }

                    var BillNumbering = db.BillNumbering.Where(x => x.Type == "ProductionOrderID").FirstOrDefault();
                    BillNumbering.Number = BillNumbering.Number + 1;

                    db.SaveChanges();
                    dx.Commit();
                    var result = new { Message = "success" };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                catch (Exception EX)
                {
                    var result = new { Message = EX.Message };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
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
        public JsonResult InvoicePrint(int ID) 
        {
            try
            {
               
                int Count = 1;
                string CustomerAddress = "", CustomerName = "", OrderDate = "";
               

                iTextSharp.text.Font font5 = iTextSharp.text.FontFactory.GetFont(FontFactory.HELVETICA, 9);
                iTextSharp.text.Font font10 = iTextSharp.text.FontFactory.GetFont(FontFactory.HELVETICA, 11);

                DateTime dt = DateTime.Now;
                var aa = dt.ToString("HH");
                var bb = dt.ToString("mm");
                var cc = dt.ToString("ss");

                Document document = new Document(PageSize.A4, 5f, 5f, 5f, 5f);
                string path = Server.MapPath("~/Reports/IssuedToProduction/");
                string filename1 = path + "" + ID + "_" + aa + bb + cc + ".pdf";
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filename1, FileMode.Create));
                document.Open();
                Session["fileName1"] = filename1;

                PdfPTable table = new PdfPTable(6);
                float[] widths = new float[] { 1.2f, 4f, 2f, 4f, 2f, 0.0f };
                table.SetWidths(widths);
                table.WidthPercentage = 96;

                PdfPCell p1 = new PdfPCell(new Phrase(new Phrase("Sr. No.", FontFactory.GetFont("Century Gothic", 10, BaseColor.WHITE))));
                p1.HorizontalAlignment = 1;
                p1.BackgroundColor = BaseColor.BLACK;
                table.AddCell(p1);


                PdfPCell p2 = new PdfPCell(new Phrase(new Phrase("Procuct", FontFactory.GetFont("Century Gothic", 10, BaseColor.WHITE))));
                p2.HorizontalAlignment = 1;
                p2.BackgroundColor = BaseColor.BLACK;
                table.AddCell(p2);



                PdfPCell pp4 = new PdfPCell(new Phrase(new Phrase("Qty", FontFactory.GetFont("Century Gothic", 10, BaseColor.WHITE))));
                pp4.HorizontalAlignment = 1;
                pp4.BackgroundColor = BaseColor.BLACK;
                table.AddCell(pp4);

                PdfPCell p2010 = new PdfPCell(new Phrase(new Phrase("Component Name", FontFactory.GetFont("Century Gothic", 10, BaseColor.WHITE))));
                p2010.HorizontalAlignment = 1;
                p2010.BackgroundColor = BaseColor.BLACK;
                table.AddCell(p2010);
                

                PdfPCell pp2124 = new PdfPCell(new Phrase(new Phrase("Issued Qty", FontFactory.GetFont("Century Gothic", 10, BaseColor.WHITE))));
                pp2124.HorizontalAlignment = 1;
                pp2124.BackgroundColor = BaseColor.BLACK;
                table.AddCell(pp2124);


                PdfPCell p7d21 = new PdfPCell(new Phrase(new Phrase("", FontFactory.GetFont("Century Gothic", 10, BaseColor.WHITE))));
                p7d21.HorizontalAlignment = 0;
                table.AddCell(p7d21);

                PdfPTable table1 = new PdfPTable(7);
                float[] width1 = new float[] { 0.0f, 1.2f, 4f, 2f, 4f, 2f, 0.0f };
                table1.SetWidths(width1);
                table1.WidthPercentage = 96;
                table1.DefaultCell.Padding = 0;

                int cnt = 1;
                var data = db.IssueToProductionDetails.Where(a => a.IssueToProductionID == ID).OrderBy(a=>a.Product).ToList();
                foreach (var r in data)
                {
                    
                        Paragraph pr931 = new Paragraph();
                        pr931.Add(new Phrase("", FontFactory.GetFont("Century Gothic", 9)));
                        PdfPCell prr931 = new PdfPCell(pr931);
                        prr931.HorizontalAlignment = 1;
                        table1.AddCell(prr931);

                        Paragraph pr31 = new Paragraph();
                        pr31.Add(new Phrase("" + Count + "\n", FontFactory.GetFont("Century Gothic", 9)));
                        PdfPCell prr31 = new PdfPCell(pr31);
                        prr31.HorizontalAlignment = 1;
                        prr31.UseAscender = true;
                        table1.AddCell(prr31);


                    Paragraph pr32 = new Paragraph();
                    pr32.Add(new Phrase("" + r.Product + "", FontFactory.GetFont("Century Gothic", 9)));
                    PdfPCell prr32 = new PdfPCell(pr32);
                    prr32.HorizontalAlignment = 0;
                    table1.AddCell(prr32);


                    Paragraph pr34 = new Paragraph();
                    pr34.Add(new Phrase("" + r.ProductQty + "", FontFactory.GetFont("Century Gothic", 9)));
                    PdfPCell prr34 = new PdfPCell(pr34);
                    prr34.HorizontalAlignment = 1;
                    table1.AddCell(prr34);


                    Paragraph pr3542 = new Paragraph();
                    pr3542.Add(new Phrase("" + r.ProductComponent + "", FontFactory.GetFont("Century Gothic", 9)));
                        PdfPCell prr3552 = new PdfPCell(pr3542);
                    prr3552.HorizontalAlignment = 0;
                        table1.AddCell(prr3552);


                        Paragraph pr3454 = new Paragraph();
                    pr3454.Add(new Phrase("" + r.IssueToProductionQty + "", FontFactory.GetFont("Century Gothic", 9)));
                        PdfPCell prr324 = new PdfPCell(pr3454);
                    prr324.HorizontalAlignment = 1;
                        table1.AddCell(prr324);

                        Paragraph pr36g44 = new Paragraph();
                        pr36g44.Add(new Phrase("", FontFactory.GetFont("Century Gothic", 9)));
                        PdfPCell prr36g44 = new PdfPCell(pr36g44);
                        prr36g44.HorizontalAlignment = 0;
                        prr36g44.Border = Rectangle.RIGHT_BORDER;
                        table1.AddCell(prr36g44);
                        Count++;
                    }
                


                PdfPTable table4 = new PdfPTable(2);
                float[] widths5 = new float[] { 10f, 6f };
                table4.SetWidths(widths5);
                table4.WidthPercentage = 95;
                table4.HorizontalAlignment = 1;

                string imageURL = Server.MapPath("/Photo/MicraftLogo.png");
                iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(imageURL);
                jpg.ScaleToFit(80, 50);
                jpg.SpacingBefore = 5f;
                jpg.SpacingAfter = 1f;
                jpg.Alignment = Element.ALIGN_LEFT;

                Paragraph p44 = new Paragraph();
                p44.Add(new Phrase());
                PdfPCell c11144 = new PdfPCell(jpg);
                c11144.Border = Rectangle.NO_BORDER;
                table4.AddCell(c11144);

                Paragraph p4 = new Paragraph();
                p4.Add(new Phrase("\nMicraft Solutions", FontFactory.GetFont("Century Gothic", 13, Font.BOLD)));
                p4.Add(new Phrase("\nOffice No.02, Wing-A, PristoonWood, New \nDP Rd, Pimple Nilakh, Pune, Maharashtra 411027\nPH : 098501 00030    Email : listen@micraft.com\nWebsite : www.micraft.com ", FontFactory.GetFont("Century Gothic", 9)));
                // p4.Add(new Phrase("\nInorbvict Healthcare India Private Limited\nOffice No.02, Wing-A, PristoonWood, New \nDP Rd, Pimple Nilakh, Pune, Maharashtra 411027\n098501 00030\n", FontFactory.GetFont("Century Gothic", 9, Font.BOLD)));
                PdfPCell c1114 = new PdfPCell(p4);
                c1114.HorizontalAlignment = 0;
                c1114.Border = Rectangle.NO_BORDER;
                table4.AddCell(c1114);
                var TermsAndCondition = "";
                //if (DocType == "Domestic")
                //{
                //    TermsAndCondition = db.TermsAndConditions.Where(a => a.Orders == "Delivery Challan (Domestic)").Select(a => a.TermsAndCondition).Single();
                //}


                PdfPTable table7 = new PdfPTable(4);
                float[] width7 = new float[] { 5.2f, 2f, 0.0f, 0.0f };
                table7.SetWidths(width7);
                table7.WidthPercentage = 96;
                table7.HorizontalAlignment = 1;

                Paragraph pr185 = new Paragraph();
                pr185.Add(new Phrase("   \n\n", FontFactory.GetFont("Century Gothic", 10, Font.BOLD)));
                pr185.Add(new Phrase("", FontFactory.GetFont("Century Gothic", 8)));
                PdfPCell pc185 = new PdfPCell(pr185);
                pc185.HorizontalAlignment = 0;
                pc185.Border = Rectangle.LEFT_BORDER;
                table7.AddCell(pc185);



                Paragraph pr285 = new Paragraph();
                pr285.Add(new Phrase("For Micraft Solutions  \n\n\n\n", FontFactory.GetFont("Century Gothic", 8, Font.BOLD)));
                pr285.Add(new Phrase("                                      Authorised Signatory", FontFactory.GetFont("Century Gothic", 7, Font.BOLD)));
                PdfPCell pc285 = new PdfPCell(pr285);
                pc285.HorizontalAlignment = 1;
                pc285.Colspan = 3;
                pc285.Border = Rectangle.LEFT_BORDER;
                table7.AddCell(pc285);


                Paragraph pr185789 = new Paragraph();
                pr185789.Add(new Phrase("\n", FontFactory.GetFont("Century Gothic", 10, Font.BOLD)));
                pr185789.Add(new Phrase("" , FontFactory.GetFont("Century Gothic", 8)));
                PdfPCell pc185897 = new PdfPCell(pr185789);
                pc185897.HorizontalAlignment = 0;

                pc185897.Colspan = 4;
                table7.AddCell(pc185897);


                //PdfPTable table7 = new PdfPTable(4);
                //float[] width7 = new float[] { 5.2f, 2f, 0.0f, 0.0f };
                //table7.SetWidths(width7);
                //table7.WidthPercentage = 96;
                //table7.HorizontalAlignment = 1;

                //Paragraph pr185 = new Paragraph();
                //pr185.Add(new Phrase("   \n\n", FontFactory.GetFont("Century Gothic", 10, Font.BOLD)));
                //pr185.Add(new Phrase("", FontFactory.GetFont("Century Gothic", 8)));
                //PdfPCell pc185 = new PdfPCell(pr185);
                //pc185.HorizontalAlignment = 0;
                //pc185.Border = Rectangle.LEFT_BORDER;
                //table8.AddCell(pc185);



                //Paragraph pr285 = new Paragraph();
                //pr285.Add(new Phrase("For Inorbvict Healthcare  \n\n\n\n", FontFactory.GetFont("Century Gothic", 8, Font.BOLD)));
                //pr285.Add(new Phrase("                                      Authorised Signatory", FontFactory.GetFont("Century Gothic", 7, Font.BOLD)));
                //PdfPCell pc285 = new PdfPCell(pr285);
                //pc285.HorizontalAlignment = 1;
                //pc285.Border = Rectangle.LEFT_BORDER;
                //table8.AddCell(pc285);


                //Paragraph pr185789 = new Paragraph();
                //pr185789.Add(new Phrase("TermsAndCondition :  \n", FontFactory.GetFont("Century Gothic", 10, Font.BOLD)));
                //pr185789.Add(new Phrase("" + TermsAndCondition, FontFactory.GetFont("Century Gothic", 8)));
                //PdfPCell pc185897 = new PdfPCell(pr185789);
                //pc185897.HorizontalAlignment = 0;

                //pc185897.Colspan = 4;
                //table8.AddCell(pc185897);


                //PdfPTable table5 = new PdfPTable(4);
                //float[] width5 = new float[] { 0.5f, 7f, 1f, 7f };
                //table5.SetWidths(width5);
                //table5.WidthPercentage = 96;
                //table5.HorizontalAlignment = 1;

                //PdfPCell pc2 = new PdfPCell();
                //pc2.HorizontalAlignment = 0;
                //pc2.Border = Rectangle.NO_BORDER;
                //table5.AddCell(pc2);

                //Paragraph pr1 = new Paragraph();
                //pr1.Add(new Phrase("Billed to : ", FontFactory.GetFont("Century Gothic", 9, Font.BOLD)));
                //pr1.Add(new Phrase(CustomerName, FontFactory.GetFont("Century Gothic", 9)));
                //pr1.Add(new Phrase("\n" + CustomerAddress, FontFactory.GetFont("Century Gothic", 9)));
                //PdfPCell pc1 = new PdfPCell(pr1);
                //pc1.HorizontalAlignment = 0;
                //pc1.FixedHeight = 50f;
                //pc1.Border = Rectangle.NO_BORDER;
                //table5.AddCell(pc1);


                //PdfPCell pc53 = new PdfPCell();
                //pc53.HorizontalAlignment = 0;
                //pc53.Border = Rectangle.NO_BORDER;
                //table5.AddCell(pc53);

                //Paragraph pr3 = new Paragraph();
                //pr3.Add(new Phrase("Ship To : ", FontFactory.GetFont("Century Gothic", 9, Font.BOLD)));
                //pr3.Add(new Phrase(CustomerName, FontFactory.GetFont("Century Gothic", 9)));
                //pr3.Add(new Phrase("\n" + CustomerAddress, FontFactory.GetFont("Century Gothic", 9)));
                //PdfPCell pc3 = new PdfPCell(pr3);
                //pc3.HorizontalAlignment = 0;
                //pc3.Border = Rectangle.NO_BORDER;
                //table5.AddCell(pc3);



                PdfPTable table9 = new PdfPTable(1);
                float[] width9 = new float[] { 20f };
                table9.SetWidths(width9);
                table9.WidthPercentage = 96;

                Paragraph pr226 = new Paragraph();
                pr226.Add(new Phrase("Date:" + DateTime.Now.ToString("dd/mm/yyyy") + "\n", FontFactory.GetFont("Century Gothic", 8, Font.BOLD)));

                PdfPCell p79013 = new PdfPCell();
                p79013.HorizontalAlignment = 0;
                table9.AddCell(p79013);


                PdfPTable table3 = new PdfPTable(3);
                float[] widths55 = new float[] { 2f, 6f, 10 };
                table3.SetWidths(widths55);
                table3.WidthPercentage = 95;
                table3.HorizontalAlignment = 1;


                Paragraph pr53 = new Paragraph();
                pr53.Add(new Phrase("\n\n   Issue No\n", FontFactory.GetFont("Century Gothic", 9)));
                pr53.Add(new Phrase("   Date", FontFactory.GetFont("Century Gothic", 9)));

                PdfPCell c112 = new PdfPCell(pr53);
                c112.Border = Rectangle.TOP_BORDER;
                c112.FixedHeight = 50f;
                c112.HorizontalAlignment = 0;
                table3.AddCell(c112);

                
                var IssueToProduction = db.IssueToProduction.Where(a => a.ID == ID).FirstOrDefault();

                DateTime dtt = Convert.ToDateTime( IssueToProduction.CreatedDate);
               var DueDate = dtt.ToString("dd/MM/yyyy");
                Paragraph pr539 = new Paragraph();
                pr539.Add(new Phrase("\n\n: " + IssueToProduction.IssueToProductionNo + "\n", FontFactory.GetFont("Century Gothic", 9)));
                pr539.Add(new Phrase(": " + DueDate + "", FontFactory.GetFont("Century Gothic", 9))); 

                PdfPCell c1129 = new PdfPCell(pr539);
                c1129.Border = Rectangle.TOP_BORDER;
                c1129.FixedHeight = 50f;
                c1129.HorizontalAlignment = 0;
                table3.AddCell(c1129);

                Paragraph p49 = new Paragraph();
                p49.Add(new Phrase("ISSUED TO PRODUCTION", FontFactory.GetFont("Century Gothic", 11, Font.BOLD)));
                PdfPCell c1115 = new PdfPCell(p49);
                c1115.HorizontalAlignment = 0;
                c1115.Border = Rectangle.TOP_BORDER;
                table3.AddCell(c1115);

                PdfPTable table6 = new PdfPTable(3);
                float[] width6 = new float[] { 5f, 7f, 4f };
                table6.SetWidths(width6);
                table6.WidthPercentage = 96;
                table6.HorizontalAlignment = 1;

                Paragraph pr18 = new Paragraph();
                pr18.Add(new Phrase("_", FontFactory.GetFont("Century Gothic", 1)));

                PdfPCell pc18 = new PdfPCell(pr18);
                pc18.HorizontalAlignment = 0;
                pc18.Border = Rectangle.BOTTOM_BORDER;
                table6.AddCell(pc18);

                Paragraph pr28 = new Paragraph();
                pr28.Add(new Phrase(".", FontFactory.GetFont("Century Gothic", 1)));
                PdfPCell pc28 = new PdfPCell(pr28);
                pc28.HorizontalAlignment = 1;
                pc28.Border = Rectangle.BOTTOM_BORDER;
                table6.AddCell(pc28);

                Paragraph pr38 = new Paragraph();
                pr38.Add(new Phrase("", FontFactory.GetFont("Century Gothic", 1)));
                PdfPCell pc38 = new PdfPCell(pr38);
                pc38.HorizontalAlignment = 1;
                pc38.Border = Rectangle.BOTTOM_BORDER;
                table6.AddCell(pc38);

                Paragraph pr388 = new Paragraph();
                pr388.Add(new Phrase("", FontFactory.GetFont("Century Gothic", 1)));
                PdfPCell pc388 = new PdfPCell(pr388);
                pc388.HorizontalAlignment = 1;
                pc388.Border = Rectangle.BOTTOM_BORDER;
                table6.AddCell(pc388);

                document.Add(table4);
                document.Add(table3);
                document.Add(table6);
                //document.Add(table5);
                document.Add(table);
                document.Add(table1);
                document.Add(table7);
                //   document.Add(table8);

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
    }
}
