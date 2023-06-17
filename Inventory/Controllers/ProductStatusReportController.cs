using Inventory.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using static iTextSharp.text.pdf.PdfStructTreeController;

namespace Inventory.Controllers
{
    public class ProductStatusReportController : Controller
    {
        private InventoryModel db = new InventoryModel();

        // GET: ProductStatusReport
        public ActionResult Index()
        {
            ViewBag.Productdatasource = db.Products.Where(a => a.IsActive == true && a.SerialNoApplicable == true).ToList();
            return View();
        }
        [HttpPost]
        public string GetData(string sEcho)
        {


            var Used = (from row in db.ProductSerialNo
                        where (row.Status == "DCSold" || row.Status == "Sold" || row.Status == "Sale")
                        //group row by new { row.InvoiceNo, row.ProductCode } into g
                        group row by row.InvoiceNo into g
                        select new
                        {
                            InvoiceNo = g.Key,
                            ProductCode = g.Select(x => x.ProductCode).Distinct(),
                            SerialNos = g.Select(x => x.SerialNo),
                            CustomerName = db.orderMain.FirstOrDefault(a => a.OrderNo == db.Sales.Where(s => s.InvoiceNo == g.Key).Select(s => s.OrderNo).FirstOrDefault()).CustomerName
                        })
              .ToList();



            var UnUsed = (from row in db.ProductSerialNo
                          where row.Status == "inward" || row.Status == "SO Return" || row.Status == "DC Return"
                          
                          group row by new {  row.ProductCode } into g
                          select new
                          {
                   
                              ProductCode = g.Key.ProductCode,
                              SerialNos = g.Select(x => x.SerialNo),
                     

                          })
                         .ToList();

            var result1 = Used.Select(u => new
            {
                u.InvoiceNo,
                u.ProductCode,
                u.CustomerName,
                UsedSerialNos = u.SerialNos,
                QtyUsed = u.SerialNos.Count()
            })
               .ToList();


            //var result1 = Used.Join(UnUsed,
            //            u => new { u.ProductCode },
            //            v => new { v.ProductCode },
            //            (u, v) => new
            //            {
            //                u.InvoiceNo,
            //                u.ProductCode,
            //                u.CustomerName,
            //                UsedSerialNos = u.SerialNos,
            //                UnUsedSerialNos = v.SerialNos,
            //                QtyUsed = u.SerialNos.Count(),
            //                QtyUnUsed = v.SerialNos.Count()
            //            })
            //      .ToList();

            List<object> mergedSerialNos = new List<object>();
            List<string> consecutiveSerialNos = new List<string>();
            foreach (var item in result1)
            {
                List<string> consecutiveSerialNos1 = new List<string>();
                
                //List<string> unUsedSerialNos = item.UnUsedSerialNos.ToList();
                List<string> usedSerialNos = item.UsedSerialNos.ToList();
                var replacedSerialNo = "";
                for (int i = 1; i < usedSerialNos.Count; i++)
                {
                    string serialNo1 = usedSerialNos[i - 1];
                    string serialNo2 = usedSerialNos[i];
                    string firstSeralNo = usedSerialNos[0];

                    int suffix1 = GetSuffix(serialNo1);
                    int suffix2 = GetSuffix(serialNo2);

                    if (i == 1)
                    {
                        consecutiveSerialNos.Add(firstSeralNo);
                    }

                    if (suffix2 != suffix1 + 1)
                    {
                        consecutiveSerialNos.Add("- " + replacedSerialNo);


                        consecutiveSerialNos.Add(", " + serialNo2);
                    }
                    else
                    {
                        replacedSerialNo = serialNo1.Replace(serialNo1, serialNo2);

                    }
                }
                var used = consecutiveSerialNos;
                mergedSerialNos.Add(new { UsedSerialNos = used });
                // var replacedSerialNo1 = "";
                //    for (int i = 1; i < unUsedSerialNos.Count; i++)
                //    {
                //        string serialNo1 = unUsedSerialNos[i - 1];
                //        string serialNo2 = unUsedSerialNos[i];
                //        string firstSeralNo = unUsedSerialNos[0];

                //        int suffix1 = GetSuffix(serialNo1);
                //        int suffix2 = GetSuffix(serialNo2);

                //        if (i == 1)
                //        {
                //            consecutiveSerialNos1.Add(firstSeralNo);
                //        }

                //        if (suffix2 != suffix1 + 1)
                //        {
                //            consecutiveSerialNos1.Add("- " + replacedSerialNo1 + ".  ");

                //            consecutiveSerialNos1.Add(", " + serialNo1);
                //        }
                //        else
                //        {
                //            replacedSerialNo1 = serialNo1.Replace(serialNo1, serialNo2);
                //        }
                //    }
                //    var unused = consecutiveSerialNos1;
                //    mergedSerialNos.Add(new { InvoiceNo = item.InvoiceNo,  UsedSerialNos = used, UnUsedSerialNos = unused });
            }

            //var result0 = mergedSerialNos;

            var result2 = Used.Select(u => new
            {
                u.InvoiceNo,
                u.ProductCode,
                u.CustomerName,
                UsedSerialNos = consecutiveSerialNos,
                QtyUsed = u.SerialNos.Count()
            })
               .ToList();

            int totalRecords = result1.Count;
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"sEcho\":");
            sb.Append(sEcho);
            sb.Append(",");
            sb.Append("\"iTotalRecords\":");
            sb.Append(totalRecords);
            sb.Append(",");
            sb.Append("\"iTotalDisplayRecords\":");
            sb.Append(totalRecords);
            sb.Append(",");
            sb.Append("\"aaData\":");
            sb.Append(Newtonsoft.Json.JsonConvert.SerializeObject(result1));
            sb.Append("}");
            return sb.ToString();
        }





        private int GetSuffix(string serialNo)
        {
            string suffix = serialNo.Substring(serialNo.Length - 6);
            int suffixValue;
            if (int.TryParse(suffix, out suffixValue))
            {
                return suffixValue;
            }
            return 0;
        }


        public ActionResult SearchData(string SerialNo)
        {
            try
            {
                Session["UsedSerialNos"] = SerialNo;
                var result = new { Message = "success" };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = new { Message = ex.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
    }
}