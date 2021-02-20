using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using k8s.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ImagePrinter.Tests
{
    public class ApiTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public ApiTests(WebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }
            
        [Fact]
        public async Task TestDefaultPage()
        {
            var defaultPage = await _client.GetAsync("/");
            Assert.Equal(HttpStatusCode.NotFound, defaultPage.StatusCode);
        }
        
        [Fact]
        public async Task TestPostAdmissionReview()
        {
            var jsonStream = this.GetType().Assembly.GetManifestResourceStream("ImagePrinter.Tests.admission-review.json");
            using var sr = new StreamReader(jsonStream!);
            var json = await sr.ReadToEndAsync();

            var reviewBody = new StringContent(json, Encoding.Default, "application/json");
            var httpResponse = await _client.PostAsync("/validate", reviewBody);
            
            var responseText = await httpResponse.Content.ReadAsStringAsync();
            var reviewInResponse = JsonSerializer.Deserialize<AdmissionReview>(responseText, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
            
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            Assert.False(reviewInResponse.Response.Allowed);
        }
    }
}
