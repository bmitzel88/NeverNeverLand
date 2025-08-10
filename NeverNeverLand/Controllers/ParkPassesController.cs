using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NeverNeverLand.Data;
using NeverNeverLand.Models;

namespace NeverNeverLand.Controllers
{
    public class ParkPassesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ParkPassesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ParkPasses
        public async Task<IActionResult> Index()
        {
            return View(await _context.ParkPass.ToListAsync());
        }


        /// 
        /// ADMIN PANEL
        /// 



        // GET: ParkPasses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkPass = await _context.ParkPass
                .FirstOrDefaultAsync(m => m.Id == id);
            if (parkPass == null)
            {
                return NotFound();
            }

            return View(parkPass);
        }


        // GET: ParkPasses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ParkPasses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Price,ValidFrom,ValidUntil,IsActive")] ParkPass parkPass)
        {
            if (ModelState.IsValid)
            {
                _context.Add(parkPass);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(parkPass);
        }

        // GET: ParkPasses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkPass = await _context.ParkPass.FindAsync(id);
            if (parkPass == null)
            {
                return NotFound();
            }
            return View(parkPass);
        }

        // POST: ParkPasses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,ValidFrom,ValidUntil,IsActive")] ParkPass parkPass)
        {
            if (id != parkPass.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(parkPass);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ParkPassExists(parkPass.Id))
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
            return View(parkPass);
        }

        // GET: ParkPasses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkPass = await _context.ParkPass
                .FirstOrDefaultAsync(m => m.Id == id);
            if (parkPass == null)
            {
                return NotFound();
            }

            return View(parkPass);
        }

        // POST: ParkPasses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var parkPass = await _context.ParkPass.FindAsync(id);
            if (parkPass != null)
            {
                _context.ParkPass.Remove(parkPass);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ParkPassExists(int id)
        {
            return _context.ParkPass.Any(e => e.Id == id);
        }
    }
}
