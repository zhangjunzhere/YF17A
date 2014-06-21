using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using YF17A.AccessDatabase;

using System.Data;
using System.IO;
using System.Diagnostics;

namespace YF17A
{
    class ContentProvider
    {
        private OleDbConnection mCurrentConn;
        Dictionary<String, OleDbConnection> connMap = new Dictionary<string, OleDbConnection>();
       
        private static ContentProvider sInstance = null;
        public  String DATABASEPATH = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "YF17A.mdb");
      
        public static  ContentProvider getInstance()
        {
            if (sInstance == null)
            {
                sInstance = new ContentProvider();
            }

            OleDbConnection conn = sInstance.mCurrentConn;
            String databasePath = sInstance.DATABASEPATH;

            if (sInstance.connMap.ContainsKey(databasePath))
            {
                sInstance.connMap.TryGetValue(databasePath, out conn);
            }
            else
            {
                //@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=D:/school.mdb"
                String connectString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + databasePath;
                sInstance.mCurrentConn = new OleDbConnection(connectString);
                sInstance.connMap.Add(databasePath, conn);
            }

            if (sInstance.mCurrentConn.State != ConnectionState.Open)
            {
                sInstance.mCurrentConn.Open();
            }

            return sInstance;
        }
        
        private ContentProvider()
        {      
             DATABASEPATH = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "YF17A.mdb");
        }

        public void Open()
        {
            if (mCurrentConn != null)
            {
                mCurrentConn.Open();
            }
        }

        public void Close()
        {
            if (mCurrentConn != null)
            {
                mCurrentConn.Close();
            }
        }

        private int excuteCommand(String sql)
        {         
             OleDbCommand command = new OleDbCommand(sql, mCurrentConn);
             try
             {
                 return command.ExecuteNonQuery();
             }
             catch (Exception e)
             {
                 Console.WriteLine(e.ToString());
                 return -1;
             }
            
        }
      
        /// <summary>
        /// 根据SQL命令返回数据DataTable数据表,
        /// 可直接作为dataGridView的数据源
        /// </summary>
        /// <param name="SQL"></param>
        /// <returns></returns>
        public DataTable query(string SQL)
        {           
            OleDbDataAdapter adapter = new OleDbDataAdapter();
            OleDbCommand command = new OleDbCommand(SQL, mCurrentConn);
            adapter.SelectCommand = command;
            DataTable Dt = new DataTable();
            adapter.Fill(Dt);
            return Dt;
        }


        public int delete(String sql)
        {
           return excuteCommand(sql);
        }

        public int update(String sql)
        {
            return excuteCommand(sql);
        }

        public int insert(String sql)
        {
            return excuteCommand(sql);
        }
    }
}
