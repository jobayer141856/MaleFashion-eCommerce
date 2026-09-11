using System;

namespace MaleFashion.Domain.ValueOfObjects
{
    public record Address
    {
        public string StreetAddress { get; set; } = string.Empty;
        public string? ApartmentSuite { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = "Bangladesh";

        public Address() { }

        public Address(string streetAddress, string? apartmentSuite, string city, string state, string postalCode, string country)
        {
            StreetAddress = streetAddress;
            ApartmentSuite = apartmentSuite;
            City = city;
            State = state;
            PostalCode = postalCode;
            Country = country;
        }
    }
}