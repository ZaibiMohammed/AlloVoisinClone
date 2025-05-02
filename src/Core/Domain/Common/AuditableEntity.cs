using System;

namespace AlloVoisinClone.Domain.Common
{
    /// <summary>
    /// Base class for all auditable entities
    /// </summary>
    public abstract class AuditableEntity : Entity
    {
        /// <summary>
        /// The date and time when this entity was created
        /// </summary>
        public DateTime Created { get; set; }
        
        /// <summary>
        /// The user who created this entity
        /// </summary>
        public string CreatedBy { get; set; }
        
        /// <summary>
        /// The date and time when this entity was last modified
        /// </summary>
        public DateTime? LastModified { get; set; }
        
        /// <summary>
        /// The user who last modified this entity
        /// </summary>
        public string LastModifiedBy { get; set; }
    }
}
