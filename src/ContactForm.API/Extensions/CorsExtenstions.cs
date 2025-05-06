using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactForm.API.Constants;
using ContactForm.API.Models;

namespace ContactForm.API.Extensions;

public static class CorsExtenstions
{
    public static IServiceCollection ConfigureAppCors(this IServiceCollection services, IConfiguration configuration)
    {
        CorsOptions corsOptions = new();
        configuration.GetSection("cors").Bind(corsOptions);
        services.AddCors(options =>
        {
            options.AddPolicy(
                AppConstant.CorsPolicyName,
                policy =>
                {
                    if (corsOptions != null && corsOptions.Origins.Length > 0)
                    {
                        policy.WithOrigins(corsOptions.Origins);
                    }
                    else
                    {
                        policy.AllowAnyOrigin();
                    }

                    if (corsOptions != null && corsOptions.Methods.Length > 0)
                    {
                        policy.WithMethods(corsOptions.Methods);
                    }
                    else
                    {
                        policy.AllowAnyMethod();
                    }

                    if (corsOptions != null && corsOptions.Headers.Length > 0)
                    {
                        policy.WithHeaders(corsOptions.Headers);
                    }
                    else
                    {
                        policy.AllowAnyHeader();
                    }

                    if (corsOptions != null && corsOptions.ExposedHeaders.Length > 0)
                    {
                        policy.WithExposedHeaders(corsOptions.ExposedHeaders);
                    }

                    if (corsOptions != null && corsOptions.AllowCredentials)
                    {
                        policy.AllowCredentials();
                    }
                    else
                    {
                        policy.DisallowCredentials();
                    }
                }
            );
        });

        return services;
    }
}
