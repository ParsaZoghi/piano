using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Keystage.Core;
using UnityEngine;

namespace Keystage.Remote
{
    /// <summary>
    /// Lets a phone on the same network act as a keyboard (replaces the web
    /// version's Vite relay). The game runs a tiny web server: the phone opens
    /// the keyboard page over HTTP and sends notes back over a WebSocket.
    /// Network work happens on background threads; notes reach the game
    /// through a queue drained on the main thread.
    /// </summary>
    public static class RemoteServer
    {
        public enum State { Off, Ready, Unavailable }

        public const int FirstPort = 8765;
        const string WebSocketGuid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        static readonly Regex TypeField = new Regex("\"t\"\\s*:\\s*\"(on|off)\"");
        static readonly Regex NoteField = new Regex("\"n\"\\s*:\\s*(\\d+)");
        static readonly Regex VelocityField = new Regex("\"v\"\\s*:\\s*([0-9.]+)");

        public static State Status { get; private set; } = State.Off;
        public static int Port { get; private set; }
        public static string Room { get; private set; } = "";
        public static string Url { get; private set; } = "";
        public static int Peers { get; private set; }
        public static event Action Changed;

        struct NoteMessage
        {
            public bool on;
            public int midi;
            public float velocity;
            public int connection;
            public bool releaseAll;
        }

        static readonly ConcurrentQueue<NoteMessage> Messages = new ConcurrentQueue<NoteMessage>();
        static readonly List<TcpClient> Clients = new List<TcpClient>();
        static TcpListener listener;
        static Thread acceptThread;
        static string page = "";
        static int peerCount;
        static int nextConnection;
        static volatile bool running;

        /// <summary>Start serving the phone page (once per session).</summary>
        public static void Start()
        {
            if (running) return;
            var pageAsset = Resources.Load<TextAsset>("Phone/phone");
            page = pageAsset != null ? pageAsset.text : "<h1>Keystage</h1>";

            string ip = LanAddress();
            for (int port = FirstPort; port < FirstPort + 10 && listener == null; port++)
            {
                try
                {
                    var l = new TcpListener(IPAddress.Any, port);
                    l.Start();
                    listener = l;
                    Port = port;
                }
                catch (SocketException)
                {
                    // Port busy: try the next one.
                }
            }

            if (listener == null || ip == null)
            {
                listener?.Stop();
                listener = null;
                Status = State.Unavailable;
                Changed?.Invoke();
                return;
            }

            if (string.IsNullOrEmpty(Room)) Room = NewRoomCode();
            running = true;
            acceptThread = new Thread(AcceptLoop) { IsBackground = true, Name = "Keystage phone server" };
            acceptThread.Start();
            Status = State.Ready;
            RefreshUrl(ip);
            Changed?.Invoke();
        }

        /// <summary>The link embeds the game's theme, language and note names for the phone page.</summary>
        public static void RefreshUrl(string ip = null)
        {
            if (Status != State.Ready) return;
            ip = ip ?? LanAddress();
            var s = Settings.Data;
            Url = $"http://{ip}:{Port}/?remote={Room}&theme={s.theme}&lang={s.lang}&names={s.naming}";
        }

        public static void Stop()
        {
            running = false;
            try
            {
                listener?.Stop();
            }
            catch (Exception)
            {
                // Already closed.
            }
            listener = null;
            lock (Clients)
            {
                foreach (var c in Clients) c.Close();
                Clients.Clear();
            }
            Status = State.Off;
            Peers = peerCount = 0;
        }

        /// <summary>Hand queued phone notes to the game (main thread, once per frame).</summary>
        public static void Update()
        {
            while (Messages.TryDequeue(out var m))
            {
                string source = "remote" + m.connection;
                if (m.releaseAll) InputBus.ReleaseAll(source);
                else if (m.on) InputBus.Press(m.midi, m.velocity, source);
                else InputBus.Release(m.midi, source);
            }
            int peers = Volatile.Read(ref peerCount);
            if (peers != Peers)
            {
                Peers = peers;
                Changed?.Invoke();
            }
        }

        static string NewRoomCode()
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var bytes = new byte[5];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(bytes);
            var sb = new StringBuilder();
            foreach (var b in bytes) sb.Append(alphabet[b % alphabet.Length]);
            return sb.ToString();
        }

        /// <summary>The computer's address on the local network (first private IPv4 found).</summary>
        static string LanAddress()
        {
            try
            {
                string fallback = null;
                foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.OperationalStatus != OperationalStatus.Up || nic.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
                    foreach (var addr in nic.GetIPProperties().UnicastAddresses)
                    {
                        if (addr.Address.AddressFamily != AddressFamily.InterNetwork) continue;
                        var b = addr.Address.GetAddressBytes();
                        bool isPrivate = b[0] == 10 || (b[0] == 172 && b[1] >= 16 && b[1] <= 31) || (b[0] == 192 && b[1] == 168);
                        if (isPrivate && nic.GetIPProperties().GatewayAddresses.Count > 0) return addr.Address.ToString();
                        if (isPrivate || (b[0] != 169 && b[0] != 127)) fallback = fallback ?? addr.Address.ToString();
                    }
                }
                return fallback;
            }
            catch (Exception)
            {
                return null;
            }
        }

        static void AcceptLoop()
        {
            while (running)
            {
                try
                {
                    var client = listener.AcceptTcpClient();
                    client.NoDelay = true;
                    lock (Clients) Clients.Add(client);
                    new Thread(() => Serve(client)) { IsBackground = true, Name = "Keystage phone client" }.Start();
                }
                catch (Exception)
                {
                    if (!running) return;
                }
            }
        }

        static void Serve(TcpClient client)
        {
            try
            {
                using (client)
                {
                    var stream = client.GetStream();
                    stream.ReadTimeout = 10000;
                    string request = ReadHeaders(stream);
                    if (request == null) return;

                    var lines = request.Split(new[] { "\r\n" }, StringSplitOptions.None);
                    var parts = lines[0].Split(' ');
                    if (parts.Length < 2 || parts[0] != "GET")
                    {
                        Respond(stream, "405 Method Not Allowed", "text/plain", "Method not allowed");
                        return;
                    }
                    string target = parts[1];
                    string path = target.Split('?')[0];
                    var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    for (int i = 1; i < lines.Length; i++)
                    {
                        int colon = lines[i].IndexOf(':');
                        if (colon > 0) headers[lines[i].Substring(0, colon).Trim()] = lines[i].Substring(colon + 1).Trim();
                    }

                    if (path == "/ws" && headers.TryGetValue("Sec-WebSocket-Key", out var key))
                    {
                        if (Query(target, "room") != Room)
                        {
                            Respond(stream, "403 Forbidden", "text/plain", "Wrong room");
                            return;
                        }
                        RunWebSocket(stream, key);
                    }
                    else if (path == "/" || path == "/index.html") Respond(stream, "200 OK", "text/html; charset=utf-8", page);
                    else Respond(stream, "404 Not Found", "text/plain", "Not found");
                }
            }
            catch (Exception)
            {
                // The phone went away or sent something unexpected; just drop it.
            }
            finally
            {
                lock (Clients) Clients.Remove(client);
            }
        }

        static string ReadHeaders(Stream stream)
        {
            var buffer = new List<byte>(1024);
            while (buffer.Count < 16384)
            {
                int b = stream.ReadByte();
                if (b < 0) return null;
                buffer.Add((byte)b);
                int n = buffer.Count;
                if (n >= 4 && buffer[n - 4] == '\r' && buffer[n - 3] == '\n' && buffer[n - 2] == '\r' && buffer[n - 1] == '\n')
                    return Encoding.ASCII.GetString(buffer.ToArray(), 0, n - 4);
            }
            return null;
        }

        static string Query(string target, string name)
        {
            int q = target.IndexOf('?');
            if (q < 0) return null;
            foreach (var pair in target.Substring(q + 1).Split('&'))
            {
                var kv = pair.Split('=');
                if (kv.Length == 2 && kv[0] == name) return Uri.UnescapeDataString(kv[1]);
            }
            return null;
        }

        static void Respond(Stream stream, string status, string type, string body)
        {
            byte[] content = Encoding.UTF8.GetBytes(body);
            string head = $"HTTP/1.1 {status}\r\nContent-Type: {type}\r\nContent-Length: {content.Length}\r\nCache-Control: no-store\r\nConnection: close\r\n\r\n";
            byte[] headBytes = Encoding.ASCII.GetBytes(head);
            stream.Write(headBytes, 0, headBytes.Length);
            stream.Write(content, 0, content.Length);
        }

        static void RunWebSocket(NetworkStream stream, string key)
        {
            string accept;
            using (var sha = SHA1.Create()) accept = Convert.ToBase64String(sha.ComputeHash(Encoding.ASCII.GetBytes(key + WebSocketGuid)));
            byte[] handshake = Encoding.ASCII.GetBytes($"HTTP/1.1 101 Switching Protocols\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Accept: {accept}\r\n\r\n");
            stream.Write(handshake, 0, handshake.Length);
            stream.ReadTimeout = Timeout.Infinite;

            int connection = Interlocked.Increment(ref nextConnection);
            Interlocked.Increment(ref peerCount);
            try
            {
                SendText(stream, "{\"t\":\"host\",\"on\":true}");
                while (running)
                {
                    if (!ReadFrame(stream, out int opcode, out byte[] payload)) break;
                    if (opcode == 0x8) break; // close
                    if (opcode == 0x9)
                    {
                        SendFrame(stream, 0xA, payload); // ping -> pong
                        continue;
                    }
                    if (opcode != 0x1) continue;
                    HandleMessage(Encoding.UTF8.GetString(payload), connection);
                }
            }
            finally
            {
                Interlocked.Decrement(ref peerCount);
                Messages.Enqueue(new NoteMessage { connection = connection, releaseAll = true });
            }
        }

        static void HandleMessage(string json, int connection)
        {
            var type = TypeField.Match(json);
            var note = NoteField.Match(json);
            if (!type.Success || !note.Success || !int.TryParse(note.Groups[1].Value, out int midi)) return;
            if (midi < Notes.PianoMin || midi > Notes.PianoMax) return;
            float velocity = 0.8f;
            var v = VelocityField.Match(json);
            if (v.Success && float.TryParse(v.Groups[1].Value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float parsed))
                velocity = Mathf.Clamp01(parsed);
            Messages.Enqueue(new NoteMessage { on = type.Groups[1].Value == "on", midi = midi, velocity = velocity, connection = connection });
        }

        static bool ReadFrame(Stream stream, out int opcode, out byte[] payload)
        {
            opcode = 0;
            payload = null;
            int b0 = stream.ReadByte(), b1 = stream.ReadByte();
            if (b0 < 0 || b1 < 0) return false;
            opcode = b0 & 0x0F;
            bool masked = (b1 & 0x80) != 0;
            long length = b1 & 0x7F;
            if (length == 126)
            {
                var ext = ReadExact(stream, 2);
                length = (ext[0] << 8) | ext[1];
            }
            else if (length == 127) return false; // never needed for note messages
            if (length > 1024) return false;
            byte[] mask = masked ? ReadExact(stream, 4) : null;
            payload = ReadExact(stream, (int)length);
            if (masked)
                for (int i = 0; i < payload.Length; i++) payload[i] ^= mask[i % 4];
            return true;
        }

        static byte[] ReadExact(Stream stream, int count)
        {
            var data = new byte[count];
            int read = 0;
            while (read < count)
            {
                int n = stream.Read(data, read, count - read);
                if (n <= 0) throw new IOException("Connection closed");
                read += n;
            }
            return data;
        }

        static void SendText(Stream stream, string text) => SendFrame(stream, 0x1, Encoding.UTF8.GetBytes(text));

        static void SendFrame(Stream stream, int opcode, byte[] payload)
        {
            payload = payload ?? new byte[0];
            var frame = new List<byte> { (byte)(0x80 | opcode) };
            if (payload.Length < 126) frame.Add((byte)payload.Length);
            else
            {
                frame.Add(126);
                frame.Add((byte)(payload.Length >> 8));
                frame.Add((byte)payload.Length);
            }
            frame.AddRange(payload);
            lock (stream) stream.Write(frame.ToArray(), 0, frame.Count);
        }
    }
}
