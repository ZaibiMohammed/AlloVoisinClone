using AlloVoisinClone.Domain.Common;
using AlloVoisinClone.Domain.Entities;

namespace AlloVoisinClone.Domain.Events
{
    /// <summary>
    /// Event raised when a user is created
    /// </summary>
    public class UserCreatedEvent : DomainEvent
    {
        public User User { get; }
        
        public UserCreatedEvent(User user)
        {
            User = user;
        }
    }
}
