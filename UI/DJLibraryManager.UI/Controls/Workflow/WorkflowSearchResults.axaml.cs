using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

using DJLibraryManager.UI.Models.Search;
using DJLibraryManager.UI.Search.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DJLibraryManager.UI.Controls.Workflow;

/// <summary>
/// Displays results returned by the Search workflow.
///
/// The control supports category-specific presentation:
///
/// - Duplicate Search displays SearchResult candidates.
/// - Metadata Search displays current metadata and proposed
///   metadata changes.
/// - Missing File Search displays the affected track and its
///   last known file location.
///
/// The control does not modify the DIASISS library.
/// It only presents Search information and records presentation
/// selections.
/// </summary>
public partial class WorkflowSearchResults :
    UserControl,
    INotifyPropertyChanged
{
    public WorkflowSearchResults()
    {
        InitializeComponent();

        ConfirmRecommendedMetadataChangesCommand =
            new DelegateCommand(
                SelectRecommendedMetadataChanges);

        // ========================================================
        // Establish the initial presentation state.
        //
        // ResultType can be supplied through a binding after the
        // control has been constructed, so OnPropertyChanged()
        // will also update this state when that happens.
        // ========================================================

        UpdateResultTypeVisibility(
            ResultType);
    }

    // ============================================================
    // Property Changed
    // ============================================================

    public new event PropertyChangedEventHandler? PropertyChanged;

    private void RaisePropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(
                propertyName));
    }

    // ============================================================
    // Results
    // ============================================================

    public static readonly StyledProperty<object?> ResultsProperty =
        AvaloniaProperty.Register<WorkflowSearchResults, object?>(
            nameof(Results));

    public object? Results
    {
        get => GetValue(ResultsProperty);
        set => SetValue(
            ResultsProperty,
            value);
    }

    // ============================================================
    // Heading
    // ============================================================

    public static readonly StyledProperty<string> HeadingProperty =
        AvaloniaProperty.Register<WorkflowSearchResults, string>(
            nameof(Heading),
            "Search Results");

    public string Heading
    {
        get => GetValue(HeadingProperty);
        set => SetValue(
            HeadingProperty,
            value);
    }

    // ============================================================
    // Result Type
    // ============================================================

    public static readonly StyledProperty<string> ResultTypeProperty =
        AvaloniaProperty.Register<WorkflowSearchResults, string>(
            nameof(ResultType),
            "Duplicates");

    public string ResultType
    {
        get => GetValue(ResultTypeProperty);
        set => SetValue(
            ResultTypeProperty,
            value);
    }

    public static readonly StyledProperty<bool>
        IsDuplicateResultsProperty =
        AvaloniaProperty.Register<WorkflowSearchResults, bool>(
            nameof(IsDuplicateResults),
            true);

    public bool IsDuplicateResults
    {
        get => GetValue(IsDuplicateResultsProperty);
        private set => SetValue(
            IsDuplicateResultsProperty,
            value);
    }

    public static readonly StyledProperty<bool>
        IsMetadataResultsProperty =
        AvaloniaProperty.Register<WorkflowSearchResults, bool>(
            nameof(IsMetadataResults),
            false);

    public bool IsMetadataResults
    {
        get => GetValue(IsMetadataResultsProperty);
        private set => SetValue(
            IsMetadataResultsProperty,
            value);
    }

    public static readonly StyledProperty<bool>
        IsMissingFileResultsProperty =
        AvaloniaProperty.Register<WorkflowSearchResults, bool>(
            nameof(IsMissingFileResults),
            false);

    public bool IsMissingFileResults
    {
        get => GetValue(IsMissingFileResultsProperty);
        private set => SetValue(
            IsMissingFileResultsProperty,
            value);
    }

    // ============================================================
    // Search Issue
    // ============================================================

    public static readonly StyledProperty<SearchIssue?> IssueProperty =
        AvaloniaProperty.Register<WorkflowSearchResults, SearchIssue?>(
            nameof(Issue));

    public SearchIssue? Issue
    {
        get => GetValue(IssueProperty);
        set => SetValue(
            IssueProperty,
            value);
    }

    // ============================================================
    // Metadata Identity
    // ============================================================

    public string MetadataDisplayName =>
        Issue?.DisplayName
        ?? "Unknown Track";

    public string MetadataFilePath =>
        Issue?.FilePath
        ?? string.Empty;

    // ============================================================
    // Missing File Identity
    // ============================================================

    public string MissingFileDisplayName =>
        Issue?.DisplayName
        ?? "Unknown Track";

    public string MissingFilePath =>
        Issue?.FilePath
        ?? string.Empty;

    public bool HasMissingFilePath =>
        !string.IsNullOrWhiteSpace(
            Issue?.FilePath);

    // ============================================================
    // Existing Metadata
    // ============================================================

    public string MetadataArtist =>
        DisplayValue(
            Issue?.Artist);

    public string MetadataTitle =>
        DisplayValue(
            Issue?.TrackTitle);

    public string MetadataAlbum =>
        DisplayValue(
            Issue?.Album);

    public string MetadataGenre =>
        DisplayValue(
            Issue?.Genre);

    public string MetadataYear =>
        Issue?.Year?.ToString()
        ?? "—";

    public string MetadataBpm =>
        Issue?.Bpm?.ToString("0.##")
        ?? "—";

    public string MetadataKey =>
        DisplayValue(
            Issue?.Key);

    public string MetadataDuration =>
        Issue?.Duration is TimeSpan duration
            ? duration.ToString(@"mm\:ss")
            : "—";

    // ============================================================
    // Metadata Recommendations
    // ============================================================

    public IEnumerable<MetadataChangeRecommendation>
        MetadataRecommendations =>
        Issue?.MetadataRecommendations
        ?? Enumerable.Empty<MetadataChangeRecommendation>();

    /// <summary>
    /// Indicates whether there is at least one metadata
    /// recommendation that can be selected.
    ///
    /// The recommendation service is responsible for deciding
    /// whether a recommendation is valid. The UI does not impose
    /// an additional percentage threshold.
    /// </summary>
    public bool HasConfirmableMetadataChanges =>
        Issue?
            .MetadataRecommendations
            .Any(
                recommendation =>
                    recommendation.IsRecommended &&
                    recommendation.IsChange)
        ?? false;

    public int ConfirmableMetadataChangeCount =>
        Issue?
            .MetadataRecommendations
            .Count(
                recommendation =>
                    recommendation.IsRecommended &&
                    recommendation.IsChange)
        ?? 0;

    // ============================================================
    // Metadata Bulk Selection Command
    // ============================================================

    public ICommand ConfirmRecommendedMetadataChangesCommand
    {
        get;
    }

    /// <summary>
    /// Selects every metadata recommendation that the
    /// MetadataRecommendationService has marked as recommended.
    ///
    /// This is retained only for compatibility with the existing
    /// presentation while the bulk action is moved to the Search
    /// workspace header.
    ///
    /// User-modified recommendations are never overwritten.
    /// </summary>
    private void SelectRecommendedMetadataChanges()
    {
        if (Issue is null)
            return;

        foreach (var recommendation in
                 Issue.MetadataRecommendations)
        {
            if (!recommendation.IsRecommended)
                continue;

            if (!recommendation.IsChange)
                continue;

            if (recommendation.IsUserModified)
                continue;

            recommendation.SetRecommendedSelection(
                true);
        }

        RaiseMetadataPropertiesChanged();
    }

    // ============================================================
    // Result Selection
    // ============================================================

    public event EventHandler<SearchResult>? ResultSelected;

    public event EventHandler<MetadataChangeRecommendation>?
        MetadataRecommendationSelected;

    // ============================================================
    // Duplicate Result Handling
    // ============================================================

    private void KeepResult_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.DataContext is not SearchResult result)
            return;

        SelectResult(result);
    }

    public void SelectResult(
        SearchResult? result)
    {
        if (result is null)
            return;

        ResultSelected?.Invoke(
            this,
            result);
    }

    // ============================================================
    // Metadata Recommendation Handling
    // ============================================================

    private void MetadataRecommendation_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (sender is not CheckBox checkBox)
            return;

        if (checkBox.DataContext
            is not MetadataChangeRecommendation recommendation)
        {
            return;
        }

        // A checkbox click is an explicit user decision.
        // Record it as such so future bulk recommendation
        // operations cannot overwrite the user's choice.
        recommendation.SetUserSelection(
            checkBox.IsChecked == true);

        MetadataRecommendationSelected?.Invoke(
            this,
            recommendation);

        RaiseMetadataPropertiesChanged();
    }

    // ============================================================
    // Avalonia Property Changes
    // ============================================================

    protected override void OnPropertyChanged(
        AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ResultTypeProperty)
        {
            var resultType =
                change.GetNewValue<string>()
                ?? string.Empty;

            UpdateResultTypeVisibility(
                resultType);
        }

        if (change.Property == IssueProperty)
        {
            RaiseMetadataPropertiesChanged();

            RaisePropertyChanged(
                nameof(MissingFileDisplayName));

            RaisePropertyChanged(
                nameof(MissingFilePath));

            RaisePropertyChanged(
                nameof(HasMissingFilePath));
        }
    }

    private void UpdateResultTypeVisibility(
        string? resultType)
    {
        resultType ??= string.Empty;

        var isMetadata =
            string.Equals(
                resultType,
                "Metadata",
                StringComparison.OrdinalIgnoreCase);

        var isDuplicates =
            string.Equals(
                resultType,
                "Duplicates",
                StringComparison.OrdinalIgnoreCase);

        var isMissingFiles =
            string.Equals(
                resultType,
                "Missing Files",
                StringComparison.OrdinalIgnoreCase);

        IsMetadataResults =
            isMetadata;

        IsDuplicateResults =
            isDuplicates;

        IsMissingFileResults =
            isMissingFiles;

        RaisePropertyChanged(
            nameof(IsMetadataResults));

        RaisePropertyChanged(
            nameof(IsDuplicateResults));

        RaisePropertyChanged(
            nameof(IsMissingFileResults));
    }

    // ============================================================
    // Metadata Property Refresh
    // ============================================================

    private void RaiseMetadataPropertiesChanged()
    {
        RaisePropertyChanged(
            nameof(MetadataDisplayName));

        RaisePropertyChanged(
            nameof(MetadataFilePath));

        RaisePropertyChanged(
            nameof(MetadataArtist));

        RaisePropertyChanged(
            nameof(MetadataTitle));

        RaisePropertyChanged(
            nameof(MetadataAlbum));

        RaisePropertyChanged(
            nameof(MetadataGenre));

        RaisePropertyChanged(
            nameof(MetadataYear));

        RaisePropertyChanged(
            nameof(MetadataBpm));

        RaisePropertyChanged(
            nameof(MetadataKey));

        RaisePropertyChanged(
            nameof(MetadataDuration));

        RaisePropertyChanged(
            nameof(MetadataRecommendations));

        RaisePropertyChanged(
            nameof(HasConfirmableMetadataChanges));

        RaisePropertyChanged(
            nameof(ConfirmableMetadataChangeCount));
    }

    // ============================================================
    // Helpers
    // ============================================================

    private static string DisplayValue(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "—"
            : value.Trim();
    }

    // ============================================================
    // Delegate Command
    // ============================================================

    private sealed class DelegateCommand : ICommand
    {
        private readonly Action _execute;

        public DelegateCommand(
            Action execute)
        {
            _execute =
                execute
                ?? throw new ArgumentNullException(
                    nameof(execute));
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(
            object? parameter)
        {
            return true;
        }

        public void Execute(
            object? parameter)
        {
            _execute();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(
                this,
                EventArgs.Empty);
        }
    }
}