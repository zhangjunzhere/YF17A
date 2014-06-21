using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace YF17A
{  
    public class Category
    {
        public int id{get;set;}
        public string description{get;set;}

        public static List<Category> GetCategories(String filter)
        {
            string sql = @"SELECT [id],[description] FROM [category] where table='" + filter + "'";

            ContentProvider provider = ContentProvider.getInstance();
            DataTable dt = provider.query(sql);

            List<Category> cats = new List<Category>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                Category cat = new Category();
                cat.id = int.Parse(row["id"].ToString());
                cat.description =row["description"].ToString();
                cats.Add(cat);
            }

            return cats;
        }
    }
    
}
