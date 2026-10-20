namespace NWSDB.Core.Entities; // Defining the namespace for the TransactionAudit entity

public class TransactionAudit // Represents an audit record for a transaction performed in the system
{
    public int Id { get; set; } // Unique identifier for the transaction audit record
    public DateTime Timestamp { get; set; } = DateTime.UtcNow; // The date and time when the transaction occurred
    public string Action { get; set; } = string.Empty; // e.g. "BillPayment", "MeterUpdate", "BillGenerated"
    public string PerformedBy { get; set; } = "System"; // The user or system that performed the action
    public string? AccountNumber { get; set; } // The account number associated with the transaction (nullable)
    public string? ReferenceId { get; set; } // A reference ID for the transaction (nullable)
    public string Details { get; set; } = string.Empty; // Additional details about the transaction
    public string SourceIpOrClient { get; set; } = "Local"; // The source IP address or client identifier from which the action was performed
}
