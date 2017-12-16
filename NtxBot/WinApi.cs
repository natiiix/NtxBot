using System;
using System.Runtime.InteropServices;

namespace NtxBot
{
    public static class WinApi
    {
        // Get the focused window.
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();

        // Send a message to a specific process via the handle.
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        //// Places (posts) a message in the message queue associated with the thread that created
        //// the specified window and returns without waiting for the thread to process the message.
        //[DllImport("user32.dll", CharSet = CharSet.Auto)]
        //public static extern IntPtr PostMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
    }
}