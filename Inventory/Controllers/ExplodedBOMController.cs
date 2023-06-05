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
    public class ExplodedBOMController : Controller
    {
        InventoryModel db = new InventoryModel();
        // GET: ExplodedBOM
        public ActionResult Index()
        {           
            var result = (from c in db.ExplodedBOM
                          orderby (c.Id)
                          join Product in db.Products on c.FinishGoods equals Product.ProductCode into ProductData
                          from prd in ProductData.DefaultIfEmpty()
                          group c by new
                          {
                              //c.ProductId,
                              //c.Quantity,
                              prd.ProductCode,
                              prd.ProductName
                          } into gcs

                          select new
                          {
                              Id = gcs.Max(a => a.Id),
                              //ProductId = gcs.Key.ProductId,
                              //Quantity = gcs.Key.Quantity,
                              ProductCode = gcs.Key.ProductCode,
                              ProductName = gcs.Key.ProductName,

                          }).ToList();

            ViewBag.datasource = result.OrderByDescending(a => a.Id).ToList();
            return View();
        }
        public ActionResult Create(string id)
        {
            ViewBag.datasource = (from bom in db.ExplodedBOM.Where(a => a.FinishGoods == id)

                                  join FinGood in db.Products on bom.ProductId equals FinGood.ProductCode into F
                                  from FinishGood in F.DefaultIfEmpty()

                                  join Comp in db.Products on bom.ProductId equals Comp.ProductCode into C
                                  from Component in C.DefaultIfEmpty()

                                  join SubComp in db.Products on bom.ComponentId equals SubComp.ProductCode into SubC
                                  from SubComponent in SubC.DefaultIfEmpty()

                                  orderby bom.Id ascending
                                  select new { FinishGood = FinishGood.ProductName, Component = Component.ProductName, SubComponent = SubComponent.ProductName, Quantity = bom.Quantity }).ToList();

            Session["ExplodedBomPId"] = id;
            return View();
        }
       
        [ValidateInput(false)]
        public void ExportToExcelDetail(string GridModel)
        {
            var ProductCode = Session["ExplodedBomPId"].ToString();

            ExcelExport exp = new ExcelExport();
            var CategoryMaster = new List<Category>(db.Categories);
            var DataSource = (from bom in db.ExplodedBOM.Where(a => a.FinishGoods == ProductCode)
                              join FinGood in db.Products on bom.ProductId equals FinGood.ProductCode into F
                              from FinishGood in F.DefaultIfEmpty()

                              join Comp in db.Products on bom.ProductId equals Comp.ProductCode into C
                              from Component in C.DefaultIfEmpty()

                              join SubComp in db.Products on bom.ComponentId equals SubComp.ProductCode into SubC
                              from SubComponent in SubC.DefaultIfEmpty()

                              orderby bom.Id ascending
                              select new { FinishGood = FinishGood.ProductName, Component = Component.ProductName, SubComponent = SubComponent.ProductName, Quantity = bom.Quantity }).ToList();


            GridProperties obj = ConvertGridObject(GridModel);
            exp.Export(obj, DataSource, "ExplodedBomDetail.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
        }

        [ValidateInput(false)]
        public void ExportToExcel(string GridModel)
        {
            ExcelExport exp = new ExcelExport();
            var CategoryMaster = new List<Category>(db.Categories);
            var DataSource = (from c in db.ExplodedBOM
                              orderby (c.Id)
                              join Product in db.Products on c.FinishGoods equals Product.ProductCode into ProductData
                              from prd in ProductData.DefaultIfEmpty()
                              group c by new
                              {
                                  //c.ProductId,
                                  //c.Quantity,
                                  prd.ProductCode,
                                  prd.ProductName
                              } into gcs
                              select new
                              {
                                  Id = gcs.Max(a => a.Id),
                                  //ProductId = gcs.Key.ProductId,
                                  //Quantity = gcs.Key.Quantity,
                                  ProductCode = gcs.Key.ProductCode,
                                  ProductName = gcs.Key.ProductName,
                              }).ToList();

            GridProperties obj = ConvertGridObject(GridModel);
            exp.Export(obj, DataSource, "ExplodedBom.xlsx", ExcelVersion.Excel2010, false, false, "flat-saffron");
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


        //private void SubComponents(string ProductId, string BOMProductID, decimal? Quantity)
        //{
        //    var BOMComponents = db.Bom.Where(a => a.ProductId == ProductId && a.hasSubComponent == false).ToList();
        //    InsertBom(BOMComponents, BOMProductID, Quantity);

        //    var BOMSubComponents2 = db.Bom.Where(a => a.ProductId == ProductId && a.hasSubComponent == true).ToList();
        //    SubComponents2(BOMSubComponents2, BOMProductID, Quantity);
        //}

        //private void SubComponents2(List<BOM> bOMComponents, string BOMProductID, decimal? Quantity)
        //{
        //    foreach (var x in bOMComponents)
        //    {
        //        var BOMComponents = db.Bom.Where(a => a.ComponentId == x.ComponentId && a.hasSubComponent == false).ToList();
        //        InsertBom(BOMComponents, BOMProductID, Quantity);

        //        var BOMSubComponents2 = db.Bom.Where(a => a.ComponentId == x.ComponentId && a.hasSubComponent == true).ToList();
        //        foreach (var x2 in bOMComponents)
        //        {
        //            SubComponents(x2.ComponentId, BOMProductID, x2.Quantity * Quantity);
        //        }
        //    }
        //}

        //private void InsertBom(List<BOM> bOMComponents, string BOMProductID, decimal? Qty)
        //{
        //    foreach (var x in bOMComponents)
        //    {
        //        ExplodedBOM bom = new ExplodedBOM();
        //        bom.FinishGoods = BOMProductID;
        //        bom.ComponentId = x.ComponentId;
        //        bom.ProductId = x.ProductId;
        //        bom.Quantity = x.Quantity * Qty;
        //        db.ExplodedBOM.Add(bom);
        //        db.SaveChanges();
        //    }
        //}
    }
}