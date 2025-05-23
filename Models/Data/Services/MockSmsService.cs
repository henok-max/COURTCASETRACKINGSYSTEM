using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;  // For ILogger


public class MockSmsService : ISmsService
{
    private readonly List<SmsLogEntry> _sentMessages = new();
    private readonly ILogger<MockSmsService> _logger;

    public MockSmsService(ILogger<MockSmsService> logger)
    {
        _logger = logger;
    }

    public Task SendSms(string phoneNumber, string message)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return Task.CompletedTask;

        _sentMessages.Add(new SmsLogEntry
        {
            Timestamp = DateTime.UtcNow,
            PhoneNumber = phoneNumber,
            Message = message
        });
        
        _logger.LogInformation("Mock SMS to {Phone}: {Message}", 
            phoneNumber, message);
            
        return Task.CompletedTask;
    }

    public List<SmsLogEntry> GetSentMessages() => _sentMessages;
}