using System.IO.Abstractions;
using System.Text.RegularExpressions;

namespace API.Services;

public interface IDirectoryService
{
    IFileSystem FileSystem { get; }
    string LogDirectory { get; }
    string ConfigDirectory { get; }
    string LocalizationDirectory { get; }
    string ThemeDirectory { get; }
    string TempDirectory { get; }

    bool Exists(string directory);
    bool ExistOrCreate(string directoryPath);
    void CopyFileToDirectory(string fullFilePath, string targetDirectory);
    IEnumerable<string> GetFiles(string path, string fileNameRegex = "", SearchOption searchOption = SearchOption.TopDirectoryOnly);

    IEnumerable<string> GetFilesWithCertainExtensions(string path, string searchPatternExpression = "",
        SearchOption searchOption = SearchOption.TopDirectoryOnly);
    string[] GetFilesWithExtension(string path, string searchPatternExpression = "");
    IEnumerable<string> GetDirectories(string folderPath);
}

public class DirectoryService: IDirectoryService
{
    private const string MacOsMetadataFileStartsWith = @"._";
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(500);
    private readonly ILogger<DirectoryService> _logger;

    public IFileSystem FileSystem { get; }
    public string LogDirectory { get; }
    public string ConfigDirectory { get; }
    public string LocalizationDirectory { get; }
    public string ThemeDirectory { get; }
    public string TempDirectory { get; }

    public DirectoryService(ILogger<DirectoryService> logger, IFileSystem fileSystem)
    {
        _logger = logger;

        FileSystem = fileSystem;
        var root = FileSystem.Directory.GetCurrentDirectory();

        ConfigDirectory = Path.Combine(root, "config");
        ExistOrCreate(ConfigDirectory);

        LogDirectory = FileSystem.Path.Join(ConfigDirectory, "logs");
        ExistOrCreate(LogDirectory);

        LocalizationDirectory = FileSystem.Path.Join(root, "I18N");
        ExistOrCreate(LocalizationDirectory);
        
        ThemeDirectory = FileSystem.Path.Join(ConfigDirectory, "themes");
        ExistOrCreate(ThemeDirectory);
        
        TempDirectory = FileSystem.Path.Join(ConfigDirectory, "temp");
        ExistOrCreate(TempDirectory);

    }

    public bool Exists(string directory)
    {
        var dir = FileSystem.DirectoryInfo.New(directory);
        return dir.Exists;
    }

    public bool ExistOrCreate(string directoryPath)
    {
        var dir = FileSystem.DirectoryInfo.New(directoryPath);
        if (dir.Exists) return true;
        try
        {
            FileSystem.Directory.CreateDirectory(directoryPath);
        }
        catch (Exception)
        {
            return false;
        }
        return true;
    }

    public void CopyFileToDirectory(string fullFilePath, string targetDirectory)
    {
        try
        {
            var fileInfo = FileSystem.FileInfo.New(fullFilePath);
            if (!fileInfo.Exists) return;

            ExistOrCreate(targetDirectory);
            fileInfo.CopyTo(FileSystem.Path.Join(targetDirectory, fileInfo.Name), true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "There was a critical error when copying {File} to {Directory}", fullFilePath, targetDirectory);
        }
    }

    public IEnumerable<string> GetFiles(string path, string fileNameRegex = "", SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        if (!FileSystem.Directory.Exists(path))
        {
            yield break;
        }

        Regex? reSearchPattern = null;
        if (!string.IsNullOrEmpty(fileNameRegex))
        {
            reSearchPattern = new Regex(fileNameRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled, RegexTimeout);
        }

        foreach (var file in FileSystem.Directory.EnumerateFiles(path, "*", searchOption))
        {
            var fileName = FileSystem.Path.GetFileName(file);

            if (fileName.StartsWith(MacOsMetadataFileStartsWith))
                continue;

            if (reSearchPattern != null && !reSearchPattern.IsMatch(fileName))
                continue;

            yield return file;
        }
    }

    public IEnumerable<string> GetFilesWithCertainExtensions(string path, string searchPatternExpression = "",
        SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        if (!FileSystem.Directory.Exists(path))
        {
            yield break;
        }

        var reSearchPattern = new Regex(searchPatternExpression,
            RegexOptions.IgnoreCase | RegexOptions.Compiled,
            RegexTimeout);


        foreach (var file in FileSystem.Directory.EnumerateFiles(path, "*", searchOption))
        {
            var fileName = FileSystem.Path.GetFileName(file);
            var fileExtension = FileSystem.Path.GetExtension(file);

            if (reSearchPattern.IsMatch(fileExtension) && !fileName.StartsWith(MacOsMetadataFileStartsWith))
            {
                yield return file;
            }
        }
    }

    public string[] GetFilesWithExtension(string path, string searchPatternExpression = "")
    {
        if (searchPatternExpression != string.Empty)
        {
            return GetFilesWithCertainExtensions(path, searchPatternExpression).ToArray();
        }

        return !FileSystem.Directory.Exists(path) ? [] : FileSystem.Directory.GetFiles(path);
    }

    public IEnumerable<string> GetDirectories(string folderPath)
    {
        if (!Exists(folderPath))
        {
            return [];
        }

        return FileSystem.Directory.GetDirectories(folderPath);
    }
}