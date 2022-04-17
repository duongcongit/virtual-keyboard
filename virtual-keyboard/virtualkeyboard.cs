using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace virtual_keyboard
{
    public partial class virtualkeyboard : Form
    {
        // Const for SetWindowPos()
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        // Import function SetWindowPos
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        // Import function SendInput
        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

        // Import function GetMessageExtraInfo
        [DllImport("user32.dll")]
        private static extern IntPtr GetMessageExtraInfo();


        public virtualkeyboard()
        {
            InitializeComponent();
        }

        private void virtualkeyboard_Load(object sender, EventArgs e)
        {
            // Set size
            this.Size = new Size(1141, 435);

            // Call function SetWindowPos
            // Virtual keyboard will Always on top (over all program)
            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);

            // Set positon of layout 2
            panel2.Left = panel1.Left;
            panel2.Top = panel1.Bottom;
            
        }

        // Overide method
        // Windows will not focus to virtual keyboard when typing
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams param = base.CreateParams;
                param.ExStyle |= 0x08000000;
                return param;
            }
        }


        // ==============

        [StructLayout(LayoutKind.Sequential)]
        public struct KeyboardInput
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        //

        [StructLayout(LayoutKind.Sequential)]
        public struct MouseInput
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        //

        [StructLayout(LayoutKind.Sequential)]
        public struct HardwareInput
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        //

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)] public MouseInput mi;
            [FieldOffset(0)] public KeyboardInput ki;
            [FieldOffset(0)] public HardwareInput hi;
        }

        //

        public struct Input
        {
            public int type;
            public InputUnion u;
        }

        [Flags]
        public enum InputType
        {
            Mouse = 0,
            Keyboard = 1,
            Hardware = 2
        }

        //

        [Flags]
        public enum KeyEventF
        {
            KeyDown = 0x0000,
            ExtendedKey = 0x0001,
            KeyUp = 0x0002,
            Unicode = 0x0004,
            Scancode = 0x0008
        }

        //

        [Flags]
        public enum MouseEventF
        {
            Absolute = 0x8000,
            HWheel = 0x01000,
            Move = 0x0001,
            MoveNoCoalesce = 0x2000,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            VirtualDesk = 0x4000,
            Wheel = 0x0800,
            XDown = 0x0080,
            XUp = 0x0100
        }


        // Send key down
        public static void SendKeyDown(KeyCode keyCode)
        {
            Input[] inputs = new Input[]
                {
                    new Input
                    {
                        type = (int)InputType.Keyboard,
                        u = new InputUnion
                        {
                            ki = new KeyboardInput
                            {
                                wVk = (ushort)keyCode,
                                wScan = 0,
                                dwFlags = 0, // 0 = Key down
                                dwExtraInfo = GetMessageExtraInfo()
                            }
                        }
                    }
                 };
            //
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
        }


        // Send key up
        public static void SendKeyUP(KeyCode keyCode)
        {
            Input[] inputs = new Input[]
                {
                    new Input
                    {
                        type = (int)InputType.Keyboard,
                        u = new InputUnion
                        {
                            ki = new KeyboardInput
                            {
                                wVk = (ushort)keyCode,
                                wScan = 0,
                                dwFlags = 2, // 2 = Key up
                                dwExtraInfo = GetMessageExtraInfo()
                            }
                        }
                    }
                 };
            //
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
        }


        // Switch layout 2 to layout 1 (main)
        private void Switch_Layout_To_Main(object sender, EventArgs e)
        {
            panel2.Left = panel1.Left;
            panel2.Top = panel1.Bottom;
        }

        // Switch to layout 2
        private void Switch_Layout_To_L2(object sender, EventArgs e)
        {
            panel2.Left = panel1.Left;
            panel2.Top = panel1.Top;            
        }



        // PROCESS KEYS EVENT

        // For A-Z, 0-9 keys
        private void btn_char_Click(object sender, EventArgs e)
        {
            Control btn = (Button)sender;
            KeyCode key;
            Enum.TryParse<KeyCode>(btn.Name.ToString(), out key);

            // Send key
            SendKeyDown(key);
            SendKeyUP(key);

            // Reset all special key to original state
            Reset_Special_Key();

        }


        // For Function Keys F1-F12
        private void FunctionKey_Click(object sender, EventArgs e)
        {
            Control btn = (Button)sender;
            KeyCode key;
            Enum.TryParse<KeyCode>(btn.Name.ToString(), out key);

            // Send key
            SendKeyDown(key);
            SendKeyUP(key);

            // Reset all special key to original state
            Reset_Special_Key();
        }


        /* For PrtSc, ScrLk, Pause, Insert, Home, Page Up, Page Down, Delete, End
           Up, Down, Left, Right Keys */
        private void Navigation_Key(object sender, EventArgs e)
        {
            Control btn = (Button)sender;
            KeyCode key;
            Enum.TryParse<KeyCode>(btn.Name.ToString(), out key);

            //
            SendKeyDown(key);
            SendKeyUP(key);
            //
        }


        // For ESC, TAB, SPACE BAR, ENTER, BACKSPACE keys
        private void Special_Key_Click(object sender, EventArgs e)
        {
            Control btn = (Button)sender;
            KeyCode key;
            Enum.TryParse<KeyCode>(btn.Tag.ToString(), out key);

            // Send Key
            SendKeyDown(key);
            SendKeyUP(key);

            // Reset all special key to original state
            Reset_Special_Key();
        }


        // For Left Shift and Right Shift key
        private void shift_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            KeyCode key;
            // Get Tag of key and change to Keycode type
            Enum.TryParse<KeyCode>(btn.Tag.ToString(), out key);

            // If Shift key is pressing, send key up
            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                SendKeyUP(key);

                // Set all character keys to lowcase
                foreach (Control control in panel1.Controls)
                {
                    if (control.TabIndex == 1)
                    {
                        control.Text = control.Text.ToLower();
                    }
                }

                // Reset color of button
                btn.BackColor = Color.White;
                btn.ForeColor = Color.Black;
            }
            // If Shift key is not pressing, send key down
            else
            {
                SendKeyDown(key);

                // Set all character keys to upcase
                foreach (Control control in panel1.Controls)
                {
                    if (control.TabIndex == 1)
                    {
                        control.Text = control.Text.ToUpper();
                    }
                }

                // Set color for button
                btn.BackColor = Color.RoyalBlue;
                btn.ForeColor = Color.White;


            }
        }


        // For Caps Lock key
        private void CAPS_LOCK_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            KeyCode key = KeyCode.CAPS_LOCK;
                    
            // If Caps lock key is pressing, send key up
            if (Control.IsKeyLocked(Keys.CapsLock))
            {
                // Set all character keys to lowcase
                foreach (Control control in panel1.Controls)
                {
                    if (control.TabIndex == 1)
                    {
                        control.Text = control.Text.ToLower();
                    }
                }

                // Reset color of button
                btn.BackColor = Color.White;
                btn.ForeColor = Color.Black;
            }
            // If Caps lock key is not pressing, send key down
            else
            {
                // Set all character keys to upcase
                foreach (Control control in panel1.Controls)
                {
                    if (control.TabIndex == 1)
                    {
                        control.Text = control.Text.ToUpper();
                    }
                }

                // Set color for button
                btn.BackColor = Color.RoyalBlue;
                btn.ForeColor = Color.White;


            }

            SendKeyDown(key);
            SendKeyUP(key);
        }

        //


        // For Left Control and Right Control key
        private void CONTROL_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            KeyCode key;
            // Get Tag of key and change to Keycode type
            Enum.TryParse<KeyCode>(btn.Tag.ToString(), out key);

            // If Control key is pressing, send key up
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                SendKeyUP(key);

                // Reset color of button
                btn.BackColor = Color.White;
                btn.ForeColor = Color.Black;
            }
            // If Control key is not pressing, send key down
            else
            {
                SendKeyDown(key);

                // Set color for button
                btn.BackColor = Color.RoyalBlue;
                btn.ForeColor = Color.White;
            }
        }


        // For Left Alt and Right Alt key
        private void ALT_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            KeyCode key = KeyCode.ALT;
            // Enum.TryParse<KeyCode>(btn.Name.ToString(), out key);

            // If Alt key is pressing, send key up
            if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
            {
                SendKeyUP(key);

                // Reset color of button
                btn.BackColor = Color.White;
                btn.ForeColor = Color.Black;
            }
            // If Alt key is not pressing, send key down
            else
            {
                SendKeyDown(key);

                // Set color for button
                btn.BackColor = Color.RoyalBlue;
                btn.ForeColor = Color.White;
            }

        }



        // Reset all special key to original state (CAPSLOCK, SHIFT, CONTROL, ALT WINDOW KEY)
        public void Reset_Special_Key()
        {
            
            if((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                SendKeyUP(KeyCode.SHIFT);
                LSHIFT.BackColor = Color.White;
                LSHIFT.ForeColor = Color.Black;
            }
            if((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                SendKeyUP(KeyCode.CONTROL);
                LCONTROL.BackColor = Color.White;
                LCONTROL.ForeColor = Color.Black;
            }
            if((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
            {
                SendKeyUP(KeyCode.ALT);
                LALT.BackColor = Color.White;
                LALT.ForeColor = Color.Black;
            }

            //

            foreach (Control control in panel1.Controls)
            {
                if (control.TabIndex == 1)
                {
                    control.Text = control.Text.ToLower();
                }

            }
        }


        // Keys code

        public enum KeyCode : ushort
        {
            // Next track if a song is playing
            MEDIA_NEXT_TRACK = 0xb0,
                        
            // Play pause            
            MEDIA_PLAY_PAUSE = 0xb3,
                        
            // Previous track            
            MEDIA_PREV_TRACK = 0xb1,
            
            // Stop            
            MEDIA_STOP = 0xb2,

            
            // Key "+"            
            ADD = 0x6b,
            
            // "*" key            
            MULTIPLY = 0x6a,
            
            // "/" key            
            DIVIDE = 0x6f,
            
            // Subtract key "-"            
            SUBTRACT = 0x6d,

            
            // Go Back            
            BROWSER_BACK = 0xa6,
            
            // Favorites            
            BROWSER_FAVORITES = 0xab,
            
            // Forward            
            BROWSER_FORWARD = 0xa7,
            
            // Home            
            BROWSER_HOME = 0xac,
            
            // Refresh            
            BROWSER_REFRESH = 0xa8,
            
            // browser search            
            BROWSER_SEARCH = 170,
            
            // Stop            
            BROWSER_STOP = 0xa9,

            
            //             
            NUMPAD0 = 0x60,
            
            //             
            NUMPAD1 = 0x61,
            
            //             
            NUMPAD2 = 0x62,
            
            //             
            NUMPAD3 = 0x63,
            
            //             
            NUMPAD4 = 100,
            
            //             
            NUMPAD5 = 0x65,
            
            //             
            NUMPAD6 = 0x66,
            
            //             
            NUMPAD7 = 0x67,
            
            //             
            NUMPAD8 = 0x68,
            
            //             
            NUMPAD9 = 0x69,


            
            // F1            
            F1 = 0x70,
            
            // F10            
            F10 = 0x79,
            
            //             
            F11 = 0x7a,
            
            //             
            F12 = 0x7b,
            
            //             
            F13 = 0x7c,
            
            //             
            F14 = 0x7d,
            
            //             
            F15 = 0x7e,
            
            //             
            F16 = 0x7f,
            
            //             
            F17 = 0x80,
            
            //             
            F18 = 0x81,
            
            //             
            F19 = 130,
            
            //             
            F2 = 0x71,
            
            //             
            F20 = 0x83,
            
            //             
            F21 = 0x84,
            
            //             
            F22 = 0x85,
            
            //             
            F23 = 0x86,
            
            //             
            F24 = 0x87,
            
            //             
            F3 = 0x72,
            
            //             
            F4 = 0x73,
            
            //             
            F5 = 0x74,
            
            //             
            F6 = 0x75,
            
            //             
            F7 = 0x76,
            
            //             
            F8 = 0x77,
            
            //             
            F9 = 120,


            
            //             
            OEM_1 = 0xba,
            
            //             
            OEM_102 = 0xe2,
            
            //             
            OEM_2 = 0xbf,
            
            //             
            OEM_3 = 0xc0,
            
            //             
            OEM_4 = 0xdb,
            
            //             
            OEM_5 = 220,
            
            //             
            OEM_6 = 0xdd,
            
            //             
            OEM_7 = 0xde,
            
            //             
            OEM_8 = 0xdf,
            
            //             
            OEM_CLEAR = 0xfe,
            
            //             
            OEM_COMMA = 0xbc,
            
            //             
            OEM_MINUS = 0xbd,
            
            //             
            OEM_PERIOD = 190,
            
            //             
            OEM_PLUS = 0xbb,

                        
            //             
            KEY_0 = 0x30,
            
            //             
            KEY_1 = 0x31,
            
            //             
            KEY_2 = 50,
            
            //             
            KEY_3 = 0x33,
            
            //             
            KEY_4 = 0x34,
            
            //             
            KEY_5 = 0x35,
            
            //             
            KEY_6 = 0x36,
            
            //             
            KEY_7 = 0x37,
            
            //             
            KEY_8 = 0x38,
            
            //             
            KEY_9 = 0x39,
            
            //             
            KEY_A = 0x41,
            
            //             
            KEY_B = 0x42,
            
            //             
            KEY_C = 0x43,
            
            //             
            KEY_D = 0x44,
            
            //             
            KEY_E = 0x45,
            
            //             
            KEY_F = 70,
            
            //             
            KEY_G = 0x47,
            
            //             
            KEY_H = 0x48,
            
            //             
            KEY_I = 0x49,
            
            //             
            KEY_J = 0x4a,
            
            //             
            KEY_K = 0x4b,
            
            //             
            KEY_L = 0x4c,
            
            //             
            KEY_M = 0x4d,
            
            //             
            KEY_N = 0x4e,
            
            //             
            KEY_O = 0x4f,
            
            //             
            KEY_P = 80,
            
            //             
            KEY_Q = 0x51,
            
            //             
            KEY_R = 0x52,
            
            //             
            KEY_S = 0x53,
            
            //             
            KEY_T = 0x54,
            
            //             
            KEY_U = 0x55,
            
            //             
            KEY_V = 0x56,
            
            //             
            KEY_W = 0x57,
            
            //             
            KEY_X = 0x58,
            
            //             
            KEY_Y = 0x59,
            
            //             
            KEY_Z = 90,

            
            // Decrese volume            
            VOLUME_DOWN = 0xae,

            
            // Mute volume            
            VOLUME_MUTE = 0xad,

            
            // Increase volue            
            VOLUME_UP = 0xaf,


            
            // Take snapshot of the screen and place it on the clipboard            
            SNAPSHOT = 0x2c,

            // Send right click from keyboard
            RightClick = 0x5d,

            
            // Go Back or delete            
            BACKSPACE = 8,

            
            // Control + Break "When debuging if you step into an infinite loop this will stop debug"            
            CANCEL = 3,
            
            // Caps lock key to send cappital letters            
            CAPS_LOCK = 20,
            
            // Ctlr key            
            CONTROL = 0x11,

            
            // Alt key            
            ALT = 18,

            
            // "." key            
            DECIMAL = 110,

            
            // Delete Key            
            DELETE = 0x2e,

            
            // Arrow down key            
            DOWN = 40,

            
            // End key            
            END = 0x23,

            
            // Escape key            
            ESC = 0x1b,

            
            // Home key            
            HOME = 0x24,

            
            // Insert key            
            INSERT = 0x2d,

            
            // Open my computer            
            LAUNCH_APP1 = 0xb6,
            
            // Open calculator            
            LAUNCH_APP2 = 0xb7,

            
            // Open default email in my case outlook            
            LAUNCH_MAIL = 180,

            
            // Opend default media player (itunes, winmediaplayer, etc)            
            LAUNCH_MEDIA_SELECT = 0xb5,

            
            // Left control            
            LCONTROL = 0xa2,

            
            // Left arrow            
            LEFT = 0x25,

            
            // Left shift            
            LSHIFT = 160,

            
            // left windows key            
            LWIN = 0x5b,

            //
            SCROLL_LOCK = 0x91,

            //
            PAUSE_BREAK = 0x13,
            
            // Next "page down"            
            PAGEDOWN = 0x22,
            
            // Num lock to enable typing numbers            
            NUMLOCK = 0x90,
            
            // Page up key            
            PAGE_UP = 0x21,
            
            // Right control            
            RCONTROL = 0xa3,
            
            // Return key            
            ENTER = 13,
            
            // Right arrow key            
            RIGHT = 0x27,
            
            // Right shift            
            RSHIFT = 0xa1,
            
            // Right windows key            
            RWIN = 0x5c,
            
            // Shift key            
            SHIFT = 0x10,
            
            // Space back key            
            SPACE_BAR = 0x20,
            
            // Tab key            
            TAB = 9,
            
            // Up arrow key            
            UP = 0x26,

        }

        // 
    }




        
}
