namespace ADPA.Web.Services;

public class SimpleSignalRService
{
    public bool IsConnected => false;

    public async Task StartAsync()
    {
        await Task.CompletedTask;
    }
}