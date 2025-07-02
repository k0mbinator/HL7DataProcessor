using Efferent.HL7.V2;
using Oracle.ManagedDataAccess.Client;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using HL7DataProcessor.Models;

namespace HL7DataProcessor
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            string? dbPassword = Environment.GetEnvironmentVariable("PATIENT_APP_DB_PASSWORD");
            if (string.IsNullOrEmpty(dbPassword))
            {
                Console.WriteLine("Datenbank-Passwort nicht gesetzt. Bitte setze die Umgebungsvariable PATIENT_APP_DB_PASSWORD.");
                return;
            }
            string connectionString = $"DATA SOURCE=localhost:1521/XEPDB1;USER ID=PatientAppUser;PASSWORD={dbPassword};";

            string hl7Directory = Path.Combine(AppContext.BaseDirectory, "HL7_Messages");

            if (!Directory.Exists(hl7Directory))
            {
                Directory.CreateDirectory(hl7Directory);
                Console.WriteLine($"Ordner '{hl7Directory}' erstellt. Bitte HL7-Dateien dort ablegen und Programm neu starten.");
                return;
            }

            string[] hl7Files = Directory.GetFiles(hl7Directory, "*.hl7");
            if (hl7Files.Length == 0)
            {
                Console.WriteLine($"Keine HL7-Dateien im Ordner '{hl7Directory}' gefunden.");
                return;
            }

            await using (var connection = new OracleConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    Console.WriteLine("Verbindung zur Oracle-Datenbank erfolgreich!");

                    // Tabelle vor dem Einfügen leeren
                    using (OracleCommand truncateCommand = new OracleCommand("TRUNCATE TABLE PATIENTS", connection))
                    {
                        await truncateCommand.ExecuteNonQueryAsync();
                        Console.WriteLine("Tabelle PATIENTS geleert.");
                    }



                    Console.WriteLine($"\nStarte Verarbeitung von {hl7Files.Length} HL7-Datei(en)...");
                    foreach (var hl7FilePath in hl7Files)
                        await ProcessHl7FileAsync(hl7FilePath, connection);

                    Console.WriteLine("\n--- Alle HL7-Dateien verarbeitet. ---");
                    await DisplayPatientsAsync(connection);
                }
                catch (OracleException ex)
                {
                    Console.WriteLine($"Oracle Fehler: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Allgemeiner Fehler: {ex.Message}");
                }
            }
        }

        private static async Task ProcessHl7FileAsync(string hl7FilePath, OracleConnection connection)
        {
            string hl7MessageString = await File.ReadAllTextAsync(hl7FilePath);
            Console.WriteLine($"\nVerarbeite Datei: {Path.GetFileName(hl7FilePath)}");

            var message = new Message(hl7MessageString);
            if (!message.ParseMessage())
            {
                Console.WriteLine($"  Fehler beim Parsen der Nachricht aus Datei '{Path.GetFileName(hl7FilePath)}'.");
                return;
            }
            Console.WriteLine("--- Nachricht erfolgreich geparst.");

            var patient = ExtractPatientData(message);
            if (string.IsNullOrEmpty(patient.PatientId))
            {
                Console.WriteLine($"  Warnung: Keine Patient ID in '{Path.GetFileName(hl7FilePath)}' gefunden. Überspringe Einfügen.");
                return;
            }
            await InsertPatientAsync(connection, patient);
        }

        private static Patient ExtractPatientData(Message message)
        {
            var patient = new Patient
            {
                PatientId = "",
                FirstName = "",
                LastName = "",
                ReceivedDate = DateTime.Now
            };

            ExtractMessageType(message, patient);
            var pid = message.Segments("PID").FirstOrDefault();
            if (pid != null)
            {
                patient.PatientId = ExtractPatientId(pid);
                (patient.LastName, patient.FirstName) = ExtractPatientName(pid);
                patient.DateOfBirth = ExtractDateOfBirth(pid);
                patient.Gender = ExtractGender(pid);
                (patient.AddressStreet, patient.AddressCity, patient.AddressState, patient.AddressZip) = ExtractAddress(pid);
            }
            return patient;
        }

        private static void ExtractMessageType(Message message, Patient patient)
        {
            var msh = message.Segments("MSH").FirstOrDefault();
            var field = msh?.Fields(9);
            if (field != null && field.Components(1) != null && field.Components(2) != null)
                patient.Hl7MessageType = $"{field.Components(1).Value}^{field.Components(2).Value}";
        }

        private static DateTime? ExtractDateOfBirth(Segment pid)
        {
            var field = pid.Fields(7);
            if (!string.IsNullOrEmpty(field.Value) &&
                DateTime.TryParseExact(field.Value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dobParsed))
                return dobParsed;
            return null;
        }

        private static string ExtractPatientId(Segment pid)
        {
            var field = pid.Fields(3);
            return !string.IsNullOrEmpty(field.Value) ? field.Value : "";
        }

        private static (string LastName, string FirstName) ExtractPatientName(Segment pid)
        {
            var field = pid.Fields(5);
            if (!string.IsNullOrEmpty(field.Value))
                return (field.Components(1).Value, field.Components(2).Value);
            return ("", "");
        }

        private static string ExtractGender(Segment pid)
        {
            var field = pid.Fields(8);
            return !string.IsNullOrEmpty(field.Value) ? field.Value : "";
        }

        private static (string AddressStreet, string AddressCity, string AddressState, string AddressZip) ExtractAddress(Segment pid)
        {
            var field = pid.Fields(11);
            if (!string.IsNullOrEmpty(field.Value))
                return (
                    field.Components(1).Value,
                    field.Components(3).Value,
                    field.Components(4).Value,
                    field.Components(5).Value
                );
            return ("", "", "", "");
        }

        private static async Task InsertPatientAsync(OracleConnection connection, Patient patient)
        {
            string insertQuery = @"
            INSERT INTO PATIENTS (
                PATIENT_ID, FIRST_NAME, LAST_NAME, DATE_OF_BIRTH, GENDER,
                ADDRESS_STREET, ADDRESS_CITY, ADDRESS_STATE, ADDRESS_ZIP,
                HL7_MESSAGE_TYPE, RECEIVED_DATE
            ) VALUES (
                :p_patient_id, :p_first_name, :p_last_name, :p_date_of_birth, :p_gender,
                :p_address_street, :p_address_city, :p_address_state, :p_address_zip,
                :p_hl7_message_type, :p_received_date
            )";

            using (var command = new OracleCommand(insertQuery, connection))
            {
                command.Parameters.Add("p_patient_id", OracleDbType.Varchar2).Value = patient.PatientId;
                command.Parameters.Add("p_first_name", OracleDbType.Varchar2).Value = patient.FirstName;
                command.Parameters.Add("p_last_name", OracleDbType.Varchar2).Value = patient.LastName;
                command.Parameters.Add("p_date_of_birth", OracleDbType.Date).Value = patient.DateOfBirth.HasValue ? patient.DateOfBirth.Value : (object)DBNull.Value;
                command.Parameters.Add("p_gender", OracleDbType.Varchar2).Value = patient.Gender;
                command.Parameters.Add("p_address_street", OracleDbType.Varchar2).Value = patient.AddressStreet;
                command.Parameters.Add("p_address_city", OracleDbType.Varchar2).Value = patient.AddressCity;
                command.Parameters.Add("p_address_state", OracleDbType.Varchar2).Value = patient.AddressState;
                command.Parameters.Add("p_address_zip", OracleDbType.Varchar2).Value = patient.AddressZip;
                command.Parameters.Add("p_hl7_message_type", OracleDbType.Varchar2).Value = patient.Hl7MessageType;
                command.Parameters.Add("p_received_date", OracleDbType.TimeStamp).Value = patient.ReceivedDate;

                try
                {
                    int rowsInserted = await command.ExecuteNonQueryAsync();
                    Console.WriteLine($"  Patient '{patient.PatientId}' eingefügt: {rowsInserted} Zeile(n).");
                }
                catch (OracleException ex)
                {
                    Console.WriteLine($"  Fehler beim Einfügen von Patient '{patient.PatientId}': {ex.Message}");
                    if (ex.Number == 1)
                        Console.WriteLine($"    (Patient '{patient.PatientId}' existiert bereits. Überspringe Einfügen.)");
                }
            }
        }

        private static async Task DisplayPatientsAsync(OracleConnection connection)
        {
            string selectQuery = "SELECT * FROM PATIENTS ORDER BY PATIENT_ID";
            using (var cmd = new OracleCommand(selectQuery, connection))
            using (var rdr = await cmd.ExecuteReaderAsync())
            {
                Console.WriteLine("\nAktuelle Patientendaten in der DB:");
                if (!rdr.HasRows)
                {
                    Console.WriteLine("  Keine Daten gefunden.");
                }
                while (await rdr.ReadAsync())
                {
                    Console.WriteLine($"ID: {rdr["PATIENT_ID"]}, Name: {rdr["FIRST_NAME"]} {rdr["LAST_NAME"]}, Geb.Datum: {rdr["DATE_OF_BIRTH"]:dd.MM.yyyy}, Geschlecht: {rdr["GENDER"]}, Adresse: {rdr["ADDRESS_STREET"]}, {rdr["ADDRESS_ZIP"]} {rdr["ADDRESS_CITY"]}, Typ: {rdr["HL7_MESSAGE_TYPE"]}");
                }
            }
        }
    }
}