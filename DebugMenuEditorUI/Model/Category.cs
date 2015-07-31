using GameFormatReader.Common;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

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

        public void Load(EndianBinaryReader stream)
        {
            // Store the position of the stream so we can reset back to it after we jump to read children.
            long streamPos = stream.BaseStream.Position;

            DisplayName = Encoding.GetEncoding("shift-jis").GetString(stream.ReadBytesUntil(0));
            stream.BaseStream.Position = streamPos + 0x20;

            short entryCount = stream.ReadInt16();
            Trace.Assert(stream.ReadInt16() == 0x00); // Padding

            int entryOffset = stream.ReadInt32();
            stream.BaseStream.Position = entryOffset;

            for (int i = 0; i < entryCount; i++)
            {
                CategoryEntry entry = new CategoryEntry();
                entry.Load(stream);

                Entries.Add(entry);
            }

            // Reset the stream to the start of the struct (+ size of struct) since reading the sub-options jumps us
            // around in the stream.
            stream.BaseStream.Position = streamPos + 0x28;
        }

        public void Save(EndianBinaryWriter stream, ref long entryOffsetStart)
        {
            // Write the first 0x20 characters of the DisplayName. DisplayName should be limited to 0x20 but we're going to
            // ensure that it is by not writing any more than that!
            long streamPos = stream.BaseStream.Position;
            byte[] encodedName = Encoding.GetEncoding("shift-jis").GetBytes(DisplayName);
            for (int i = 0; i < 0x20; i++)
            {
                if (i < encodedName.Length)
                    stream.Write((byte)encodedName[i]);
                else
                    stream.Write((byte)0);
            }

            // Write the entry count and padding
            stream.Write((short)Entries.Count);
            stream.Write((short)0);
            stream.Write((int)entryOffsetStart);

            // Write the entries
            stream.BaseStream.Position = entryOffsetStart;
            foreach (var entry in Entries)
            {
                entry.Save(stream);

                // Advance the sub-entry offset by the size of an entry so that when the next category
                // is written to disk, it writes them after our current ones.
                entryOffsetStart += 0x2C;
            }

            // Finally, restore our stream back to the end of the Category header so that the stream
            // writes the next category right after this one.
            stream.BaseStream.Position = streamPos + 0x28;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
