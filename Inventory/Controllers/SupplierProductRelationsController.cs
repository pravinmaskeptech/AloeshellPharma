using Inventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class SupplierProductRelationsController : Controller
    {
        private InventoryModel db = new InventoryModel();
        // GET: SupplierProductRelations
        public ActionResult Create()
        {
            ViewBag.datasource = db.suppliers.Where(a => a.IsActive == true).ToList();
            ViewBag.Custdatasource = db.Customers.Where(a => a.IsActive == true).ToList();
            ViewBag.Productdatasource = db.Products.Where(a => a.IsActive == true).ToList();
            return View();
        }
        public JsonResult checkSupplier(string supplier)
        {
            var result = 0;
            if (supplier != "")
            {
                result = db.suppliers.Where(x => x.SupplierName.Equals(supplier)).Count();               
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult getAllData(int SupplierId)
        {
            List<SupplierProductRelations> result = new List<SupplierProductRelations>();
            result = db.SupplierProductRelations.Where(a=>a.SupplierId==SupplierId).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult checkProduct(string product)
        {
            var result = 0;
            if (product != "")
            {
                result = db.Products.Where(x => x.ProductName.Equals(product)).Count();
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetProductName(string productCode)
        {
            try
            {
                var result = "";
                if (productCode != "")
                {
                    result = db.Products.Where(x => x.ProductCode.Equals(productCode)).Select(a => a.ProductName).Single();
                }
                return Json(result, JsonRequestBehavior.AllowGet);
            }catch
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult save(List<SupplierProductRelations> relation)
        {
            try
            {
                bool status = false;
                var TotalCount = relation.ToList().Count; 
                if (TotalCount == 0 || relation==null)
                {
                    return new JsonResult { Data = new { status = false, Message = "No Data Found....Please Add Record...!" } };
                }
                else
                {                    
                        using (var dbt = db.Database.BeginTransaction())
                        {
                            foreach (var x in relation)
                            {
                                SupplierProductRelations suply = new SupplierProductRelations();
                                if (x.SupplierProductId == 0)
                                {
                                    suply.IsActive = true;
                                    suply.SupplierId = x.SupplierId;
                                    suply.ProductCode = x.ProductCode;
                                    suply.ProductPrice = x.ProductPrice;
                                    suply.Discount = x.Discount;
                                    suply.DiscountIn = x.DiscountIn;
                                suply.Tax = x.Tax;
                                suply.Delivery = x.Delivery;
                                db.SupplierProductRelations.Add(suply);
                                }
                                else
                                {
                                    //update Data
                                    var spRelation = db.SupplierProductRelations.Where(a => a.SupplierProductRelationId.Equals(x.SupplierProductId)).FirstOrDefault();
                                    spRelation.ProductCode = x.ProductCode;
                                    spRelation.SupplierId = Convert.ToInt32(x.SupplierId);
                                    spRelation.ProductPrice = Convert.ToDecimal(x.ProductPrice);
                                    spRelation.Discount = Convert.ToDecimal(x.Discount);
                                    spRelation.DiscountIn = x.DiscountIn;
                                spRelation.Delivery = x.Delivery;
                                spRelation.Tax = x.Tax;
                                db.SaveChanges();                                    
                                }
                            db.SaveChanges();
                            status = true;
                        }
                        dbt.Commit();
                    }
                    return new JsonResult { Data = new { status,Message=" Products Added Successfully..>!" } };
                }               
            }
            catch (Exception ex)
            {              
                var Message = ex.InnerException.InnerException.Message;
                return new JsonResult { Data = new { status=false,Message }};
            }
        }

      
    
    }
}