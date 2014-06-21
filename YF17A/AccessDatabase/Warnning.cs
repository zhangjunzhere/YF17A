using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace YF17A
{  
    public class Warnning
    {
        public const int ERROR = 21;
        public const int WANNING = 22;
        public const int INFO = 23;
       
        //public class Record
        //{           
        //    public int index{get;set;}
        //    public String  category{get;set;}
        //    public string description { get; set; }
        //    public string code { get; set; }
        //    public DateTime whenhappened { get; set; }
        //    public DateTime whenresolved { get; set; }
        //}

        //record infomation runtime
        public static int write(WarnningDataSource.ErrorInfo info)
        {
            ContentProvider provider = ContentProvider.getInstance();
            string sql = String.Format(@"INSERT INTO [warnning] ([level],[category_id],[code],[description],[whenhappened] ) 
                                                    VALUES({0},{1},'{2}','{3}','{4}')", info.level, info.category_id, info.code, info.description, DateTime.Now.ToString());

           int  rowAffected = provider.insert(sql);
          
            return rowAffected;
        }

        public static int resolve(WarnningDataSource.ErrorInfo info)
        {
            ContentProvider provider = ContentProvider.getInstance();
            String sqlQuery = String.Format(@"SELECT [_id],[whenresolved]FROM [warnning] where level={0} ORDER BY [whenhappened] desc", info.level);
            DataTable dt = provider.query(sqlQuery);
            int rowAffected = 0;
            foreach ( DataRow row in dt.Rows)
            {
                string whenresolved = row["whenresolved"].ToString();
                if (String.IsNullOrEmpty(whenresolved))
                {
                    string id = row["_id"].ToString();
                    string sql = String.Format(@"UPDATE [warnning] SET [whenresolved]='{0}' where [_id]={1} ", DateTime.Now.ToString(), id);
                    rowAffected = provider.update(sql);
                    break;
                }
            }
           
            return rowAffected;
        }

        public static List<WarnningDataSource.ErrorInfo> getAllRecords(String filter)
        {
            String where = "";
            if (!String.IsNullOrEmpty(filter))
            {
                where = " where " + filter;
            }

            string sql = @"SELECT [level] ,
                                                [code] ,
                                                [category.description] as category,
                                                [warnning.description] as description,
                                                [whenhappened] ,
                                                [whenresolved] 
                                    FROM [warnning] INNER JOIN [category] ON warnning.category_id=category.id" 
                                    + where + " ORDER BY [whenhappened] desc ";

            ContentProvider provider = ContentProvider.getInstance();
            DataTable dt = provider.query(sql);

            List<WarnningDataSource.ErrorInfo> records = new List<WarnningDataSource.ErrorInfo>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                WarnningDataSource.ErrorInfo record = new WarnningDataSource.ErrorInfo();                
                record.level = int.Parse(row["level"].ToString());
                record.category = row["category"].ToString();
                record.description = row["description"].ToString();
                record.whenhappened = row["whenhappened"].ToString();
                record.whenresolved = row["whenresolved"].ToString();
            
      
                 record.code = row["code"].ToString();
          
                records.Add(record);
            }

            return records;
        }
    }
    
}
