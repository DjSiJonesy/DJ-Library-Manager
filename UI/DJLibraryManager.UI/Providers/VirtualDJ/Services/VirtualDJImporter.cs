using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DJLibraryManager.UI.Models;
using DJLibraryManager.UI.Models.Import;
using DJLibraryManager.UI.Models.Media;
using DJLibraryManager.UI.Providers.Interfaces;
using DJLibraryManager.UI.Providers.VirtualDJ.Translators;
using DJLibraryManager.UI.Services;

namespace DJLibraryManager.UI.Providers.VirtualDJ.Services;

/// <summary>
/// Imports a VirtualDJ library into the common DJLM media model.
/// </summary>
public sealed class VirtualDJImporter : IProviderImporter
{
    private readonly VirtualDJDatabaseLoader _databaseLoader = new();
    private readonly VirtualDJSongReader _songReader = new();
    private readonly VirtualDJMediaTranslator _translator = new();

    private readonly IProgressReporter _progressReporter;

    public VirtualDJImporter(IProgressReporter progressReporter)
    {
        _progressReporter = progressReporter
            ?? throw new ArgumentNullException(nameof(progressReporter));
    }

    public string ProviderName => "VirtualDJ";

    public Task<ImportResult> ImportAsync(ProviderInfo provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        try
        {
            if (string.IsNullOrWhiteSpace(provider.DatabasePath))
            {
                return Task.FromResult(new ImportResult
                {
                    Success = false,
                    ProviderName = ProviderName,
                    ImportedAt = DateTime.Now,
                    ErrorMessage = "No VirtualDJ database path has been configured."
                });
            }

            _progressReporter.ReportStage("Opening VirtualDJ database...");

            var database = _databaseLoader.Load(provider.DatabasePath);

            _progressReporter.ReportStage("Reading songs...");

            var songs = _songReader.ReadSongs(database);

            _progressReporter.ReportStage("Translating media...");

            var songList = new List<dynamic>(songs);

            var mediaItems = new List<DJLMMediaItem>(songList.Count);

            for (int i = 0; i < songList.Count; i++)
            {
                var mediaItem = _translator.Translate(songList[i]);

                mediaItems.Add(mediaItem);

                _progressReporter.ReportProgress(
                    i + 1,
                    songList.Count,
                    $"{mediaItem.Artist} - {mediaItem.Title}");
            }

            _progressReporter.ReportStage("Building media library...");

            var result = new ImportResult
            {
                Success = true,
                ProviderName = ProviderName,
                ImportedAt = DateTime.Now,
                TrackCount = mediaItems.Count,
                PlaylistCount = 0,
                MediaItems = mediaItems
            };

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ImportResult
            {
                Success = false,
                ProviderName = ProviderName,
                ImportedAt = DateTime.Now,
                ErrorMessage = ex.Message
            });
        }
    }
}