using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using CA.Platform.Application.Common;
using CA.Platform.Application.Contracts;
using CA.Platform.Application.Interfaces;
using CA.Platform.Exceptions;
using Microsoft.AspNetCore.Http;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CA.Platform.Infrastructure.UserContext
{
    class WebUserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationProvider _applicationProvider;

        public WebUserContext(IHttpContextAccessor httpContextAccessor, ApplicationProvider applicationProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _applicationProvider = applicationProvider;
        }

        public Guid GetCurrentUserId()
        {
            var value = _httpContextAccessor.HttpContext.User?.Claims.SingleOrDefault(a => a.Type == ClaimTypes.Name)?.Value;
            if (value == null)
                return Guid.Empty;
            
            return new Guid(value);
        }

        public List<string> GetPermissions()
        {
            if ( _httpContextAccessor.HttpContext.User == null)
                throw  new NotAthenticatedException();

            var value = _httpContextAccessor.HttpContext.User.Claims.Single(a => a.Type == "Permissions").Value;
            var permissions = JsonSerializer.Deserialize<List<PermissionDto>>(value);
            
            return permissions.Where(t => t.AppKey == _applicationProvider.GetAppKey()).Select(a => a.Key).ToList();
        }

        public UserDto GetCurrentUser()
        {
            var value = _httpContextAccessor.HttpContext.User.Claims.SingleOrDefault(a => a.Type == "User")?.Value;
            return value == null ? null : JsonSerializer.Deserialize<UserDto>(value);
        }
    }
}
