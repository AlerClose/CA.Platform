using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CA.Platform.Entities;
using CA.Platform.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace CA.Platform.Infrastructure.DataBase
{
    public abstract class BaseDbContext: DbContext
    {
        protected BaseDbContext(DbContextOptions options) : base(options)
        {
        }

        protected BaseDbContext()
        {
            
        }
        
        public IEnumerable<PropertyInfo> GetDbSetProperties()
        {
            foreach (var property in GetType().GetProperties())
            {
                if (property.PropertyType.IsGenericType &&
                    property.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                    
                    yield return property;
            }
        }
        
        public DbSet<Platform.Entities.Identity.Application> Applications { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<AuditRecord> AuditRecords { get; set; } 
        public DbSet<EntityField> EntityFields { get; set; }
        public DbSet<EntityType> EntityTypes { get; set; }
        public DbSet<User> Users { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<UserRole>()
                .HasKey(t => new {t.RoleId, t.UserId});

            modelBuilder.Entity<User>()
                .HasMany(a => a.Roles)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId);

            modelBuilder.Entity<Role>()
                .HasMany(a => a.Users)
                .WithOne(a => a.Role)
                .HasForeignKey(a => a.RoleId);;

            modelBuilder.Entity<RolePermission>()
                .HasKey(t => new {t.RoleId, t.PermissionId});

            modelBuilder.Entity<Role>()
                .HasMany(a => a.Permissions)
                .WithOne(a => a.Role)
                .HasForeignKey(a => a.RoleId);

            modelBuilder.Entity<Permission>()
                .HasMany(a => a.Roles)
                .WithOne(a => a.Permission)
                .HasForeignKey(a => a.PermissionId);

            SetDefaultDbSetProperties(modelBuilder);

            foreach (var modelExtender in PlatformExtensions.ModelExtenders)
            {
                modelExtender.ExtendEntities(modelBuilder);   
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
        }

        private void SetDefaultDbSetProperties(ModelBuilder modelBuilder)
        {
            foreach (var property in GetType().GetProperties())
            {
                if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                {
                    var entityType = property.PropertyType.GenericTypeArguments.First();
                    var entityTypeBuilder = modelBuilder
                        .Entity(entityType);

                    if (typeof(BaseObject).IsAssignableFrom(entityType))
                        entityTypeBuilder
                            .HasKey(nameof(BaseObject.Id));
                }
            }
        }
    }
}