namespace ORLtrack.Tests.Infrastructure;

internal static class SolutionPaths
{
    public static string RepositoryRoot
    {
        get
        {
            var current = new DirectoryInfo(AppContext.BaseDirectory);
            while (current != null)
            {
                if (File.Exists(Path.Combine(current.FullName, "movieRecom.sln")))
                {
                    return current.FullName;
                }

                current = current.Parent;
            }

            throw new DirectoryNotFoundException("Не удалось найти корень решения movieRecom.sln.");
        }
    }

    public static string CombineFromRoot(params string[] parts)
    {
        return Path.Combine(new[] { RepositoryRoot }.Concat(parts).ToArray());
    }
}
