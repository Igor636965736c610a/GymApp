﻿using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using FluentEmail.Core;
using GymAppCore.IRepo;
using GymAppCore.Models.Entities;
using GymAppInfrastructure.Context;
using GymAppInfrastructure.IServices;
using GymAppInfrastructure.Options;
using GymAppInfrastructure.Repo;
using GymAppInfrastructure.Requirements;
using GymAppInfrastructure.Services;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Refit;
using FacebookOptions = GymAppInfrastructure.Options.FacebookOptions;

namespace GymAppInfrastructure.Extensions;

public static class ProgramExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IExerciseService, ExerciseService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ISimpleExerciseService, SimpleExerciseService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IChartService, ChartService>();
        services.AddSingleton<IAuthorizationRequirement, SourceRequirement>();
        services.AddSingleton<IAuthorizationHandler, SourceRequirementHandler>();

        return services;
    }
    
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IExerciseRepo, ExerciseRepo>();
        services.AddScoped<ISimpleExerciseRepo, SimpleExerciseRepo>();
        services.AddScoped<IUserRepo, UserRepo>();

        return services;
    }

    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = new JwtSettings();
        configuration.Bind(nameof(jwtSettings), jwtSettings);
        services.AddSingleton(jwtSettings);

        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(x =>
        {
            x.SaveToken = true;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)),
                ValidateIssuer = false,
                ValidateAudience = false,
                RequireExpirationTime = false,
                ValidateLifetime = true
            };
        }).AddFacebook(options => 
        {
           options.AppId = configuration["FacebookOptions:AppId"];
           options.AppSecret = configuration["FacebookOptions:AppSecret"];
           options.Events = new OAuthEvents()
           {
               OnCreatingTicket = async context =>
               {
                   var identity = (ClaimsIdentity)context.Principal.Identity;
                   identity.AddClaim(new Claim("id", Guid.NewGuid().ToString()));
               }
           };
       });

        return services;
    }

    public static IServiceCollection AddAuthorizationSet(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("SSO", policyBuilder =>
            {
                policyBuilder.Requirements.Add(new SourceRequirement());
            });
        });

        return services;
    }

    public static IServiceCollection AddMvcModel(this IServiceCollection services)
    {
        services.AddSwaggerGen(option =>
        {
            option.SwaggerDoc("v1", new OpenApiInfo { Title = "GymApp", Version = "v1" });
            option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            option.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type=ReferenceType.SecurityScheme,
                            Id="Bearer"
                        }
                    },
                    new string[]{}
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddCookies(this IServiceCollection services)
    {
        services.ConfigureApplicationCookie(options =>
        {
            options.Events.OnRedirectToAccessDenied =
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.FromResult<object>(null!);
                };
        });

        return services;
    }

    public static IServiceCollection AddDb(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<GymAppContext>(options => {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });
        
        services.AddIdentity<User, IdentityRole<Guid>>()
            .AddEntityFrameworkStores<GymAppContext>()
            .AddDefaultTokenProviders();

        return services;
    }

    public static IServiceCollection ConfigureRefit(this IServiceCollection services)
    {
        services.AddRefitClient<IJokeApiService>()
            .ConfigureHttpClient(x => x.BaseAddress = new Uri("https://v2.jokeapi.dev/"));
        services.AddRefitClient<IFacebookApiService>()
            .ConfigureHttpClient(x => x.BaseAddress = new Uri("https://graph.facebook.com"));

        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
        
        return services;
    }

    public static IServiceCollection BindOptions(this IServiceCollection services, IConfiguration configuration)
    {
        var emailOptions = new EmailOptions();
        configuration.GetSection(nameof(EmailOptions)).Bind(emailOptions);
        services.AddSingleton(emailOptions);
        var facebookOptions = new FacebookOptions();
        configuration.GetSection(nameof(FacebookOptions)).Bind(facebookOptions);
        services.AddSingleton(facebookOptions);

        return services;
    }
}