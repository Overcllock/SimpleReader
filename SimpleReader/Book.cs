using System;
using System.IO;
using System.Runtime.Serialization;

namespace SimpleReader {
    [DataContract]
    public class Book {
        //Название книги
        [DataMember]
        private string Name;
        //Автор
        [DataMember]
        private string Author;
        //Размер файла
        [DataMember]
        private long Size;
        //Путь к файлу
        [DataMember]
        private string Source;
        //Путь к обложке
        [DataMember]
        private string CoverSource;
        //Избранная книга
        [DataMember]
        public bool IsFavorite;
        //Тип ресурса
        public enum ResourceType {
            Local = 1,
            Internet
        }
        [DataMember]
        private ResourceType Type;

        public Book(string source, ResourceType type) {
            CoverSource = string.Empty;
            Source = source;
            Type = type;
            Name = GetNameByPath(source);
            if (type == ResourceType.Local) {
                FileInfo info = new FileInfo(source);
                Size = info.Length;
            }
            else Size = 0;
        }

        public void SetName(string name) { Name = name; }
        public void SetAuthor(string author) { Author = author; }
        public void SetSource(string source) { Source = source; }
        public void SetCoverSource(string source) { CoverSource = source; }

        //Получает имя файла из полного пути
        public static string GetNameByPath(string source) {
            string Name = source;
            char[] symbols = new char[] { '/', '\\' };
            int idx = Name.LastIndexOfAny(symbols);
            if (idx > -1)
                Name = Name.Substring(idx + 1);
            string extension = Path.GetExtension(Name);
            if (!extension.Equals(string.Empty))
                Name = Name.Remove(Name.IndexOf(extension));
            return Name;
        }

        public string GetName() { return Name; }
        public string GetAuthor() { return Author; }
        public string GetSize() {
            try {
                double sizeinbytes = Size;
                double sizeinkbytes = Math.Round((sizeinbytes / 1024));
                double sizeinmbytes = Math.Round((sizeinkbytes / 1024));
                double sizeingbytes = Math.Round((sizeinmbytes / 1024));
                if (sizeingbytes > 1)
                    return string.Format("{0} GB", sizeingbytes);
                else if (sizeinmbytes > 1)
                    return string.Format("{0} MБ", sizeinmbytes);
                else if (sizeinkbytes > 1)
                    return string.Format("{0} KБ", sizeinkbytes);
                else
                    return string.Format("{0} байт", sizeinbytes);
            }
            catch { return "Ошибка получения размера файла"; }
        }
        public ResourceType GetSourceType() { return Type; }
        public string GetSource() { return Source; }
        public string GetCoverSource() { return CoverSource; }

        public override string ToString() {
            if (Type == ResourceType.Local)
                return string.Format("{0}\nАвтор: {1}\nРазмер файла: {2}\n" +
                    "Тип файла: локальный файл", Name, Author, GetSize());
            return string.Format("{0}\nАвтор: {1}\nТип файла: интернет-ресурс", Name, Author);
        }
    }
}
