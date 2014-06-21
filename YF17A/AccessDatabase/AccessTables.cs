using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.IO;
using System.Diagnostics;
using System.Data;

namespace YF17A.AccessDatabase
{
    public class AccessTables{
      
        
        public  String DATABASEPATH = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "YF17A.mdb");
 
        public User getUserInfoByName(String userName)
        {
            User usr= null;
            ContentProvider provider = ContentProvider.getInstance();
            string sql = @"SELECT user.account,UserRole.role FROM [user] INNER JOIN [UserRole] ON user.level=UserRole.userlevel" +
                " WHERE account='" + userName + "'";
            try
            {
                DataTable dt = provider.query(sql);
             
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    //usr = new User();                    
                   
                    //usr.Account = row["account"].ToString();
                    //usr.Role = row["role"].ToString();
                }
               
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return usr;
        }

        public Log getLogByTag(String Tag)
        {
            Log log = new Log();
            return log;
        }

        public void test()
        {
            User user = getUserInfoByName("100");
            user = getUserInfoByName("200");
            user = getUserInfoByName("admin");           
        }

       
    }
   
}
