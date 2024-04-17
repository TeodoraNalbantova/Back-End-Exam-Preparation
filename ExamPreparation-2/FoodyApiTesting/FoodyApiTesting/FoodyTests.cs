using FoodyApiTesting.DTO;
using RestSharp;
using RestSharp.Authenticators;
using System.Linq.Expressions;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace FoodyApiTesting
{
    public class FoodyTests
    {


        private RestClient client;
        private static string foodId;

        [OneTimeSetUp]
        public void Setup()

        {
            //Get Auth
            string accessToken = GetAccessToken("Ted", "123456");

            //Тук се създава нов обект от тип RestClientOptions, който съдържа настройките за RestClient.
            var restOptions = new RestClientOptions("http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:86")
            {
                //Тук се задава Authenticator за клиента, който в този случай е JwtAuthenticator, който се използва за автентикация с JWT токен. Подаденият токен е този, който сме получили от GetAccessToken()
                Authenticator = new JwtAuthenticator(accessToken),
            };

            //Тук се създава нов клиент за изпращане на заявки към API, като се използват зададените RestClientOptions.
            this.client = new RestClient(restOptions);

        }

        private string GetAccessToken(string username, string password)
        {
            //Създава се нов клиент (authClient) за изпращане на заявки към API за аутентикация.
            
            var authClient = new RestClient("http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:86");
           
            //Създава се нова заявка за аутентикация(authRequest) с HTTP метод POST, която е насочена към пътя / api / User / Authentication.
            var authRequest = new RestRequest("/api/User/Authentication", Method.Post);
            authRequest.AddJsonBody(new
            {
                //Предоставените потребителско име и парола се включват като JSON тяло на заявката.

                userName = username,
                password = password
            });
            //Изпраща се заявката за аутентикация чрез метода Execute() на клиента.
            var response = authClient.Execute(authRequest);
            if (response.IsSuccessStatusCode)
            {
                //Ако отговорът е успешен, тогава се извлича токенът за достъп от отговорното съдържание (response.Content) и се връща.
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var accessToken = content.GetProperty("accessToken").GetString();
                return accessToken;
            }
            else
            {
                throw new InvalidOperationException("Authentication failed");
            }
        }

        [Order(1)]
        [Test]

        public void CreateNewFood_WithRequiredFields_ShouldSuccessed()
        {
            //Arrange
            var newFood = new FoodDto
            {
                Name = "New Test Food",
                Description = "Description",
                Url = "",

            };

            var request = new RestRequest("/api/Food/Create", Method.Post);

            request.AddJsonBody(newFood);
            //Act
            var response = this.client.Execute(request);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var data = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            foodId = data.FoodId;

        }

        [Order(2)]
        [Test]

        public void EditFood_WithNewTitle_ShouldSuccessed()
        {
            // Arrange
            var request = new RestRequest($"/api/Food/Edit/{foodId}", Method.Patch);

            request.AddJsonBody(new[]
            {
                new
                {
                    path = "/name",
                    op = "replace",
                    value = "New Food Title",
                },
            });

            // Act
            var response = this.client.Execute(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(content.Message, Is.EqualTo("Successfully edited"));

        }

        [Order(3)]
        [Test]
        public void GetAllFood_ShouldReturnAnArrayOfItems()
        {
            //Arrange
            var request = new RestRequest("/api/Food/All", Method.Get);

            //Act
            var response = client.Execute(request);
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = JsonSerializer.Deserialize<List<ApiResponseDto>>(response.Content);
            Assert.IsNotEmpty(content);
        }


        [Order(4)]
        [Test]
        public void DeleteFood_WithCorrectId_ShouldBeSuccessful()
        {
            //Arrange
            var reuest = new RestRequest($"/api/Food/Delete/{foodId}", Method.Delete);
            //Act
            var response = client.Execute(reuest);
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);
            Assert.That(content.Message, Is.EqualTo("Deleted successfully!"));

        }

        [Order(5)]
        [Test]

        public void CreateNewFood_WithIncorectData_ShouldFail()
        {
            //Arrange
            

            var request = new RestRequest("/api/Food/Create", Method.Post);

            request.AddJsonBody(new { });

            //Act
            var response = this.client.Execute(request);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));


        }

        [Order(6)]
        [Test]

        public void EditFood_WithNotExistingId_ShouldFail()
        {
            // Arrange
            var request = new RestRequest($"/api/Food/Edit/xxxxxxxx", Method.Patch);

            request.AddJsonBody(new[]
            {
                new
                {
                    path = "/name",
                    op = "replace",
                    value = "New Food Title",
                },
            });

            // Act
            var response = this.client.Execute(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            var content = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(content.Message, Is.EqualTo("No food revues..."));

        }

        [Order(7)]
        [Test]
        public void DeleteFood_WithNonExistingId_ShouldFail()
        {
            // Arrange
            var request = new RestRequest("/api/Food/Delete/XASDAXAS", Method.Delete);
            // Act
            var response = this.client.Execute(request);

            // Assert

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var content = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(content.Message, Is.EqualTo("Unable to delete this food revue!"));
        }
     }
}