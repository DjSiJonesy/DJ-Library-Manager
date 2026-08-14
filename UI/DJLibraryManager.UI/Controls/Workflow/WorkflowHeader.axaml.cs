using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using DJLibraryManager.Core.Services;
using DJLibraryManager.UI.Services;
using System.IO;
using System.Windows.Input;

namespace DJLibraryManager.UI.Controls.Workflow;

public partial class WorkflowHeader : UserControl
{
    public WorkflowHeader()
    {
        InitializeComponent();

        OpenDuplicatesCommand =
            new RelayCommand(OpenDuplicatesFolder);
    }

    // ============================================================
    // Title
    // ============================================================

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<WorkflowHeader, string>(
            nameof(Title),
            string.Empty);

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    // ============================================================
    // Description
    // ============================================================

    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<WorkflowHeader, string>(
            nameof(Description),
            string.Empty);

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    // ============================================================
    // Previous
    // ============================================================

    public static readonly StyledProperty<string> PreviousTextProperty =
        AvaloniaProperty.Register<WorkflowHeader, string>(
            nameof(PreviousText),
            string.Empty);

    public string PreviousText
    {
        get => GetValue(PreviousTextProperty);
        set => SetValue(PreviousTextProperty, value);
    }

    public static readonly StyledProperty<ICommand?> PreviousCommandProperty =
        AvaloniaProperty.Register<WorkflowHeader, ICommand?>(
            nameof(PreviousCommand));

    public ICommand? PreviousCommand
    {
        get => GetValue(PreviousCommandProperty);
        set => SetValue(PreviousCommandProperty, value);
    }

    // ============================================================
    // Visibility
    // ============================================================

    public static readonly StyledProperty<bool> ShowPreviousProperty =
        AvaloniaProperty.Register<WorkflowHeader, bool>(
            nameof(ShowPrevious),
            true);

    public bool ShowPrevious
    {
        get => GetValue(ShowPreviousProperty);
        set => SetValue(ShowPreviousProperty, value);
    }

    public static readonly StyledProperty<bool> ShowNextProperty =
        AvaloniaProperty.Register<WorkflowHeader, bool>(
            nameof(ShowNext),
            true);

    public bool ShowNext
    {
        get => GetValue(ShowNextProperty);
        set => SetValue(ShowNextProperty, value);
    }

    // ============================================================
    // Secondary
    // ============================================================

    public static readonly StyledProperty<bool> ShowSecondaryProperty =
        AvaloniaProperty.Register<WorkflowHeader, bool>(
            nameof(ShowSecondary),
            false);

    public bool ShowSecondary
    {
        get => GetValue(ShowSecondaryProperty);
        set => SetValue(ShowSecondaryProperty, value);
    }

    public static readonly StyledProperty<string> SecondaryTextProperty =
        AvaloniaProperty.Register<WorkflowHeader, string>(
            nameof(SecondaryText),
            string.Empty);

    public string SecondaryText
    {
        get => GetValue(SecondaryTextProperty);
        set => SetValue(SecondaryTextProperty, value);
    }

    public static readonly StyledProperty<ICommand?> SecondaryCommandProperty =
        AvaloniaProperty.Register<WorkflowHeader, ICommand?>(
            nameof(SecondaryCommand));

    public ICommand? SecondaryCommand
    {
        get => GetValue(SecondaryCommandProperty);
        set => SetValue(SecondaryCommandProperty, value);
    }

    // ============================================================
    // Next
    // ============================================================

    public static readonly StyledProperty<string> NextTextProperty =
        AvaloniaProperty.Register<WorkflowHeader, string>(
            nameof(NextText),
            string.Empty);

    public string NextText
    {
        get => GetValue(NextTextProperty);
        set => SetValue(NextTextProperty, value);
    }

    public static readonly StyledProperty<ICommand?> NextCommandProperty =
        AvaloniaProperty.Register<WorkflowHeader, ICommand?>(
            nameof(NextCommand));

    public ICommand? NextCommand
    {
        get => GetValue(NextCommandProperty);
        set => SetValue(NextCommandProperty, value);
    }

    // ============================================================
    // Duplicate Protection
    // ============================================================

    /// <summary>
    /// Determines whether the Duplicate Protection notification
    /// is displayed in the workflow header.
    /// </summary>
    public static readonly StyledProperty<bool>
        ShowDuplicateProtectionProperty =
            AvaloniaProperty.Register<WorkflowHeader, bool>(
                nameof(ShowDuplicateProtection),
                false);

    public bool ShowDuplicateProtection
    {
        get => GetValue(ShowDuplicateProtectionProperty);
        set => SetValue(ShowDuplicateProtectionProperty, value);
    }

    /// <summary>
    /// Location where DIASISS retains duplicate files that the
    /// user has not selected to keep.
    /// </summary>
    public static readonly StyledProperty<string>
        DuplicateFolderPathProperty =
            AvaloniaProperty.Register<WorkflowHeader, string>(
                nameof(DuplicateFolderPath),
                ApplicationPaths.DiasissDuplicates);

    public string DuplicateFolderPath
    {
        get => GetValue(DuplicateFolderPathProperty);
        set => SetValue(DuplicateFolderPathProperty, value);
    }

    /// <summary>
    /// Opens the DIASISS Duplicates folder.
    ///
    /// The folder is created when the user explicitly chooses
    /// to open it.
    /// </summary>
    public ICommand OpenDuplicatesCommand { get; }

    private void OpenDuplicatesFolder()
    {
        var path = DuplicateFolderPath;

        if (string.IsNullOrWhiteSpace(path))
            return;

        try
        {
            Directory.CreateDirectory(path);

            FolderLauncher.Open(path);
        }
        catch
        {
            // FolderLauncher already handles Explorer failures.
            // Do not allow a UI action to crash the application.
        }
    }
}