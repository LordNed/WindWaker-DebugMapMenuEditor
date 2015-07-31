using DebugMenuEditorUI.Model;
using DebugMenuEditorUI.ViewModel;
using GameFormatReader.Common;
using System.ComponentModel;
using System.IO;
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

        private void OnShowHelpDialog(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (EndianBinaryReader reader = new EndianBinaryReader(File.Open(@"C:\Users\Helios\Downloads\Menu1_Out.dat", FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Endian.Big))
            {
                Menu LoadedFile = new Menu();
                LoadedFile.Load(reader);
                ((MainWindowViewModel)DataContext).LoadedFile = LoadedFile;
            }

        }
    }
}
