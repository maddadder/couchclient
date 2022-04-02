﻿using System;
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
    [Route("/api/v1/profile")]
    public class ProfileController
        : Controller
    {
        private readonly IClusterProvider _clusterProvider;
        private readonly IBucketProvider _bucketProvider;
        private readonly ILogger _logger;

        private readonly CouchbaseConfig _couchbaseConfig;

        public ProfileController(
            IClusterProvider clusterProvider,
            IBucketProvider bucketProvider,
	        IOptions<CouchbaseConfig> options,
            ILogger<ProfileController> logger)
        {
	        _clusterProvider = clusterProvider;
	        _bucketProvider = bucketProvider;
            _logger = logger;

	        _couchbaseConfig = options.Value;
        }

        [HttpGet("{id:Guid}", Name = "UserProfile-GetById")]
        [SwaggerOperation(OperationId = "UserProfile-GetById", Summary = "Get user profile by Id", Description = "Get a user profile by Id from the request")]
        [SwaggerResponse(200, "Returns a report")]
        [SwaggerResponse(404, "Report not found")]
        [SwaggerResponse(500, "Returns an internal error")]
        public async Task<ActionResult<Profile>> GetById([FromRoute] Guid id)
        {
            try
            {
                var bucket = await _bucketProvider.GetBucketAsync(_couchbaseConfig.BucketName);
		        var scope = bucket.Scope(_couchbaseConfig.ScopeName);
                var collection = scope.Collection(_couchbaseConfig.CollectionName); 
		        var result = await collection.GetAsync(id.ToString());
                return Ok(result.ContentAs<Profile>());

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
        [SwaggerOperation(OperationId = "UserProfile-Post", Summary = "Create a user profile", Description = "Create a user profile from the request")]
        [SwaggerResponse(201, "Create a user profile")]
        [SwaggerResponse(409, "the email of the user already exists")]
        [SwaggerResponse(500, "Returns an internal error")]
        public async Task<IActionResult> Post([FromBody] ProfileCreateRequestCommand request)
        {
            try
            {
		        if (!string.IsNullOrEmpty(request.Email) && !string.IsNullOrEmpty(request.Password))
		        {
		            var bucket = await _bucketProvider.GetBucketAsync(_couchbaseConfig.BucketName);
		            var collection = bucket.Collection(_couchbaseConfig.CollectionName);
		            var profile = request.GetProfile();
                    profile.Pid = Guid.NewGuid();
                    profile.Created = DateTime.UtcNow;
                    profile.Modified = DateTime.UtcNow;
		            await collection.InsertAsync(profile.Pid.ToString(), profile);

                    return Created($"/api/v1/profile/{profile.Pid}", profile);
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
        [SwaggerOperation(OperationId = "UserProfile-Update", Summary = "Update a user profile", Description = "Update a user profile from the request")]
        [SwaggerResponse(200, "Update a user profile")]
        [SwaggerResponse(404, "user profile not found")]
        [SwaggerResponse(500, "Returns an internal error")]
        public async Task<IActionResult> Update([FromBody] ProfileUpdateRequestCommand request)
        {
            try
            {
                var bucket = await _bucketProvider.GetBucketAsync(_couchbaseConfig.BucketName);
                var collection = bucket.Collection(_couchbaseConfig.CollectionName);
                var result = await collection.GetAsync(request.Pid.ToString());
                var profile = result.ContentAs<Profile>();
                var profileRequest = request.GetProfile();
                profileRequest.Created = profile.Created;
                profileRequest.Modified = DateTime.UtcNow;
                var updateResult = await collection.ReplaceAsync<Profile>(request.Pid.ToString(), profileRequest);
                return Ok(request);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
            }
        }


        [HttpDelete("{id:Guid}")]
        [SwaggerOperation(OperationId = "UserProfile-Delete", Summary = "Delete a profile", Description = "Delete a profile from the request")]
        [SwaggerResponse(200, "Delete a profile")]
        [SwaggerResponse(404, "profile not found")]
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
	    [Route("/api/v1/profiles")]
        [SwaggerOperation(OperationId = "UserProfile-List", Summary = "Search for user profiles", Description = "Get a list of user profiles from the request")]
        [SwaggerResponse(200, "Returns the list of user profiles")]
        [SwaggerResponse(500, "Returns an internal error")]
        public async Task<ActionResult<List<Profile>>> List([FromQuery] ProfileListRequestQuery request)
        {
            try
            {
                if(string.IsNullOrEmpty(request.Search)){
                    return BadRequest("Search parameters are empty");
                }
                var cluster = await _clusterProvider.GetClusterAsync();
                var query = $"SELECT p.* FROM `{_couchbaseConfig.BucketName}`.`{_couchbaseConfig.ScopeName}`.`{_couchbaseConfig.CollectionName}` p WHERE __T == 'up' AND lower(p.firstName) LIKE '%{request.Search.ToLower()}%' OR lower(p.lastName) LIKE '%{request.Search.ToLower()}%' ORDER BY p.firstname ASC, p.lastname ASC LIMIT {request.Limit} OFFSET {request.Skip}";
                _logger.LogInformation(query);
                var results = await cluster.QueryAsync<Profile>(query);
                var items = await results.Rows.ToListAsync<Profile>();
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
