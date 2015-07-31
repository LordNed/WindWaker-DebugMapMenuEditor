using DebugMenuEditorUI.Model;
using GameFormatReader.Common;
using System;
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

        public void Load(EndianBinaryReader stream)
        {
            if (stream == null || stream.BaseStream.Length == 0)
                throw new ArgumentException("Null or empty stream specified", "stream");

            // Read the header
            byte entryCount = stream.ReadByte();
            stream.Skip(3); // Unknown Constants (0x5E, 0, 0x61)

            // Offset to the first entry (realistically it's right after the header and always 0x8
            int offsetToFirstEntry = stream.ReadInt32();

            // Load categories and their entries.
            stream.BaseStream.Position = offsetToFirstEntry;
            for(int i = 0; i < entryCount; i++)
            {
                Category category = new Category();
                category.Load(stream);

                Categories.Add(category);
            }
        }

        public void Save(EndianBinaryWriter stream)
        {
            if (stream == null)
                throw new ArgumentException("Null stream to save to!", "stream");

            // Header
            stream.Write((byte)Categories.Count);
            stream.Write((byte)0x5E);
            stream.Write((byte)0x00);
            stream.Write((byte)0x61);

            // Offset to the first entry
            stream.Write((int)stream.BaseStream.Position + 0x4);

            // Pre-allocate the offset to sub-entries based on the size of the Category header and the # of Categories
            long entryOffsetStart = stream.BaseStream.Position + (Categories.Count * 0x28);

            // Write each category and its children.
            foreach(var category in Categories)
            {
                category.Save(stream, ref entryOffsetStart);
            }

            // Pad file to a 32-byte alignment boundry using the formula:
            // (0 + (n-1)) & ~(n-1)
            long nextAlignedPos = (stream.BaseStream.Length + 0x1F) & ~0x1F;
            long delta = nextAlignedPos - stream.BaseStream.Length;
            stream.Seek(0, System.IO.SeekOrigin.End);

            stream.Write(new byte[delta]);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
    