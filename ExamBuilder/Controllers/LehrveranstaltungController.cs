using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ExamBuilder.Data;
using ExamBuilder.Models;

namespace ExamBuilder.Controllers
{
    [Authorize]
    public class LehrveranstaltungController : Controller
    {
        private readonly ExamBuilderContext _context;

        public LehrveranstaltungController(ExamBuilderContext context)
        {
            _context = context;
        }

        // GET: Lehrveranstaltung
        public async Task<IActionResult> Index()
        {
            var lehrveranstaltungen = await _context.Lehrveranstaltungen
                .Include(lv => lv.Kapitel)
                .Include(lv => lv.Pruefungen)
                .OrderBy(lv => lv.Titel)
                .ToListAsync();

            return View(lehrveranstaltungen);
        }

        // GET: Lehrveranstaltung/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var lehrveranstaltung = await _context.Lehrveranstaltungen
                .Include(lv => lv.Kapitel)
                .Include(lv => lv.Pruefungen)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lehrveranstaltung == null) return NotFound();

            return View(lehrveranstaltung);
        }

        // GET: Lehrveranstaltung/Create
        public IActionResult Create()
        {
            ViewBag.AbschlussartList = new SelectList(Enum.GetValues(typeof(Abschlussart)));
            return View();
        }

        // POST: Lehrveranstaltung/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Titel,Dozentenname,Abschlussart")] Lehrveranstaltung lehrveranstaltung)
        {
            if (ModelState.IsValid)
            {
                _context.Add(lehrveranstaltung);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.AbschlussartList = new SelectList(Enum.GetValues(typeof(Abschlussart)));
            return View(lehrveranstaltung);
        }

        // GET: Lehrveranstaltung/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var lehrveranstaltung = await _context.Lehrveranstaltungen.FindAsync(id);
            if (lehrveranstaltung == null) return NotFound();

            ViewBag.AbschlussartList = new SelectList(Enum.GetValues(typeof(Abschlussart)), lehrveranstaltung.Abschlussart);
            return View(lehrveranstaltung);
        }

        // POST: Lehrveranstaltung/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titel,Dozentenname,Abschlussart")] Lehrveranstaltung lehrveranstaltung)
        {
            if (id != lehrveranstaltung.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lehrveranstaltung);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LehrveranstaltungExists(lehrveranstaltung.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.AbschlussartList = new SelectList(Enum.GetValues(typeof(Abschlussart)), lehrveranstaltung.Abschlussart);
            return View(lehrveranstaltung);
        }

        // GET: Lehrveranstaltung/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var lehrveranstaltung = await _context.Lehrveranstaltungen
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lehrveranstaltung == null) return NotFound();

            return View(lehrveranstaltung);
        }

        // POST: Lehrveranstaltung/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lehrveranstaltung = await _context.Lehrveranstaltungen.FindAsync(id);
            if (lehrveranstaltung != null)
            {
                _context.Lehrveranstaltungen.Remove(lehrveranstaltung);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool LehrveranstaltungExists(int id)
        {
            return _context.Lehrveranstaltungen.Any(e => e.Id == id);
        }
    }
}
