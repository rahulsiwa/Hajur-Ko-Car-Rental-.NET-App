using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRentalHajurKo.Models;
using Microsoft.AspNetCore;
using Newtonsoft.Json;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Utils;
using Microsoft.Extensions.Configuration;

namespace CarRentalHajurKo.Controllers
{

    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHost;
        private readonly EmailSettings _emailSettings;

        public OrdersController(ApplicationDbContext context, IWebHostEnvironment webHost, IConfiguration configuration)
        {
            _context = context;
            _webHost = webHost;
            _emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>();
        }

        public class EmailSettings
        {
            public string SMTPServer { get; set; }
            public int SMTPPort { get; set; }
            public string SMTPUsername { get; set; }
            public string SMTPPassword { get; set; }
            public string SenderName { get; set; }
            public string SenderEmail { get; set; }
        }


        // GET: Orders
        public async Task<IActionResult> Index(string sortOrder, DateTime? searchDate)
        {
            ViewData["OrderDateSortParm"] = String.IsNullOrEmpty(sortOrder) ? "orderDate_desc" : "";

            var orders = from o in _context.Orders
                         select o;

            if (searchDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate.Date == searchDate.Value.Date);
            }

            switch (sortOrder)
            {
                case "orderDate_desc":
                    orders = orders.OrderByDescending(o => o.OrderDate);
                    break;
                default:
                    orders = orders.OrderBy(o => o.OrderDate);
                    break;
            }

            return View(await orders.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            return View();
        }
        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order)
        {
            string uniqueFileName = GetProfilePhotoFileName(order);
            order.PhotoUrl = uniqueFileName;

            _context.Add(order);
            _context.SaveChanges();

            // send email to the customer
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            emailMessage.To.Add(new MailboxAddress(order.Email, order.Email));
            emailMessage.Subject = "Car Rental Hajur Ko: Your Order Details";

            var builder = new BodyBuilder();
            builder.HtmlBody = "<p>Thank you for booking with Car Rental Hajur Ko. Here are your order details:</p>"
                              + "<ul>"
                              + "<li>Order No: " + order.OrderNo + "</li>"
                              + "<li>Customer ID: " + order.CustomerID + "</li>"
                              + "<li>Phone: " + order.Phone + "</li>"
                              + "<li>Email: " + order.Email + "</li>"
                              + "<li>Address: " + order.Address + "</li>"
                              + "<li>Order Date: " + order.OrderDate.ToShortDateString() + "</li>"
                              + "<li>Return Date: " + order.ReturnDate.ToShortDateString() + "</li>"
                              + "<li>Total Price: " + CalculateTotalPrice(order.OrderDate, order.ReturnDate, order.Rate) + "</li>"
                              + "</ul>";
            if (!string.IsNullOrEmpty(uniqueFileName))
            {
                var image = builder.LinkedResources.Add(Path.Combine(_webHost.WebRootPath, "images", uniqueFileName));
                image.ContentId = MimeUtils.GenerateMessageId();
                builder.HtmlBody += "<p>Photo:</p><img src=\"cid:" + image.ContentId + "\">";
            }

            emailMessage.Body = builder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_emailSettings.SMTPServer, _emailSettings.SMTPPort, false);
                await client.AuthenticateAsync(_emailSettings.SMTPUsername, _emailSettings.SMTPPassword);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }

            TempData["SuccessMessage"] = $"Successfully Booked. Email: {order.Email}";

            return RedirectToAction("Index", "Home");
        }



        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,OrderNo,CustomerID,Phone,Email,Address,OrderDate,ReturnDate,PhotoUrl")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Orders == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Orders'  is null.");
            }
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(Guid id)
        {
            return (_context.Orders?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        private string GetProfilePhotoFileName(Order order)
        {
            string uniqueFileName = null;

            if (order.Photo != null)
            {
                string uploadsFolder = Path.Combine(_webHost.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + order.Photo.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    order.Photo.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }

        private decimal CalculateTotalPrice(DateTime orderDate, DateTime returnDate, int dailyPrice)
        {
            TimeSpan duration = returnDate - orderDate;
            int numberOfDays = duration.Days;

            decimal totalPrice = numberOfDays * dailyPrice;

            return totalPrice;
        }



    }
}
