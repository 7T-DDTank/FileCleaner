namespace KempDec.FileCleaner.Models;

/// <summary>
/// Associação recursiva das configurações do aplicativo.
/// </summary>
internal record AppSettings
{
    public int? LoopDelay { get; init; } = null;
    /// <summary>
    /// Obtém ou inicializa as tarefas de limpeza a serem executadas pelo aplicativo.
    /// </summary>
    public IReadOnlyList<CleaningTask>? CleaningTasks { get; init; }
}
