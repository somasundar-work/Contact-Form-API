using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactForm.API.Models;

namespace ContactForm.API.Extensions;

public static class ConfigureOptionsExtensions
{
    public static IServiceCollection ConfigureAppOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ProfileOptions>(configuration.GetSection("profile"));
        services.Configure<CorsOptions>(configuration.GetSection("cors"));
        return services;
    }
}
