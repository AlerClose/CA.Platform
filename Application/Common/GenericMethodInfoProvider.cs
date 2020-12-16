using System;
using System.Collections.Generic;
using System.Reflection;

namespace CA.Platform.Application.Common
{
    public class GenericMethodInfoProvider<T> where T: class
    {
        private readonly MethodInfo _methodInfo;

        private readonly Dictionary<Type, MethodInfo> _genericMethodsCache;
        
        public GenericMethodInfoProvider(string methodName)
        {
            _methodInfo = typeof(T).GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.IgnoreReturn | BindingFlags.NonPublic);
            
            if (_methodInfo == null)
                throw new NotSupportedException();
            
            _genericMethodsCache = new Dictionary<Type, MethodInfo>();
        }

        public MethodInfo GetGenericMethod(Type type)
        {
            if (!_genericMethodsCache.ContainsKey(type))
                _genericMethodsCache.Add(type, _methodInfo.MakeGenericMethod(type));
            
            return _genericMethodsCache[type];
        }
    }
}