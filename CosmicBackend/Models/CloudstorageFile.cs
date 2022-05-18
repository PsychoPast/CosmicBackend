using System;

namespace CosmicBackend.Models
{
    internal class CloudstorageFile
    {
        internal string contentType { get; set; }

        internal bool doNotCache { get; set; }

        internal string filename { get; set; }

        internal string hash { get; set; }

        internal string hash256 { get; set; }

        internal int length { get; set; }

        internal StorageIds storageIds { get; set; }

        internal string storageType { get; set; }

        internal string uniqueFilename { get; set; }

        internal DateTime uploaded { get; set; }

        internal class StorageIds
        {
            internal string DSS { get; set; }
        }
    }
}