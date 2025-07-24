# HL7DataProcessor

Dieses Projekt zielt darauf ab, ein **vollständiges End-to-End-Beispiel für die Verarbeitung von HL7v2-Nachrichten** zu demonstrieren. Es liest HL7-Daten aus Dateisystemen, parst diese mittels einer C#-Bibliothek, extrahiert relevante Patienteninformationen und speichert diese in einer Oracle-Datenbank. Abschließend werden diese Daten über eine Webanwendung zugänglich gemacht, um CRUD-Operationen (Create, Read, Update, Delete) zu ermöglichen und die Patientenverwaltung zu visualisieren. Damit wird meine Fähigkeit zur Implementierung **medizinischer Schnittstellen und Dateninfrastrukturen** unter Beweis gestellt.

---
### Web-Applikation: PatientSolution (Blazor WebAssembly)

Als logische Erweiterung der HL7-Datenintegrations-Pipeline wurde eine eigenständige, interaktive Web-Applikation namens **PatientSolution** entwickelt. Diese Blazor WebAssembly-Anwendung dient dazu, die in der Oracle-Datenbank gespeicherten Patientendaten übersichtlich zu visualisieren und zu verwalten.

**Hauptmerkmale der PatientSolution:**
* **Interaktive Patientenübersicht:** Anzeige der extrahierten Patientendaten in einem dynamischen und filterbaren Grid.
* **Moderne Benutzeroberfläche:** Implementiert mit Blazor Bootstrap für eine ansprechende und responsive Darstellung.
* **API-Integration:** Kommunikation mit einer ASP.NET Core Web API, um Patientendaten sicher abzurufen und zu verarbeiten.

Dieses Frontend-Projekt demonstriert die Fähigkeit, eine vollständige End-to-End-Lösung von der Datenaufnahme bis zur benutzerfreundlichen Visualisierung zu realisieren.

**Das Quellcode-Repository der PatientSolution finden Sie hier:**
[https://github.com/k0mbinator/PatientSolution](https://github.com/k0mbinator/PatientSolution)

---

### Entwicklungsprozess und genutzte Technologien

Für dieses Projekt habe ich moderne Entwicklungstools und -methoden genutzt, um Effizienz und Code-Qualität zu gewährleisten:

* **KI-gestützte Entwicklung:** Ich habe GitHub Copilot eingesetzt, um den Lernprozess in neuen Bereichen wie HL7 zu beschleunigen, Boilerplate-Code effizient zu generieren und die Syntax zu vervollständigen. Dies ermöglichte es mir, mich voll auf die Geschäftslogik der HL7-Verarbeitung und die Integration mit der Oracle-Datenbank zu konzentrieren.
* **Code-Qualität & Sicherheit:** Ich habe SonarQube für die statische Code-Analyse genutzt, um Best Practices zu integrieren und die Code-Qualität kontinuierlich zu überwachen. Auf GitHub wurden zudem die integrierten Security Features wie CodeQL und Secret Scanning aktiviert, um potenzielle Schwachstellen frühzeitig zu erkennen und ein Bewusstsein für sichere Code-Entwicklung zu demonstrieren.

Jeder Codeabschnitt wurde von mir verstanden, angepasst und getestet, um die Funktionalität und Robustheit der Lösung sicherzustellen.

---

## Methodenübersicht (Program.cs)

Eine kurze Übersicht über die wichtigsten Methoden in der `Program.cs`-Datei:

* ### Main
    Der Einstiegspunkt der Anwendung. Initialisiert die Datenbankverbindung, prüft die HL7-Dateiverzeichnisse, steuert den gesamten Prozess der Dateiverarbeitung und zeigt die Ergebnisse an.
* ### ProcessHl7FileAsync
    Verantwortlich für das Einlesen einer einzelnen HL7-Datei, deren Parsen und die Übergabe der extrahierten Patientendaten zum Einfügen in die Datenbank.
* ### ExtractPatientData
    Extrahiert alle relevanten Patientendaten aus einem geparsten HL7-Nachrichtenobjekt und kapselt diese in ein `Patient`-Objekt.
* ### ExtractMessageType
    Liest den Nachrichtentyp (z.B. ADT^A01) aus dem MSH-Segment der HL7-Nachricht.
* ### ExtractDateOfBirth
    Extrahiert das Geburtsdatum aus dem PID-Segment und konvertiert es in einen `DateTime?`-Wert.
* ### ExtractPatientId
    Liest die eindeutige Patienten-ID aus dem PID-Segment.
* ### ExtractPatientName
    Liest Nachname und Vorname aus dem PID-Segment.
* ### ExtractGender
    Liest das Geschlecht des Patienten aus dem PID-Segment.
* ### ExtractAddress
    Liest Adressdetails (Straße, Stadt, Bundesland, PLZ) aus dem PID-Segment.
* ### InsertPatientAsync
    Fügt einen `Patient`-Datensatz in die `PATIENTS`-Tabelle der Oracle-Datenbank ein. Behandelt potenzielle Duplikatsfehler, die durch Unique Constraints verursacht werden.
* ### DisplayPatientsAsync
    Ruft alle Patientendatensätze aus der Datenbank ab und gibt sie in einem lesbaren Format auf der Konsole aus.

---

### Hinweise zur Ausführung

* **Datenbankvorbereitung:** Bei jedem Programmlauf wird die `PATIENTS`-Tabelle in der Oracle-Datenbank geleert (`TRUNCATE TABLE PATIENTS`), um einen sauberen Import und das Testen neuer HL7-Dateien zu gewährleisten. Dies ist für Demozwecke und die Vermeidung von Duplikatsfehlern eingerichtet. Im produktiven Einsatz würde eine differenziertere Logik (z.B. `MERGE` oder `INSERT ... ON CONFLICT`) verwendet werden, um bestehende Patientendaten zu aktualisieren.
* **HL7-Quelldateien:** Stellen Sie sicher, dass sich Ihre `.hl7`-Dateien im Ordner `HL7_Messages` befinden, der sich im Hauptverzeichnis des Projekts befindet. Jede Datei sollte eine einzelne HL7v2-Nachricht enthalten, wobei jedes Segment durch einen Zeilenumbruch (`\r`) getrennt ist.

---



### Beispiel-Output

```bash
PS C:\Users\User\VsCode Projects\HL7DataProcessor> dotnet run
Verbindung zur Oracle-Datenbank erfolgreich!
Tabelle PATIENTS geleert.

Starte Verarbeitung von 2 HL7-Datei(en)...

Verarbeite Datei: patient_anna.hl7
--- Nachricht erfolgreich geparst.
  Patient 'P001^^^PI' eingefügt: 1 Zeile(n).

Verarbeite Datei: patient_max.hl7
--- Nachricht erfolgreich geparst.
  Patient 'P002^^^PI' eingefügt: 1 Zeile(n).

--- Alle HL7-Dateien verarbeitet. ---

Aktuelle Patientendaten in der DB:
ID: P001^^^PI, Name: Anna Meier, Geb.Datum: 15.03.1985, Geschlecht: F, Adresse: Eichenweg 12, 10115 Berlin, Typ: ADT^A01
ID: P002^^^PI, Name: Max Schmidt, Geb.Datum: 22.07.1972, Geschlecht: M, Adresse: Birkenallee 5, 20095 Hamburg, Typ: ADT^A01
PS C:\Users\User\VsCode Projects\HL7DataProcessor>
