using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace ReminderApplication.Models
{
    public class NotificationService
    {
        public void ShowToast(string message, string accentColor)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var toast = new Window
                {
                    SizeToContent = SizeToContent.WidthAndHeight,
                    WindowStyle = WindowStyle.None,
                    AllowsTransparency = true,
                    Background = Brushes.Transparent,
                    Topmost = true,
                    ShowInTaskbar = false,
                    ResizeMode = ResizeMode.NoResize
                };

                var desktopWorkingArea = SystemParameters.WorkArea;
                toast.Left = desktopWorkingArea.Right - 320;
                toast.Top = desktopWorkingArea.Bottom - 150;


                var border = new Border
                {
                    Background = new SolidColorBrush(Colors.White) { Opacity = 0.5 },
                    CornerRadius = new CornerRadius(8),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5ba54d")) { Opacity = 0.2 },
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(15),
                    Effect = new DropShadowEffect
                    {
                        Color = Colors.Black,
                        BlurRadius = 10,
                        ShadowDepth = 3,
                        Opacity = 0.6
                    }
                };
                var textBlock = new TextBlock
                {
                    Text = message,
                    Foreground = Brushes.White,
                    FontSize = 14,
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = 280,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                var contentStack = new StackPanel();
                contentStack.Children.Add(textBlock);
                border.Child = contentStack;
                toast.Content = border;

                toast.Opacity = 0;
                toast.Show();
                DoubleAnimation fadeIn = new DoubleAnimation(1, TimeSpan.FromSeconds(0.3));
                toast.BeginAnimation(Window.OpacityProperty, fadeIn);
                var closeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
                closeTimer.Tick += (s, e) =>
                {
                    closeTimer.Stop();
                    CloseWithAnimation(toast);
                };
                closeTimer.Start();
            });
        }

        private void CloseWithAnimation(Window window)
        {
            DoubleAnimation fadeOut = new DoubleAnimation(0, TimeSpan.FromSeconds(0.5));
            fadeOut.Completed += (s, e) => window.Close();
            window.BeginAnimation(Window.OpacityProperty, fadeOut);
        }
    }
}