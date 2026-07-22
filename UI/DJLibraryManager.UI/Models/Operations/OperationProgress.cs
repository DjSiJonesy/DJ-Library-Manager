using CommunityToolkit.Mvvm.ComponentModel;

namespace DJLibraryManager.UI.Models.Operations;

public partial class OperationProgress : ObservableObject
{
    [ObservableProperty]
    private string operationName = string.Empty;

    /// <summary>
    /// Overall lifecycle state of the operation.
    /// </summary>
    [ObservableProperty]
    private OperationStage stage = OperationStage.None;

    /// <summary>
    /// Human-readable description of the current activity.
    /// Example: "Reading songs..."
    /// </summary>
    [ObservableProperty]
    private string currentStage = string.Empty;

    [ObservableProperty]
    private string currentItem = string.Empty;

    [ObservableProperty]
    private int currentValue;

    [ObservableProperty]
    private int totalValue;

    [ObservableProperty]
    private double percentage;

    [ObservableProperty]
    private string? errorMessage;

    /// <summary>
    /// Indicates whether the operation progress is indeterminate.
    /// </summary>
    public bool IsIndeterminate => TotalValue <= 0;

    /// <summary>
    /// Convenience properties for XAML bindings.
    /// </summary>
    public bool IsRunning => Stage == OperationStage.Running;

    public bool IsCompleted => Stage == OperationStage.Completed;

    public bool HasFailed => Stage == OperationStage.Failed;

    public bool IsCancelled => Stage == OperationStage.Cancelled;

    partial void OnTotalValueChanged(int value)
    {
        OnPropertyChanged(nameof(IsIndeterminate));
    }

    partial void OnStageChanged(OperationStage value)
    {
        OnPropertyChanged(nameof(IsRunning));
        OnPropertyChanged(nameof(IsCompleted));
        OnPropertyChanged(nameof(HasFailed));
        OnPropertyChanged(nameof(IsCancelled));
    }

    public void Reset(string operationName)
    {
        OperationName = operationName;

        Stage = OperationStage.Pending;

        CurrentStage = string.Empty;
        CurrentItem = string.Empty;

        CurrentValue = 0;
        TotalValue = 0;
        Percentage = 0;

        ErrorMessage = null;
    }

    public void ReportStage(string stage)
    {
        Stage = OperationStage.Running;
        CurrentStage = stage;
    }

    public void ReportProgress(int current, int total, string? item = null)
    {
        CurrentValue = current;
        TotalValue = total;

        if (!string.IsNullOrWhiteSpace(item))
        {
            CurrentItem = item;
        }

        Percentage = total > 0
            ? (double)current / total * 100.0
            : 0;
    }

    public void Complete()
    {
        if (TotalValue > 0)
        {
            CurrentValue = TotalValue;
        }

        Percentage = 100;
        Stage = OperationStage.Completed;
    }

    public void Cancel()
    {
        Stage = OperationStage.Cancelled;
    }

    public void Fail(string message)
    {
        ErrorMessage = message;
        Stage = OperationStage.Failed;
    }
}