using AirlineSendAgent.Dtos;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Net.Http.Headers;

namespace AirlineSendAgent.Client
{
    public class WebhookClient : IWebhookClient
    {
        private IHttpClientFactory _httpClientFactory;

        public WebhookClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task SendWebhookNotification(FlightDetailChangePayloadDto flightDetailChangePayloadDto)
        {
            //serialize this payload
            var serializedPayload = JsonSerializer.Serialize(flightDetailChangePayloadDto);
            
            var httpClient = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, flightDetailChangePayloadDto.WebhookURI);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(serializedPayload);

            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            try 
            {
              using(var response = await httpClient.SendAsync(request)){
                Console.WriteLine("Success");
                response.EnsureSuccessStatusCode();
            }

            } catch (Exception ex) 
            {
                Console.WriteLine($"Unsuccessfull {ex.Message}");
            }
        }
    }
}