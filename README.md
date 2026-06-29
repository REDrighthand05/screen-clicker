# 6.29 隔离发展 — Toolchain

## 目录

`
6.29隔离发展/
├── ocr/       ScreenOcr v0.2.0 — 屏幕文字识别
│   └── output/ScreenOcr.exe / ScreenOcrMcp.exe
├── clicker/   ScreenClicker v0.2.0 — 鼠标/键盘模拟  
│   └── output/ScreenClicker.exe
└── panel/     Web 仪表盘
    └── src/server.js — Node.js :9617
`

## 使用

`powershell
# OCR
ScreenOcr.exe                          # 全屏OCR
ScreenOcr.exe --region 100,200,400,300 # 区域OCR
ScreenOcr.exe --compact                # 纯文本
ScreenOcr.exe --every 5000             # 每5秒扫描
ScreenOcr.exe --mcp                    # MCP Server模式

# Clicker
ScreenClicker.exe click 500 300 left   # 点击
ScreenClicker.exe move 100 200         # 移动  
ScreenClicker.exe type "hello"         # 打字
ScreenClicker.exe --seq actions.json   # 序列执行
ScreenClicker.exe --mcp                # MCP Server模式

# Panel  
node server.js                         # 启动Web仪表盘
# 访问 http://127.0.0.1:9617
`

## 架构

OCR 和 Clicker 都支持 MCP 协议，可以通过 stdio JSON-RPC 被 Codex 调用。
Panel 读取工具状态和 relay 日志，展示为 Web 仪表盘。