using DebugMenuEditorUI.Model;
using System.ComponentModel;

namespace DebugMenuEditorUI.ViewModel
{
    public class CategoryViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Category SelectedCategory
        {
            get { return m_selectedCategory; }
            set
            {
                m_selectedCategory = value;
                
                if(SelectedCategory != null && SelectedCategory.Entries.Count > 0)
                {
                    SelectedEntryViewModel.SelectedEntry = SelectedCategory.Entries[0];
                }
                else
                {
                    SelectedEntryViewModel.SelectedEntry = null;
                }

                OnPropertyChanged("SelectedCategory");
            }
        }

        public CategoryEntryViewModel SelectedEntryViewModel {get; private set;}
        private Category m_selectedCategory;

        public CategoryViewModel()
        {
            SelectedEntryViewModel = new CategoryEntryViewModel();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
