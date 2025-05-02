using System;

namespace AlloVoisinClone.Domain.Exceptions
{
    /// <summary>
    /// Base exception for all domain exceptions
    /// </summary>
    public class DomainException : Exception
    {
        public DomainException()
        {
        }

        public DomainException(string message)
            : base(message)
        {
        }

        public DomainException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
