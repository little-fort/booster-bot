namespace BoosterBot.Core.Models;

public class PipelineResult<T>
{
    public T? Result { get; set; }
    public bool Success { get; set; }
    public List<string> Logs { get; set; } = [];

    private PipelineResult(T? result, bool success, List<string>? logs = null)
    {
        Result = result;
        Success = success;
        if (logs != null) Logs = logs;
    }

    public static PipelineResult<T> Ok(T result, List<string>? logs = null) => new(result, true, logs);
    public static PipelineResult<T> Fail(List<string>? logs = null) => new(default, false, logs);
}
