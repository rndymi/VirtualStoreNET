using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VirtualStore.Models;

namespace VirtualStore.Controllers
{
    [RoutePrefix("api/products")]
    public class ProductsAPIController : ApiController
    {

        private ModelVirtualStoreContainer con = new ModelVirtualStoreContainer();

        // GET: api/products
        [HttpGet]
        [Route("")]
        public IHttpActionResult Get()
        {
            var products = con.Products
                .Where(p => p.isActiveProd)
                .Select(p => new
                {
                    p.Id,
                    name = p.nameProd,
                    description = p.descripProd,
                    p.price,
                    stock = p.stockProd,
                    image = p.imageProd
                })
                .ToList();

            return Ok(products);
        }
    }
}
