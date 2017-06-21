using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using AForge.Video;
using AForge.Video.DirectShow;

namespace AForge.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<FilterInfo> VideoDevices { get; set; }

        public FilterInfo CurrentDevice
        {
            get => _currentDevice;
            set { _currentDevice = value; OnPropertyChanged("CurrentDevice"); }
        }
        private FilterInfo _currentDevice;

        public Presenter Presenter { get; set; }


        private IVideoSource _videoSource;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            GetVideoDevices();
            Closing += MainWindow_Closing;

        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            StopCamera();
            Presenter.Close();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            StartCamera();
        }

        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                BitmapImage bi;
                using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    bi = bitmap.ToBitmapImage();
                }
                bi.Freeze(); // avoid cross thread operations and prevents leaks
                Dispatcher.BeginInvoke(new ThreadStart(delegate { Presenter.VideoPlayer.Source = bi; }));
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error on _videoSource_NewFrame:\n" + exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StopCamera();
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            StopCamera();
        }

        private void GetVideoDevices()
        {
            VideoDevices = new ObservableCollection<FilterInfo>();
            foreach (FilterInfo filterInfo in new FilterInfoCollection(FilterCategory.VideoInputDevice))
            {
                VideoDevices.Add(filterInfo);
            }
            if (VideoDevices.Any())
            {
                CurrentDevice = VideoDevices[0];
            }
            else
            {
                MessageBox.Show("No video sources found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartCamera()
        {
            if (CurrentDevice != null)
            {
                _videoSource = new VideoCaptureDevice(CurrentDevice.MonikerString);
                _videoSource.NewFrame += video_NewFrame;
                _videoSource.Start();
            }
        }

        private void StopCamera()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.SignalToStop();
                _videoSource.NewFrame -= video_NewFrame;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {

            FileInfo bannerPath = new FileInfo($"Resources/{Settings.Default.BannerPath[0]}.png");
            FileInfo logoPath = new FileInfo($"Resources/{Settings.Default.LogoPath}.png");
            Presenter = new Presenter
            {
                Presentation = {Source = new BitmapImage(new Uri(bannerPath.FullName))},
                Logo = {Source = new BitmapImage(new Uri(logoPath.FullName))}
            };
            Presenter.Show();

            this.Banners.ItemsSource = Settings.Default.BannerPath;
            this.Banners.SelectionChanged += Banners_SelectionChanged;


        }

        private void Banners_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            FileInfo logoPath = new FileInfo($"Resources/{this.Banners.SelectedItems[0]}.png");
            if (logoPath.Exists)
            {
                Presenter.Presentation.Source = new BitmapImage(new Uri(logoPath.FullName));
            }
        }
    }
}
