using System;
using System.Windows.Forms;

namespace NtxBot
{
    public class FlashClient
    {
        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;

        private class KeyWrapper
        {
            private bool pressed;
            private Action<uint> sendKeyEventToFlash;

            public bool Pressed
            {
                get => pressed;

                set
                {
                    if (value != pressed)
                    {
                        // Update state
                        pressed = value;

                        // Send the new state to the Flash window
                        sendKeyEventToFlash(KeyStateToEvent(pressed));
                    }
                }
            }

            public KeyWrapper(Action<uint> sendKeyEventToFlash)
            {
                pressed = false;
                this.sendKeyEventToFlash = sendKeyEventToFlash;
            }

            private uint KeyStateToEvent(bool state) => state ? WM_KEYDOWN : WM_KEYUP;
        }

        private IntPtr flashPtr;
        private KeyWrapper keyW;
        private KeyWrapper keyA;
        private KeyWrapper keyS;
        private KeyWrapper keyD;

        public bool W { set => keyW.Pressed = value; }
        public bool A { set => keyA.Pressed = value; }
        public bool S { set => keyS.Pressed = value; }
        public bool D { set => keyD.Pressed = value; }

        public FlashClient()
        {
            //flashPtr = GetFlashHandle();
            flashPtr = WinApi.GetForegroundWindow();

            keyW = new KeyWrapper(x => SendKeyEventToFlash(Keys.W, x));
            keyA = new KeyWrapper(x => SendKeyEventToFlash(Keys.A, x));
            keyS = new KeyWrapper(x => SendKeyEventToFlash(Keys.S, x));
            keyD = new KeyWrapper(x => SendKeyEventToFlash(Keys.D, x));
        }

        public void StopMovement()
        {
            W = A = S = D = false;
        }

        private void SendKeyEventToFlash(Keys key, uint keyEvent)
        {
            // PostMessages causes the Flash window to freeze
            //WinApi.PostMessage(flashPtr, keyEvent, (IntPtr)key, IntPtr.Zero);
            // SendMessage doesn't seem to suffer from this problem
            WinApi.SendMessage(flashPtr, keyEvent, (IntPtr)key, IntPtr.Zero);
        }

        //private static bool IsFlashProcess(Process proc) => proc.ProcessName.ToLower().StartsWith("flashplayer");

        //private static IntPtr GetFlashHandle() => Process.GetProcesses().Single(IsFlashProcess).MainWindowHandle;
    }
}