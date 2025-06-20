using KempDec.FileCleaner.Core;
using KempDec.FileCleaner.Models;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using static System.Console;
using static KempDec.FileCleaner.Helpers.ConsoleHelper;

IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .Build();

AppSettings? appSettings = configuration.GetSection(nameof(AppSettings)).Get<AppSettings>();

if (appSettings is null)
{
    WriteErrorLine($"A seção '{nameof(AppSettings)}' no arquivo de configuração appsettings.json não foi encontrado.");

    ReadLine();

    return;
}

if (appSettings.CleaningTasks is not { Count: > 0 })
{
    WriteErrorLine("Nenhuma tarefa de limpeza foi encontrada.");

    ReadLine();

    return;
}

if (appSettings!.CleaningTasks!.Any(x => x.MovePath is not null && (x.MoveFilesDaysAgo ?? 0) >= x.FilesDaysAgo))
{
    WriteErrorLine($"Há tarefas de limpeza com o prazo para mover arquivos maior que o de deletar arquivos.");

    ReadLine();

    return;
}

do
{
    foreach (CleaningTask cleaningTask in appSettings.CleaningTasks)
    {
        if (string.IsNullOrEmpty(cleaningTask.Path))
        {
            WriteErrorLine("O caminho da tarefa de limpeza está vazio.");

            continue;
        }

        CleanFiles(cleaningTask.Path, cleaningTask.FilesDaysAgo, cleaningTask.MovePath, cleaningTask.MoveFilesDaysAgo);
    }

    ResetColor();

    if (appSettings.LoopDelay is not null)
    {
        WriteLine($"Aguardando {appSettings.LoopDelay} minutos para verificar novamente...");
        Thread.Sleep(TimeSpan.FromMinutes(appSettings.LoopDelay.Value));
    }

} while (appSettings.LoopDelay is not null);

WriteLine("Pressione Enter para Sair");
ReadLine();

static void CleanFiles(string path, int? daysAgo, string movePath, int? moveDaysAgo)
{
    try
    {
        var cleaner = new FileCleaner(path, movePath);

        WriteLine("==========================================================");
        WriteLine($"Caminho: {cleaner.Path}");
        WriteLine("Iniciando a limpeza...");

        ForegroundColor = ConsoleColor.Green;

        var progress = new Progress<(int Count, int TotalCount)>(WriteProgress);
        var stopwatch = Stopwatch.StartNew();

        int moveCount = 0;
        int count = 0;

        if (string.IsNullOrWhiteSpace(movePath))
        {
            count = daysAgo is null
                ? cleaner.DeleteAllFiles(progress)
                : cleaner.DeleteAllOldFiles(daysAgo.Value, progress);
        }
        else if (moveDaysAgo is not null)
        {
            moveCount = cleaner.MoveAllOldFiles(moveDaysAgo.Value, progress);

            count = daysAgo is null
                ? cleaner.DeleteAllFiles(progress)
                : cleaner.DeleteAllOldFiles(daysAgo.Value, progress);
        }
        else
        {
            WriteErrorLine("O período para mover os arquivos não está definido.");
            return;
        }

        stopwatch.Stop();

        ResetColor();

        if (count > 0)
        {
            // Escreve uma linha para evitar que o resultado seja escrito na frente do progresso.
            WriteLine();
        }

        WriteLine($"Limpeza concluída. {count} arquivos foram excluídos e {moveCount} arquivos foram movidos em {stopwatch.Elapsed.TotalSeconds:N2}s!");
    }
    catch (Exception ex)
    {
        WriteErrorLine(ex.Message);
    }
}

static void WriteProgress((int Count, int TotalCount) progress)
{
    WriteLineReplace($"Por favor, aguarde. Processando {progress.Count}/{progress.TotalCount} arquivos.");
}
