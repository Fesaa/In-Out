using System.Text.Json;
using API.Data;
using Microsoft.Extensions.Caching.Memory;

namespace API.Services;

public interface ILocalizationService
{
    /// <summary>
    /// Translate the key in the given locale, falls back to English
    /// </summary>
    /// <param name="locale">Locale</param>
    /// <param name="key">Key to translate</param>
    /// <param name="args">Argument to format the translation with</param>
    /// <returns>Formated translated string</returns>
    Task<string> Get(string locale, string key, params object[] args);
    /// <summary>
    /// Translate the key in the locale of the user, fallback to English
    /// </summary>
    /// <param name="userId">User to get the locale form</param>
    /// <param name="key">Key to translate</param>
    /// <param name="args">Argument to format the translation with</param>
    /// <returns>Formated translated string</returns>
    Task<string> Translate(string userId, string key, params object[] args);
    /// <summary>
    /// Translate the key in the English local, convince function for AllowAnonymous routes
    /// </summary>
    /// <param name="key">Key to translate</param>
    /// <param name="args">Argument to format the translation with</param>
    /// <returns>Formated translated string</returns>
    Task<string> Translate(string key, params object[] args);
    IEnumerable<string> GetLocales();
    
    string DefaultLocale { get; }
}

public class LocalizationService: ILocalizationService
{

    private readonly ILogger<LocalizationService> _logger;
    private readonly IDirectoryService _directoryService;
    private readonly IMemoryCache _memoryCache;
    private readonly IUnitOfWork _unitOfWork;

    private readonly string _localizationDirectoryUi;
    private readonly MemoryCacheEntryOptions _cacheOptions;

    public LocalizationService(ILogger<LocalizationService> logger, IDirectoryService directoryService,
        IHostEnvironment environment, IMemoryCache memoryCache, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _directoryService = directoryService;
        _memoryCache = memoryCache;
        _unitOfWork = unitOfWork;

        _cacheOptions = new MemoryCacheEntryOptions().SetSize(1).SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

        if (environment.IsDevelopment())
        {
            _localizationDirectoryUi = directoryService.FileSystem.Path.Join(
                directoryService.FileSystem.Directory.GetCurrentDirectory(), "../UI/Web/src/public/assets/langs");
        }
        else
        {
            _localizationDirectoryUi = directoryService.FileSystem.Path.Join(
                directoryService.FileSystem.Directory.GetCurrentDirectory(), "wwwroot", "assets/langs");
        }
    }

    public async Task<Dictionary<string, string>?> LoadLanguage(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            languageCode = DefaultLocale;
        }

        if (_memoryCache.TryGetValue(languageCode, out Dictionary<string, string>? language))
        {
            _logger.LogTrace("Returning language ({LanguageKey}) from memoryCache", languageCode);
            return language;
        }

        var languageFile = _directoryService.FileSystem.Path.Join(_directoryService.LocalizationDirectory, languageCode + ".json");
        _logger.LogDebug("Retrieving translations from {File}", languageFile);
        if (!_directoryService.FileSystem.File.Exists(languageFile))
        {
            throw new ArgumentException($"Language {languageCode} does not exist");
        }

        var json = await _directoryService.FileSystem.File.ReadAllTextAsync(languageFile);
        var lang = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        
        _memoryCache.Set(languageCode, lang, _cacheOptions);
        return lang;
    }

    public async Task<string> Get(string locale, string key, params object[] args)
    {
        var cacheKey = $"{locale}_{key}";
        if (!_memoryCache.TryGetValue(cacheKey, out string? translatedString))
        {
            var translationData = await LoadLanguage(locale);
            if (translationData != null && translationData.TryGetValue(key, out var value))
            {
                translatedString = value;
                _memoryCache.Set(cacheKey, translatedString, _cacheOptions);
            }
        }

        if (string.IsNullOrEmpty(translatedString))
        {
            if (locale == DefaultLocale)
            {
                return key;
            }

            return await Get(DefaultLocale, key, args);
        }

        if (args.Length > 0)
        {
            translatedString = string.Format(translatedString, args);
        }
        return translatedString;
    }

    public async Task<string> Translate(string userId, string key, params object[] args)
    {
        var userLocale = await _unitOfWork.UsersRepository.GetLocaleAsync(userId);
        return await Get(userLocale ?? DefaultLocale, key, args);
    }

    public Task<string> Translate(string key, params object[] args)
    {
        return Get(DefaultLocale, key, args);
    }

    public IEnumerable<string> GetLocales()
    {
        var uiLanguages = _directoryService
            .GetFilesWithExtension(_directoryService.FileSystem.Path.GetFullPath(_localizationDirectoryUi), @"\.json")
            .Select(f => _directoryService.FileSystem.Path.GetFileName(f).Replace(".json", string.Empty));

        var backendLanguages = _directoryService
            .GetFilesWithExtension(_directoryService.LocalizationDirectory, @"\.json")
            .Select(f => _directoryService.FileSystem.Path.GetFileName(f).Replace(".json", string.Empty));

        return uiLanguages.Intersect(backendLanguages).Distinct();
    }

    public string DefaultLocale => "en";
}