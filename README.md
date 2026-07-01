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

### 2. Azurite installieren

Azurite emuliert Azure Blob Storage lokal und wird für den PDF-Upload benötigt:

```
npm install -g azurite
```

### 3. Gemini API-Key eintragen

In `ExamBuilder/appsettings.json` den eigenen API-Key eintragen:

```json
"Gemini": {
  "ApiKey": "DEIN_API_KEY"
}
```

Den API-Key erhält man kostenlos über Google AI Studio (nach "Google AI Studio" suchen).

## App starten

### Schritt 1 — Azurite starten

In einem Terminal:

```
azurite
```

Dieses Terminal-Fenster offen lassen solange die App läuft.

### Schritt 2 — App starten

In Rider: Run-Button klicken (oder Shift+F10).

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

- Der Gemini API-Key darf **nicht** in Git eingecheckt werden. Die Datei `appsettings.json` sollte in `.gitignore` eingetragen werden oder der Key über User Secrets verwaltet werden.
- Die Datenbankdatei `ExamBuilder.db` wird lokal im Projektordner erstellt und sollte ebenfalls nicht in Git eingecheckt werden.
