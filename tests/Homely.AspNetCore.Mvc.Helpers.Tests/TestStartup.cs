﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestWebApplication;
using TestWebApplication.Repositories;

namespace Homely.AspNetCore.Mvc.Helpers.Tests
{
    public class TestStartup : Startup
    {
        public TestStartup(IConfiguration configuration,
                           IHostingEnvironment hostingEnvironment) : base(configuration, hostingEnvironment)
        {
        }

        public override void ConfigureRepositories(IServiceCollection services)
        {
            // Create our fake data.
            var stubbedFakeVehicleRepository = StubbedFakeVehicleRepository.CreateAFakeVehicleRepository();

            // Inject our repo.
            services.AddSingleton<IFakeVehicleRepository>(stubbedFakeVehicleRepository);
        }
    }
}
