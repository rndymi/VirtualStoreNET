using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using VirtualStore;

namespace VirtualStore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        ModelVirtualStoreContainer con = new ModelVirtualStoreContainer();

        // GET: Categories
        public ActionResult Index()
        {
            var categories = con.Categories.OrderBy(c => c.nameCate).ToList();
            return View(categories);
        }

        // GET: Categories/Create
        public ActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            ViewBag.BackText = "Back";
            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                returnUrl.IndexOf("/Manage", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                ViewBag.BackText = "Back to AdminPanel";
            }

            return View(new Category());
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "nameCate,descripCate,isActiveCate")] Category category,
            string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.BackText = "Back";

            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                returnUrl.IndexOf("/Manage", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                ViewBag.BackText = "Back to AdminPanel";
            }

            if (!ModelState.IsValid)
                return View(category);

            con.Categories.Add(category);
            con.SaveChanges();

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index");
        }

        // GET: Categories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var category = con.Categories.Find(id);
            if (category == null) return HttpNotFound();

            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,nameCate,descripCate,isActiveCate")] Category category)
        {
            if (!ModelState.IsValid) return View(category);

            var db = con.Categories.Find(category.Id);
            if (db == null) return HttpNotFound();

            db.nameCate = category.nameCate;
            db.descripCate = category.descripCate;
            db.isActiveCate = category.isActiveCate;

            con.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Categories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var category = con.Categories.Find(id);
            if (category == null) return HttpNotFound();

            bool hasProducts = con.Products.Any(p => p.Category.Id == id.Value);
            ViewBag.HasProducts = hasProducts;

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var category = con.Categories.Find(id);
            if (category == null) return HttpNotFound();

            bool hasProducts = con.Products.Any(p => p.Category.Id == id);
            if (hasProducts)
            {
                TempData["Error"] = "Cannot delete this category because it has products assigned.";
                return RedirectToAction("Index");
            }

            con.Categories.Remove(category);
            con.SaveChanges();
            TempData["Success"] = "Category deleted.";
            return RedirectToAction("Index");
        }
    }
}
