using System.Security.Cryptography;

namespace Passwordless.Service.Helpers;

public static class GuidHelper
{

    /// Generates a cryptographically secure random Guid.
    ///
    /// Characteristics
    ///     - Variant: RFC 4122
    ///     - Version: 4
    /// RFC
    ///     https://tools.ietf.org/html/rfc4122#section-4.1.3
    /// Stackoverflow
    ///     https://stackoverflow.com/a/59437504/10830091
    public static Guid CreateCryptographicallySecureRandomRFC4122Guid()
    {
        // byte indices
        int versionByteIndex = BitConverter.IsLittleEndian ? 7 : 6;
        const int variantByteIndex = 8;

        // version mask & shift for `Version 4`
        const int versionMask = 0x0F;
        const int versionShift = 0x40;

        // variant mask & shift for `RFC 4122`
        const int variantMask = 0x3F;
        const int variantShift = 0x80;

        // get bytes of cryptographically-strong random values            
        var bytes = RandomNumberGenerator.GetBytes(16);

        // Set version bits -- 6th or 7th byte according to Endianness, big or little Endian respectively
        bytes[versionByteIndex] = (byte)(bytes[versionByteIndex] & versionMask | versionShift);

        // Set variant bits -- 9th byte
        bytes[variantByteIndex] = (byte)(bytes[variantByteIndex] & variantMask | variantShift);

        // Initialize Guid from the modified random bytes
        return new Guid(bytes);
    }
}