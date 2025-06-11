-- 1. Neuen Benutzer mit Passwort erstellen

CREATE USER PatientAppUser IDENTIFIED BY "&&patient_app_user_password";

-- 2. Dem Benutzer Rechte zum Verbinden (Session erstellen) geben
GRANT CREATE SESSION TO PatientAppUser;

-- 3. Dem Benutzer Rechte zum Erstellen von Tabellen geben
GRANT CREATE TABLE TO PatientAppUser;

-- 4. Dem Benutzer Rechte zum Erstellen von Views geben (falls später nötig)
GRANT CREATE VIEW TO PatientAppUser;

-- 5. Dem Benutzer Rechte zum Erstellen von Sequenzen geben (falls später nötig, z.B. für auto-inkrementierende IDs)
GRANT CREATE SEQUENCE TO PatientAppUser;

-- 6. Dem Benutzer unbegrenzten Tabellenplatz zuweisen (wichtig, damit er Daten speichern kann)
GRANT UNLIMITED TABLESPACE TO PatientAppUser;

-- Optional: Wenn du den Benutzer später löschen willst (NICHT JETZT AUSFÜHREN)
-- DROP USER PatientAppUser CASCADE;
