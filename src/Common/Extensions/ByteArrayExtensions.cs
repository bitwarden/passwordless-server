using System.Buffers;

namespace Passwordless.Common.Extensions;

public static class ByteArrayExtensions
{
    /// <summary>
    /// Converts a byte array to a Base64Url encoded string.
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public static string ToBase64Url(this byte[] arg)
    {
        int minimumLength = (int)((arg.Length + 2L) / 3 * 4);
        char[] array = ArrayPool<char>.Shared.Rent(minimumLength);

        Convert.TryToBase64Chars(arg, array, out var charsWritten);

        Span<char> span = array.AsSpan(0, charsWritten);


        for (int i = 0; i < span.Length; i++)
        {
            ref char reference = ref span[i];
            switch (reference)
            {
                case '+':
                    reference = '-';
                    break;
                case '/':
                    reference = '_';
                    break;
            }
        }
        int num = span.IndexOf('=');
        if (num > -1)
        {
            span = span.Slice(0, num);
        }

        string result = new string(span);

        ArrayPool<char>.Shared.Return(array, clearArray: true);
        return result;
    }
}