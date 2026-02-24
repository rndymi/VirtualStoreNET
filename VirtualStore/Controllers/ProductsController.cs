using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VirtualStore.Models.Metadata;
using VirtualStore.Models.ViewModel;
using System.Globalization;

namespace VirtualStore.Controllers
{
    public class ProductsController : Controller
    {
        ModelVirtualStoreContainer con = new ModelVirtualStoreContainer();

        // GET: Products
        public ActionResult Index()
        {
            var query = con.Products.AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                query = query.Where(p => p.isActiveProd);
            }

            return View(query.ToList());
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var product = con.Products.Find(id);
            if (product == null)
                return HttpNotFound();

            if (!User.IsInRole("Admin") && !product.isActiveProd)
                return HttpNotFound();

            return View(product);
        }

        // GET: Products/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            LoadCategories();
            var product = new Product
            {
                isActiveProd = true
            };
            return View(product);
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create(
            [Bind(Include = "nameProd,descripProd,price,stockProd,isActiveProd")] Product product,
            int? Category_Id,
            HttpPostedFileBase imageFile)
        {
            LoadCategories(Category_Id);

            if (Category_Id == null || Category_Id <= 0)
                ModelState.AddModelError("Category_Id", "Category is required.");

            if (imageFile != null && imageFile.ContentLength > 0)
            {
                var allowed = new[] { ".png", ".jpg", ".jpeg", ".webp" };
                var ext = System.IO.Path.GetExtension(imageFile.FileName).ToLowerInvariant();

                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("imageFile", "Only .png, .jpg, .jpeg or .webp files are allowed.");
                }
            }

            if (!ModelState.IsValid)
                return View(product);

            var category = con.Categories.Find(Category_Id.Value);
            if (category == null)
            {
                ModelState.AddModelError("Category_Id", "Selected category does not exist.");
                return View(product);
            }
            product.Category = category;

            if (imageFile != null && imageFile.ContentLength > 0)
            {
                var ext = System.IO.Path.GetExtension(imageFile.FileName).ToLowerInvariant();
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

            con.Products.Add(product);
            UpsertStockAlert(product);

            con.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Products/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var product = con.Products.Find(id);
            if (product == null)
                return HttpNotFound();

            int? categoryId = con.Database.SqlQuery<int?>(
                "SELECT Category_Id FROM Products WHERE Id = @p0", product.Id)
                .FirstOrDefault();

            var vm = ToProductVm(product, categoryId);

            return View(vm);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(ProductViewModel vm)
        {
            vm.Categories = BuildCategoriesSelectList(vm.Category_Id);

            var productDb = con.Products.Find(vm.Id);
            if (productDb == null)
                return HttpNotFound();

            if (!ModelState.IsValid)
            {
                vm.imageProd = productDb.imageProd;
                return View(vm);
            }

            var category = con.Categories.Find(vm.Category_Id.Value);
            if (category == null)
            {
                ModelState.AddModelError("CategoryId", "Selected category does not exist.");
                vm.imageProd = productDb.imageProd;
                return View(vm);
            }

            productDb.nameProd = vm.nameProd;
            productDb.descripProd = vm.descripProd;
            productDb.price = vm.price;
            productDb.stockProd = vm.stockProd;
            productDb.isActiveProd = vm.isActiveProd;
            productDb.Category = category;

            if (vm.imageFile != null && vm.imageFile.ContentLength > 0)
            {
                var allowed = new[] { ".png", ".jpg", ".jpeg", ".webp" };
                var ext = System.IO.Path.GetExtension(vm.imageFile.FileName).ToLowerInvariant();

                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("imageFile", "Only .png, .jpg, .jpeg or .webp files are allowed.");
                    vm.imageProd = productDb.imageProd;
                    return View(vm);
                }

                var fileName = $"{System.Guid.NewGuid():N}{ext}";
                var relativePath = "/Content/images/" + fileName;
                var physicalPath = Server.MapPath("~/Content/images/" + fileName);

                vm.imageFile.SaveAs(physicalPath);
                productDb.imageProd = relativePath;
            }

            UpsertStockAlert(productDb);

            con.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Products/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            var product = con.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return HttpNotFound();

            bool hasOrders = con.OrderDetails.Any(od => od.Product.Id == id);
            ViewBag.HasOrders = hasOrders;

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int id, FormCollection collection)
        {
            var product = con.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return HttpNotFound();

            bool hasOrders = con.OrderDetails.Any(od => od.Product.Id == id);

            var alert = con.StockAlerts.FirstOrDefault(a => a.Product.Id == id);
            if (alert != null)
                con.StockAlerts.Remove(alert);

            if (hasOrders)
            {
                product.isActiveProd = false;
                product.stockProd = 0;

                con.SaveChanges();
                TempData["CartError"] = "Product has orders associated. It was marked as DISCONTINUED instead of deleted.";

                return RedirectToAction("Index");
            }

            con.Products.Remove(product);
            con.SaveChanges();

            return RedirectToAction("Index");

        }



        // Helpers

        private void LoadCategories(int? selectedCategoryId = null)
        {
            var categories = con.Categories
                .OrderBy(c => c.nameCate)
                .ToList();

            ViewBag.Category_Id = new SelectList(categories, "Id", "nameCate", selectedCategoryId);
        }

        private IEnumerable<SelectListItem> BuildCategoriesSelectList(int? selectedId)
        {
            return con.Categories
                .OrderBy(c => c.nameCate)
                .ToList()
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.nameCate,
                    Selected = selectedId.HasValue && c.Id == selectedId.Value
                });
        }

        private ProductViewModel ToProductVm(Product p, int? categoryId)
        {
            return new ProductViewModel
            {
                Id = p.Id,
                nameProd = p.nameProd,
                descripProd = p.descripProd,
                price = p.price,
                stockProd = p.stockProd,
                isActiveProd = p.isActiveProd,
                imageProd = p.imageProd,
                Category_Id = categoryId,
                Categories = BuildCategoriesSelectList(categoryId)
            };
        }

        private void UpsertStockAlert(Product product)
        {
            var alert = con.StockAlerts.FirstOrDefault(a => a.Product.Id == product.Id);

            if (product.stockProd < 2)
            {
                if (alert == null)
                {
                    alert = new StockAlert
                    {
                        Product = product,
                        currentStock = product.stockProd,
                        modification = DateTime.Now,
                        statusStock = "LOW_STOCK"
                    };
                    con.StockAlerts.Add(alert);
                }
                else
                {
                    alert.currentStock = product.stockProd;
                    alert.modification = DateTime.Now;
                    alert.statusStock = "LOW_STOCK";
                }
            }
            else
            {
                if (alert != null)
                {
                    con.StockAlerts.Remove(alert);
                }
            }
        }
    }
}
