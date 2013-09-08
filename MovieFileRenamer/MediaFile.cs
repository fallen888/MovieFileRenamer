using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileRenamer
{
    public class MediaFile
    {
        public string FolderPath { get; set; }
        public string OriginalName { get; set; }
        public string NewName { get; set; }
    }
}
