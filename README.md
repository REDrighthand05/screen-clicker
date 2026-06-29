# Screen Clicker

![Windows](https://img.shields.io/badge/platform-Windows-blue)
![C#](https://img.shields.io/badge/language-C%23-178600)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![License](https://img.shields.io/badge/license-MIT-green)
![Version](https://img.shields.io/badge/version-0.2.0-orange)

**给 AI 装上一双手。** Screen Clicker 是一个纯本地的鼠标键盘模拟工具，直接调用 Win32 API (user32.dll)，无需任何依赖，毫秒级响应。

---

## ✨ 特性

* **纯本地运行** — 直接调用 Win32 API，零网络请求
* **零依赖** — 不需要 Python、Node.js 或任何第三方库，编译即可用
* **鼠标操控** — 单击、双击、右键、中键、移动、拖拽、滚动
* **键盘模拟** — 支持任意按键输入，大小写自动处理
* **序列引擎** — 用 JSON 文件编排复杂操作序列
* **MCP Server** — 内置 Model Context Protocol 支持，注册为 AI agent 的"手"
* **可编程** — 适合与 OCR 工具联动：读屏→识别→定位→点击的全自动化

---

## 🚀 快速开始

### 从源码运行

```powershell
# 查看帮助
dotnet run --project src/clicker.csproj

# 点击坐标 (500,300) 左键
dotnet run --project src/clicker.csproj -- click 500 300 left

# 移动鼠标到 (100,200)
dotnet run --project src/clicker.csproj -- move 100 200

# 输入文字
dotnet run --project src/clicker.csproj -- type "Hello World"

# 滚动
dotnet run --project src/clicker.csproj -- scroll -3

# 拖拽 (从 100,100 到 500,400)
dotnet run --project src/clicker.csproj -- drag 100 100 500 400

# 批量点击序列
dotnet run --project src/clicker.csproj -- seq 100 200 left 300 400 right 500 600 double
```

### 使用编译好的可执行文件

```powershell
.\output\ScreenClicker.exe click 500 300 left
.\output\ScreenClicker.exe move 100 200
```

### JSON 序列文件

创建 `actions.json`:

```json
[
  {"action": "move", "x": 500, "y": 300},
  {"action": "click", "x": 500, "y": 300, "button": "left"},
  {"action": "type", "text": "Hello World"},
  {"action": "scroll", "count": -3, "wait_ms": 500}
]
```

执行:

```powershell
dotnet run --project src/clicker.csproj -- --seq actions.json
```

### MCP Server 模式

```powershell
dotnet run --project src/clicker.csproj -- --mcp
```

---

## 🔌 注册为 Codex Desktop 插件

在 `~/.codex/config.toml` 中添加:

```toml
[mcp_servers."screen-clicker"]
command = "D:\\path\\to\\ScreenClicker.exe"
args = ["--mcp"]
```

重启 Codex Desktop 后即可在对话中调用 5 个 MCP 工具:
`click` / `move` / `type` / `scroll` / `drag`

---

## 🏗 架构

```
ScreenClicker.exe
├── Win32 API (user32.dll)
│   ├── mouse_event        鼠标点击/滚动/拖拽
│   ├── SetCursorPos       鼠标移动
│   ├── keybd_event        键盘输入
│   └── VkKeyScan          字符转虚拟键码
├── SequenceEngine         JSON 序列执行器
└── MCP Server (--mcp)     stdin/stdout JSON-RPC
```

---

## 🔗 与 Screen OCR 联动

Screen Clicker 与 [Screen OCR](https://github.com/REDrighthand05/screen-ocr) 配合使用可以实现完整的视觉自动化:

```python
# 伪代码: OCR 读屏 → Clicker 执行
ocr_result = run_ocr()           # 读屏幕文字
coord = find_text("下一步")       # 找目标按钮
clicker.click(coord.x, coord.y)  # 点击按钮
```

---

## 📄 License

MIT