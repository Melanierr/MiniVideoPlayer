using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace MiniVideoWidget {
  public partial class MainWindow: Window {
    private bool hasVideo = false;
    private bool minimalMode = false;

    public MainWindow() {
      InitializeComponent();
      Player.Volume = 0.5;
      ToggleMinimalMode();
    }
    private void Header_MouseLeftButtonDown(
      object sender,
      MouseButtonEventArgs e) {
      if (e.ChangedButton == MouseButton.Left) {
        DragMove();
      }
    }

    private void Minimize_Click(
      object sender,
      RoutedEventArgs e) {
      WindowState = WindowState.Minimized;
    }

    private void Close_Click(
      object sender,
      RoutedEventArgs e) {
      Close();
    }
    private void Open_Click(
      object sender,
      RoutedEventArgs e) {
      OpenFileDialog dialog = new OpenFileDialog {
        Title = "Choose a video",
          Filter =
          "Video files|*.mp4;*.wmv;*.avi;*.mov;*.m4v|" +
          "All files|*.*"
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
      hasVideo = true;

      Player.Play();
    }
    private void Play_Click(
      object sender,
      RoutedEventArgs e) {
      if (hasVideo)
        Player.Play();
    }

    private void Pause_Click(
      object sender,
      RoutedEventArgs e) {
      if (hasVideo)
        Player.Pause();
    }

    private void Stop_Click(
      object sender,
      RoutedEventArgs e) {
      if (hasVideo)
        Player.Stop();
    }

    private void Player_MediaEnded(
      object sender,
      RoutedEventArgs e) {
      if (LoopCheck.IsChecked == true) {
        Player.Position = TimeSpan.Zero;
        Player.Play();
      } else {
        Player.Stop();
      }
    }

    private void Player_MediaFailed(
      object sender,
      ExceptionRoutedEventArgs e) {
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

    private void Topmost_Changed(
      object sender,
      RoutedEventArgs e) {
      if (TopmostCheck != null)
        Topmost = TopmostCheck.IsChecked == true;
    }

    private void Volume_Changed(
      object sender,
      RoutedPropertyChangedEventArgs < double > e) {
      if (Player != null)
        Player.Volume = e.NewValue / 100.0;
    }

    protected override void OnClosed(EventArgs e) {
      Player.Close();
      base.OnClosed(e);
    }
    private void ToggleMinimalMode() {
      minimalMode = !minimalMode;

      if (minimalMode) {
        HeaderPanel.Visibility = Visibility.Collapsed;
        ControlsPanel.Visibility = Visibility.Collapsed;

        HeaderRow.Height = new GridLength(0);
        ControlsRow.Height = new GridLength(0);
      } else {
        HeaderPanel.Visibility = Visibility.Visible;
        ControlsPanel.Visibility = Visibility.Visible;

        HeaderRow.Height = new GridLength(34);
        ControlsRow.Height = GridLength.Auto;
      }
    }
    private void Video_MouseLeftButtonDown(
      object sender,
      MouseButtonEventArgs e) {
      if (e.ChangedButton == MouseButton.Left) {
        DragMove();
      }
    }
    private void Video_MouseRightButtonUp(
      object sender,
      MouseButtonEventArgs e) {
      ToggleMinimalMode();
    }
  }
}