using Newtonsoft.Json;

using couchclient;
using couchclient.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace Org.Quickstart.IntegrationTests.ControllerTests
{
    public class ProfileTests
        : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;
		private readonly string baseHostname = "/api/v1/UserProfile";
		private readonly string baseHostnameSearch = "/api/v1/UserProfile";

        public ProfileTests(CustomWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task InsertProfileTestAsync()
        {
	        //create user
            var userProfile = GetNewProfile();
            var newUser = JsonConvert.SerializeObject(userProfile);
            var content = new StringContent(newUser, Encoding.UTF8, "application/json");
	        var response = await _client.PostAsync(baseHostname, content);

	        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
	        var jsonResults = await response.Content.ReadAsStringAsync();
	        var newUserResult = JsonConvert.DeserializeObject<UserProfile>(jsonResults);
            
	        //validate creation 
	        Assert.Equal(userProfile.FirstName, newUserResult.FirstName);  
	        Assert.Equal(userProfile.LastName, newUserResult.LastName);  
	        Assert.Equal(userProfile.PreferredUsername, newUserResult.PreferredUsername);  

	        //remove user
	        var deleteResponse = await _client.DeleteAsync($"{baseHostname}/{newUserResult.Pid}"); 
	        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task UpdateProfileTestAsync()
        {
            //create user
            var userProfile = GetNewProfile();
            var newUser = JsonConvert.SerializeObject(userProfile);
            var content = new StringContent(newUser, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(baseHostname, content);

            var jsonResults = await response.Content.ReadAsStringAsync();
            var newUserResult = JsonConvert.DeserializeObject<UserProfile>(jsonResults);
            
	        //update user
	        UpdateProfile(newUserResult);            
            
	        var updateUserJson = JsonConvert.SerializeObject(newUserResult);
	        var updateContent =  new StringContent(updateUserJson, Encoding.UTF8, "application/json");
	        var updateResponse = await _client.PutAsync($"{baseHostname}/", updateContent);
            
	        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
	        var jsonResult = await updateResponse.Content.ReadAsStringAsync();    
            var updateUserResult = JsonConvert.DeserializeObject<UserProfile>(updateUserJson);

	        //validate update worked
	        Assert.Equal(newUserResult.PreferredUsername, updateUserResult.PreferredUsername);
	        Assert.Equal(newUserResult.FirstName, updateUserResult.FirstName);
	        Assert.Equal(newUserResult.LastName, updateUserResult.LastName);

			//remove user
			var deleteResponse = await _client.DeleteAsync($"{baseHostname}/{updateUserResult.Pid}");
			Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

		}

        [Fact]
        public async Task GetProfileTestAsync()
        {
			//create user
			var userProfile = GetNewProfile();
			var newUser = JsonConvert.SerializeObject(userProfile);
			var content = new StringContent(newUser, Encoding.UTF8, "application/json");
			var response = await _client.PostAsync(baseHostname, content);

			Assert.Equal(HttpStatusCode.Created, response.StatusCode);
			var jsonResults = await response.Content.ReadAsStringAsync();
			var newUserResult = JsonConvert.DeserializeObject<UserProfile>(jsonResults);
			
			//get the user from the main API
			var getResponse = await _client.GetAsync($"{baseHostname}/{newUserResult.Pid}"); 
			Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
			var getJsonResult = await getResponse.Content.ReadAsStringAsync();
			var getUserResult = JsonConvert.DeserializeObject<UserProfile>(getJsonResult);

			//validate it got the same user
			Assert.Equal(newUserResult.PreferredUsername, getUserResult.PreferredUsername);
	        Assert.Equal(newUserResult.FirstName, getUserResult.FirstName);
	        Assert.Equal(newUserResult.LastName, getUserResult.LastName);

			//remove user
			var deleteResponse = await _client.DeleteAsync($"{baseHostname}/{newUserResult.Pid}");
			Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
		}

		[Fact]
		public async Task GetProfileSearchTestAsync()
		{
			//create user
			var userProfile = GetNewProfile();
			var newUser = JsonConvert.SerializeObject(userProfile);
			var content = new StringContent(newUser, Encoding.UTF8, "application/json");
			var response = await _client.PostAsync(baseHostname, content);

			Assert.Equal(HttpStatusCode.Created, response.StatusCode);
			var jsonResults = await response.Content.ReadAsStringAsync();
			var newUserResult = JsonConvert.DeserializeObject<UserProfile>(jsonResults);

			//get the user from the main API
			var getResponse = await _client.GetAsync($"{baseHostnameSearch}?Search={userProfile.FirstName}&Skip=0&Limit=5");
			Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
			var getJsonResult = await getResponse.Content.ReadAsStringAsync();
			var getUserResult = JsonConvert.DeserializeObject<List<UserProfile>>(getJsonResult);

			//validate it got the same user
			Assert.Equal(newUserResult.PreferredUsername, getUserResult[0].PreferredUsername);
			Assert.Equal(newUserResult.FirstName, getUserResult[0].FirstName);
			Assert.Equal(newUserResult.LastName, getUserResult[0].LastName);

			//remove user
			var deleteResponse = await _client.DeleteAsync($"{baseHostname}/{newUserResult.Pid}");
			Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
		}

		private NewUserProfile GetNewProfile()
	    {
	        return new UserProfileCreateRequestCommand(){
		        FirstName = "John",
		        LastName = "Doe",
		        PreferredUsername = "john.doe@couchbase.com",
		        Password = "password"
		    }.GetProfile(); 
	    }

	    private void UpdateProfile (UserProfile profile)
	    {
	        profile.FirstName = "Jane";
	        profile.LastName = "Smith";
	        profile.PreferredUsername = "Jane.Smith@couchbase.com";
	        profile.Password = "password1";
	    }
    }
}
