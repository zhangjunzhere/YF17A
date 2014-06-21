using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;

namespace YF17A
{  
    public class Parameter
    {
        public const String GOOD = "value0";
        public const String BETTER = "value1";
        public const String BEST = "value2";

        public class Record
        {
            public String Name { get; set; }
            public String Description { get; set; }
            public String Value { get; set; }
        }

        //backup all paramters into one of good,better, best records
        public static int backup(List<Record> items, String filter)
        {
            ContentProvider provider = ContentProvider.getInstance();
            string sql = "SELECT COUNT(*) AS [size] FROM [parameter] ";
            DataTable dt = provider.query(sql);
            int size = Int32.Parse(dt.Rows[0]["size"].ToString());
            StringBuilder sqlBuilder = new StringBuilder();
            if (size == 0)
            {
                //insert               
                //sqlBuilder.Append(String.Format(" INSERT INTO [parameter] ([name],[description],[{0}]) VALUES(",filter));
                //foreach (Record item in items)
                //{
                //    string value = String.Format("('{0}','{1}',{2}),", item.Name, item.Description, item.Value);
                //    sqlBuilder.Append(value);
                //}
                //sql = sqlBuilder.Append(")").ToString();
                //return provider.insert(sql);
                foreach(Record item in items)
                {
                    sql = String.Format(" INSERT INTO [parameter] ([name],[description],[{0}]) VALUES('{1}','{2}','{3}');", filter, item.Name, item.Description, item.Value);
                    provider.insert(sql);
                }               
            }
            else
            {
                //update
                //sqlBuilder.Append(String.Format(" UPDATE  [parameter] SET [{0}] = ( CASE name", filter));
                //foreach (Record item in items)
                //{
                //    string value = String.Format("WHEN '{0}' THEN {1}", item.Name, item.Value);
                //    sqlBuilder.Append(value);
                //}

                //sqlBuilder.Append(String.Format(" ELSE {0} END) ", filter));
                //sql = sqlBuilder.Append(")").ToString();
                //return provider.update(sql);

                foreach (Record item in items)
                {
                    sql = String.Format(" UPDATE [parameter]  SET [{0}] = '{1}' WHERE [name]='{2}' ;", filter,  item.Value,item.Name);
                    provider.update(sql);
                }
            }
            return 0;
        }

        //restore good, better, or best parameters
        public static void restore(String columFilter) 
        {
            if (!User.GetInstance().GetCurrentUserInfo().IsLogin)
            {
                MessageBox.Show("您需要登录才能操作");
                Dictionary<String, Object> bundle = new Dictionary<string, object>();
                bundle.Add(PageUserRegister.USER_PAGE, PageUserRegister.ID_LOGIN);
                PageDataExchange.getInstance().putExtra(PageUserRegister.TAG, bundle);
                Utils.NavigateToPage(MainWindow.sFrameReportName, PageUserRegister.TAG);
                return;
            }
            List<Record> items = getParameterList(columFilter);
            foreach (Record item in items)
            {          
                BeckHoff beckhoff = Utils.GetBeckHoffInstance();
                beckhoff.writeInt(item.Name, item.Value);
            }         
        }

        public static List<Record> getParameterList(String columFilter)
        {            
            string sql = String.Format(@"SELECT [name],[description],[{0}] AS [value]  FROM [parameter] ", columFilter);
            BeckHoff beckhoff = Utils.GetBeckHoffInstance();

            ContentProvider provider = ContentProvider.getInstance();
            DataTable dt = provider.query(sql);

            List<Record> records = new List<Record>();
            foreach (DataRow row in dt.Rows)
            {
                Record item = new Record();
                item.Name = row["name"].ToString();
                item.Description = row["description"].ToString();
                item.Value = row["value"].ToString();    
                records.Add(item);
            }
            return records;
        }
    }
    
}
