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
using Microsoft.AspNetCore.Authorization;

namespace couchclient.Controllers
{
    [ApiController]
    [Route("/api/v1/GameEntry")]
    public class GameEntryController
        : Controller
    {
        private readonly IClusterProvider _clusterProvider;
        private readonly IBucketProvider _bucketProvider;
        private readonly ILogger _logger;

        private readonly CouchbaseConfig _couchbaseConfig;

        public GameEntryController(
            IClusterProvider clusterProvider,
            IBucketProvider bucketProvider,
	        IOptions<CouchbaseConfig> options,
            ILogger<GameEntryController> logger)
        {
	        _clusterProvider = clusterProvider;
	        _bucketProvider = bucketProvider;
            _logger = logger;

	        _couchbaseConfig = options.Value;
        }

        [HttpGet("GetById/{id:Guid}", Name = "GameEntry-GetById")]
        [SwaggerOperation(OperationId = "GameEntry-GetById", Summary = "Get gameEntry by Id", Description = "Get a gameEntry by Id from the request")]
        [SwaggerResponse(200, "Returns a report")]
        [SwaggerResponse(404, "Report not found")]
        [SwaggerResponse(500, "Returns an internal error")]
        public async Task<ActionResult<GameEntry>> GetById([FromRoute] Guid id)
        {
            try
            {
                var bucket = await _bucketProvider.GetBucketAsync(_couchbaseConfig.BucketName);
		        var scope = bucket.Scope(_couchbaseConfig.ScopeName);
                var collection = scope.Collection(_couchbaseConfig.CollectionName); 
		        var result = await collection.GetAsync(id.ToString());
                return Ok(result.ContentAs<GameEntry>());

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
        [SwaggerOperation(OperationId = "GameEntry-Post", Summary = "Create a gameEntry", Description = "Create a gameEntry from the request")]
        [SwaggerResponse(201, "Create a gameEntry")]
        [SwaggerResponse(409, "the href of the link already exists")]
        [SwaggerResponse(500, "Returns an internal error")]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Post([FromBody] GameEntryCreateRequestCommand request)
        {
            try
            {
		        if (!string.IsNullOrEmpty(request.name) && request.description != null && request.options != null)
		        {
		            var bucket = await _bucketProvider.GetBucketAsync(_couchbaseConfig.BucketName);
		            var collection = bucket.Collection(_couchbaseConfig.CollectionName);
		            var gameEntry = request.GetGameEntry();
                    gameEntry.Pid = Guid.NewGuid();
                    gameEntry.Created = DateTime.UtcNow;
                    gameEntry.Modified = DateTime.UtcNow;
		            await collection.InsertAsync(gameEntry.Pid.ToString(), gameEntry);
                    return Created($"/api/v1/GameEntry/{gameEntry.Pid}", gameEntry);
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
        [SwaggerOperation(OperationId = "GameEntry-Update", Summary = "Update a gameEntry", Description = "Update a gameEntry from the request")]
        [SwaggerResponse(200, "Update a gameEntry")]
        [SwaggerResponse(404, "gameEntry not found")]
        [SwaggerResponse(500, "Returns an internal error")]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Update([FromBody] GameEntryUpdateRequestCommand request)
        {
            try
            {
                var bucket = await _bucketProvider.GetBucketAsync(_couchbaseConfig.BucketName);
                var collection = bucket.Collection(_couchbaseConfig.CollectionName);
                var result = await collection.GetAsync(request.Pid.ToString());
                var gameEntry = result.ContentAs<GameEntry>();
                var usermessageRequest = request.GetGameEntry();
                usermessageRequest.Created = gameEntry.Created;
                usermessageRequest.Modified = DateTime.UtcNow;
                var updateResult = await collection.ReplaceAsync<GameEntry>(request.Pid.ToString(), usermessageRequest);
                return Ok(request);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
            }
        }


        [HttpDelete("Delete/{id:Guid}")]
        [SwaggerOperation(OperationId = "GameEntry-Delete", Summary = "Delete a gameEntry", Description = "Delete a gameEntry from the request")]
        [SwaggerResponse(200, "Delete a gameEntry")]
        [SwaggerResponse(404, "gameEntry not found")]
        [SwaggerResponse(500, "Returns an internal error")]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
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
        [SwaggerOperation(OperationId = "GameEntry-List", Summary = "Search for gameEntries", Description = "Get a list of gameEntries from the request")]
        [SwaggerResponse(200, "Returns the list of gameEntries")]
        [SwaggerResponse(500, "Returns an internal error")]
        public async Task<ActionResult<List<GameEntry>>> List([FromQuery] GameEntryListRequestQuery request)
        {
            try
            {
                var cluster = await _clusterProvider.GetClusterAsync();
                var query = $"SELECT p.* FROM `{_couchbaseConfig.BucketName}`.`{_couchbaseConfig.ScopeName}`.`{_couchbaseConfig.CollectionName}` p WHERE __T = 'ge' ORDER BY p.created ASC LIMIT {request.Limit} OFFSET {request.Skip}";
                _logger.LogInformation(query);
                var results = await cluster.QueryAsync<GameEntry>(query);
                var items = await results.Rows.ToListAsync<GameEntry>();
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
