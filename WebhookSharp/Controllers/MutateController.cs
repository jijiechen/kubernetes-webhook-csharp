using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using k8s.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebhookSharp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MutateController: ControllerBase
    {
        
        const string PluginAnnotationKeyDisabled = "csharp.jijiechen.com/disable-pod-mutation";
        const string PluginAnnotationKeyMutation = "csharp.jijiechen.com/pod-mutation";
        private readonly ILogger<MutateController> _logger;

        public MutateController(ILogger<MutateController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public AdmissionReview Post(AdmissionReview reviewRequest)
        {
            var allowedResponse = WrapAsReview(new AdmissionResponse() {Allowed = true}, reviewRequest);
            var isPod = reviewRequest.Request.Kind.Kind == "Pod";
            if (!isPod)
            {
                return allowedResponse;
            }

            var podJson = reviewRequest.Request.Object.GetRawText();
            var podResource = JsonSerializer.Deserialize<V1Pod>(podJson, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
            if (DisabledByAnnotation(podResource.Metadata.Annotations))
            {
                return allowedResponse;
            }
            
            var patch = new
            {
                op = "add",
                path = "/metadata/annotations",
                value = new Dictionary<string, string>()
                {
                    {PluginAnnotationKeyMutation, "value-from-mutation"}
                }
            };
            var patches = new[] {patch};
            var reviewResponse = new AdmissionResponse()
            {
                Allowed = true,
                PatchType = "JSONPatch",
                Patch = Encoding.Default.GetBytes(JsonSerializer.Serialize(patches))
            };
            return WrapAsReview(reviewResponse, reviewRequest);
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
        
        private static bool DisabledByAnnotation(IDictionary<string, string> annotations)
        {
            var trueValues = new[] { "y", "yes", "true", "1" };
            
            return annotations.TryGetValue(PluginAnnotationKeyDisabled, out var annotation) &&
                   trueValues.Contains(annotation.ToLower());
        }
    }
}