using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VirtualStore.Models;

namespace VirtualStore.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private ModelVirtualStoreContainer con = new ModelVirtualStoreContainer();

        // GET: Orders
        public ActionResult Index()
        {
            var userName = User.Identity.Name;

            var orders = con.Orders
                .Where(o => o.userName == userName)
                .OrderByDescending(o => o.dateOrder)
                .ToList();

            return View(orders);
        }

        // GET: Orders/Details/5
        [Authorize]
        public ActionResult Details(int id)
        {
            var order = con.Orders
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            if (order.userName != User.Identity.Name)
            {
                return new HttpUnauthorizedResult();
            }

            return View(order);
        }

        // GET: Orders/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Orders/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
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
        }

        // GET: Orders/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Orders/Edit/5
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

        // GET: Orders/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Orders/Delete/5
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
