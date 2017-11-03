using System.Windows;
using MB_manager.Infrastructure;
using System.ComponentModel;




namespace MB_manager
{
    public partial class MainWindow : Window
    {
        static AccountManager ac_manager;
        static ApiManager api_manager;
        static BackgroundWorker loader_bw;
        static BackgroundWorker request_sender_bw;




        public MainWindow()
        {
            InitializeComponent();

            ac_manager = new AccountManager(accounts_stack);
            api_manager = new ApiManager();

            loader_bw = new BackgroundWorker();
            loader_bw.WorkerReportsProgress = true;
            loader_bw.DoWork += loader_bw_DoWork;
            loader_bw.ProgressChanged += loader_bw_ProgressChanged;
            loader_bw.RunWorkerCompleted += loader_bw_RunWorkerCompleted;

            request_sender_bw = new BackgroundWorker();
            request_sender_bw.WorkerReportsProgress = true;
            request_sender_bw.DoWork += request_sender_bw_DoWork;
            request_sender_bw.ProgressChanged += request_sender_bw_ProgressChanged;
            request_sender_bw.RunWorkerCompleted += request_sender_bw_RunWorkerCompleted;
        }




        private async void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (AccountGrid grid in accounts_stack.Children)
            {
                await grid.Auth();
                grid.Redraw();
            }
        }


        private void AcLoadButton_Click(object sender, RoutedEventArgs e)
        {
            account_loader_button.Visibility = Visibility.Collapsed;
            prog_bar.Visibility = Visibility.Visible;

            account_loader_button.Click -= AcLoadButton_Click;
            loader_bw.RunWorkerAsync();
            account_loader_button.Click += AcLoadButton_Click;

            prog_bar.Visibility = Visibility.Collapsed;
            account_loader_button.Visibility = Visibility.Visible;
        }


        private void req_send_Click(object sender, RoutedEventArgs e)
        {
            req_send_button.Visibility = Visibility.Collapsed;
            req_progbar.Visibility = Visibility.Visible;

            req_send_button.Click -= req_send_Click;
            request_sender_bw.RunWorkerAsync(req_request.Text);
            req_send_button.Click += req_send_Click;

            req_progbar.Visibility = Visibility.Collapsed;
            req_send_button.Visibility = Visibility.Visible;
        }




        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (AccountGrid grid in accounts_stack.Children)
            {
                select_all_button.Click -= SelectAll_Click;

                grid.check_selection.IsChecked = true;
                grid.Redraw();

                select_all_button.Content = "Убрать выделение";
                select_all_button.Click += DeSelectAll_Click;
            }
        }


        private void DeSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (AccountGrid grid in accounts_stack.Children)
            {
                select_all_button.Click -= DeSelectAll_Click;

                grid.check_selection.IsChecked = false;
                grid.Redraw();

                select_all_button.Content = "Выбрвть все";
                select_all_button.Click += SelectAll_Click;
            }
        }

        
        private void inf_save_button_Click(object sender, RoutedEventArgs e)
        {
            ac_manager.SaveDisplayedAccount();
        }


        private void req_clear_Click(object sender, RoutedEventArgs e)
        {
            req_history.Text = "";
        }




        //events for loader_bw
        private void loader_bw_DoWork(object sender, DoWorkEventArgs e)
        {
            ac_manager.LoadBw(loader_bw);
        }


        private void loader_bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            prog_bar.Value = e.ProgressPercentage;
        }


        private void loader_bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ac_manager.Display();
        }


        private void script_button_Click(object sender, RoutedEventArgs e)
        {
            if (ac_manager.account_displayed != null)
                MessageBox.Show(api_manager.SendApi(ac_manager.account_displayed, script_text.Text));
            else
                MessageBox.Show("Ошибка, аккаунт не выбран");
        }




        //events for request_sender_bw
        private void request_sender_bw_DoWork(object sender, DoWorkEventArgs e)
        {
            string request = e.Argument as string;

             e.Result = api_manager.SendApi(ac_manager.accounts_selected, request, request_sender_bw);
        }


        private void request_sender_bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            prog_bar.Value = e.ProgressPercentage;
        }


        private void request_sender_bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            req_history.Text = e.Result as string;
        }
    }
}
