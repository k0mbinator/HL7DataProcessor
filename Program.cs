using System.Security.Cryptography.Pkcs;
using Efferent.HL7.V2;
using Oracle.ManagedDataAccess;
using Oracle.ManagedDataAccess.Client;

static async Task Main(string[] args)
{
    // Get the database password from an environment variable for security
    string? dbPassword = Environment.GetEnvironmentVariable("PATIENT_APP_DB_PASSWORD");
    if (string.IsNullOrEmpty(dbPassword))
    {
        Console.WriteLine("Database password not set. Please set the PATIENT_APP_DB_PASSWORD environment variable.");
        return;
    }
    string connectionString = $"DATA SOURCE=localhost:1521/XEPDB1;USER ID=PatientAppUser;PASSWORD={dbPassword};";
    await using (OracleConnection connection = new OracleConnection(connectionString))
    {
        try
        {
            await connection.OpenAsync();
            Console.WriteLine("Connection successful!");

            // //Clear Table before Insert
            // using (OracleCommand truncateCommand = new OracleCommand("TRUNCATE TABLE PATIENTS", connection))
            // {
            //     await truncateCommand.ExecuteNonQueryAsync();
            //     Console.WriteLine("Tabelle PATIENTS geleert.");
            // }

            //Reading data from HL7 file

            string hl7folderPath = Environment.GetEnvironmentVariable("HL7_FOLDER_PATH")!;
            if (string.IsNullOrEmpty(hl7folderPath))
            {
                Console.WriteLine("HL7_FOLDER_PATH environment variable not set.\n" +
                                  "Please set the HL7_FOLDER_PATH environment variable to the path where your HL7 files are stored.\n" +
                                  "Example: HL7_FOLDER_PATH=/path/to/your/hl7/files");
                 
                return;
            }
            Console.WriteLine($"Reading HL7 files from: {hl7folderPath}");
            

            //Reading data from the database
            string selectQuery = "SELECT * FROM PATIENTS";
            using (OracleCommand command = new OracleCommand(selectQuery, connection))
            {
                using (OracleDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Console.WriteLine($"PatientID: {reader["Patient_ID"]}, FirstName: {reader["First_Name"]}, LastName: {reader["Last_Name"]}, DateOfBirth: {reader["Date_Of_Birth"]}");
                    }
                }
            }

        }
        catch (OracleException ex)
        {
            Console.WriteLine($"Oracle error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General error: {ex.Message}");
        }
    }
}



class HL7Processor
{

    public string PatientID { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
    
    public DateTime DateOfBirth { get; set; }


    //Ich will die HL7-Dateien in einem bestimmten Ordner verarbeiten
    //Was brauche ich dafür?
    //1. Ich brauche den Pfad zu dem Ordner, in dem die HL7-Dateien gespeichert sind
    //2. Ich brauche eine Methode, die alle HL7-Dateien in diesem Ordner liest und verarbeitet
    //3. Ich brauche eine Methode, die die HL7-Dateien in einer bestimmten Reihenfolge verarbeitet
    //4. Ich brauche eine Methode, die die HL7-Dateien möglicherweise für OracleDB formatiert
    //5. Ich brauche eine Methode, die die HL7-Dateien in die OracleDB einfügt

    public


    public static async Task ProcessHL7FilesAsync(string hl7folderPath)
    {
        if (!Directory.Exists(hl7folderPath))
        {
            Console.WriteLine($"The specified HL7 folder path does not exist: {hl7folderPath}");
            return;
        }

        // Get all HL7 files in the specified folder
        string[] hl7Files = Directory.GetFiles(hl7folderPath, "*.hl7");
        foreach (var hl7File in hl7Files)
        {
            Console.WriteLine($"Processing file: {hl7File}");
            string hl7Message = await File.ReadAllTextAsync(hl7File);
            var message = new Message(hl7Message);

            // Extracting patient data from the HL7 message
            var pidSegment = message.Segments("PID").FirstOrDefault();
            if (pidSegment == null)
            {
                Console.WriteLine("PID segment not found in HL7 message.");
                continue;
            }
            string PatientID = pidSegment.Fields(2).Value; // PID-3 (index 2, as HL7 is 1-based)
            string LastName = pidSegment.Fields(5).Components(1).Value; // PID-5.1 (index 1)
            string FirstName = pidSegment.Fields(5).Components(2).Value; // PID-5.2 (index 2)
            DateTime DateOfBirth = DateTime.Parse(pidSegment.Fields(7).Value); // PID-7
        }

        // Parse each HL7 file

        foreach ()



    }
}










// Message message = new Message();
// // Notice that every HL7 message needs a header segment to be considered valid!
// // The header segment is the first segment of the message and contains information about the message itself.
// // The header segment is always the first segment of the message and is called MSH (Message Header).
// // The MSH segment contains information about the message, such as the message type, the sending and receiving application, and the date and time of the message.

// message.AddSegmentMSH(sendingApplication: "SendingApp", sendingFacility: "SendingFacility", receivingApplication: "ReceivingApp", receivingFacility: "ReceivingFacility", messageType: "ADT^A01", messageControlId: "123456", processingId: "P", versionId: "2.5");




//Sample Data
//const string HL7_ADT_A01 = "HL7_ADT_A01";
//         var patients = new List<dynamic>
// {
//     new { ID = "P001", FN = "Anna", LN = "Meier", DOB = new DateTime(1985, 3, 15, 0, 0, 0, DateTimeKind.Utc), G = "F", Street = "Eichenweg 12", City = "Berlin", State = "BE", Zip = "10115", MsgType = HL7_ADT_A01 },
//     new { ID = "P002", FN = "Max", LN = "Schmidt", DOB = new DateTime(1972, 7, 22, 0, 0, 0, DateTimeKind.Utc), G = "M", Street = "Birkenallee 5", City = "Hamburg", State = "HH", Zip = "20095", MsgType = HL7_ADT_A01 },
//     new { ID = "P003", FN = "Lena", LN = "Weber", DOB = new DateTime(1993, 11, 5, 0, 0, 0, DateTimeKind.Utc), G = "F", Street = "Ahornweg 7", City = "München", State = "BY", Zip = "80331", MsgType = HL7_ADT_A01 },
//     new { ID = "P004", FN = "Tim", LN = "Fischer", DOB = new DateTime(1968, 4, 18, 0, 0, 0, DateTimeKind.Utc), G = "M", Street = "Kastanienstr. 3", City = "Köln", State = "NRW", Zip = "50667", MsgType = HL7_ADT_A01 },
//     new { ID = "P005", FN = "Julia", LN = "Becker", DOB = new DateTime(2001, 9, 30, 0, 0, 0, DateTimeKind.Utc), G = "F", Street = "Ulmenweg 99", City = "Frankfurt", State = "HE", Zip = "60311", MsgType = HL7_ADT_A01 },
//     new { ID = "P006", FN = "Felix", LN = "Meyer", DOB = new DateTime(1989, 1, 10, 0, 0, 0, DateTimeKind.Utc), G = "M", Street = "Lindenstr. 1", City = "Stuttgart", State = "BW", Zip = "70173", MsgType = HL7_ADT_A01 },
//     new { ID = "P007", FN = "Sophie", LN = "Wagner", DOB = new DateTime(1975, 6, 25, 0, 0, 0, DateTimeKind.Utc), G = "F", Street = "Buchenweg 23", City = "Düsseldorf", State = "NRW", Zip = "40213", MsgType = HL7_ADT_A01 },
//     new { ID = "P008", FN = "Paul", LN = "Schulz", DOB = new DateTime(1996, 12, 1, 0, 0, 0, DateTimeKind.Utc), G = "M", Street = "Fichtenweg 45", City = "Leipzig", State = "SN", Zip = "04109", MsgType = HL7_ADT_A01 },
//     new { ID = "P009", FN = "Mia", LN = "Koch", DOB = new DateTime(1982, 8, 8, 0, 0, 0, DateTimeKind.Utc), G = "F", Street = "Weidenweg 78", City = "Dortmund", State = "NRW", Zip = "44135", MsgType = HL7_ADT_A01 },
//     new { ID = "P010", FN = "Luca", LN = "Müller", DOB = new DateTime(1999, 2, 14, 0, 0, 0, DateTimeKind.Utc), G = "M", Street = "Kiefernstr. 50", City = "Essen", State = "NRW", Zip = "45127", MsgType = HL7_ADT_A01 },
//     new { ID = "P011", FN = "Emilia", LN = "Richter", DOB = new DateTime(1970, 10, 3, 0, 0, 0, DateTimeKind.Utc), G = "F", Street = "Zedernweg 11", City = "Bremen", State = "HB", Zip = "28195", MsgType = HL7_ADT_A01 },
//     new { ID = "P012", FN = "Jonas", LN = "Hartmann", DOB = new DateTime(1987, 5, 20, 0, 0, 0, DateTimeKind.Utc), G = "M", Street = "Tannenweg 13", City = "Dresden", State = "SN", Zip = "01067", MsgType = HL7_ADT_A01 },
//     new { ID = "P013", FN = "Laura", LN = "Hoffmann", DOB = new DateTime(1994, 9, 7, 0, 0, 0, DateTimeKind.Utc), G = "F", Street = "Rosenstr. 4", City = "Hannover", State = "NI", Zip = "30159", MsgType = HL7_ADT_A01 },
//     new { ID = "P014", FN = "Noah", LN = "Keller", DOB = new DateTime(1965, 3, 29, 0, 0, 0, DateTimeKind.Utc), G = "M", Street = "Tulpenweg 10", City = "Nürnberg", State = "BY", Zip = "90403", MsgType = HL7_ADT_A01 },
//     new { ID = "P015", FN = "Mila", LN = "Lorenz", DOB = new DateTime(1991, 1, 1, 0, 0, 0, DateTimeKind.Utc), G = "F", Street = "Nelkenstr. 22", City = "Duisburg", State = "NRW", Zip = "47051", MsgType = HL7_ADT_A01 }
// };


//         // Insert-Loop
//         string insertQuery = @"
//     INSERT INTO PATIENTS (
//         PATIENT_ID, FIRST_NAME, LAST_NAME, DATE_OF_BIRTH, GENDER,
//         ADDRESS_STREET, ADDRESS_CITY, ADDRESS_STATE, ADDRESS_ZIP,
//         HL7_MESSAGE_TYPE, RECEIVED_DATE
//     ) VALUES (
//         :p_patient_id, :p_first_name, :p_last_name, :p_date_of_birth, :p_gender,
//         :p_address_street, :p_address_city, :p_address_state, :p_address_zip,
//         :p_hl7_message_type, :p_received_date
//     )";

//         foreach (var p in patients)
//         {
//             using (OracleCommand command = new OracleCommand(insertQuery, connection))
//             {
//                 // Wichtig: OracleDbType für korrekte Typzuordnung!
//                 command.Parameters.Add("p_patient_id", OracleDbType.Varchar2).Value = p.ID;
//                 command.Parameters.Add("p_first_name", OracleDbType.Varchar2).Value = p.FN;
//                 command.Parameters.Add("p_last_name", OracleDbType.Varchar2).Value = p.LN;
//                 command.Parameters.Add("p_date_of_birth", OracleDbType.Date).Value = p.DOB;
//                 command.Parameters.Add("p_gender", OracleDbType.Varchar2).Value = p.G;
//                 command.Parameters.Add("p_address_street", OracleDbType.Varchar2).Value = p.Street;
//                 command.Parameters.Add("p_address_city", OracleDbType.Varchar2).Value = p.City;
//                 command.Parameters.Add("p_address_state", OracleDbType.Varchar2).Value = p.State;
//                 command.Parameters.Add("p_address_zip", OracleDbType.Varchar2).Value = p.Zip;
//                 command.Parameters.Add("p_hl7_message_type", OracleDbType.Varchar2).Value = p.MsgType;
//                 command.Parameters.Add("p_received_date", OracleDbType.TimeStamp).Value = DateTime.Now; // Immer aktuelles Datum

//                 await command.ExecuteNonQueryAsync();
//             }
//         }