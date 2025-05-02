using System;
using System.Collections.Generic;
using AlloVoisinClone.Domain.Common;
using AlloVoisinClone.Domain.Exceptions;

namespace AlloVoisinClone.Domain.ValueObjects
{
    /// <summary>
    /// Represents a geographical location with address and coordinates
    /// </summary>
    public class Location : ValueObject
    {
        public string Address { get; }
        public string City { get; }
        public string PostalCode { get; }
        public string Country { get; }
        public double Latitude { get; }
        public double Longitude { get; }

        private Location() { } // Required by EF Core

        public Location(string address, string city, string postalCode, string country, double latitude, double longitude)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentNullException(nameof(address));

            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentNullException(nameof(city));

            if (string.IsNullOrWhiteSpace(postalCode))
                throw new ArgumentNullException(nameof(postalCode));

            if (string.IsNullOrWhiteSpace(country))
                throw new ArgumentNullException(nameof(country));

            // Validate latitude (-90 to 90)
            if (latitude < -90 || latitude > 90)
                throw new DomainException("Latitude must be between -90 and 90");

            // Validate longitude (-180 to 180)
            if (longitude < -180 || longitude > 180)
                throw new DomainException("Longitude must be between -180 and 180");

            Address = address;
            City = city;
            PostalCode = postalCode;
            Country = country;
            Latitude = latitude;
            Longitude = longitude;
        }

        // Create a location with address only (no coordinates)
        public static Location CreateWithoutCoordinates(string address, string city, string postalCode, string country)
        {
            return new Location(address, city, postalCode, country, 0, 0);
        }

        // Calculate distance in kilometers between two locations using the Haversine formula
        public double DistanceTo(Location other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            const double EarthRadiusKm = 6371.0;

            var lat1Rad = Latitude * Math.PI / 180;
            var lat2Rad = other.Latitude * Math.PI / 180;
            var dLat = (other.Latitude - Latitude) * Math.PI / 180;
            var dLon = (other.Longitude - Longitude) * Math.PI / 180;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadiusKm * c;
        }

        // Check if a location is within a certain distance (in kilometers)
        public bool IsWithinDistance(Location other, double maxDistanceKm)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (maxDistanceKm <= 0)
                throw new ArgumentException("Maximum distance must be positive", nameof(maxDistanceKm));

            return DistanceTo(other) <= maxDistanceKm;
        }

        public override string ToString()
        {
            return $"{Address}, {PostalCode} {City}, {Country}";
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Address;
            yield return City;
            yield return PostalCode;
            yield return Country;
            yield return Latitude;
            yield return Longitude;
        }
    }
}
