﻿namespace KempDec.FileCleaner.Core;

/// <summary>
/// Responsável pelo gerenciamento da limpeza de arquivos de um caminho.
/// </summary>
public class FileCleaner
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="FileCleaner"/>.
    /// </summary>
    /// <param name="path">O caminho ao qual será gerenciado para limpeza de arquivos.</param>
    /// <param name="movePath">O caminho ao qual serão movido os arquivos.</param></param>
    /// <exception cref="ArgumentException">É lançado quando o caminho especificado em <paramref name="path"/> não
    /// existe.</exception>
    public FileCleaner(string path, string movePath)
    {
        Path = path;
        MovePath = movePath;

        if (string.IsNullOrWhiteSpace(MovePath))
        {
            MovePath = null;
        }

        if (MovePath is not null && !Directory.Exists(MovePath))
        {
            Directory.CreateDirectory(MovePath);
        }

        if (!Directory.Exists(path))
        {
            throw new ArgumentException($"O caminho '{path}' não existe.", nameof(path));
        }
    }

    /// <summary>
    /// Obtém o caminho ao qual será gerenciado para limpeza de arquivos.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Obtém o caminho ao qual será movido os arquivos.
    /// </summary>
    public string? MovePath { get; }

    /// <summary>
    /// Apaga todos os arquivos.
    /// </summary>
    /// <param name="progress">O provedor para atualizações de progresso ao apagar os arquivos.</param>
    /// <returns>A quantidade de arquivos apagados.</returns>
    public int DeleteAllFiles(IProgress<(int Count, int TotalCount)>? progress = null)
    {
        List<FileInfo> files = GetAllMovedFiles();

        int deletionCount = 0;

        foreach (FileInfo file in files)
        {
            File.Delete(file.FullName);

            progress?.Report((++deletionCount, files.Count));
        }

        return files.Count;
    }

    /// <summary>
    /// Apaga todos os arquivos antigos, se houver algum, que possui a quantidade de dias atrás especificado.
    /// </summary>
    /// <param name="daysAgo">A quantidade de dias atrás para um arquivo ser antigo.</param>
    /// <param name="progress">O provedor para atualizações de progresso ao apagar os arquivos.</param>
    /// <returns>A quantidade de arquivos antigos apagados, se houver algum, que possui a quantidade de dias atrás
    /// especificado.</returns>
    public int MoveAllOldFiles(int daysAgo, IProgress<(int Count, int TotalCount)>? progress = null)
    {
        List<FileInfo> files = GetAllOriginFiles();
        DateTime greatestDate = DateTime.Now.AddDays(-daysAgo);

        files.RemoveAll(e => e.LastWriteTime.Date > greatestDate.Date);

        int moveCount = 0;

        foreach (FileInfo file in files)
        {
            var relativePath = file.FullName[new DirectoryInfo(Path).FullName.Length..].TrimStart('/', '\\');
            var destinationPath = System.IO.Path.Combine(new DirectoryInfo(MovePath).FullName, relativePath);
            var destinationDir = System.IO.Path.GetDirectoryName(destinationPath);

            if (!Directory.Exists(destinationDir))
                Directory.CreateDirectory(destinationDir!);

            File.Move(file.FullName, destinationPath);

            progress?.Report((++moveCount, files.Count));
        }

        return files.Count;
    }

    /// <summary>
    /// Apaga todos os arquivos antigos, se houver algum, que possui a quantidade de dias atrás especificado.
    /// </summary>
    /// <param name="daysAgo">A quantidade de dias atrás para um arquivo ser antigo.</param>
    /// <param name="progress">O provedor para atualizações de progresso ao apagar os arquivos.</param>
    /// <returns>A quantidade de arquivos antigos apagados, se houver algum, que possui a quantidade de dias atrás
    /// especificado.</returns>
    public int DeleteAllOldFiles(int daysAgo, IProgress<(int Count, int TotalCount)>? progress = null)
    {
        List<FileInfo> files = GetAllMovedFiles();
        DateTime greatestDate = DateTime.Now.AddDays(-daysAgo);

        files.RemoveAll(e => e.LastWriteTime.Date > greatestDate.Date);

        int deletionCount = 0;

        foreach (FileInfo file in files)
        {
            File.Delete(file.FullName);

            progress?.Report((++deletionCount, files.Count));
        }

        return files.Count;
    }

    /// <summary>
    /// Obtém todos os arquivos do caminho especificado, incluindo os arquivos em subpastas.
    /// </summary>
    /// <returns>Todos os arquivos do caminho especificado, incluindo os arquivos em subpastas.</returns>
    private List<FileInfo> GetAllOriginFiles()
    {
        List<FileInfo> files = GetFiles(Path);

        string[] paths = Directory.GetDirectories(Path);

        foreach (string path in paths)
        {
            List<FileInfo> pathFiles = GetFiles(path);

            files.AddRange(pathFiles);
        }

        return files;
    }

    /// <summary>
    /// Obtém todos os arquivos do caminho especificado, incluindo os arquivos em subpastas.
    /// </summary>
    /// <returns>Todos os arquivos do caminho especificado, incluindo os arquivos em subpastas.</returns>
    private List<FileInfo> GetAllMovedFiles()
    {
        List<FileInfo> files = GetFiles(MovePath ?? Path);

        string[] paths = Directory.GetDirectories(MovePath ?? Path);

        foreach (string path in paths)
        {
            List<FileInfo> pathFiles = GetFiles(path);

            files.AddRange(pathFiles);
        }

        return files;
    }

    /// <summary>
    /// Obtém os arquivos do caminho especificado.
    /// </summary>
    /// <param name="path">O caminho ao qual os arquivos serão buscados.</param>
    /// <returns>Os arquivos do caminho especificado.</returns>
    private static List<FileInfo> GetFiles(string path)
    {
        var files = new List<FileInfo>();
        string[] fileNames = Directory.GetFiles(path);

        foreach (string fileName in fileNames)
        {
            if (File.Exists(fileName))
            {
                var file = new FileInfo(fileName);

                files.Add(file);
            }
        }

        return files;
    }
}
