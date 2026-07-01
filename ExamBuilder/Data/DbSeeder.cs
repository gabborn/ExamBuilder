using ExamBuilder.Models;
using Microsoft.AspNetCore.Identity;

namespace ExamBuilder.Data;

public static class DbSeeder
{
    public static async Task Seed(ExamBuilderContext context,
        UserManager<ExamBuilderUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // Rollen anlegen
        string[] rollen = { "Administrator", "Dozent", "Mitarbeiter" };
        foreach (var rolle in rollen)
        {
            if (!await roleManager.RoleExistsAsync(rolle))
                await roleManager.CreateAsync(new IdentityRole(rolle));
        }

        // Demo-User anlegen
        await CreateUser(userManager, "admin", "John", "Admin", "admin@exambuilder.de", "Admin123!", "Administrator");
        await CreateUser(userManager, "professor", "Max", "Mustermann", "professor@exambuilder.de", "Prof123!", "Dozent");
        await CreateUser(userManager, "maria", "Maria", "Musterfrau", "maria@exambuilder.de", "Mitarbeiter123!", "Mitarbeiter");
        await CreateUser(userManager, "eva", "Eva", "Mustermann", "eva@exambuilder.de", "Mitarbeiter123!", "Mitarbeiter");

        if (context.Lehrveranstaltungen.Any()) return;

        // =====================================================================
        // DEMODATEN — kann als Block entfernt werden
        // =====================================================================

        SeedDemoData(context);
    }

    private static void SeedDemoData(ExamBuilderContext context)
    {
        // --- Lehrveranstaltungen ---
        var lehrveranstaltungen = new List<Lehrveranstaltung>
        {
            new() { Titel = "Web Engineering",              Dozentenname = "Prof. Dr. Müller",  Abschlussart = Abschlussart.Bachelor },
            new() { Titel = "Datenbanksysteme",             Dozentenname = "Prof. Dr. Schmidt", Abschlussart = Abschlussart.Bachelor },
            new() { Titel = "Informationssysteme",          Dozentenname = "Prof. Dr. Weber",   Abschlussart = Abschlussart.Bachelor },
            new() { Titel = "Business Intelligence",        Dozentenname = "Prof. Dr. Fischer", Abschlussart = Abschlussart.Master   },
            new() { Titel = "IT-Projektmanagement",         Dozentenname = "Prof. Dr. Bauer",   Abschlussart = Abschlussart.Master   },
        };
        context.Lehrveranstaltungen.AddRange(lehrveranstaltungen);
        context.SaveChanges();

        // --- Kapitel, Fragen, Antworten ---
        var kapitelDaten = new Dictionary<string, List<(string Titel, List<(string Frage, string Korrekt, string[] Falsch)> Fragen)>>
        {
            ["Web Engineering"] = new()
            {
                ("ASP.NET Core MVC", new()
                {
                    ("Welches Entwurfsmuster liegt ASP.NET Core MVC zugrunde?",
                        "Model-View-Controller",
                        new[] { "Model-View-Presenter", "Model-View-ViewModel", "Flux" }),
                    ("Welche HTTP-Methode wird für das Anlegen neuer Daten verwendet?",
                        "POST",
                        new[] { "GET", "PUT", "DELETE" }),
                    ("Was ist die Aufgabe eines Controllers in MVC?",
                        "Anfragen entgegennehmen und die passende View zurückgeben",
                        new[] { "Datenbankzugriffe durchführen", "HTML rendern", "CSS verwalten" }),
                }),
                ("Entity Framework Core", new()
                {
                    ("Welcher Ansatz beschreibt Code-First in EF Core?",
                        "Klassen werden zuerst definiert, die Datenbank wird daraus generiert",
                        new[] { "Die Datenbank wird zuerst erstellt", "SQL-Skripte werden manuell geschrieben", "Tabellen werden per Drag & Drop erstellt" }),
                    ("Welcher Befehl erstellt eine neue Migration?",
                        "dotnet ef migrations add",
                        new[] { "dotnet ef database update", "dotnet ef migrations remove", "dotnet build" }),
                    ("Was ist ein DbSet in EF Core?",
                        "Eine Sammlung von Entitäten, die einer Datenbanktabelle entspricht",
                        new[] { "Eine Verbindungszeichenfolge", "Eine SQL-Abfrage", "Ein Migrationsskript" }),
                }),
                ("Azure Blob Storage", new()
                {
                    ("Welcher Azure-Dienst eignet sich für die Speicherung von Dateien wie PDFs?",
                        "Azure Blob Storage",
                        new[] { "Azure SQL Database", "Azure Cosmos DB", "Azure Queue Storage" }),
                    ("Was ist Azurite?",
                        "Ein lokaler Azure Storage-Emulator für die Entwicklung",
                        new[] { "Eine Azure-Datenbank", "Ein Azure-Monitoring-Tool", "Ein CI/CD-Dienst" }),
                    ("Wie wird ein Blob-Container in C# erstellt?",
                        "Mit BlobServiceClient.GetBlobContainerClient()",
                        new[] { "Mit SqlConnection.Open()", "Mit HttpClient.PostAsync()", "Mit File.Create()" }),
                }),
            },
            ["Datenbanksysteme"] = new()
            {
                ("Relationale Datenbanken", new()
                {
                    ("Was ist ein Primärschlüssel?",
                        "Ein eindeutiger Bezeichner für jeden Datensatz in einer Tabelle",
                        new[] { "Ein doppelt vorkommender Wert", "Ein optionales Attribut", "Ein Fremdschlüssel" }),
                    ("Was beschreibt die 1. Normalform (1NF)?",
                        "Alle Attribute sind atomar und es gibt keine Wiederholungsgruppen",
                        new[] { "Alle Attribute sind redundant", "Tabellen haben keine Primärschlüssel", "Fremdschlüssel sind optional" }),
                    ("Was ist ein JOIN in SQL?",
                        "Eine Operation zum Verknüpfen von Daten aus mehreren Tabellen",
                        new[] { "Eine Einfügeoperation", "Eine Löschoperation", "Eine Indexerstellung" }),
                }),
                ("SQL Grundlagen", new()
                {
                    ("Welche SQL-Anweisung wird zum Lesen von Daten verwendet?",
                        "SELECT",
                        new[] { "INSERT", "UPDATE", "DELETE" }),
                    ("Was bewirkt GROUP BY in SQL?",
                        "Zeilen mit gleichen Werten werden zu Gruppen zusammengefasst",
                        new[] { "Zeilen werden sortiert", "Zeilen werden gefiltert", "Zeilen werden gelöscht" }),
                    ("Was ist eine Transaktion?",
                        "Eine Folge von Datenbankoperationen, die als Einheit ausgeführt werden",
                        new[] { "Eine einzelne SQL-Abfrage", "Ein Datenbankindex", "Ein Backup-Vorgang" }),
                }),
                ("Datenbankdesign", new()
                {
                    ("Was ist ein Entity-Relationship-Diagramm (ERD)?",
                        "Ein grafisches Modell zur Darstellung von Entitäten und deren Beziehungen",
                        new[] { "Ein Datenbankbackup", "Ein SQL-Skript", "Eine Netzwerktopologie" }),
                    ("Was bedeutet Kardinalität im Datenbankdesign?",
                        "Die Anzahl der möglichen Beziehungen zwischen Entitäten",
                        new[] { "Die Anzahl der Tabellen", "Die Größe der Datenbank", "Die Anzahl der Indizes" }),
                    ("Was ist Denormalisierung?",
                        "Das gezielte Einführen von Redundanz zur Performanceverbesserung",
                        new[] { "Das Entfernen aller Tabellen", "Das Hinzufügen von Primärschlüsseln", "Das Erstellen von Backups" }),
                }),
            },
            ["Informationssysteme"] = new()
            {
                ("Grundlagen der Informationssysteme", new()
                {
                    ("Was ist ein Informationssystem?",
                        "Ein System zur Erfassung, Verarbeitung, Speicherung und Bereitstellung von Informationen",
                        new[] { "Ein Betriebssystem", "Eine Programmiersprache", "Ein Netzwerkprotokoll" }),
                    ("Was versteht man unter ERP?",
                        "Enterprise Resource Planning — integrierte Verwaltung von Geschäftsprozessen",
                        new[] { "Electronic Resource Processing", "External Resource Planning", "Enterprise Remote Protocol" }),
                    ("Was ist ein Data Warehouse?",
                        "Ein zentrales Repository für strukturierte Daten aus verschiedenen Quellen",
                        new[] { "Ein physisches Lager", "Ein Cloud-Server", "Ein Netzwerkspeicher" }),
                }),
                ("Geschäftsprozesse", new()
                {
                    ("Was ist ein Geschäftsprozess?",
                        "Eine strukturierte Abfolge von Aktivitäten zur Erreichung eines Unternehmensziels",
                        new[] { "Ein technischer Algorithmus", "Ein Datenbankschema", "Ein Netzwerkprotokoll" }),
                    ("Was bedeutet BPM?",
                        "Business Process Management",
                        new[] { "Basic Process Modeling", "Binary Process Machine", "Batch Processing Mode" }),
                    ("Welche Notation wird häufig für Geschäftsprozesse verwendet?",
                        "BPMN (Business Process Model and Notation)",
                        new[] { "UML", "ERD", "HTML" }),
                }),
                ("Sicherheit und Datenschutz", new()
                {
                    ("Was regelt die DSGVO?",
                        "Den Schutz personenbezogener Daten in der EU",
                        new[] { "Den Handel mit digitalen Waren", "Die Netzwerksicherheit", "Den E-Mail-Verkehr" }),
                    ("Was ist Authentifizierung?",
                        "Der Nachweis der Identität eines Nutzers",
                        new[] { "Die Vergabe von Zugriffsrechten", "Die Verschlüsselung von Daten", "Die Datensicherung" }),
                    ("Was versteht man unter dem Prinzip der minimalen Rechtevergabe?",
                        "Nutzer erhalten nur die Rechte, die für ihre Aufgaben notwendig sind",
                        new[] { "Alle Nutzer erhalten Adminrechte", "Rechte werden nie vergeben", "Rechte werden täglich erneuert" }),
                }),
            },
            ["Business Intelligence"] = new()
            {
                ("Data Warehousing", new()
                {
                    ("Was ist ein Faktum im Data Warehouse?",
                        "Eine messbare Größe, z. B. Umsatz oder Anzahl von Verkäufen",
                        new[] { "Eine Dimension", "Ein Attribut", "Ein Fremdschlüssel" }),
                    ("Was beschreibt ein Sternschema?",
                        "Eine Faktentabelle im Zentrum, umgeben von Dimensionstabellen",
                        new[] { "Mehrere verbundene Faktentabellen", "Eine einzelne Tabelle", "Ein hierarchisches Modell" }),
                    ("Was ist ETL?",
                        "Extract, Transform, Load — Prozess zur Datenintegration",
                        new[] { "Enterprise Transaction Layer", "External Transfer Logic", "Encrypted Text Language" }),
                }),
                ("OLAP und Reporting", new()
                {
                    ("Wofür steht OLAP?",
                        "Online Analytical Processing",
                        new[] { "Online Application Protocol", "Offline Analytical Processing", "Open Layer API Platform" }),
                    ("Was ist ein KPI?",
                        "Key Performance Indicator — eine Kennzahl zur Leistungsmessung",
                        new[] { "Key Process Integration", "Knowledge Performance Index", "Key Protocol Interface" }),
                    ("Welches Tool wird häufig für BI-Dashboards verwendet?",
                        "Power BI",
                        new[] { "Microsoft Word", "Visual Studio", "SQL Server Management Studio" }),
                }),
                ("Datenanalyse", new()
                {
                    ("Was ist Deskriptive Analyse?",
                        "Die Beschreibung vergangener Ereignisse anhand von Daten",
                        new[] { "Die Vorhersage zukünftiger Ereignisse", "Die Steuerung von Prozessen", "Die Erhebung neuer Daten" }),
                    ("Was ist der Unterschied zwischen Korrelation und Kausalität?",
                        "Korrelation beschreibt einen Zusammenhang, Kausalität eine Ursache-Wirkung-Beziehung",
                        new[] { "Es gibt keinen Unterschied", "Kausalität ist schwächer als Korrelation", "Korrelation impliziert immer Kausalität" }),
                    ("Was versteht man unter Data Mining?",
                        "Das Entdecken von Mustern und Zusammenhängen in großen Datenmengen",
                        new[] { "Das Speichern von Daten in der Cloud", "Das Löschen veralteter Daten", "Das Erstellen von Backups" }),
                }),
            },
            ["IT-Projektmanagement"] = new()
            {
                ("Projektplanung", new()
                {
                    ("Was ist ein Gantt-Diagramm?",
                        "Ein Balkendiagramm zur Darstellung von Projektphasen und Zeitplänen",
                        new[] { "Ein Netzwerkdiagramm", "Ein Kreisdiagramm", "Ein Flussdiagramm" }),
                    ("Was beschreibt der kritische Pfad?",
                        "Die längste Abfolge abhängiger Aufgaben, die die Mindestprojektdauer bestimmt",
                        new[] { "Der kürzeste Weg im Projektplan", "Die teuerste Aufgabe", "Die zuerst erledigte Aufgabe" }),
                    ("Was ist ein Meilenstein im Projektmanagement?",
                        "Ein wichtiges Ereignis oder ein Ziel, das zu einem bestimmten Zeitpunkt erreicht werden muss",
                        new[] { "Eine Aufgabe mit hohem Aufwand", "Ein Budget-Posten", "Ein Teammitglied" }),
                }),
                ("Agile Methoden", new()
                {
                    ("Was ist Scrum?",
                        "Ein agiles Framework mit Sprints, täglichen Stand-ups und klar definierten Rollen",
                        new[] { "Eine Programmiersprache", "Ein Datenbanksystem", "Ein Netzwerkprotokoll" }),
                    ("Was ist ein Product Backlog?",
                        "Eine priorisierte Liste aller gewünschten Features und Anforderungen eines Produkts",
                        new[] { "Ein abgeschlossenes Aufgabenprotokoll", "Ein Bugtracker", "Ein Deployment-Plan" }),
                    ("Was ist der Unterschied zwischen Scrum und Kanban?",
                        "Scrum arbeitet in festen Sprints, Kanban nutzt einen kontinuierlichen Fluss",
                        new[] { "Kanban hat Sprints, Scrum nicht", "Beide sind identisch", "Scrum hat kein Team" }),
                }),
                ("Risikomanagement", new()
                {
                    ("Was ist ein Risiko im Projektkontext?",
                        "Ein ungewisses Ereignis, das den Projekterfolg negativ beeinflussen kann",
                        new[] { "Ein geplantes Ereignis", "Ein sicherer Verlust", "Ein Teammitglied" }),
                    ("Was versteht man unter Risikovermeidung?",
                        "Maßnahmen, die das Eintreten eines Risikos verhindern",
                        new[] { "Das Akzeptieren eines Risikos", "Das Übertragen eines Risikos auf Dritte", "Das Ignorieren eines Risikos" }),
                    ("Was ist ein Risikomatrix?",
                        "Ein Werkzeug zur Bewertung von Risiken nach Eintrittswahrscheinlichkeit und Auswirkung",
                        new[] { "Eine Projektstrukturplan", "Ein Gantt-Diagramm", "Ein Organigramm" }),
                }),
            },
        };

        foreach (var lv in lehrveranstaltungen)
        {
            if (!kapitelDaten.TryGetValue(lv.Titel, out var kapitelListe)) continue;

            var alleFragenDerLv = new List<McFrage>();

            for (int ki = 0; ki < kapitelListe.Count; ki++)
            {
                var (kapitelTitel, fragenListe) = kapitelListe[ki];
                var kapitel = new Kapitel
                {
                    Titel = kapitelTitel,
                    Kapitelnummer = ki + 1,
                    LehrveranstaltungId = lv.Id
                };
                context.Kapitel.Add(kapitel);
                context.SaveChanges();

                foreach (var (fragentext, korrekt, falsch) in fragenListe)
                {
                    var frage = new McFrage
                    {
                        Fragentext = fragentext,
                        KapitelId = kapitel.Id,
                        McAntworten = new List<McAntwort>
                        {
                            new() { Antworttext = korrekt,   Korrekt = true  },
                            new() { Antworttext = falsch[0], Korrekt = false },
                            new() { Antworttext = falsch[1], Korrekt = false },
                            new() { Antworttext = falsch[2], Korrekt = false },
                        }
                    };
                    context.McFragen.Add(frage);
                    alleFragenDerLv.Add(frage);
                }
                context.SaveChanges();
            }

            // 2 Prüfungen pro Lehrveranstaltung
            var pruefung1 = new Pruefung
            {
                Titel = $"Zwischenprüfung {lv.Titel}",
                Datum = new DateTime(2025, 6, 15),
                Beschreibung = "Prüfung zu den ersten beiden Kapiteln",
                LehrveranstaltungId = lv.Id,
                McFragen = alleFragenDerLv.Take(6).ToList()
            };
            var pruefung2 = new Pruefung
            {
                Titel = $"Abschlussprüfung {lv.Titel}",
                Datum = new DateTime(2025, 7, 20),
                Beschreibung = "Prüfung zu allen Kapiteln",
                LehrveranstaltungId = lv.Id,
                McFragen = alleFragenDerLv.ToList()
            };
            context.Pruefungen.AddRange(pruefung1, pruefung2);
            context.SaveChanges();
        }
    }

    private static async Task CreateUser(UserManager<ExamBuilderUser> userManager,
        string username, string vorname, string nachname, string email, string passwort, string rolle)
    {
        if (await userManager.FindByNameAsync(username) == null)
        {
            var user = new ExamBuilderUser
            {
                UserName = username,
                Email = email,
                Vorname = vorname,
                Nachname = nachname,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(user, passwort);
            await userManager.AddToRoleAsync(user, rolle);
        }
    }
}
