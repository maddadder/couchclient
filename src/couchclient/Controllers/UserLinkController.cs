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
    [Route("/api/v1/userlink")]
    public class UserLinkController
        : Controller
    {
        private readonly IClusterProvider _clusterProvider;
        private readonly IBucketProvider _bucketProvider;
        private readonly ILogger _logger;

        private readonly CouchbaseConfig _couchbaseConfig;

        public UserLinkController(
            IClusterProvider clusterProvider,
            IBucketProvider bucketProvider,
	        IOptions<CouchbaseConfig> options,
            ILogger<UserLinkController> logger)
        {
	        _clusterProvider = clusterProvider;
	        _bucketProvider = bucketProvider;
            _logger = logger;

	        _couchbaseConfig = options.Value;
        }

        [HttpGet("{id:Guid}", Name = "UserLink-GetById")]
        [SwaggerOperation(OperationId = "UserLink-GetById", Summary = "Get userlink by Id", Description = "Get a userlink by Id from the request")]
        [SwaggerResponse(200, "Returns a report")]
        [SwaggerResponse(404, "Report not found")]
        [SwaggerResponse(500, "Returns an internal error")]
        public async Task<ActionResult<UserLink>> GetById([FromRoute] Guid id)
        {
            try
            {
                var bucket = await _bucketProvider.GetBucketAsync(_couchbaseConfig.BucketName);
		        var scope = bucket.Scope(_couchbaseConfig.ScopeName);
                var collection = scope.Collection(_couchbaseConfig.CollectionName); 
		        var result = await collection.GetAsync(id.ToString());
                return Ok(result.ContentAs<UserLink>());

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
        [SwaggerOperation(OperationId = "UserLink-Post", Summary = "Create a userlink", Description = "Create a userlink from the request")]
        [SwaggerResponse(201, "Create a userlink")]
        [SwaggerResponse(409, "the href of the link already exists")]
        [SwaggerResponse(500, "Returns an internal error")]
        public async Task<IActionResult> Post([FromBody] UserLinkCreateRequestCommand request)
        {
            try
            {
		        if (!string.IsNullOrEmpty(request.Href) && !string.IsNullOrEmpty(request.Content))
		        {
		            var bucket = await _bucketProvider.GetBucketAsync(_couchbaseConfig.BucketName);
		            var collection = bucket.Collection(_couchbaseConfig.CollectionName);
		            var userlink = request.GetUserLink();
                    userlink.Pid = Guid.NewGuid();
                    userlink.Created = DateTime.UtcNow;
                    userlink.Modified = DateTime.UtcNow;
		            await collection.InsertAsync(userlink.Pid.ToString(), userlink);

                    return Created($"/api/v1/userlink/{userlink.Pid}", userlink);
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

        [HttpPut]
        [SwaggerOperation(OperationId = "UserLink-Update", Summary = "Update a userlink", Description = "Update a userlink from the request")]
        [SwaggerResponse(200, "Update a userlink")]
        [SwaggerResponse(404, "userlink not found")]
        [SwaggerResponse(500, "Returns an internal error")]
        public async Task<IActionResult> Update([FromBody] UserLinkUpdateRequestCommand request)
        {
            try
            {
                var bucket = await _bucketProvider.GetBucketAsync(_couchbaseConfig.BucketName);
                var collection = bucket.Collection(_couchbaseConfig.CollectionName);
                var result = await collection.GetAsync(request.Pid.ToString());
                var userlink = result.ContentAs<UserLink>();
                var userlinkRequest = request.GetUserLink();
                userlinkRequest.Created = userlink.Created;
                userlinkRequest.Modified = DateTime.UtcNow;
                var updateResult = await collection.ReplaceAsync<UserLink>(request.Pid.ToString(), userlinkRequest);
                return Ok(request);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
            }
        }


        [HttpDelete("{id:Guid}")]
        [SwaggerOperation(OperationId = "UserLink-Delete", Summary = "Delete a userlink", Description = "Delete a userlink from the request")]
        [SwaggerResponse(200, "Delete a userlink")]
        [SwaggerResponse(404, "userlink not found")]
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
	    [Route("/api/v1/userlinks")]
        [SwaggerOperation(OperationId = "UserLink-List", Summary = "Search for userlinks", Description = "Get a list of userlinks from the request")]
        [SwaggerResponse(200, "Returns the list of userlinks")]
        [SwaggerResponse(500, "Returns an internal error")]
        public async Task<ActionResult<List<UserLink>>> List([FromQuery] UserLinkListRequestQuery request)
        {
            try
            {
                var cluster = await _clusterProvider.GetClusterAsync();
                var query = $"SELECT p.* FROM `{_couchbaseConfig.BucketName}`.`{_couchbaseConfig.ScopeName}`.`{_couchbaseConfig.CollectionName}` p WHERE __T = 'ul' AND lower(p.href) LIKE '%{request.Search.ToLower()}%' OR lower(p.content) LIKE '%{request.Search.ToLower()}%' ORDER BY p.content ASC LIMIT {request.Limit} OFFSET {request.Skip}";
                _logger.LogInformation(query);
                var results = await cluster.QueryAsync<UserLink>(query);
                var items = await results.Rows.ToListAsync<UserLink>();
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
