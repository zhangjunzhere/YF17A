using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace YF17A
{
    /// <summary>
    /// Interaction logic for PagePopup.xaml
    /// </summary>
    public partial class PagePopup : Page
    {
        public PagePopup()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnShowDlg_Click(object sender, RoutedEventArgs e)
        {
            ShowModalDialog(true);
            UpdateWindow();
        }


        private void ShowModalDialog(bool bShow)
        {
            this.ModalDialog.IsOpen = bShow;
            this.MainPanel.IsEnabled = !bShow;            

            //this.ModalDialog.Margin = new FrameworkElement.Margin();
        }

       
        private void Dlg_BtnClose_Click(object sender, RoutedEventArgs e)
        {
            ShowModalDialog(false);
        }

        private void Dlg_BtnOK_Click(object sender, RoutedEventArgs e)
        {
            ShowModalDialog(false);

            labelString.Content = "The string is: " + TxtBoxInput.Text;
        }

        private void ShowKeyboardAtLocation(Button btn, int offsetX, int offsetY)
        {
            Window window = Window.GetWindow(this);
            Point pt0 = this.TransformToAncestor(window).Transform(new Point(0, 0));
            Point pt1 = btn.TranslatePoint(new Point(), this);

            int left = (int)(pt0.X + pt1.X) + offsetX;
            int top = (int)(pt0.Y + pt1.Y) + offsetY;

            var hwnd = ((HwndSource)PresentationSource.FromVisual(this.ModalDialog.Child)).Handle;

            RECT rect;
            if (GetWindowRect(hwnd, out rect))
            {
                SetWindowPos(hwnd, -2, left, top, -1, -1, 0);
            }
        }


        private void UpdateWindow()
        {
            Window window = Window.GetWindow(this);
            Point pt0 = this.TransformToAncestor(window).Transform(new Point(0, 0));
                        
            Point pt1 = BtnShowDlg.TranslatePoint(new Point(), this);

            Point pt2 = BtnShowDlg.PointToScreen(new Point());


            //int left = (int) (pt0.X + pt1.X);
            //int top = (int) (pt0.Y + pt1.Y);

            int left = (int)(pt2.X );
            int top = (int)(pt2.Y);
           
            var hwnd = ((HwndSource)PresentationSource.FromVisual(this.ModalDialog.Child)).Handle;

            RECT rect;
            if (GetWindowRect(hwnd, out rect))
            {
                SetWindowPos(hwnd, -2, left, top, -1, -1, 0);
            }
        }

        #region P/Invoke imports & definitions
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32", EntryPoint = "SetWindowPos")]
        private static extern int SetWindowPos(IntPtr hWnd, int hwndInsertAfter, int x, int y, int cx, int cy, int wFlags);
        #endregion

        private void ModalDialog_LostFocus(object sender, RoutedEventArgs e)
        {
            ShowModalDialog(false);
        }

    }
}
