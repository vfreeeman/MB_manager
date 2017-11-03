using System.Collections.Generic;
using System.ComponentModel;
using VK;
using System.Linq;




namespace MB_manager.Infrastructure
{
    class ApiManager
    {
        //история для составления подробной статистики
        //добавить позжк
        Dictionary<Account, string> history; 




        public ApiManager()
        {
            history = new Dictionary<Account, string>();
        }




        public string SendApi(Account account, string requset)
        {
            if (account.is_auth)
            {
                try
                {
                    ApiResponse response;
                    response = account.api.ApiMethod($"https://api.vk.com/method/{ParseRequest(requset)}&access_token={account.token}");
                    return response.tokens.ToString();
                }
                catch
                {
                    return "сервер вернул http ошибку";
                }
            }
            else
                return "";
        }


        public string SendApi(List<Account> list, string request, BackgroundWorker bw)
        {
            string res = "";
            string buf;

            for(int i = 0; i< list.Count; i++)
            {
                buf = SendApi(list[i], request) + "\r\n";
                res += $"{i}. {list[i].fname} {list[i].lname}:\r\n{buf}";

                if (!history.Keys.Contains(list[i]))
                    history[list[i]] = buf;
                else
                    history[list[i]] += buf;

                bw.ReportProgress((i + 1) / list.Count * 100);
            }

            return res;
        }




        //предварительная обработка запроса, замен параметров и все такое
        //добавить позже
        string ParseRequest(string request)
        {
            return request;
        }
    }
}
