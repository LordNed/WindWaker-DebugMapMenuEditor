using DebugMenuEditorUI.Model;
using System.ComponentModel;

namespace DebugMenuEditorUI.ViewModel
{
    public class CategoryEntryViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public CategoryEntry SelectedEntry
        {
            get { return m_selectedEntry; }
            set
            {
                m_selectedEntry = value;
                OnPropertyChanged("SelectedEntry");
            }
        }

        private CategoryEntry m_selectedEntry;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
