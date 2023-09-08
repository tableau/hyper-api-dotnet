using System;
using System.Text;


namespace Tableau.HyperAPI
{
    /// <summary>
    /// A Hyper Service version number of the form 'major.minor'.
    /// </summary>
    public struct HyperServiceVersion
    {
        /// <summary>
        /// The major part of the version number.
        /// </summary>
        public uint Major { get; }
        /// <summary>
        /// The minor part of the version number.
        /// </summary>
        public uint Minor { get; }

        /// <summary>
        /// Constructs a HyperServiceVersion.
        /// </summary>
        /// <param name="major">Major part of the version number.</param>
        /// <param name="minor">Minor part of the version number.</param>
        public HyperServiceVersion(uint major, uint minor)
        {
            Major = major;
            Minor = minor;
        }

        /// <summary>
        /// Get the string representation of the version number
        /// </summary>
        public override string ToString()
        {
            return $"{Major}.{Minor}";
        }

        /// <summary>
        /// Compares two <see cref="HyperServiceVersion"/> values.
        /// </summary>
        /// <param name="other">Value to compare with.</param>
        /// <returns>Whether this value compares equal to <c>other</c>.</returns>
        public bool Equals(HyperServiceVersion other)
        {
            return Major == other.Major && Minor == other.Minor;
        }

        /// <summary>
        /// Compares two <see cref="HyperServiceVersion"/> values.
        /// </summary>
        /// <param name="obj">Object to compare with.</param>
        /// <returns>Whether this object compares equal to <c>obj</c>.</returns>
        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case HyperServiceVersion version:
                    return Equals(version);

                default:
                    return false;
            }
        }

        /// <summary>
        /// Computes the hash code.
        /// </summary>
        /// <returns>Hash code.</returns>
        public override int GetHashCode()
        {
            return Impl.HashCode.Combine(Major, Minor);
        }
    }
}
