// <copyright file="CultureInfoPocObject.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Json.Specs.Samples;

using System;
using System.Globalization;

/// <summary>
/// A plain old CLR object.
/// </summary>
public class CultureInfoPocObject : IEquatable<CultureInfoPocObject>
{
    /// <summary>
    /// Gets or sets a culture info.
    /// </summary>
    public CultureInfo? SomeCulture { get; set; }

    /// <summary>
    /// Compares two instances of PocObject for equality.
    /// </summary>
    /// <param name="left">The first object to compare.</param>
    /// <param name="right">The second object to compare.</param>
    /// <returns>True if the instances are equal, false otherwise.</returns>
    public static bool operator ==(CultureInfoPocObject left, CultureInfoPocObject right)
    {
        return left?.Equals(right) ?? false;
    }

    /// <summary>
    /// Compares two instances of PocObject for inequality.
    /// </summary>
    /// <param name="left">The first object to compare.</param>
    /// <param name="right">The second object to compare.</param>
    /// <returns>False if the instances are equal, true otherwise.</returns>
    public static bool operator !=(CultureInfoPocObject left, CultureInfoPocObject right)
    {
        return !(left == right);
    }

    /// <inheritdoc />
    public bool Equals(CultureInfoPocObject? other)
    {
        return other is not null && this.SomeCulture?.Name == other.SomeCulture?.Name;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is CultureInfoPocObject sci)
        {
            return this.Equals(sci);
        }

        return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.SomeCulture);
    }

    public override string ToString()
    {
        return this.SomeCulture?.ToString() ?? "null";
    }
}