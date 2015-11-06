using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HNHB.Models.Entities;

namespace HNHB.Controllers
{
    public class ItemController : Controller
    {
        // GET: Item
        Entities db = new Entities();
        public ActionResult Index()
        {
            return View();
        }

        public PartialViewResult Create()
        {
            return PartialView();
        }

        public PartialViewResult EditPlaceItem()
        {
            return PartialView();
        }
        // Insert Item to database
        [HttpPost]
        public ActionResult AddItem(string name, double price, int placeId)
        {
            Item item = new Item
            {
                Name = name,
                Price = price,
                PlaceId = placeId,
                isActive = true
            };
            db.Items.Add(item);
            db.SaveChanges();
            return Json(new { Id = item.Id });
        }
        // Delete item
        [HttpPost]
        public ActionResult RemoveItem(int id)
        {
            Item item = db.Items.Find(id);
            if (item != null)
            {
                db.Items.Remove(item);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}