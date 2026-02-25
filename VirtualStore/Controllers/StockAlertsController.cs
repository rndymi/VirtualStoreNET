using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VirtualStore;


namespace VirtualStore.Controllers
{

    [Authorize(Roles = "Admin")]
    public class StockAlertsController : Controller
    {

        ModelVirtualStoreContainer con = new ModelVirtualStoreContainer();

        // GET: StockAlerts
        public ActionResult Index()
        {
            var alerts = con.StockAlerts
                .OrderByDescending(a => a.modification)
                .ToList();

            return View(alerts);
        }
    }
}