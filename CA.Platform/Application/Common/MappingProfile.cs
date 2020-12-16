using System;
using System.Linq;
using System.Reflection;
using AutoMapper;
using CA.Platform.Application.Interfaces;

namespace CA.Platform.Application.Common
{
    public class MappingProfile : Profile
    {
        internal static Assembly[] Assemblies = new Assembly[0];
        
        public MappingProfile()
        {
            foreach (var assembly in Assemblies)
            {
                ApplyMappingsFromAssembly(assembly);
            }
        }

        private void ApplyMappingsFromAssembly(Assembly assembly)
        {
            var types = assembly.GetExportedTypes()
                .Where(t => t.GetInterfaces().Any(i => 
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapFrom<>)))
                .ToList();

            foreach (var type in types)
            {
                if (type.IsAbstract)
                    continue;
                
                var instance = Activator.CreateInstance(type);

                var methodInfo = type.GetMethod("Mapping") 
                                 ?? type.GetInterface("IMapFrom`1").GetMethod("Mapping");
                
                methodInfo?.Invoke(instance, new object[] { this });

            }
        }
    }
}