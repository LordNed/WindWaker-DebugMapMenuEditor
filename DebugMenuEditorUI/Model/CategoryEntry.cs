using GameFormatReader.Common;
using System.ComponentModel;
using System.Text;

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
        private Layer m_layerIndex;

        public CategoryEntry()
        {
            LayerIndex = Layer.Default;
        }

        public void Load(EndianBinaryReader stream)
        {
            long streamPos = stream.BaseStream.Position;

            DisplayName = Encoding.GetEncoding("shift-jis").GetString(stream.ReadBytesUntil(0));
            stream.BaseStream.Position = streamPos + 0x21;

            MapName = Encoding.GetEncoding("shift-jis").GetString(stream.ReadBytesUntil(0));
            stream.BaseStream.Position = streamPos + 0x29;

            RoomIndex = stream.ReadByte();
            SpawnIndex = stream.ReadByte();
            LayerIndex = (Layer)stream.ReadByte();
        }

        public void Save(EndianBinaryWriter stream)
        {
            // Write 0x20 bytes of the name in shift-jis encoded and then force null terminator.
            byte[] encodedDisplayName = Encoding.GetEncoding("shift-jis").GetBytes(DisplayName);
            for(int i = 0; i < 0x20; i++)
            {
                if (i < encodedDisplayName.Length)
                    stream.Write((byte)encodedDisplayName[i]);
                else
                    stream.Write((byte)0);
            }
            stream.Write((byte)0); // Null terminator

            byte[] encodedMapName = Encoding.GetEncoding("shift-jis").GetBytes(MapName);
            for (int i = 0; i < 0x8; i++)
            {
                if (i < encodedMapName.Length)
                    stream.Write((byte)encodedMapName[i]);
                else
                    stream.Write((byte)0);
            }

            stream.Write(RoomIndex);
            stream.Write(SpawnIndex);
            stream.Write((byte)LayerIndex);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        
    }
}
