using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;

using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Web;

using System.Web.Mvc;


using System.Xml.Linq;

using Pet_Management_System.Models;
using System.Windows;
using System.Diagnostics.Eventing.Reader;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Pet_Management_System.Controllers
{
    public class DriverAssignmentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: DriverAssignments
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
        public ActionResult MyAssignments(string email = "Email")
        {
            if (email == "Email")
            {
                email = User.Identity.Name;
                ViewBag.Title = "MyAssignments";
            }
            else
            {
                ViewBag.Title = "Driver Assignments";
            }

            var orderedProducts = db.Orderedproducts
                .Include(o => o.CustomerOrder)
                .Include(o => o.Product)
                .Where(x => x.CustomerOrder.DriverEmail == email)
                .OrderBy(x => x.CustomerOrder.DateCreated)
                .ToList();

            // Group ordered products by date and time
            var groupedOrderedProducts = orderedProducts
                .GroupBy(op => Tuple.Create(op.CustomerOrder.DateCreated.Date, op.CustomerOrder.DateCreated.TimeOfDay))
                .ToList();

            return View(groupedOrderedProducts);
        }

        public ActionResult OrderReady(int id)
        {
            var assignment = db.DriverAssignments.Find(id);
            var order = db.CustomerOrders.Find(assignment.CustomerOrderId);
            assignment.Status = "Ready for pickup";
            order.Status = "Ready for pickup";
            db.Entry(assignment).State = EntityState.Modified;
            db.Entry(order).State = EntityState.Modified;

            try
            {
                // Prepare email message
                var email2 = new MailMessage();
                email2.From = new MailAddress("Poultry.dbn@outlook.com");
                email2.To.Add(assignment.Email);
                email2.Subject = "Order Ready |  " + assignment.CustomerOrderId;
                string emailBody = $"Order Number: " + assignment.CustomerOrderId + "\t\tDelivery Date: " + assignment.DeliveryDate + "\t\t Delivery Time: " + assignment.DeliveryTime + " \n\n" +
               $"Hi {assignment.Driver.Name}, \n\n" +
               $"Please note that, order {assignment.CustomerOrderId} is ready to be picked up for delivery to address {assignment.OrderedProduct.CustomerOrder.Address}, please procced with order delivery within due time.\n\n" +



               $"Regards,\r\nPoultry Team";
                email2.Body = emailBody;


                var smtpClient = new SmtpClient();

                smtpClient.Send(email2);

            }
            catch (Exception ex)
            {
                TempData["Message"] = "Failed to send email due to, " + ex.Message;
                return RedirectToAction("Index");
            }
            db.SaveChanges();
            TempData["Message"] = "Order Successfully Marked as Ready for Delivery";
            return RedirectToAction("Index");
        }
        public ActionResult StartOrderReturn(int id, string time)
        {
            var assignment = db.DriverAssignments.Find(id);
            assignment.DeliveryTime = time;
            var order = db.CustomerOrders.Find(assignment.CustomerOrderId);
            assignment.Status = "Driver On the way";
            order.Status = "Driver On the way";
            db.Entry(assignment).State = EntityState.Modified;
            db.Entry(order).State = EntityState.Modified;

            try
            {
                // Prepare email message
                var email2 = new MailMessage();
                email2.From = new MailAddress("Poultry.dbn@outlook.com");
                email2.To.Add(order.Email);
                email2.Subject = "Driver is the way to make return| " + assignment.CustomerOrderId;
                string emailBody = $"Order Number: " + assignment.CustomerOrderId + "\t\t Estimated Arrival Time: " + time + " \n\n" +
                    $"Hi {assignment.Driver.Name}, \n\n" +
                    $"Please note that, driver is now navigating to {order.Address} to pickup order  {assignment.CustomerOrderId} for return.\n\n" +
                    $"Your order is now out for delivery. The driver will be at your door around {time}.\n\n" +
                    $"Please present this unique code to the driver: {order.UniqueCode}\n\n or the attached QR Code." +

                    $"Regards,\r\nPoultry Team";
                email2.Body = emailBody;

                // Correct the attachment path
                string imagePath = Server.MapPath("~/images/" + order.qrCodePicture);
                Attachment attachment = new Attachment(imagePath, MediaTypeNames.Image.Jpeg);

                // Add the attachment to the email
                email2.Attachments.Add(attachment);
                var smtpClient = new SmtpClient();

                smtpClient.Send(email2);

            }
            catch (Exception ex)
            {
                TempData["Message"] = "Failed to send email due to, " + ex.Message;
                return RedirectToAction("MyAssignments");
            }
            db.SaveChanges();
            TempData["Message"] = "Order Return Sucessfully Started Please Navigate to Customer Location";
            return RedirectToAction("Navigation", new { id = order.Id });
        }

        public ActionResult StartOrderDelivery(int id, string time)
        {
            var assignment = db.DriverAssignments.Find(id);
            assignment.DeliveryTime = time;
            var order = db.CustomerOrders.Find(assignment.CustomerOrderId);
            assignment.Status = "On the way";
            order.Status = "On the way";
            db.Entry(assignment).State = EntityState.Modified;
            db.Entry(order).State = EntityState.Modified;

            try
            {
                // Prepare email message
                var email2 = new MailMessage();
                email2.From = new MailAddress("Poultry.dbn@outlook.com");
                email2.To.Add(order.Email);
                email2.Subject = "Order Delivery Started | " + assignment.CustomerOrderId;
                string emailBody = $"Order Number: " + assignment.CustomerOrderId + "\t\t Estimated Arrival Time: " + time + " \n\n" +
                    $"Hi {assignment.Driver.Name}, \n\n" +
                    $"Please note that, order {assignment.CustomerOrderId} is picked up by friver for delivery to address {assignment.OrderedProduct.CustomerOrder.Address}.\n\n" +
                    $"Your order is now out for delivery. The driver will be at your door around {time}.\n\n" +
                    $"Please present this unique code to the driver: {order.UniqueCode}\n\n or the attached QR Code." +

                    $"Regards,\r\nPoultry Team";
                email2.Body = emailBody;

                // Correct the attachment path
                string imagePath = Server.MapPath("~/images/" + order.qrCodePicture);
                Attachment attachment = new Attachment(imagePath, MediaTypeNames.Image.Jpeg);

                // Add the attachment to the email
                email2.Attachments.Add(attachment);
                var smtpClient = new SmtpClient();

                smtpClient.Send(email2);

            }
            catch (Exception ex)
            {
                TempData["Message"] = "Failed to send email due to, " + ex.Message;
                return RedirectToAction("MyAssignments");
            }
            db.SaveChanges();
            TempData["Message"] = "Order Delivery Sucessfully Started Please Navigate to Customer Location";
            return RedirectToAction("Navigation", new { id = order.Id });
        }
        public ActionResult Navigation(int id)
        {
            HttpCookie cookie = new HttpCookie("OrdID");

            // Set cookie value
            cookie.Value = id.ToString();

            // Set cookie expiration (optional)
            cookie.Expires = DateTime.Now.AddDays(1);

            // Add the cookie to the response
            Response.Cookies.Add(cookie);




            var order = db.CustomerOrders.Find(id);
            ViewBag.DestinationAddress = order.Address;
            return View();
        }
        public ActionResult FinishOrderDelivery()
        {
            return View();
        }
        [HttpPost]
        public ActionResult FinishOrderDelivery(string code)
        {


            HttpCookie cookie = Request.Cookies["OrdID"];
            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                string ordId = cookie.Value;
                int id = int.Parse(ordId);
                var order = db.CustomerOrders.Find(id);
                if (order.DeliveryType != "Return")
                {
                    var assign = db.DriverAssignments.Where(x => x.CustomerOrderId == id && x.Email == User.Identity.Name).FirstOrDefault();
                    db.Entry(order).State = EntityState.Modified;
                    db.Entry(assign).State = EntityState.Modified;
                    if (int.Parse(code) == order.UniqueCode)
                    {
                        order.Status = "Order Recived";
                        order.DeliveredOn = DateTime.Now;
                        order.DeliveredBy = assign.DrivId;
                        assign.Status = "Completed";

                        assign.IsActive = false;
                        try
                        {
                            // Prepare email message
                            var email2 = new MailMessage();
                            email2.From = new MailAddress("Poultry.dbn@outlook.com");
                            email2.To.Add(order.Email);
                            email2.Subject = "Order Delivered";
                            string emailBody = $"Order Number: " + order.Id + " \n\n" +
                           $"Hi {order.FirstName}, \n\n" +
                           $"Your order {order.Id} has been delivered to {order.Address} on {DateTime.Now.Date.ToLongDateString()} at {DateTime.Now.ToShortTimeString()}.\n\n" +

                           $"Thank you for shopping with us!\n\n" +
                           $"Regards,\r\nPoultry Team";
                            email2.Body = emailBody;


                            var smtpClient = new SmtpClient();

                            smtpClient.Send(email2);

                        }
                        catch (Exception ex)
                        {
                            TempData["Message"] = "Failed to send email due to, " + ex.Message;
                            return RedirectToAction("FinishOrderDelivery");
                        }

                        db.SaveChanges();

                    }
                    else
                    {
                        TempData["Message"] = "Incorrect Code, Please Try again";
                        return RedirectToAction("FinishOrderDelivery");
                    }
                    TempData["Message"] = "Order Delivered Sucessfully";
                    return RedirectToAction("MyAssignments");
                }
                else
                {
                    return RedirectToAction("FinishReturn", new { code });
                }


            }
            else
            {
                TempData["Message"] = "Something went wrong";
                return RedirectToAction("MyAssignments");
            }

        }
        public ActionResult FinishReturn(string code)
        {
            Session["Code"] = code;
           
            return View();
        }
        [HttpPost]
        public ActionResult FinishReturn(string drivSignature, string custSignature)
        {
            if (Session["Code"] != null)
            {


                string code = Session["Code"] as string;

                HttpCookie cookie = Request.Cookies["OrdID"];
                if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
                {
                    string ordId = cookie.Value;
                    int id = int.Parse(ordId);
                    var order = db.CustomerOrders.Find(id);
                    var assign = db.DriverAssignments.Where(x => x.CustomerOrderId == id && x.Email == User.Identity.Name).FirstOrDefault();
                    db.Entry(order).State = EntityState.Modified;
                    db.Entry(assign).State = EntityState.Modified;
                    if (int.Parse(code) == order.UniqueCode)
                    {
                        order.Status = "Order Collected";
                        order.DeliveredOn = DateTime.Now;
                        order.DeliveredBy = assign.DrivId;
                        assign.Status = "Completed";

                        assign.IsActive = false;


                        try
                        {


                            MemoryStream memoryStream = new MemoryStream();
                            Document document = new Document(PageSize.A5, 0, 0, 0, 0);
                            PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                            document.Open();



                            // Add a title
                            Font titleFont = new Font(Font.FontFamily.HELVETICA, 24, Font.BOLD);
                            Paragraph title = new Paragraph("Order Return Document", titleFont);
                            title.Alignment = Element.ALIGN_CENTER;
                            document.Add(title);

                            // Create the heading paragraph with the headig font
                            PdfPTable table1 = new PdfPTable(1);
                            PdfPTable table2 = new PdfPTable(5);
                            PdfPTable table3 = new PdfPTable(1);

                            iTextSharp.text.pdf.draw.VerticalPositionMark seperator = new iTextSharp.text.pdf.draw.LineSeparator();
                            seperator.Offset = -6f;
                            // Remove table cell
                            table1.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                            table3.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;

                            table1.WidthPercentage = 80;
                            table1.SetWidths(new float[] { 100 });
                            table2.WidthPercentage = 80;
                            table3.SetWidths(new float[] { 100 });
                            table3.WidthPercentage = 80;

                            PdfPCell subtitleCell = new PdfPCell();
                            subtitleCell.AddElement(new Paragraph("Return Details"));
                            subtitleCell.HorizontalAlignment = Element.ALIGN_CENTER;

                            PdfPCell signatureTitleCell = new PdfPCell();
                            signatureTitleCell.AddElement(new Paragraph("Customer Signature"));
                            signatureTitleCell.HorizontalAlignment = Element.ALIGN_CENTER; 

                            PdfPCell drivSignatureTitleCell = new PdfPCell();
                            drivSignatureTitleCell.AddElement(new Paragraph("Driver Signature"));
                            drivSignatureTitleCell.HorizontalAlignment = Element.ALIGN_CENTER;


                            PdfPCell cell = new PdfPCell(new Phrase(""));
                            cell.Colspan = 3;
                            table1.AddCell("\n");
                            table1.AddCell(cell);
                            table1.AddCell("\n\n");
                            table1.AddCell(
                                "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t" +
                                "Poultry \n" +
                                "Email :Poultry.dbn@outlook.com" + "\n" +
                                "\n" + "\n");
                            table1.AddCell(subtitleCell);
                            table1.AddCell("Customer Name : \t" + order.FirstName);
                            table1.AddCell("Customer Surname : \t" + order.LastName);
                            table1.AddCell("Client Email : \t" + order.Email);
                            table2.AddCell("Driver Name : \t" + assign.Driver.Name);
                            table2.AddCell("Driver Surname : \t" + assign.Driver.Surname);
                            table1.AddCell("Driver Email : \t" +assign.Driver.Email);
                            table1.AddCell("Basic Cost: \tR " + order.Amount);
                            table1.AddCell("Delivery Cost : \t" + order.deliveryFee);
                            table1.AddCell("Total Cost : \tR " + order.deliveryFee + order.Amount);


                            table1.AddCell("\n\n");
                            table1.AddCell(signatureTitleCell);






                            // Convert the signature image to a byte array
                            var CustcleanerBase64 = custSignature.Substring(22);
                            byte[] CustsignatureBytes = Convert.FromBase64String(CustcleanerBase64);

                            // Create an image from the byte array
                            iTextSharp.text.Image CustsignatureImage = iTextSharp.text.Image.GetInstance(CustsignatureBytes);

                            // Scale the image to fit within the cell
                            CustsignatureImage.ScaleToFit(100, 100);

                            // Add the image to the cell
                            PdfPCell CustsignatureCell = new PdfPCell(CustsignatureImage);
                            CustsignatureCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                            table1.AddCell(CustsignatureCell);



                            table1.AddCell("\n\n");
                            table1.AddCell(drivSignatureTitleCell);

                            // Convert the signature image to a byte array
                            var DrivcleanerBase64 = drivSignature.Substring(22);
                            byte[] DrivsignatureBytes = Convert.FromBase64String(DrivcleanerBase64);

                            // Create an image from the byte array
                            iTextSharp.text.Image DrivsignatureImage = iTextSharp.text.Image.GetInstance(DrivsignatureBytes);

                            // Scale the image to fit within the cell
                            DrivsignatureImage.ScaleToFit(100, 100);

                            // Add the image to the cell
                            PdfPCell DrivsignatureCell = new PdfPCell(DrivsignatureImage);
                            CustsignatureCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                            table1.AddCell(DrivsignatureCell);

                            table1.AddCell("\n");





                            table1.AddCell(cell);
                            document.Add(table1);

                            document.Add(table3);
                            document.Close();

                            byte[] bytes = memoryStream.ToArray();
                            memoryStream.Close();


                            var attachments = new List<Attachment>();
                            attachments.Add(new Attachment(new MemoryStream(bytes), "Order Return", "application/pdf"));
                            var email = new MailMessage();
                            email.From = new MailAddress("Poultry.dbn@outlook.com");
                            email.To.Add(order.Email);
                            email.Subject = "Order Returned";
                            email.Body = $"Order Number: " + order.Id + " \n\n" +
                               $"Hi {order.FirstName}, \n\n" +
                               $"Your order {order.Id} has been sucessfully collected for return from {order.Address} on {DateTime.Now.Date.ToLongDateString()} at {DateTime.Now.ToShortTimeString()}.\n\n" +

                               $"Thank you for shopping with us!\n\n" +
                               $"Regards,\r\nPoultry Team";
                            // Attach the files to the email
                            foreach (var attachment in attachments)
                            {
                                email.Attachments.Add(attachment);
                            }
                            // Use the SMTP settings from web.config
                            var smtpClient = new SmtpClient();

                            // The SmtpClient will automatically use the settings from web.config
                            smtpClient.Send(email);

                           
                            // Specify the file path and name
                            string filePath = Server.MapPath("~/images/") + order.Id + ".pdf";

                            // Write the PDF bytes to the file
                            System.IO.File.WriteAllBytes(filePath, bytes);


                        }
                        catch
                        {
                            TempData["Message"] = "System Failed to send email.";
                            return RedirectToAction("FinishOrderDelivery");
                        }

                        

                    }
                    else
                    {
                        TempData["Message"] = "Incorrect Code, Please Try again";
                        return RedirectToAction("FinishOrderDelivery");
                    }

                   
                }
                else
                {
                    TempData["Message"] = "Something Went Wrong please try again later";
                    return RedirectToAction("MyAssignments");
                }
            }
            else
            {
                TempData["Message"] = "Sorry your Session Ended";
                return RedirectToAction("FinishOrderDelivery");
            }
            db.SaveChanges();
            TempData["Message"] = "Order Returned Sucessfully";
            return RedirectToAction("MyAssignments");
        }


        [HttpPost]
        public ActionResult NoResponseAction()
        {
            HttpCookie cookie = Request.Cookies["OrdID"];
            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                string ordId = cookie.Value;
                int id = int.Parse(ordId);
                var order = db.CustomerOrders.Find(id);
                if (order.DeliveryType != "Return")
                {
                    var assign = db.DriverAssignments.Where(x => x.CustomerOrderId == id).FirstOrDefault();
                    db.Entry(order).State = EntityState.Modified;
                    db.Entry(assign).State = EntityState.Modified;

                    order.Status = "No Response";
                    assign.Status = "No Response";
                    assign.IsActive = false;
                    try
                    {
                        // Prepare email message
                        var email2 = new MailMessage();
                        email2.From = new MailAddress("Poultry.dbn@outlook.com");
                        email2.To.Add(order.Email);
                        email2.Subject = "No Response";
                        string emailBody = $"Order Number: " + order.Id + " \n\n" +
                       $"Hi {order.FirstName}, \n\n" +
                       $"Your order {order.Id} could not be delivered, driver tried to reach you at {order.Address} on {DateTime.Now.Date.ToLongDateString()} around {DateTime.Now.ToShortTimeString()} but there was no one to recieve order.\n\n" +
                       $"Your delivery will be rescheduled for another day. \n\n " +
                       $"Failure to collect order on the next date will result in cancellation and your order amount will be refunded to your account. \n\n " +
                       $"Thank you for shopping with us!\n\n" +
                       $"Regards,\r\nPoultry Team";
                        email2.Body = emailBody;


                        var smtpClient = new SmtpClient();

                        smtpClient.Send(email2);

                    }
                    catch (Exception ex)
                    {
                        TempData["Message"] = "Failed to send email due to, " + ex.Message;
                        return RedirectToAction("FinishOrderDelivery");
                    }

                    db.SaveChanges();
                }
                else
                {
                    var assign = db.DriverAssignments.Where(x => x.CustomerOrderId == id).FirstOrDefault();
                    var orderReturn = db.OrderReturns.Where(x => x.CustomerOrderId == id).FirstOrDefault();
                    orderReturn.Status = "Cancelled";
                    db.Entry(orderReturn).State = EntityState.Modified;
                    db.Entry(order).State = EntityState.Modified;
                    db.Entry(assign).State = EntityState.Modified;

                    order.Status = "No Response + Return";
                    assign.Status = "No Response + Return";
                    assign.IsActive = false;
                    try
                    {
                        // Prepare email message
                        var email2 = new MailMessage();
                        email2.From = new MailAddress("Poultry.dbn@outlook.com");
                        email2.To.Add(order.Email);
                        email2.Subject = "No Response";
                        string emailBody = $"Order Number: " + order.Id + " \n\n" +
                       $"Hi {order.FirstName}, \n\n" +
                       $"Your order {order.Id} could not be delivered, driver tried to reach you at {order.Address} on {DateTime.Now.Date.ToLongDateString()} around {DateTime.Now.ToShortTimeString()} but there was no one to recieve order.\n\n" +
                       $"Your delivery will be rescheduled for another day. \n\n " +
                       $"Failure to collect order on the next date will result in cancellation and your order amount will be refunded to your account. \n\n " +
                       $"Thank you for shopping with us!\n\n" +
                       $"Regards,\r\nPoultry Team";
                        email2.Body = emailBody;


                        var smtpClient = new SmtpClient();

                        smtpClient.Send(email2);

                    }
                    catch (Exception ex)
                    {
                        TempData["Message"] = "Failed to send email due to, " + ex.Message;
                        return RedirectToAction("FinishOrderDelivery");
                    }

                    db.SaveChanges();
                }




            }
            else
            {
                TempData["Message"] = "Something went Wrong Please try again later";
                return RedirectToAction("FinishOrderDelivery");
            }
            TempData["Message"] = "Order Delivery marked as no response from customer.";
            return RedirectToAction("MyAssignments");
        }

        // GET: DriverAssignments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DriverAssignment driverAssignment = db.DriverAssignments.Find(id);
            if (driverAssignment == null)
            {
                return HttpNotFound();
            }
            return View(driverAssignment);
        }

        // GET: DriverAssignments/Create
        public ActionResult Create(int id)
        {
            string ProductId = " ";
            string CustOrdID = " ";
            string deliveryType = " ";
          
            if (Session["ProductId"] != null)
            {
                ProductId = Session["ProductId"] as string;
                CustOrdID = Session["CustOrdID"] as string;
                deliveryType = Session["DeliveryType"] as string;
                if (deliveryType != "Return")
                {
                    ViewBag.Title = "Create";
                }
                else
                {
                    ViewBag.Title = "Schedule Return";
                }

            }
            else
            {
                
                TempData["Message"] = "Sorry Your Session Ended.";
                return RedirectToAction("Index", "OrderedProducts");
            }

            int productId = int.Parse(ProductId);
            int custOrdID = int.Parse(CustOrdID);


            var Driver = db.Drivers.Find(id);
            var order = db.CustomerOrders.Find(custOrdID);
            DriverAssignment b = new DriverAssignment()
            {
                preffaredTime = order.preffaredTime,
                GenDeliveryDate = order.deliveryDate.ToLongDateString(),
                ProductId = productId,
                CustomerOrderId = custOrdID,
                DrivId = id,
                Email = Driver.Email
            };


            return View(b);
        }

        // POST: DriverAssignments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AssDrivId,CustomerOrderId,ProductId,DrivId,Name,Surname,Email,Status,DeliveryDate,DeliveryTime,Created,GenDeliveryDate,preffaredTime")] DriverAssignment driverAssignment)
        {
            if (ModelState.IsValid)
            {
                string deliveryType = " ";
                if (Session["DeliveryType"] != null)
                {
                    deliveryType = Session["DeliveryType"] as string;

                }
                else
                {
                    TempData["Message"] = "Sorry Your Session Ended.";
                    return RedirectToAction("Index", "OrderedProducts");
                }
                
                if (deliveryType != "Return")
                {
                    var assign = db.DriverAssignments.Where(x => x.CustomerOrderId == driverAssignment.CustomerOrderId && x.DrivId == driverAssignment.DrivId).FirstOrDefault();
                    if (assign == null)
                    {
                        var assign2 = db.DriverAssignments.Where(x => x.CustomerOrderId == driverAssignment.CustomerOrderId).FirstOrDefault();
                        if (assign2 == null)
                        {
                            var driver = db.Drivers.Find(driverAssignment.DrivId);
                            var order = db.CustomerOrders.Find(driverAssignment.CustomerOrderId);
                            order.Status = "Delivery Scheduled";
                            order.DriverEmail = driver.Email;
                            driverAssignment.Surname = driver.Surname;
                            driverAssignment.Name = driver.Name;
                            driverAssignment.Status = "Assigned";
                            driverAssignment.Created = DateTime.Now;
                            driverAssignment.IsActive = true;
                            db.Entry(order).State = EntityState.Modified;
                            db.DriverAssignments.Add(driverAssignment);
                            try
                            {
                                // Prepare email message
                                var email = new MailMessage();
                                email.From = new MailAddress("Poultry.dbn@outlook.com");
                                email.To.Add(User.Identity.Name);
                                email.Subject = "Delivery Assignment |  " + order.Id;
                                string emailBody = $"Delivery Date: " + driverAssignment.DeliveryDate + "\t\t Estimated Delivery Time: " + driverAssignment.DeliveryTime + " \n\n" +
                               $"Hi {driver.Name}, \n\n" +
                               $"Please note that, you have been assigned to a new delivery for order {order.Id} to {order.Address}.\n\n" +


                               $"We'll email you the moment the order is ready for pickup." +
                               $"Regards,\r\nPoultry Team";
                                email.Body = emailBody;


                                var smtpClient = new SmtpClient();

                                smtpClient.Send(email);



                                // Prepare email message
                                var email2 = new MailMessage();
                                email2.From = new MailAddress("Poultry.dbn@outlook.com");
                                email2.To.Add(User.Identity.Name);
                                email2.Subject = "Delivery Scheduled |  " + order.Id;
                                string emailBody2 = $"Order Number: " + order.Id + "\t\tDelivery Date: " + driverAssignment.DeliveryDate + "\t\t Estimated Delivery Time: " + driverAssignment.DeliveryTime + " \n\n" +
                               $"Hi {order.FirstName}, \n\n" +
                               $"Please note that, we’ve scheduled delivery for order {order.Id}\n\n" +
                               $"Your order is not out for delivery yet. It should arrive on {driverAssignment.DeliveryDate} at {driverAssignment.DeliveryTime}.\n\n" +

                               $"We'll email you the moment the delivery starts." +
                               $"Regards,\r\nPoultry Team";
                                email2.Body = emailBody2;




                                smtpClient.Send(email2);

                            }
                            catch (Exception ex)
                            {
                                TempData["Message"] = "Failed to send email due to, " + ex.Message;
                                return RedirectToAction("Index", "OrderedProducts");
                            }
                            db.SaveChanges();

                            Session["ProductId"] = null;
                            Session["CustOrdID"] = null;

                            TempData["Message"] = "Driver Assigned Sucessfully";
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            var driver = db.Drivers.Find(driverAssignment.DrivId);
                            var order = db.CustomerOrders.Find(driverAssignment.CustomerOrderId);
                            order.Status = "Delivery Scheduled";
                            order.DriverEmail = driver.Email;
                            order.DeliveredBy = driverAssignment.DrivId;
                            order.IsDeliveryRescheduled = true;
                            
                            driverAssignment.Surname = driver.Surname;
                            driverAssignment.Name = driver.Name;
                            driverAssignment.Status = "Assigned";
                            driverAssignment.Created = DateTime.Now;
                            driverAssignment.IsActive = true;
                            db.Entry(order).State = EntityState.Modified;
                            db.DriverAssignments.Add(driverAssignment);
                            try
                            {
                                // Prepare email message
                                var email = new MailMessage();
                                email.From = new MailAddress("Poultry.dbn@outlook.com");
                                email.To.Add(User.Identity.Name);
                                email.Subject = "Delivery Assignment |  " + order.Id;
                                string emailBody = $"Delivery Date: " + driverAssignment.DeliveryDate + "\t\t Estimated Delivery Time: " + driverAssignment.DeliveryTime + " \n\n" +
                               $"Hi {driver.Name}, \n\n" +
                               $"Please note that, you have been assigned to a new delivery for order {order.Id} to {order.Address}.\n\n" +


                               $"We'll email you the moment the order is ready for pickup." +
                               $"Regards,\r\nPoultry Team";
                                email.Body = emailBody;


                                var smtpClient = new SmtpClient();

                                smtpClient.Send(email);

                                // Prepare email message
                                var email2 = new MailMessage();
                                email2.From = new MailAddress("Poultry.dbn@outlook.com");
                                email2.To.Add(User.Identity.Name);
                                email2.Subject = "Delivery Rescheduled |  " + order.Id;
                                string emailBody2 = $"Order Number: " + order.Id + "\t\tDelivery Date: " + driverAssignment.DeliveryDate + "\t\t Estimated Delivery Time: " + driverAssignment.DeliveryTime + " \n\n" +
                               $"Hi {order.FirstName}, \n\n" +
                               $"Please note that, we’ve rescheduled delivery for order {order.Id}\n\n" +
                               $"Your order is not out for delivery yet. It should arrive on {driverAssignment.DeliveryDate} at {driverAssignment.DeliveryTime}.\n\n" +

                               $"We'll email you the moment the delivery starts." +
                               $"Regards,\r\nPoultry Team";
                                email2.Body = emailBody2;




                                smtpClient.Send(email2);

                            }
                            catch (Exception ex)
                            {
                                TempData["Message"] = "Failed to send email due to, " + ex.Message;
                                return RedirectToAction("Index", "OrderedProducts");
                            }
                            db.SaveChanges();

                            Session["ProductId"] = null;
                            Session["CustOrdID"] = null;

                            TempData["Message"] = "Driver Assigned Sucessfully";
                            return RedirectToAction("Index");
                        }

                    }
                    else
                    {
                        var driver = db.Drivers.Find(driverAssignment.DrivId);
                        var order = db.CustomerOrders.Find(driverAssignment.CustomerOrderId);
                        order.Status = "Delivery Scheduled";
                        order.DriverEmail = driver.Email;
                        order.IsDeliveryRescheduled = true;
                        assign.Status = "Assigned";
                        assign.Created = DateTime.Now;
                        assign.IsActive = true;
                        db.Entry(order).State = EntityState.Modified;
                        db.Entry(assign).State = EntityState.Modified;

                        try
                        {
                            // Prepare email message
                            var email = new MailMessage();
                            email.From = new MailAddress("Poultry.dbn@outlook.com");
                            email.To.Add(User.Identity.Name);
                            email.Subject = "Delivery Assignment |  " + order.Id;
                            string emailBody = $"Delivery Date: " + driverAssignment.DeliveryDate + "\t\t Estimated Delivery Time: " + driverAssignment.DeliveryTime + " \n\n" +
                           $"Hi {driver.Name}, \n\n" +
                           $"Please note that, you have been assigned to a new delivery for order {order.Id} to {order.Address}.\n\n" +


                           $"We'll email you the moment the order is ready for pickup." +
                           $"Regards,\r\nPoultry Team";
                            email.Body = emailBody;


                            var smtpClient = new SmtpClient();

                            smtpClient.Send(email);


                            // Prepare email message
                            var email2 = new MailMessage();
                            email2.From = new MailAddress("Poultry.dbn@outlook.com");
                            email2.To.Add(User.Identity.Name);
                            email2.Subject = "Delivery Rescheduled |  " + order.Id;
                            string emailBody2 = $"Order Number: " + order.Id + "\t\tDelivery Date: " + driverAssignment.DeliveryDate + "\t\t Estimated Delivery Time: " + driverAssignment.DeliveryTime + " \n\n" +
                           $"Hi {order.FirstName}, \n\n" +
                           $"Please note that, we’ve rescheduled delivery for order {order.Id}\n\n" +
                           $"Your order is not out for delivery yet. It should arrive on {driverAssignment.DeliveryDate} at {driverAssignment.DeliveryTime}.\n\n" +

                           $"We'll email you the moment the delivery starts." +
                           $"Regards,\r\nPoultry Team";
                            email2.Body = emailBody2;




                            smtpClient.Send(email2);

                        }
                        catch (Exception ex)
                        {
                            TempData["Message"] = "Failed to send email due to, " + ex.Message;
                            return RedirectToAction("Index", "OrderedProducts");
                        }
                        db.SaveChanges();

                        Session["ProductId"] = null;
                        Session["CustOrdID"] = null;

                        TempData["Message"] = "Driver Assigned Sucessfully";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    var assign = db.DriverAssignments.Where(x => x.CustomerOrderId == driverAssignment.CustomerOrderId && x.DrivId == driverAssignment.DrivId).FirstOrDefault();
                    if (assign == null)
                    {


                        var driver = db.Drivers.Find(driverAssignment.DrivId);
                        var order = db.CustomerOrders.Find(driverAssignment.CustomerOrderId);
                        var orderReturn = db.OrderReturns.Where(x => x.CustomerOrderId == driverAssignment.CustomerOrderId).FirstOrDefault();
                        order.Status = "Return Scheduled";
                        order.DeliveryType = "Return";
                        order.DriverEmail = driver.Email;
                        orderReturn.Status = "Return Scheduled";
                        driverAssignment.Surname = driver.Surname;
                        driverAssignment.Name = driver.Name;
                        driverAssignment.Status = "Assigned";
                        driverAssignment.Created = DateTime.Now;
                        driverAssignment.IsActive = true;

                        db.Entry(order).State = EntityState.Modified;
                        db.Entry(orderReturn).State = EntityState.Modified;
                        db.DriverAssignments.Add(driverAssignment);
                        try
                        {
                            // Prepare email message
                            var email = new MailMessage();
                            email.From = new MailAddress("Poultry.dbn@outlook.com");
                            email.To.Add(driver.Email);
                            email.Subject = "Order Return Assignment |  " + order.Id;
                            string emailBody = $"Delivery Date: " + driverAssignment.DeliveryDate + "\t\t Estimated Delivery Time: " + driverAssignment.DeliveryTime + " \n\n" +
                           $"Hi {driver.Name}, \n\n" +
                           $"Please note that, you have been assigned to a new return for order {order.Id} at {order.Address}.\n\n" +


                           $"We'll email you the moment please proceed to make the return within due time." +
                           $"Regards,\r\nPoultry Team";
                            email.Body = emailBody;


                            var smtpClient = new SmtpClient();

                            smtpClient.Send(email);

                            // Prepare email message
                            var email2 = new MailMessage();
                            email2.From = new MailAddress("Poultry.dbn@outlook.com");
                            email2.To.Add(User.Identity.Name);
                            email2.Subject = "Order Return Scheduled |  " + order.Id;
                            string emailBody2 = $"Order Number: " + order.Id + "\t\tPuckup Date: " + driverAssignment.DeliveryDate + "\t\t Estimated Pickup Time: " + driverAssignment.DeliveryTime + " \n\n" +
                           $"Hi {order.FirstName}, \n\n" +
                           $"Please note that, we’ve scheduled return for {order.Id}\n\n" +
                           $"Deiver is not out to make the return yet. the driver should arrive on {driverAssignment.DeliveryDate} at {driverAssignment.DeliveryTime}.\n\n" +

                           $"We'll email you the moment the driver starts navigating to your place." +
                           $"Regards,\r\nPoultry Team";
                            email2.Body = emailBody2;




                            smtpClient.Send(email2);

                        }
                        catch (Exception ex)
                        {
                            TempData["Message"] = "Failed to send email due to, " + ex.Message;
                            return RedirectToAction("Index", "OrderedProducts");
                        }
                        db.SaveChanges();

                        Session["ProductId"] = null;
                        Session["CustOrdID"] = null;
                        Session["DeliveryType"] = null;
                        TempData["Message"] = "Driver Assigned Sucessfully";
                        return RedirectToAction("ReturnRequests", "OrderedProducts");


                    }
                    else
                    {
                        var driver = db.Drivers.Find(driverAssignment.DrivId);
                        var order = db.CustomerOrders.Find(driverAssignment.CustomerOrderId);
                        var orderReturn = db.OrderReturns.Where(x => x.CustomerOrderId == driverAssignment.CustomerOrderId).FirstOrDefault();
                        order.DriverEmail = driver.Email;
                        order.Status = "Return Scheduled";
                        order.DeliveryType = "Return";
                        orderReturn.Status = "Return Scheduled";
                        driverAssignment.Status = "Assigned";
                        assign.Created = DateTime.Now;
                        assign.IsActive = true;
                        db.Entry(orderReturn).State = EntityState.Modified;
                        db.Entry(order).State = EntityState.Modified;
                        db.Entry(assign).State = EntityState.Modified;

                        try
                        {
                            // Prepare email message
                            var email = new MailMessage();
                            email.From = new MailAddress("Poultry.dbn@outlook.com");
                            email.To.Add(driver.Email);
                            email.Subject = "Order Return Assignment |  " + order.Id;
                            string emailBody = $"Delivery Date: " + driverAssignment.DeliveryDate + "\t\t Estimated Delivery Time: " + driverAssignment.DeliveryTime + " \n\n" +
                           $"Hi {driver.Name}, \n\n" +
                           $"Please note that, you have been assigned to a new return for order {order.Id} at {order.Address}.\n\n" +


                           $"We'll email you the moment please proceed to make the return within due time." +
                           $"Regards,\r\nPoultry Team";
                            email.Body = emailBody;


                            var smtpClient = new SmtpClient();

                            smtpClient.Send(email);

                            // Prepare email message
                            var email2 = new MailMessage();
                            email2.From = new MailAddress("Poultry.dbn@outlook.com");
                            email2.To.Add(User.Identity.Name);
                            email2.Subject = "Order Return Scheduled |  " + order.Id;
                            string emailBody2 = $"Order Number: " + order.Id + "\t\tPuckup Date: " + driverAssignment.DeliveryDate + "\t\t Estimated Pickup Time: " + driverAssignment.DeliveryTime + " \n\n" +
                           $"Hi {order.FirstName}, \n\n" +
                           $"Please note that, we’ve scheduled return for {order.Id}\n\n" +
                           $"Deiver is not out to make the return yet. the driver should arrive on {driverAssignment.DeliveryDate} at {driverAssignment.DeliveryTime}.\n\n" +

                           $"We'll email you the moment the driver starts navigating to your place." +
                           $"Regards,\r\nPoultry Team";
                            email2.Body = emailBody2;




                            smtpClient.Send(email2);

                        }
                        catch (Exception ex)
                        {
                            TempData["Message"] = "Failed to send email due to, " + ex.Message;
                            return RedirectToAction("Index", "OrderedProducts");
                        }
                        db.SaveChanges();

                        Session["ProductId"] = null;
                        Session["CustOrdID"] = null;
                        Session["DeliveryType"] = null;
                        TempData["Message"] = "Driver Assigned Sucessfully";
                        return RedirectToAction("ReturnRequests", "OrderedProducts");
                    }
                }

            }


            return View(driverAssignment);
        }

        // GET: DriverAssignments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DriverAssignment driverAssignment = db.DriverAssignments.Find(id);
            if (driverAssignment == null)
            {
                return HttpNotFound();
            }
            ViewBag.Id = new SelectList(db.CustomerOrders, "Id", "FirstName", driverAssignment.ProductId);
            ViewBag.DrivId = new SelectList(db.Drivers, "DrivId", "Name", driverAssignment.DrivId);
            return View(driverAssignment);
        }

        // POST: DriverAssignments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AssDrivId,Id,DrivId,Name,Surname,Email,Status,DeliveryDate,DeliveryTime,Created")] DriverAssignment driverAssignment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(driverAssignment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Id = new SelectList(db.CustomerOrders, "Id", "FirstName", driverAssignment.ProductId);
            ViewBag.DrivId = new SelectList(db.Drivers, "DrivId", "Name", driverAssignment.DrivId);
            return View(driverAssignment);
        }

        // GET: DriverAssignments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DriverAssignment driverAssignment = db.DriverAssignments.Find(id);
            if (driverAssignment == null)
            {
                return HttpNotFound();
            }
            return View(driverAssignment);
        }

        // POST: DriverAssignments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DriverAssignment driverAssignment = db.DriverAssignments.Find(id);
            db.DriverAssignments.Remove(driverAssignment);
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
