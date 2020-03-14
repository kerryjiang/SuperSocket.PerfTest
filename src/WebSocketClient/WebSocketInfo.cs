using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketClient
{
    public class WebSocketInfo
    {
        public string SessionID { get; set; }

        public int Total { get; set; }

        public ClientWebSocket WebSocket { get; set; }
        
        public async ValueTask SendAsync(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            await WebSocket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}