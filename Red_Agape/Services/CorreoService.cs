using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using QRCoder;
using System.Net;

namespace Red_Agape.Services
{
    public class CorreoService
    {
        private readonly IConfiguration _cfg;
        public CorreoService(IConfiguration cfg) => _cfg = cfg;

        public static byte[] GenerarQrPng(string contenido)
        {
            using var gen = new QRCodeGenerator();
            using var datos = gen.CreateQrCode(contenido, QRCodeGenerator.ECCLevel.Q);
            return new PngByteQRCode(datos).GetGraphic(12);
        }

        public async Task EnviarQrDonacionAsync(string para, string nombreDonante,
            string codigo, string contenidoQr, string organizacion, int totalArticulos)
        {
            var s = _cfg.GetSection("Correo");

            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress("Red Ágape", s["Usuario"]));
            msg.To.Add(new MailboxAddress(nombreDonante, para));
            msg.Subject = $"Tu código QR de donación {codigo}";

            var body = new BodyBuilder();
            var img = body.LinkedResources.Add("qr.png", GenerarQrPng(contenidoQr));
            img.ContentId = MimeKit.Utils.MimeUtils.GenerateMessageId();

            body.HtmlBody = $@"
                <h2>¡Gracias por donar, {WebUtility.HtmlEncode(nombreDonante)}!</h2>
                <p>Código de donación: <strong>{codigo}</strong></p>
                <p>Organización: {WebUtility.HtmlEncode(organizacion)} · Artículos: {totalArticulos}</p>
                <p>Muestra este QR al coordinador cuando reciba tus artículos:</p>
                <img src=""cid:{img.ContentId}"" alt=""QR {codigo}"" width=""250"" height=""250"" />";
            body.TextBody = $"Gracias por donar. Tu código de donación es {codigo}.";

            msg.Body = body.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(s["Servidor"], int.Parse(s["Puerto"]!), SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(s["Usuario"], s["Contrasena"]);
            await smtp.SendAsync(msg);
            await smtp.DisconnectAsync(true);
        }
    }
}