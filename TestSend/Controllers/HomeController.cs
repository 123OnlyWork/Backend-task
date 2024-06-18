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
                _logger.LogInformation("Подготовка к отправке верификационного кода...");

                // Проверяем, что emailAddress не пустой или null
                if (string.IsNullOrEmpty(request.EmailAddress))
                {
                    _logger.LogError("Адрес электронной почты пустой или null.");
                    return BadRequest(new { ErrorMessage = "Адрес электронной почты пустой или null." });
                }

                // Проверяем формат адреса электронной почты
                if (!IsValidEmail(request.EmailAddress))
                {
                    _logger.LogError("Неверный формат адреса электронной почты.");
                    return BadRequest(new { ErrorMessage = "Неверный формат адреса электронной почты." });
                }

                // Генерируем верификационный код
                var random = new Random();
                var verificationCode = random.Next(1000, 10000).ToString();

                // Сохраняем код в словарь
                if (verificationCodes.ContainsKey(request.EmailAddress))
                {
                    verificationCodes[request.EmailAddress] = verificationCode;
                }
                else
                {
                    verificationCodes.Add(request.EmailAddress, verificationCode);
                }

                // Отправка письма с использованием сервиса
                var emailSubject = "Verification Code";
                var emailBody = $"<h1>Ваш верификационный код: {verificationCode}</h1>";

                var result = _emailService.SendEmail(request.EmailAddress, emailSubject, emailBody);

                if (result.Success)
                {
                    _logger.LogInformation("Верификационный код успешно отправлен!");
                    return Ok(new { Message = $"Верификационный код отправлен на адрес {request.EmailAddress}." });
                }
                else
                {
                    _logger.LogError($"Ошибка при отправке верификационного кода: {result.ErrorMessage}");
                    return StatusCode(500, new { ErrorMessage = "Ошибка при отправке верификационного кода. Попробуйте позже." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при отправке верификационного кода: {ex.Message}");
                return StatusCode(500, new { ErrorMessage = "Ошибка при отправке верификационного кода. Попробуйте позже." });
            }
        }

        [HttpPost]
        public IActionResult VerifyCode([FromBody] SendingEmail.Requests.VerifyCodeRequest request)
        {
            try
            {
                _logger.LogInformation($"Проверка верификационного кода для адреса: {request.EmailAddress}");

                // Проверяем, что emailAddress не пустой или null
                if (string.IsNullOrEmpty(request.EmailAddress))
                {
                    _logger.LogError("Адрес электронной почты пустой или null.");
                    return BadRequest(new { ErrorMessage = "Адрес электронной почты пустой или null." });
                }

                // Проверяем, что код соответствует сгенерированному ранее
                if (verificationCodes.TryGetValue(request.EmailAddress, out var storedCode) && storedCode == request.Code)
                {
                    _logger.LogInformation("Верификационный код успешно проверен.");
                    // Удаляем код из словаря после успешной верификации
                    verificationCodes.Remove(request.EmailAddress);
                    return Ok(new { Success = true });
                }
                else
                {
                    _logger.LogError("Неверный верификационный код или код устарел.");
                    return BadRequest(new { ErrorMessage = "Неверный верификационный код или код устарел." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при проверке верификационного кода: {ex.Message}");
                return StatusCode(500, new { ErrorMessage = "Ошибка при проверке верификационного кода. Попробуйте позже." });
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
