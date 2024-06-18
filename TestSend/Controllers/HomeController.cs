using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using SendingEmail.Services;
using SendingEmail.Requests;
using System.Diagnostics;
using TestSend.Models;
using System.Collections.Generic;

namespace SendingEmail.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEmailService _emailService;
        private static Dictionary<string, string> verificationCodes = new Dictionary<string, string>();

        public HomeController(ILogger<HomeController> logger, IEmailService emailService)
        {
            _logger = logger;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SendVerificationCode([FromBody] SendingEmail.Requests.SendVerificationCodeRequest request)
        {
            try
            {
                _logger.LogInformation("���������� � �������� ���������������� ����...");

                // ���������, ��� emailAddress �� ������ ��� null
                if (string.IsNullOrEmpty(request.EmailAddress))
                {
                    _logger.LogError("����� ����������� ����� ������ ��� null.");
                    return BadRequest(new { ErrorMessage = "����� ����������� ����� ������ ��� null." });
                }

                // ��������� ������ ������ ����������� �����
                if (!IsValidEmail(request.EmailAddress))
                {
                    _logger.LogError("�������� ������ ������ ����������� �����.");
                    return BadRequest(new { ErrorMessage = "�������� ������ ������ ����������� �����." });
                }

                // ���������� ��������������� ���
                var random = new Random();
                var verificationCode = random.Next(1000, 10000).ToString();

                // ��������� ��� � �������
                if (verificationCodes.ContainsKey(request.EmailAddress))
                {
                    verificationCodes[request.EmailAddress] = verificationCode;
                }
                else
                {
                    verificationCodes.Add(request.EmailAddress, verificationCode);
                }

                // �������� ������ � �������������� �������
                var emailSubject = "Verification Code";
                var emailBody = $"<h1>��� ��������������� ���: {verificationCode}</h1>";

                var result = _emailService.SendEmail(request.EmailAddress, emailSubject, emailBody);

                if (result.Success)
                {
                    _logger.LogInformation("��������������� ��� ������� ���������!");
                    return Ok(new { Message = $"��������������� ��� ��������� �� ����� {request.EmailAddress}." });
                }
                else
                {
                    _logger.LogError($"������ ��� �������� ���������������� ����: {result.ErrorMessage}");
                    return StatusCode(500, new { ErrorMessage = "������ ��� �������� ���������������� ����. ���������� �����." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"������ ��� �������� ���������������� ����: {ex.Message}");
                return StatusCode(500, new { ErrorMessage = "������ ��� �������� ���������������� ����. ���������� �����." });
            }
        }

        [HttpPost]
        public IActionResult VerifyCode([FromBody] SendingEmail.Requests.VerifyCodeRequest request)
        {
            try
            {
                _logger.LogInformation($"�������� ���������������� ���� ��� ������: {request.EmailAddress}");

                // ���������, ��� emailAddress �� ������ ��� null
                if (string.IsNullOrEmpty(request.EmailAddress))
                {
                    _logger.LogError("����� ����������� ����� ������ ��� null.");
                    return BadRequest(new { ErrorMessage = "����� ����������� ����� ������ ��� null." });
                }

                // ���������, ��� ��� ������������� ���������������� �����
                if (verificationCodes.TryGetValue(request.EmailAddress, out var storedCode) && storedCode == request.Code)
                {
                    _logger.LogInformation("��������������� ��� ������� ��������.");
                    // ������� ��� �� ������� ����� �������� �����������
                    verificationCodes.Remove(request.EmailAddress);
                    return Ok(new { Success = true });
                }
                else
                {
                    _logger.LogError("�������� ��������������� ��� ��� ��� �������.");
                    return BadRequest(new { ErrorMessage = "�������� ��������������� ��� ��� ��� �������." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"������ ��� �������� ���������������� ����: {ex.Message}");
                return StatusCode(500, new { ErrorMessage = "������ ��� �������� ���������������� ����. ���������� �����." });
            }
        }


        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public IActionResult Congratulations()
        {
            return View();
        }

        public IActionResult ErrorPage(string message)
        {
            ViewBag.ErrorMessage = message;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
