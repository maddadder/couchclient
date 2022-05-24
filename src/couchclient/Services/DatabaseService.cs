using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

using Couchbase;
using Couchbase.Core.Exceptions;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.Management.Buckets;
using Couchbase.Management.Collections;
using Couchbase.Query;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using couchclient.Models;
using System;

namespace couchclient.Services
{
    public class DatabaseService
    {
        private readonly IClusterProvider _clusterProvider;
        private readonly IBucketProvider _bucketProvider;
		private readonly ILogger<DatabaseService> _logger;
		private readonly CouchbaseConfig _couchbaseConfig;

        public DatabaseService(
	        IClusterProvider clusterProvider,
	        IBucketProvider bucketProvider,
			IOptions<CouchbaseConfig> options,
			ILogger<DatabaseService> logger)
        {
	        _clusterProvider = clusterProvider;
	        _bucketProvider = bucketProvider;
			_couchbaseConfig = options.Value; 
			_logger = logger;
	    
        }

	    public async Task SetupDatabase()
	    {
			ICluster cluster = null;
			IBucket bucket = null;

			//try to create bucket, if exists will just fail which is fine
			try 
			{
				cluster = await _clusterProvider.GetClusterAsync();	
				if (cluster != null)
				{
					var bucketSettings = new BucketSettings { 
						Name = _couchbaseConfig.BucketName, 
						BucketType = BucketType.Couchbase, 
						RamQuotaMB = 256,
			 
					};
					bucket = await _bucketProvider.GetBucketAsync(_couchbaseConfig.BucketName);
					if(bucket == null)
						await cluster.Buckets.CreateBucketAsync(bucketSettings);
				}
				else 
					throw new System.Exception("Can't create bucket - cluster is null, please check database configuration.");
			}

			catch (BucketExistsException)
			{
				_logger.LogWarning($"Bucket {_couchbaseConfig.BucketName} already exists");
			}

			bucket = await _bucketProvider.GetBucketAsync(_couchbaseConfig.BucketName);
			if (bucket != null) 
			{
				if (!_couchbaseConfig.ScopeName.StartsWith("_"))
				{
					//try to create scope - if fails it's ok we are probably using default
					_logger.LogWarning($"try to create scope {_couchbaseConfig.ScopeName}");
					try
					{
						await bucket.Collections.CreateScopeAsync(_couchbaseConfig.CollectionName);
					}
					catch(ScopeExistsException)
					{
						_logger.LogWarning($"Scope {_couchbaseConfig.ScopeName} already exists, probably default");
					}
					catch (HttpRequestException)
					{
						_logger.LogWarning($"HttpRequestExcecption when creating Scope {_couchbaseConfig.ScopeName}");
					}
				}
				
				//try to create collection - if fails it's ok the collection probably exists
				_logger.LogWarning($"Attempting to create collection with scope:{_couchbaseConfig.ScopeName}, collection:{_couchbaseConfig.CollectionName} in bucket:{_couchbaseConfig.BucketName}.");
				try 
				{
					await bucket.Collections.CreateCollectionAsync(new CollectionSpec(_couchbaseConfig.ScopeName, _couchbaseConfig.CollectionName));
				}
				catch (CollectionExistsException)
				{
					_logger.LogWarning($"Collection {_couchbaseConfig.CollectionName} already exists in {_couchbaseConfig.BucketName}.");
				}
				catch (HttpRequestException)
				{
					_logger.LogWarning($"HttpRequestExcecption when creating collection  {_couchbaseConfig.CollectionName}");
				}
				catch(Exception)
				{
					_logger.LogWarning($"Exception when creating collection  {_couchbaseConfig.CollectionName}");
				}

				//try to create index - if fails it probably already exists
			
				await Task.Delay(5000);
				var queries = new List<string> 
				{ 
					$"CREATE PRIMARY INDEX default_profile_index ON {_couchbaseConfig.BucketName}.{_couchbaseConfig.ScopeName}.{_couchbaseConfig.CollectionName} USING GSI WITH {{\"num_replica\": 0}}",
					$"CREATE Primary INDEX ON {_couchbaseConfig.BucketName} USING GSI WITH {{\"num_replica\": 0}}",
					$"CREATE INDEX adv_T                 ON `{_couchbaseConfig.BucketName}`:`{_couchbaseConfig.BucketName}`.`{_couchbaseConfig.ScopeName}`.`{_couchbaseConfig.CollectionName}`(`__T`) USING GSI WITH {{\"num_replica\": 0}}",
					$"CREATE INDEX adv_lower_firstName_T ON `{_couchbaseConfig.BucketName}`:`{_couchbaseConfig.BucketName}`.`{_couchbaseConfig.ScopeName}`.`{_couchbaseConfig.CollectionName}`(lower(`firstName`)) WHERE `__T` = 'up' USING GSI WITH {{\"num_replica\": 0}}",
					$"CREATE INDEX adv_lower_lastName    ON `{_couchbaseConfig.BucketName}`:`{_couchbaseConfig.BucketName}`.`{_couchbaseConfig.ScopeName}`.`{_couchbaseConfig.CollectionName}`(lower(`lastName`))  WHERE `__T` = 'up' USING GSI WITH {{\"num_replica\": 0}}",
					$"CREATE INDEX adv_apiVersion_T      ON `{_couchbaseConfig.BucketName}`:`{_couchbaseConfig.BucketName}`.`{_couchbaseConfig.ScopeName}`.`{_couchbaseConfig.CollectionName}`(`apiVersion`,`__T`) USING GSI WITH {{\"num_replica\": 0}}",
					$"CREATE INDEX adv_lower_href_T      ON `{_couchbaseConfig.BucketName}`:`{_couchbaseConfig.BucketName}`.`{_couchbaseConfig.ScopeName}`.`{_couchbaseConfig.CollectionName}`(lower(`href`)) WHERE `__T` = 'ul' USING GSI WITH {{\"num_replica\": 0}}",
					$"CREATE INDEX adv_lower_content     ON `{_couchbaseConfig.BucketName}`:`{_couchbaseConfig.BucketName}`.`{_couchbaseConfig.ScopeName}`.`{_couchbaseConfig.CollectionName}`(lower(`content`)) USING GSI WITH {{\"num_replica\": 0}}",
				};
				foreach (var query in queries)
				{
					try
					{
						_logger.LogWarning($"executing query {query}.");
						var result = await cluster.QueryAsync<dynamic>(query);
						_logger.LogWarning($"Created index {query} with status {result.MetaData.Status}");
					}
					catch (IndexExistsException)
					{
						_logger.LogWarning($"Collection {_couchbaseConfig.CollectionName} already exists in {_couchbaseConfig.BucketName}, {query}");
					}
					catch (System.Exception)
					{
						_logger.LogWarning($"failed to run query: {query}");
					}
				}
				
			}
			else 
				throw new System.Exception("Can't retreive bucket.");
	    }
    }
}
