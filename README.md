# Screen Clicker

让 AI agent 拥有鼠标键盘。纯本地、零依赖。

## 用法

`powershell
# 左键单击
ScreenClicker.exe click 500 300 left
# 输入文字
ScreenClicker.exe type "Hello World"
# 序列操作
ScreenClicker.exe seq 100 200 left 300 400 double
`

## 配合 Screen OCR

OCR 读屏 → 得到坐标 → Clicker 点击 → OCR 验证。

## 许可证 MIT
