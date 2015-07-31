using DebugMenuEditorUI.Model;
using System.ComponentModel;
using System.Windows.Input;

namespace DebugMenuEditorUI.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public const string ApplicationName = "Debug Menu Editor";

        #region Command Callbacks
        public ICommand OnRequestNewFile;
        public ICommand OnRequestFileOpen;
        public ICommand OnRequestFileClose;
        public ICommand OnRequestFileSave;
        public ICommand OnRequestFileSaveAs;
        public ICommand OnRequestApplicationClose;
        public ICommand OnRequestNewCategory;
        public ICommand OnRequestNewEntry;
        public ICommand OnRequestCopyEntries;
        public ICommand OnRequestPasteEntries;
        public ICommand OnRequestDeleteCategory;
        public ICommand OnRequestDeleteEntries;
        public ICommand OnRequestHelp;

        #endregion

        public string WindowTitle
        {
            get { return m_windowTitle; }
            set
            {
                m_windowTitle = value;
                OnPropertyChanged("WindowTitle");
            }
        }

        public Menu LoadedFile
        {
            get { return m_loadedFile; }
            set
            {
                m_loadedFile = value;
                OnPropertyChanged("LoadedFile");
            }
        }


        public MainWindowViewModel()
        {

        }


        private void UpdateWindowTitle()
        {
            if(LoadedFile == null)
            {
                WindowTitle = ApplicationName;
            }
            else
            {
                WindowTitle = string.Format("{0} - {1}", LoadedFile.FileName, ApplicationName);
            }
        }

        internal void OnWindowClosing(object sender, CancelEventArgs e)
        {
            // Check to see if they want to save changes to the file before exiting. 
            if (LoadedFile != null)
            {
                if (System.Windows.MessageBox.Show("Save changes to the file?", ApplicationName, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                {
                    //e.Cancel = true;
                    //return;
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
