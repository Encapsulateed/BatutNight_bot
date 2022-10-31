using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sample
{
    internal class Admin
    {
        public long chatId { get; set; }
        public string? commandLine { get; set; }
        public bool isMainAdmin { get; set; }


        public static async Task Registation(long chatId, bool main)
        {
            string insert = $"INSERT IGNORE INTO admins VALUES({(long)chatId},{"NULL"},{main})";
            await SqlController.insert(insert);
        }

        public async Task Update<T>(string param, T value, string where, long key)
        {
            string update = null;

            if (value is string)
                update = $"UPDATE admins SET {param}='{value}' WHERE {where}={key}";
            else
                update = $"UPDATE admins SET {param}={value} WHERE {where}={key}";
            if (update != null)
                await SqlController.Update(update);
        }

        

        public static async Task<Admin> GetAdminByChatId(long chatId)
        {

            return await SqlController.GetAdmin(chatId);
        }

    }
}
