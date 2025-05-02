using System;
using System.Collections.Generic;
using System.Linq;
using AlloVoisinClone.Domain.Common;
using AlloVoisinClone.Domain.Enums;
using AlloVoisinClone.Domain.Events;
using AlloVoisinClone.Domain.Exceptions;
using AlloVoisinClone.Domain.ValueObjects;

namespace AlloVoisinClone.Domain.Entities
{
    /// <summary>
    /// Represents a rental transaction between a renter and an item owner
    /// </summary>
    public class Rental : AuditableEntity, IAggregateRoot
    {
        private readonly List<RentalMessage> _messages = new();
        private readonly List<Payment> _payments = new();
        private readonly List<Review> _reviews = new();
        
        private Rental() { } // Required by EF Core
        
        public Rental(Item item, User renter, DateTime startDate, DateTime endDate, string message)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
                
            if (renter == null)
                throw new ArgumentNullException(nameof(renter));
                
            if (item.OwnerId == renter.Id)
                throw new DomainException("Cannot rent your own item");
                
            if (!item.IsAvailable(startDate, endDate))
                throw new DomainException("Item is not available for the selected dates");
                
            ItemId = item.Id;
            Item = item;
            RenterId = renter.Id;
            Renter = renter;
            OwnerId = item.OwnerId;
            Owner = item.Owner;
            StartDate = startDate;
            EndDate = endDate;
            Status = RentalStatus.Pending;
            TotalPrice = item.CalculatePrice(startDate, endDate);
            SecurityDeposit = item.SecurityDeposit;
            
            if (!string.IsNullOrEmpty(message))
            {
                _messages.Add(new RentalMessage(this, renter, message));
            }
            
            AddDomainEvent(new RentalCreatedEvent(this));
        }
        
        public Guid ItemId { get; private set; }
        public Item Item { get; private set; }
        public Guid RenterId { get; private set; }
        public User Renter { get; private set; }
        public Guid OwnerId { get; private set; }
        public User Owner { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public RentalStatus Status { get; private set; }
        public Money TotalPrice { get; private set; }
        public Money SecurityDeposit { get; private set; }
        public DateTime? PickupDate { get; private set; }
        public DateTime? ReturnDate { get; private set; }
        public string CancellationReason { get; private set; }
        public string DeclineReason { get; private set; }
        
        // Navigation properties
        public IReadOnlyCollection<RentalMessage> Messages => _messages.AsReadOnly();
        public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();
        public IReadOnlyCollection<Review> Reviews => _reviews.AsReadOnly();
        
        public void Accept()
        {
            if (Status != RentalStatus.Pending)
                throw new DomainException("Rental can only be accepted when in pending status");
                
            Status = RentalStatus.Accepted;
            AddDomainEvent(new RentalAcceptedEvent(this));
        }
        
        public void Decline(string reason)
        {
            if (Status != RentalStatus.Pending)
                throw new DomainException("Rental can only be declined when in pending status");
                
            Status = RentalStatus.Declined;
            DeclineReason = reason;
            
            AddDomainEvent(new RentalDeclinedEvent(this));
        }
        
        public void Cancel(string reason)
        {
            if (Status != RentalStatus.Pending && Status != RentalStatus.Accepted)
                throw new DomainException("Rental can only be cancelled when in pending or accepted status");
                
            Status = RentalStatus.Cancelled;
            CancellationReason = reason;
            
            AddDomainEvent(new RentalCancelledEvent(this));
        }
        
        public void ConfirmPayment(Payment payment)
        {
            if (Status != RentalStatus.Accepted)
                throw new DomainException("Payment can only be confirmed when rental is in accepted status");
                
            _payments.Add(payment);
            Status = RentalStatus.Paid;
            
            AddDomainEvent(new RentalPaidEvent(this));
        }
        
        public void ConfirmPickup()
        {
            if (Status != RentalStatus.Paid)
                throw new DomainException("Pickup can only be confirmed when rental is in paid status");
                
            Status = RentalStatus.InProgress;
            PickupDate = DateTime.UtcNow;
            
            AddDomainEvent(new RentalPickupConfirmedEvent(this));
        }
        
        public void ConfirmReturn()
        {
            if (Status != RentalStatus.InProgress)
                throw new DomainException("Return can only be confirmed when rental is in progress");
                
            Status = RentalStatus.Completed;
            ReturnDate = DateTime.UtcNow;
            
            AddDomainEvent(new RentalReturnConfirmedEvent(this));
        }
        
        public void AddMessage(User sender, string content)
        {
            if (string.IsNullOrEmpty(content))
                throw new ArgumentNullException(nameof(content));
                
            if (sender == null)
                throw new ArgumentNullException(nameof(sender));
                
            if (sender.Id != RenterId && sender.Id != OwnerId)
                throw new DomainException("Only the renter or owner can add messages to a rental");
                
            var message = new RentalMessage(this, sender, content);
            _messages.Add(message);
            
            AddDomainEvent(new RentalMessageAddedEvent(this, message));
        }
        
        public void AddReview(User reviewer, float rating, string content)
        {
            if (Status != RentalStatus.Completed)
                throw new DomainException("Reviews can only be added when rental is completed");
                
            if (reviewer == null)
                throw new ArgumentNullException(nameof(reviewer));
                
            if (reviewer.Id != RenterId && reviewer.Id != OwnerId)
                throw new DomainException("Only the renter or owner can add reviews to a rental");
                
            // Check if this user already reviewed this rental
            if (_reviews.Any(r => r.ReviewerId == reviewer.Id))
                throw new DomainException("User has already reviewed this rental");
                
            // Create the review
            var review = new Review(this, reviewer, reviewer.Id == RenterId ? Owner : Renter, rating, content);
            _reviews.Add(review);
            
            // Update the reviewee's rating
            if (reviewer.Id == RenterId)
            {
                Item.AddRating(rating);
                Owner.AddRating(rating);
            }
            else
            {
                Renter.AddRating(rating);
            }
            
            AddDomainEvent(new RentalReviewAddedEvent(this, review));
        }
        
        public int GetRentalDays()
        {
            return (int)Math.Ceiling((EndDate - StartDate).TotalDays);
        }
    }
}
