using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using IoTEdgeDeploymentApi.Model;
using IoTEdgeDeploymentApi.Security;
using IoTEdgeDeploymentEngine;
using IoTEdgeDeploymentEngine.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace IoTEdgeDeploymentApi
{
	/// <summary>
	/// Http controller for layered deployment
	/// </summary>
	public class AutomaticDeployment
	{
		private readonly IIoTEdgeDeploymentBuilder _ioTEdgeDeploymentBuilder;

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="ioTEdgeDeploymentBuilder">IoTEdgeDeploymentBuilder instance per DI</param>
		public AutomaticDeployment(IIoTEdgeDeploymentBuilder ioTEdgeDeploymentBuilder)
		{
			_ioTEdgeDeploymentBuilder = ioTEdgeDeploymentBuilder;
		}

		/// <summary>
		/// Creates a new automatic deployment manifest file.
		/// </summary>
		/// <param name="req">Http request</param>
		/// <returns></returns>
		[FunctionName("AddAutomaticDeployment")]
		[OpenApiOperation(operationId: "AddAutomaticDeployment", tags: new[] { "IoTEdgeAutomaticDeployment" })]
		[OpenApiSecurity("implicit_auth", SecuritySchemeType.OAuth2, Flows = typeof(ImplicitAuthFlow))]
		[OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
		[OpenApiRequestBody(contentType: "application/json", bodyType: typeof(DeploymentFile), Required = true, Description = "Specifies a file name without file extension and content for the deployment manifest")]
		[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
		public async Task<IActionResult> AddAutomaticDeployment(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
		{
			try
			{
				var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
				var data = JsonConvert.DeserializeObject<DeploymentFile>(requestBody);

				await _ioTEdgeDeploymentBuilder.AddDeployment(data?.FileName, data?.FileContent, DeploymentCategory.AutomaticDeployment);

				return new OkObjectResult("Succeeded");
			}
			catch (System.Exception ex)
			{
				return new BadRequestErrorMessageResult(ex.Message);
			}
		}
		
		/// <summary>
		/// Gets the content of an automatic deployment manifest file.
		/// </summary>
		/// <param name="req">Http request</param>
		/// <returns></returns>
		[FunctionName("GetAutomaticDeploymentFileContent")]
		[OpenApiOperation(operationId: "GetAutomaticDeploymentFileContent", tags: new[] { "IoTEdgeAutomaticDeployment" })]
		[OpenApiParameter(name: "fileName", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **fileName** parameter")]		
		[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
		[OpenApiSecurity("implicit_auth", SecuritySchemeType.OAuth2, Flows = typeof(ImplicitAuthFlow))]
		[OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
		public async Task<IActionResult> GetAutomaticDeploymentFileContent(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
		{
			try
			{
				var fileName = req.Query["fileName"];
		
				var content = await _ioTEdgeDeploymentBuilder.GetFileContent(fileName, DeploymentCategory.AutomaticDeployment);
				
				return new OkObjectResult(content);
			}
			catch (System.Exception ex)
			{
				return new BadRequestErrorMessageResult(ex.Message);
			}
		}

        /// <summary>
        /// Applies all automatic deployments - for testing
        /// </summary>
        /// <param name="req">Http request</param>
        /// <returns></returns>
		[FunctionName("ApplyDeployments")]
		[OpenApiOperation(operationId: "ApplyDeployments", tags: new[] { "IoTEdgeAutomaticDeployment" })]
        public async Task<IActionResult> ApplyDeployments(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            await _ioTEdgeDeploymentBuilder.ApplyDeployments();
            return new OkObjectResult("Applied");
        }
	}
}

