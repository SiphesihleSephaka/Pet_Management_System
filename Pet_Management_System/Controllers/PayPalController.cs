using Pet_Management_System.Models;
using Microsoft.Ajax.Utilities;
using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using QRCoder;
using System.Drawing;
using System.IO;
using System.Net.Mail;
using System.Runtime.ConstrainedExecution;

namespace Pet_Management_System.Controllers
{
    public class PayPalController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult CreatePayment(double CartTotal, string ShipMethod = "Collect")
        {
            Session["ShipMethod"] = ShipMethod;
                var CurrentUser = User.Identity.Name;
                double convertedTot = Math.Round(CartTotal / 14.352);
                int Rem = (int)(CartTotal % 14.352);
                string Cost = convertedTot.ToString() + "." + Rem;

                // Set up the PayPal API context
                var apiContext = PayPalConfig.GetAPIContext();

                // Retrieve the API credentials from configuration
                var clientId = ConfigurationManager.AppSettings["PayPalClientId"];
                var clientSecret = ConfigurationManager.AppSettings["PayPalClientSecret"];
                apiContext.Config = new Dictionary<string, string> { { "mode", "sandbox" } };
                var accessToken = new OAuthTokenCredential(clientId, clientSecret, apiContext.Config).GetAccessToken();
                apiContext.AccessToken = accessToken;

                // Create a new payment object
                var payment = new Payment
                {
                    intent = "sale",
                    payer = new Payer { payment_method = "paypal" },
                    transactions = new List<Transaction>
                {
            new Transaction
            {
                amount = new Amount
                {

                    total = Cost,
                    currency = "USD"
                },

                description = "Shop Payment"
            }
        },
                    redirect_urls = new RedirectUrls
                    {
                        return_url = Url.Action("CompletePayment", "PayPal", null, Request.Url.Scheme),
                        cancel_url = Url.Action("CancelPayment", "PayPal", null, Request.Url.Scheme)
                    }
                };

                // Create the payment and get the approval URL
                var createdPayment = payment.Create(apiContext);
                var approvalUrl = createdPayment.links.FirstOrDefault(l => l.rel == "approval_url")?.href;

                // Redirect the user to the PayPal approval URL
                return Redirect(approvalUrl);
            
        }


        public ActionResult CompletePayment(string paymentId, string token, string PayerID)
        {
            // Set up the PayPal API context
            var apiContext = PayPalConfig.GetAPIContext();

            // Execute the payment
            var paymentExecution = new PaymentExecution { payer_id = PayerID };
            var executedPayment = new Payment { id = paymentId }.Execute(apiContext, paymentExecution);

            // Process the payment completion
            // You can save the transaction details or perform other necessary actions

            // Redirect the user to a success page
            return RedirectToAction("PaymentSuccess");
        }

        public ActionResult CancelPayment()
        {
            // Handle the payment cancellation
            // You can redirect the user to a cancellation page or perform other necessary actions

            // Redirect the user to a cancellation page
            return RedirectToAction("PaymentCancelled");
        }

        public ActionResult PaymentSuccess()
        {
            string ShipMethod;
            if (Session["ShipMethod"] != null)
            {
                ShipMethod = Session["ShipMethod"] as string;
            }
            else
            {
                TempData["Message"] = "Sorry your Session Ended.";
                return RedirectToAction("Index", "ShoppingCart");
            }
            if(ShipMethod == "Delivery")
            {
               
                var userinfo = db.CustInfos.Where(x => x.Email == User.Identity.Name).FirstOrDefault();
                var cart = ShoppingCart.GetCart(this.HttpContext);
                var order = new CustomerOrder();

                Meths meth = new Meths();
                int UniqueCode = Meths.GenerateRandomCode();
                bool conflict = db.CustomerOrders.Where(x => x.UniqueCode == UniqueCode).Any();
                while (conflict)
                {
                    UniqueCode = Meths.GenerateRandomCode();
                    conflict = db.CustomerOrders.Where(x => x.UniqueCode == UniqueCode).Any();
                }

                // Generate QR code
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(UniqueCode.ToString(), QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(20); // Change the size of the QR code image as needed

                // Convert the Bitmap to a byte array
                using (MemoryStream stream = new MemoryStream())
                {
                    qrCodeImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] byteArr = stream.ToArray();

                    // Save the image to a folder
                    string fileName = $"{Guid.NewGuid().ToString()}.png";
                    string filePath = Path.Combine(Server.MapPath("~/images/"), fileName);
                    System.IO.File.WriteAllBytes(filePath, byteArr);

                    // Save the filename to the database
                    order.qrCodePicture = fileName;

                   

                }
                order.UniqueCode = UniqueCode;

                order.deliveryDate = meth.GetWorkingDay(DateTime.Now, 3);
                order.deliveryFee = 0;

                order.Status = "Placed";
                order.Email = User.Identity.Name;
                order.Amount = cart.GetTotal();

                

                TryUpdateModel(order);
                order.UniqueCode = UniqueCode;
                order.CustomerUserName = User.Identity.Name;
                order.DateCreated = DateTime.Now;

                order.LastName = userinfo.Name;
                order.FirstName = userinfo.RecipientName;
                order.Phone = userinfo.RecipientNumber;
                order.Address = userinfo.Address;
                order.City = userinfo.City;
                //order.Country = "";
                order.State = userinfo.Province;
                order.PostalCode = userinfo.PostalCode;
                order.ShippingMethod = userinfo.ShippingMethod;
                
                order.deliveryFee = userinfo.deliveryFee;
                order.preffaredTime = userinfo.preffaredTime;
                order.Status = "Placed";
                order.Email = User.Identity.Name;
                order.Amount = cart.GetTotal();

                db.CustomerOrders.Add(order);

                db.SaveChanges();
                try
                {
                    // Prepare email message
                    var email2 = new MailMessage();
                    email2.From = new MailAddress("Poultry.dbn@outlook.com");
                    email2.To.Add(User.Identity.Name);
                    email2.Subject = "Payment Confirmation |  " + order.Id;
                    string emailBody = $"Order Number: " + order.Id + "\t\t Estimated Delivery Date: " + order.deliveryDate.ToShortDateString() + " \n\n" +
                   $"Hi {order.FirstName}, \n\n" +
                   $"Thank you, we’ve received your payment for order {order.Id}\n\n" +
                   $"Your order is not out for delivery yet.> We'll now begin to process your order for delivery and it should be ready from {order.deliveryDate.ToLongDateString()}.\n\n" +
                   $"We'll email you the moment it's ready." +
                   $"Regards,\r\nPoultry Team";
                    email2.Body = emailBody;


                    var smtpClient = new SmtpClient();

                    smtpClient.Send(email2);

                }
                catch (Exception ex)
                {
                    TempData["Message"] = "Failed to send email due to, " + ex.Message;
                    return RedirectToAction("Index", "ShoppingCart");
                }



                


                cart.CreateOrder(order);
                db.SaveChanges();




                return View();
            }
            else
            {
               
                var cart = ShoppingCart.GetCart(this.HttpContext);
                var order = new CustomerOrder();
                TryUpdateModel(order);
                order.CustomerUserName = User.Identity.Name;
                order.DateCreated = DateTime.Now;

               
                //order.Country = "";
              
                order.ShippingMethod = "PickUp";
                
                Meths meth = new Meths();
                int UniqueCode = Meths.GenerateRandomCode();
                bool conflict = db.CustomerOrders.Where(x => x.UniqueCode == UniqueCode).Any();
                while (conflict)
                {
                    UniqueCode = Meths.GenerateRandomCode();
                    conflict = db.CustomerOrders.Where(x => x.UniqueCode == UniqueCode).Any();
                }

                // Generate QR code
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(UniqueCode.ToString(), QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(20); // Change the size of the QR code image as needed

                // Convert the Bitmap to a byte array
                using (MemoryStream stream = new MemoryStream())
                {
                    qrCodeImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] byteArr = stream.ToArray();

                    // Save the image to a folder
                    string fileName = $"{Guid.NewGuid().ToString()}.png";
                    string filePath = Path.Combine(Server.MapPath("~/images/"), fileName);
                    System.IO.File.WriteAllBytes(filePath, byteArr);

                    // Save the filename to the database
                    order.qrCodePicture = fileName;

                    order.UniqueCode = UniqueCode;

                    order.deliveryDate = meth.GetWorkingDay(DateTime.Now, 3);
                    order.deliveryFee = 0;

                    order.Status = "Placed";
                    order.Email = User.Identity.Name;
                    order.Amount = cart.GetTotal();

                    db.CustomerOrders.Add(order);

                }






                try
                {
                    // Prepare email message
                    var email2 = new MailMessage();
                    email2.From = new MailAddress("Poultry.dbn@outlook.com");
                    email2.To.Add(User.Identity.Name);
                    email2.Subject = "Payment Confirmation |  " + order.Id;
                    string emailBody = $"Order Number: "+ order.Id+ "\t\t Estimated Pickup Date: "+ order.deliveryDate.ToShortDateString()+ " \n\n" +
                   $"Hi {order.FirstName}, \n\n" +
                   $"Thank you, we’ve received your payment for order {order.Id}\n\n" +
                   $"<b>Your order is not ready for collection yet.</b> We'll now begin to process your order for collection and it should be ready from {order.deliveryDate.ToLongDateString()}.\n\n" +
                   $"We'll email you the moment it's ready." +
                   $"Regards,\r\nPoultry Team";
                    email2.Body = emailBody;

                   
                    var smtpClient = new SmtpClient();
                    
                    smtpClient.Send(email2);

                }
                catch (Exception ex)
                {
                    TempData["Message"] = "Failed to send email due to, " + ex.Message;
                    return RedirectToAction("Index", "ShoppingCart");
                }



              
           








                db.SaveChanges();

                cart.CreateOrder(order);
                db.SaveChanges();


                return View();
            }
        }

    }
}