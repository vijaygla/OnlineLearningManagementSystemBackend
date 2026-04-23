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
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> IssueCertificate(IssueCertificateRequest request)
    {
        var issuedBy = User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        var certificate = await _service.IssueCertificateAsync(request, issuedBy);
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
        var userRoleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

        if (certificate.StudentId.ToString() != userIdClaim && userRoleClaim != "Admin")
            return Forbid();

        return Ok(certificate);
    }

    [HttpGet("verify/{certificateNumber}")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyCertificate(string certificateNumber)
    {
        var result = await _service.VerifyCertificateAsync(certificateNumber);
        return Ok(result);
    }
}
