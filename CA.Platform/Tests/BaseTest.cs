using System;
using CA.Platform.Application.Contracts;
using CA.Platform.Application.Interfaces;
using CA.Platform.Infrastructure.DataBase;
using CA.Platform.Infrastructure.Interfaces;
using CA.Platform.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace CA.Platform.Tests
{
    public abstract class BaseTest<TContext> where TContext : BaseDbContext
    {
        private readonly UserDto _currentUser = Generator.Get<UserDto>();

        protected ServiceProvider ServiceProvider { get; private set; }

        protected IMediator Mediator => ServiceProvider.GetService<IMediator>();

        protected IDbContext DbContext => ServiceProvider.GetService<IDbContext>();
        
        protected IUserContext UserContext => ServiceProvider.GetService<IUserContext>();

        protected IStringHashService StringHashService => ServiceProvider.GetService<IStringHashService>();

        protected IAuditService AuditService => ServiceProvider.GetService<IAuditService>();
            
        [SetUp]
        public virtual void Setup()
        {
            var services = new ServiceCollection();
            
            SetupInternal(services);

            services.AddApplication("test");
            services.AddLogging();

            AddDataBase(services);
            
            AddUserContext(services);
            
            AddAuditService(services);
            
            AddEntityService(services);
            
            services.AddScoped(typeof(IDbContext), typeof(DataContextWrapper<TContext>));
            services.AddScoped<StringConvertService<TContext>>();
            services.AddScoped<IStringHashService, StringHashService>();

            ServiceProvider = services.BuildServiceProvider();

        }

        protected virtual void SetupInternal(IServiceCollection services)
        {
            
        }
        
        protected virtual void AddDataBase(ServiceCollection services)
        {
            services.AddDbContext<TContext>(builder => builder.UseInMemoryDatabase("testDataBase"));
            
            services.AddScoped<IEntitySaveHandler, DefaultPropsHandler>();

            services.AddScoped<IEntitySaveHandler, AuditHandler>();
        }

        protected virtual void AddAuditService(ServiceCollection services)
        {
            services.AddScoped(builder =>
            {
                var mock = new Mock<IAuditService>();
                mock.Setup(a => a.NeedToWriteAudit(It.IsAny<Type>())).Returns(false);
                return mock.Object;
            });
        }

        protected virtual void AddEntityService(ServiceCollection services)
        {
            services.AddScoped(builder => new Mock<IEntityService>().Object);
        }

        protected virtual void AddUserContext(ServiceCollection services)
        {
            services.AddScoped(a =>
            {
                var mock = new Mock<IUserContext>();
                mock.Setup(serviceProvider => serviceProvider.GetCurrentUser()).Returns(_currentUser);
                mock.Setup(serviceProvider => serviceProvider.GetCurrentUserId()).Returns(_currentUser.Id);
                return mock.Object;
            });
        }

        [TearDown]
        protected virtual void TearDown()
        {
            ServiceProvider.Dispose();
        }
    }
}