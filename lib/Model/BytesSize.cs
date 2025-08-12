using System;
using System.Collections.Generic;
using System.Globalization;

namespace lib.Model;

/// <summary>
/// Represents an immutable file/data size value with convenient factory methods, arithmetic and comparison operators,
/// and human-friendly formatting. The value is stored in bytes and exposed as both total values (TotalKb, TotalMb, ...)
/// and decomposed components (Eb, Pb, Tb, Gb, Mb, Kb, Bytes).
/// </summary>
/// <remarks>
/// <para>
/// ByteSizes uses a binary scale (base 1024) for all units:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Unit</term>
///     <description>Value</description>
///   </listheader>
///   <item><term>KB</term><description>1 KB = 1024 B</description></item>
///   <item><term>MB</term><description>1 MB = 1024 KB</description></item>
///   <item><term>GB</term><description>1 GB = 1024 MB</description></item>
///   <item><term>TB</term><description>1 TB = 1024 GB</description></item>
///   <item><term>PB</term><description>1 PB = 1024 TB</description></item>
///   <item><term>EB</term><description>1 EB = 1024 PB</description></item>
/// </list>
/// <para>
/// The <see cref="ToString()"/> method returns a compact, pretty string like "1 GB, 200 MB, 10 KB" with only
/// the non-zero parts. You can compare, add, subtract and scale sizes using provided operators.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var a = ByteSizes.FromMb(1.5);    // 1 MB and 512 KB
/// var b = ByteSizes.FromKb(256);     // 256 KB
/// var sum = a + b;                   // 1 MB, 768 KB
/// var bigger = 2 * sum;              // scaling by a factor
/// Console.WriteLine(sum);            // "1 MB, 768 KB"
/// Console.WriteLine(bigger > sum);   // True
/// </code>
/// </example>
public readonly struct ByteSizes : IEquatable<ByteSizes>, IComparable<ByteSizes>, IFormattable
{
    #region constants

    /// <summary>Number of bytes in one kilobyte (binary): 1 KB = 1024 B.</summary>
    public const ulong KB = 1024;
    /// <summary>Number of bytes in one megabyte (binary): 1 MB = 1024 KB.</summary>
    public const ulong MB = KB * 1024;
    /// <summary>Number of bytes in one gigabyte (binary): 1 GB = 1024 MB.</summary>
    public const ulong GB = MB * 1024;
    /// <summary>Number of bytes in one terabyte (binary): 1 TB = 1024 GB.</summary>
    public const ulong TB = GB * 1024;
    /// <summary>Number of bytes in one petabyte (binary): 1 PB = 1024 TB.</summary>
    public const ulong PB = TB * 1024;
    /// <summary>Number of bytes in one exabyte (binary): 1 EB = 1024 PB.</summary>
    public const ulong EB = PB * 1024;

    #endregion

    #region operators

    public static bool operator !=(ByteSizes l, ByteSizes r) => !(l == r);
    public static bool operator ==(ByteSizes l, ByteSizes r) => l.Equals(r);

    public static ByteSizes operator *(ByteSizes left, double multiply)
    {
        return new ByteSizes(left.TotalBytes * multiply);
    }

    public static ByteSizes operator *(double multiply, ByteSizes left)
    {
        return new ByteSizes(left.TotalBytes * multiply);
    }

    public static bool operator >(ByteSizes l, ByteSizes r) => l.TotalBytes > r.TotalBytes;
    public static bool operator <(ByteSizes l, ByteSizes r) => l.TotalBytes < r.TotalBytes;
    public static bool operator >=(ByteSizes l, ByteSizes r) => l.TotalBytes >= r.TotalBytes;
    public static bool operator <=(ByteSizes l, ByteSizes r) => l.TotalBytes <= r.TotalBytes;

    public static ByteSizes operator +(ByteSizes left, ByteSizes right)
    {
        return new ByteSizes(left.TotalBytes + right.TotalBytes);
    }

    public static ByteSizes operator -(ByteSizes left, ByteSizes right)
    {
        return new ByteSizes(left.TotalBytes - right.TotalBytes);
    }

    #endregion

    #region ctor

    private ByteSizes(ulong totalBytes)
    {
        TotalBytes = totalBytes;
        var b = (double)TotalBytes;

        TotalEb = TotalBytes / EB;
        Eb = (int)Math.Floor(b / EB);
        b -= ((ulong)Eb * EB);

        TotalPb = TotalBytes / PB;
        Pb = (int)Math.Floor(b / PB);
        b -= ((ulong)Pb * PB);

        TotalTb = TotalBytes / TB;
        Tb = (int)Math.Floor(b / TB);
        b -= ((ulong)Tb * TB);

        TotalGb = TotalBytes / GB;
        Gb = (int)Math.Floor(b / GB);
        b -= ((ulong)Gb * GB);

        TotalMb = TotalBytes / MB;
        Mb = (int)Math.Floor(b / MB);
        b -= ((ulong)Mb * MB);

        TotalKb = TotalBytes / KB;
        Kb = (int)Math.Floor(b / KB);
        b -= ((ulong)Kb * KB);

        Bytes = (int)b;
    }

    private ByteSizes(double bytes) : this((ulong)Math.Floor(bytes)) { }

    /// <summary>
    /// Creates a <see cref="ByteSizes"/> from a number of bytes.
    /// </summary>
    /// <param name="bytes">The total number of bytes.</param>
    /// <returns>A new <see cref="ByteSizes"/> instance.</returns>
    public static ByteSizes FromB(ulong bytes) => new(bytes);
    /// <summary>
    /// Creates a <see cref="ByteSizes"/> from kibibytes (KB, base 1024).
    /// </summary>
    /// <param name="kb">The number of KB.</param>
    public static ByteSizes FromKb(double kb) => new(kb * KB);
    /// <summary>
    /// Creates a <see cref="ByteSizes"/> from mebibytes (MB, base 1024).
    /// </summary>
    /// <param name="mb">The number of MB.</param>
    public static ByteSizes FromMb(double mb) => new(mb * MB);
    /// <summary>
    /// Creates a <see cref="ByteSizes"/> from gibibytes (GB, base 1024).
    /// </summary>
    /// <param name="gb">The number of GB.</param>
    public static ByteSizes FromGb(double gb) => new(gb * GB);
    /// <summary>
    /// Creates a <see cref="ByteSizes"/> from tebibytes (TB, base 1024).
    /// </summary>
    /// <param name="tb">The number of TB.</param>
    public static ByteSizes FromTb(double tb) => new(tb * TB);
    /// <summary>
    /// Creates a <see cref="ByteSizes"/> from pebibytes (PB, base 1024).
    /// </summary>
    /// <param name="pb">The number of PB.</param>
    public static ByteSizes FromPb(double pb) => new(pb * PB);
    /// <summary>
    /// Creates a <see cref="ByteSizes"/> from exbibytes (EB, base 1024).
    /// </summary>
    /// <param name="eb">The number of EB.</param>
    public static ByteSizes FromEb(double eb) => new(eb * EB);

    /// <summary>Implicit conversion to <see cref="ulong"/> returning <see cref="TotalBytes"/>.</summary>
    public static implicit operator ulong(ByteSizes s) => s.TotalBytes;
    /// <summary>Implicit conversion to <see cref="long"/> returning <see cref="TotalBytes"/>.</summary>
    public static implicit operator long(ByteSizes s) => (long)s.TotalBytes;

    #endregion

    #region IEquatable

    public int CompareTo(ByteSizes other) => TotalBytes.CompareTo(other.TotalBytes);
    public bool Equals(ByteSizes other) => TotalBytes == other.TotalBytes;

    public override bool Equals(object obj) => obj is ByteSizes other && Equals(other);

    public override int GetHashCode() => TotalBytes.GetHashCode();

    /// <summary>
    /// Formats the size as a human-readable string by concatenating non-zero parts, e.g. "1 GB, 200 MB, 10 KB".
    /// </summary>
    /// <param name="format">Unused. Reserved for future formatting options.</param>
    /// <param name="formatProvider">Culture info (currently not affecting output).</param>
    /// <returns>A pretty, compact representation of this size.</returns>
    public string ToString(string format, IFormatProvider formatProvider)
    {
        var parts = new List<string>();

        if (Eb > 0) parts.Add($"{Eb} EB");
        if (Pb > 0) parts.Add($"{Pb} PB");
        if (Tb > 0) parts.Add($"{Tb} TB");
        if (Gb > 0) parts.Add($"{Gb} GB");
        if (Mb > 0) parts.Add($"{Mb} MB");
        if (Kb > 0) parts.Add($"{Kb} KB");
        if (Bytes > 0) parts.Add($"{Bytes} B");

        return string.Join(", ", parts);
    }

    /// <inheritdoc />
    public override string ToString() => ToString("", CultureInfo.CurrentCulture);

    #endregion

    #region props

    /// <summary>Total size in bytes.</summary>
    public ulong TotalBytes { get; }
    /// <summary>Total size in whole kilobytes (KB = 1024 B).</summary>
    public ulong TotalKb { get; }
    /// <summary>Total size in whole mebibytes (MB = 1024 KB).</summary>
    public ulong TotalMb { get; }
    /// <summary>Total size in whole gibibytes (GB = 1024 MB).</summary>
    public ulong TotalGb { get; }
    /// <summary>Total size in whole tebibytes (TB = 1024 GB).</summary>
    public ulong TotalTb { get; }
    /// <summary>Total size in whole pebibytes (PB = 1024 TB).</summary>
    public ulong TotalPb { get; }
    /// <summary>Total size in whole exbibytes (EB = 1024 PB).</summary>
    public ulong TotalEb { get; }

    /// <summary>Bytes component after decomposition (0..1023).</summary>
    public int Bytes { get; }
    /// <summary>Kilobytes component after decomposition (0..1023).</summary>
    public int Kb { get; }
    /// <summary>Megabytes component after decomposition (0..1023).</summary>
    public int Mb { get; }
    /// <summary>Gigabytes component after decomposition (0..1023).</summary>
    public int Gb { get; }
    /// <summary>Terabytes component after decomposition (0..1023).</summary>
    public int Tb { get; }
    /// <summary>Petabytes component after decomposition (0..1023).</summary>
    public int Pb { get; }
    /// <summary>Exabytes component after decomposition (0..1023).</summary>
    public int Eb { get; }

    #endregion
}