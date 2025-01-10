using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace SendEmail{
    public class Email{
        public string Provedor { get; private set; }//contrutores para o email
        public string Username { get; private set; }
        public string Password { get; private set; }
    
        public Email(string? provedor, string? username, string? password){
            Provedor = provedor ?? throw new ArgumentNullException(nameof(provedor));
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Password = password ?? throw new ArgumentNullException(nameof(password));
        }

        public void SendEmail(string emailTo, string emailSubject, string emailMessage){//dispara o smtp

            var message = PreparateMessage(emailTo, emailSubject, emailMessage);
            SendEmailSMTP(message);
        }

        private MailMessage PreparateMessage(string emailTo, string emailSubject, string emailMessage){//prepara os dados que serao enviados
            var mail = new MailMessage();
            mail.From = new MailAddress(Username);
            if(ValidateEmail(emailTo))
                mail.To.Add(emailTo);
            mail.Subject = emailSubject;
            mail.Body = emailMessage;
            mail.IsBodyHtml = true;

            return mail;
        }

        private void SendEmailSMTP(MailMessage message){//configura como sera enviado o email
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = Provedor;
            smtpClient.Port = 587;
            smtpClient.EnableSsl = true;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.Timeout = 50000;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(Username, Password);
            smtpClient.Send(message);
            smtpClient.Dispose();
        }

        private bool ValidateEmail(string email){//valida o email
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            if (regex.IsMatch(email))
                return true;
            return false;
        }
    }
}