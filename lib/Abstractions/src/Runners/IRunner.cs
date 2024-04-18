using Abstractions.src.Models;

namespace Abstractions.src.Runners;

public interface IRunner
{
    public RunnerStatus Status { get; set; }
    public Task Continue();
    public Task Stop();
    public Task Suspend();
    public Task Run();
}
