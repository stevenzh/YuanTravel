using System;
using System.Data.Linq.Mapping;
using System.Reflection;

namespace Arch.Common.Linq
{
    public static class PropertyInfoExtensions
    {
        public static bool HasAttributeOf(this PropertyInfo property, Type attributeType)
        {
            object[] attributes = property.GetCustomAttributes(attributeType, true);
            return attributes.Length > 0;
        }

        public static bool HasAttributeOf<TAttribute>(this PropertyInfo propertyInfo)
        {
            object[] attributes = propertyInfo.GetCustomAttributes(typeof(TAttribute), true);
            return attributes.Length > 0;
        }

        public static TAttribute GetAttributeOf<TAttribute>(this PropertyInfo propertyInfo)
        {
            object[] attributes = propertyInfo.GetCustomAttributes(typeof(TAttribute), true);
            if (attributes.Length == 0)
            {
                return default(TAttribute);
            }
            return (TAttribute)attributes[0];
        }

        public static bool IsPrimaryKey(this PropertyInfo propertyInfo)
        {
            var columnAttribute = propertyInfo.GetAttributeOf<ColumnAttribute>();
            if (columnAttribute == null) return false;
            return columnAttribute.IsPrimaryKey;
        }

        public static bool IsForeignKey(this PropertyInfo propertyInfo)
        {
            var association = propertyInfo.GetAttributeOf<AssociationAttribute>();
            if(association == null) return false;
            return association.IsForeignKey;
        }

        public static string ForeignKeyIdField(this PropertyInfo propertyInfo)
        {
            var association = propertyInfo.GetAttributeOf<AssociationAttribute>();
            if(association == null) return null;
            return association.ThisKey;
        }

        public static bool IsEntity(this PropertyInfo propertyInfo)
        {
            return typeof (IEntity).IsAssignableFrom(propertyInfo.PropertyType);
        }

		public static string HtmlName(this PropertyInfo propertyInfo)
		{
			return propertyInfo.IsEntity() ? propertyInfo.Name + ".Id" : propertyInfo.Name;
		}

 
        public static bool AllowNull(this PropertyInfo propertyInfo)
        {
            // nasty hack because Castle DynamicProxy2 doesn't propogate custom attributes of virtual
            // properties to the proxy.
            if (propertyInfo.DeclaringType.IsProxy())
            {
                // get the same property from the type that is being proxied
                propertyInfo = propertyInfo.DeclaringType.BaseType.GetProperty(propertyInfo.Name);
            }

            return propertyInfo.IsDefined(typeof(NullableEntityAttribute), true);
        }
    }



    public interface IEntity
    {
        int Id { get; set; }
        bool IsNew { get; }
    }


    /// <summary>
    /// Add this attribute to any entity property that can be null
    /// This is used by the validating binder to decide whether to raise an exception
    /// when an attempt is made to bind a child entity with Id == 0
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NullableEntityAttribute : System.Attribute
    {
    }
}