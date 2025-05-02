using System;
using System.Collections.Generic;
using System.Linq;

namespace AlloVoisinClone.Domain.Common
{
    /// <summary>
    /// Base class for all value objects
    /// </summary>
    public abstract class ValueObject
    {
        /// <summary>
        /// Gets the equality components of this value object
        /// </summary>
        protected abstract IEnumerable<object> GetEqualityComponents();

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var other = (ValueObject)obj;
            
            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Select(x => x != null ? x.GetHashCode() : 0)
                .Aggregate((x, y) => x ^ y);
        }
        
        public static bool operator ==(ValueObject left, ValueObject right)
        {
            if (left is null && right is null)
                return true;
            
            if (left is null || right is null)
                return false;
            
            return left.Equals(right);
        }
        
        public static bool operator !=(ValueObject left, ValueObject right)
        {
            return !(left == right);
        }
        
        /// <summary>
        /// Creates a copy of this value object with updated properties
        /// </summary>
        public static T Create<T>(T original, Action<T> updater) where T : ValueObject, new()
        {
            var copy = new T();
            
            // Copy all properties from original
            foreach (var property in typeof(T).GetProperties())
            {
                if (property.CanWrite)
                {
                    property.SetValue(copy, property.GetValue(original));
                }
            }
            
            // Apply updates
            updater(copy);
            
            return copy;
        }
    }
}
