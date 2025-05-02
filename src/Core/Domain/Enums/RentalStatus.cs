namespace AlloVoisinClone.Domain.Enums
{
    /// <summary>
    /// Represents the status of a rental
    /// </summary>
    public enum RentalStatus
    {
        /// <summary>
        /// Rental request has been created and is awaiting approval from the owner
        /// </summary>
        Pending = 1,
        
        /// <summary>
        /// Rental request has been accepted by the owner but not yet paid by the renter
        /// </summary>
        Accepted = 2,
        
        /// <summary>
        /// Rental has been declined by the owner
        /// </summary>
        Declined = 3,
        
        /// <summary>
        /// Rental has been cancelled by the renter or the owner
        /// </summary>
        Cancelled = 4,
        
        /// <summary>
        /// Rental has been paid by the renter
        /// </summary>
        Paid = 5,
        
        /// <summary>
        /// Item has been picked up and rental is in progress
        /// </summary>
        InProgress = 6,
        
        /// <summary>
        /// Item has been returned and rental is completed
        /// </summary>
        Completed = 7,
        
        /// <summary>
        /// There was a dispute during the rental
        /// </summary>
        Disputed = 8,
        
        /// <summary>
        /// Rental was refunded
        /// </summary>
        Refunded = 9
    }
}
