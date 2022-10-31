using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;


namespace sample
{
    internal class SqlController
    {
        public static string connection = Strings.Tokens.MySqlConnection;
        public static MySqlConnection database = new MySqlConnection(connection);
        public static MySqlCommand command = null;

        public SqlController()
        {
            if (database.State == System.Data.ConnectionState.Closed)
                database.OpenAsync().Wait();
        }

        public static async Task insert(string insert)
        {

            using (var conn = new MySqlConnection(connection))
            {
                using (MySqlCommand comm = new MySqlCommand(insert, conn))
                {
                    await conn.OpenAsync();

                    await comm.ExecuteNonQueryAsync();

                    await conn.CloseAsync();

                }
            }
        }

        public static async Task<T> select<T>(string select)
        {
            T output;
            using (var conn = new MySqlConnection(connection))
            {
                using (MySqlCommand comm = new MySqlCommand(select, conn))
                {
                    await conn.OpenAsync();
                    using (MySqlDataReader reader = (MySqlDataReader)await comm.ExecuteReaderAsync())
                    {


                        await reader.ReadAsync();

                        output = (T)reader[0];

                        await reader.CloseAsync();


                    }
                    await conn.CloseAsync();
                }
            }
            return output;
        }
        
        public static async Task<User> GetUser(long chatId)
        {
            User user = new User();
            using (var conn = new MySqlConnection(connection))
            {
                using (MySqlCommand comm = new MySqlCommand($"SELECT * FROM users WHERE chatId={chatId} LIMIT 1", conn))
                {
                    await conn.OpenAsync();
                    using (MySqlDataReader reader = (MySqlDataReader)await comm.ExecuteReaderAsync())
                    {


                        await reader.ReadAsync();

                        user.ChatId = (long)reader[0];
                        user.Code = (int)reader[1];
                        user.CommandLine = (string)reader[2];
                        user.IsAdmin = (bool)reader[3];
                        user.Fio = (string)reader[4];
                        user.University = (string)reader[5];
                        user.Group = (string)reader[6];
                        user.Birth = (string)reader[7];
                        user.Contact = (string)reader[8];
                        user.NeedScores = (bool)reader[9];
                        user.IsRegEnd = (bool)reader[10];
                        user.tg = (string)reader[11];

                        user.ExelId = (string)reader[12];
                        await reader.CloseAsync();


                    }
                    await conn.CloseAsync();
                }
            }

            return user;
        }
        public static async Task<User> GetUserCode(int code)
        {
            User user = new User();
            using (var conn = new MySqlConnection(connection))
            {
                using (MySqlCommand comm = new MySqlCommand($"SELECT * FROM users WHERE code={code} LIMIT 1", conn))
                {
                    await conn.OpenAsync();
                    using (MySqlDataReader reader = (MySqlDataReader)await comm.ExecuteReaderAsync())
                    {


                        await reader.ReadAsync();

                        user.ChatId = (long)reader[0];
                        user.Code = (int)reader[1];
                        user.CommandLine = (string)reader[2];
                        user.IsAdmin = (bool)reader[3];
                        user.Fio = (string)reader[4];
                        user.University = (string)reader[5];
                        user.Group = (string)reader[6];
                        user.Birth = (string)reader[7];
                        user.Contact = (string)reader[8];
                        user.NeedScores = (bool)reader[9];
                        user.IsRegEnd = (bool)reader[10];
                        user.tg = (string)reader[11];
                        user.ExelId = (string)reader[12];

                        await reader.CloseAsync();


                    }
                    await conn.CloseAsync();
                }
            }

            return user;
        }
 

        public static async Task<Admin> GetAdmin(long chatId)
        {
            Admin admin = new Admin();
            using (var conn = new MySqlConnection(connection))
            {
                using (MySqlCommand comm = new MySqlCommand($"SELECT * FROM admins WHERE chatId={chatId} LIMIT 1", conn))
                {
                    await conn.OpenAsync();
                    using (MySqlDataReader reader = (MySqlDataReader)await comm.ExecuteReaderAsync())
                    {


                        await reader.ReadAsync();

                        admin.chatId = (long)reader[0];
                        admin.commandLine=(string)reader[1];    
                        admin.isMainAdmin = (bool)reader[2];    

     

                        await reader.CloseAsync();


                    }
                    await conn.CloseAsync();
                }
            }

            return admin;
        }
        public static async Task Update(string update)
        {
            using (var conn = new MySqlConnection(connection))
            {

                using (MySqlCommand comm = new MySqlCommand(update, conn))
                {
                    await conn.OpenAsync();

                    await comm.ExecuteNonQueryAsync();

                    await conn.CloseAsync();

                }

            }
        }

       
    }
}
