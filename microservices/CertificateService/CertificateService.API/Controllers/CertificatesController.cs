using CertificateService.Application.DTOs;
using CertificateService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CertificateService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CertificatesController : ControllerBase
{
    private readonly ICertificateService _service;

    public CertificatesController(ICertificateService service)
    {
        _service = service;
    }

    [HttpPost("issue")]
    public async Task<IActionResult> IssueCertificate(IssueCertificateRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (request.StudentId.ToString() != userIdClaim && !User.IsInRole("Admin"))
            return Forbid();

        var certificate = await _service.IssueCertificateAsync(request);
        return Ok(certificate);
    }

    [HttpGet("student")]
    public async Task<IActionResult> GetMyCertificates()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

        var studentId = Guid.Parse(userIdClaim);
        var certificates = await _service.GetStudentCertificatesAsync(studentId);
        return Ok(certificates);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCertificate(Guid id)
    {
        var certificate = await _service.GetCertificateByIdAsync(id);
        if (certificate == null) return NotFound();

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (certificate.StudentId.ToString() != userIdClaim && !User.IsInRole("Admin"))
            return Forbid();

        return Ok(certificate);
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadCertificate(Guid id)
    {
        try
        {
            var certificate = await _service.GetCertificateByIdAsync(id);
            if (certificate == null) return NotFound();

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (certificate.StudentId.ToString() != userIdClaim && !User.IsInRole("Admin"))
                return Forbid();

            var pdfBytes = await _service.GenerateCertificatePdfAsync(id);
            return File(pdfBytes, "application/pdf", $"Certificate-{certificate.CertificateNumber}.pdf");
        }
        catch (Exception)
        {
            return BadRequest("Failed to generate PDF.");
        }
    }

    [HttpGet("verify/{certificateNumber}")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyCertificate(string certificateNumber)
    {
        var result = await _service.VerifyCertificateAsync(certificateNumber);
        return Ok(result);
    }
}
