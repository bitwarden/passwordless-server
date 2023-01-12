using System;

namespace Service.Helpers
{
    public static class SpanHelpers
    {
        public static Span<byte> Combine(Span<byte> first, Span<byte> second)
        {
            if (second == null || second.Length == 0)
            {
                return first;
            }

            byte[] buffer = new byte[first.Length + second.Length];

            var final = new Span<byte>(buffer);

            first.CopyTo(final.Slice(0, first.Length));
            second.CopyTo(final.Slice(first.Length, second.Length));

            return final;
        }
    }
}