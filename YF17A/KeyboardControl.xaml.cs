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
    public partial class KeyboardControl : UserControl,IObserverResult
    {
        public const String TAG = "KeyboardControl.xaml";
      //  private IObserverResult mObserver;
        
       // private Dictionary<String, Object> mBundle = new Dictionary<String, Object>();

        private String  mSourceControlName;
        private BeckHoff mBeckHoff = Utils.GetBeckHoffInstance();
       

        public KeyboardControl()
        {
            InitializeComponent();
        }
        //public void registerObserver(IObserverResult observer)
        //{
        //    mObserver = observer;
        //}

        //private void notifyChanged(Dictionary<String, Object> bundle)
        //{
        //    if (mObserver != null)
        //    {
        //        mObserver.onRecieveResult(bundle);
        //    }
        //}

        public void onRecieveResult(Dictionary<String, Object> bundle)
        {
            //mBundle = bundle;
            Object senderName;
            Object senderValue;
            Object senderLimit;
            bundle.TryGetValue(PageDataExchange.KEY_SENDER_NAME, out senderName);           
            bundle.TryGetValue(PageDataExchange.KEY_SENDER_VALUE, out senderValue);
            bundle.TryGetValue(PageDataExchange.KEY_THREAD_HOLD, out senderLimit);

            double max = 100;
            double min = 0;
            BeckHoff.ThresHold limit = (BeckHoff.ThresHold)senderLimit;
            if (limit != null)
            {                
                max =  limit.max / limit.ratio;
                min = limit.min / limit.ratio;
            }

            this.tb_limit_max.Text = max.ToString();
            this.tb_limit_min.Text = min.ToString();
            this.tb_current_value.Text = min.ToString();
            //this.tb_current_value.Text = senderValue.ToString();
            this.lb_input.Content = "";

            mSourceControlName = senderName.ToString();         
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

        public string UserInput
        {
            get { return lb_input.Content.ToString(); }
            set { lb_input.Content = value; }
        }

        private void NumberClick(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            UserInput += btn.Content.ToString();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb =  new StringBuilder();
            if (!String.IsNullOrEmpty(UserInput))
            {
                sb.Append(UserInput);
                sb.Remove(UserInput.Length - 1, 1);
                UserInput = sb.ToString();
            }               
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            //this.Visibility = Visibility.Hidden;
            //Dictionary<String, Object> bundle = new Dictionary<string, object>();
            //bundle.Add(PageDataExchange.KEY_SENDER_NAME, TAG);
            //bundle.Add(PageDataExchange.KEY_SENDER_VALUE, mSourceControlName);
            //bundle.Add(PageDataExchange.KEY_RESULT_VALUE, UserInput);
            //notifyChanged(bundle);

            if (!String.IsNullOrEmpty(UserInput))
            {
                String plcVarName = mSourceControlName.Replace("btn_", ".");
                mBeckHoff.writeInt(plcVarName, UserInput);
                this.Visibility = Visibility.Hidden;
            }
        }
    }
}
