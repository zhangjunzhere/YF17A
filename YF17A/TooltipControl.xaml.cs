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
   

    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    /// 
    public partial class TooltipControl : UserControl
    {
        private IObserverResult mObserver;

        public  const String KEYSENDERNAME = "SenderName";
        public  const String KEYSENDERVALUE = "SenderValue";
        public  const String KEYRESULTVALUE = "ResultValue";
        
     //   private Dictionary<String, String> mHelpMap = new Dictionary<String, String>();

        public TooltipControl()
        {
            InitializeComponent();
        }
        public void registerObserver(IObserverResult observer)
        {
            mObserver = observer;
        }

        private void notifyChanged(Dictionary<String, Object> bundle)
        {
            if (mObserver != null)
            {
                mObserver.onRecieveResult(bundle);
            }
        }

        public void putExtra(Dictionary<String, Object> bundle)
        {
            //mBundle = bundle;
            Object senderName;
            bundle.TryGetValue( PageDataExchange.KEY_SENDER_NAME, out senderName);

            Object senderValue;
            bundle.TryGetValue( PageDataExchange.KEY_SENDER_VALUE, out senderValue);

     //       title.Text = senderName.ToString();
            description.Text = senderValue.ToString();
            //lb_input.Content = senderValue.ToString();
        }
       
         public double UserControlToolTipX
        {
            get { return this.UserControlToolTipXY.X; }
            set { this.UserControlToolTipXY.X = value; }
        }

        public double UserControlToolTipY
        {
            get { return this.UserControlToolTipXY.Y; }
            set { this.UserControlToolTipXY.Y = value; }
        }
    }
}
