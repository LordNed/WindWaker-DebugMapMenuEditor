using DebugMenuEditorUI.Model;
using System.ComponentModel;

namespace DebugMenuEditorUI.Model
{
    public class Menu : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// List of categories which are contained in this Debug Map Menu file.
        /// </summary>
        public BindingList<Category> Categories
        {
            get { return m_categories; }
            set
            {
                m_categories = value;
                OnPropertyChanged("Categories");
            }
        }

        public string FileName;
        public string FolderPath;

        private BindingList<Category> m_categories;

        public Menu()
        {
            Categories = new BindingList<Category>();
            FileName = "Untitled.dat";
            FolderPath = string.Empty;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
    