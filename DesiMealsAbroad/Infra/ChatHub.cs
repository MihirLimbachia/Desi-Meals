namespace DesiMealsAbroad.Infra;
using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        Console.WriteLine(message + " from " + user);
        await Clients.All.SendAsync("ReceiveMessage", user, message);

    }
  
}

