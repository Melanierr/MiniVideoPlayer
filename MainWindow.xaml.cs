using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Media;
namespace MiniVideoWidget{
    public partial class MainWindow : Window{

    [StructLayout(LayoutKind.Sequential)]
    private struct CursorPoint
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out CursorPoint point);

    private double resizeStartWidth;
    private double resizeExtraWidth;
    private double resizeExtraHeight;
    private CursorPoint resizeStartCursor;

        private bool hasVideo = false;
        private bool minimalMode = false;
        private double videoRatio = 16.0 / 9.0;
        public MainWindow()
        {
            InitializeComponent();

            Player.Volume = 0.5;
            ToggleMinimalMode();
        }

        private void Header_MouseLeftButtonDown(
            object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void Video_MouseLeftButtonDown(
            object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void Video_MouseRightButtonUp(
            object sender, MouseButtonEventArgs e)
        {
            ToggleMinimalMode();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Open_Click(object sender, RoutedEventArgs e){
            OpenFileDialog dialog = new OpenFileDialog{
                Title = "Choose a video",
                Filter ="Video files|*.mp4;*.wmv;*.avi;*.mov;*.m4v|" + "All files|*.*"
            };

            if (dialog.ShowDialog() != true)
                return;

            hasVideo = false;
            Player.Stop();

            Player.Source = new Uri(
                dialog.FileName,
                UriKind.Absolute
            );

            FileLabel.Text = Path.GetFileName(dialog.FileName);
            Player.Play();
        }

        private void Player_MediaOpened(object sender, RoutedEventArgs e){
            double videoWidth = Player.NaturalVideoWidth;
            double videoHeight = Player.NaturalVideoHeight;

            if (videoWidth <= 0 || videoHeight <= 0)
                return;

            hasVideo = true;
            videoRatio = videoWidth / videoHeight;

            Dispatcher.BeginInvoke(new Action(() =>{
                double extraWidth = Math.Max(
                    0, ActualWidth - VideoContainer.ActualWidth
                );

                double extraHeight = Math.Max(
                    0, ActualHeight - VideoContainer.ActualHeight
                );

                double maxWidth =
                    SystemParameters.WorkArea.Width * 0.7;

                double maxHeight =
                    SystemParameters.WorkArea.Height * 0.7;

                double scale = Math.Min(
                    1.0,
                    Math.Min(
                        (maxWidth - extraWidth) / videoWidth,
                        (maxHeight - extraHeight) / videoHeight
                    )
                );

                double minVideoWidth = Math.Min(
                    160,
                    Math.Min(
                        maxWidth - extraWidth,
                        (maxHeight - extraHeight) * videoRatio
                    )
                );

                double targetWidth = Math.Max(
                    minVideoWidth,
                    videoWidth * scale
                );

                double targetHeight = targetWidth / videoRatio;

                MinWidth = 0;
                MinHeight = 0;

                Width = targetWidth + extraWidth;
                Height = targetHeight + extraHeight;

            }), DispatcherPriority.Loaded);
        }

        private void ResizeGrip_DragStarted(object sender, DragStartedEventArgs e){
            resizeStartWidth = Width;

            resizeExtraWidth = Math.Max(
                0, ActualWidth - VideoContainer.ActualWidth
            );

            resizeExtraHeight = Math.Max(
                0, ActualHeight - VideoContainer.ActualHeight
            );

            GetCursorPos(out resizeStartCursor);
        }

    private void ResizeGrip_DragDelta(object sender, DragDeltaEventArgs e){
        if (!GetCursorPos(out CursorPoint cursor))
            return;

        double dpiScale = VisualTreeHelper.GetDpi(this).DpiScaleX;

        double horizontalChange =
            (cursor.X - resizeStartCursor.X) / dpiScale;

        double verticalChange =
            (cursor.Y - resizeStartCursor.Y) / dpiScale;

        double widthChange =
            Math.Abs(horizontalChange) >=
            Math.Abs(verticalChange * videoRatio)
                ? horizontalChange
                : verticalChange * videoRatio;

        double maxVideoWidth = Math.Min(
            SystemParameters.WorkArea.Width - resizeExtraWidth,
            (SystemParameters.WorkArea.Height - resizeExtraHeight)
                * videoRatio
        );

        maxVideoWidth = Math.Max(1, maxVideoWidth);

        double minVideoWidth = Math.Min(160, maxVideoWidth);

        double newVideoWidth = Math.Clamp(
            resizeStartWidth - resizeExtraWidth + widthChange,
            minVideoWidth,
            maxVideoWidth
        );

        Width = newVideoWidth + resizeExtraWidth;

        Height =
            newVideoWidth / videoRatio + resizeExtraHeight;
    }


        private void ToggleMinimalMode(){
            double oldVideoHeight = VideoContainer.ActualHeight;

            minimalMode = !minimalMode;

            if (minimalMode)
            {
                HeaderPanel.Visibility = Visibility.Collapsed;
                ControlsPanel.Visibility = Visibility.Collapsed;

                HeaderRow.Height = new GridLength(0);
                ControlsRow.Height = new GridLength(0);
            }
            else
            {
                HeaderPanel.Visibility = Visibility.Visible;
                ControlsPanel.Visibility = Visibility.Visible;

                HeaderRow.Height = new GridLength(34);
                ControlsRow.Height = GridLength.Auto;
            }
            if (IsLoaded && oldVideoHeight > 0)
            {
                UpdateLayout();

                double extraHeight = Math.Max(
                    0, ActualHeight - VideoContainer.ActualHeight
                );

                Height = oldVideoHeight + extraHeight;
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e){
            if (hasVideo)
                Player.Play();
        }

        private void Pause_Click(object sender, RoutedEventArgs e){
            if (hasVideo)
                Player.Pause();
        }

        private void Stop_Click(object sender, RoutedEventArgs e){
            if (hasVideo)
                Player.Stop();
        }

        private void Player_MediaEnded(object sender, RoutedEventArgs e){
            if (LoopCheck.IsChecked == true)
            {
                Player.Position = TimeSpan.Zero;
                Player.Play();
            }
            else
            {
                Player.Stop();
            }
        }

        private void Player_MediaFailed(object sender, ExceptionRoutedEventArgs e){
            hasVideo = false;
            Player.Close();

            FileLabel.Text = "Playback failed";

            MessageBox.Show(
                e.ErrorException?.Message ??
                "Unable to play this video.",
                "Playback Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
        }

        private void Topmost_Changed(object sender, RoutedEventArgs e){
            if (TopmostCheck != null)
                Topmost = TopmostCheck.IsChecked == true;
        }

        private void Volume_Changed(object sender, RoutedPropertyChangedEventArgs<double> e){
            if (Player != null)
                Player.Volume = e.NewValue / 100.0;
        }

        protected override void OnClosed(EventArgs e)
        {
            Player.Close();
            base.OnClosed(e);
        }

      
        private void Video_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e){
            if (e.ChangedButton != MouseButton.Left) return;
            if (e.OriginalSource is DependencyObject source){
                DependencyObject? current = source;
                while (current != null){
                    if (current is Thumb)
                        return;

                    if (current == VideoContainer)
                        break;
                    current = VisualTreeHelper.GetParent(current);
              }
          }

          DragMove();
          e.Handled = true;
      }


      private void Video_PreviewMouseRightButtonUp(
          object sender, MouseButtonEventArgs e)
      {
          ToggleMinimalMode();
          e.Handled = true;
      }
      
    }
}
