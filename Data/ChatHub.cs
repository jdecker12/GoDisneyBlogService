using Microsoft.AspNetCore.SignalR;
using GoDisneyBlog.Data.Entities;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace GoDisneyBlog.Data
{
    [Route("/chatHub")]
    public class ChatHub : Hub
    {
        private static List<ChatMessage> chatMessages = new List<ChatMessage>();

        public async Task SendMessage(string user, string message)
        {
            var chatMessage = new ChatMessage { User = user, Message = message };
            chatMessages.Add(chatMessage);
            await Clients.All.SendAsync("RecieveMessage", user, message);
        }
    }
}
