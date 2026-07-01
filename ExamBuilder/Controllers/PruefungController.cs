using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExamBuilder.Data;
using ExamBuilder.Models;

namespace ExamBuilder.Controllers
{
    [Authorize]
    public class PruefungController : Controller
    {
        private readonly ExamBuilderContext _context;

        public PruefungController(ExamBuilderContext context)
        {
            _context = context;
        }

        // GET: Pruefung?lehrveranstaltungId=5
        public async Task<IActionResult> Index(int lehrveranstaltungId)
        {
            var lehrveranstaltung = await _context.Lehrveranstaltungen
                .FirstOrDefaultAsync(lv => lv.Id == lehrveranstaltungId);

            if (lehrveranstaltung == null) return NotFound();

            var pruefungen = await _context.Pruefungen
                .Where(p => p.LehrveranstaltungId == lehrveranstaltungId)
                .OrderBy(p => p.Datum)
                .ToListAsync();

            ViewBag.Lehrveranstaltung = lehrveranstaltung;
            return View(pruefungen);
        }

        // GET: Pruefung/Create?lehrveranstaltungId=5
        public IActionResult Create(int lehrveranstaltungId)
        {
            ViewBag.LehrveranstaltungId = lehrveranstaltungId;
            return View();
        }

        // POST: Pruefung/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Titel,Datum,Beschreibung,LehrveranstaltungId")] Pruefung pruefung,
            int anzahlFragen)
        {
            ModelState.Remove("Lehrveranstaltung");
            if (ModelState.IsValid)
            {
                var alleFragen = await _context.McFragen
                    .Where(f => f.Kapitel.LehrveranstaltungId == pruefung.LehrveranstaltungId)
                    .ToListAsync();

                if (alleFragen.Count < anzahlFragen)
                {
                    ModelState.AddModelError(string.Empty,
                        $"Nicht genug Fragen vorhanden. Verfügbar: {alleFragen.Count}, angefordert: {anzahlFragen}.");
                    ViewBag.LehrveranstaltungId = pruefung.LehrveranstaltungId;
                    return View(pruefung);
                }

                var zufaelligeFragen = alleFragen
                    .OrderBy(_ => Guid.NewGuid())
                    .Take(anzahlFragen)
                    .ToList();

                pruefung.McFragen = zufaelligeFragen;
                _context.Add(pruefung);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index), new { lehrveranstaltungId = pruefung.LehrveranstaltungId });
            }

            ViewBag.LehrveranstaltungId = pruefung.LehrveranstaltungId;
            return View(pruefung);
        }

        // GET: Pruefung/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var pruefung = await _context.Pruefungen
                .Include(p => p.Lehrveranstaltung)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (pruefung == null) return NotFound();

            return View(pruefung);
        }

        // POST: Pruefung/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pruefung = await _context.Pruefungen
                .Include(p => p.McFragen)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pruefung != null)
            {
                pruefung.McFragen.Clear();
                _context.Pruefungen.Remove(pruefung);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { lehrveranstaltungId = pruefung?.LehrveranstaltungId });
        }

        // GET: Pruefung/Drucken/5
        public async Task<IActionResult> Drucken(int? id)
        {
            if (id == null) return NotFound();

            var pruefung = await _context.Pruefungen
                .Include(p => p.Lehrveranstaltung)
                .Include(p => p.McFragen)
                    .ThenInclude(f => f.McAntworten)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pruefung == null) return NotFound();

            return View(pruefung);
        }

        private bool PruefungExists(int id)
        {
            return _context.Pruefungen.Any(e => e.Id == id);
        }
    }
}
