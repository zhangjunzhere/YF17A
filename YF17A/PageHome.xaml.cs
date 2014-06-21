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
using System.Windows.Threading;
using System.ComponentModel;


namespace YF17A
{
    /// <summary>
    /// Interaction logic for PageHelp.xaml
    /// </summary>
    public partial class PageHome : Page, IObserverResult
    {
        public const string TAG = "PageHome.xaml";  
        private BeckHoff mBeckHoff;

        //binding property
        StoreModel mDataModel = new StoreModel();

        public PageHome()
        {
            InitializeComponent();
            mBeckHoff = Utils.GetBeckHoffInstance();
            
            Utils.NavigateToPage(MainWindow.sFrameToolbarName, ToolbarMain.TAG);         
  
            //data binding
            this.tb_Store_CigNum.DataContext = this.mDataModel;
            this.tb_Store_percent.DataContext = this.mDataModel;           
        }
        
        //override
        public void onRecieveResult(Dictionary<String, Object> bundle)
        {
            Object senderName;
            Object senderValue;
            bundle.TryGetValue( PageDataExchange.KEY_SENDER_NAME, out senderName);
            bundle.TryGetValue( PageDataExchange.KEY_SENDER_VALUE, out senderValue);

            if (BeckHoff.TAG.Equals(senderName))
            {
                String plcVarName = senderValue.ToString();
                Object plcValue;
                mBeckHoff.plcVarUserdataMap.TryGetValue(plcVarName, out plcValue);

                if (mDataModel.WatchList.Contains(plcVarName))
                {
                    UpdateView();
                }
            }     
        }

        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            PageDataExchange context = PageDataExchange.getInstance();
            context.addResultObserver(TAG,this);

            //beck hoff register
            mBeckHoff.RegisterObserver(TAG, this);

            //init views           
            UpdateView();          
        }

        private void Canvas_Unloaded(object sender, RoutedEventArgs e)
        {
            PageDataExchange context = PageDataExchange.getInstance();
            context.removeResultObserver(TAG);
          
            //beck hoff unregister
            mBeckHoff.UnregisterObserver(TAG);
        }       
          
        private void UpdateView()
        {
            Object value0, value1;
            mBeckHoff.plcVarUserdataMap.TryGetValue(".Store_CigNum", out value0);
            mBeckHoff.plcVarUserdataMap.TryGetValue(".Store_CigNum2", out value1);
            int num1 = ((int)value1 << 16) + (int)value0;
            mDataModel.Store_CigNum = num1.ToString();
            mDataModel.Store_percent = Utils.GetFormatedPlcValue(".Store_percent");   
        }
    }


    class StoreModel : INotifyPropertyChanged
    {
        private string _CigNum ;
        public string Store_CigNum //Store_CigNum	Int	存储烟支数		只读	数值显示	MW1880
        {
            get
            {
                return _CigNum;
            }
            set
            {
                _CigNum = value;
                OnPropertyChanged("Store_CigNum");
            }
        }

        private string _percent;
        public string Store_percent //Store_percent	Int	存储位置百分比		只读	数值显示	DB8.DBW50 
        {
            get
            {
                return _percent;
            }
            set
            {
                _percent = value;
                OnPropertyChanged("Store_percent");
            }
        }

        private HashSet<String> _watchList = new HashSet<string>();
        public HashSet<String> WatchList
        {
            get
            {
                if (_watchList.Count == 0)
                {                   
                    _watchList.Add(".Store_CigNum");
                    _watchList.Add(".Store_CigNum2");
                    _watchList.Add(".Store_percent");
                }
                return _watchList;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
