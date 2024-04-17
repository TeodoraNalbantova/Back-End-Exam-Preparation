using IdeaAPITests.Models;
using NUnit.Framework.Constraints;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace IdeaAPITests
{
    [TestFixture]
    public class IdeaAPITests
    {
        private RestClient client;
        
        private const string BASEURL = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:84";
        private const string EMAIL = "teddy@abv.bg";
        private const string PASSWORD = "123456";
            
        private static string lastIdeaId;

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken(EMAIL,PASSWORD);

            var options = new RestClientOptions(BASEURL)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };
            // клиента е все едно браузера, който прави заявките към АПИто
            client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            //нов клиент само само за аутинтикацията
            RestClient authClient = new RestClient(BASEURL);
            var request = new RestRequest("api/User/Authentication");
            request.AddJsonBody(new
            {
                email, password
            });

            var response = authClient.Execute(request, Method.Post);

            if (response.StatusCode == HttpStatusCode.OK) 
            { 
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();
                
             if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Access is null or empty.");
                    
                }
                return token;
                // визаме си токен, който сме десириализирали
            }
            else
            {
                throw new InvalidOperationException($"Unexpected response type {response.StatusCode} with data {response.Content}");
                // хвърляме ексепшъни, когато трябва
            }

        }

      

        [Test, Order(1)]
        public void CreateIdea_WithRequiredFields_ShouldSucceed()
        {
            //Arrange
            var ideaRequest = new IdeaDto
            {
                Title = "TestTitle",
                Url = "",
                Description = "Test description."

            };

            var request = new RestRequest("/api/Idea/Create");
            request.AddBody(ideaRequest);
            //Act
            var response = client.Execute(request, Method.Post);

            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            Assert.That(responseData.Msg, Is.EqualTo($"Successfully created!"));

        }

        [Test, Order(2)]
        public void GetAllIdeas_ShouldRetunrNotEmptyArray()
        {

            // Arrange
            var request = new RestRequest("/api/Idea/All");

            //Act

            var response = client.Execute(request, Method.Get);

            var responseDataArray = JsonSerializer.Deserialize<ApiResponseDTO[]>(response.Content);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            Assert.That(responseDataArray.Length, Is.GreaterThan(0))

            // Взимаме Id на последнта идея от масива

            lastIdeaId = responseDataArray[responseDataArray.Length - 1].IdeaId;

        }

        [Test, Order(3)]
        public void EditIdea_WithCorrectData_ShouldSuccceed()
        {
            //Arange
            var ideaRequest = new IdeaDto
            {
                Title = "EditedTestTitle",
                Description = "Test description with edits."

            };
            
            var request = new RestRequest("/api/Idea/Edit");

            request.AddQueryParameter("ideaId", lastIdeaId);
            request.AddBody(ideaRequest);
            
            // Act
            var response = client.Execute(request, Method.Put);
            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            Assert.That(responseData.Msg, Is.EqualTo($"Edited successfully"));

        }

        [Test, Order(4)]
        public void DeleteIdea_ShouldSucceed()
        {
            
            // Arrange
            var request = new RestRequest("/api/Idea/Delete");
            request.AddQueryParameter("ideaId", lastIdeaId);
 
            // Act
            var response = client.Execute(request, Method.Delete);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            string expectedResponse = "The idea is deleted!"; // Response is string, not JSON object
            Assert.That(response.Content, Does.Contain(expectedResponse));

        }

        [Test, Order(5)]
        public void CreateIdea_WithWithoutCorrectData_ShouldNotSucceed()
        {
            
            //Arrange
            var ideaRequest = new IdeaDto
            {
                Title = "TestTitle",
                

            };

            var request = new RestRequest("/api/Idea/Create");
            request.AddBody(ideaRequest);
            //Act
            var response = client.Execute(request, Method.Post);

            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        }

        [Test, Order(3)]
        public void EditIdea_WithWrongId_ShouldFail()
        {
           // Arrange
            var ideaRequest = new IdeaDto
            {
                Title = "EditedTestTitle",
                Description = "Test description with edits."

            };

            var request = new RestRequest("/api/Idea/Edit");
            request.AddQueryParameter("ideaId", "112233");
            request.AddBody(ideaRequest);

            //Act
            var response = client.Execute(request, Method.Put);
            

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain($"There is no such idea!"));

        }


        [Test, Order(7)]
        public void DeleteIdea_WithWrongId_ShoudFail()
        {

            //Arrange
            var request = new RestRequest("/api/Idea/Delete");
            request.AddQueryParameter("ideaId", "123456");

            //Act
            var response = client.Execute(request, Method.Delete);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain($"There is no such idea!"));

        }
    }
}