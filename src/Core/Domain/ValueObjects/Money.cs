using System;
using System.Collections.Generic;
using AlloVoisinClone.Domain.Common;
using AlloVoisinClone.Domain.Exceptions;

namespace AlloVoisinClone.Domain.ValueObjects
{
    /// <summary>
    /// Represents a monetary value with a currency
    /// </summary>
    public class Money : ValueObject
    {
        public decimal Amount { get; }
        public string Currency { get; }

        private Money() { } // Required by EF Core

        public Money(decimal amount, string currency)
        {
            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentNullException(nameof(currency));

            // Ensure currency code is valid (3-letter ISO code)
            if (currency.Length != 3)
                throw new DomainException("Currency code must be a 3-letter ISO code");

            // Normalize to uppercase
            currency = currency.ToUpperInvariant();

            // Ensure amount is not negative
            if (amount < 0)
                throw new DomainException("Money amount cannot be negative");

            // Round to 2 decimal places
            amount = Math.Round(amount, 2);

            Amount = amount;
            Currency = currency;
        }

        // Convenience constructor for EUR
        public static Money Euro(decimal amount) => new Money(amount, "EUR");

        // Convenience constructor for USD
        public static Money Dollar(decimal amount) => new Money(amount, "USD");

        // Add two Money objects (must be same currency)
        public Money Add(Money other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (Currency != other.Currency)
                throw new DomainException("Cannot add money with different currencies");

            return new Money(Amount + other.Amount, Currency);
        }

        // Subtract another Money object (must be same currency)
        public Money Subtract(Money other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (Currency != other.Currency)
                throw new DomainException("Cannot subtract money with different currencies");

            return new Money(Amount - other.Amount, Currency);
        }

        // Multiply by a factor
        public Money Multiply(decimal factor)
        {
            return new Money(Amount * factor, Currency);
        }

        // Divide by a divisor
        public Money Divide(decimal divisor)
        {
            if (divisor == 0)
                throw new DivideByZeroException();

            return new Money(Amount / divisor, Currency);
        }

        // Check if this amount is zero
        public bool IsZero() => Amount == 0;

        // Check if this amount is positive (greater than zero)
        public bool IsPositive() => Amount > 0;

        public override string ToString()
        {
            return $"{Amount:F2} {Currency}";
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }
}
