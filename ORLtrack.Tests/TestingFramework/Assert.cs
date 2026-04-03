namespace ORLtrack.Tests.TestingFramework;

internal static class Assert
{
    public static void True(bool condition, string? message = null)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message ?? "Expected condition to be true.");
        }
    }

    public static void False(bool condition, string? message = null)
    {
        if (condition)
        {
            throw new InvalidOperationException(message ?? "Expected condition to be false.");
        }
    }

    public static void Equal<T>(T expected, T actual, string? message = null)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException(message ?? $"Expected: {expected}; Actual: {actual}.");
        }
    }

    public static T IsType<T>(object? value)
    {
        if (value is not T typed)
        {
            throw new InvalidOperationException($"Expected type {typeof(T).Name}, but got {value?.GetType().Name ?? "null"}.");
        }

        return typed;
    }

    public static T Single<T>(IEnumerable<T> values)
    {
        var list = values.ToList();
        if (list.Count != 1)
        {
            throw new InvalidOperationException($"Expected a single item, but found {list.Count}.");
        }

        return list[0];
    }

    public static void Contains(string expectedSubstring, string actualString)
    {
        if (!actualString.Contains(expectedSubstring, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected to find '{expectedSubstring}' in the provided text.");
        }
    }

    public static void Contains<T>(IEnumerable<T> values, Func<T, bool> predicate)
    {
        if (!values.Any(predicate))
        {
            throw new InvalidOperationException("Expected collection to contain an item matching the predicate.");
        }
    }
}
