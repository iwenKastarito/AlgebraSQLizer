using System.Windows;

namespace AlgebraSQLizer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // This method handles the MouseDown event to enable window dragging
        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        // This method handles the Button click to open the Dash window
        private void OpenDashButton_Click(object sender, RoutedEventArgs e)
        {
            // Open the Dash window
            Dash dashWindow = new Dash();
            dashWindow.Show();

            // Optionally, you can close the current window after opening Dash
            // this.Close();
        }
    }
}
