namespace AlloVoisinClone.Domain.Enums
{
    /// <summary>
    /// Represents the status of a user
    /// </summary>
    public enum UserStatus
    {
        /// <summary>
        /// User is active and can use the platform
        /// </summary>
        Active = 1,
        
        /// <summary>
        /// User has been deactivated by themselves
        /// </summary>
        Deactivated = 2,
        
        /// <summary>
        /// User has been banned by an administrator
        /// </summary>
        Banned = 3
    }
}
