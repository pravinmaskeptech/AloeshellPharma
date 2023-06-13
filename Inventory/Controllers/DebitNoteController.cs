using Inventory.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class DebitNoteController : Controller
    {
        InventoryModel db = new InventoryModel();
        public ActionResult Index()
        {
            //var results = from p in db.GRNDetail where p.ReturnQty>0 && p.DebitDocNo==null
            //              join supplier in db.suppliers on p.SupplierID equals supplier.SupplierID
            //                select new {p.GRNNo, p.GRNId, p.PONo, p.PODate, p.ReturnQty, p.AmountPerItem, supplier.SupplierName} into x
            //              group x by x.PONo into g
            //              select new {SupplierName=g.Select(x => x.SupplierName).FirstOrDefault(), PONO = g.Select(x => x.PONo).FirstOrDefault(), GRNId = g.Select(x => x.GRNId).FirstOrDefault(), PODate = g.Select(x => x.PODate).FirstOrDefault(), TotalQty = g.Sum(x => x.ReturnQty), TotAmount = g.Sum(x => (x.ReturnQty * x.AmountPerItem)) };

            var results = from p in db.GRNDetail
                          where p.ReturnQty > 0 /*&& p.DebitDocNo == null*/
                          join supplier in db.suppliers on p.SupplierID equals supplier.SupplierID

                          group new { p.GRNNo, p.GRNId, p.PONo, p.PODate, p.ReturnQty, p.AmountPerItem, supplier.SupplierName } by p.PONo into g
                          select new
                          {
                              SupplierName = g.Select(x => x.SupplierName).FirstOrDefault(),
                              PONO = g.Key,
                              GRNId = g.Select(x => x.GRNId).FirstOrDefault(),
                              PODate = g.Select(x => x.PODate).FirstOrDefault(),
                              TotalQty = g.Sum(x => x.ReturnQty),
                              TotAmount = g.Sum(x => x.ReturnQty * x.AmountPerItem),

                          };


            ViewBag.datasource = results.ToList();

            return View();
        }

        public JsonResult GetDataFromGRNDetails()
        {

            var grndetails = db.GRNDetail.Where(a => a.ReturnQty > 0 && a.DebitDocNo != null);

            var debitnote = db.DebitNote.ToList();


            return Json(grndetails, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Detail(string Id)
        {
            ViewBag.PONO = Id;
            var count = db.BillNumbering.Where(x => x.Type == "DebitDocNo").FirstOrDefault();
            var Number = count.Number;
            ViewBag.DocNo = "Doc_" + Number;
            var GRN = db.GRNDetail.Select(x => new { x.SupplierID, x.PONo }).Where(x => x.PONo == Id).FirstOrDefault();
            var Supplier = db.suppliers.Where(x => x.SupplierID == GRN.SupplierID).Select(x => new { x.SupplierName }).FirstOrDefault();
            ViewBag.Supplier = Supplier.SupplierName;
            return View();
        }

        [HttpGet]
        public ActionResult GetData(string PONO)
        {
            var result = (from GRN in db.GRNDetail
                          where GRN.PONo == PONO && GRN.ReturnQty > 0 /*&& GRN.DebitDocNo == null*/
                          join P in db.Products on GRN.ProductCode equals P.ProductCode into prod
                          from product in prod.DefaultIfEmpty()
                          select new { ProductCode = GRN.ProductCode, ProductName = product.ProductName, ItemQty = GRN.ReturnQty, Tax = GRN.Tax, Discount = (GRN.Discount * GRN.ReturnQty), TaxAmount = ((GRN.IGST + GRN.SGST + GRN.CGST) * GRN.ReturnQty), Amount = (GRN.AmountPerItem * GRN.ReturnQty) }
                            ).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Save(string PONO, string DocNo)
        {
            try
            {
                var GRN = db.GRNDetail.Where(x => x.PONo == PONO && x.ReturnQty > 0 && x.DebitDocNo == null).ToList();
                GRN.ForEach(x => x.DebitDocNo = DocNo);

                var results = from p in db.GRNDetail
                              where p.ReturnQty > 0 && p.PONo == PONO
                              join supplier in db.suppliers on p.SupplierID equals supplier.SupplierID
                              select new { p.PONo, p.PODate, p.ReturnQty, p.AmountPerItem, supplier.SupplierName, p.SupplierID } into x
                              group x by x.PONo into g
                              select new { PODate = g.Select(x => x.PODate).FirstOrDefault(), TotalQty = g.Sum(x => x.ReturnQty), TotAmount = g.Sum(x => (x.ReturnQty * x.AmountPerItem)), SupplierId = g.Select(x => x.SupplierID).FirstOrDefault() };

                var data = results.FirstOrDefault();

                DebitNote debitnote = new DebitNote();
                debitnote.DocNo = DocNo;
                debitnote.DocDate = DateTime.Now;
                debitnote.PONO = PONO;
                debitnote.PODate = data.PODate;
                debitnote.ReturnItems = data.TotalQty.ToString();
                debitnote.Value = Convert.ToDecimal(data.TotAmount);
                debitnote.CreatedBy = User.Identity.Name;
                debitnote.CreatedDate = DateTime.Now;
                debitnote.IsActive = true;
                debitnote.SupplierId = data.SupplierId;
                debitnote.CompanyID = Convert.ToInt32(Session["CompanyID"]);
                db.DebitNote.Add(debitnote);

                var BillNumbers = db.BillNumbering.Where(x => x.Type == "DebitDocNo").FirstOrDefault();
                int number = Convert.ToInt32(BillNumbers.Number) + 1;
                BillNumbers.Number = number;

                db.SaveChanges();

                var message = "success";

                return Json(message);
            }
            catch (Exception EX)
            {
                return Json(EX.Message);
            }
        }

        public JsonResult PrintDebitNote(int id)
        {
            try
            {
                var getGRNNo = db.GRNDetail.Where(a => a.GRNId == id).FirstOrDefault();
                if (getGRNNo == null)
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }

                var Productsmaster = new List<Products>(db.Products);
                var Storesmaster = new List<StoreLocations>(db.StoreLocations);
                var Warehousemaster = new List<Warehouse>(db.Warehouses);
                var POReturnsMaster = new List<POReturns>(db.pOReturns);
                var result12 = (from returns in POReturnsMaster.Where(a => a.GrnNo == getGRNNo.GRNNo /*&&  a.ProductCode == getGRNNo.ProductCode*/)

                                join Product in Productsmaster on returns.ProductCode equals Product.ProductCode into products
                                from prd in products.DefaultIfEmpty()

                                join Warehouse in Warehousemaster on returns.WarehouseID equals Warehouse.WareHouseID into warehouse
                                from Whouse in warehouse.DefaultIfEmpty()

                                join StoreLocation in Storesmaster on returns.StoreLocationId equals StoreLocation.StoreLocationId into storeLoc
                                from store in storeLoc.DefaultIfEmpty()

                                orderby returns.CreatedDate descending
                                select new { SerialNumber = returns.SerialNumber, GrnNo = returns.GrnNo, ReturnDate = returns.ReturnDate, BatchNo = returns.BatchNo, ReturnQty = returns.ReturnQty, ReturnReason = returns.ReturnReason, ProductCode = prd == null ? string.Empty : prd.ProductName, Warehouse = Whouse == null ? string.Empty : Whouse.WareHouseName, StoreLocation = store == null ? string.Empty : store.StoreLocation, Status = returns.Status }
                                    ).ToList();
                var distinctByProductCode = result12
    .GroupBy(x => x.ProductCode)
    .Select(g => new
    {
        ProductCode = g.Key,
        ReturnQty = g.Sum(x => x.ReturnQty),
        SerialNumber = g.First().SerialNumber,
        GrnNo = g.First().GrnNo,
        ReturnDate = g.First().ReturnDate,
        BatchNo = g.First().BatchNo,
        ReturnReason = g.First().ReturnReason,
        Warehouse = g.First().Warehouse,
        StoreLocation = g.First().StoreLocation,
        Status = g.First().Status
    })
    .ToList();


                var supplier = db.suppliers.Where(a => a.SupplierID == getGRNNo.SupplierID).FirstOrDefault();

                iTextSharp.text.Font font5 = iTextSharp.text.FontFactory.GetFont(FontFactory.HELVETICA, 9);
                iTextSharp.text.Font font10 = iTextSharp.text.FontFactory.GetFont(FontFactory.HELVETICA, 11);

                DateTime dt = DateTime.Now;
                var aa = dt.ToString("HH");
                var bb = dt.ToString("mm");
                var cc = dt.ToString("ss");

                Document document = new Document(PageSize.A4, 5f, 5f, 5f, 5);
                string path = Server.MapPath("~/InvoicePrint/");
                //string[] parts = OrderNo.Split('/');
                //string numberString = parts[2];
                string filename1 = path + "" + "DebitNote" + "_" + aa + bb + cc + ".pdf";
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



                PdfPCell pp4 = new PdfPCell(new Phrase(new Phrase("Return Qty", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.WHITE))));
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
                foreach (var r in distinctByProductCode)
                {
                    var prd = db.Products.Where(a => a.ProductName == r.ProductCode).FirstOrDefault();
                    var grn = db.GRNDetail.Where(a => a.GRNNo == getGRNNo.GRNNo && a.ProductCode == prd.ProductCode).FirstOrDefault();

                    var qty = r.ReturnQty;
                    var amt = grn.BasicRate;
                    var Netamot = Convert.ToDecimal(qty) * Convert.ToDecimal(amt);
                    totAmt = totAmt + Netamot;
                    var Count = 1;
                    //var orderdetails=db.orderDetails.Where(a=>a.OrderNo==r.)
                    if (distinctByProductCode.Count > 0)
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
                        pr32.Add(new Phrase("" + prd.ProductName + " - " + prd.Size, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell prr32 = new PdfPCell(pr32);
                        prr32.HorizontalAlignment = 0;
                        table1.AddCell(prr32);


                        Paragraph pr34 = new Paragraph();
                        pr34.Add(new Phrase("" + string.Format("{0:0.00 }", r.ReturnQty) + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell prr34 = new PdfPCell(pr34);
                        prr34.HorizontalAlignment = 1;
                        table1.AddCell(prr34);


                        Paragraph pr346 = new Paragraph();
                        pr346.Add(new Phrase("" + string.Format("{0:0.00 }", amt) + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                        PdfPCell prr34878 = new PdfPCell(pr346);
                        prr34878.HorizontalAlignment = 1;
                        table1.AddCell(prr34878);

                        Paragraph pr359894 = new Paragraph();
                        pr359894.Add(new Phrase("" + string.Format("{0:0.00 }", Netamot) + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
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
                    pr931.Add(new Phrase("Amount In Words : " + AmtInwords, FontFactory.GetFont("Arial", 8, Font.BOLD)));
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
                    pr359894.Add(new Phrase("" + string.Format("{0:0.00 }", Math.Round(totAmt)) + "", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                    PdfPCell prr3sdd4 = new PdfPCell(pr359894);
                    prr3sdd4.HorizontalAlignment = 2;
                    table1.AddCell(prr3sdd4);

                    Paragraph pr36g44 = new Paragraph();
                    pr36g44.Add(new Phrase("", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                    PdfPCell prr36g44 = new PdfPCell(pr36g44);
                    prr36g44.HorizontalAlignment = 0;
                    prr36g44.Border = Rectangle.RIGHT_BORDER;
                    table1.AddCell(prr36g44);

                }
                catch
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
                //pr38525.Add(new Phrase("Terms And Conditions : \n\n", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                //pr38525.Add(new Phrase("1) Above rates are excluding GST\n", FontFactory.GetFont("Arial", 8, Font.NORMAL)));
                //pr38525.Add(new Phrase("2) Post Dated Cheque (PDC) required at the time of delivery of goods\n", FontFactory.GetFont("Arial", 8, Font.NORMAL)));
                //pr38525.Add(new Phrase("3) Transportation charges - extra as applicable\n", FontFactory.GetFont("Arial", 8, Font.NORMAL)));
                //pr38525.Add(new Phrase("" + trarmandcondition, FontFactory.GetFont("Arial", 8, Font.NORMAL)));
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
                table5.WidthPercentage = 98; ;
                table5.HorizontalAlignment = 1;

                PdfPCell pc2 = new PdfPCell();
                pc2.HorizontalAlignment = 0;
                pc2.Border = Rectangle.NO_BORDER;
                table5.AddCell(pc2);

                Paragraph pr1 = new Paragraph();
                pr1.Add(new Phrase("Supplier Name : ", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                pr1.Add(new Phrase(supplier.SupplierName, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                pr1.Add(new Phrase("\nAddress : ", FontFactory.GetFont("Arial", 9, Font.BOLD)));
                pr1.Add(new Phrase("\n" + supplier.BillingAddress, FontFactory.GetFont("Arial", 9, Font.NORMAL)));
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
                table9.WidthPercentage = 98; ;

                Paragraph pr226 = new Paragraph();
                // pr226.Add(new Phrase("Date:" + DateTime.Now.ToString("dd/MM/yyyy") + "\n", FontFactory.GetFont("Arial", 8, Font.BOLD)));

                PdfPCell p79013 = new PdfPCell();
                p79013.HorizontalAlignment = 0;
                table9.AddCell(p79013);


                PdfPTable table3 = new PdfPTable(3);
                float[] widths55 = new float[] { 2f, 8f, 10 };
                table3.SetWidths(widths55);
                table3.WidthPercentage = 98;
                table3.HorizontalAlignment = 1;


                Paragraph pr53 = new Paragraph();
                pr53.Add(new Phrase("\n\n   CN No\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                pr53.Add(new Phrase("   Date", FontFactory.GetFont("Arial", 9, Font.NORMAL)));

                PdfPCell c112 = new PdfPCell(pr53);
                c112.Border = Rectangle.TOP_BORDER;
                c112.FixedHeight = 50f;
                c112.HorizontalAlignment = 0;
                table3.AddCell(c112);

                var crdata = db.DebitNote.Where(a => a.PONO == getGRNNo.PONo).FirstOrDefault();
                if (crdata == null)
                {
                    var result0 = new { Message = "Complete Debit Note First" };
                    return Json(result0, JsonRequestBehavior.AllowGet);
                }
                Paragraph pr539 = new Paragraph();
                pr539.Add(new Phrase("\n\n: " + crdata.DocNo + "\n", FontFactory.GetFont("Arial", 9, Font.NORMAL)));
                pr539.Add(new Phrase(": " + crdata.DocDate.ToString("dd/MM/yyyy") + "", FontFactory.GetFont("Arial", 9, Font.NORMAL)));

                PdfPCell c1129 = new PdfPCell(pr539);
                c1129.Border = Rectangle.TOP_BORDER;
                c1129.FixedHeight = 50f;
                c1129.HorizontalAlignment = 0;
                table3.AddCell(c1129);

                Paragraph p49 = new Paragraph();
                p49.Add(new Phrase("DEBIT NOTE", FontFactory.GetFont("Arial", 13, Font.BOLD)));
                PdfPCell c1115 = new PdfPCell(p49);
                c1115.HorizontalAlignment = 0;
                c1115.Border = Rectangle.TOP_BORDER;
                table3.AddCell(c1115);

                PdfPTable table6 = new PdfPTable(3);
                float[] width6 = new float[] { 5f, 7f, 4f };
                table6.SetWidths(width6);
                table6.WidthPercentage = 98; ;
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
