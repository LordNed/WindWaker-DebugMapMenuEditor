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
        /// <summary> User has requested that we create a new <see cref="Menu"/> file from scratch. Save current, then create new. </summary>
        public ICommand OnRequestNewFile
        {
            get { return new RelayCommand(x => CreateNewFile()); }
        }

        /// <summary> User has requested that we open a <see cref="Menu"/> file from disk. Ask user for file, save current, then load. </summary>
        public ICommand OnRequestFileOpen
        {
            get { return new RelayCommand(x => OpenFile()); }
        }

        /// <summary> User has requested that we close the current file. Save if applicable, then close. </summary>
        public ICommand OnRequestFileClose
        {
            get { return new RelayCommand(x => CloseFile(), x => LoadedFile != null); }
        }

        /// <summary> User has requested that we save the current file. </summary>
        public ICommand OnRequestFileSave
        {
            get { return new RelayCommand(x => SaveFile(), x => LoadedFile != null); }
        }

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

        /// <summary>
        /// What is the title of the <see cref="MainWindow"/>? Adjusted based on the name of the currently loaded file.
        /// </summary>
        public string WindowTitle
        {
            get { return m_windowTitle; }
            set
            {
                m_windowTitle = value;
                OnPropertyChanged("WindowTitle");
            }
        }

        /// <summary>
        /// The currently loaded Debug Menu file, null if not loaded.
        /// </summary>
        public Menu LoadedFile
        {
            get { return m_loadedFile; }
            set
            {
                m_loadedFile = value;
                OnPropertyChanged("LoadedFile");
            }
        }

        private string m_windowTitle;
        private Menu m_loadedFile;

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
