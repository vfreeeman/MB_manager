using System;
using VK;
using System.IO;
using System.Xml.Serialization;




namespace MB_manager.Infrastructure
{
    [Serializable]
    public class Account
    {
        [NonSerialized]
        public ApiVK api = new ApiVK();
        [NonSerialized]
        public bool is_auth = false;
        public string token;
        public string login;
        public string pass;
        public string[] groups;
        public string fname;
        public string lname;
        public string uid;
        public string pic_adr;



        
        public bool Auth()
        {
            try
            {
                token = api.Auth(login, pass, "274556")[0];
                return is_auth = true;
            }
            catch
            {
                return is_auth = false;
            }
        }


        static public Account Load(string adr)
        {
            XmlSerializer formatter = new XmlSerializer(typeof(Account));

            using (FileStream fs = new FileStream(adr, FileMode.OpenOrCreate))
                try
                {
                    return (Account)formatter.Deserialize(fs);
                }
                catch
                {
                    return null;
                }
        }


        public void Save(string adr)
        {
            File.Delete($"{adr}");
            XmlSerializer formatter = new XmlSerializer(typeof(Account));

            using (FileStream fs = new FileStream($"{adr}", FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                formatter.Serialize(fs, this);
        }
    }
}
