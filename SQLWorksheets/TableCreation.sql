CREATE TABLE PATIENTS (
    PATIENT_ID VARCHAR2(50) PRIMARY KEY,      -- PID.3 (z.B. Patienten-ID des Krankenhauses)
    FIRST_NAME VARCHAR2(100) NOT NULL,        -- PID.5.2 (Vorname)
    LAST_NAME VARCHAR2(100) NOT NULL,         -- PID.5.1 (Nachname)
    DATE_OF_BIRTH DATE,                       -- PID.7 (Geburtsdatum)
    GENDER VARCHAR2(10),                      -- PID.8 (Geschlecht, z.B. 'M' oder 'F')
    ADDRESS_STREET VARCHAR2(200),             -- PID.11.1 (Straße)
    ADDRESS_CITY VARCHAR2(100),               -- PID.11.3 (Stadt)
    ADDRESS_STATE VARCHAR2(50),               -- PID.11.4 (Bundesland/Region)
    ADDRESS_ZIP VARCHAR2(20),                 -- PID.11.5 (PLZ)
    HL7_MESSAGE_TYPE VARCHAR2(20),            -- MSH.9 (Der Typ der HL7-Nachricht, z.B. ADT^A01)
    RECEIVED_DATE TIMESTAMP                   -- Wann die Nachricht empfangen/verarbeitet wurde
);