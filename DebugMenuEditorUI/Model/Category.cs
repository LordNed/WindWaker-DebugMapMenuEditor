using System.ComponentModel;

namespace DebugMenuEditorUI.Model
{
    public class Category : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// What is the name of this category? Supports a maximum of <see cref="DisplayNameMaxLength"/> characters.
        /// </summary>
        public string DisplayName
        {
            get { return m_displayName; }
            set
            {
                m_displayName = value;
                OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Maximum length of a <see cref="DisplayName"/> entry as defined by the file structure.
        /// </summary>
        public int DisplayNameMaxLength { get { return 0x20; } }

        /// <summary>
        /// Which entries does this category contain?
        /// </summary>
        public BindingList<CategoryEntry> Entries
        {
            get { return m_entries; }
            set
            {
                m_entries = value;
                OnPropertyChanged("Entries");
            }
        }

        private string m_displayName;
        private BindingList<CategoryEntry> m_entries;

        public Category()
        {
            Entries = new BindingList<CategoryEntry>();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
