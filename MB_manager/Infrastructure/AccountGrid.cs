using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Imaging;




namespace MB_manager.Infrastructure
{
    class AccountGrid : Grid
    {
        public Account account_linked;
        AccountManager ac_manager;
        public CheckBox check_selection;
        Label label;
        bool? is_auth;



        public AccountGrid(Account account, AccountManager ac_manager) : base()
        {
            account_linked = account;
            this.ac_manager = ac_manager;
            Height = 40;
            Margin = new System.Windows.Thickness(5);

            Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            check_selection = new CheckBox() { IsChecked = false,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                VerticalAlignment = System.Windows.VerticalAlignment.Center };
            check_selection.Checked += Selection_checked;
            Children.Add(check_selection);

            label = new Label() { Content = $"{account_linked.fname} {account_linked.lname}",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center };
                label.MouseDoubleClick += ShowAccount;
            Children.Add(label);            
        }


        public void Redraw()
        {
            if (is_auth == true)
                Background = new SolidColorBrush(Color.FromRgb(76, 187, 23));
            if (is_auth == false)
                Background = new SolidColorBrush(Color.FromRgb(186, 0, 0));
        }


        public Task Auth()
        {
            return Task.Run(() => { is_auth = account_linked.Auth(); });
        }


        public void Selection_checked(object sender, RoutedEventArgs e)
        {
            if (check_selection.IsChecked == true)
                ac_manager.accounts_selected.Add(account_linked);
            if (check_selection.IsChecked == false)
                ac_manager.accounts_selected.Remove(account_linked);
        }


        public void ShowAccount(object sender, RoutedEventArgs e)
        {
            ac_manager.account_displayed = account_linked;
            MainWindow win = Application.Current.MainWindow as MainWindow;

            win.info_name.Text = $"{account_linked.fname} {account_linked.lname}";
            win.info_pass.Text = account_linked.login;
            win.info_login.Text = account_linked.login;
            win.info_id.Text = account_linked.uid;

            if (account_linked.pic_adr != null)
                win.info_avatar.Source = new BitmapImage(new Uri(account_linked.pic_adr, UriKind.Absolute));
        }
    }
}
