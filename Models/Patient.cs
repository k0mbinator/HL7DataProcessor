using System.Runtime.InteropServices;

namespace HL7DataProcessor.Models
{
    public class Patient
    {
        public required string PatientId { get; set; }
        public required string LastName { get; set; }
        public required string FirstName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? AddressStreet { get; set; }
        public string? AddressCity { get; set; }
        public string? AddressState { get; set; }
        public string? AddressZip { get; set; }
        public string? Hl7MessageType { get; set; }
        public required DateTime ReceivedDate { get; set; }
    }
}
