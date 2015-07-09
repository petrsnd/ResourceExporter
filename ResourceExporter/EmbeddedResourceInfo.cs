namespace ResourceExporter
{
    public class EmbeddedResourceInfo
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public long Size { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((EmbeddedResourceInfo) obj);
        }

        protected bool Equals(EmbeddedResourceInfo e)
        {
            return string.Equals(FileName, e.FileName) && string.Equals(FileType, e.FileType) && Size == e.Size;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (FileName != null ? FileName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (FileType != null ? FileType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Size.GetHashCode();
                return hashCode;
            }
        }
    }
}
