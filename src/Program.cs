using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

class Program
{
    [DllImport("user32.dll")]
    static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, IntPtr dwExtraInfo);
    
    [DllImport("user32.dll")]
    static extern bool SetCursorPos(int x, int y);
    
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
    
    [DllImport("user32.dll")]
    static extern bool keybd_event(byte bVk, byte bScan, uint dwFlags, IntPtr dwExtraInfo);
    
    [DllImport("user32.dll")]
    static extern short VkKeyScan(char ch);
    
    const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    const uint MOUSEEVENTF_LEFTUP = 0x0004;
    const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    const uint MOUSEEVENTF_RIGHTUP = 0x0010;
    const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
    const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
    const uint MOUSEEVENTF_WHEEL = 0x0800;
    const uint KEYEVENTF_KEYDOWN = 0x0000;
    const uint KEYEVENTF_KEYUP = 0x0002;

    static void Main(string[] args)
    {
        if (args.Length == 0 || args[0] == "--help")
        {
            Console.WriteLine("ScreenClicker.exe — 鼠标/键盘模拟工具");
            Console.WriteLine("  click <x> <y> <left|right|middle|double>");
            Console.WriteLine("  move <x> <y>");
            Console.WriteLine("  type \"<text>\"");
            Console.WriteLine("  scroll <count>");
            Console.WriteLine("  drag <x1> <y1> <x2> <y2>");
            Console.WriteLine("  seq <x1> <y1> <c1> [<x2> <y2> <c2> ...]");
            return;
        }

        try
        {
            switch (args[0].ToLower())
            {
                case "click":
                    if (args.Length >= 4)
                    {
                        int x = int.Parse(args[1]), y = int.Parse(args[2]);
                        SetCursorPos(x, y);
                        System.Threading.Thread.Sleep(50);
                        string btn = args[3].ToLower();
                        MouseClick(btn);
                        Console.WriteLine($"[OK] {btn} at ({x},{y})");
                    }
                    break;
                    
                case "move":
                    if (args.Length >= 3)
                    {
                        SetCursorPos(int.Parse(args[1]), int.Parse(args[2]));
                        Console.WriteLine($"[OK] Move to ({args[1]},{args[2]})");
                    }
                    break;
                    
                case "type":
                    if (args.Length >= 2)
                    {
                        System.Threading.Thread.Sleep(200);
                        SendKeys(args[1]);
                        Console.WriteLine($"[OK] Typed: \"{args[1]}\"");
                    }
                    break;
                    
                case "scroll":
                    if (args.Length >= 2)
                    {
                        int count = int.Parse(args[1]);
                        mouse_event(MOUSEEVENTF_WHEEL, 0, 0, (uint)(count * 120), IntPtr.Zero);
                        Console.WriteLine($"[OK] Scroll {count}");
                    }
                    break;
                    
                case "drag":
                    if (args.Length >= 5)
                    {
                        int x1 = int.Parse(args[1]), y1 = int.Parse(args[2]);
                        int x2 = int.Parse(args[3]), y2 = int.Parse(args[4]);
                        SetCursorPos(x1, y1);
                        System.Threading.Thread.Sleep(100);
                        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, IntPtr.Zero);
                        for (float t = 0; t <= 1; t += 0.05f)
                            SetCursorPos((int)(x1 + (x2 - x1) * t), (int)(y1 + (y2 - y1) * t));
                        System.Threading.Thread.Sleep(50);
                        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, IntPtr.Zero);
                        Console.WriteLine($"[OK] Drag ({x1},{y1})->({x2},{y2})");
                    }
                    break;

                case "seq":
                    if (args.Length >= 4 && (args.Length - 1) % 3 == 0)
                    {
                        for (int i = 1; i < args.Length; i += 3)
                        {
                            int x = int.Parse(args[i]), y = int.Parse(args[i + 1]);
                            string btn = args[i + 2].ToLower();
                            SetCursorPos(x, y);
                            System.Threading.Thread.Sleep(80);
                            MouseClick(btn);
                            Console.WriteLine($"  [{btn}] ({x},{y})");
                            System.Threading.Thread.Sleep(100);
                        }
                        Console.WriteLine($"[OK] Sequence complete");
                    }
                    break;
                    
                default:
                    Console.WriteLine($"[FAIL] Unknown command: {args[0]}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FAIL] {ex.GetType().Name}: {ex.Message}");
        }
    }

    static void MouseClick(string button)
    {
        uint down, up;
        switch (button)
        {
            case "left": down = MOUSEEVENTF_LEFTDOWN; up = MOUSEEVENTF_LEFTUP; break;
            case "right": down = MOUSEEVENTF_RIGHTDOWN; up = MOUSEEVENTF_RIGHTUP; break;
            case "middle": down = MOUSEEVENTF_MIDDLEDOWN; up = MOUSEEVENTF_MIDDLEUP; break;
            case "double":
                MouseClick("left"); System.Threading.Thread.Sleep(30); MouseClick("left");
                return;
            default: return;
        }
        mouse_event(down, 0, 0, 0, IntPtr.Zero);
        System.Threading.Thread.Sleep(30);
        mouse_event(up, 0, 0, 0, IntPtr.Zero);
    }

    static void SendKeys(string text)
    {
        foreach (char ch in text)
        {
            short vk = VkKeyScan(ch);
            if (vk == -1) continue;
            byte vkCode = (byte)(vk & 0xFF);
            byte shift = (byte)((vk >> 8) & 0xFF);
            
            if ((shift & 1) != 0) keybd_event(0x10, 0, KEYEVENTF_KEYDOWN, IntPtr.Zero);
            keybd_event(vkCode, 0, KEYEVENTF_KEYDOWN, IntPtr.Zero);
            System.Threading.Thread.Sleep(10);
            keybd_event(vkCode, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
            if ((shift & 1) != 0) keybd_event(0x10, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
        }
    }
}
