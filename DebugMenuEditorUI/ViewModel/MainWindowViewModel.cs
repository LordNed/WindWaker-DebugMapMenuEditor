using DebugMenuEditorUI.Model;
using GameFormatReader.Common;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
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

        /// <summary> User has requested that we save the current file under a different name. </summary>
        public ICommand OnRequestFileSaveAs
        {
            get { return new RelayCommand(x => SaveFileAs(), x => LoadedFile != null); }
        }

        /// <summary> The user has requested that we close the application. Save file (if required). </summary>
        public ICommand OnRequestApplicationClose
        {
            get { return new RelayCommand(x => QuitApplication()); }
        }

        ///// <summary> Add a new category to the loaded <see cref="Menu"/>. </summary>
        //public ICommand OnRequestNewCategory
        //{
        //    get { return new RelayCommand(x => LoadedFile.CreateNewCategory(), x => LoadedFile != null); }
        //}

        ///// <summary> Add a new entry to the currently selected category. </summary>
        //public ICommand OnRequestNewEntry
        //{
        //    get { return new RelayCommand(x => LoadedFile.SelectedCategory.CreateNewEntry(), x => LoadedFile != null && LoadedFile.SelectedCategory != null); }
        //}

        ///// <summary> Take the currently selected entries from the List and add them to the copy buffer. </summary>
        //public ICommand OnRequestCopyEntries;

        ///// <summary> Take the current entries in the copy buffer and paste them into the currently selected <see cref="Category"/>. </summary>
        //public ICommand OnRequestPasteEntries;

        ///// <summary> Delete the currently selected category. </summary>
        //public ICommand OnRequestDeleteCategory
        //{
        //    get { return new RelayCommand(x => LoadedFile.Categories.Remove(LoadedFile.SelectedCategory), x => LoadedFile != null && LoadedFile.SelectedCategory != null); }
        //}

        ///// <summary> Delete the currently selected entries from the currently selected category. </summary>
        //public ICommand OnRequestDeleteEntries
        //{
        //    get { return new RelayCommand(x => LoadedFile.SelectedCategory.RemoveRange(LoadedFile.SelectedCategory.SelectedEntries)); }
        //}

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
        /// Indicates the last action performed by the application and displayed in the lower corner of the UI.
        /// </summary>
        public string ApplicationStatus
        {
            get { return m_applicationStatus; }
            set
            {
                m_applicationStatus = value;
                OnPropertyChanged("ApplicationStatus");
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
                UpdateWindowTitle();

                if(m_loadedFile != null)
                {
                    if (LoadedFile.Categories.Count > 0)
                        CategoryViewModel.SelectedCategory = LoadedFile.Categories[0];
                }
                else
                {
                    CategoryViewModel.SelectedCategory = null;
                }

                OnPropertyChanged("LoadedFile");
            }
        }

        public CategoryViewModel CategoryViewModel { get; private set;}

        private string m_windowTitle;
        private string m_applicationStatus;
        private Menu m_loadedFile;

        public MainWindowViewModel()
        {
            CategoryViewModel = new CategoryViewModel();
            UpdateWindowTitle();
        }

        /// <summary> User has requested that we create a new <see cref="Menu"/> file from scratch. Save current, then create new. </summary>
        private void CreateNewFile()
        {
            ConfirmSaveDesireAndSave();
            LoadedFile = new Menu();
            ApplicationStatus = string.Format("Created {0}", LoadedFile.FileName);
        }

        /// <summary> User has requested that we open a <see cref="Menu"/> file from disk. Ask user for file, save current, then load. </summary>
        private void OpenFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ValidateNames = true;
            ofd.Filter = "Debug Menu Files (*.dat)|*.dat|All Files (*.*)|*.*";
            if(ofd.ShowDialog() == true)
            {
                ConfirmSaveDesireAndSave();

                ApplicationStatus = string.Format("Loading {0}...", ofd.SafeFileName);
                string folderName = Path.GetDirectoryName(ofd.FileName);
                string fileName = Path.GetFileName(ofd.FileName);

                Menu newMenu = new Menu();
                newMenu.FileName = fileName;
                newMenu.FolderPath = folderName;

                try
                {
                    using(EndianBinaryReader reader = new EndianBinaryReader(File.Open(ofd.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Endian.Big))
                    {
                        newMenu.Load(reader);
                        LoadedFile = newMenu;
                    }

                }
                catch (Exception ex)
                {
                    ApplicationStatus = string.Format("Exception while loading: {0}", ex.ToString());
                }
                ApplicationStatus = string.Format("Loaded {0}", ofd.SafeFileName);
            }
        }

        /// <summary> User has requested that we close the current file. Save if applicable, then close. </summary>
        private void CloseFile()
        {
            if (LoadedFile == null)
                return;

            ConfirmSaveDesireAndSave();

            ApplicationStatus = string.Format("Unloaded {0}", LoadedFile.FileName);
            LoadedFile = null;
        }

        /// <summary> User has requested that we save the current file. </summary>
        private void SaveFile()
        {
            if (LoadedFile == null)
                throw new InvalidOperationException("No file loaded to save!");

            if (string.IsNullOrEmpty(LoadedFile.FolderPath) || string.IsNullOrEmpty(LoadedFile.FileName))
            {
                // Invoke a save-file dialog and make them choose where to save it.
                SaveFileAs();
            }
            else
            {
                SaveFileAtPath(Path.Combine(LoadedFile.FolderPath, LoadedFile.FileName));
            }
        }

        /// <summary> User has requested that we save the current file under a different name. </summary>
        private void SaveFileAs()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.RestoreDirectory = true;
            sfd.ValidateNames = true;
            sfd.Filter = "Debug Menu Files (*.dat)|*.dat|All Files (*.*)|*.*";

            if(sfd.ShowDialog() == true)
            {
                SaveFileAtPath(sfd.FileName);
            }
        }

        private void SaveFileAtPath(string filePath)
        {
            if (LoadedFile == null)
                throw new InvalidOperationException("No file loaded to save as!");

            string folderName = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileName(filePath);
            LoadedFile.FileName = fileName;
            LoadedFile.FolderPath = folderName;
            UpdateWindowTitle();

            ApplicationStatus = string.Format("Saving file {0}...", fileName);
            try
            {
                using(EndianBinaryWriter writer = new EndianBinaryWriter(File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite), Endian.Big))
                {
                    LoadedFile.Save(writer);
                }
            }
            catch (Exception ex)
            {
                ApplicationStatus = string.Format("Exception while saving: {0}", ex.ToString());
            }

            LoadedFile.FileName = fileName;
            ApplicationStatus = string.Format("Saved file {0}", LoadedFile.FileName);
        }

        /// <summary> The user has requested that we close the application. Save file (if required). </summary>
        private void QuitApplication()
        {
            // Check if they want to save changes to the file before exiting.
            ConfirmSaveDesireAndSave();
            Application.Current.MainWindow.Close();
        }

        private void UpdateWindowTitle()
        {
            if (LoadedFile == null)
            {
                WindowTitle = ApplicationName;
            }
            else
            {
                WindowTitle = string.Format("{0} - {1}", LoadedFile.FileName, ApplicationName);
            }
        }

        private void ConfirmSaveDesireAndSave()
        {
            if (LoadedFile != null)
            {
                // Ask the user if they'd like to save before making a new file.
                if (System.Windows.MessageBox.Show("Save changes to the file?", ApplicationName, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                {
                    SaveFile();
                }
            }
        }

        internal void OnWindowClosing(object sender, CancelEventArgs e)
        {
            // Check to see if they want to save changes to the file before exiting. 
            ConfirmSaveDesireAndSave();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
