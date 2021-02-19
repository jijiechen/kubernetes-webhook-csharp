using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
            var reviewResponse = await _client.PostAsync("/validate", reviewBody);
            
            Assert.Equal(HttpStatusCode.OK, reviewResponse.StatusCode);
        }
    }
}
