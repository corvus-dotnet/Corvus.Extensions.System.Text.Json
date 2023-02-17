// <copyright file="DateTimeOffsetPocObject.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Json.Specs.Samples;

using System;

/// <summary>
/// A plain old CLR object with <see cref="DateTimeOffset"/> properties.
/// </summary>
public class DateTimeOffsetPocObject : IEquatable<DateTimeOffsetPocObject>
{
    /// <summary>
    /// Gets or sets a date time offset.
    /// </summary>
    public DateTimeOffset SomeDateTime { get; set; }

    /// <summary>
    /// Gets or sets a nullable date time offset.
    /// </summary>
    public DateTimeOffset? SomeNullableDateTime { get; set; }

    /// <summary>
    /// Compares two instances of PocObject for equality.
    /// </summary>
    /// <param name="left">The first object to compare.</param>
    /// <param name="right">The second object to compare.</param>
    /// <returns>True if the instances are equal, false otherwise.</returns>
    public static bool operator ==(DateTimeOffsetPocObject left, DateTimeOffsetPocObject right)
    {
        return left?.Equals(right) ?? false;
    }

    /// <summary>
    /// Compares two instances of DateTimeOffsetPocObject for inequality.
    /// </summary>
    /// <param name="left">The first object to compare.</param>
    /// <param name="right">The second object to compare.</param>
    /// <returns>False if the instances are equal, true otherwise.</returns>
    public static bool operator !=(DateTimeOffsetPocObject left, DateTimeOffsetPocObject right)
    {
        return !(left == right);
    }

    /// <inheritdoc />
    public bool Equals(DateTimeOffsetPocObject? other)
    {
        return other is not null && this.GetDefiningTuple() == other.GetDefiningTuple();
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is DateTimeOffsetPocObject sci)
        {
            return this.Equals(sci);
        }

        return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.SomeDateTime, this.SomeNullableDateTime);
    }

    public override string ToString()
    {
        return this.GetDefiningTuple().ToString();
    }

    private (DateTimeOffset SomeDateTime, DateTimeOffset? SomeNullableDateTime) GetDefiningTuple() =>
        (this.SomeDateTime, this.SomeNullableDateTime);
}