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
    public class DamageRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHost;
        private readonly EmailSettings _emailSettings;

        public DamageRequestsController(ApplicationDbContext context, IWebHostEnvironment webHost, IConfiguration configuration)
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

        // GET: DamageRequests
        public async Task<IActionResult> Index()
        {
              return _context.DamageRequest != null ? 
                          View(await _context.DamageRequest.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.DamageRequest'  is null.");
        }

        // GET: DamageRequests/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.DamageRequest == null)
            {
                return NotFound();
            }

            var damageRequest = await _context.DamageRequest
                .FirstOrDefaultAsync(m => m.Id == id);
            if (damageRequest == null)
            {
                return NotFound();
            }

            return View(damageRequest);
        }

        // GET: DamageRequests/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DamageRequests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DamageRequest damageRequests)
        {
            string uniqueFileName = GetProfilePhotoFileName(damageRequests);
            damageRequests.PhotoUrl = uniqueFileName;

            _context.Add(damageRequests);
            _context.SaveChanges();

            // send email to the customer
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            emailMessage.To.Add(new MailboxAddress(damageRequests.Email, damageRequests.Email));
            emailMessage.Subject = "Hajur Ko Car Rental: Your Damage Request Form Details";

            var builder = new BodyBuilder();
            builder.HtmlBody = "<p>Thank you for submitting the Damage Request form. We'll get back to you shortly. The details you filled in the form are given below:</p>"
                              + "<ul>"
                              + "<li>Order No: " + damageRequests.OrderNo + "</li>"
                              + "<li>Customer ID: " + damageRequests.CustomerID + "</li>"
                              + "<li>Phone: " + damageRequests.Phone + "</li>"
                              + "<li>Email: " + damageRequests.Email + "</li>"
                              + "<li>Model: " + damageRequests.Model + "</li>"
                              + "<li>Damaged Part: " + damageRequests.DamagePart + "</li>"

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

            TempData["SuccessMessage"] = $"Damage Request Successfully Sent in the Email: {damageRequests.Email}";

            return RedirectToAction("Index", "Home");
        }

        // GET: DamageRequests/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.DamageRequest == null)
            {
                return NotFound();
            }

            var damageRequest = await _context.DamageRequest.FindAsync(id);
            if (damageRequest == null)
            {
                return NotFound();
            }
            return View(damageRequest);
        }

        // POST: DamageRequests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,OrderNo,CustomerID,Phone,Email,Model,DamagePart,PhotoUrl")] DamageRequest damageRequest)
        {
            if (id != damageRequest.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(damageRequest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DamageRequestExists(damageRequest.Id))
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
            return View(damageRequest);
        }

        // GET: DamageRequests/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.DamageRequest == null)
            {
                return NotFound();
            }

            var damageRequest = await _context.DamageRequest
                .FirstOrDefaultAsync(m => m.Id == id);
            if (damageRequest == null)
            {
                return NotFound();
            }

            return View(damageRequest);
        }

        // POST: DamageRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.DamageRequest == null)
            {
                return Problem("Entity set 'ApplicationDbContext.DamageRequest'  is null.");
            }
            var damageRequest = await _context.DamageRequest.FindAsync(id);
            if (damageRequest != null)
            {
                _context.DamageRequest.Remove(damageRequest);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DamageRequestExists(Guid id)
        {
          return (_context.DamageRequest?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private string GetProfilePhotoFileName(DamageRequest damageRequests)
        {
            string uniqueFileName = null;

            if (damageRequests.DamagedPartPhoto != null)
            {
                string uploadsFolder = Path.Combine(_webHost.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + damageRequests.DamagedPartPhoto.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    damageRequests.DamagedPartPhoto.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }
    }
}
