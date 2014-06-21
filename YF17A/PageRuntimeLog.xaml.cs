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
    public partial class PageRuntimeLog : Page, IObserverResult
    {
        public const String TAG = "PageRuntimeLog.xaml";

        private const String TABLE = "journal";
      
        private List<WarnningDataSource.ErrorInfo> mListError = new List<WarnningDataSource.ErrorInfo>();        

        public PageRuntimeLog()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Page_Loaded);
            this.Unloaded += new RoutedEventHandler(Page_Unloaded);
            initView();
            Utils.NavigateToPage(MainWindow.sFrameToolbarName, ToolbarParameter.TAG);
        }

        #region toolbarparameter
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {         
            PageDataExchange context = PageDataExchange.getInstance();
            context.addResultObserver(TAG, this);       
       
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {          
            PageDataExchange context = PageDataExchange.getInstance();
            context.removeResultObserver(TAG);
        }
        #endregion

        #region IActionBar members
        //override beckhoff changed       
        public void onRecieveResult(Dictionary<String, Object> bundle)
        {
            //Object senderName;          
            //Object senderValue;

            //bundle.TryGetValue( PageDataExchange.KEY_SENDER_NAME, out senderName);          
           // bundle.TryGetValue( PageDataExchange.KEY_SENDER_VALUE, out senderValue);
            
           //  WarnningDataSource.ErrorInfo info = (WarnningDataSource.ErrorInfo)senderValue;
             //this.tb_code.Text = info.code;
             //this.tb_solution.Text = info.solution;
        }
        #endregion

        private void btn_query_Click(object sender, RoutedEventArgs e)
        {         

            StringBuilder query = new StringBuilder();
            String StartDate = this.datePicker_start.SelectedDate.ToString();
            String EndDate = this.datePicker_end.SelectedDate.ToString();
            Boolean hasContent = false;

            if (!String.IsNullOrEmpty(StartDate))
            {
                query.Append(" datediff('d', [log.when] , '" + StartDate + "') <=0");
                hasContent = true;
            }
           
            if (!String.IsNullOrEmpty(EndDate))
            {
                if(hasContent)
                {
                    query.Append(" AND ");
                }
                query.Append(" datediff('d', [log.when] , '" + EndDate + "') >=0");
               hasContent = true;
            }

            int category = int.Parse(this.cb_category.SelectedValue.ToString());
            if (category != -1)
            {
                if(hasContent)
                {
                    query.Append(" AND ");
                }
                query.Append("category.id = " + category);
            }
            List<Log.Journal> listLogs = Log.getAllLogs(query.ToString());
            this.lv_logs.ItemsSource = listLogs;
         }

        private void initView()
        {
            //List<Log.Journal> listLogs = Log.getAllLogs(null);
            //this.lv_logs.ItemsSource = listLogs;

            this.datePicker_start.SelectedDate = DateTime.Now.AddDays(-1);
            this.datePicker_end.SelectedDate = DateTime.Now;

            List<Category> cats = Category.GetCategories(TABLE);
            cats.Insert(0, new Category() { id = -1, description = "全部" });
            this.cb_category.ItemsSource = cats;
            this.cb_category.DisplayMemberPath = "description";
            this.cb_category.SelectedValuePath = "id";
            this.cb_category.SelectedIndex = 0;            
        }

        private void cb_category_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btn_query_Click(sender, null);
        }
    }
}

