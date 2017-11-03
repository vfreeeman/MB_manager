using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Threading;




namespace VK
{
    public class ApiResponse
    {
        public string request;
        public bool isCorrect;
        public JToken tokens;



        public ApiResponse()
        {

        }


        public ApiResponse(bool isCorrect, JObject response, string request)
        {
            this.request = request;
            this.isCorrect = isCorrect;
            if (isCorrect)
                this.tokens = response["response"];
            else
                this.tokens = response["error"];
        }
    }




    public class ApiVK
    {
        public int max_req_count;
        private int requesr_control_counter;
        private DateTime lastRequestTime;




        public ApiVK()
        {
            max_req_count = 2;
            requesr_control_counter = 0;
        }


        public ApiVK(int max_req_count)
        {
            this.max_req_count = max_req_count;
            requesr_control_counter = 0;
        }




        string cookiestring(List<string> list)
        {
            string cookiestr = "";
            foreach (string str in list)
                cookiestr = cookiestr + str + ";";
            return cookiestr;
        }


        public string[] Auth(string login, string password, string scope)
        {
            byte[] byteData;
            Stream dataWriter;
            StreamReader dataReader;
            string location, html, postData = "";
            string[] cookies;
            Match matchValue, matchName;
            Regex value, name;
            List<string> allCookies = new List<string>();  //разкомментить потом
            lastRequestTime = DateTime.UtcNow;

            //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            HttpWebRequest request1 = (HttpWebRequest)HttpWebRequest.Create($"https://oauth.vk.com/authorize?client_id=5635484&redirect_uri=https://oauth.vk.com/blank.html&scope={scope}&response_type=token&v=5.53&display=wap");
            HttpWebResponse response1 = (HttpWebResponse)request1.GetResponse();

            dataReader = new StreamReader(response1.GetResponseStream());
            html = dataReader.ReadToEnd();
            dataReader.Close();

            value = new Regex(@"<input[^>]+value\s*=\s*""(\S*)""[^>]*>", RegexOptions.Multiline | RegexOptions.Singleline);
            name = new Regex(@"<input[^>]+name\s*=\s*""(\S*)""[^>]*>", RegexOptions.Multiline | RegexOptions.Singleline);
            matchValue = value.Match(html);
            matchName = name.Match(html);

            for (int i = 0; i < 4; i++)
            {
                postData = postData + matchName.Groups[1].Value + "=" + matchValue.Groups[1].Value + "&";
                matchName = matchName.NextMatch();
                matchValue = matchValue.NextMatch();
            }

            postData = postData + "email=" + login + "&pass=" + password;
            if (html.Contains("sid"))
            {
                string str = Console.ReadLine();
                return (new string[] { str, "none", login, password, scope });
            }


            //--------------------------------------------------------------------отправка формы с паролем--------------------------------------------------------------------------------------------------------------
            request1 = (HttpWebRequest)HttpWebRequest.Create("https://login.vk.com/?act=login&soft=1");
            request1.AllowAutoRedirect = false;

            cookies = response1.Headers["Set-Cookie"].Split(';', ',');
            allCookies.Add(cookies[0]);
            allCookies.Add(cookies[5]);

            request1.Headers["Cookie"] = cookiestring(allCookies);
            request1.Method = "POST";

            byteData = Encoding.UTF8.GetBytes(postData);
            dataWriter = request1.GetRequestStream();
            dataWriter.Write(byteData, 0, byteData.Length);
            dataWriter.Close();

            response1 = (HttpWebResponse)request1.GetResponse();
            location = response1.Headers["Location"];
            //--------------------------------------------------------------------переход по locations------------------------------------------------------------------------------------------------------------------
            request1 = (HttpWebRequest)HttpWebRequest.Create(location);
            request1.AllowAutoRedirect = false;

            cookies = response1.Headers["Set-Cookie"].Split(';', ',');
            allCookies.Add(cookies[0]);
            allCookies.Add(cookies[6]);
            allCookies.Add(cookies[13]);
            allCookies.Add(cookies[20]);
            allCookies.Add(cookies[27]);
            request1.Headers["Cookie"] = cookiestring(allCookies);

            response1 = (HttpWebResponse)request1.GetResponse();
            location = response1.Headers["Location"];
            dataReader = new StreamReader(response1.GetResponseStream());
            html = dataReader.ReadToEnd();
            dataReader.Close();

            if (html == "")//если не неужно подтверждение, то продолжаем стандартную авторизацию
            {
                request1 = (HttpWebRequest)HttpWebRequest.Create(location);
                request1.AllowAutoRedirect = false;

                cookies = response1.Headers["Set-Cookie"].Split(';', ',');
                allCookies.Add(cookies[0]);
                allCookies.Add(cookies[5]);
                allCookies.Add(cookies[10]);
                allCookies.Add(cookies[15]);
                allCookies.Add(cookies[20]);
                request1.Headers["Cookie"] = cookiestring(allCookies);

                response1 = (HttpWebResponse)request1.GetResponse();
                location = response1.Headers["Location"];
                string[] Temp = location.Split('=', '&');

                return (new string[] { Temp[1], Temp[3], login, password, scope });
            }
            else
            {
                name = new Regex(@"action=\W\b(.+)\b");
                matchName = name.Match(html);
                foreach (Match match in name.Matches(html))
                    location = match.Groups[1].Value;

                //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                request1 = (HttpWebRequest)HttpWebRequest.Create(location);
                request1.Method = "POST";
                request1.AllowAutoRedirect = false;
                request1.ContentLength = 0;
                request1.Headers["Cookie"] = cookiestring(allCookies);

                response1 = (HttpWebResponse)request1.GetResponse();
                location = response1.Headers["Location"];
                string[] Temp = location.Split('=', '&');

                StreamWriter fileDic = new StreamWriter("D:\\token.txt");
                fileDic.WriteLine(Temp[1] + " ");
                fileDic.Flush();

                return (new string[] { Temp[1], Temp[3], login, password, scope });
            }
        }




        private void RequestAcceptionCheck()
        {
            TimeSpan lastRequestTimeSec = DateTime.UtcNow - lastRequestTime;
            if (lastRequestTimeSec.TotalSeconds > 1)
                requesr_control_counter = 0;
            if (requesr_control_counter > max_req_count)
            {
                Thread.Sleep(1000);
                requesr_control_counter = 0;
            }
        }


        private bool CheckResponse(JObject json)
        {
            if (json["error"] != null)
                return false;
            else
                return true;
        }




        public ApiResponse ApiMethod(string request)
        {
            RequestAcceptionCheck();
            requesr_control_counter++;
            HttpWebRequest apiRequest = (HttpWebRequest)HttpWebRequest.Create(request);
            HttpWebResponse apiRespose = (HttpWebResponse)apiRequest.GetResponse();
            StreamReader respStream = new StreamReader(apiRespose.GetResponseStream());
            JObject json = JObject.Parse(respStream.ReadToEnd());

            respStream.Close();
            apiRequest.Abort();
            lastRequestTime = DateTime.UtcNow;
            return new ApiResponse(CheckResponse(json), json, request);
        }


        public ApiResponse ApiMethodPost(Dictionary<string, string> param, string method)
        {
            RequestAcceptionCheck();
            requesr_control_counter++;

            HttpWebRequest apiRequest = (HttpWebRequest)HttpWebRequest.Create(method);
            apiRequest.Method = "POST";
            Stream postWriter = apiRequest.GetRequestStream();
            string postParam = "";

            foreach (string key in param.Keys)
                postParam += $"{key}={param[key]}&";

            byte[] postParamByte = Encoding.UTF8.GetBytes(postParam.Remove(postParam.Length - 1, 1));
            postWriter.Write(postParamByte, 0, postParamByte.Length);
            postWriter.Close();

            HttpWebResponse apiRespose = (HttpWebResponse)apiRequest.GetResponse();
            StreamReader respStream = new StreamReader(apiRespose.GetResponseStream());
            JObject json = JObject.Parse(respStream.ReadToEnd());

            respStream.Close();
            apiRequest.Abort();

            lastRequestTime = DateTime.UtcNow;
            return new ApiResponse(CheckResponse(json), json, method);
        }


        public void ApiMethodEmpty(string request)
        {
            RequestAcceptionCheck();
            requesr_control_counter++;
            HttpWebResponse apiRespose;
            HttpWebRequest apiRequest;
            apiRequest = (HttpWebRequest)HttpWebRequest.Create(request);
            apiRespose = (HttpWebResponse)apiRequest.GetResponse();
            apiRequest.Abort();
            lastRequestTime = DateTime.UtcNow;
        }


        public void ApiMethodPostEmpty(Dictionary<string, string> param, string method)
        {
            RequestAcceptionCheck();
            requesr_control_counter++;

            HttpWebRequest apiRequest = (HttpWebRequest)HttpWebRequest.Create(method);
            apiRequest.Method = "POST";
            Stream postWriter = apiRequest.GetRequestStream();
            string postParam = "";
            foreach (string key in param.Keys)
                postParam += $"{key}={param[key]}&";

            byte[] postParamByte = Encoding.UTF8.GetBytes(postParam.Remove(postParam.Length - 1, 1));
            postWriter.Write(postParamByte, 0, postParamByte.Length);
            postWriter.Close();

            HttpWebResponse apiRespose = (HttpWebResponse)apiRequest.GetResponse();
            apiRequest.Abort();
            lastRequestTime = DateTime.UtcNow;
        }




        public string UploadPhoto(string picAdr, string token)
        {
            ApiResponse response = ApiMethod($"https://api.vk.com/method/photos.getMessagesUploadServer?access_token={token}&v=V5.63");
            string adr = (string)response.tokens["upload_url"];

            if (response.isCorrect) //загрузка пикч в вк
            {
                HttpClient client = new HttpClient();
                MultipartContent content = new MultipartFormDataContent();
                using (var fstream = File.OpenRead(picAdr))
                {
                    var streamContent = new StreamContent(fstream);
                    streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        Name = "photo",
                        FileName = Path.GetFileName(picAdr)
                    };
                    content.Add(streamContent);

                    HttpResponseMessage httpResponse = client.PostAsync(new Uri(adr), content).Result;
                    StreamReader readStream = new StreamReader(httpResponse.Content.ReadAsStreamAsync().Result, Encoding.UTF8);
                    JObject json = JObject.Parse(readStream.ReadToEnd());
                    //JObject json = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                    response = new ApiResponse();
                    response.request = adr;
                    response.isCorrect = (string)json["photo"] != "[]";
                    response.tokens = json;
                }

                if (response.isCorrect)
                    response = ApiMethod($"https://api.vk.com/method/photos.saveMessagesPhoto?access_token={token}&server={response.tokens["server"]}&hash={response.tokens["hash"]}&photo={Convert.ToString(response.tokens["photo"]).Replace("\\", "")}&v=V5.63");
                else
                    throw new Exception("error while processing photos on the server");

                if (response.isCorrect)
                    return (string)response.tokens[0]["id"];
                else
                    throw new Exception("failed to obtain the address of the photo");
            }
            else
                throw new Exception("failed to get the address to download the photos");
        }


        public void SendPhotos(ref List<string> photos, int start, int stop, string message, string uid, string token)
        {
            string attachments = "";

            for (int i = start; i < stop && i < photos.Count; i++)
                attachments += $",{photos[i]}";

            SendPhotos(attachments, message, uid, token);
        }


        public void SendPhotos(string photos, string message, string uid, string token)
        {
            if (photos != "")
                ApiMethodPostEmpty(new Dictionary<string, string>()
                {
                    { "message",message},
                    { "uid",uid},
                    { "attachment", photos.Remove(0,1) },
                    { "access_token",token},
                    { "v","V5.53"}
                },
                    "https://api.vk.com/method/messages.send");
        }
    }
}
