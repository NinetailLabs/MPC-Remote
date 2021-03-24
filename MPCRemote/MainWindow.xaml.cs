using System;
using System.Threading;
using System.Windows;

namespace MPCRemote
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            _context = new RemoteContext();
            _lastPosition = string.Empty;
            DataContext = _context;

            _updateTimer = new Timer(UpdateCallback, null, TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(0.5));
        }

        /// <summary>
        /// Occurs when the update timer elapses
        /// </summary>
        /// <param name="obj">Always null</param>
        private void UpdateCallback(object? obj)
        {
            if(_lastPosition.Equals(_context.Position))
            {
                return;
            }

            Dispatcher.Invoke(() =>
            {
                PositionDisplay.Text = _context.Position;
                _lastPosition = _context.Position;
            });
        }

        private string _lastPosition;

        /// <summary>
        /// Occurs when the user changes the slider value
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(_movingSlider)
            {
                _context.SeekToPosition((long)e.NewValue);
            }
        }

        /// <summary>
        /// RemoteContext instance
        /// </summary>
        private RemoteContext _context;

        /// <summary>
        /// Indicate if the slider is being moved by the user
        /// </summary>
        private bool _movingSlider;

        /// <summary>
        /// Timer used to update the position
        /// </summary>
        private Timer _updateTimer;

        /// <summary>
        /// Occurs when the user clicks on the slider
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event argument</param>
        private void Slider_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _movingSlider = true;
        }

        /// <summary>
        /// Occurs when the user is no longer clicking on the slider
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        private void Slider_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _movingSlider = false;
        }

        /// <summary>
        /// Occurs when the user double clicks on a listbox entry
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        private void ListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var itemIndex = DataGridPlaylist.SelectedIndex;
            _context.PlayFileInPlaylist(itemIndex);
        }
    }
}
