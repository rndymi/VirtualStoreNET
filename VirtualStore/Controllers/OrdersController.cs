using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
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
        public ActionResult Details(int id, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var order = con.Orders
                .Include(o => o.OrderDetail.Select(od => od.Product))
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
                return HttpNotFound();

            bool isAdmin = User.IsInRole("Admin");
            bool isStaff = User.IsInRole("Logistics");
            bool isOwner = string.Equals(order.userName, User.Identity.Name, StringComparison.OrdinalIgnoreCase);

            if (!isAdmin && !isStaff && !isOwner)
                return HttpNotFound();

            if (!string.IsNullOrWhiteSpace(returnUrl) && !Url.IsLocalUrl(returnUrl))
            {
                ViewBag.ReturnUrl = null;
            }

            return View(order);
        }

        // GET: Orders/AdminIndex
        [Authorize(Roles = "Admin")]
        public ActionResult AdminIndex()
        {
            var orders = con.Orders
                .OrderByDescending(o => o.dateOrder)
                .ToList();

            return View(orders);
        }

        // GET: Orders/EditStatus/5
        [Authorize(Roles = "Admin,Logistics")]
        public ActionResult EditStatus(int id, string returnUrl)
        {
            var order = con.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null) return HttpNotFound();

            ViewBag.ReturnUrl = returnUrl;

            ViewBag.AllowedStatus = new SelectList(new[]
            {
                "Created", "Processing", "Shipped", "Delivered", "Cancelled"
            }, order.statusOrder);

            return View(order);
        }

        // POST: Orders/EditStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Logistics")]
        public ActionResult EditStatus(int id, string statusOrder, string returnUrl)
        {
            var order = con.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null) return HttpNotFound();

            var allowed = new[] { "Created", "Processing", "Shipped", "Delivered", "Cancelled" };
            if (!allowed.Contains(statusOrder))
            {
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.AllowedStatus = new SelectList(allowed, statusOrder);
                ModelState.AddModelError("statusOrder", "Invalid status.");
                return View(order);
            }

            order.statusOrder = statusOrder;
            con.SaveChanges();

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Details", new { id = order.Id });
        }
    }
}
