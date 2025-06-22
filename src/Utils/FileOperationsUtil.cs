﻿using Microsoft.Extensions.Logging;
using Soenneker.Extensions.String;
using Soenneker.Git.Util.Abstract;
using Soenneker.X.Runners.OpenApiClient.Utils.Abstract;
using Soenneker.Utils.Dotnet.Abstract;
using Soenneker.Utils.Environment;
using Soenneker.Utils.Process.Abstract;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Extensions.ValueTask;
using Soenneker.Utils.File.Abstract;
using Soenneker.Utils.File.Download.Abstract;
using Soenneker.Utils.FileSync.Abstract;
using Soenneker.Utils.Usings.Abstract;

namespace Soenneker.X.Runners.OpenApiClient.Utils;

///<inheritdoc cref="IFileOperationsUtil"/>
public sealed class FileOperationsUtil : IFileOperationsUtil
{
    private readonly ILogger<FileOperationsUtil> _logger;
    private readonly IGitUtil _gitUtil;
    private readonly IDotnetUtil _dotnetUtil;
    private readonly IProcessUtil _processUtil;
    private readonly IOpenApiFixer _openApiFixer;
    private readonly IFileDownloadUtil _fileDownloadUtil;
    private readonly IFileUtilSync _fileUtilSync;
    private readonly IFileUtil _fileUtil;
    private readonly IUsingsUtil _usingsUtil;

    public FileOperationsUtil(ILogger<FileOperationsUtil> logger, IGitUtil gitUtil, IDotnetUtil dotnetUtil, IProcessUtil processUtil,
        IOpenApiFixer openApiFixer, IFileDownloadUtil fileDownloadUtil, IFileUtilSync fileUtilSync, IFileUtil fileUtil, IUsingsUtil usingsUtil)
    {
        _logger = logger;
        _gitUtil = gitUtil;
        _dotnetUtil = dotnetUtil;
        _processUtil = processUtil;
        _openApiFixer = openApiFixer;
        _fileDownloadUtil = fileDownloadUtil;
        _fileUtilSync = fileUtilSync;
        _fileUtil = fileUtil;
        _usingsUtil = usingsUtil;
    }

    public async ValueTask Process(CancellationToken cancellationToken = default)
    {
        string gitDirectory =
            await _gitUtil.CloneToTempDirectory($"https://github.com/soenneker/{Constants.Library.ToLowerInvariantFast()}", cancellationToken);

        string targetFilePath = Path.Combine(gitDirectory, "openapi.json");

        _fileUtilSync.DeleteIfExists(targetFilePath);

        string? filePath = await _fileDownloadUtil.Download("https://api.x.com/2/openapi.json", targetFilePath, fileExtension: ".json",
            cancellationToken: cancellationToken);

        await _processUtil.Start("dotnet", null, "tool update --global Microsoft.OpenApi.Kiota", waitForExit: true, cancellationToken: cancellationToken);

        string srcDirectory = Path.Combine(gitDirectory, "src");

        DeleteAllExceptCsproj(srcDirectory);

        await _processUtil.Start("kiota", gitDirectory, $"kiota generate -l CSharp -d \"{filePath}\" -o src -c XOpenApiClient -n {Constants.Library}",
                              waitForExit: true, cancellationToken: cancellationToken)
                          .NoSync();

        string projFilePath = Path.Combine(gitDirectory, "src", $"{Constants.Library}.csproj");

        await _dotnetUtil.Restore(projFilePath, cancellationToken: cancellationToken);

        await _usingsUtil.AddMissing(projFilePath, true, 5, cancellationToken);

        await BuildAndPush(gitDirectory, cancellationToken).NoSync();
    }

    public void DeleteAllExceptCsproj(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            _logger.LogWarning("Directory does not exist: {DirectoryPath}", directoryPath);
            return;
        }

        try
        {
            // Delete all files except .csproj
            foreach (string file in Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories))
            {
                if (!file.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        File.Delete(file);
                        _logger.LogInformation("Deleted file: {FilePath}", file);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to delete file: {FilePath}", file);
                    }
                }
            }

            // Delete all empty subdirectories
            foreach (string dir in Directory.GetDirectories(directoryPath, "*", SearchOption.AllDirectories)
                                            .OrderByDescending(d => d.Length)) // Sort by depth to delete from deepest first
            {
                try
                {
                    if (Directory.Exists(dir) && !Directory.EnumerateFileSystemEntries(dir).Any())
                    {
                        Directory.Delete(dir, recursive: false);
                        _logger.LogInformation("Deleted empty directory: {DirectoryPath}", dir);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete directory: {DirectoryPath}", dir);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while cleaning the directory: {DirectoryPath}", directoryPath);
        }
    }

    private async ValueTask BuildAndPush(string gitDirectory, CancellationToken cancellationToken)
    {
        string projFilePath = Path.Combine(gitDirectory, "src", $"{Constants.Library}.csproj");

        await _dotnetUtil.Restore(projFilePath, cancellationToken: cancellationToken);

        bool successful = await _dotnetUtil.Build(projFilePath, true, "Release", false, cancellationToken: cancellationToken);

        if (!successful)
        {
            _logger.LogError("Build was not successful, exiting...");
            return;
        }

        string gitHubToken = EnvironmentUtil.GetVariableStrict("GH__TOKEN");

        await _gitUtil.CommitAndPush(gitDirectory, "soenneker", "jake@soenneker.com", gitHubToken, "Automated update", cancellationToken);
    }
}