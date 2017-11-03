using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.ComponentModel;
using System.Threading.Tasks;




namespace MB_manager.Infrastructure
{
    class AccountManager
    {
        const string dir_adr = "accs";
        List <Account> accounts_all;
        public List<Account> accounts_selected;
        public StackPanel stack_list;
        public Account account_displayed;




        public AccountManager(StackPanel stack_list)
        {
            accounts_all = new List<Account>();
            accounts_selected = new List<Account>();
            account_displayed = null;

            this.stack_list = stack_list;
        }
        



        public void SaveAccountsSync()
        {
            if (!Directory.Exists(dir_adr))
                Directory.CreateDirectory(dir_adr);

            foreach (Account account in accounts_all)
                account.Save($"{dir_adr}/{account.login}.xml");
        }


        public void SaveDisplayedAccount()
        {
            if (!Directory.Exists(dir_adr))
                Directory.CreateDirectory(dir_adr);

            account_displayed.Save($"{dir_adr}/{account_displayed.login}.xml");
        }



        public void Display()
        {
            if (stack_list != null)
            {
                stack_list.Children.RemoveRange(0, stack_list.Children.Count);

                foreach (Account acc in accounts_all)
                    stack_list.Children.Add(new AccountGrid(acc, this));
            }
        }


        public void LoadBw(BackgroundWorker bw)
        {
            if (Directory.Exists(dir_adr))
            {
                string[] files = Directory.GetFiles(dir_adr);
                Account account;

                accounts_all = new List<Account>();
                accounts_selected = new List<Account>();

                for (int i = 0; i < files.Length; i++)
                {
                    account = Account.Load(files[i]);
                    if (account != null)
                    {
                        accounts_all.Add(account);
                        bw.ReportProgress((i+1)/files.Length*100);
                    }
                }
            }
        }
    }
}
