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

        // Season runs April 1 – Oct 31 
        private static readonly DateTime SeasonStart = new(DateTime.Now.Year, 4, 1);
        private static readonly DateTime SeasonEnd = new(DateTime.Now.Year, 10, 31);

        public ParkPassesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ParkPasses
        public IActionResult Index()
        {
            var today = DateTime.Today;

            // Pricing 
            const decimal earlyBird = 90m;   // Jan 1 – Mar 31
            const decimal regular = 120m;  // Apr 1 – Jul 31
            const decimal late = 96m;   // Aug 1 – Oct 31

            string phase;
            decimal price;
            string blurb;

            // Determine current phase and price
            var year = today.Year;
            var ebStart = new DateTime(year, 1, 1);
            var ebEnd = new DateTime(year, 3, 31);
            var regEnd = new DateTime(year, 7, 31);
            var lateEnd = new DateTime(year, 10, 31);

            if (today >= ebStart && today <= ebEnd)
            {
                phase = "Early Bird";
                price = earlyBird;
                blurb = "Buy now and lock in the lowest price for the season.";
            }
            else if (today >= SeasonStart && today <= regEnd)
            {
                phase = "Regular";
                price = regular;
                blurb = "Enjoy unlimited visits all season long.";
            }
            else if (today >= new DateTime(year, 8, 1) && today <= lateEnd)
            {
                phase = "Late-Season";
                price = late;
                blurb = "Join now and enjoy the rest of the season at a reduced price.";
            }
            else
            {
                // Off-season: hide price / disable buy if you prefer
                phase = "Off-Season";
                price = regular; // or 0m
                blurb = "Season passes will be available again before next season.";
            }

            var vm = new PassViewModel
            {
                CurrentPrice = price,
                PhaseLabel = phase,
                Blurb = blurb
            };

            return View(vm);
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
