using Inventory.Models;
using Syncfusion.EJ.Export;
using Syncfusion.JavaScript.Models;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Inventory.Controllers
{
    public class CustomerContactsController : Controller
    {
        private InventoryModel db = new InventoryModel();       
        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /CustomerContacts/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /CustomerContacts/Create
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /CustomerContacts/Create
        [HttpPost]
        public ActionResult Create(CustomerContacts collection)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // TODO: Add insert logic here

                    return RedirectToAction("Index");
                }
                catch
                {
                    return View();
                }
                return View();
            }
            return View();
        }

        //
        // GET: /CustomerContacts/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /CustomerContacts/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /CustomerContacts/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /CustomerContacts/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
