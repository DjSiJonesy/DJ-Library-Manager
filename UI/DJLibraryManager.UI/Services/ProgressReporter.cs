using DJLibraryManager.UI.Models.Operations;

namespace DJLibraryManager.UI.Services;

/// <summary>
/// Reports the progress of the currently running operation.
/// </summary>
public sealed class ProgressReporter : IProgressReporter
{
    public OperationProgress CurrentOperation { get; } = new();

    public void BeginOperation(string operationName)
    {
        CurrentOperation.Reset(operationName);
    }

    public void ReportStage(string stage)
    {
        CurrentOperation.ReportStage(stage);
    }

    public void ReportProgress(int current, int total, string? currentItem = null)
    {
        CurrentOperation.ReportProgress(current, total, currentItem);
    }

    public void Complete()
    {
        CurrentOperation.Complete();
    }

    public void Cancel()
    {
        CurrentOperation.Cancel();
    }

    public void Fail(string message)
    {
        CurrentOperation.Fail(message);
    }
}