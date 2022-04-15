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


        public virtualkeyboard()
        {
            InitializeComponent();
        }

        private void virtualkeyboard_Load(object sender, EventArgs e)
        {
            // Gọi hàm SetWindowPos
            // Bàn phím ảo sẽ luôn ở trên các chương trình khác trên màn hình
            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
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


        


    }
}
