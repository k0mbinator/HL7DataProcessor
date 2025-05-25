# HL7DataProcessor

Dieses Projekt verarbeitet HL7-Dateien, extrahiert Patientendaten und speichert sie in einer Oracle-Datenbank.
### Hinweise zur Ausführung

* **Datenbankvorbereitung:** Bei jedem Programmlauf wird die `PATIENTS`-Tabelle in der Oracle-Datenbank geleert (`TRUNCATE TABLE PATIENTS`), um einen sauberen Import und das Testen neuer HL7-Dateien zu gewährleisten. Dies ist für Demozwecke und die Vermeidung von Duplikatsfehlern eingerichtet. Im produktiven Einsatz würde eine differenziertere Logik (z.B. `MERGE` oder `INSERT ... ON CONFLICT`) verwendet werden, um bestehende Patientendaten zu aktualisieren.
* **HL7-Quelldateien:** Stellen Sie sicher, dass sich Ihre `.hl7`-Dateien im Ordner `HL7_Messages` befinden, der sich im Hauptverzeichnis des Projekts befindet. Jede Datei sollte eine einzelne HL7v2-Nachricht enthalten, wobei jedes Segment durch einen Zeilenumbruch (`\r`) getrennt ist.


## Methodenübersicht (Program.cs)

### Main
- Einstiegspunkt. Prüft Umgebungsvariablen, sucht HL7-Dateien, öffnet die DB-Verbindung und steuert den Ablauf.

### ProcessHl7FileAsync
- Liest eine HL7-Datei ein, parst sie und übergibt die Patientendaten zum Einfügen in die Datenbank.

### ExtractPatientData
- Extrahiert alle relevanten Patientendaten aus einer HL7-Nachricht in ein Patient-Objekt.

### ExtractMessageType
- Liest den Nachrichtentyp (z.B. ADT^A01) aus dem MSH-Segment der HL7-Nachricht.

### ExtractDateOfBirth
- Extrahiert das Geburtsdatum aus dem PID-Segment und gibt es als `DateTime?` zurück.

### ExtractPatientId
- Liest die Patienten-ID aus dem PID-Segment.

### ExtractPatientName
- Liest Nachname und Vorname aus dem PID-Segment.

### ExtractGender
- Liest das Geschlecht aus dem PID-Segment.

### ExtractAddress
- Liest Straße, Stadt, Bundesland und PLZ aus dem PID-Segment.

### InsertPatientAsync
- Fügt einen Patienten-Datensatz in die Datenbank ein. Behandelt auch Duplikate.

### DisplayPatientsAsync
- Gibt alle Patientendatensätze aus der Datenbank auf der Konsole aus.

---

**Hinweis:**
- Die HL7-Feldzuordnung erfolgt nach Standard (PID-Segment).
- Fehlerbehandlung und Logging sind integriert.
- Für Anpassungen an der Datenbankstruktur oder HL7-Parsing bitte die jeweiligen Methoden anpassen.
