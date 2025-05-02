using System;
using System.Collections.Generic;

namespace AlloVoisinClone.Domain.Common
{
    /// <summary>
    /// Base class for all entities
    /// </summary>
    public abstract class Entity
    {
        private readonly List<DomainEvent> _domainEvents = new();
        
        public Guid Id { get; protected set; }
        
        /// <summary>
        /// Domain events occurred on this entity
        /// </summary>
        public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
        
        protected Entity()
        {
            Id = Guid.NewGuid();
        }
        
        /// <summary>
        /// Adds a domain event
        /// </summary>
        protected void AddDomainEvent(DomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }
        
        /// <summary>
        /// Removes a domain event
        /// </summary>
        public void RemoveDomainEvent(DomainEvent domainEvent)
        {
            _domainEvents.Remove(domainEvent);
        }
        
        /// <summary>
        /// Clears all domain events
        /// </summary>
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
        
        public override bool Equals(object obj)
        {
            if (obj is not Entity other)
                return false;
                
            if (ReferenceEquals(this, other))
                return true;
                
            if (GetType() != other.GetType())
                return false;
                
            if (Id == Guid.Empty || other.Id == Guid.Empty)
                return false;
                
            return Id == other.Id;
        }
        
        public static bool operator ==(Entity a, Entity b)
        {
            if (a is null && b is null)
                return true;
                
            if (a is null || b is null)
                return false;
                
            return a.Equals(b);
        }
        
        public static bool operator !=(Entity a, Entity b)
        {
            return !(a == b);
        }
        
        public override int GetHashCode()
        {
            return (GetType().ToString() + Id).GetHashCode();
        }
    }
}
