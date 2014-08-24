using validate;

namespace validate
{
	public static class notify
	{

		bool notify = true;

		if (notify)
		{
		    MailMessage() = Console.WriteLine("Event has been verified");
		}
		else
		{
		    MailMessage() = Console.WriteLine("Event has not been verified");
		}

		SmtpClient smtpClient = new SmtpClient("mail.MyWebsiteDomainName.com", 25);

		smtpClient.Credentials = new System.Net.NetworkCredential("info@MyWebsiteDomainName.com", "myIDPassword");
		smtpClient.UseDefaultCredentials = true;
		smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
		smtpClient.EnableSsl = true;
		MailMessage mail = new MailMessage();

		//Setting From , To and CC
		mail.From = new MailAddress("elkingtowa@gmail.com", "D.A.");
		//get email from input
		mail.To.Add(new MailAddress("@MyWebsiteDomainName"));
		mail.CC.Add(new MailAddress("elkingtowa@gmail.com"));

		smtpClient.Send(mail);
	}
}