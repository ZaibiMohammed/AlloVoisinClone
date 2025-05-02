namespace AlloVoisinClone.Domain.Enums
{
    /// <summary>
    /// Represents the status of an item
    /// </summary>
    public enum ItemStatus
    {
        /// <summary>
        /// Item is in draft mode, not visible to others
        /// </summary>
        Draft = 1,
        
        /// <summary>
        /// Item is published and visible to others
        /// </summary>
        Published = 2,
        
        /// <summary>
        /// Item has been reported and is under review
        /// </summary>
        UnderReview = 3,
        
        /// <summary>
        /// Item has been blocked by an administrator
        /// </summary>
        Blocked = 4
    }
}
