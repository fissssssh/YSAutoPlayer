using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using System.Windows.Interop;

namespace YSAutoPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var handle = new WindowInteropHelper(this).Handle;
            var vm = new MainWindowViewModel();
            DataContext = vm;
            Loaded += (s, e) =>
            {
                vm.RegisterHotkey(handle);
            };
            Unloaded += (s, e) =>
            {
                vm.UnregisterHotkey();
            };
        }
    }
}
