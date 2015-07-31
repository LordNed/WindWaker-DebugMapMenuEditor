using DebugMenuEditorUI.ViewModel;
using System.ComponentModel;
using System.Windows;

namespace DebugMenuEditorUI.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnMainWindowClosing(object sender, CancelEventArgs e)
        {
            ((MainWindowViewModel)DataContext).OnWindowClosing(sender, e);
        }
    }
}
