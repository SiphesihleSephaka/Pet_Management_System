using Pet_Management_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pet_Management_System.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        const String PromoCode = "FREE";
        public ActionResult AddressAndPayment()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddressAndPayment(FormCollection values)
        {
            var order = new CustomerOrder();

            TryUpdateModel(order);

            try
            {
                
                    order.CustomerUserName = User.Identity.Name;
                    order.DateCreated = DateTime.Now;

                    db.CustomerOrders.Add(order);
                    db.SaveChanges();

                    var cart = ShoppingCart.GetCart(this.HttpContext);
                    cart.CreateOrder(order);

                    db.SaveChanges();//we have received the total amount lets update it

                    return RedirectToAction("CreatePayment","PayPal", new { CartTotal = order.Amount });
                
            }
            catch (Exception ex)
            {
                ex.InnerException.ToString();
                return View(order);
            }
        }

        public ActionResult Complete(int id)
        {
            bool isValid = db.CustomerOrders.Any(
                o => o.Id == id &&
                     o.CustomerUserName == User.Identity.Name
                );

            if (isValid)
            {
                return View(id);
            }
            else
            {
                return View("Error");
            }
        }
    }
}