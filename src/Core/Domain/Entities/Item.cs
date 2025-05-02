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
    /// Represents an item that can be rented
    /// </summary>
    public class Item : AuditableEntity, IAggregateRoot
    {
        private readonly List<ItemImage> _images = new();
        private readonly List<ItemCategory> _categories = new();
        private readonly List<Rental> _rentals = new();
        private readonly List<ItemAvailability> _availabilities = new();
        private readonly List<Review> _reviews = new();
        
        private Item() { } // Required by EF Core
        
        public Item(string title, string description, Money dailyPrice, User owner, Location location)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            DailyPrice = dailyPrice ?? throw new ArgumentNullException(nameof(dailyPrice));
            OwnerId = owner?.Id ?? throw new ArgumentNullException(nameof(owner));
            Owner = owner;
            Location = location ?? throw new ArgumentNullException(nameof(location));
            Status = ItemStatus.Draft;
            
            AddDomainEvent(new ItemCreatedEvent(this));
        }
        
        public string Title { get; private set; }
        public string Description { get; private set; }
        public Money DailyPrice { get; private set; }
        public Money SecurityDeposit { get; private set; }
        public Guid OwnerId { get; private set; }
        public User Owner { get; private set; }
        public ItemStatus Status { get; private set; }
        public Location Location { get; private set; }
        public float AverageRating { get; private set; }
        public int RatingsCount { get; private set; }
        public int ViewCount { get; private set; }
        
        // Navigation properties
        public IReadOnlyCollection<ItemImage> Images => _images.AsReadOnly();
        public IReadOnlyCollection<ItemCategory> Categories => _categories.AsReadOnly();
        public IReadOnlyCollection<Rental> Rentals => _rentals.AsReadOnly();
        public IReadOnlyCollection<ItemAvailability> Availabilities => _availabilities.AsReadOnly();
        public IReadOnlyCollection<Review> Reviews => _reviews.AsReadOnly();
        
        public void Update(string title, string description, Money dailyPrice, Money securityDeposit, Location location)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            DailyPrice = dailyPrice ?? throw new ArgumentNullException(nameof(dailyPrice));
            SecurityDeposit = securityDeposit;
            Location = location ?? throw new ArgumentNullException(nameof(location));
            
            AddDomainEvent(new ItemUpdatedEvent(this));
        }
        
        public void Publish()
        {
            if (Status == ItemStatus.Published)
                return;
                
            if (_images.Count == 0)
                throw new DomainException("Item must have at least one image to be published");
                
            if (_categories.Count == 0)
                throw new DomainException("Item must have at least one category to be published");
                
            if (_availabilities.Count == 0)
                throw new DomainException("Item must have at least one availability period to be published");
                
            Status = ItemStatus.Published;
            AddDomainEvent(new ItemPublishedEvent(this));
        }
        
        public void Unpublish()
        {
            if (Status == ItemStatus.Draft)
                return;
                
            Status = ItemStatus.Draft;
            AddDomainEvent(new ItemUnpublishedEvent(this));
        }
        
        public void AddImage(ItemImage image)
        {
            // Ensure cover image uniqueness
            if (image.IsCoverImage)
            {
                foreach (var existingImage in _images.Where(i => i.IsCoverImage))
                {
                    existingImage.SetAsCoverImage(false);
                }
            }
            
            _images.Add(image);
        }
        
        public void RemoveImage(Guid imageId)
        {
            var image = _images.FirstOrDefault(i => i.Id == imageId);
            
            if (image != null)
            {
                _images.Remove(image);
                
                // If we removed the cover image and have other images, make another one the cover
                if (image.IsCoverImage && _images.Count > 0)
                {
                    _images.First().SetAsCoverImage(true);
                }
            }
        }
        
        public void AddCategory(ItemCategory category)
        {
            if (!_categories.Any(c => c.CategoryId == category.CategoryId))
            {
                _categories.Add(category);
            }
        }
        
        public void RemoveCategory(Guid categoryId)
        {
            var category = _categories.FirstOrDefault(c => c.CategoryId == categoryId);
            
            if (category != null)
            {
                _categories.Remove(category);
            }
        }
        
        public void AddAvailability(ItemAvailability availability)
        {
            // Check for overlapping availabilities
            var overlapping = _availabilities.Any(a => 
                (availability.StartDate <= a.EndDate && availability.EndDate >= a.StartDate));
                
            if (overlapping)
            {
                throw new DomainException("Availability period overlaps with an existing availability period");
            }
            
            _availabilities.Add(availability);
        }
        
        public void RemoveAvailability(Guid availabilityId)
        {
            var availability = _availabilities.FirstOrDefault(a => a.Id == availabilityId);
            
            if (availability != null)
            {
                _availabilities.Remove(availability);
            }
        }
        
        public void IncrementViewCount()
        {
            ViewCount++;
        }
        
        public bool IsAvailable(DateTime startDate, DateTime endDate)
        {
            if (Status != ItemStatus.Published)
                return false;
                
            // Check if there are any availabilities that cover the entire rental period
            bool hasAvailability = _availabilities.Any(a => 
                a.StartDate <= startDate && a.EndDate >= endDate);
                
            if (!hasAvailability)
                return false;
                
            // Check if there are any overlapping rentals
            bool hasOverlappingRental = _rentals.Any(r => 
                r.Status != RentalStatus.Cancelled && 
                r.Status != RentalStatus.Declined &&
                r.StartDate <= endDate && 
                r.EndDate >= startDate);
                
            return !hasOverlappingRental;
        }
        
        public void AddRating(float rating)
        {
            float totalRating = AverageRating * RatingsCount;
            RatingsCount++;
            AverageRating = (totalRating + rating) / RatingsCount;
        }
        
        public Money CalculatePrice(DateTime startDate, DateTime endDate)
        {
            int days = (int)Math.Ceiling((endDate - startDate).TotalDays);
            
            if (days <= 0)
                throw new DomainException("End date must be after start date");
                
            return new Money(DailyPrice.Amount * days, DailyPrice.Currency);
        }
    }
}
