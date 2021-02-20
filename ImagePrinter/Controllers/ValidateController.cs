using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using k8s.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ImagePrinter.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ValidateController : ControllerBase
    {
        private readonly ILogger<ValidateController> _logger;

        public ValidateController(ILogger<ValidateController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public AdmissionReview Post(AdmissionReview reviewRequest)
        {
            var response = Validate(reviewRequest);
            return WrapAsReview(response, reviewRequest);
        }
        
        private AdmissionResponse Validate(AdmissionReview review)
        {
            var podJson = review.Request.Object.GetRawText();
            var podResource = JsonSerializer.Deserialize<V1Pod>(podJson, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.Default
            });

            var allowed = true;
            if (podResource?.Spec != null)
            {
                var usedImages = new List<string>()
                    .Concat(podResource.Spec.Containers.NotEmpty().Select(c => c.Image))
                    .Concat(podResource.Spec.InitContainers.NotEmpty().Select(c => c.Image))
                    .Concat(podResource.Spec.EphemeralContainers.NotEmpty().Select(c => c.Image))
                    .Distinct()
                    .ToList();
                
                usedImages.ForEach(img =>
                {
                    _logger.LogInformation($"Container Image: {img}");
                });
                
                allowed = usedImages.All(img => !img.Contains("gcr.io"));
            }

            return new AdmissionResponse
            {
                Allowed = allowed,
                Status = new Status
                {
                    Code = (int) HttpStatusCode.OK,
                    Message = string.Empty
                }
            };
        }

        private static AdmissionReview WrapAsReview(AdmissionResponse response, AdmissionReview originalRequest)
        {
            response.Uid = originalRequest.Request.Uid;
            return new AdmissionReview
            {
                ApiVersion = originalRequest.ApiVersion,
                Kind = originalRequest.Kind,
                Response = response
            };
        }

    }

    static class NotEmptyEnumerable
    {
        public static IEnumerable<T> NotEmpty<T>(this IEnumerable<T> items)
        {
            return items ?? new T[0];
        }
    }
}
