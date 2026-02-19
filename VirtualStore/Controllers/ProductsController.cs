using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VirtualStore.Controllers
{
    public class ProductsController : Controller
    {
        ModelVirtualStoreContainer con = new ModelVirtualStoreContainer();

        // GET: Products
        public ActionResult Index()
        {
            var list = con.Products
                .Where(p => p.isActiveProd) // && p.stockProd > 0  Oculta los products sin stock.
                .ToList();
            return View(list);
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            var product = con.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Products/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            LoadCategories();
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create(
            [Bind(Include = "nameProd,descripProd,stockProd,isActiveProd")] Product product,
            string price,
            int? Category_Id,
            HttpPostedFileBase imageFile)
        {
            LoadCategories(Category_Id);

            if (Category_Id == null || Category_Id <= 0)
            {
                ModelState.AddModelError("Category_Id", "Category is required.");
            }

            if (string.IsNullOrWhiteSpace(price))
            {
                ModelState.AddModelError("price", "Price is required.");
            }
            else
            {
                var normalized = price.Replace(",", ".");
                if (decimal.TryParse(normalized,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var parsed))
                {
                    product.price = parsed;
                }
                else
                {
                    ModelState.AddModelError("price", "Invalid price format.");
                }
            }

            if (imageFile != null && imageFile.ContentLength > 0)
            {
                var allowed = new[] { ".png", ".jpg", ".jpeg", ".webp" };
                var ext = System.IO.Path.GetExtension(imageFile.FileName).ToLowerInvariant();

                if (!allowed.Contains(ext))
                {
                    ViewBag.ImageError = "Only .png, .jpg, .jpeg or .webp files are allowed.";
                    return View(product);
                }

                var fileName = $"{Guid.NewGuid():N}{ext}";
                var relativePath = "/Content/images/" + fileName;
                var physicalPath = Server.MapPath("~/Content/images/" + fileName);

                imageFile.SaveAs(physicalPath);
                product.imageProd = relativePath;
            }
            else
            {
                product.imageProd = "/Content/images/product-default-image.png";
            }

            if (!ModelState.IsValid)
            {
                return View(product);
            }

            var category = con.Categories.Find(Category_Id.Value);
            if (category == null)
            {
                ModelState.AddModelError("Category_Id", "Selected category does not exist.");
                return View(product);
            }
            product.Category = category;

            con.Products.Add(product);
            con.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Products/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
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

        // GET: Products/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Products/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
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



        private void LoadCategories(int? selectedCategoryId = null)
        {
            var categories = con.Categories
                .Where(c => c.isActiveCate)
                .OrderBy(c => c.nameCate)
                .ToList();

            ViewBag.Category_Id = new SelectList(categories, "Id", "nameCate", selectedCategoryId);
        }
    }
}
