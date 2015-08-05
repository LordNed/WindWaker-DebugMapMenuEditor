using DebugMenuEditorUI.Model;
using GameFormatReader.Common;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Data;
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

        /// <summary> Add a new category to the loaded <see cref="Menu"/>. </summary>
        public ICommand OnRequestNewCategory
        {
            get { return new RelayCommand(x => AddCategory(), x => LoadedFile != null); }
        }

        /// <summary> Add a new entry to the currently selected category. </summary>
        public ICommand OnRequestNewEntry
        {
            get { return new RelayCommand(x => CreateNewEntry(), x => LoadedFile != null && CategoryViewModel.SelectedCategory != null); }
        }

        ///// <summary> Take the currently selected entries from the List and add them to the copy buffer. </summary>
        //public ICommand OnRequestCopyEntries;

        ///// <summary> Take the current entries in the copy buffer and paste them into the currently selected <see cref="Category"/>. </summary>
        //public ICommand OnRequestPasteEntries;

        /// <summary> Delete the currently selected category. </summary>
        public ICommand OnRequestDeleteCategory
        {
            get { return new RelayCommand(x => RemoveCategory(), x => LoadedFile != null && CategoryViewModel.SelectedCategory != null); }
        }

        /// <summary> Delete the currently selected entries from the currently selected category. </summary>
        public ICommand OnRequestDeleteEntries
        {
            get { return new RelayCommand(x => RemoveEntry(), x=> LoadedFile != null && CategoryViewModel.SelectedEntryViewModel.SelectedEntry != null); }
        }

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

        public CollectionViewSource CollViewSource { get; set; }

        public string SearchFilter
        {
            get { return m_searchFilter; }
            set
            {
                m_searchFilter = value;
                if (!string.IsNullOrEmpty(m_searchFilter))
                    AddFilter();

                CollViewSource.View.Refresh();
                OnPropertyChanged("SearchFilter");
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

                    // Update our collection source which contains a list of all entries from all categories.
                    List<CategoryEntry> allEntries = new List<CategoryEntry>();
                    foreach(var cat in LoadedFile.Categories)
                    {
                        foreach (var entry in cat.Entries)
                            allEntries.Add(entry);
                    }

                    CollViewSource.Source = allEntries;
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
        private string m_searchFilter;
        private Menu m_loadedFile;

        public MainWindowViewModel()
        {
            CategoryViewModel = new CategoryViewModel(this);
            CollViewSource = new CollectionViewSource();
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

        private void AddCategory()
        {
            if (LoadedFile == null)
                throw new InvalidOperationException("Cannot add category when no file is loaded!");

            Category newCategory = new Category();
            LoadedFile.Categories.Add(newCategory);
            CategoryViewModel.SelectedCategory = newCategory;
        }

        private void RemoveCategory()
        {
            if(LoadedFile == null)
                throw new InvalidOperationException("Cannot remove category when no file is loaded!");

            // Get the index of the currently selected one so that when we remove it, we can select the category one before.
            int index = LoadedFile.Categories.IndexOf(CategoryViewModel.SelectedCategory);
            LoadedFile.Categories.RemoveAt(index);

            // Removing the category shifts everyone up by one, so if its still in a valid range, use it, otherwise subtract one (ie: was last thing)
            // and ensure that is in range.
            if(index >= LoadedFile.Categories.Count)
                index--;

            if(index >= 0)
                CategoryViewModel.SelectedCategory = LoadedFile.Categories[index];
        }

        private void CreateNewEntry()
        {
            if (LoadedFile == null || CategoryViewModel.SelectedCategory == null)
                throw new InvalidOperationException("Cannot add entry to unloaded file or null category!");

            CategoryEntry entry = new CategoryEntry();
            CategoryViewModel.SelectedCategory.Entries.Add(entry);
            CategoryViewModel.SelectedEntryViewModel.SelectedEntry = entry;
        }

        private void RemoveEntry()
        {
            if (LoadedFile == null || CategoryViewModel.SelectedCategory == null)
                throw new InvalidOperationException("Cannot remove entry from unloaded file or null category!");

            // Get the index of the currently selected one(s)
            int index = CategoryViewModel.SelectedCategory.Entries.IndexOf(CategoryViewModel.SelectedEntryViewModel.SelectedEntry);
            CategoryViewModel.SelectedCategory.Entries.RemoveAt(index);

            // It shifted us up by one, so if still in valid range, use it, otherwise we subtract one (ie: it was the last one)
            if (index >= CategoryViewModel.SelectedCategory.Entries.Count)
                index--;

            if (index >= 0)
                CategoryViewModel.SelectedEntryViewModel.SelectedEntry = CategoryViewModel.SelectedCategory.Entries[index];
        }

        private void AddFilter()
        {
            CollViewSource.Filter -= Filter;
            CollViewSource.Filter += Filter;
        }

        private void Filter(object sender, FilterEventArgs e)
        {
            var src = e.Item as CategoryEntry;
            e.Accepted = false;

            if (src == null)
                return;

            if (src.DisplayName.Contains(SearchFilter))
                e.Accepted = true;
            else if (src.MapName.Contains(SearchFilter))
                e.Accepted = true;
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
