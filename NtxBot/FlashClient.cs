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
        private KeyWrapper keyUp;
        private KeyWrapper keyLeft;
        private KeyWrapper keyDown;
        private KeyWrapper keyRight;

        public bool Up { set => keyUp.Pressed = value; }
        public bool Left { set => keyLeft.Pressed = value; }
        public bool Down { set => keyDown.Pressed = value; }
        public bool Right { set => keyRight.Pressed = value; }

        public FlashClient()
        {
            flashPtr = WinApi.GetForegroundWindow();

            keyUp = new KeyWrapper(x => SendKeyEventToFlash(Keys.W, x));
            keyLeft = new KeyWrapper(x => SendKeyEventToFlash(Keys.A, x));
            keyDown = new KeyWrapper(x => SendKeyEventToFlash(Keys.S, x));
            keyRight = new KeyWrapper(x => SendKeyEventToFlash(Keys.D, x));
        }

        public void StopMovement()
        {
            Up = Left = Down = Right = false;
        }

        public void UseAbility()
        {
            const Keys KEY_ABILITY = Keys.Space;

            SendKeyEventToFlash(KEY_ABILITY, WM_KEYDOWN);
            SendKeyEventToFlash(KEY_ABILITY, WM_KEYUP);
        }

        private void SendKeyEventToFlash(Keys key, uint keyEvent)
        {
            // PostMessage causes the Flash window to freeze
            // SendMessage doesn't seem to suffer from this problem

            // Send a message about the key event to the Flash window via Windows API
            WinApi.SendMessage(flashPtr, keyEvent, (IntPtr)key, IntPtr.Zero);
        }
    }
}