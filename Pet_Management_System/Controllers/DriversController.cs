using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Pet_Management_System.Models;

namespace Pet_Management_System.Controllers
{
    public class DriversController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Drivers
        public ActionResult Index(int? prodId, int? custOrdID,string deliveryType = "Initial")
        {
            Session["DeliveryType"] = deliveryType;
            Session["ProductId"] = prodId.ToString();
            Session["CustOrdID"] = custOrdID.ToString();

            return View(db.Drivers.ToList());
        }
        public ActionResult MyProfile(string email ="Email" )
        {
            if(email== "Email")
            {
                email = User.Identity.Name;
                ViewBag.Title = "MyProfile";
            }
            else
            {
                ViewBag.Title = "Single";
            }
            
            return View(db.Drivers.Where(x=>x.Email == email).ToList());
        }

        // GET: Drivers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Driver driver = db.Drivers.Find(id);
            if (driver == null)
            {
                return HttpNotFound();
            }
            return View(driver);
        }

        // GET: Drivers/Create
        public ActionResult Create()
        {
            Driver b = new Driver()
            {
                Email = User.Identity.Name
            };
            return View(b);
        }

        // POST: Drivers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "DrivId,Name,Surname,Email,Picture,IsAvailable,CarName,CarModel,CarReg,CarType,PhoneNumber,Address")] Driver driver,HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                // Save the picture file on the server
                string pictureFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string picturePath = Path.Combine(Server.MapPath("~/images/"), pictureFileName);
                file.SaveAs(picturePath);

                // Set the picture path in the record
                driver.Picture = pictureFileName;
                db.Drivers.Add(driver);
                db.SaveChanges();
                return RedirectToAction("MyProfile");
            }

            return View(driver);
        }

        // GET: Drivers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Driver driver = db.Drivers.Find(id);
            if (driver == null)
            {
                return HttpNotFound();
            }
            return View(driver);
        }

        // POST: Drivers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "DrivId,Name,Surname,Email,Picture,IsAvailable,CarName,CarModel,CarReg,CarType")] Driver driver)
        {
            if (ModelState.IsValid)
            {
                db.Entry(driver).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(driver);
        }

        // GET: Drivers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Driver driver = db.Drivers.Find(id);
            if (driver == null)
            {
                return HttpNotFound();
            }
            return View(driver);
        }

        // POST: Drivers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Driver driver = db.Drivers.Find(id);
            db.Drivers.Remove(driver);
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
