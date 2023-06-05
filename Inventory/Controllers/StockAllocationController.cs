using Inventory.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class StockAllocationController : Controller
    {
        private InventoryModel db = new InventoryModel();
        public ActionResult Index()
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
                    cmd.CommandText = "SP_GetStockAllocation";
                    cnn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    var orderList = new List<ODetails>();
                    while (dr.Read())
                    {
                        ODetails order = new ODetails();
                        order.OrderNo = dr["OrderNo"].ToString();
                        order.OrderDate = dr["OrderDate"].ToString();
                        order.DeliverTo = dr["DeliverTo"].ToString();
                        order.CustomerName = dr["CustomerName"].ToString();

                        orderList.Add(order);
                    }
                    cnn.Close();

                    ViewBag.datasource = orderList;
                }
                catch (Exception EX)
                {
                    var result = EX.Message;
                    return Json(result, JsonRequestBehavior.AllowGet);
                    cnn.Close();
                }
            }
            catch (Exception EX)
            {
                return View();
            }
            return View();
        }

        public ActionResult Create(string id)
        {
            ViewBag.OrderNo = id;
            return View();
        }

        public ActionResult GetOrder(string OrderNo)
        {
            try
            {
                string cnnString = System.Configuration.ConfigurationManager.ConnectionStrings["InventoryModel"].ConnectionString;
                SqlConnection cnn = new SqlConnection(cnnString);
                SqlCommand cmd = new SqlCommand();

                cmd.Connection = cnn;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetStockAllocationOne";
                cmd.Parameters.AddWithValue("@OrderNo", OrderNo);
                cnn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                var orderList = new List<ODetails>();
                var orderListManufacture = new List<ODetails>();
                while (dr.Read())
                {
                    ODetails order = new ODetails();
                    order.OrderDetailsID = dr["OrderDetailsID"].ToString();
                    order.OrderNo = dr["OrderNo"].ToString();
                    order.OrderQty = dr["OrderQty"].ToString();
                    order.ProductName = dr["ProductName"].ToString();
                    order.AvailableQty = dr["AvailableQty"].ToString();
                    order.AllocatedQty = dr["Allocated"].ToString();
                    order.ProductClass= dr["ProductClass"].ToString();
                    if (order.ProductClass == "Manufacture")
                        orderListManufacture.Add(order);
                    else
                        orderList.Add(order);
                }
                cnn.Close();

                var result = new { Message = "success", orderList = orderList, orderListManufacture= orderListManufacture };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception EX)
            {
                var result = new { Message = EX.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ValidateStockAllocationIndex()
        {
            try
            {
                var Prod = db.StockAllocation.Select(a=>a.SONO).ToList();
                var result = new { Message = "success", Prod = Prod };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception EX)
            {
                var result = new { Message = EX.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        

        [HttpPost]
        public ActionResult AllocateStock(List<ODetails> oDetails)
        {
            try
            {
                string OrderDetailsIDs = "",  OrderNo="";
                List<StockAllocation> stockList = new List<StockAllocation>();
                if (oDetails != null)
                {
                    foreach (var temp in oDetails)
                    {
                        if (temp.Allocate == null || temp.Allocate == "")
                        {
                            ;
                        }
                        else
                        {
                            if (OrderDetailsIDs == "")
                                OrderDetailsIDs = temp.OrderDetailsID;
                            else
                                OrderDetailsIDs = OrderDetailsIDs+"," + temp.OrderDetailsID;

                            OrderNo = temp.OrderNo;
                            var OrderDetailsID = Convert.ToInt32(temp.OrderDetailsID);
                            var Product = db.orderDetails.Where(x => x.OrderDetailsID == OrderDetailsID).Select(x => new { x.ProductCode }).FirstOrDefault();

                            StockAllocation stock = new StockAllocation();
                            var ExistingStockAllocation = db.StockAllocation.Where(x => x.SONO == temp.OrderNo && x.Item == Product.ProductCode).FirstOrDefault();
                            if (ExistingStockAllocation == null)
                            {
                                stock.SONO = temp.OrderNo;
                                stock.OrderDetailsID = OrderDetailsID;
                                stock.Item = Product.ProductCode;
                                stock.Quantity = Convert.ToInt32(temp.Allocate);
                                stock.Shortage = Convert.ToInt32(temp.Shortage);
                                stock.CreatedBy = User.Identity.Name;
                                stock.CreatedDate = DateTime.Now;
                                stock.IsActive = false;

                                stockList.Add(stock);
                            }
                            else
                            {
                                var Q = ExistingStockAllocation == null ? 0 : ExistingStockAllocation.Quantity;
                                ExistingStockAllocation.Quantity = Q + Convert.ToInt32(temp.Allocate);
                                db.SaveChanges();
                            }
                            var ProdMaster = db.Products.Where(x => x.ProductCode == Product.ProductCode).FirstOrDefault();
                            decimal AllcatedQ = 0;
                            if (ProdMaster.AllocatedQty == null)
                                AllcatedQ = 0;
                            else
                                AllcatedQ = Convert.ToDecimal(ProdMaster.AllocatedQty);

                            ProdMaster.AllocatedQty = AllcatedQ + Convert.ToDecimal(temp.Allocate);
                            db.SaveChanges();                            
                        }
                    }

                    if (stockList.Count > 0)
                    {
                        db.StockAllocation.AddRange(stockList);
                        db.SaveChanges();
                    }
                    string cnnString = System.Configuration.ConfigurationManager.ConnectionStrings["InventoryModel"].ConnectionString;
                    SqlConnection cnn = new SqlConnection(cnnString);
                    SqlCommand cmd = new SqlCommand();
                    try
                    {
                        cmd.Connection = cnn;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.CommandText = "SP_AutoGeneratedPOFromStockAllocation";
                        cmd.Parameters.AddWithValue("@OrderDetailsIDs", OrderDetailsIDs);
                        cmd.Parameters.AddWithValue("@CreatedBy", User.Identity.Name);
                        cmd.Parameters.AddWithValue("@OrderNO", OrderNo);
                        cnn.Open();
                        string response = cmd.ExecuteScalar().ToString();
                        cnn.Close();
                    }
                    catch (Exception EX)
                    {
                        ;
                    }
                }
                var result = new { Message = "success" };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch(Exception EX)
            {
                var result = new { Message = EX.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GenerateProdOrder(List<ODetails> oDetails)
        {
            try
            {
                string OrderDetailsIDs = "", OrderNo = "";
                List<StockAllocation> stockList = new List<StockAllocation>();
                if (oDetails != null)
                {
                    foreach (var temp in oDetails)
                    {
                        if (temp.Allocate == null || temp.Allocate == "")
                        {
                            ;
                        }
                        else
                        {
                            if (OrderDetailsIDs == "")
                                OrderDetailsIDs = temp.OrderDetailsID;
                            else
                                OrderDetailsIDs = OrderDetailsIDs + "," + temp.OrderDetailsID;

                            OrderNo = temp.OrderNo;
                            var OrderDetailsID = Convert.ToInt32(temp.OrderDetailsID);
                            var Product = db.orderDetails.Where(x => x.OrderDetailsID == OrderDetailsID).Select(x => new { x.ProductCode }).FirstOrDefault();

                            StockAllocation stock = new StockAllocation();
                            var ExistingStockAllocation = db.StockAllocation.Where(x => x.SONO == temp.OrderNo && x.Item == Product.ProductCode).FirstOrDefault();
                            if (ExistingStockAllocation == null)
                            {
                                stock.SONO = temp.OrderNo;
                                stock.OrderDetailsID = OrderDetailsID;
                                stock.Item = Product.ProductCode;
                                stock.Quantity = Convert.ToInt32(temp.Allocate);
                                stock.Shortage = Convert.ToInt32(temp.Shortage);
                                stock.CreatedBy = User.Identity.Name;
                                stock.CreatedDate = DateTime.Now;
                                stock.IsActive = false;

                                stockList.Add(stock);
                            }
                            else
                            {
                                var Q = ExistingStockAllocation == null ? 0 : ExistingStockAllocation.Quantity;
                                ExistingStockAllocation.Quantity = Q + Convert.ToInt32(temp.Allocate);
                                db.SaveChanges();
                            }
                            var ProdMaster = db.Products.Where(x => x.ProductCode == Product.ProductCode).FirstOrDefault();
                            decimal AllcatedQ = 0;
                            if (ProdMaster.AllocatedQty == null)
                                AllcatedQ = 0;
                            else
                                AllcatedQ = Convert.ToDecimal(ProdMaster.AllocatedQty);

                            ProdMaster.AllocatedQty = AllcatedQ + Convert.ToDecimal(temp.Allocate);
                            db.SaveChanges();
                        }
                    }

                    if (stockList.Count > 0)
                    {
                        db.StockAllocation.AddRange(stockList);
                        db.SaveChanges();
                    }

                    string cnnString = System.Configuration.ConfigurationManager.ConnectionStrings["InventoryModel"].ConnectionString;
                    SqlConnection cnn = new SqlConnection(cnnString);
                    SqlCommand cmd = new SqlCommand();
                    try
                    {
                        cmd.Connection = cnn;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.CommandText = "SP_CreateProductionOrder";
                        cmd.Parameters.AddWithValue("@OrderDetailsIDs", OrderDetailsIDs);
                        cmd.Parameters.AddWithValue("@CreatedBy", User.Identity.Name);
                        cmd.Parameters.AddWithValue("@OrderNO", OrderNo);
                        cnn.Open();
                        string response = cmd.ExecuteScalar().ToString();
                        cnn.Close();
                    }
                    catch (Exception EX)
                    {
                        ;
                    }
                }
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

    public class ODetails
    {
        public string OrderNo { get; set; }
        public string OrderDate { get; set; }
        public string DeliverTo { get; set; }
        public string CustomerName { get; set; }
        public string OrderQty { get; set; }
        public string AvailableQty { get; set; }
        public string ProductName { get; set; }
        public string OrderDetailsID { get; set; }
        public string Allocate { get; set; }
        public string AllocatedQty { get; set; }
        public string ProductClass { get; set; }
        public string Shortage { get; set; }
    }
}
