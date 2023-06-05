using Inventory.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class GetQuotationsController : Controller
    {
        private InventoryModel db = new InventoryModel();
        // GET: GetQuotations
        public ActionResult Index()
        {
            ViewBag.datasource = db.PRNMains.Where(a => a.Status == "Approve" && a.ShortListed != true).OrderByDescending(a=>a.PRNID).ToList();
            return View();
        }
        public ActionResult SendQuotations()  
        {
            ViewBag.datasource = db.PRNMains.Where(a => a.Status == "Approve" && a.ShortListed != true).OrderByDescending(a=>a.PRNID).ToList();
            return View();
        }
        
        public ActionResult GenrateQuotationIndex() 
        {
            ViewBag.datasource = db.PRNMains.Where(a => a.Status == "Approve" && a.ShortListed == true).ToList();
            return View();
        }


        public ActionResult GetSuppliers(int? id) 
        {
            ViewBag.Dept = db.Departments.OrderBy(a => a.DeptName).ToList();
            ViewBag.products = db.Products.OrderBy(a => a.ProductName).ToList();
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            PRNMain pRNMain = db.PRNMains.Find(id);
            if (pRNMain == null)
            {
                return HttpNotFound();
            }
            return View(pRNMain);
        }
        public ActionResult Edit(int? id)
        {
            ViewBag.Dept = db.Departments.OrderBy(a => a.DeptName).ToList();
            ViewBag.products = db.Products.OrderBy(a => a.ProductName).ToList();
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            PRNMain pRNMain = db.PRNMains.Find(id);
            if (pRNMain == null)
            {
                return HttpNotFound();
            }
            return View(pRNMain);
        }

        
        public JsonResult GETPRNData(int ID)
        {
            try
            {
                var PRN = db.PRNMains.Where(a => a.PRNID == ID).FirstOrDefault();

                string conn = System.Configuration.ConfigurationManager.ConnectionStrings["InventoryModel"].ConnectionString;
                SqlConnection cnn = new SqlConnection(conn);
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = cnn;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetQuotationDetails";
                cmd.Parameters.AddWithValue("@ID", ID);
                cnn.Open();

                DataTable dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                cnn.Close();
                cmd.Dispose();
                List<Quotations> Quot = new List<Quotations>();
                foreach (DataRow dr in dt.Rows)
                {
                    Quotations t = new Quotations();
                    t.ProductCode = dr["ProductCode"].ToString();
                    t.SupplierproductrelationID = dr["SupplierproductrelationID"].ToString();
                    t.ProductName = dr["ProductName"].ToString();
                    t.ProductPrice = dr["ProductPrice"].ToString();
                    t.DeliveryInDays = dr["DeliveryInDays"].ToString();
                    t.SupplierName = dr["SupplierName"].ToString();

                    Quot.Add(t);
                }
                var result = new { Message = "success", PRN, DTL= Quot };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            catch (Exception Ex)
            {
                var result = new { Message = Ex.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GETPRNSupplierDetails(int ID) 
        {
            try
            {
                var PRN = db.PRNMains.Where(a => a.PRNID == ID).FirstOrDefault();

                string conn = System.Configuration.ConfigurationManager.ConnectionStrings["InventoryModel"].ConnectionString;
                SqlConnection cnn = new SqlConnection(conn);
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = cnn;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetOrderSuppliers";
                cmd.Parameters.AddWithValue("@ID", ID);
                cnn.Open();

                DataTable dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                cnn.Close();
                cmd.Dispose();
                List<Quotations> Quot = new List<Quotations>();
                foreach (DataRow dr in dt.Rows)
                {
                    Quotations t = new Quotations();
                  
                    t.SupplierID = dr["SupplierID"].ToString();
                    t.SupplierName = dr["SupplierName"].ToString();

                    Quot.Add(t);
                }
                var result = new { Message = "success", PRN, DTL = Quot };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            catch (Exception Ex)
            {
                var result = new { Message = Ex.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GenrateQuotations(List<PRNDetails> OrderDetails)  
        {
            try
            {
                var PRNNO = OrderDetails[0].PRNNo;
                if (OrderDetails.Count > 0)
                {
                    using (var dbcxtransaction = db.Database.BeginTransaction())
                    {
                        foreach (var x in OrderDetails.OrderBy(a => a.SupplierName))
                        {
                            var prn = db.PRNDetails.Where(a => a.ProductCode == x.ProductCode && a.PRNNo == x.PRNNo).FirstOrDefault();
                            prn.SupplierProductID = Convert.ToInt32(x.SupplierproductrelationID);
                            prn.ShortListed = true;
                        }
                        //    db.SaveChanges();
                       

                        var relationID = OrderDetails.Select(a => a.SupplierProductID).ToList();

                        var resultsss = (from spr in OrderDetails
                                             
                                             join Prndetalss in db.PRNDetails.Where(a => a.PRNNo == PRNNO)
                                             on spr.ProductCode equals Prndetalss.ProductCode into products
                                             from prn in products.DefaultIfEmpty()

                                             join PrnMain in db.PRNMains
                                             on spr.PRNNo equals PrnMain.PRNNo into PrnMaina
                                             from prnmain in PrnMaina.DefaultIfEmpty() 

                                             join ProductDetails in db.Products                                        
                                             on spr.ProductCode equals ProductDetails.ProductCode into ProductDetails
                                             from prd in ProductDetails.DefaultIfEmpty()

                                             join TAxmasters in db.TaxMasters on prd.HsnCode equals TAxmasters.HSNCode into TAxdetails
                                             from tax in TAxdetails.DefaultIfEmpty() 

                                             join Supplierss in db.suppliers                                                                       
                                             on spr.SupplierName equals Supplierss.SupplierName into Supplierr
                                             from sp in Supplierr.DefaultIfEmpty()

                                             join relations in db.SupplierProductRelations.Where(a=> relationID.Contains(a.SupplierProductRelationId))
                                             on spr.SupplierProductID equals relations.SupplierProductRelationId into relation
                                             from rel in relation.DefaultIfEmpty()
                                             
                                             orderby spr.ProductCode ascending
                                                                           select new
                                                                           {
                                                                               ProductName = prn.ProductName,
                                                                               ProductCode = prn.ProductCode,
                                                                               PRNNo = prn.PRNNo,
                                                                               SupplierName = sp.SupplierName,
                                                                               SupplierID = sp == null ? 0 : sp.SupplierID, 
                                                                               Quantity = prn == null ? 0 : prn.Quantity,
                                                                               TaxPercent = tax == null ? 0 : tax.TaxPercent,
                                                                               sup = prn == null ? 0 : prn.Quantity,
                                                                               SupplierProductID = prn == null ? 0 : prn.SupplierProductID,
                                                                               Discount = rel == null ? 0 : rel.Discount,
                                                                               DiscountIn = rel == null ? string.Empty : rel.DiscountIn,
                                                                               HSNCode = tax == null ? string.Empty : tax.HSNCode, 
                                                                               ProductPrice = rel == null ? 0 : rel.ProductPrice,
                                                                               IGST = sp == null ? false : sp.IGST,
                                                                               RequiredDate = prnmain == null ? DateTime.Today : prnmain.RequiredDate,    
                                                                           }

                                     ).ToList();
                           var result111 = OrderDetails.Select(m => new { m.SupplierName }).Distinct().ToList();
                        foreach (var aa in result111)
                        {

                            var code = 0;
                            var temp = 1;
                            var SupplierDetails = db.suppliers.Where(a => a.SupplierName == aa.SupplierName).FirstOrDefault(); 
                            var billNo = db.BillNumbering.Where(a => a.Type == "POMain").FirstOrDefault();

                         
                            foreach (var xx in resultsss.Where(a=>a.SupplierName==aa.SupplierName ))
                              {      
                                if (temp == 1)
                                {
                                    POMain main = new POMain();
                                    main.SupplierID = SupplierDetails.SupplierID;
                                    main.PurchaseOrderNo = "PONO_" + billNo.Number;
                                    main.PurchaseOrderDate = DateTime.Today;
                                    main.CompanyID = Convert.ToInt32(Session["CompanyID"]);
                                    main.ExpectedDeliveryDate = xx.RequiredDate;
                                    main.CustomerID = null;
                                    main.ShipVia = "Road";
                                    main.ShippingTerms = "Once sold, there is no accept goods";


                                    //Calculations

                                    //var Rate = aa.ProductPrice;
                                    //var Disc = aa.ProductPrice; 

                                    main.NetAmount = 0;
                                    main.IGST = 0;
                                    main.SGST = 0;
                                    main.CGST = 0;
                                    main.TotalAmount = 0;
                                    main.Discount = 0;                                    
                                    //main.WarehouseId = x.WarehouseId;
                                    //main.BarcodeApplicable = x.BarcodeApplicable;

                                    main.POStatus = "Auto Generated";
                                    main.PayAmount = 0;
                                    main.freeze = true;
                                    main.CreatedBy = User.Identity.Name;
                                    main.CreatedDate = DateTime.Today;
                                    db.POMains.Add(main);
                                    db.SaveChanges();
                                    code = db.POMains.Max(a => a.PurchaseOrderID);
                                }


                                var Rate = xx.ProductPrice;
                                var NetAmount = Rate * xx.Quantity;
                                Decimal DiscAmt = 0;                              

                                var TaxPer = xx.TaxPercent;
                                var ISIGST = xx.IGST;
                              
                                if (xx.DiscountIn == "Rupee")
                                {
                                    DiscAmt = Convert.ToDecimal(xx.Discount);
                                }
                                else
                                {
                                    var disc= (NetAmount*xx.Discount)/ 100;
                                }
                                var TotalNetAmount = NetAmount - DiscAmt;
                                var TotalGST = (TotalNetAmount * xx.TaxPercent) / 100;
                                var mains = db.POMains.Where(a => a.PurchaseOrderID == code).FirstOrDefault();
                                mains.NetAmount = mains.NetAmount + NetAmount;
                                decimal? IGSTAMTT = 0;
                                decimal? CGSTAMTT = 0; 
                                if (xx.IGST==true)
                                {
                                    IGSTAMTT= mains.IGST + TotalGST;
                                    mains.IGST = IGSTAMTT;
                                }
                                else
                                {
                                    var gst = (mains.IGST + TotalGST)/2; 
                                    mains.CGST = IGSTAMTT;
                                    mains.SGST = gst;
                                    CGSTAMTT = gst;
                                }
                                mains.Discount = mains.Discount + DiscAmt;
                                mains.TotalAmount = mains.TotalAmount = TotalNetAmount + TotalGST;

                                PODetails details = new PODetails();
                                details.PRNNO = PRNNO;
                                details.PurchaseOrderID = code;
                                details.OrderQty = Convert.ToDecimal(xx.Quantity);
                                details.ProductCode = xx.ProductCode;
                                details.GSTPercentage = Convert.ToDecimal(xx.TaxPercent);
                                details.Price = Convert.ToDecimal(xx.ProductPrice);
                                details.NetAmount = Convert.ToDecimal(NetAmount);
                                details.SGSTAmount = Convert.ToDecimal(CGSTAMTT);
                                details.CGSTAmount = Convert.ToDecimal(CGSTAMTT);
                                details.IGSTAmount = Convert.ToDecimal(IGSTAMTT);
                                details.TotalAmount = Convert.ToDecimal(TotalNetAmount + TotalGST);
                                details.CompanyID = Convert.ToInt32(Session["CompanyID"]);
                                details.HSNCode = xx.HSNCode;
                                details.Discount = DiscAmt;

                                if (xx.Discount == null)
                                {
                                    details.DiscountAmount = 0;
                                }
                                else
                                {
                                    details.DiscountAmount = DiscAmt;
                                }
                                details.PONO = "PONO_" + billNo.Number;
                                details.ReceivedQty = 0;
                                details.IsActive = true;
                                details.DiscountAs = xx.DiscountIn;
                                details.CreatedBy = User.Identity.Name;
                                details.CreatedDate = DateTime.Now;
                             //   details.BarcodeApplicable = x.BarcodeApplicable;
                                db.poDetails.Add(details);
                                temp++;
                            }
                            
                            billNo.Number = Convert.ToInt32(billNo.Number) + 1;
                        }
                        db.SaveChanges();
                        var TOTCount = db.PRNDetails.Where(a => a.PRNNo == PRNNO).Count();
                        var ShhortlistCount = db.PRNDetails.Where(a => a.PRNNo == PRNNO && a.ShortListed == true).Count();
                        if (TOTCount == ShhortlistCount)
                        {
                            var prn = db.PRNMains.Where(a => a.PRNNo == PRNNO).FirstOrDefault();
                            prn.ShortListed = true;
                             db.SaveChanges();
                        }
                        dbcxtransaction.Commit();
                            var result = new { Message = "success" };
                            return Json(result, JsonRequestBehavior.AllowGet);
                        }
                    
                }
                else
                {
                    var result = new { Message = "No Data " };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ee)
            {
                var result = new { Message = ee.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }


        public JsonResult SendEmail(List<PRNDetails> OrderDetails) 
        {
            try
            {
                var PRNNO = OrderDetails[0].PRNNo;
                if (OrderDetails.Count > 0)
                {
                    using (var dbcxtransaction = db.Database.BeginTransaction())
                    {
                        foreach (var x in OrderDetails.OrderBy(a => a.SupplierName))
                        {
                            
                                var results = (from spr in db.SupplierProductRelations

                                              join Prndetalss in db.PRNDetails 
                                              on spr.ProductCode equals Prndetalss.ProductCode into products
                                              from prn in products.DefaultIfEmpty() 

                                              join Supplierss in db.suppliers
                                              on spr.SupplierId equals Supplierss.SupplierID into Supplierr
                                              from sp in Supplierr.DefaultIfEmpty()                                            

                                              orderby spr.ProductCode ascending
                                              select new
                                              {
                                                  ProductName = prn.ProductName,                                                
                                                  Quantity = prn == null ? 0 : prn.Quantity,
                                                  PRNNo = prn.PRNNo,
                                                  SupplierName=sp.SupplierName

                                              }
                                   
                                     ).ToList();

                            var SupplierEmail = db.suppliers.Where(a => a.SupplierName == x.SupplierName).FirstOrDefault();
                            MailMessage message = new MailMessage();
                            message.From = new MailAddress("gangardehanu@gmail.com");
                            message.To.Add(new MailAddress(SupplierEmail.BillingEmail));                           
                            message.To.Add(new MailAddress("raj@micraft.co.in"));
                          
                            message.Subject = "Micraft Solutions Pune";

                            string textBody = "Please Send Quotations\n\n"+ "<table border=" + 1 + " cellpadding=" + 0 + " cellspacing=" + 0 + " width = " + 400 + ">" +
                                
                            "<tr bgcolor='#4da6ff'><td><b>Inventory Item</b>            </td> <td> <b> Required Qunatity </b> </td></tr>";
                            var StingTAble = "";
                            var tableData = results.Where(a => a.PRNNo == x.PRNNo && a.SupplierName == x.SupplierName).ToList();
                            foreach (var b in tableData)
                            {
                               var xx= "<tr bgcolor='#DDA0DD'><td><b>" + b.ProductName+"</b>            </td> <td> <b> "+b.Quantity+" </b> </td></tr>";
                                StingTAble = StingTAble + xx;
                            }
                            
                            textBody += StingTAble+ "</table>";


                            message.Body = textBody;
                            message.IsBodyHtml = true;                           
                            SmtpClient smtp = new SmtpClient();                          
                            smtp.Send(message);
                          
                        }
                         
                        dbcxtransaction.Commit();
                        var result = new { Message = "success" };
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }


                }
                else
                {
                    var result = new { Message = "No Data " };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ee)
            {
                var result = new { Message = ee.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }






        internal class Quotations
    {
        public string SupplierproductrelationID { get; set; } 
        public string ProductName { get; set; }
        public string SupplierName { get; set; }

            public string SupplierID { get; set; } 
            public string ProductPrice { get; set; }
        public string DeliveryInDays { get; set; }
        public string ProductCode { get; set; }

            public int? ID { get; set; }
            public string PRNNo { get; set; } 

        }
}


    
}