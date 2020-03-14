using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketClient
{
    class Program
    {
        private static WebSocketInfo[] _websocketInfoArr;

        static async Task Main(string[] args)
        {
            var connCount = int.Parse(args[0]);

            _websocketInfoArr = Enumerable
                .Range(0, connCount)
                .Select(i => new WebSocketInfo())
                .ToArray();

            if (args.Length >= 3 && args[2].Equals("console", StringComparison.OrdinalIgnoreCase))
            {
                ListenStart();
            }

            var uri = new Uri(args[1]);

            await AsyncParallel.ForEach(_websocketInfoArr, async w =>
            {
                await RunClient(w, uri);
            }, connCount);

            foreach (var w in _websocketInfoArr)
            {
                Console.WriteLine($"{w.SessionID}: {w.Total}");
            }
        }

        private static async void ListenStart()
        {
            while (true)
            {
                var line = Console.ReadLine();

                if ("start".Equals(line, StringComparison.OrdinalIgnoreCase))
                {
                    await _websocketInfoArr[0].SendAsync("StartPush");
                    break;
                }
            }
        }


        private static async ValueTask RunClient(WebSocketInfo websocketInfo, Uri uri)
        {
            var websocket = new ClientWebSocket();

            try
            {
                await websocket.ConnectAsync(uri, CancellationToken.None);
                Console.WriteLine("Connected");
            }
            catch
            {
                return;
            }            

            var buffer = new byte[1024 * 5];

            var result = await websocket.ReceiveAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), CancellationToken.None);

            websocketInfo.SessionID = Encoding.UTF8.GetString(buffer, 0, result.Count);

            while (true)
            {
                try
                {
                    result = await websocket.ReceiveAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), CancellationToken.None);
                }
                catch (WebSocketException)
                {
                    break;
                }                

                if (result.CloseStatus != null || result.Count == 0)
                    break;

                websocketInfo.Total += result.Count;
            }            
        }
    }
}
