using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CourtCaseTrackingSystem.Models;

public class FileStorageService
{
    private readonly IWebHostEnvironment _env;
    private const string UploadsFolder = "CaseDocuments";
    private const string SummonsFolder = "Summons"; // New folder for summon letters
private const long MaxFileSize = 50 * 1024 * 1024;
    private static readonly string[] AllowedExtensions = [".pdf", ".doc", ".docx"];

    public FileStorageService(IWebHostEnvironment env)
    {
        _env = env;

        // Create summons directory if it doesn't exist
        var summonsPath = Path.Combine(_env.WebRootPath, SummonsFolder);
        Directory.CreateDirectory(summonsPath);
    }

    // Existing document save method remains unchanged
   public async Task<string?> SaveDocument(IFormFile file, DocumentType documentType)
{
    if (file == null || file.Length == 0) return null;

    ValidateFile(file);

    // Determine folder based on document type
    var folder = documentType switch
    {
        DocumentType.Defense => "DefenseDocuments",
        DocumentType.Witness => "WitnessDocuments",
        _ => "CaseDocuments" // Default for DocumentType.Case
    };

    var uploadsPath = Path.Combine(_env.WebRootPath, folder);
    Directory.CreateDirectory(uploadsPath);

    var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
    var filePath = Path.Combine(uploadsPath, uniqueFileName);

    using (var stream = new FileStream(filePath, FileMode.Create))
    {
        await file.CopyToAsync(stream);
    }

    return $"/{folder}/{uniqueFileName}";
}

    // New method for saving summon letters
    public string SaveSummonLetter(byte[] fileContent, string fileName)
    {
        var summonsPath = Path.Combine(_env.WebRootPath, "summons");
        Directory.CreateDirectory(summonsPath);

        var safeFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{Guid.NewGuid():n}.pdf";
        var fullPath = Path.Combine(summonsPath, safeFileName);

        File.WriteAllBytes(fullPath, fileContent);
        return $"/summons/{safeFileName}";
    }

    // Existing validation method
    private void ValidateFile(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException("Unsupported file type");

        if (file.Length > MaxFileSize)
            throw new InvalidOperationException("File size exceeds 50MB limit");
    }
}