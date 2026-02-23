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

            con.SaveChanges();
            return RedirectToAction("Index");
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
    }
}
