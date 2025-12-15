using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Music_Synchronizer.Services;

public class MusicClient {
    private TcpClient _client;
    private bool _isConnected;

    public event Action<SyncMessage> OnMessageReceived;
    public event Action<string> OnLog;

    public bool IsConnected => _isConnected;

    public MusicClient() {
        OnMessageReceived += message => {
            Console.WriteLine(message);
        };
    }
    public async Task ConnectToHost(string hostIp, int port = 5000) {
        _client = new TcpClient();
        await _client.ConnectAsync(hostIp, port);
        _isConnected = true;

        OnLog?.Invoke($"Connected to host {hostIp}:{port}");

        // Listen for messages
        _ = Task.Run(async () => {
            var stream = _client.GetStream();
            var buffer = new byte[4096];
            var messageBuffer = new StringBuilder();

            while (_isConnected) {
                try {
                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    var data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    messageBuffer.Append(data);

                    // Process complete messages (delimited by newline)
                    var messages = messageBuffer.ToString().Split('\n');
                    for (int i = 0; i < messages.Length - 1; i++) {
                        var msg = JsonSerializer.Deserialize<SyncMessage>(messages[i]);
                        OnMessageReceived?.Invoke(msg);
                    }

                    messageBuffer.Clear();
                    if (messages.Length > 0)
                        messageBuffer.Append(messages[^1]);
                }
                catch (Exception ex) {
                    if (_isConnected)
                        OnLog?.Invoke($"Error: {ex.Message}");
                    break;
                }
            }

            OnLog?.Invoke("Disconnected from host");
        });
    }

    public void Disconnect() {
        _isConnected = false;
        _client?.Close();
    }
}