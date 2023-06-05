using Inventory.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class SettingsController : Controller
    {
        InventoryModel db = new InventoryModel();
        //
        // GET: /Settings/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetSetting()
        {
            var result = db.Settings.ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Update(List<Settings> Settings)
        {
            var message = "";
            try
            {
                Settings.ForEach(x => { x.UpdatedBy = User.Identity.Name; x.UpdatedDate = DateTime.Now; });
                Settings.ForEach(p => db.Entry(p).State = EntityState.Modified);                
                db.SaveChanges();
                message = "success";
                return Json(message);
            }
            catch (Exception EX)
            {
                message = EX.Message;
                return Json(message);
            }            
        }
	}
}