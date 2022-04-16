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
        // Khai báo hằng số cho hàm SetWindowPos
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        // Import hàm SetWindowPos
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern IntPtr GetMessageExtraInfo();


        public virtualkeyboard()
        {
            InitializeComponent();
        }

        private void virtualkeyboard_Load(object sender, EventArgs e)
        {
            // Gọi hàm SetWindowPos
            // Bàn phím ảo sẽ luôn ở trên các chương trình khác trên màn hình
            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);

            panel2.Left = panel1.Left;
            panel2.Top = panel1.Bottom;
        }

        // Ghi đè phương thức
        // Khi thao tác trên bàn phím (click) thì vẫn focus ở ô nhận liệu
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


        // ===================

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
                                dwFlags = 0,
                                dwExtraInfo = GetMessageExtraInfo()
                            }
                        }
                    }
                 };
            //
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
        }

        //

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
                                dwFlags = 2,
                                dwExtraInfo = GetMessageExtraInfo()
                            }
                        }
                    }
                 };
            //
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
        }


        // ===================
        private void Switch_Layout_To_Main(object sender, EventArgs e)
        {
            panel2.Left = panel1.Left;
            panel2.Top = panel1.Bottom;
        }

        private void Switch_Layout_To_L2(object sender, EventArgs e)
        {
            panel2.Left = panel1.Left;
            panel2.Top = panel1.Top;            
        }



        // ===================

        private void btn_char_Click(object sender, EventArgs e)
        {
            Control btn = (Button)sender;
            KeyCode key;
            Enum.TryParse<KeyCode>(btn.Name.ToString(), out key);

            //
            SendKeyDown(key);
            SendKeyUP(key);
            //
            Reset_Special_Key();

        }

        private void FunctionKey_Click(object sender, EventArgs e)
        {
            Control btn = (Button)sender;
            KeyCode key;
            Enum.TryParse<KeyCode>(btn.Name.ToString(), out key);

            //
            SendKeyDown(key);
            SendKeyUP(key);
            //

            Reset_Special_Key();
        }

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

        private void shift_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            KeyCode key;
            Enum.TryParse<KeyCode>(btn.Name.ToString(), out key);

            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                SendKeyUP(key);
                foreach (Control control in panel1.Controls)
                {
                    if (control.TabIndex == 1)
                    {
                        control.Text = control.Text.ToLower();
                    }
                }
                //

                btn.BackColor = Color.White;
                btn.ForeColor = Color.Black;
            }
            else
            {
                SendKeyDown(key);
                foreach (Control control in panel1.Controls)
                {
                    if (control.TabIndex == 1)
                    {
                        control.Text = control.Text.ToUpper();
                    }
                }
                //
                btn.BackColor = Color.RoyalBlue;
                btn.ForeColor = Color.White;


            }
        }

        private void CAPS_LOCK_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            KeyCode key = KeyCode.CAPS_LOCK;

            if (Control.IsKeyLocked(Keys.CapsLock))
            {
                foreach (Control control in panel1.Controls)
                {
                    if (control.TabIndex == 1)
                    {
                        control.Text = control.Text.ToLower();
                    }
                }
                btn.BackColor = Color.White;
                btn.ForeColor = Color.Black;
            }
            else
            {
                foreach (Control control in panel1.Controls)
                {
                    if (control.TabIndex == 1)
                    {
                        control.Text = control.Text.ToUpper();
                    }
                }
                btn.BackColor = Color.RoyalBlue;
                btn.ForeColor = Color.White;
            }

            SendKeyDown(key);
            SendKeyUP(key);
        }

        //

        private void CONTROL_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            KeyCode key;
            Enum.TryParse<KeyCode>(btn.Name.ToString(), out key);

            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                SendKeyUP(key);

                btn.BackColor = Color.White;
                btn.ForeColor = Color.Black;
            }
            else
            {
                SendKeyDown(key);

                btn.BackColor = Color.RoyalBlue;
                btn.ForeColor = Color.White;

            }
        }

        private void ALT_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            KeyCode key = KeyCode.ALT;
            // Enum.TryParse<KeyCode>(btn.Name.ToString(), out key);

            if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
            {
                SendKeyUP(key);

                btn.BackColor = Color.White;
                btn.ForeColor = Color.Black;
            }
            else
            {
                SendKeyDown(key);

                btn.BackColor = Color.RoyalBlue;
                btn.ForeColor = Color.White;

            }

        }

        //
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


        // =========

        public enum KeyCode : ushort
        {

            /// <summary>
            /// Next track if a song is playing
            /// </summary>
            MEDIA_NEXT_TRACK = 0xb0,

            /// <summary>
            /// Play pause
            /// </summary>
            MEDIA_PLAY_PAUSE = 0xb3,

            /// <summary>
            /// Previous track
            /// </summary>
            MEDIA_PREV_TRACK = 0xb1,

            /// <summary>
            /// Stop
            /// </summary>
            MEDIA_STOP = 0xb2,


            /// <summary>Key "+"</summary>
            ADD = 0x6b,
            /// <summary>
            /// "*" key
            /// </summary>
            MULTIPLY = 0x6a,

            /// <summary>
            /// "/" key
            /// </summary>
            DIVIDE = 0x6f,

            /// <summary>
            /// Subtract key "-"
            /// </summary>
            SUBTRACT = 0x6d,


            /// <summary>
            /// Go Back
            /// </summary>
            BROWSER_BACK = 0xa6,
            /// <summary>
            /// Favorites
            /// </summary>
            BROWSER_FAVORITES = 0xab,
            /// <summary>
            /// Forward
            /// </summary>
            BROWSER_FORWARD = 0xa7,
            /// <summary>
            /// Home
            /// </summary>
            BROWSER_HOME = 0xac,
            /// <summary>
            /// Refresh
            /// </summary>
            BROWSER_REFRESH = 0xa8,
            /// <summary>
            /// browser search
            /// </summary>
            BROWSER_SEARCH = 170,
            /// <summary>
            /// Stop
            /// </summary>
            BROWSER_STOP = 0xa9,

            /// <summary>
            /// 
            /// </summary>
            NUMPAD0 = 0x60,
            /// <summary>
            /// 
            /// </summary>
            NUMPAD1 = 0x61,
            /// <summary>
            /// 
            /// </summary>
            NUMPAD2 = 0x62,
            /// <summary>
            /// 
            /// </summary>
            NUMPAD3 = 0x63,
            /// <summary>
            /// 
            /// </summary>
            NUMPAD4 = 100,
            /// <summary>
            /// 
            /// </summary>
            NUMPAD5 = 0x65,
            /// <summary>
            /// 
            /// </summary>
            NUMPAD6 = 0x66,
            /// <summary>
            /// 
            /// </summary>
            NUMPAD7 = 0x67,
            /// <summary>
            /// 
            /// </summary>
            NUMPAD8 = 0x68,
            /// <summary>
            /// 
            /// </summary>
            NUMPAD9 = 0x69,


            /// <summary>
            /// F1
            /// </summary>
            F1 = 0x70,
            /// <summary>
            /// F10
            /// </summary>
            F10 = 0x79,
            /// <summary>
            /// 
            /// </summary>
            F11 = 0x7a,
            /// <summary>
            /// 
            /// </summary>
            F12 = 0x7b,
            /// <summary>
            /// 
            /// </summary>
            F13 = 0x7c,
            /// <summary>
            /// 
            /// </summary>
            F14 = 0x7d,
            /// <summary>
            /// 
            /// </summary>
            F15 = 0x7e,
            /// <summary>
            /// 
            /// </summary>
            F16 = 0x7f,
            /// <summary>
            /// 
            /// </summary>
            F17 = 0x80,
            /// <summary>
            /// 
            /// </summary>
            F18 = 0x81,
            /// <summary>
            /// 
            /// </summary>
            F19 = 130,
            /// <summary>
            /// 
            /// </summary>
            F2 = 0x71,
            /// <summary>
            /// 
            /// </summary>
            F20 = 0x83,
            /// <summary>
            /// 
            /// </summary>
            F21 = 0x84,
            /// <summary>
            /// 
            /// </summary>
            F22 = 0x85,
            /// <summary>
            /// 
            /// </summary>
            F23 = 0x86,
            /// <summary>
            /// 
            /// </summary>
            F24 = 0x87,
            /// <summary>
            /// 
            /// </summary>
            F3 = 0x72,
            /// <summary>
            /// 
            /// </summary>
            F4 = 0x73,
            /// <summary>
            /// 
            /// </summary>
            F5 = 0x74,
            /// <summary>
            /// 
            /// </summary>
            F6 = 0x75,
            /// <summary>
            /// 
            /// </summary>
            F7 = 0x76,
            /// <summary>
            /// 
            /// </summary>
            F8 = 0x77,
            /// <summary>
            /// 
            /// </summary>
            F9 = 120,


            /// <summary>
            /// 
            /// </summary>
            OEM_1 = 0xba,
            /// <summary>
            /// 
            /// </summary>
            OEM_102 = 0xe2,
            /// <summary>
            /// 
            /// </summary>
            OEM_2 = 0xbf,
            /// <summary>
            /// 
            /// </summary>
            OEM_3 = 0xc0,
            /// <summary>
            /// 
            /// </summary>
            OEM_4 = 0xdb,
            /// <summary>
            /// 
            /// </summary>
            OEM_5 = 220,
            /// <summary>
            /// 
            /// </summary>
            OEM_6 = 0xdd,
            /// <summary>
            /// 
            /// </summary>
            OEM_7 = 0xde,
            /// <summary>
            /// 
            /// </summary>
            OEM_8 = 0xdf,
            /// <summary>
            /// 
            /// </summary>
            OEM_CLEAR = 0xfe,
            /// <summary>
            /// 
            /// </summary>
            OEM_COMMA = 0xbc,
            /// <summary>
            /// 
            /// </summary>
            OEM_MINUS = 0xbd,
            /// <summary>
            /// 
            /// </summary>
            OEM_PERIOD = 190,
            /// <summary>
            /// 
            /// </summary>
            OEM_PLUS = 0xbb,


            /// <summary>
            /// 
            /// </summary>
            KEY_0 = 0x30,
            /// <summary>
            /// 
            /// </summary>
            KEY_1 = 0x31,
            /// <summary>
            /// 
            /// </summary>
            KEY_2 = 50,
            /// <summary>
            /// 
            /// </summary>
            KEY_3 = 0x33,
            /// <summary>
            /// 
            /// </summary>
            KEY_4 = 0x34,
            /// <summary>
            /// 
            /// </summary>
            KEY_5 = 0x35,
            /// <summary>
            /// 
            /// </summary>
            KEY_6 = 0x36,
            /// <summary>
            /// 
            /// </summary>
            KEY_7 = 0x37,
            /// <summary>
            /// 
            /// </summary>
            KEY_8 = 0x38,
            /// <summary>
            /// 
            /// </summary>
            KEY_9 = 0x39,
            /// <summary>
            /// 
            /// </summary>
            KEY_A = 0x41,
            /// <summary>
            /// 
            /// </summary>
            KEY_B = 0x42,
            /// <summary>
            /// 
            /// </summary>
            KEY_C = 0x43,
            /// <summary>
            /// 
            /// </summary>
            KEY_D = 0x44,
            /// <summary>
            /// 
            /// </summary>
            KEY_E = 0x45,
            /// <summary>
            /// 
            /// </summary>
            KEY_F = 70,
            /// <summary>
            /// 
            /// </summary>
            KEY_G = 0x47,
            /// <summary>
            /// 
            /// </summary>
            KEY_H = 0x48,
            /// <summary>
            /// 
            /// </summary>
            KEY_I = 0x49,
            /// <summary>
            /// 
            /// </summary>
            KEY_J = 0x4a,
            /// <summary>
            /// 
            /// </summary>
            KEY_K = 0x4b,
            /// <summary>
            /// 
            /// </summary>
            KEY_L = 0x4c,
            /// <summary>
            /// 
            /// </summary>
            KEY_M = 0x4d,
            /// <summary>
            /// 
            /// </summary>
            KEY_N = 0x4e,
            /// <summary>
            /// 
            /// </summary>
            KEY_O = 0x4f,
            /// <summary>
            /// 
            /// </summary>
            KEY_P = 80,
            /// <summary>
            /// 
            /// </summary>
            KEY_Q = 0x51,
            /// <summary>
            /// 
            /// </summary>
            KEY_R = 0x52,
            /// <summary>
            /// 
            /// </summary>
            KEY_S = 0x53,
            /// <summary>
            /// 
            /// </summary>
            KEY_T = 0x54,
            /// <summary>
            /// 
            /// </summary>
            KEY_U = 0x55,
            /// <summary>
            /// 
            /// </summary>
            KEY_V = 0x56,
            /// <summary>
            /// 
            /// </summary>
            KEY_W = 0x57,
            /// <summary>
            /// 
            /// </summary>
            KEY_X = 0x58,
            /// <summary>
            /// 
            /// </summary>
            KEY_Y = 0x59,
            /// <summary>
            /// 
            /// </summary>
            KEY_Z = 90,

            /// <summary>
            /// Decrese volume
            /// </summary>
            VOLUME_DOWN = 0xae,

            /// <summary>
            /// Mute volume
            /// </summary>
            VOLUME_MUTE = 0xad,

            /// <summary>
            /// Increase volue
            /// </summary>
            VOLUME_UP = 0xaf,



            /// <summary>
            /// Take snapshot of the screen and place it on the clipboard
            /// </summary>
            SNAPSHOT = 0x2c,

            /// <summary>Send right click from keyboard "key that is 2 keys to the right of space bar"</summary>
            RightClick = 0x5d,

            /// <summary>
            /// Go Back or delete
            /// </summary>
            BACKSPACE = 8,

            /// <summary>
            /// Control + Break "When debuging if you step into an infinite loop this will stop debug"
            /// </summary>
            CANCEL = 3,
            /// <summary>
            /// Caps lock key to send cappital letters
            /// </summary>
            CAPS_LOCK = 20,
            /// <summary>
            /// Ctlr key
            /// </summary>
            CONTROL = 0x11,

            /// <summary>
            /// Alt key
            /// </summary>
            ALT = 18,

            /// <summary>
            /// "." key
            /// </summary>
            DECIMAL = 110,

            /// <summary>
            /// Delete Key
            /// </summary>
            DELETE = 0x2e,


            /// <summary>
            /// Arrow down key
            /// </summary>
            DOWN = 40,

            /// <summary>
            /// End key
            /// </summary>
            END = 0x23,

            /// <summary>
            /// Escape key
            /// </summary>
            ESC = 0x1b,

            /// <summary>
            /// Home key
            /// </summary>
            HOME = 0x24,

            /// <summary>
            /// Insert key
            /// </summary>
            INSERT = 0x2d,

            /// <summary>
            /// Open my computer
            /// </summary>
            LAUNCH_APP1 = 0xb6,
            /// <summary>
            /// Open calculator
            /// </summary>
            LAUNCH_APP2 = 0xb7,

            /// <summary>
            /// Open default email in my case outlook
            /// </summary>
            LAUNCH_MAIL = 180,

            /// <summary>
            /// Opend default media player (itunes, winmediaplayer, etc)
            /// </summary>
            LAUNCH_MEDIA_SELECT = 0xb5,

            /// <summary>
            /// Left control
            /// </summary>
            LCONTROL = 0xa2,

            /// <summary>
            /// Left arrow
            /// </summary>
            LEFT = 0x25,

            /// <summary>
            /// Left shift
            /// </summary>
            LSHIFT = 160,

            /// <summary>
            /// left windows key
            /// </summary>
            LWIN = 0x5b,

            //
            SCROLL_LOCK = 0x91,

            //
            PAUSE_BREAK = 0x13,

            /// <summary>
            /// Next "page down"
            /// </summary>
            PAGEDOWN = 0x22,

            /// <summary>
            /// Num lock to enable typing numbers
            /// </summary>
            NUMLOCK = 0x90,

            /// <summary>
            /// Page up key
            /// </summary>
            PAGE_UP = 0x21,

            /// <summary>
            /// Right control
            /// </summary>
            RCONTROL = 0xa3,

            /// <summary>
            /// Return key
            /// </summary>
            ENTER = 13,

            /// <summary>
            /// Right arrow key
            /// </summary>
            RIGHT = 0x27,

            /// <summary>
            /// Right shift
            /// </summary>
            RSHIFT = 0xa1,

            /// <summary>
            /// Right windows key
            /// </summary>
            RWIN = 0x5c,

            /// <summary>
            /// Shift key
            /// </summary>
            SHIFT = 0x10,

            /// <summary>
            /// Space back key
            /// </summary>
            SPACE_BAR = 0x20,

            /// <summary>
            /// Tab key
            /// </summary>
            TAB = 9,

            /// <summary>
            /// Up arrow key
            /// </summary>
            UP = 0x26,

        }

        private void button5_Click(object sender, EventArgs e)
        {
            Control btn = (Button)sender;
            KeyCode key;
            Enum.TryParse<KeyCode>(btn.Tag.ToString(), out key);

            //
            SendKeyDown(key);
            SendKeyUP(key);
        }




        // 
    }




    

        
}
