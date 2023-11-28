namespace KempDec.FileCleaner.Helpers;

/// <summary>
/// Classe com métodos auxiliares para <see cref="Console"/>.
/// </summary>
public static class ConsoleHelper
{
    /// <summary>
    /// Escreve uma mensagem que representa um erro na cor vermelha usando <see cref="Console.WriteLine()"/>.
    /// </summary>
    /// <param name="message">A mensagem a ser escrita.</param>
    public static void WriteErrorLine(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    /// <summary>
    /// Escreve uma mensagem, substituindo a mensagem atual.
    /// </summary>
    /// <param name="message">A mensagem a ser escrita.</param>
    public static void WriteReplace(string message) => Console.Write($"\r{message}");
}
