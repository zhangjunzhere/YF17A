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

namespace YF17A
{
    public class ImageButton : Button
    {
        static ImageButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageButton), new FrameworkPropertyMetadata(typeof(ImageButton)));
            NormalStateProperty = DependencyProperty.Register("NormalState", typeof(string), typeof(ImageButton));
            MouseOverStateProperty = DependencyProperty.Register("MouseOverState", typeof(string), typeof(ImageButton));
            PressedStateProperty = DependencyProperty.Register("PressedState", typeof(string), typeof(ImageButton));
            DisableStateProperty = DependencyProperty.Register("DisableState", typeof(string), typeof(ImageButton));
        }

        public string NormalState
        {
            get { return (string)GetValue(NormalStateProperty); }
            set { SetValue(NormalStateProperty, value); }
        }

        public string MouseOverState
        {
            get { return (string)GetValue(MouseOverStateProperty); }
            set { SetValue(MouseOverStateProperty, value); }
        }

        public string PressedState
        {
            get { return (string)GetValue(PressedStateProperty); }
            set { SetValue(PressedStateProperty, value); }
        }

        public string DisableState
        {
            get { return (string)GetValue(DisableStateProperty); }
            set { SetValue(DisableStateProperty, value); }
        }

        public static DependencyProperty NormalStateProperty;
        public static DependencyProperty MouseOverStateProperty;
        public static DependencyProperty PressedStateProperty;
        public static DependencyProperty DisableStateProperty;
    }
}
