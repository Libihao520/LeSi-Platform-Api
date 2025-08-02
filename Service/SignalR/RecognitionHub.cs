using Microsoft.AspNetCore.SignalR;

namespace Service.SignalR;

public class RecognitionHub : Hub
{
    public async Task RegisterTask(string taskId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, taskId);
    }

    public string GetConnectionId()
    {
        return Context.ConnectionId;
    }
}