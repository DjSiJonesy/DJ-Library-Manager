namespace DJLibraryManager.UI.Services;

/// <summary>
/// Reports the progress of a long-running operation.
/// </summary>
public interface IProgressReporter
{
    void BeginOperation(string operationName);

    void ReportStage(string stage);

    void ReportProgress(int current, int total, string? currentItem = null);

    void Complete();

    void Cancel();

    void Fail(string message);
}