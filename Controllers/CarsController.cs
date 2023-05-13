using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CarRentalHajurKo.Models;
using Microsoft.AspNetCore.Authorization;

namespace CarRentalHajurKo.Controllers
{
    [Authorize(Roles = "Admin")]
    [Authorize]
    public class CarsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHost;

        public CarsController(ApplicationDbContext context, IWebHostEnvironment wedHost)
        {
            _context = context;
            _webHost = wedHost;
        }

        // GET: Cars
        public async Task<IActionResult> Index()
        {
            return _context.Car != null ?
                        View(await _context.Car.ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.Car'  is null.");
        }

        // GET: Cars/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Car == null)
            {
                return NotFound();
            }

            var car = await _context.Car
                .FirstOrDefaultAsync(m => m.Id == id);
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        public async Task<IActionResult> ViewDetails(Guid id)
        {
            var car = await _context.Car.FirstOrDefaultAsync(m => m.Id == id);
            if (car == null)
            {
                return NotFound();
            }

            return View("Details", car);
        }

        // GET: Cars/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cars/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,Name,Model,Color,Year,RegistrationNo,PhotoUrl,status")] Car car)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        car.Id = Guid.NewGuid();
        //        _context.Add(car);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(car);
        //}
        public IActionResult Create(Car car)
        {
            string uniqueFileName = GetProfilePhotoFileName(car);
            car.PhotoUrl = uniqueFileName;

            _context.Add(car);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // GET: Cars/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Car == null)
            {
                return NotFound();
            }

            var car = await _context.Car.FindAsync(id);
            if (car == null)
            {
                return NotFound();
            }
            return View(car);
        }

        // POST: Cars/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,Model,Color,Year,RegistrationNo,PhotoUrl,status")] Car car)
        //{
        //    if (id != car.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(car);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!CarExists(car.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(car);
        //}

        public IActionResult Edit(Car car)
        {
            if (car.Photo != null)
            {
                string uniqueFileName = GetProfilePhotoFileName(car);
                car.PhotoUrl = uniqueFileName;
            }
            _context.Attach(car);
            _context.Entry(car).State = EntityState.Modified;
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // GET: Cars/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Car == null)
            {
                return NotFound();
            }

            var car = await _context.Car
                .FirstOrDefaultAsync(m => m.Id == id);
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        // POST: Cars/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Car == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Car'  is null.");
            }
            var car = await _context.Car.FindAsync(id);
            if (car != null)
            {
                _context.Car.Remove(car);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CarExists(Guid id)
        {
            return (_context.Car?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        private string GetProfilePhotoFileName(Car car)
        {
            string uniqueFileName = null;

            if (car.Photo != null)
            {
                string uploadsFolder = Path.Combine(_webHost.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + car.Photo.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    car.Photo.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }
    }
}
