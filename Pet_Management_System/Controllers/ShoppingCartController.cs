using PayPal.Api;
using Pet_Management_System.Models;
using Pet_Management_System.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pet_Management_System.Controllers
{
    public class ShoppingCartController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);

            var viewModel = new ShoppingCartViewModel
            {
                CartItems = cart.GetCartItems(),
                CartTotal = cart.GetTotal()
            };

            return View(viewModel);
        }

        public ActionResult AddToCart(int id)
        {
            var addedProduct = db.Products.Single(product => product.Id == id);

            var cart = ShoppingCart.GetCart(this.HttpContext);

            cart.AddToCart(addedProduct);

            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult UpdateCart(int productId, int newQuantity)
        {
            var cart = ShoppingCart.GetCart(HttpContext);
            cart.UpdateCartItemQuantity(productId, newQuantity);

            // Recalculate the cart total
            decimal cartTotal = cart.GetTotal();

            // Return JSON object with updated cart total and any messages
            return Json(new { cartTotal = cartTotal, message = "Cart updated successfully." });
        }

        [HttpPost]
        public ActionResult CreateCustInfo(string RecipientName, string RecipientNumber, string Address, string City, string Province, string totalCost, DateTime deliveryDate, string preffaredTime,string ShippingMethod,string deliveryFee)
        {
            CustInfo custInfo = new CustInfo();
            custInfo.RecipientName = RecipientName;
            custInfo.RecipientNumber = RecipientNumber;
            custInfo.Address = Address;
            custInfo.City = City;
            custInfo.Province = Province;
            custInfo.Email = User.Identity.Name;
            custInfo.deliveryDate = deliveryDate;
            custInfo.preffaredTime = preffaredTime;
            custInfo.ShippingMethod = ShippingMethod;
            string deleviveryFeeVal = deliveryFee.Substring(0, deliveryFee.Length - 3);
            custInfo.deliveryFee = double.Parse(deleviveryFeeVal);
            string totalCostVal = totalCost.Substring(0, totalCost.Length - 3);
            double FtotalCost = double.Parse(totalCostVal);
            db.CustInfos.Add(custInfo);
            db.SaveChanges();
            return RedirectToAction("CreatePayment", "PayPal", new { CartTotal = FtotalCost, ShipMethod = "Delivery" });
        }
        [HttpPost]
        public ActionResult UpdateCustInfo(string RecipientName, string RecipientNumber, string Address, string City, string Province, string totalCost, DateTime deliveryDate, string preffaredTime, string ShippingMethod, string deliveryFee)
        {
            var custInfo = db.CustInfos.Where(x=>x.Email == User.Identity.Name).FirstOrDefault();
            custInfo.RecipientName = RecipientName;
            custInfo.RecipientNumber = RecipientNumber;
            custInfo.Address = Address;
            custInfo.City = City;
            custInfo.Province = Province;
            custInfo.Email = User.Identity.Name;
            custInfo.deliveryDate = deliveryDate;
            custInfo.preffaredTime = preffaredTime;
            custInfo.ShippingMethod = ShippingMethod;
            string deleviveryFeeVal = deliveryFee.Substring(0, deliveryFee.Length - 3);
            custInfo.deliveryFee = double.Parse(deleviveryFeeVal);
            string totalCostVal = totalCost.Substring(0, totalCost.Length - 3);
            double FtotalCost = double.Parse(totalCostVal);

            db.Entry(custInfo).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("CreatePayment", "PayPal", new { CartTotal = FtotalCost, ShipMethod = "Delivery" });
        }
        public ActionResult RemoveFromCart(int id)
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);

            string productName = db.Carts.FirstOrDefault(item => item.ProductId == id).Product.Name;

            int itemCount = cart.RemoveFromCart(id);

            var results = new ShoppingCartRemoveViewModel
            {
                Message = Server.HtmlEncode(productName) + " has been removed from your shopping cart",
                CartTotal = cart.GetTotal(),
                CartCount = cart.GetCount(),
                ItemCount = itemCount,
                DeleteId = id
            };

            return RedirectToAction("Index");
        }

        [ChildActionOnly]
        public ActionResult CartSummary()
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);

            ViewData["CartCount"] = cart.GetCount();
            return PartialView("CartSummary");
        }

    }
}