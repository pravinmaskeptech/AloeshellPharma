using Inventory.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class DebitReportController : Controller
    {
        private InventoryModel db = new InventoryModel();

        // GET: DebitReport
        public ActionResult Index()
        {
            return View();
        }
       
        [HttpPost]
        public string GetData(string sEcho, int iDisplayLength, int iDisplayStart, string sSearch)
        {
            try

            {
                DateTime currentDate = DateTime.Now;
                var result = (from dn in db.DebitNote
                              join sp in db.suppliers on dn.SupplierId equals sp.SupplierID
                              where dn.DocDate < currentDate
                              select new
                              {
                                  sp.SupplierName,
                                  DebitNoteNo = dn.DocNo,
                                  DebitNoteDate = dn.DocDate,
                                  PONO = dn.PONO,
                                  Qty = dn.ReturnItems,
                                  Value = dn.Value
                              }).ToList();




                var FromDate = Session["FromDate"];
                var ToDate = Session["ToDate"];
                if (FromDate == "" || ToDate == "" || ToDate == null || FromDate == null)
                {
                     result = (from dn in db.DebitNote
                                  join sp in db.suppliers on dn.SupplierId equals sp.SupplierID
                                  where dn.DocDate < currentDate
                                  select new
                                  {
                                      sp.SupplierName,
                                      DebitNoteNo = dn.DocNo,
                                      DebitNoteDate = dn.DocDate,
                                      PONO = dn.PONO,
                                      Qty = dn.ReturnItems,
                                      Value = dn.Value
                                  }).ToList();


                }
                else
                {
                    DateTime fromDateValue = new DateTime();
                    DateTime toDateValue = new DateTime();
                    try
                    { fromDateValue = DateTime.ParseExact(FromDate.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture); }
                    catch { }

                    try
                    { toDateValue = DateTime.ParseExact(ToDate.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture); }
                    catch { }
                    Session["FromDate"] = "";
                    Session["ToDate"] = "";

                  result = (from dn in db.DebitNote
                                  join sp in db.suppliers on dn.SupplierId equals sp.SupplierID
                                  where dn.DocDate < currentDate
                                  select new
                                  {
                                      sp.SupplierName,
                                      DebitNoteNo = dn.DocNo,
                                      DebitNoteDate = dn.DocDate,
                                      PONO = dn.PONO,
                                      Qty = dn.ReturnItems,
                                      Value = dn.Value
                                  }).ToList();


                }

                int totalRecords = result.Count();

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
                sb.Append(Newtonsoft.Json.JsonConvert.SerializeObject(result));
                sb.Append("}");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return ex.Message;

            }
        }

        public ActionResult SearchData(string FromDate, string ToDate)
        {
            try
            {
                //var dtFrom = DateTime.ParseExact(FromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                //var dtTo = DateTime.ParseExact(ToDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                //var dtTo = dtTo1.AddDays(1); 

                Session["FromDate"] = FromDate;
                Session["ToDate"] = ToDate;

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
}