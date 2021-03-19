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
            DataContext = _context;            
        }

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

        private void ListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var itemIndex = DataGridPlaylist.SelectedIndex;
            _context.PlayFileInPlaylist(itemIndex);
        }
    }
}
