﻿using Homely.AspNetCore.Mvc.Helpers.Models;
using Homely.AspNetCore.Mvc.Helpers.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Shouldly;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Homely.AspNetCore.Mvc.Helpers.Tests.TestControllerTests
{
    public class ErrorTests : TestSetup
    {
        private static ErrorViewModel DefaultErrorViewModel = CreateErrorViewModel();
        
        [Fact]
        public async Task GivenAGetRequest_Error_ReturnsAnHttp500()
        {
            // Arrange.

            // Act.
            var response = await Client.GetAsync("/test/error");

            // Assert.
            response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
            await response.Content.ShouldLookLike(DefaultErrorViewModel);
        }
        
        [Fact]
        public async Task GivenAGetRequestAndACustomExceptionFunction_Error_ReturnsAnHttp426()
        {
            // Arrange.

            // We want to use our own startup stuff so we can define our error callback.
            var server = new TestServer(new WebHostBuilder().UseStartup<TestStartupWithCustomJsonExceptionPageCustomHandler>());
            var client = server.CreateClient();

            var errors = new
            {
                errors = new List<ApiError>
                {
                    new ApiError
                    {
                        Message = "I'm a little tea pot, short and stout."
                    }
                }
            };

            // Act.
            var response = await client.GetAsync("/test/error");

            // Assert.
            response.StatusCode.ShouldBe(HttpStatusCode.UpgradeRequired);
            await response.Content.ShouldLookLike(errors);

            //// We can't use the normal model<-->model compare because the response contains
            //// a massive stack trace now (by design) and that's too hard to setup. So we'll
            //// do a simple property-by-property check.
            //var apiErrorsJson = await response.Content.ReadAsStringAsync();
            //var apiErrorResult = JsonConvert.DeserializeObject<ApiErrorResult>(apiErrorsJson);
            //apiErrorResult.Errors.ShouldLookLike(errors);
            //apiErrorResult.StackTrace.ShouldBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task GivenAGetRequestAndAIncludeStackTrace_Error_ReturnsAnHttp500()
        {
            // Arrange.

            // We want to use our own startup stuff so we can define our error callback.
            var server = new TestServer(new WebHostBuilder().UseStartup<TestStartupWithCustomJsonExceptionPageIncludeStackTrace>());
            var client = server.CreateClient();

            // Act.
            var response = await client.GetAsync("/test/error");

            // Assert.
            response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);

            // We can't use the normal model<-->model compare because the response contains
            // a massive stack trace now (by design) and that's too hard to setup. So we'll
            // do a simple property-by-property check.
            var apiErrorsJson = await response.Content.ReadAsStringAsync();
            var apiErrorResult = JsonConvert.DeserializeObject<ApiErrorResult>(apiErrorsJson);
            apiErrorResult.Errors.ShouldLookLike(DefaultErrorViewModel.Errors);
            apiErrorResult.StackTrace.ShouldNotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task GivenAnAjaxGetRequestWithCorsEnabled_Error_ReturnsAnHttp500()
        {
            // Arrange.

            // We want to use our own startup stuff so we can define our error callback.
            var server = new TestServer(new WebHostBuilder().UseStartup<TestStartupWithCors>());
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("Origin", "http://foo.example");

            // Act.
            var response = await client.GetAsync("/test/error");

            // Assert.
            response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
            await response.Content.ShouldLookLike(DefaultErrorViewModel);
            var corsResponseValue = string.Join(string.Empty, response.Headers.GetValues("Access-Control-Allow-Origin"));
            corsResponseValue.ShouldBe("*");
        }

        [Fact]
        public async Task GivenAnAjaxRequestWithNoCorsEnabled_Error_ReturnsACorsError()
        {
            // Arrange.
            Client.DefaultRequestHeaders.Add("Origin", "http://foo.example");

            // Act.
            var response = await Client.GetAsync("/test/error");

            // Assert.
            response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
            await response.Content.ShouldLookLike(DefaultErrorViewModel);
            var doesHeaderExist = response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values);
            doesHeaderExist.ShouldBeFalse();
        }

        private static ErrorViewModel CreateErrorViewModel(string errorMessage = null)
        {
            var errors = new List<ApiError>
                         {
                             new ApiError
                             {
                                 Message = errorMessage ?? "Something bad ass happened."
                             }
                         };
            return new ErrorViewModel(errors);
        }
    }
}
