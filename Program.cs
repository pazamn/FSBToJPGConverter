using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FsbJpgConverter
{
    /// <summary>
    /// To decode .FSB files FSBReader should be installed and assotiated with .FSB files
    /// </summary>
    public class Program
    {
        private const int Timeout = 400;
        private const bool ChangePassword = false;

        private const uint WmLbuttondown = 0x201;
        private const uint WmLbuttonup = 0x202;
        private const uint GetNextWindowCommand = 2;

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className,  string  windowTitle);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
        
        public static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Hello, World!");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Enter path with fsb files:");

            Console.ForegroundColor = ConsoleColor.Gray;
            var directoryPath = Console.ReadLine();
            if (directoryPath == null)
            {
                return;
            }

            var files = Directory.GetFiles(directoryPath, "*.fsb").ToList();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("Found " + files.Count + " file(s) to convert.");
            Console.WriteLine();

            foreach (var file in files)
            {
                Decode(file);

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("    Done: " + Path.GetFileName(file) + " at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static void Decode(string path)
        {
            try
            {
                if (ChangePassword)
                {
                    var bytes = File.ReadAllBytes(path);

                    var replaceWhat = new byte[] { 6, 0, 0, 0, 4,  0, 0, 0, 28,  0, 0, 0, 107, 0, 0, 0, 28,  0, 0, 0, 80, 0, 0, 0, 129, 0, 0, 0 };
                    var replaceWhom = new byte[] { 6, 0, 0, 0, 32, 0, 0, 0, 139, 0, 0, 0, 9,   0, 0, 0, 139, 0, 0, 0, 26, 0, 0, 0, 155, 0, 0, 0 };

                    var x3 = bytes.Skip(replaceWhom.Count()).ToList();

                    var x4 = new List<byte>();
                    x4.AddRange(replaceWhom);
                    x4.AddRange(x3);

                    File.WriteAllBytes(path, x4.ToArray());
                }

                var process = Process.Start(path);
                if (process == null)
                {
                    throw new Exception("Process is null.");
                }

                Thread.Sleep(Timeout);
                var messageBoxHandle = FindWindowByCaption(IntPtr.Zero, "Программа просмотра изображений FSB");
                var okButtonHandle = FindWindowEx(messageBoxHandle, IntPtr.Zero, "Button", "OK");

                SendMessage(okButtonHandle, WmLbuttondown, IntPtr.Zero, IntPtr.Zero);
                SendMessage(okButtonHandle, WmLbuttonup, IntPtr.Zero, IntPtr.Zero);
                SendMessage(okButtonHandle, WmLbuttondown, IntPtr.Zero, IntPtr.Zero);
                SendMessage(okButtonHandle, WmLbuttonup, IntPtr.Zero, IntPtr.Zero);

                Thread.Sleep(Timeout);
                var passwordWindowHandle = FindWindowByCaption(IntPtr.Zero, "HI!");
                var passwordPanelHandle = FindWindowEx(passwordWindowHandle, IntPtr.Zero, "TPanel", "");
                var passwordEditBox = FindWindowEx(passwordPanelHandle, IntPtr.Zero, "TRzEdit", "");
                var passwordOkButtonHandle = FindWindowEx(passwordPanelHandle, IntPtr.Zero, "TRzButton", "OK");
                SetForegroundWindow(passwordEditBox);

                Thread.Sleep(Timeout);
                SendKeys.SendWait("InLoad");
                SendMessage(passwordOkButtonHandle, WmLbuttondown, IntPtr.Zero, IntPtr.Zero);
                SendMessage(passwordOkButtonHandle, WmLbuttonup, IntPtr.Zero, IntPtr.Zero);

                Thread.Sleep(Timeout);
                var mainWindow = FindWindowByCaption(IntPtr.Zero, path + " - FSBReader 1.3");
                var picturePanel = FindWindowEx(mainWindow, IntPtr.Zero, "TPanel", "");
                var wideToolbarPanel = GetWindow(picturePanel, GetNextWindowCommand);
                var toolbarPanel = FindWindowEx(wideToolbarPanel, IntPtr.Zero, "TPanel", "");

                Action openSaveDialogAction = delegate
                    {
                        SetForegroundWindow(toolbarPanel);
                        SendMessage(toolbarPanel, WmLbuttondown, IntPtr.Zero, (IntPtr)((17 << 16) | 184));
                        SendMessage(toolbarPanel, WmLbuttonup, IntPtr.Zero, (IntPtr)((17 << 16) | 184));
                    };

                Thread.Sleep(Timeout);
                Task.Factory.StartNew(openSaveDialogAction);

                Thread.Sleep(Timeout);
                var saveDialog = FindWindowByCaption(IntPtr.Zero, "Save As");
                var saveButton = FindWindowEx(saveDialog, IntPtr.Zero, "Button", "&Save");

                SendMessage(saveButton, WmLbuttondown, IntPtr.Zero, IntPtr.Zero);
                SendMessage(saveButton, WmLbuttonup, IntPtr.Zero, IntPtr.Zero);

                process.Kill();
                var i = 0;
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}