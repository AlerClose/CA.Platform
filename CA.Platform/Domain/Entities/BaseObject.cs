using System;
using CA.Platform.Entities.Interfaces;

namespace CA.Platform.Entities
{
    public abstract class BaseObject: IBaseObject
    {
        public Guid? LastModifiedBy { get; set; }
        
        public DateTime? LastModified { get; set; }
        
        public DateTime Created { get; set; }
        
        public Guid CreatedBy { get; set; }
        
        public Guid Id { get; set; }

        public void Delete()
        {
            IsDeleted = true;
        }
        
        public bool IsDeleted { get; set; }

        public virtual string GetDisplayName()
        {
            if (this is ITitle title)
            {
                return title.Title;
            }

            if (GetStringField("Title", out string titleValue))
                return titleValue;
            
            if (GetStringField("Name", out string nameValue))
                return nameValue;

            return ToString();
        }

        private bool GetStringField(string fieldName, out string value)
        {
            value = null;
            
            var propertyInfo = GetType().GetProperty(fieldName);
            if (propertyInfo != null)
            {
                value = propertyInfo.GetValue(this)?.ToString();
                return true;
            }

            return false;
        }
    }
}