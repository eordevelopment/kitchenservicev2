using System;

namespace KitchenServiceV2.Tests
{
    public static class TestExtensions
    {
        public static DateTimeOffset Truncate(this DateTime dateTime, TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero) return dateTime; // Or could throw an ArgumentException
            return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
        }

        public static DateTimeOffset DaysFromNow(this int days)
        {
            return DateTimeOffset.UtcNow.AddDays(days).Date;
        }
    }
}
