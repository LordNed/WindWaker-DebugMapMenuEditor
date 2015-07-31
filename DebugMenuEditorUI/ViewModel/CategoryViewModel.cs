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
                OnPropertyChanged("SelectedCategory");
            }
        }

        private Category m_selectedCategory;


        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
