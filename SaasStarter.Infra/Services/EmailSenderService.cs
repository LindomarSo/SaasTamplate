using Microsoft.Extensions.Configuration;
using Resend;
using SaasStarter.Application.Common.Interfaces;

namespace SaasStarter.Infra.Services;

public class EmailSenderService : IEmailSenderService
{
    private readonly IResend _resend;
    private readonly string _fromAddress;
    private readonly string _baseUrl;

    // TODO: substitua pelo nome do seu SaaS
    private const string AppName = "SaasStarter";

    public EmailSenderService(IResend resend, IConfiguration configuration)
    {
        _resend = resend;
        _fromAddress = configuration["Resend:FromAddress"]
            ?? throw new InvalidOperationException("Resend:FromAddress não configurado.");
        _baseUrl = configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
    }

    public async Task SendEmailConfirmationAsync(string email, string confirmationLink, string userName, CancellationToken cancellationToken = default)
    {
        var message = new EmailMessage
        {
            From = _fromAddress,
            To = { email },
            Subject = $"Confirme seu e-mail — {AppName}",
            HtmlBody = BuildConfirmEmailHtml(confirmationLink, userName)
        };

        await _resend.EmailSendAsync(message, cancellationToken);
    }

    public async Task SendPasswordResetAsync(string email, string resetLink, CancellationToken cancellationToken = default)
    {
        var message = new EmailMessage
        {
            From = _fromAddress,
            To = { email },
            Subject = $"Redefinição de senha — {AppName}",
            HtmlBody = BuildResetPasswordHtml(resetLink)
        };

        await _resend.EmailSendAsync(message, cancellationToken);
    }

    private string BuildConfirmEmailHtml(string link, string userName) => $"""
        <!DOCTYPE html>
        <html lang="pt-BR">
        <head>
          <meta charset="UTF-8" />
          <meta name="viewport" content="width=device-width, initial-scale=1.0" />
          <title>Confirme seu e-mail</title>
          <style>
            body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; background: #f4f4f8; margin: 0; padding: 40px 16px; }}
            .container {{ max-width: 560px; margin: auto; background: #fff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 24px rgba(0,0,0,.08); }}
            .header {{ background: #2563eb; padding: 40px 40px 32px; color: #fff; }}
            .header h1 {{ margin: 0; font-size: 26px; font-weight: 700; }}
            .body {{ padding: 40px; color: #374151; line-height: 1.65; }}
            .btn {{ display: inline-block; margin: 24px 0; padding: 14px 32px; background: #2563eb; color: #fff; border-radius: 8px; text-decoration: none; font-weight: 600; font-size: 15px; }}
            .footer {{ background: #f9fafb; padding: 20px 40px; font-size: 12px; color: #9ca3af; text-align: center; border-top: 1px solid #e5e7eb; }}
          </style>
        </head>
        <body>
          <div class="container">
            <div class="header">
              <h1>{AppName}</h1>
              <p style="margin:8px 0 0;opacity:.85">Confirme seu endereço de e-mail</p>
            </div>
            <div class="body">
              <p>Olá, <strong>{userName}</strong>!</p>
              <p>Clique no botão abaixo para confirmar seu e-mail e acessar a plataforma. O link é válido por 24 horas.</p>
              <a href="{link}" class="btn">Confirmar meu e-mail</a>
              <p style="font-size:13px;color:#6b7280">Se não conseguir clicar, copie este link: <br/><a href="{link}" style="color:#2563eb">{link}</a></p>
              <p style="font-size:12px;color:#9ca3af;margin-top:24px">Se você não criou esta conta, ignore este e-mail.</p>
            </div>
            <div class="footer">© {DateTime.UtcNow.Year} {AppName}. Todos os direitos reservados.</div>
          </div>
        </body>
        </html>
        """;

    private string BuildResetPasswordHtml(string link) => $"""
        <!DOCTYPE html>
        <html lang="pt-BR">
        <head>
          <meta charset="UTF-8" />
          <title>Redefinição de senha</title>
          <style>
            body {{ font-family: -apple-system, sans-serif; background: #f4f4f8; margin: 0; padding: 40px 16px; }}
            .container {{ max-width: 560px; margin: auto; background: #fff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 24px rgba(0,0,0,.08); }}
            .header {{ background: #dc2626; padding: 40px; color: #fff; }}
            .body {{ padding: 40px; color: #374151; line-height: 1.65; }}
            .btn {{ display: inline-block; margin: 24px 0; padding: 14px 32px; background: #dc2626; color: #fff; border-radius: 8px; text-decoration: none; font-weight: 600; font-size: 15px; }}
            .footer {{ background: #f9fafb; padding: 20px 40px; font-size: 12px; color: #9ca3af; text-align: center; border-top: 1px solid #e5e7eb; }}
          </style>
        </head>
        <body>
          <div class="container">
            <div class="header"><h1 style="margin:0">{AppName} — Redefinição de senha</h1></div>
            <div class="body">
              <p>Recebemos uma solicitação para redefinir a senha da sua conta. Clique no botão abaixo. O link expira em 1 hora.</p>
              <a href="{link}" class="btn">Redefinir senha</a>
              <p style="font-size:12px;color:#9ca3af;margin-top:24px">Se você não solicitou a redefinição, ignore este e-mail com segurança.</p>
            </div>
            <div class="footer">© {DateTime.UtcNow.Year} {AppName}.</div>
          </div>
        </body>
        </html>
        """;
}
