using System;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace YSAutoPlayer
{
    public sealed class HotKey : IDisposable
    {
        public event Action<HotKey>? HotKeyPressed;

        private bool _isKeyRegistered;
        private readonly int _id;
        private readonly IntPtr _handle;
        private ushort _key;

        public HotKey(ushort key, IntPtr handle)
        {
            _id = GetHashCode();
            _handle = handle;
            ComponentDispatcher.ThreadPreprocessMessage += ThreadPreprocessMessageMethod;
            Key = key;
        }

        ~HotKey()
        {
            Dispose();
        }

        public ushort Key
        {
            get => _key; set
            {
                _key = value;
                RegisterHotKey(_handle);
            }
        }

        #region Windows API

        public const int WmHotKey = 0x0312;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, ModifierKeys fsModifiers, ushort vk);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        #endregion Windows API

        public void RegisterHotKey(IntPtr handle)
        {
            Contract.Requires(handle != IntPtr.Zero);
            if (Key == 0)
                return;
            if (_isKeyRegistered)
                UnregisterHotKey();
            _isKeyRegistered = RegisterHotKey(_handle, _id, ModifierKeys.None, Key);
            if (!_isKeyRegistered)
                throw new ApplicationException("该热键已被占用，请尝试更换其他键");
        }

        public void UnregisterHotKey()
        {
            _isKeyRegistered = !UnregisterHotKey(_handle, _id);
        }

        public void Dispose()
        {
            ComponentDispatcher.ThreadPreprocessMessage -= ThreadPreprocessMessageMethod;
            UnregisterHotKey();
        }

        private void ThreadPreprocessMessageMethod(ref MSG msg, ref bool handled)
        {
            if (!handled)
            {
                if (msg.message == WmHotKey
                    && (int)(msg.wParam) == _id)
                {
                    HotKeyPressed?.Invoke(this);
                    handled = true;
                }
            }
        }
    }
}
