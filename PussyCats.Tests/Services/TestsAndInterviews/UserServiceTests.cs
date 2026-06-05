using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Tests_and_Interviews.Dtos;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Services;
using Xunit;

namespace PussyCats.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _httpClient = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost/api/")
            };
            _service = new UserService(_httpClient);
        }

        [Fact]
        public async Task GetAllAsync_WhenSuccessful_ReturnsUsers()
        {
            // Arrange
            var dtos = new List<UserDto>
            {
                new UserDto { Id = 1, Name = "User 1" },
                new UserDto { Id = 2, Name = "User 2" }
            };

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString().EndsWith("/users")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(dtos)
                });

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("User 1", result[0].Name);
        }

        [Fact]
        public async Task GetAllAsync_WhenNotFound_ReturnsEmptyList()
        {
            // Arrange
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                });

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_WhenApiFails_ThrowsHttpRequestException()
        {
            // Arrange
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _service.GetAllAsync());
        }

        [Fact]
        public void DefaultConstructor_InitializesHttpClient()
        {
            // Act
            var service = new UserService();

            // Assert
            var httpField = typeof(UserService).GetField("http", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(httpField);
            Assert.Same(Tests_and_Interviews.Api.ApiClient.Http, httpField.GetValue(service));
        }

        [Fact]
        public void ParametrizedConstructor_WithNull_InitializesWithDefault()
        {
            // Act
            var service = new UserService(null!);

            // Assert
            var httpField = typeof(UserService).GetField("http", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(httpField);
            Assert.Same(Tests_and_Interviews.Api.ApiClient.Http, httpField.GetValue(service));
        }
    }
}
