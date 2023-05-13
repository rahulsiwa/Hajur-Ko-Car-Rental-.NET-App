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
    public class DamageReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        
        private readonly EmailSettings _emailSettings;


        public DamageReportsController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
           

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

        // GET: DamageReports
        public async Task<IActionResult> Index()
        {
              return _context.DamageReport != null ? 
                          View(await _context.DamageReport.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.DamageReport'  is null.");
        }

        // GET: DamageReports/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.DamageReport == null)
            {
                return NotFound();
            }

            var damageReport = await _context.DamageReport
                .FirstOrDefaultAsync(m => m.Id == id);
            if (damageReport == null)
            {
                return NotFound();
            }

            return View(damageReport);
        }

        // GET: DamageReports/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DamageReports/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DamageReport damageReport)
        {

            _context.Add(damageReport);
            _context.SaveChanges();

            // send email to the customer
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            emailMessage.To.Add(new MailboxAddress(damageReport.Email, damageReport.Email));
            emailMessage.Subject = "Hajur Ko Car Rental: Your Damage Request Form Details";

            var builder = new BodyBuilder();
            builder.HtmlBody = "<p>Thank you for submitting the Damage Request form. We'll get back to you shortly. The details you filled in the form are given below:</p>"
                              + "<ul>"
                              + "<li>Order No: " + damageReport.OrderNo + "</li>"
                              + "<li>Customer ID: " + damageReport.CustomerID + "</li>"
                              + "<li>Customer Name: " + damageReport.CustomerName + "</li>"
                              + "<li>Email: " + damageReport.Email + "</li>"
                              + "<li>Car Model: " + damageReport.CarModel + "</li>"
                              + "<li>Damaged Part: " + damageReport.DamagePart + "</li>"
                              + "<li>Damaged Fee: " + damageReport.DamageFee + "</li>"

                              + "</ul>";
          
            emailMessage.Body = builder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_emailSettings.SMTPServer, _emailSettings.SMTPPort, false);
                await client.AuthenticateAsync(_emailSettings.SMTPUsername, _emailSettings.SMTPPassword);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }

            TempData["SuccessMessage"] = $"Damage report Successfully Sent in the Email: {damageReport.Email}";

            return RedirectToAction("Index", "Home");
        }

        // GET: DamageReports/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.DamageReport == null)
            {
                return NotFound();
            }

            var damageReport = await _context.DamageReport.FindAsync(id);
            if (damageReport == null)
            {
                return NotFound();
            }
            return View(damageReport);
        }

        // POST: DamageReports/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,OrderNo,CustomerID,CustomerName,Email,CarModel,DamagePart,DamageFee")] DamageReport damageReport)
        {
            if (id != damageReport.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(damageReport);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DamageReportExists(damageReport.Id))
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
            return View(damageReport);
        }

        // GET: DamageReports/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.DamageReport == null)
            {
                return NotFound();
            }

            var damageReport = await _context.DamageReport
                .FirstOrDefaultAsync(m => m.Id == id);
            if (damageReport == null)
            {
                return NotFound();
            }

            return View(damageReport);
        }

        // POST: DamageReports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.DamageReport == null)
            {
                return Problem("Entity set 'ApplicationDbContext.DamageReport'  is null.");
            }
            var damageReport = await _context.DamageReport.FindAsync(id);
            if (damageReport != null)
            {
                _context.DamageReport.Remove(damageReport);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DamageReportExists(Guid id)
        {
          return (_context.DamageReport?.Any(e => e.Id == id)).GetValueOrDefault();
        }

       
    }
}
