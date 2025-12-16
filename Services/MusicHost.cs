using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Music_Synchronizer.Services;

public class MusicHost {
    private TcpListener _listener;
    private List<TcpClient> _clients = new();
    private bool _isRunning;

    public bool IsRunning => _isRunning;
    public event Action<string> OnLog;

    public List<TcpClient> Clients => _clients;

    public Action<TcpClient> OnClientConnected;
    public Action<TcpClient> OnClientDisconnected;


    public MusicHost() {
        OnLog += message => { Console.WriteLine(message); };
    }

    public async Task StartHost(int port = 5000) {
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();


        _isRunning = true;

        OnLog?.Invoke($"Host started on port {port}");

        // Accept clients in background
        _ = Task.Run(async () => {
            while (_isRunning) {
                try {
                    var client = await _listener.AcceptTcpClientAsync();
                    _clients.Add(client);
                    OnClientConnected?.Invoke(client);
                    OnLog?.Invoke($"Client connected: {client.Client.RemoteEndPoint}");
                }
                catch (Exception ex) {
                    if (_isRunning) {
                        OnLog?.Invoke($"Error accepting client: {ex.Message}");
                    }
                }
            }
        });
    }

    public async Task BroadcastMessage(PlayerState message, TcpClient? tcpClient = null) {
        message.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var json = JsonSerializer.Serialize(message);
        var data = Encoding.UTF8.GetBytes(json + "\n");

        var disconnected = new List<TcpClient>();

        foreach (var client in _clients) {
            try {
                if (tcpClient != null && tcpClient != client) {
                    continue;
                }

                await client.GetStream().WriteAsync(data);
            }
            catch {
                disconnected.Add(client);
                OnClientDisconnected?.Invoke(client);
            }
        }

        // Remove disconnected clients
        foreach (var client in disconnected) {
            _clients.Remove(client);
            OnLog?.Invoke("Client disconnected");
        }
    }

    public void Stop() {
        _isRunning = false;
        foreach (var client in _clients)
            client.Close();
        _clients.Clear();
        _listener?.Stop();
        OnLog?.Invoke("Host stopped");
    }
}

public class PlayerState {
    public bool FileLoaded { get; set; }
    public string FileUrl { get; set; }

    public string FileId { get; set; }
    
    public long Timestamp { get; set; }
}