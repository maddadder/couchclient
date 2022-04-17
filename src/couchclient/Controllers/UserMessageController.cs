using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Couchbase.Core.Exceptions.KeyValue;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.Query;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using couchclient.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace couchclient.Controllers
{
    [ApiController]
    [Route("/api/v1/UserMessage")]
    public class UserMessageController
        : Controller
    {
        private readonly IClusterProvider _clusterProvider;
        private readonly IBucketProvider _bucketProvider;
        private readonly ILogger _logger;

        private readonly CouchbaseConfig _couchbaseConfig;

        public UserMessageController(
            IClusterProvider clusterProvider,
            IBucketProvider bucketProvider,
	        IOptions<CouchbaseConfig> options,
            ILogger<UserMessageController> logger)
        {
	        _clusterProvider = clusterProvider;
	        _bucketProvider = bucketProvider;
            _logger = logger;

	        _couchbaseConfig = options.Value;
        }

        [HttpGet("GetById/{id:Guid}", Name = "UserMessage-GetById")]
        [SwaggerOperation(OperationId = "UserMessage-GetById", Summary = "Get usermessage by Id", Description = "Get a usermessage by Id from the request")]
        [SwaggerResponse(200, "Returns a report")]
        [SwaggerResponse(404, "Report not found")]
        [SwaggerResponse(500, "Returns an internal error")]
        public async Task<ActionResult<UserMessage>> GetById([FromRoute] Guid id)
        {
            try
            {
                var bucket = await _bucketProvider.GetBucketAsync(_couchbaseConfig.BucketName);
		        var scope = bucket.Scope(_couchbaseConfig.ScopeName);
                var collection = scope.Collection(_couchbaseConfig.CollectionName); 
		        var result = await collection.GetAsync(id.ToString());
                return Ok(result.ContentAs<UserMessage>());

            }
	        catch (DocumentNotFoundException)
	        {
		        return NotFound();
		    }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
            }
        }

        [HttpPost]
        [SwaggerOperation(OperationId = "UserMesssage-Post", Summary = "Create a usermessage", Description = "Create a usermessage from the request")]
        [SwaggerResponse(201, "Create a usermessage")]
        [SwaggerResponse(409, "the href of the link already exists")]
        [SwaggerResponse(500, "Returns an internal error")]
        public async Task<IActionResult> Post([FromBody] UserMessageCreateRequestCommand request)
        {
            try
            {
		        if (!string.IsNullOrEmpty(request.To) && !string.IsNullOrEmpty(request.From) && !string.IsNullOrEmpty(request.Body) && !string.IsNullOrEmpty(request.ApiVersion))
		        {
		            var bucket = await _bucketProvider.GetBucketAsync(_couchbaseConfig.BucketName);
		            var collection = bucket.Collection(_couchbaseConfig.CollectionName);
		            var usermessage = request.GetUserMessage();
                    usermessage.Pid = Guid.NewGuid();
                    usermessage.Created = DateTime.UtcNow;
                    usermessage.Modified = DateTime.UtcNow;
		            await collection.InsertAsync(usermessage.Pid.ToString(), usermessage);
                    return Created($"/api/v1/UserMessage/{usermessage.Pid}", usermessage);
		        }
		        else 
		        {
		           return UnprocessableEntity();  
		        }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
            }
        }

        [Route("twilio")]
        [HttpPost]
        [SwaggerOperation(OperationId = "UserMessage-Post-Twilio", Summary = "Create a usermessage", Description = "Create a usermessage from the request")]
        [SwaggerResponse(201, "Create a usermessage")]
        [SwaggerResponse(409, "the href of the link already exists")]
        [SwaggerResponse(500, "Returns an internal error")]
        public async Task<IActionResult> Post([FromForm] UserMessageCreateRequestCommandTwilio request)
        {
            try
            {
		        if (!string.IsNullOrEmpty(request.To) && !string.IsNullOrEmpty(request.From) && !string.IsNullOrEmpty(request.Body) && !string.IsNullOrEmpty(request.ApiVersion))
		        {
		            var bucket = await _bucketProvider.GetBucketAsync(_couchbaseConfig.BucketName);
		            var collection = bucket.Collection(_couchbaseConfig.CollectionName);
		            var usermessage = request.GetUserMessage();
                    usermessage.Pid = Guid.NewGuid();
                    usermessage.Created = DateTime.UtcNow;
                    usermessage.Modified = DateTime.UtcNow;
		            await collection.InsertAsync(usermessage.Pid.ToString(), usermessage);
                    return Ok();
                    //return Created($"/api/v1/usermessage/{usermessage.Pid}", usermessage);
		        }
		        else 
		        {
		           return UnprocessableEntity();  
		        }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
            }
        }

        [HttpPut("Update/{id:Guid}")]
        [SwaggerOperation(OperationId = "UserMessage-Update", Summary = "Update a usermessage", Description = "Update a usermessage from the request")]
        [SwaggerResponse(200, "Update a usermessage")]
        [SwaggerResponse(404, "usermessage not found")]
        [SwaggerResponse(500, "Returns an internal error")]
        public async Task<IActionResult> Update([FromBody] UserMessageUpdateRequestCommand request)
        {
            try
            {
                var bucket = await _bucketProvider.GetBucketAsync(_couchbaseConfig.BucketName);
                var collection = bucket.Collection(_couchbaseConfig.CollectionName);
                var result = await collection.GetAsync(request.Pid.ToString());
                var usermessage = result.ContentAs<UserMessage>();
                var usermessageRequest = request.GetUserMessage();
                usermessageRequest.Created = usermessage.Created;
                usermessageRequest.Modified = DateTime.UtcNow;
                var updateResult = await collection.ReplaceAsync<UserMessage>(request.Pid.ToString(), usermessageRequest);
                return Ok(request);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
            }
        }


        [HttpDelete("Delete/{id:Guid}")]
        [SwaggerOperation(OperationId = "UserMessage-Delete", Summary = "Delete a usermessage", Description = "Delete a usermessage from the request")]
        [SwaggerResponse(200, "Delete a usermessage")]
        [SwaggerResponse(404, "usermessage not found")]
        [SwaggerResponse(500, "Returns an internal error")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            try
            {
		        var bucket = await _bucketProvider.GetBucketAsync(_couchbaseConfig.BucketName);
		        var collection = bucket.Collection(_couchbaseConfig.CollectionName);
		        await collection.RemoveAsync(id.ToString());
                return this.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet]
	    [Route("List")]
        [SwaggerOperation(OperationId = "UserMessage-List", Summary = "Search for usermessages", Description = "Get a list of usermessages from the request")]
        [SwaggerResponse(200, "Returns the list of usermessages")]
        [SwaggerResponse(500, "Returns an internal error")]
        public async Task<ActionResult<List<UserMessage>>> List([FromQuery] UserMessageListRequestQuery request)
        {
            try
            {
                var cluster = await _clusterProvider.GetClusterAsync();
                var query = $"SELECT p.* FROM `{_couchbaseConfig.BucketName}`.`{_couchbaseConfig.ScopeName}`.`{_couchbaseConfig.CollectionName}` p WHERE __T = 'um' AND p.apiVersion == '{request.Search.ToLower()}' ORDER BY p.modified ASC LIMIT {request.Limit} OFFSET {request.Skip}";
                _logger.LogInformation(query);
                var results = await cluster.QueryAsync<UserMessage>(query);
                var items = await results.Rows.ToListAsync<UserMessage>();
                if (items.Count == 0)
                    return NotFound();

                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
            }
        }
    }
}
