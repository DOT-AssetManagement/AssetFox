namespace AssetFox.Core.WorkQueue;

public interface IWorkStarter
{
    void StartWork(IServiceProvider serviceProvider);
}
