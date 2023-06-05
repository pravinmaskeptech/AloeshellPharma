using Inventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class CustomerProductRelationsController : Controller
    {
        private InventoryModel db = new InventoryModel();
        // GET: SupplierProductRelations
        public ActionResult Create()
        {

            ViewBag.Custdatasource = db.Customers.Where(a => a.IsActive == true).ToList();
            ViewBag.Productdatasource = db.Products.Where(a => a.IsActive == true).ToList();
            return View();
        }
        public JsonResult checkSupplier(string customer)
        {
            var result = 0;
            if (customer != "")
            {
                result = db.Customers.Where(x => x.CustomerName.Equals(customer)).Count();
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getAllData(int CustomerId)
        {
            List<CustomerProductRelations> result = new List<CustomerProductRelations>();
            result = db.CustomerProductRelations.Where(a => a.CustomerId == CustomerId).ToList();
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
                Products result = new Products();
                if (productCode != "")
                {
                    result = db.Products.Where(x => x.ProductCode.Equals(productCode)).FirstOrDefault();
                }
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult save(List<CustomerProductRelations> relation)
        {
            try
            {
                bool status = false;
                var TotalCount = relation.ToList().Count;
                if (TotalCount == 0 || relation == null)
                {
                    return new JsonResult { Data = new { status = false, Message = "No Data Found....Please Add Record...!" } };
                }
                else
                {
                    using (var dbt = db.Database.BeginTransaction())
                    {
                        foreach (var x in relation)
                        {
                            CustomerProductRelations suply = new CustomerProductRelations();
                            if (x.SupplierProductId == 0)
                            {
                                suply.IsActive = true;
                                suply.CustomerId = x.CustomerId;
                                suply.ProductCode = x.ProductCode;
                                suply.ProductPrice = x.ProductPrice;
                                suply.Tax = x.Tax;

                                db.CustomerProductRelations.Add(suply);
                            }
                            else
                            {
                                //update Data
                                var spRelation = db.CustomerProductRelations.Where(a => a.CustomerProductRelationId==x.SupplierProductId).FirstOrDefault();
                                spRelation.ProductCode = x.ProductCode;
                                spRelation.CustomerId = x.CustomerId;
                                spRelation.ProductPrice = x.ProductPrice;
                               
                                spRelation.Tax = x.Tax;
                                
                            }
                            db.SaveChanges();
                            status = true;
                        }
                        dbt.Commit();
                    }
                    return new JsonResult { Data = new { status, Message = " Products Added Successfully..>!" } };
                }
            }
            catch (Exception ex)
            {
                var Message = ex.InnerException.InnerException.Message;
                return new JsonResult { Data = new { status = false, Message } };
            }
        }



    }
}