using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace YF17A
{  
    public class Log
    {
        public const int CATEGOTY_LOGIN = 31;
        public const int CATEGOTY_PARAMETER = 32;
        public const int CATEGOTY_RUN = 33;

        public class Journal
        {
            public String category{get;set;}
            public String description { get; set; }
            public String when { get; set; } //datatime
        }
      
        //record infomation runtime
        public static int write(int category, string description)
        {
            String time = DateTime.Now.ToString();

            string sql = @"INSERT INTO [log] ([when],[category_id],[description]) VALUES('" + DateTime.Now.ToString() + "'," + category + ",'" + description + "')";

            ContentProvider provider = ContentProvider.getInstance();
            int rowAffected = provider.insert(sql);

            return rowAffected ;
        }

        public static List<Journal> getAllLogs(String filter)
        {            
            String where = "";
            if (!String.IsNullOrEmpty(filter))
            {
                where = " where " + filter;
            }
            string sql = @"SELECT [category.description] as category,[log.description] as description,[when] FROM [log] INNER JOIN [category] ON log.category_id=category.id"
                                + where + " ORDER BY [when] desc";
            
            ContentProvider provider = ContentProvider.getInstance();
            DataTable dt = provider.query(sql);

            List<Journal> logs = new List<Journal>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                Journal log = new Journal();
                log.when = row["when"].ToString();
                log.category = row["category"].ToString();
                log.description = row["description"].ToString();
                logs.Add(log);
            }

            return logs;
        }
     }
}
