using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using Pet_Management_System.Models;

namespace Pet_Management_System.Controllers
{
    public class OrderedProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: OrderedProducts
        public ActionResult Index()
        {

            var orderedProducts = db.Orderedproducts
                .Include(o => o.CustomerOrder)
                .Include(o => o.Product)
                
                .OrderBy(x => x.CustomerOrder.DateCreated)
                .ToList();

            // Group ordered products by date and time
            var groupedOrderedProducts = orderedProducts
                .GroupBy(op => Tuple.Create(op.CustomerOrder.DateCreated.Date, op.CustomerOrder.DateCreated.TimeOfDay))
                .ToList();

            return View(groupedOrderedProducts);
        }
       
        public ActionResult MyOrders()
        {
            var orderedProducts = db.Orderedproducts
                .Include(o => o.CustomerOrder)
                .Include(o => o.Product)
                .Where(x => x.CustomerOrder.Email == User.Identity.Name)
                .OrderBy(x => x.CustomerOrder.DateCreated)
                .ToList();

            // Group ordered products by date and time
            var groupedOrderedProducts = orderedProducts
                .GroupBy(op => Tuple.Create(op.CustomerOrder.DateCreated.Date, op.CustomerOrder.DateCreated.TimeOfDay))
                .ToList();

            return View(groupedOrderedProducts);
        }
        public ActionResult ReturnRequests()
        {
            var orderedProducts = db.Orderedproducts
                .Include(o => o.CustomerOrder)
                .Include(o => o.Product)

                .OrderBy(x => x.CustomerOrder.DateCreated)
                .ToList();

            // Group ordered products by date and time
            var groupedOrderedProducts = orderedProducts
                .GroupBy(op => Tuple.Create(op.CustomerOrder.DateCreated.Date, op.CustomerOrder.DateCreated.TimeOfDay))
                .ToList();

            return View(groupedOrderedProducts);
        }

        public ActionResult MyReturnRequests()
        {
           
            var orderedProducts = db.Orderedproducts
               .Include(o => o.CustomerOrder)
               .Include(o => o.Product)
               .Where(x => x.CustomerOrder.Email == User.Identity.Name)
               .OrderBy(x => x.CustomerOrder.DateCreated)
               .ToList();

            // Group ordered products by date and time
            var groupedOrderedProducts = orderedProducts
                .GroupBy(op => Tuple.Create(op.CustomerOrder.DateCreated.Date, op.CustomerOrder.DateCreated.TimeOfDay))
                .ToList();

            return View(groupedOrderedProducts);
        }
        public ActionResult TrackOrder(int id)
        {
            var order = db.Orderedproducts
                .Include(o => o.CustomerOrder)
                .Include(o => o.Product).Where(x => x.CustomerOrderId == id).FirstOrDefault();
            return View(order);
        }
        public ActionResult TrackReturn(int id)
        {
            var order = db.Orderedproducts
                .Include(o => o.CustomerOrder)
                .Include(o => o.Product).Where(x => x.CustomerOrderId == id).FirstOrDefault();
            return View(order);
        }
        public ActionResult ReturnOrder(int id,string reason)
        {
            var orderedProduct = db.Orderedproducts.Where(x=>x.CustomerOrderId == id).FirstOrDefault();
            var order = db.CustomerOrders.Find(id);
            order.Status = "Requested Return";
            OrderReturn orderReturn = new OrderReturn();
            orderReturn.ProductId = orderedProduct.ProductId;
            orderReturn.CustomerOrderId = id;
            orderReturn.Reason = reason;
            orderReturn.Status = "Awaiting Approval";
            orderReturn.Created = DateTime.Now;
            db.Entry(order).State = EntityState.Modified;
            db.OrderReturns.Add(orderReturn);
            try
            {
                // Prepare email message
                var email2 = new MailMessage();
                email2.From = new MailAddress("Poultry.dbn@outlook.com");
                email2.To.Add(order.Email);
                email2.Subject = "Return Request Submitted";
                string emailBody = $"Order Number: " + order.Id + " \n\n" +
               $"Hi {order.FirstName}, \n\n" +
               $"Your return request for {order.Id} was submitted sucessfully.\n\n" +
               $"Your Request will be reviewed ASP, sorry for any inconvinience caused  \n\n " +
               $"Please be on a lookout for updates regarding your request \n\n " +
               $"Thank you for shopping with us!\n\n" +
               $"Regards,\r\nPoultry Team";
                email2.Body = emailBody;


                var smtpClient = new SmtpClient();

                smtpClient.Send(email2);

            }
            catch (Exception ex)
            {
                TempData["Message"] = "Failed to send email due to, " + ex.Message;
                return RedirectToAction("MyOrders");
            }



            db.SaveChanges();
            TempData["Message"] = "Order return Request successfully submitted.  ";
            return RedirectToAction("MyReturnRequests");
        }
        public ActionResult ApproveReturn(int id)
        {
            var orderReturn = db.OrderReturns.Find(id);
            var order = db.CustomerOrders.Find(orderReturn.CustomerOrderId);
            order.Status = "Schedule Return";
            orderReturn.Status = "Approved";
            db.Entry(order).State = EntityState.Modified;
            db.Entry(orderReturn).State = EntityState.Modified;

            try
            {
                // Prepare email message
                var email2 = new MailMessage();
                email2.From = new MailAddress("Poultry.dbn@outlook.com");
                email2.To.Add(order.Email);
                email2.Subject = "Return Request Approved";
                string emailBody = $"Order Number: " + order.Id + " \n\n" +
               $"Hi {order.FirstName}, \n\n" +
               $"Your return request for {order.Id} was approved.\n\n" +
               $"Driver is not out to pickup your order for return as yet, we will inform you when the driver is about to pickup the order for return.\n\n " +
               $"Thank you for shopping with us!\n\n" +
               $"Regards,\r\nPoultry Team";
                email2.Body = emailBody;


                var smtpClient = new SmtpClient();

                smtpClient.Send(email2);

            }
            catch (Exception ex)
            {
                TempData["Message"] = "Failed to send email due to, " + ex.Message;
                return RedirectToAction("ReturnRequests");
            }


            db.SaveChanges();
            TempData["Message"] = "Order return Request approved.";
            return RedirectToAction("ReturnRequests");

        }

        public ActionResult DeclineReturn(int id, string reason)
        {
            var orderReturn = db.OrderReturns.Find(id);
            var order = db.CustomerOrders.Find(orderReturn.CustomerOrderId);
            order.Status = "Return Declined";
            orderReturn.Status = "Declined";
            orderReturn.DeclineReason = reason;
            db.Entry(order).State = EntityState.Modified;
            db.Entry(orderReturn).State = EntityState.Modified;

            try
            {
                // Prepare email message
                var email2 = new MailMessage();
                email2.From = new MailAddress("Poultry.dbn@outlook.com");
                email2.To.Add(order.Email);
                email2.Subject = "Return Request Declined";
                string emailBody = $"Order Number: " + order.Id + " \n\n" +
               $"Hi {order.FirstName}, \n\n" +
               $"Your return request for {order.Id} was declined with reason {reason}.\n\n" +
               $"Sorry for any inconvenience caused, if you require more information you can visit our head office at 9 Lancers Road, Berea Grayville, Durban 4001. or give us a call on +27 873 873 873\n\n " +
               $"Thank you for shopping with us!\n\n" +
               $"Regards,\r\nPoultry Team";
                email2.Body = emailBody;


                var smtpClient = new SmtpClient();

                smtpClient.Send(email2);

            }
            catch (Exception ex)
            {
                TempData["Message"] = "Failed to send email due to, " + ex.Message;
                return RedirectToAction("ReturnRequests");
            }


            db.SaveChanges();
            TempData["Message"] = "Order return Request Declined.";
            return RedirectToAction("ReturnRequests");

        }
        public ActionResult DeclineReturnDriv(int id, string reason)
        {
            var order = db.CustomerOrders.Find(id);
            var orderReturn = db.OrderReturns.Where(x=>x.CustomerOrderId == id).FirstOrDefault();
            
            order.Status = "Return Declined";
            orderReturn.Status = "Declined";
            orderReturn.DeclineReason = reason;
            db.Entry(order).State = EntityState.Modified;
            db.Entry(orderReturn).State = EntityState.Modified;

            try
            {
                // Prepare email message
                var email2 = new MailMessage();
                email2.From = new MailAddress("Poultry.dbn@outlook.com");
                email2.To.Add(order.Email);
                email2.Subject = "Return Request Declined";
                string emailBody = $"Order Number: " + order.Id + " \n\n" +
               $"Hi {order.FirstName}, \n\n" +
               $"Your return request for {order.Id} was declined by driver with reason {reason}.\n\n" +
               $"Sorry for any inconvenience caused, if you require more information you can visit our head office at 9 Lancers Road, Berea Grayville, Durban 4001. or give us a call on +27 873 873 873\n\n " +
               $"Thank you for shopping with us!\n\n" +
               $"Regards,\r\nPoultry Team";
                email2.Body = emailBody;


                var smtpClient = new SmtpClient();

                smtpClient.Send(email2);

            }
            catch (Exception ex)
            {
                TempData["Message"] = "Failed to send email due to, " + ex.Message;
                return RedirectToAction("ReturnRequests");
            }


            db.SaveChanges();
            TempData["Message"] = "Order return Request Declined.";
            return RedirectToAction("ReturnRequests");
        }
        public ActionResult CancelRequest(int id)
        {
            var orderReturn = db.OrderReturns.Find(id);
            var order = db.CustomerOrders.Find(orderReturn.CustomerOrderId);
            order.Status = "Order Recived + Cancelled Return";
            orderReturn.Status = "Cancelled";
            
            db.Entry(order).State = EntityState.Modified;
            db.Entry(orderReturn).State = EntityState.Modified;

            try
            {
                // Prepare email message
                var email2 = new MailMessage();
                email2.From = new MailAddress("Poultry.dbn@outlook.com");
                email2.To.Add(order.Email);
                email2.Subject = "Return Request Cancelled";
                string emailBody = $"Order Number: " + order.Id + " \n\n" +
               $"Hi {order.FirstName}, \n\n" +
               $"Your return request for {order.Id} was Cancelled Sucessfully.\n\n" +
               
               $"Thank you for shopping with us!\n\n" +
               $"Regards,\r\nPoultry Team";
                email2.Body = emailBody;


                var smtpClient = new SmtpClient();

                smtpClient.Send(email2);

            }
            catch (Exception ex)
            {
                TempData["Message"] = "Failed to send email due to, " + ex.Message;
                return RedirectToAction("MyReturnRequests");
            }


            db.SaveChanges();
            TempData["Message"] = "Order return Request Declined.";
            return RedirectToAction("MyReturnRequests");
        }
        // GET: OrderedProducts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrderedProduct orderedProduct = db.Orderedproducts.Find(id);
            if (orderedProduct == null)
            {
                return HttpNotFound();
            }
            return View(orderedProduct);
        }

        // GET: OrderedProducts/Create
        public ActionResult Create()
        {
            ViewBag.CustomerOrderId = new SelectList(db.CustomerOrders, "Id", "FirstName");
            ViewBag.ProductId = new SelectList(db.Products, "Id", "Name");
            return View();
        }

        // POST: OrderedProducts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductId,CustomerOrderId,Quantity")] OrderedProduct orderedProduct)
        {
            if (ModelState.IsValid)
            {
                db.Orderedproducts.Add(orderedProduct);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CustomerOrderId = new SelectList(db.CustomerOrders, "Id", "FirstName", orderedProduct.CustomerOrderId);
            ViewBag.ProductId = new SelectList(db.Products, "Id", "Name", orderedProduct.ProductId);
            return View(orderedProduct);
        }

        // GET: OrderedProducts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrderedProduct orderedProduct = db.Orderedproducts.Find(id);
            if (orderedProduct == null)
            {
                return HttpNotFound();
            }
            ViewBag.CustomerOrderId = new SelectList(db.CustomerOrders, "Id", "FirstName", orderedProduct.CustomerOrderId);
            ViewBag.ProductId = new SelectList(db.Products, "Id", "Name", orderedProduct.ProductId);
            return View(orderedProduct);
        }

        // POST: OrderedProducts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductId,CustomerOrderId,Quantity")] OrderedProduct orderedProduct)
        {
            if (ModelState.IsValid)
            {
                db.Entry(orderedProduct).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CustomerOrderId = new SelectList(db.CustomerOrders, "Id", "FirstName", orderedProduct.CustomerOrderId);
            ViewBag.ProductId = new SelectList(db.Products, "Id", "Name", orderedProduct.ProductId);
            return View(orderedProduct);
        }

        // GET: OrderedProducts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrderedProduct orderedProduct = db.Orderedproducts.Find(id);
            if (orderedProduct == null)
            {
                return HttpNotFound();
            }
            return View(orderedProduct);
        }

        // POST: OrderedProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            OrderedProduct orderedProduct = db.Orderedproducts.Find(id);
            db.Orderedproducts.Remove(orderedProduct);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
