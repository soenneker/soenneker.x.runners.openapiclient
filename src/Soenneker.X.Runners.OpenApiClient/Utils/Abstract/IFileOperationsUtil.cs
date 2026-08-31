using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.X.Runners.OpenApiClient.Utils.Abstract;

/// <summary>
/// Regenerates, validates, and publishes the X OpenAPI client.
/// </summary>
public interface IFileOperationsUtil
{
    /// <summary>
    /// Runs the client regeneration and publishing workflow.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask Process(CancellationToken cancellationToken = default);
}
