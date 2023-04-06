using System.ComponentModel.DataAnnotations;

namespace SpendLess.Shared;

public class Message
{
    public int Id { get; set; }
    public int senderID { get; set; }
    public int ticketID { get; set; }
    public string? message { get; set; }
    public DateTime? date { get; set; }
}