using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VirtualStore.Models
{
    public class Cart
    {
        private List<CartItem> items = new List<CartItem>();

        public IEnumerable<CartItem> Items => items;

        public void AddItem(int productId, string name, decimal unitPrice, int quantity = 1)
        {
            var item = items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null)
            {
                items.Add(new CartItem
                {
                    ProductId = productId,
                    Name = name,
                    UnitPrice = unitPrice,
                    Quantity = quantity
                });
            }
            else
            {
                item.Quantity += quantity;
            }
        }

        public void RemoveItem(int productId) {
            var item = items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                items.Remove(item);
            }
        }

        public void Clear() {
            items.Clear();
        }

        public decimal TotalAmount() => items.Sum(i => i.OrderTotal);
    }
}