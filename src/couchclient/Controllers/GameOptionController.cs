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
    [Route("/api/v1/GameOption")]
    public class GameOptionController
        : Controller
    {
        private readonly IClusterProvider _clusterProvider;
        private readonly IBucketProvider _bucketProvider;
        private readonly ILogger _logger;

        private readonly CouchbaseConfig _couchbaseConfig;

        public GameOptionController(
            IClusterProvider clusterProvider,
            IBucketProvider bucketProvider,
	        IOptions<CouchbaseConfig> options,
            ILogger<GameOptionController> logger)
        {
	        _clusterProvider = clusterProvider;
	        _bucketProvider = bucketProvider;
            _logger = logger;

	        _couchbaseConfig = options.Value;
        }

        [HttpGet("GetById/{id:Guid}", Name = "GameOption-GetById")]
        [SwaggerOperation(OperationId = "GameOption-GetById", Summary = "Get gameOption by Id", Description = "Get a gameOption by Id from the request")]
        [SwaggerResponse(200, "Returns a report")]
        [SwaggerResponse(404, "Report not found")]
        [SwaggerResponse(500, "Returns an internal error")]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<GameOption>> GetById([FromRoute] Guid id)
        {
            try
            {
                var bucket = await _bucketProvider.GetBucketAsync(_couchbaseConfig.BucketName);
		        var scope = bucket.Scope(_couchbaseConfig.ScopeName);
                var collection = scope.Collection(_couchbaseConfig.CollectionName); 
		        var result = await collection.GetAsync(id.ToString());
                return Ok(result.ContentAs<GameOption>());

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
        [SwaggerOperation(OperationId = "GameOption-Post", Summary = "Create a gameOption", Description = "Create a gameOption from the request")]
        [SwaggerResponse(201, "Create a gameOption")]
        [SwaggerResponse(409, "the href of the link already exists")]
        [SwaggerResponse(500, "Returns an internal error")]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Post([FromBody] GameOptionCreateRequestCommand request)
        {
            try
            {
		        if (request.GameEntryRef != Guid.Empty && !string.IsNullOrEmpty(request.description) && !string.IsNullOrEmpty(request.next))
		        {
		            var bucket = await _bucketProvider.GetBucketAsync(_couchbaseConfig.BucketName);
		            var collection = bucket.Collection(_couchbaseConfig.CollectionName);
		            var gameOption = request.GetGameOption();
                    gameOption.Pid = Guid.NewGuid();
                    gameOption.Created = DateTime.UtcNow;
                    gameOption.Modified = DateTime.UtcNow;
		            await collection.InsertAsync(gameOption.Pid.ToString(), gameOption);
                    return Created($"/api/v1/GameOption/{gameOption.Pid}", gameOption);
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
        [SwaggerOperation(OperationId = "GameOption-Update", Summary = "Update a gameOption", Description = "Update a gameOption from the request")]
        [SwaggerResponse(200, "Update a gameOption")]
        [SwaggerResponse(404, "gameOption not found")]
        [SwaggerResponse(500, "Returns an internal error")]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Update([FromBody] GameOptionUpdateRequestCommand request)
        {
            try
            {
                var bucket = await _bucketProvider.GetBucketAsync(_couchbaseConfig.BucketName);
                var collection = bucket.Collection(_couchbaseConfig.CollectionName);
                var result = await collection.GetAsync(request.Pid.ToString());
                var gameOption = result.ContentAs<GameOption>();
                var usermessageRequest = request.GetGameOption();
                usermessageRequest.Created = gameOption.Created;
                usermessageRequest.Modified = DateTime.UtcNow;
                var updateResult = await collection.ReplaceAsync<GameOption>(request.Pid.ToString(), usermessageRequest);
                return Ok(request);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
            }
        }


        [HttpDelete("Delete/{id:Guid}")]
        [SwaggerOperation(OperationId = "GameOption-Delete", Summary = "Delete a gameOption", Description = "Delete a gameOption from the request")]
        [SwaggerResponse(200, "Delete a gameOption")]
        [SwaggerResponse(404, "gameOption not found")]
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
        [SwaggerOperation(OperationId = "GameOption-List", Summary = "Search for gameOptions", Description = "Get a list of gameOptions from the request")]
        [SwaggerResponse(200, "Returns the list of gameOptions")]
        [SwaggerResponse(500, "Returns an internal error")]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<List<GameOption>>> List([FromQuery] GameOptionListRequestQuery request)
        {
            try
            {
                var cluster = await _clusterProvider.GetClusterAsync();
                var query = $"SELECT p.* FROM `{_couchbaseConfig.BucketName}`.`{_couchbaseConfig.ScopeName}`.`{_couchbaseConfig.CollectionName}` p WHERE __T = 'go' ORDER BY p.modified ASC LIMIT {request.Limit} OFFSET {request.Skip}";
                _logger.LogInformation(query);
                var results = await cluster.QueryAsync<GameOption>(query);
                var items = await results.Rows.ToListAsync<GameOption>();
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
