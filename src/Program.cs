using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

[DllImport("user32.dll")] static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, IntPtr dwExtraInfo);
[DllImport("user32.dll")] static extern bool SetCursorPos(int x, int y);
[DllImport("user32.dll")] static extern bool keybd_event(byte bVk, byte bScan, uint dwFlags, IntPtr dwExtraInfo);
[DllImport("user32.dll")] static extern short VkKeyScan(char ch);

const uint L_DOWN = 0x0002, L_UP = 0x0004, R_DOWN = 0x0008, R_UP = 0x0010;
const uint M_DOWN = 0x0020, M_UP = 0x0040, WHEEL = 0x0800, KEY_DOWN = 0x0000, KEY_UP = 0x0002;

static void MouseClick(string btn) {
    uint d, u;
    switch (btn) {
        case "left": d = L_DOWN; u = L_UP; break;
        case "right": d = R_DOWN; u = R_UP; break;
        case "middle": d = M_DOWN; u = M_UP; break;
        case "double": MouseClick("left"); Thread.Sleep(30); MouseClick("left"); return;
        default: return;
    }
    mouse_event(d, 0, 0, 0, IntPtr.Zero); Thread.Sleep(20);
    mouse_event(u, 0, 0, 0, IntPtr.Zero);
}

static void SendKeys(string text) {
    foreach (char ch in text) {
        short vk = VkKeyScan(ch); if (vk == -1) continue;
        byte code = (byte)(vk & 0xFF); byte shift = (byte)((vk >> 8) & 0xFF);
        if ((shift & 1) != 0) keybd_event(0x10, 0, KEY_DOWN, IntPtr.Zero);
        keybd_event(code, 0, KEY_DOWN, IntPtr.Zero); Thread.Sleep(10);
        keybd_event(code, 0, KEY_UP, IntPtr.Zero);
        if ((shift & 1) != 0) keybd_event(0x10, 0, KEY_UP, IntPtr.Zero);
    }
}

static void RunSequence(string jsonFile) {
    var seqText = File.ReadAllText(jsonFile);
    var steps = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(seqText);
    if (steps == null) return;
    foreach (var step in steps) {
        string action = step.GetValueOrDefault("action", default).GetString() ?? "";
        var argsList = new List<string> { action };
        if (step.TryGetValue("x", out var x)) argsList.Add(x.GetInt32().ToString());
        if (step.TryGetValue("y", out var y)) argsList.Add(y.GetInt32().ToString());
        if (step.TryGetValue("button", out var btn)) argsList.Add(btn.GetString() ?? "left");
        if (step.TryGetValue("text", out var txt)) argsList.Add(txt.GetString() ?? "");
        if (step.TryGetValue("count", out var cnt)) argsList.Add(cnt.GetInt32().ToString());
        if (step.TryGetValue("x2", out var x2)) argsList.Add(x2.GetInt32().ToString());
        if (step.TryGetValue("y2", out var y2)) argsList.Add(y2.GetInt32().ToString());
        ExecuteCommand(argsList.ToArray());
        int wait = 150;
        if (step.TryGetValue("wait_ms", out var w)) wait = w.GetInt32();
        Thread.Sleep(wait);
    }
}

static void ExecuteCommand(string[] args) {
    if (args.Length == 0) return;
    try {
        switch (args[0].ToLower()) {
            case "click":
                if (args.Length >= 4) { SetCursorPos(int.Parse(args[1]), int.Parse(args[2])); Thread.Sleep(50); MouseClick(args[3]); }
                break;
            case "move":
                if (args.Length >= 3) SetCursorPos(int.Parse(args[1]), int.Parse(args[2]));
                break;
            case "type":
                if (args.Length >= 2) { Thread.Sleep(200); SendKeys(args[1]); }
                break;
            case "scroll":
                if (args.Length >= 2) mouse_event(WHEEL, 0, 0, (uint)(int.Parse(args[1]) * 120), IntPtr.Zero);
                break;
            case "drag":
                if (args.Length >= 5) {
                    int x1 = int.Parse(args[1]), y1 = int.Parse(args[2]), x2 = int.Parse(args[3]), y2 = int.Parse(args[4]);
                    SetCursorPos(x1, y1); Thread.Sleep(100);
                    mouse_event(L_DOWN, 0, 0, 0, IntPtr.Zero);
                    for (float t = 0; t <= 1; t += 0.03f) SetCursorPos((int)(x1 + (x2 - x1) * t), (int)(y1 + (y2 - y1) * t));
                    Thread.Sleep(50); mouse_event(L_UP, 0, 0, 0, IntPtr.Zero);
                }
                break;
            case "seq":
                if (args.Length >= 4 && (args.Length - 1) % 3 == 0) {
                    for (int i = 1; i < args.Length; i += 3) {
                        SetCursorPos(int.Parse(args[i]), int.Parse(args[i+1])); Thread.Sleep(80);
                        MouseClick(args[i+2]); Thread.Sleep(100);
                    }
                }
                break;
        }
    } catch (Exception ex) { Console.Error.WriteLine($"[FAIL] {args[0]}: {ex.Message}"); }
}

// -- Entry Point --
if (args.Length == 0 || args[0] == "--help") {
    Console.WriteLine("ScreenClicker v0.2.0");
    Console.WriteLine("  click <x> <y> <left|right|middle|double>");
    Console.WriteLine("  move <x> <y>");
    Console.WriteLine("  type \"<text>\"");
    Console.WriteLine("  scroll <count>");
    Console.WriteLine("  drag <x1> <y1> <x2> <y2>");
    Console.WriteLine("  seq <x1> <y1> <c1> [<x2> <y2> <c2> ...]");
    Console.WriteLine("  --seq <file.json>  Run sequence from JSON file");
    Console.WriteLine("  --mcp              Run as MCP Server");
    return;
}
if (args[0] == "--seq" && args.Length >= 2) { RunSequence(args[1]); return; }
if (args[0] == "--mcp") { RunMcp(); return; }
ExecuteCommand(args);

// -- MCP Server --
static void RunMcp() {
    while (true) {
        var header = Console.ReadLine();
        if (string.IsNullOrEmpty(header)) break;
        if (!header.StartsWith("Content-Length:")) continue;
        int len = int.Parse(header.AsSpan("Content-Length: ".Length));
        Console.ReadLine();
        var buf = new char[len]; int read = 0;
        while (read < len) read += Console.In.ReadBlock(buf, read, len - read);
        var reqJson = new string(buf, 0, len);
        try {
            var req = JsonSerializer.Deserialize<McpRequest>(reqJson);
            if (req == null) break;
            var result = new SortedDictionary<string, object> { ["jsonrpc"] = "2.0", ["id"] = 1 };
            if (req.Method == "initialize") {
                result["result"] = new { protocolVersion = "2024-11-05", capabilities = new { tools = new { } }, serverInfo = new { name = "screen-clicker", version = "0.2.0" } };
            } else if (req.Method == "tools/list") {
                result["result"] = new { tools = new object[] {
                    new { name = "click", description = "Click at x,y with button", inputSchema = new { type = "object", properties = new { x = new { type = "number" }, y = new { type = "number" }, button = new { type = "string" } }, required = new string[] { "x", "y" } } },
                    new { name = "move", description = "Move mouse", inputSchema = new { type = "object", properties = new { x = new { type = "number" }, y = new { type = "number" } }, required = new string[] { "x", "y" } } },
                    new { name = "type", description = "Type text", inputSchema = new { type = "object", properties = new { text = new { type = "string" } }, required = new string[] { "text" } } },
                    new { name = "scroll", description = "Scroll", inputSchema = new { type = "object", properties = new { count = new { type = "number" } }, required = new string[] { "count" } } },
                    new { name = "drag", description = "Drag", inputSchema = new { type = "object", properties = new { x1 = new { type = "number" }, y1 = new { type = "number" }, x2 = new { type = "number" }, y2 = new { type = "number" } }, required = new string[] { "x1", "y1", "x2", "y2" } } }
                } };
            } else if (req.Method == "tools/call") {
                string toolName = req.Params.TryGetProperty("name", out var np) ? np.GetString() ?? "" : "";
                var mcpArgs = new List<string> { toolName };
                if (req.Params.TryGetProperty("arguments", out var ap)) {
                    if (ap.TryGetProperty("x", out var xv)) mcpArgs.Add(xv.GetInt32().ToString());
                    if (ap.TryGetProperty("y", out var yv)) mcpArgs.Add(yv.GetInt32().ToString());
                    if (ap.TryGetProperty("button", out var bv)) mcpArgs.Add(bv.GetString() ?? "left");
                    if (ap.TryGetProperty("text", out var tv)) mcpArgs.Add(tv.GetString() ?? "");
                    if (ap.TryGetProperty("count", out var cv)) mcpArgs.Add(cv.GetInt32().ToString());
                    if (ap.TryGetProperty("x1", out var x1v)) mcpArgs.Add(x1v.GetInt32().ToString());
                    if (ap.TryGetProperty("y1", out var y1v)) mcpArgs.Add(y1v.GetInt32().ToString());
                    if (ap.TryGetProperty("x2", out var x2v)) mcpArgs.Add(x2v.GetInt32().ToString());
                    if (ap.TryGetProperty("y2", out var y2v)) mcpArgs.Add(y2v.GetInt32().ToString());
                }
                ExecuteCommand(mcpArgs.ToArray());
                result["result"] = new { content = new object[] { new { type = "text", text = $"[OK] {toolName}" } } };
            } else if (req.Method == "notifications/initialized") {
                continue;
            } else {
                result["error"] = new { code = -32601, message = "Unknown: " + req.Method };
            }
            var respJson = JsonSerializer.Serialize(result);
            var bytes = System.Text.Encoding.UTF8.GetBytes(respJson);
            Console.WriteLine("Content-Length: " + bytes.Length);
            Console.WriteLine();
            Console.Write(respJson);
            Console.Out.Flush();
        } catch { break; }
    }
}

class McpRequest {
    [JsonPropertyName("id")] public JsonElement? Id { get; set; }
    [JsonPropertyName("method")] public string Method { get; set; } = "";
    [JsonPropertyName("params")] public JsonElement Params { get; set; }
}