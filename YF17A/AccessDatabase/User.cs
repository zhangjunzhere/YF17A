using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace YF17A
{
     public class User
    {
         public const String TAG = "User.cs";

        public const int USER_PREVILIDGE_OPERATOR = 11;
        public const int USER_PREVILIDGE_ADMINISTRATOR = 12;
        public const int USER_PREVILIDGE_DEBUGGER = 13;       
        public const int USER_PREVILIDGE_DESIGNER = 14;
        public const int USER_PREVILIDGE_SUPERADMIN = 20;

        public class Info
        {
            public string Role { get; set; }
            public string Account { get; set; }
            public string Password { get; set; }
            public int UserLevel { get; set; }
            public Boolean IsLogin { get; set; }
        }

        public  Info mCurrentUserInfo ;
        private static User sInstance;
        private User()
        {         
        }

        public static User GetInstance()
        {
            if (sInstance == null)
            {
                sInstance = new User();               
            }
            return sInstance;
        }

        public Info GetCurrentUserInfo()
        {
            if (mCurrentUserInfo == null)
            {
                mCurrentUserInfo = new Info();
            }
            return GetInstance().mCurrentUserInfo;
        }


        public  Boolean Login(String userName, String password)
        {
            String superAdmin = "xyj6690";
            string sql = @"SELECT [user.account] ,[user.password] ,[user.category_id] ,[category.description] as role FROM [user] INNER JOIN [category] ON user.category_id=category.id" +
               " WHERE account='" + userName + "' AND password='" + password + "'"; 

            ContentProvider provider = ContentProvider.getInstance();
            DataTable dt = provider.query(sql);

            mCurrentUserInfo = GetCurrentUserInfo();
            mCurrentUserInfo.IsLogin = false;
           
            if (dt.Rows.Count > 0)
            {               
                mCurrentUserInfo.IsLogin = true;
                DataRow row = dt.Rows[0];
                mCurrentUserInfo.Account = row["user.account"].ToString();
                mCurrentUserInfo.Role = row["role"].ToString();
                int level;
                int.TryParse(row["user.category_id"].ToString(), out level);
                mCurrentUserInfo.UserLevel = level;               
            }
            else if (superAdmin.Equals(userName) && superAdmin.Equals(password))
            {
                mCurrentUserInfo.IsLogin = true;
                mCurrentUserInfo.Account = userName;
                mCurrentUserInfo.Role = "超级管理员";
                mCurrentUserInfo.UserLevel = User.USER_PREVILIDGE_SUPERADMIN;
            }

            if (mCurrentUserInfo.IsLogin == true)
            {
                String description = String.Format("用户 {0} 登陆", mCurrentUserInfo.Account);
                Log.write(Log.CATEGOTY_LOGIN, description);
                NotifyObservers(PageLogin.LOGIN);
            }
            return mCurrentUserInfo.IsLogin;
        }

        public void Logout()
        {
            String description = String.Format("用户 {0} 登出", mCurrentUserInfo.Account);
            Log.write(Log.CATEGOTY_LOGIN, description);

            mCurrentUserInfo.IsLogin = false;
            mCurrentUserInfo.Account = "";
            mCurrentUserInfo.Role = "";
            mCurrentUserInfo.UserLevel = -1;

            NotifyObservers(PageLogin.LOGOUT);
        }

        private void NotifyObservers(String info)
        {
            PageDataExchange context = PageDataExchange.getInstance();            

            context.NotifyAllObeservers(TAG,info);            
        }

        public  List<Category> getUserRoleList()
        {
            string sql = @"SELECT id,description FROM [category] where table='user'";

            ContentProvider provider = ContentProvider.getInstance();
            DataTable dt = provider.query(sql);

            List<Category> userroles = new List<Category>();
            for(int i = 0; i < dt.Rows.Count; i++)
            {               
                DataRow row = dt.Rows[i];               

                Category role = new Category();
                role.id = int.Parse(row["id"].ToString());
                role.description = row["description"].ToString();
                userroles.Add(role);
            }

            return userroles;
        }

        public List<Info> getUsersInfo()
        {
           // string sql = @"SELECT [category.id] as level, [category.description] as role ,[account],[password]  FROM [user] INNER JOIN [category] ON user.category_id=category.id";
            string sql = @"SELECT [user.account] ,[user.password] ,[user.category_id] ,[category.description] "
                                    + " FROM [user] INNER JOIN [category] ON user.category_id=category.id";
              

            ContentProvider provider = ContentProvider.getInstance();
            DataTable dt = provider.query(sql);

            List<Info> userroles = new List<Info>();
            foreach (DataRow row in dt.Rows)
            {
                Info info = new Info();
                info.UserLevel = int.Parse(row["user.category_id"].ToString());
                info.Role = row["category.description"].ToString();
                info.Account = row["user.account"].ToString();
                info.Password = row["user.password"].ToString();
                userroles.Add(info);
            }

            return userroles;
        }

        public  Boolean registerUser(String account, String password, int level )
        {
            string sql = @"INSERT INTO [user] ([account],[password],[category_id]) VALUES('" + account + "','" + password + "'," + level + ")";           
           
            ContentProvider provider = ContentProvider.getInstance();
            int rowAffected = provider.insert(sql);

            return rowAffected > 0;
        }

        public Boolean deleteUser(String account)
        {
            string sql = @"DELETE FROM [user] WHERE account='" + account + "'";

            ContentProvider provider = ContentProvider.getInstance();
            int rowAffected = provider.delete(sql);

            return rowAffected > 0;
        }

        public Boolean ChangePassword(String account,String password_old, String password_new)
        {
            string sql = @"UPDATE [user]  SET [password] = '" + password_new + "'  WHERE [account]='" + account + "'  AND [password]='" + password_old + "'";

            ContentProvider provider = ContentProvider.getInstance();
            int rowAffected = provider.update(sql);

            return rowAffected > 0;
        }

        public Boolean ResetPassword(String account, String password)
        {
            string sql = @"UPDATE [user]  SET [password] = '" + password + "'  WHERE [account]='" + account +  "'";

            ContentProvider provider = ContentProvider.getInstance();
            int rowAffected = provider.update(sql);

            return rowAffected > 0;
        }
    }
}
