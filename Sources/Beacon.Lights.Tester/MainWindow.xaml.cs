using System.Linq;
using System.Windows;

using Beacon.Core;

namespace Beacon.Lights.Tester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly LightFactory lightFactory = new LightFactory();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            deviceList.ItemsSource = lightFactory.SupportedDevices;
            deviceList.SelectedItem = lightFactory.SupportedDevices.First();
        }

        private void OffButton_Click(object sender, RoutedEventArgs e)
        {
            Device.NoStatus();
        }

        private void SuccessButton_Click(object sender, RoutedEventArgs e)
        {
            Device.Success();
        }

        private void FailedButton_Click(object sender, RoutedEventArgs e)
        {
            Device.Fail();
        }

        private void InvestigateButton_Click(object sender, RoutedEventArgs e)
        {
            Device.Investigate();
        }

        private IBuildLight Device
        {
            get { return lightFactory.CreateLight((string) deviceList.SelectedItem); }
        }
    }
}