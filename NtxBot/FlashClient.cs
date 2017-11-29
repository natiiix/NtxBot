using System;

namespace NtxBot
{
    public class FlashClient
    {
        private enum KeyCode : int
        {
            W = 0x57,
            A = 0x41,
            S = 0x53,
            D = 0x44
        }

        private enum KeyEvent : uint
        {
            KeyDown = 0x0100,
            KeyUp = 0x0101
        }

        private class KeyWrapper
        {
            private bool pressed;
            private Action<KeyEvent> sendKeyEventToFlash;

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

            public KeyWrapper(Action<KeyEvent> sendKeyEventToFlash)
            {
                pressed = false;
                this.sendKeyEventToFlash = sendKeyEventToFlash;
            }

            private KeyEvent KeyStateToEvent(bool state) => state ? KeyEvent.KeyDown : KeyEvent.KeyUp;
        }

        private IntPtr ptr;
        private KeyWrapper keyW;
        private KeyWrapper keyA;
        private KeyWrapper keyS;
        private KeyWrapper keyD;

        public FlashClient(IntPtr flashPtr)
        {
            ptr = flashPtr;

            keyW = new KeyWrapper(x => SendKeyEventToFlash(KeyCode.W, x));
            keyA = new KeyWrapper(x => SendKeyEventToFlash(KeyCode.A, x));
            keyS = new KeyWrapper(x => SendKeyEventToFlash(KeyCode.S, x));
            keyD = new KeyWrapper(x => SendKeyEventToFlash(KeyCode.D, x));
        }

        private void SendKeyEventToFlash(KeyCode keyCode, KeyEvent keyEvent)
        {
            WinApi.SendMessage(ptr, (uint)keyEvent, new IntPtr((int)keyCode), IntPtr.Zero);
        }
    }
}