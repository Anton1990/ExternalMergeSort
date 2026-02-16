namespace Shared;

public static class SolutionRoot
{
    private static readonly Lazy<string> _path = new(FindSolutionRoot);

    public static string Path => _path.Value;

    private static string FindSolutionRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            if (Directory.GetFiles(dir, "*.sln").Length > 0)
                return dir;
            dir = Directory.GetParent(dir)?.FullName;
        }

        return Environment.CurrentDirectory;
    }
}
