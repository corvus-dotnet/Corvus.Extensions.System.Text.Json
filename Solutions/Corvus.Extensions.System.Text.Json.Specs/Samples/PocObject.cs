// <copyright file="PocObject.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.Json.Specs.Samples
{
    using System;
    using System.Globalization;

    /// <summary>
    /// A plain old CLR object.
    /// </summary>
    public class PocObject : IEquatable<PocObject>
    {
        /// <summary>
        /// Gets or sets a simple value.
        /// </summary>
        public string SomeValue { get; set; }

        /// <summary>
        /// Gets or sets a date time offset
        /// </summary>
        public DateTimeOffset SomeDateTime { get; set; }

        /// <summary>
        /// Gets or sets a nullable date time offset
        /// </summary>
        public DateTimeOffset? SomeNullableDateTime { get; set; }

        /// <summary>
        /// Gets or sets a culture info
        /// </summary>
        public CultureInfo SomeCulture { get; set; }

        /// <summary>
        /// Gets or sets an enumeration value
        /// </summary>
        public ExampleEnum SomeEnum { get; set; }

        /// <summary>
        /// Compares two instances of PocObject for equality.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>True if the instances are equal, false otherwise.</returns>
        public static bool operator ==(PocObject left, PocObject right)
        {
            return left?.Equals(right) ?? false;
        }

        /// <summary>
        /// Compares two instances of PocObject for inequality.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>False if the instances are equal, true otherwise.</returns>
        public static bool operator !=(PocObject left, PocObject right)
        {
            return !(left == right);
        }

        /// <inheritdoc />
        public bool Equals(PocObject other)
        {
            return (this.SomeValue, this.SomeDateTime, this.SomeNullableDateTime, this.SomeCulture?.Name, this.SomeEnum) == (other.SomeValue, other.SomeDateTime, other.SomeNullableDateTime, other.SomeCulture?.Name, other.SomeEnum);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is PocObject sci)
            {
                return this.Equals(sci);
            }

            return false;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return (this.SomeValue, this.SomeDateTime, this.SomeNullableDateTime, this.SomeCulture, this.SomeEnum).GetHashCode();
        }
    }
}