using System.ComponentModel;

namespace DebugMenuEditorUI.Model
{
    public class CategoryEntry : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// What is the display name of this entry? Supports a maximum of <see cref="DisplayNameMaxLength"/> characters.
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
        /// What is the name of the map that this entry will load? Supports a maximum of <see cref="MapNameMaxLength"/> characters.
        /// </summary>
        public string MapName
        {
            get { return m_mapName; }
            set
            {
                m_mapName = value;
                OnPropertyChanged("MapName");
            }
        }

        /// <summary>
        /// Which room of the map should we load?
        /// </summary>
        public byte RoomIndex
        {
            get { return m_roomIndex; }
            set
            {
                m_roomIndex = value;
                OnPropertyChanged("RoomNumber");
            }
        }

        /// <summary>
        /// And of that room, which spawn index should we use?
        /// </summary>
        public byte SpawnIndex
        {
            get { return m_spawnIndex; }
            set
            {
                m_spawnIndex = value;
                OnPropertyChanged("SpawnIndex");
            }
        }

        /// <summary>
        /// And of that room, which layer of entities should be loaded?
        /// </summary>
        public Layer LayerIndex
        {
            get { return m_layerIndex; }
            set
            {
                m_layerIndex = value;
                OnPropertyChanged("LayerIndex");
            }
        }

        /// <summary>
        /// Maximum length of a <see cref="DisplayName"/> entry as defined by the file structure.
        /// </summary>
        public int DisplayNameMaxLength { get { return 0x21; } }

        /// <summary>
        /// Maximum length of a <see cref="MapName"/> entry as defined by the file structure.
        /// </summary>
        public int MapNameMaxLength { get { return 0x8; } }

        private string m_displayName;
        private string m_mapName;
        private byte m_roomIndex;
        private byte m_spawnIndex;
        private Layer m_layerIndex = Layer.Default;

        public CategoryEntry() { }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
