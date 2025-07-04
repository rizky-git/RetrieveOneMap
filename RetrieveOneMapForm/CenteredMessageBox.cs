using System.Runtime.InteropServices;

namespace CenteredMessageBox
{
    public static class CenteredMessageBox
    {
        public static DialogResult Show(Form parent, string text, string caption,
            MessageBoxButtons buttons = MessageBoxButtons.OK,
            MessageBoxIcon icon = MessageBoxIcon.None)
        {
            using (var hook = new MessageBoxCenteringHook(parent))
            {
                return MessageBox.Show(parent, text, caption, buttons, icon);
            }
        }
    }

    internal class MessageBoxCenteringHook : IDisposable
    {
        private const int WH_CBT = 5;
        private const int HCBT_ACTIVATE = 5;

        private IntPtr _hHook = IntPtr.Zero;
        private readonly Form _parentForm;
        private HookProc _hookProc;

        public MessageBoxCenteringHook(Form parent)
        {
            _parentForm = parent ?? throw new ArgumentNullException(nameof(parent));
            _hookProc = new HookProc(CenterWindow);
            _hHook = SetWindowsHookEx(WH_CBT, _hookProc, IntPtr.Zero, GetCurrentThreadId());
        }

        public void Dispose()
        {
            if (_hHook != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hHook);
                _hHook = IntPtr.Zero;
            }
        }

        private IntPtr CenterWindow(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode == HCBT_ACTIVATE)
            {
                RECT msgBoxRect = new RECT();
                GetWindowRect(wParam, ref msgBoxRect);

                int msgBoxWidth = msgBoxRect.Right - msgBoxRect.Left;
                int msgBoxHeight = msgBoxRect.Bottom - msgBoxRect.Top;

                var parentRect = _parentForm.Bounds;

                int x = parentRect.Left + (parentRect.Width - msgBoxWidth) / 2;
                int y = parentRect.Top + (parentRect.Height - msgBoxHeight) / 2;

                MoveWindow(wParam, x, y, msgBoxWidth, msgBoxHeight, false);
                UnhookWindowsHookEx(_hHook);
                _hHook = IntPtr.Zero;
            }

            return CallNextHookEx(_hHook, nCode, wParam, lParam);
        }

        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}
