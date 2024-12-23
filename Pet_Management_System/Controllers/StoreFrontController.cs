using Pet_Management_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Pet_Management_System.Controllers
{
    public class StoreFrontController : Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: StoreFront
        public ActionResult Index(int? id)
        {

            Category category = new Category();
            if (id == null)
            {
                 category = db.Categories.FirstOrDefault();
            }
            else
            {
                 category = db.Categories.Find(id);
            }

            
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        public ActionResult Single(int? id) 
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }
    }
}