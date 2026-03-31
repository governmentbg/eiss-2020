using System;

namespace IOWebApplication.Infrastructure.Models.Integrations.EpepFastProcess
{
    public class FileListVM
    {
        public string FileTitle { get; set; }
        public string FileName { get; set; }
        public string FileTypeName { get; set; }
        public string Hash { get; set; }
        public Guid Gid { get; set; }
        public Guid BlobKey { get; set; }
        public string SignersInfo { get; set; }
    }

    public class NomenclatureItemVM
    {
        public string Alias { get; set; }
        public string Value { get; set; }
        public string Label { get; set; }
    }
}
