using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExamBuilder.Data;
using ExamBuilder.Models;

namespace ExamBuilder.Controllers
{
    [Authorize]
    public class KapitelController : Controller
    {
        private readonly ExamBuilderContext _context;
        private readonly BlobServiceClient _blobServiceClient;
        private const string ContainerName = "vorlesungsfolien";

        public KapitelController(ExamBuilderContext context, BlobServiceClient blobServiceClient)
        {
            _context = context;
            _blobServiceClient = blobServiceClient;
        }

        // GET: Kapitel?lehrveranstaltungId=5
        public async Task<IActionResult> Index(int lehrveranstaltungId)
        {
            var lehrveranstaltung = await _context.Lehrveranstaltungen
                .FirstOrDefaultAsync(lv => lv.Id == lehrveranstaltungId);

            if (lehrveranstaltung == null) return NotFound();

            var kapitel = await _context.Kapitel
                .Where(k => k.LehrveranstaltungId == lehrveranstaltungId)
                .Include(k => k.McFragen)
                .OrderBy(k => k.Kapitelnummer)
                .ToListAsync();

            ViewBag.Lehrveranstaltung = lehrveranstaltung;
            return View(kapitel);
        }

        // GET: Kapitel/Create?lehrveranstaltungId=5
        public IActionResult Create(int lehrveranstaltungId)
        {
            ViewBag.LehrveranstaltungId = lehrveranstaltungId;
            return View();
        }

        // POST: Kapitel/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Titel,Kapitelnummer,LehrveranstaltungId")] Kapitel kapitel, IFormFile? datei)
        {
            ModelState.Remove("Lehrveranstaltung");
            if (ModelState.IsValid)
            {
                if (datei != null && datei.Length > 0)
                {
                    kapitel.Vorlesungsfolien = await UploadToBlob(datei);
                }

                _context.Add(kapitel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { lehrveranstaltungId = kapitel.LehrveranstaltungId });
            }

            ViewBag.LehrveranstaltungId = kapitel.LehrveranstaltungId;
            return View(kapitel);
        }

        // GET: Kapitel/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var kapitel = await _context.Kapitel.FindAsync(id);
            if (kapitel == null) return NotFound();

            return View(kapitel);
        }

        // POST: Kapitel/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titel,Kapitelnummer,Vorlesungsfolien,LehrveranstaltungId")] Kapitel kapitel,
            IFormFile? datei, bool dateiLoeschen = false)
        {
            if (id != kapitel.Id) return NotFound();

            ModelState.Remove("Lehrveranstaltung");
            if (ModelState.IsValid)
            {
                try
                {
                    if (dateiLoeschen && kapitel.Vorlesungsfolien != null)
                    {
                        await DeleteFromBlob(kapitel.Vorlesungsfolien);
                        kapitel.Vorlesungsfolien = null;
                    }
                    else if (datei != null && datei.Length > 0)
                    {
                        if (kapitel.Vorlesungsfolien != null)
                            await DeleteFromBlob(kapitel.Vorlesungsfolien);

                        kapitel.Vorlesungsfolien = await UploadToBlob(datei);
                    }

                    _context.Update(kapitel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KapitelExists(kapitel.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index), new { lehrveranstaltungId = kapitel.LehrveranstaltungId });
            }

            return View(kapitel);
        }

        // GET: Kapitel/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var kapitel = await _context.Kapitel
                .Include(k => k.Lehrveranstaltung)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (kapitel == null) return NotFound();

            return View(kapitel);
        }

        // POST: Kapitel/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var kapitel = await _context.Kapitel.FindAsync(id);
            if (kapitel != null)
            {
                if (kapitel.Vorlesungsfolien != null)
                    await DeleteFromBlob(kapitel.Vorlesungsfolien);

                _context.Kapitel.Remove(kapitel);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { lehrveranstaltungId = kapitel?.LehrveranstaltungId });
        }

        private bool KapitelExists(int id)
        {
            return _context.Kapitel.Any(e => e.Id == id);
        }

        // GET: Kapitel/Download/5
        public async Task<IActionResult> Download(int id)
        {
            var kapitel = await _context.Kapitel.FindAsync(id);
            if (kapitel == null || string.IsNullOrEmpty(kapitel.Vorlesungsfolien))
                return NotFound();

            var blobName = Path.GetFileName(new Uri(kapitel.Vorlesungsfolien).LocalPath);
            var container = _blobServiceClient.GetBlobContainerClient(ContainerName);
            var blobClient = container.GetBlobClient(blobName);

            var download = await blobClient.DownloadStreamingAsync();
            return File(download.Value.Content, "application/pdf", $"{kapitel.Titel}.pdf");
        }

        private async Task<string> UploadToBlob(IFormFile datei)
        {
            var container = _blobServiceClient.GetBlobContainerClient(ContainerName);
            await container.CreateIfNotExistsAsync();

            var blobName = $"{Guid.NewGuid()}{Path.GetExtension(datei.FileName)}";
            var blobClient = container.GetBlobClient(blobName);

            using var stream = datei.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true);

            return blobClient.Uri.ToString();
        }

        private async Task DeleteFromBlob(string url)
        {
            var uri = new Uri(url);
            var blobName = Path.GetFileName(uri.LocalPath);
            var container = _blobServiceClient.GetBlobContainerClient(ContainerName);
            await container.GetBlobClient(blobName).DeleteIfExistsAsync();
        }
    }
}
