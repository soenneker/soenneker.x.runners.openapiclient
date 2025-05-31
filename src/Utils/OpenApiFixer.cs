using Microsoft.Extensions.Logging;
using Soenneker.Extensions.ValueTask;
using Soenneker.X.Runners.OpenApiClient.Utils.Abstract;
using Soenneker.Utils.File.Abstract;
using Soenneker.Utils.FileSync;
using Soenneker.Utils.FileSync.Abstract;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.X.Runners.OpenApiClient.Utils;

public sealed class OpenApiFixer : IOpenApiFixer
{
    private readonly IFileUtil _fileUtil;
    private readonly ILogger<OpenApiFixer> _logger;
    private readonly IFileUtilSync _fileUtilSync;

    public OpenApiFixer(IFileUtil fileUtil, ILogger<OpenApiFixer> logger, IFileUtilSync fileUtilSync)
    {
        _fileUtil = fileUtil;
        _logger = logger;
        _fileUtilSync = fileUtilSync;
    }

    public async ValueTask Fix(string sourceFilePath, string targetFilePath, CancellationToken cancellationToken = default)
    {
        //string json = await _fileUtil.Read(sourceFilePath, cancellationToken).NoSync();

        //_fileUtilSync.DeleteIfExists(targetFilePath);

        //await _fileUtil.Write(targetFilePath, openApi.ToJsonString(new JsonSerializerOptions {WriteIndented = true}), cancellationToken);

        //_logger.LogInformation($"OpenAPI spec fixed and saved as {targetFilePath}");
    }
}
