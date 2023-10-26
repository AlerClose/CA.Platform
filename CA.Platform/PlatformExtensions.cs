using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using AutoMapper;
using CA.Platform.Application.Behaviors;
using CA.Platform.Application.Common;
using CA.Platform.Application.Interfaces;
using CA.Platform.Infrastructure.DataBase;
using CA.Platform.Infrastructure.Interfaces;
using CA.Platform.Infrastructure.Services;
using CA.Platform.Infrastructure.Settings;
using CA.Platform.Infrastructure.UserContext;
using CA.Platform.WebApp;
using CA.WebPlatform;
using MediatR;
using MediatR.Pipeline;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

[assembly:InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace CA.Platform
{
    public static class PlatformExtensions
    {
        internal static readonly List<IDbEntityModelExtender> ModelExtenders = new();
        
        private static readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
        
        public static void RegisterApplication(this IServiceCollection services, params Assembly [] assemblies)
        {
            var assemblyList = assemblies.ToList();
            assemblyList.Add(Assembly.GetExecutingAssembly());
            
            services.AddMediatR(cfg=>
            {
                cfg.RegisterServicesFromAssemblies(assemblyList.ToArray());
            });

            services.AddAutoMapper(assemblyList);
        }
        
        internal static void AddApplication(this IServiceCollection services, string applicationKey)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerfomanceLoggerBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RoleValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));
            services.AddTransient(typeof(IRequestPreProcessor<>), typeof(RequestLoggerBehavior<>));

            services.AddScoped(a => new ApplicationProvider(applicationKey));
        }

        private static void AddInfrastructure<TContext>(this IServiceCollection services) where TContext: BaseDbContext
        {
            services.AddScoped<IUserContext, WebUserContext>();

            services.AddScoped<IEntitySaveHandler, DefaultPropsHandler>();

            services.AddScoped<IEntitySaveHandler, AuditHandler>();

            services.AddScoped<IDbContext, DataContextWrapper<TContext>>();

            services.AddScoped<IAuditService, DbAuditService<TContext>>();

            services.AddScoped<IEntityService, DbEntityService<TContext>>();
            
            services.AddScoped<IStringHashService, StringHashService>();
            
            services.AddScoped<ITokenService, JwtTokenService>();

            services.AddScoped<StringConvertService<TContext>>();
            
            services.AddHttpContextAccessor();
        }

        public static void AddDbModelExtender<T>(T modelExtender) where T : IDbEntityModelExtender
        {
            if (ModelExtenders.Contains(modelExtender) || modelExtender == null) return;
            
            if (ModelExtenders.Any(a => a.GetType() == modelExtender.GetType())) return;

            ModelExtenders.Add(modelExtender);
        }
        
        public static void AddPlatform<TContext>(this IServiceCollection services, string applicationKey, IConfiguration configuration) where TContext: BaseDbContext
        {
            services.AddApplication(applicationKey);
            
            services.AddInfrastructure<TContext>();
            
            services.AddCors(builder =>
            {
                builder.AddDefaultPolicy(policyBuilder =>
                {
                    policyBuilder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
            
            services.AddMvc(options => { options.EnableEndpointRouting = false; }).AddApplicationPart(Assembly.GetExecutingAssembly());
            
            services.AddAuthentication(configuration);

            services.AddSwaggerGen(a =>
            {
                a.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = Assembly.GetExecutingAssembly().GetName().Name + " API",
                    Version = "v1"
                });
                a.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Description =
                        "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Type = SecuritySchemeType.ApiKey
                });
                a.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference 
                            { 
                                Type = ReferenceType.SecurityScheme, 
                                Id = "Bearer" 
                            },
                            Scheme = "ouath2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new string[] {}
                    }
                });
            });
        }
        
        
        private static void AddAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var appSettingsSection = configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });
        }

        public static void UsePlatform(this IApplicationBuilder app)
        {
            app.UseRouting(); // используем систему маршрутизации
            
            app.UseCors();
            
            app.UseAuthentication();

            app.UseMvc();

            app.UseMiddleware<CustomExceptionHandlerMiddleware>();
            app.UseSwagger();
            app.UseSwaggerUI(a =>
            {
                a.SwaggerEndpoint("/swagger/v1/swagger.json", Assembly.GetExecutingAssembly().GetName().Name + " API");
            });
        }
    }
}