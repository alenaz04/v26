using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using finish.Classes;

namespace finish.Pages
{
    public static class error
    {
        public static int ErrorCount { get; set; } = 0;
    }

    public partial class LoginPage : Page
    {
        private int step = 0;
        private int errors = 0;
        private bool captchaOk = false;

        public LoginPage()
        {
            InitializeComponent();
            LoadCaptcha();
        }

        private void LoadCaptcha()
        {
            step = 0;
            captchaOk = false;
            btnLogin.IsEnabled = false;

            int[] order = { 1, 2, 3, 4 };
            CaptchaGrid.Children.Clear();

            for (int i = 0; i < 4; i++)
            {
                int num = order[i];
                Button btn = new Button();
                btn.Tag = num;
                btn.Width = 100;
                btn.Height = 100;
                btn.Click += (s, e) =>
                {
                    int clicked = (int)((Button)s).Tag;
                    if (clicked == step + 1)
                    {
                        step++;
                        ((Button)s).Background = Brushes.LightGreen;
                        if (step == 4)
                        {
                            captchaOk = true;
                            btnLogin.IsEnabled = true;
                        }
                    }
                    else
                    {
                        errors++;
                        if (errors >= 3)
                        {
                            string login = TxbLog.Text;
                            if (!string.IsNullOrEmpty(login))
                            {
                                var user = AppData.db.User.FirstOrDefault(u => u.login == login);
                                if (user != null)
                                {
                                    user.block = true;
                                    AppData.db.SaveChanges();
                                }
                            }
                            MessageBox.Show("Вы заблокированы. Обратитесь к администратору");
                            btnLogin.IsEnabled = false;
                        }
                        else
                        {
                            MessageBox.Show($"Ошибка. Осталось попыток: {3 - errors}");
                            LoadCaptcha();
                        }
                    }
                };

                string imgPath = $"pack://application:,,,/Images/{num}.png";
                Image img = new Image();
                img.Source = new BitmapImage(new Uri(imgPath));
                img.Stretch = Stretch.Fill;
                btn.Content = img;

                btn.ToolTip = $"Кусок {num}";

                CaptchaGrid.Children.Add(btn);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!captchaOk)
            {
                MessageBox.Show("Соберите пазл");
                return;
            }


            var CurrentUser = AppData.db.User.FirstOrDefault(u => u.login == TxbLog.Text && u.password == TxbPas.Text);

            if (CurrentUser == null)
            {
                error.ErrorCount++;
                if (error.ErrorCount >= 3)
                {
                    var user = AppData.db.User.FirstOrDefault(u => u.login == TxbLog.Text);
                    if (user != null) user.block = true;
                    AppData.db.SaveChanges();
                    MessageBox.Show("Вы заблокированы");
                }
                else
                {
                    MessageBox.Show("Вы ввели неверный логин или пароль. Пожалуйста проверьте ещё раз введенные данные");
                    LoadCaptcha();
                }
            }
            else if (CurrentUser.block == true)
            {
                MessageBox.Show("Вы заблокированы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                error.ErrorCount = 0;
                MessageBox.Show("Вы успешно авторизовались");

                switch (CurrentUser.role_id)
                {
                    case 1: NavigationService.Navigate(new AdminPage()); 
                        break;
                    case 2: NavigationService.Navigate(new MainPage()); 
                        break;
                }
            }
        }
    }
}