namespace AssetFoxCore.Services
{
    public interface IAnalysisEventLoggingService
    {
        void Log(AnalysisEventLogEntry logEntry);
    }
}
