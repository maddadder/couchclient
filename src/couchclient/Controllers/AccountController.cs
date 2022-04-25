using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using couchclient.Models;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace couchclient.Controllers 
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController: Controller {
        private readonly JwtSettings jwtSettings;
        private readonly IClusterProvider _clusterProvider;
        private readonly IBucketProvider _bucketProvider;
        private readonly ILogger _logger;

        private readonly CouchbaseConfig _couchbaseConfig;

        public AccountController(
            IClusterProvider clusterProvider,
            IBucketProvider bucketProvider,
	        IOptions<CouchbaseConfig> options,
            ILogger<UserProfileController> logger,
            JwtSettings jwtSettings)
        {
	        _clusterProvider = clusterProvider;
	        _bucketProvider = bucketProvider;
            _logger = logger;
	        _couchbaseConfig = options.Value;
            this.jwtSettings = jwtSettings;
        }

        private async Task<IEnumerable<UserProfile>> GetAllUsers()
        {
            var cluster = await _clusterProvider.GetClusterAsync();
            var query = $"SELECT p.* FROM `{_couchbaseConfig.BucketName}`.`{_couchbaseConfig.ScopeName}`.`{_couchbaseConfig.CollectionName}` p WHERE __T == 'up'";
            _logger.LogInformation(query);
            var results = await cluster.QueryAsync<UserProfile>(query);
            var items = await results.Rows.ToListAsync<UserProfile>();
            return items;
        }
            
        [HttpPost]
        [SwaggerOperation(OperationId = "Account-Post", Summary = "Creates a UserToken", Description = "Creates a user token given the right credentials")]
        [SwaggerResponse(201, "Creates a user token")]
        [SwaggerResponse(400, "Wrong Password")]
        [SwaggerResponse(500, "Returns an internal error")]
        public async Task<ActionResult<UserToken>> Post([FromBody] UserLogin userLogin) {
            var Token = new UserToken();
            var users = await GetAllUsers();
            var Valid = users.Any(x => x.Email.Equals(userLogin.Email, StringComparison.OrdinalIgnoreCase));
            if (Valid) {
                var user = users.FirstOrDefault(x => x.Email.Equals(userLogin.Email, StringComparison.OrdinalIgnoreCase));
                Token = Extensions.JwtHelpers.GenTokenkey(new UserToken() {
                    Email = user.Email,
                    GuidId = Guid.NewGuid(),
                    Id = user.Pid,
                }, jwtSettings);
            } else 
            {
                return BadRequest($"wrong password");
            }
            return Created($"/api/v1/Account/{Token.GuidId}", Token);
        }
    }
}