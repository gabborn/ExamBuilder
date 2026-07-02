# ExamBuilder

Web-Anwendung zur Unterstützung bei der Erstellung von Multiple-Choice-Klausuren. Entwickelt mit ASP.NET Core MVC, Entity Framework Core und SQLite.

## Voraussetzungen

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org) (für Azurite)
- [JetBrains Rider](https://www.jetbrains.com/rider/) oder Visual Studio
- Gemini API-Key (kostenlos über Google AI Studio)

## Installation

### 1. Repository klonen

```
git clone <repository-url>
cd ExamBuilder
```

### 2. Node.js und Azurite installieren

Azurite emuliert Azure Blob Storage lokal und wird für den PDF-Upload benötigt. Azurite wird über npm installiert, das mit Node.js mitgeliefert wird.

**Node.js installieren** (falls noch nicht vorhanden):
1. [nodejs.org](https://nodejs.org) aufrufen und die **LTS**-Version herunterladen
2. Installer ausführen
3. Computer neu starten
4. Installation prüfen:
```
npm --version
```

**Azurite installieren:**
```
npm install -g azurite
```

### 3. Secrets konfigurieren (User Secrets)

API-Key und Blob-Storage-Verbindung werden **nicht** in einer Datei gespeichert, sondern über .NET User Secrets verwaltet. Diese werden niemals in Git eingecheckt.

Im Verzeichnis `ExamBuilder/ExamBuilder` folgende Befehle ausführen:

```
dotnet user-secrets set "Gemini:ApiKey" "DEIN_API_KEY"
dotnet user-secrets set "ConnectionStrings:AzureBlobStorage" "UseDevelopmentStorage=true"
```

Den eigenen Gemini API-Key kostenlos über [Google AI Studio](https://aistudio.google.com) erstellen und bei `DEIN_API_KEY` eintragen.

## App starten

### Schritt 1 — Azurite starten

In einem Terminal:

```
azurite --skipApiVersionCheck
```

Dieses Terminal-Fenster offen lassen solange die App läuft.

### Schritt 2 — App starten

In Rider: Run-Button klicken (oder Shift+F10).
In Visual Studio: grüner Start-Button oder F5.

Die Datenbank wird beim ersten Start automatisch erstellt und mit Demodaten befüllt.

## Demo-Zugangsdaten

| Benutzername | Passwort | Rolle |
|---|---|---|
| `admin` | `Admin123!` | Administrator |
| `professor` | `Prof123!` | Dozent |
| `maria` | `Mitarbeiter123!` | Mitarbeiter |
| `eva` | `Mitarbeiter123!` | Mitarbeiter |

## Funktionen

- Verwaltung von Lehrveranstaltungen, Kapiteln und MC-Fragen
- PDF-Upload zu Kapiteln (gespeichert in Azure Blob Storage / Azurite)
- KI-gestützte Fragengenerierung und Fragenvalidierung via Gemini API
- Prüfungserstellung mit zufälliger Fragenauswahl
- Benutzerverwaltung mit Rollensystem (Administrator, Dozent, Mitarbeiter)

## Hinweise

- Secrets (Gemini API-Key, Blob Storage Connection String) werden über .NET User Secrets gespeichert (`dotnet user-secrets set ...`) und **niemals** in Git eingecheckt. Jedes Teammitglied setzt seine eigenen Secrets lokal.
- Die Datenbankdatei `ExamBuilder.db` wird lokal erstellt und ist ebenfalls nicht im Repository.
- Azurite muss mit `--skipApiVersionCheck` gestartet werden, da neuere Azurite-Versionen sonst einen API-Versions-Fehler werfen.
- Der Gemini Free Tier erlaubt 20 Anfragen pro Tag (RPD). Das aktuelle Quota und die Verbrauchsstatistik sind unter [ai.dev/rate-limit](https://ai.dev/rate-limit) einsehbar. Bei Quota-Fehler (`RESOURCE_EXHAUSTED`) entweder bis zum nächsten Tag warten oder einen neuen API-Key in [Google AI Studio](https://aistudio.google.com) erstellen.
