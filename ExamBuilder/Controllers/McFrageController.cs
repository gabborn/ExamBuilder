using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExamBuilder.Data;
using ExamBuilder.Models;

namespace ExamBuilder.Controllers
{
    [Authorize]
    public class McFrageController : Controller
    {
        private readonly ExamBuilderContext _context;
        private readonly IConfiguration _configuration;
        private readonly BlobServiceClient _blobServiceClient;
        private const int PageSize = 5;

        public McFrageController(ExamBuilderContext context, IConfiguration configuration, BlobServiceClient blobServiceClient)
        {
            _context = context;
            _configuration = configuration;
            _blobServiceClient = blobServiceClient;
        }

        // GET: McFrage?kapitelId=5&suche=xyz&seite=1
        public async Task<IActionResult> Index(int kapitelId, string? suche, int seite = 1)
        {
            var kapitel = await _context.Kapitel
                .Include(k => k.Lehrveranstaltung)
                .FirstOrDefaultAsync(k => k.Id == kapitelId);

            if (kapitel == null) return NotFound();

            var query = _context.McFragen
                .Where(f => f.KapitelId == kapitelId)
                .Include(f => f.McAntworten)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(suche))
                query = query.Where(f => f.Fragentext.Contains(suche));

            int gesamtAnzahl = await query.CountAsync();
            int gesamtSeiten = (int)Math.Ceiling(gesamtAnzahl / (double)PageSize);
            seite = Math.Max(1, Math.Min(seite, Math.Max(1, gesamtSeiten)));

            var fragen = await query
                .Skip((seite - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.Kapitel = kapitel;
            ViewBag.Suche = suche;
            ViewBag.AktuelleSeite = seite;
            ViewBag.GesamtSeiten = gesamtSeiten;

            return View(fragen);
        }

        // GET: McFrage/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var mcFrage = await _context.McFragen
                .Include(f => f.McAntworten)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (mcFrage == null) return NotFound();

            return View(mcFrage);
        }

        // POST: McFrage/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string fragentext, List<int> antwortIds,
            List<string> antworttexte, int korrektAntwortId)
        {
            var mcFrage = await _context.McFragen
                .Include(f => f.McAntworten)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (mcFrage == null) return NotFound();

            mcFrage.Fragentext = fragentext;

            for (int i = 0; i < antwortIds.Count; i++)
            {
                var antwort = mcFrage.McAntworten.FirstOrDefault(a => a.Id == antwortIds[i]);
                if (antwort != null)
                {
                    antwort.Antworttext = antworttexte[i];
                    antwort.Korrekt = antwort.Id == korrektAntwortId;
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { kapitelId = mcFrage.KapitelId });
        }

        // GET: McFrage/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var mcFrage = await _context.McFragen
                .Include(f => f.Kapitel)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mcFrage == null) return NotFound();

            return View(mcFrage);
        }

        // POST: McFrage/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mcFrage = await _context.McFragen.FindAsync(id);
            if (mcFrage != null)
            {
                _context.McFragen.Remove(mcFrage);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { kapitelId = mcFrage?.KapitelId });
        }

        // POST: McFrage/Pruefen/5
        [HttpPost]
        public async Task<IActionResult> Pruefen(int id)
        {
            var mcFrage = await _context.McFragen
                .Include(f => f.McAntworten)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (mcFrage == null) return NotFound();

            var antworten = mcFrage.McAntworten
                .Select((a, i) => $"{i + 1}. {a.Antworttext} ({(a.Korrekt ? "korrekt" : "falsch")})")
                .ToList();

            var prompt = $"Prüfe die folgende Multiple-Choice-Frage auf sprachliche Korrektheit, " +
                         $"eindeutige Beantwortbarkeit und ob genau eine korrekte Antwort vorhanden ist.\n\n" +
                         $"Frage: {mcFrage.Fragentext}\n\nAntwortoptionen:\n{string.Join("\n", antworten)}\n\n" +
                         $"Gib eine kurze, präzise Bewertung auf Deutsch zurück.";

            var ergebnis = await SendToGemini(prompt);
            return Content(ergebnis);
        }

        // POST: McFrage/Generieren
        [HttpPost]
        public async Task<IActionResult> Generieren(int kapitelId)
        {
            var kapitel = await _context.Kapitel.FindAsync(kapitelId);
            if (kapitel == null) return NotFound();

            if (string.IsNullOrEmpty(kapitel.Vorlesungsfolien))
                return BadRequest("Für dieses Kapitel sind keine Vorlesungsfolien hinterlegt.");

            // PDF aus Blob Storage herunterladen
            byte[] pdfBytes;
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient("vorlesungsfolien");
                var blobName = Path.GetFileName(new Uri(kapitel.Vorlesungsfolien).LocalPath);
                var blobClient = containerClient.GetBlobClient(blobName);
                using var stream = new MemoryStream();
                await blobClient.DownloadToAsync(stream);
                pdfBytes = stream.ToArray();
            }
            catch (Exception ex)
            {
                return BadRequest($"PDF konnte nicht geladen werden: {ex.Message}");
            }

            var pdfBase64 = Convert.ToBase64String(pdfBytes);
            var prompt = $"Generiere eine neue Multiple-Choice-Frage basierend auf dem Inhalt des beigefügten PDF-Dokuments zum Kapitel '{kapitel.Titel}'. " +
                         $"Antworte ausschließlich im folgenden JSON-Format (kein Markdown, kein Codeblock):\n" +
                         $"{{\"frage\": \"...\", \"antworten\": [{{\"text\": \"...\", \"korrekt\": true}}, " +
                         $"{{\"text\": \"...\", \"korrekt\": false}}, {{\"text\": \"...\", \"korrekt\": false}}, " +
                         $"{{\"text\": \"...\", \"korrekt\": false}}]}}";

            var jsonAntwort = await SendToGeminiWithPdf(prompt, pdfBase64);
            jsonAntwort = jsonAntwort.Trim();
            if (jsonAntwort.StartsWith("```"))
            {
                jsonAntwort = jsonAntwort.Substring(jsonAntwort.IndexOf('\n') + 1);
                jsonAntwort = jsonAntwort.Substring(0, jsonAntwort.LastIndexOf("```")).Trim();
            }

            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(jsonAntwort);
            }
            catch
            {
                return BadRequest("Die KI hat kein gültiges JSON zurückgegeben.");
            }

            var root = doc.RootElement;
            var antwortenArray = root.GetProperty("antworten").EnumerateArray().ToList();

            if (antwortenArray.Count != 4)
                return BadRequest("Die KI hat nicht genau 4 Antwortoptionen zurückgegeben.");

            var neueFrage = new McFrage
            {
                Fragentext = root.GetProperty("frage").GetString() ?? string.Empty,
                KapitelId = kapitelId,
                McAntworten = antwortenArray.Select(a => new McAntwort
                {
                    Antworttext = a.GetProperty("text").GetString() ?? string.Empty,
                    Korrekt = a.GetProperty("korrekt").GetBoolean()
                }).ToList()
            };

            _context.McFragen.Add(neueFrage);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { kapitelId });
        }

        private async Task<string> SendToGemini(string prompt)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

            var payload = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            using var client = new HttpClient();
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            if (root.TryGetProperty("candidates", out var candidates) &&
                candidates.GetArrayLength() > 0 &&
                candidates[0].TryGetProperty("content", out var contentEl) &&
                contentEl.TryGetProperty("parts", out var parts) &&
                parts.GetArrayLength() > 0 &&
                parts[0].TryGetProperty("text", out var textEl))
            {
                return textEl.GetString() ?? string.Empty;
            }

            throw new Exception($"Unerwartete Gemini-Antwort: {responseBody}");
        }

        private async Task<string> SendToGeminiWithPdf(string prompt, string pdfBase64)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { text = prompt },
                            new { inline_data = new { mime_type = "application/pdf", data = pdfBase64 } }
                        }
                    }
                }
            };

            using var client = new HttpClient();
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            if (root.TryGetProperty("candidates", out var candidates) &&
                candidates.GetArrayLength() > 0 &&
                candidates[0].TryGetProperty("content", out var contentEl) &&
                contentEl.TryGetProperty("parts", out var parts) &&
                parts.GetArrayLength() > 0 &&
                parts[0].TryGetProperty("text", out var textEl))
            {
                return textEl.GetString() ?? string.Empty;
            }

            throw new Exception($"Unerwartete Gemini-Antwort: {responseBody}");
        }

        private bool McFrageExists(int id)
        {
            return _context.McFragen.Any(e => e.Id == id);
        }
    }
}
