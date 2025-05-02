using System;
using MediatR;

namespace AlloVoisinClone.Domain.Common
{
    /// <summary>
    /// Base class for all domain events
    /// </summary>
    public abstract class DomainEvent : INotification
    {
        /// <summary>
        /// The date and time when this event occurred
        /// </summary>
        public DateTime OccurredOn { get; }
        
        /// <summary>
        /// A unique identifier for this event
        /// </summary>
        public Guid Id { get; }
        
        protected DomainEvent()
        {
            Id = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
        }
    }
}
