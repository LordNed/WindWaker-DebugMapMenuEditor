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
            System.Diagnostics.Process.Start("https://github.com/LordNed/WindWaker-DebugMapMenuEditor/wiki");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (EndianBinaryReader reader = new EndianBinaryReader(File.Open(@"C:\Users\Matt\Documents\Wind Editor\Menu1_Rextract.dat", FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Endian.Big))
            {
                Menu LoadedFile = new Menu();
                LoadedFile.Load(reader);
                ((MainWindowViewModel)DataContext).LoadedFile = LoadedFile;
            }

        }
    }
}
