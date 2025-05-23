using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CourtCaseTrackingSystem.Models;

public interface ISmsService
{
    Task SendSms(string phoneNumber, string message);
    List<SmsLogEntry> GetSentMessages();
}

public class SmsLogEntry
{
    public DateTime Timestamp { get; set; }
    public string PhoneNumber { get; set; }
    public string Message { get; set; }
}