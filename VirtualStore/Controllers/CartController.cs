using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VirtualStore.Models;

namespace VirtualStore.Controllers
{
    public class CartController : Controller
    {
        private ModelVirtualStoreContainer con = new ModelVirtualStoreContainer();

        // GET: Cart
        public ActionResult Index(Cart cart)
        {
            return View(cart);
        }

        // POST: Cart/AddToCart/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToCart(Cart cart, int id)
        {
            var product = con.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return HttpNotFound();
            }

            if (!product.isActiveProd)
            {
                TempData["CartError"] = "This product is discontinued.";
                return RedirectToAction("Index", "Products");
            }

            int currentQty = cart.Items
                .Where(i => i.ProductId == id)
                .Select(i => i.Quantity)
                .FirstOrDefault();

            if (product.stockProd < currentQty + 1)
            {
                TempData["CartError"] = "There is not enough stock to add this product.";
                return RedirectToAction("Index", "Products");
            }

            cart.AddItem(product.imageProd, product.Id, product.nameProd, product.price, 1);
            return RedirectToAction("Index", "Products");
        }


        // Post: Cart/Increase/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Increase(Cart cart, int id)
        {

            var product = con.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return RedirectToAction("Index");

            var item = cart.Items.FirstOrDefault(i => i.ProductId == id);
            int currentQty = item?.Quantity ?? 0;

            if (product.stockProd < currentQty + 1)
            {
                TempData["CartError"] = "There is not enough stock to increase the quantity.";
                return RedirectToAction("Index");
            }

            cart.AddItem(product.imageProd, product.Id, product.nameProd, product.price, 1);
            return RedirectToAction("Index");
        }

        // Post: Cart/Decrease/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Decrease(Cart cart, int id)
        {
            var item = cart.Items.FirstOrDefault(i => i.ProductId == id);
            if (item != null)
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                }
                else
                {
                    cart.RemoveItem(id);
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Post: Cart/Remove/5
        public ActionResult Remove(Cart cart, int id)
        {
            cart.RemoveItem(id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Post: Cart/Clear
        public ActionResult Clear(Cart cart)
        {
            cart.Clear();
            return RedirectToAction("Index");
        }

        // Post: Cart/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Checkout(Cart cart)
        {
            if (!cart.Items.Any())
                return RedirectToAction("Index");

            using (var tx = con.Database.BeginTransaction())
            {
                try
                {
                    var ids = cart.Items.Select(i => i.ProductId).Distinct().ToList();
                    var products = con.Products.Where(p => ids.Contains(p.Id)).ToList();

                    foreach (var item in cart.Items)
                    {
                        var product = products.FirstOrDefault(p => p.Id == item.ProductId);

                        if (product == null)
                        {
                            TempData["CartError"] = "A product no longer exists.";
                            tx.Rollback();
                            return RedirectToAction("Index");
                        }

                        if (product.stockProd < item.Quantity)
                        {
                            TempData["CartError"] = $"Insufficient stock for {product.nameProd}.";
                            tx.Rollback();
                            return RedirectToAction("Index");
                        }
                    }

                    var order = new Order
                    {
                        dateOrder = DateTime.Now,
                        statusOrder = "Created",
                        totalAmount = cart.TotalAmount(),
                        userName = User.Identity.Name
                    };

                    con.Orders.Add(order);
                    con.SaveChanges();

                    foreach (var item in cart.Items)
                    {
                        var product = products.First(p => p.Id == item.ProductId);

                        con.OrderDetails.Add(new OrderDetail
                        {
                            Order = order,
                            Product = product,
                            quantity = item.Quantity,
                            unitPrice = item.UnitPrice,
                            orderTotal = item.OrderTotal
                        });

                        product.stockProd -= item.Quantity;
                    }

                    con.SaveChanges();
                    tx.Commit();

                    cart.Clear();
                    return RedirectToAction("Confirmation", new { id = order.Id });
                }
                catch
                {
                    tx.Rollback();
                    TempData["CartError"] = "Error processing order. Please try again.";
                    return RedirectToAction("Index");
                }
            }
        }

        // GET: Cart/Confirmation/5
        public ActionResult Confirmation(int id)
        {
            var order = con.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }
    }
}
