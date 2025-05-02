using System;
using System.Collections.Generic;
using AlloVoisinClone.Domain.Common;
using AlloVoisinClone.Domain.Entities.Identity;
using AlloVoisinClone.Domain.Enums;
using AlloVoisinClone.Domain.Events;
using AlloVoisinClone.Domain.ValueObjects;

namespace AlloVoisinClone.Domain.Entities
{
    /// <summary>
    /// Represents a user in the system
    /// </summary>
    public class User : AuditableEntity, IAggregateRoot
    {
        private readonly List<Item> _items = new();
        private readonly List<Rental> _rentalsAsRenter = new();
        private readonly List<Rental> _rentalsAsOwner = new();
        private readonly List<Review> _reviewsGiven = new();
        private readonly List<Review> _reviewsReceived = new();
        private readonly List<Address> _addresses = new();
        private readonly List<UserVerification> _verifications = new();
        private readonly List<PaymentMethod> _paymentMethods = new();

        private User() { } // Required by EF Core

        public User(string email, string firstName, string lastName, ApplicationUser identityUser)
        {
            Email = email ?? throw new ArgumentNullException(nameof(email));
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            IdentityUserId = identityUser?.Id ?? throw new ArgumentNullException(nameof(identityUser));
            Status = UserStatus.Active;
            
            AddDomainEvent(new UserCreatedEvent(this));
        }

        public string Email { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string PhoneNumber { get; private set; }
        public string Biography { get; private set; }
        public DateTime? DateOfBirth { get; private set; }
        public Gender? Gender { get; private set; }
        public string ProfilePictureUrl { get; private set; }
        public UserStatus Status { get; private set; }
        public string IdentityUserId { get; private set; }
        public ApplicationUser IdentityUser { get; private set; }
        public float AverageRating { get; private set; }
        public int RatingsCount { get; private set; }
        
        // Navigation properties
        public IReadOnlyCollection<Item> Items => _items.AsReadOnly();
        public IReadOnlyCollection<Rental> RentalsAsRenter => _rentalsAsRenter.AsReadOnly();
        public IReadOnlyCollection<Rental> RentalsAsOwner => _rentalsAsOwner.AsReadOnly();
        public IReadOnlyCollection<Review> ReviewsGiven => _reviewsGiven.AsReadOnly();
        public IReadOnlyCollection<Review> ReviewsReceived => _reviewsReceived.AsReadOnly();
        public IReadOnlyCollection<Address> Addresses => _addresses.AsReadOnly();
        public IReadOnlyCollection<UserVerification> Verifications => _verifications.AsReadOnly();
        public IReadOnlyCollection<PaymentMethod> PaymentMethods => _paymentMethods.AsReadOnly();
        
        public string FullName => $"{FirstName} {LastName}";
        
        public void UpdateProfile(string firstName, string lastName, string biography, string phoneNumber, DateTime? dateOfBirth, Gender? gender)
        {
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            Biography = biography;
            PhoneNumber = phoneNumber;
            DateOfBirth = dateOfBirth;
            Gender = gender;
            
            AddDomainEvent(new UserUpdatedEvent(this));
        }
        
        public void UpdateProfilePicture(string profilePictureUrl)
        {
            ProfilePictureUrl = profilePictureUrl;
        }
        
        public void Deactivate()
        {
            if (Status == UserStatus.Deactivated)
                return;
                
            Status = UserStatus.Deactivated;
            AddDomainEvent(new UserDeactivatedEvent(this));
        }
        
        public void Reactivate()
        {
            if (Status == UserStatus.Active)
                return;
                
            Status = UserStatus.Active;
            AddDomainEvent(new UserReactivatedEvent(this));
        }
        
        public void Ban(string reason)
        {
            if (Status == UserStatus.Banned)
                return;
                
            Status = UserStatus.Banned;
            AddDomainEvent(new UserBannedEvent(this, reason));
        }
        
        public void AddAddress(Address address)
        {
            _addresses.Add(address);
        }
        
        public void AddVerification(UserVerification verification)
        {
            _verifications.Add(verification);
            AddDomainEvent(new UserVerifiedEvent(this, verification.Type));
        }
        
        public void AddPaymentMethod(PaymentMethod paymentMethod)
        {
            _paymentMethods.Add(paymentMethod);
        }
        
        public void AddRating(float rating)
        {
            float totalRating = AverageRating * RatingsCount;
            RatingsCount++;
            AverageRating = (totalRating + rating) / RatingsCount;
        }
    }
}
