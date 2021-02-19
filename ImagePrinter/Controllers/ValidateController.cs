using System.Net;
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
            var resourceRequest = review.Request;
            
            var podResource = JsonSerializer.Deserialize<V1Pod>(resourceRequest.Object.GetRawText());
            if (podResource.Spec != null)
            {
                foreach (var container in podResource.Spec.Containers)
                {
                    _logger.LogInformation($"Container Image: {container.Image}");
                }
            }

            return new AdmissionResponse
            {
                Allowed = true,
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
}
