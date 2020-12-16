using System;

namespace CA.Platform.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Class,  
            AllowMultiple = true)  // multiuse attribute  
    ]  
    public class AccessAttribute: Attribute
    {
        public string PermissionKey { get; }
        
        public AccessAttribute(string permissionKey)
        {
            PermissionKey = permissionKey;
        }
    }
}