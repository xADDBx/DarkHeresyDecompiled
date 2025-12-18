using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Kingmaker.Utility;

public class SirenClient
{
	private class TicketClient : ITicketClient
	{
		public async Task<FindTicketsResponse> FindTickets(FindTicketsRequest request)
		{
			using WebClientWithTimeout client = new WebClientWithTimeout(1200000);
			client.Headers[HttpRequestHeader.ContentType] = "application/json";
			ServicePointManager.ServerCertificateValidationCallback = (object _, X509Certificate _, X509Chain _, SslPolicyErrors _) => true;
			string data = JsonConvert.SerializeObject(request);
			string address = "http://siren.owlcat.local/api/tickets";
			return JsonConvert.DeserializeObject<FindTicketsResponse>(await client.UploadStringTaskAsync(address, data));
		}
	}

	private const string InternalApiAddress = "http://siren.owlcat.local";

	public readonly ITicketClient Ticket = new TicketClient();
}
