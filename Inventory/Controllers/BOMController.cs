using Inventory.Models;
using Syncfusion.EJ.Export;
using Syncfusion.JavaScript.Models;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Inventory.Controllers
{
    public class BOMController : Controller
    {
        // GET: BOM
        private InventoryModel db = new InventoryModel();
        public ActionResult Create()
        {
            try
            {
                ViewBag.Componentdatasource = db.Products.Where(a => a.IsActive == true && a.ProductCode == "").ToList();
                ViewBag.Productdatasource = db.Products.Where(a => a.IsActive == true).ToList();
            }
            catch (Exception Ex)
            {
                ViewBag.Componentdatasource = db.Products.Where(a => a.IsActive == true && a.ProductCode == "").ToList();
                ViewBag.Productdatasource = db.Products.Where(a => a.IsActive == true).ToList();
                TempData["Msg"] = Ex.Message.ToString();
            }
            return View();
        }
        public JsonResult getBOMData(string productId)
        {
            try
            {
                var Productsmaster = new List<Products>(db.Products);
                var BOMMaster = new List<BOM>(db.Bom);
                var data = (from bom in BOMMaster.Where(a => a.ProductId == productId)

                            join Product in Productsmaster on bom.ComponentId equals Product.ProductCode into products
                            from prd in products.DefaultIfEmpty()

                            join Product in Productsmaster on bom.ProductId equals Product.ProductCode into products1
                            from prd1 in products1.DefaultIfEmpty()

                            orderby bom.BomId ascending
                            select new { BomId = bom.BomId, ProductId = bom.ProductId, ComponentId = bom.ComponentId, Quantity = bom.Quantity, ProductName = prd1 == null ? string.Empty : prd1.ProductName, ComponentName = prd == null ? string.Empty : prd.ProductName, hasSubComponent = bom.hasSubComponent }
                                     ).ToList();

                var IsComponent = db.Bom.Where(x => x.ComponentId == productId).Select(x => x.ProductId).ToList();
                var ExplodedBom = db.ExplodedBOM.Where(x => x.ProductId == productId || x.ComponentId == productId).Select(x => x.FinishGoods).ToList();

                var Component = db.Products.Where(x => x.IsActive == true && x.ProductCode != productId && !IsComponent.Contains(x.ProductCode) && !ExplodedBom.Contains(x.ProductCode)).Select(x => new { x.ProductCode, x.ProductName }).ToList();
                var result = new { Message = "success", data = data, Component = Component };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception EX)
            {
                var result = new { Message = EX.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult BomApplicable(string ComponentId)
        {
            var result = db.Bom.Where(a => a.ProductId == ComponentId).Count();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Edit()
        {
            ViewBag.Componentdatasource = db.Products.Where(a => a.IsActive == true).ToList();
            ViewBag.Productdatasource = db.Products.Where(a => a.IsActive == true).ToList();
            return View();
        }
        public ActionResult Index()
        {
            ViewBag.datasource = (from bom in db.Bom

                                  join Product in db.Products on bom.ComponentId equals Product.ProductCode into ProductData
                                  from prd in ProductData.DefaultIfEmpty()

                                  join Product in db.Products on bom.ProductId equals Product.ProductCode into ProductData1
                                  from prd1 in ProductData1.DefaultIfEmpty()

                                  orderby bom.BomId descending
                                  select new { BomId = bom.BomId, ComponentId = prd.ProductName, ProductId = prd1.ProductName, ProductCode = prd1.ProductCode, Quantity = bom.Quantity, IsActive = bom.IsActive }
                         ).ToList();
            return View();
        }
        public JsonResult save(List<BOM> bomdata, string UpdateAssembly)
        {
            try
            {
                var ProdCode = "";
                bool status = false;
                int t = bomdata.Count;
                if (t > 0)
                {
                    List<int> BOMIdList = new List<int>();
                    foreach (var x in bomdata)
                    {
                        BOMIdList.Add(x.BOMNo);
                        ProdCode = x.ProductId;
                    }
                    var BOM = db.Bom.Where(x => !BOMIdList.Contains(x.BomId) && x.ProductId == ProdCode).ToList();
                    db.Bom.RemoveRange(BOM);
                    db.SaveChanges();

                    foreach (var x in bomdata)
                    {
                        BOM bom = new BOM();
                        if (x.BOMNo == 0)
                        {
                            bom.ProductId = x.ProductId;
                            bom.ComponentId = x.ComponentId;
                            bom.Quantity = x.Quantity;
                            bom.CreatedBy = User.Identity.Name;
                            bom.hasSubComponent = Convert.ToBoolean(x.hasSubComponent);
                            bom.CreatedDate = DateTime.Today;
                            bom.IsActive = true;
                            db.Bom.Add(bom);
                            db.SaveChanges();
                            status = true;
                        }
                        else
                        {
                            var BMNO = db.Bom.Where(a => a.BomId.Equals(x.BOMNo)).FirstOrDefault();
                            BMNO.ComponentId = x.ComponentId;
                            BMNO.Quantity = x.Quantity;
                            BMNO.UpdateDate = DateTime.Today;
                            BMNO.hasSubComponent = Convert.ToBoolean(x.hasSubComponent);
                            BMNO.UpdatedBy = User.Identity.Name;
                            db.SaveChanges();
                            status = true;
                        }
                    }
                    BomComponent(ProdCode);

                    if (UpdateAssembly == "YES")
                    {
                        var Component = db.Bom.Where(x => x.ComponentId == ProdCode && x.hasSubComponent != true).ToList();
                        Component.ForEach(a => a.hasSubComponent = true);
                        db.SaveChanges();

                        var CComponent = db.Bom.Select(x=>new { x.ProductId}).ToList();

                        foreach (var temp in CComponent)
                       {
                            db.SaveChanges();
                            BomComponent(temp.ProductId);
                        }
                    }

                    return new JsonResult { Data = new { status } };

                }
                else
                {
                    return new JsonResult { Data = new { status } };
                }
            }
            catch (Exception ex)
            {
                bool status = false;
                return new JsonResult { Data = new { status } };
            }
        }

        public ActionResult HasComponent(string ProductCode)
        {
            try
            {
                //var Component = db.Bom.Where(x => x.ComponentId == ProductCode).Select(x=>new { x.ProductId}).ToList();
                var Component = (from Bom in db.Bom.Where(x => x.ComponentId == ProductCode && x.hasSubComponent != true)
                                 join Prd in db.Products on Bom.ProductId equals Prd.ProductCode into P
                                 from Product in P.DefaultIfEmpty()
                                 select new { ProductName = Product.ProductName }).ToList();

                var result = new { Message = "success", Component = Component };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception EX)
            {
                var result = new { Message = EX.Message };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult getAllData(string productId)
        {
            //List<SupplierProductRelations> result = new List<SupplierProductRelations>();
            //result = db.SupplierProductRelations.Where(a => a.SupplierId == SupplierId).ToList();

            var Productsmaster = new List<Products>(db.Products);
            var BOMMaster = new List<BOM>(db.Bom);
            var result = (from bom in BOMMaster.Where(a => a.ProductId == productId)

                          join Product in Productsmaster on bom.ComponentId equals Product.ProductCode into products
                          from prd in products.DefaultIfEmpty()

                          join Product in Productsmaster on bom.ProductId equals Product.ProductCode into products1
                          from prd1 in products1.DefaultIfEmpty()

                          orderby bom.BomId ascending
                          select new { BomId = bom.BomId, ProductId = bom.ProductId, ComponentId = bom.ComponentId, Quantity = bom.Quantity, ProductName = prd1 == null ? string.Empty : prd1.ProductName, ComponentName = prd == null ? string.Empty : prd.ProductName, }
                                 ).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CheckDuplicateComponent(string productId, string ComponentId)
        {
            var result = db.Bom.Where(a => a.ProductId == ComponentId).Count();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //CREATE EXPLODED BOM
        public void BomComponent(string BOMProductID)
        {
            var vps = db.ExplodedBOM.Where(a => a.FinishGoods == BOMProductID).ToList();
            foreach (var vp in vps)
                db.ExplodedBOM.Remove(vp);
            db.SaveChanges();

            var BOMComponents = db.Bom.Where(a => a.ProductId == BOMProductID && a.hasSubComponent == false).ToList();
            InsertBom(BOMComponents, BOMProductID, 1);

            var BOMSubComponents = db.Bom.Where(a => a.ProductId == BOMProductID && a.hasSubComponent == true).ToList();
            foreach (var x in BOMSubComponents)
            {
                SubComponents(x.ComponentId, BOMProductID, x.Quantity);
            }
        }

        private void SubComponents(string ProductId, string BOMProductID, decimal? Quantity)
        {
            var BOMComponents = db.Bom.Where(a => a.ProductId == ProductId && a.hasSubComponent == false).ToList();
            InsertBom(BOMComponents, BOMProductID, Quantity);

            var BOMSubComponents2 = db.Bom.Where(a => a.ProductId == ProductId && a.hasSubComponent == true).ToList();
            SubComponents2(BOMSubComponents2, BOMProductID, Quantity);
        }

        private void SubComponents2(List<BOM> bOMComponents, string BOMProductID, decimal? Quantity)
        {
            foreach (var x in bOMComponents)
            {
                var BOMComponents = db.Bom.Where(a => a.ComponentId == x.ComponentId && a.hasSubComponent == false).ToList();
                InsertBom(BOMComponents, BOMProductID, Quantity);

                var BOMSubComponents2 = db.Bom.Where(a => a.ComponentId == x.ComponentId && a.hasSubComponent == true).ToList();
                foreach (var x2 in bOMComponents)
                {
                    SubComponents(x2.ComponentId, BOMProductID, x2.Quantity * Quantity);
                }
            }
        }

        private void InsertBom(List<BOM> bOMComponents, string BOMProductID, decimal? Qty)
        {
            foreach (var x in bOMComponents)
            {
                ExplodedBOM bom = new ExplodedBOM();
                bom.FinishGoods = BOMProductID;
                bom.ComponentId = x.ComponentId;
                bom.ProductId = x.ProductId;
                bom.Quantity = x.Quantity * Qty;
                db.ExplodedBOM.Add(bom);
                db.SaveChanges();
            }
        }

        [ValidateInput(false)]
        public void ExportToExcel(string GridModel)
        {
            ExcelExport exp = new ExcelExport();
            var CategoryMaster = new List<Category>(db.Categories);
            var DataSource = (from bom in db.Bom

                              join Product in db.Products on bom.ComponentId equals Product.ProductCode into ProductData
                              from prd in ProductData.DefaultIfEmpty()

                              join Product in db.Products on bom.ProductId equals Product.ProductCode into ProductData1
                              from prd1 in ProductData1.DefaultIfEmpty()

                              orderby bom.BomId descending
                              select new { BomId = bom.BomId, ComponentId = prd.ProductName, ProductId = prd1.ProductName, ProductCode = prd1.ProductCode, Quantity = bom.Quantity, IsActive = bom.IsActive }
                         ).ToList();

            GridProperties obj = ConvertGridObject(GridModel);
            exp.Export(obj, DataSource, "BOM.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
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
    }
}