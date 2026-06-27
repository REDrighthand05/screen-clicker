# Screen Clicker

让纯文本大模型拥有鼠标键盘。

配合 Screen OCR 使用：OCR 读屏 - 理解界面 - Clicker 操作 - OCR 验证。
不需要多模态模型，不需要浏览器自动化框架。

```powershell
ScreenClicker.exe click 500 300 left
ScreenClicker.exe type "Hello World"
ScreenClicker.exe seq 100 200 left 300 400 double
ScreenClicker.exe drag 100 100 300 300
ScreenClicker.exe scroll -3
```

配套 OCR：https://github.com/REDrighthand05/screen-ocr

## 技术栈
C# (.NET 8.0) | user32.dll SendInput | 零外部依赖 | Windows 10/11

## License
MIT
