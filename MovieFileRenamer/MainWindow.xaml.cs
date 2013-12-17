using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace FileRenamer
{
    public partial class MainWindow : Window
    {
        public List<MediaFile> MediaFileList { get; set; }
        public List<string> AllowedMetaData { get; set; }
        public TextInfo _textInfo = new CultureInfo("en-US", false).TextInfo;

        public MainWindow()
        {
            InitializeComponent();

            this.MediaFileList = new List<MediaFile>();

            this.AllowedMetaData = new List<string>
            {
                "720p",
                "720",
                "1080p",
                "1080",
                "hdtv",
                "x264",
                "h264",
                "ac3",
                "dts",
                "aac",
                "brrip",
                "bdrip",
                "bluray",
                "hdrip",
                "dvdrip",
                "webrip",
                "xvid",
                "extended",
                "5.1",
                "7.1"
            };
        }


        private void grid_Drop(object sender, DragEventArgs e)
        {
            string[] files = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            var duplicates = new List<string>();

            files.ToList().ForEach((file) =>
            {
                var filename = file.Substring(file.LastIndexOf('\\') + 1);

                if (this.MediaFileList.Count(f => f.OriginalName == filename) > 0)
                {
                    duplicates.Add(filename);
                }
                else
                {
                    this.MediaFileList.Add(new MediaFile
                    {
                        FolderPath = file.Substring(0, file.LastIndexOf('\\')),
                        OriginalName = filename,
                        NewName = FormatName(filename)
                    });
                }

            });

            this.PopulateGrid();

            if (duplicates.Count > 0)
            {
                string duplicatesString = String.Join(Environment.NewLine + " - ", duplicates.ToArray());
                MessageBox.Show(Application.Current.MainWindow, "The following duplicates were not added: \n\r - " + duplicatesString, "Duplicates Found", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);
            }
        }


        protected string FormatName(string originalName)
        {
            string filename = System.IO.Path.GetFileNameWithoutExtension(originalName);
            string extension = System.IO.Path.GetExtension(originalName);

            filename = filename.Replace('.', ' ');

            var parts = filename.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            filename = parts.Aggregate((s1, s2) =>
            {
                var year = Regex.Replace(s2, "[^.0-9]", String.Empty);

                if (year.Length == 4 && (year.StartsWith("19") || year.StartsWith("20")))
                {
                    return String.Format("{0} ({1})", s1, year);
                }
                else
                {
                    return String.Format("{0} {1}", s1, s2);
                }
            });


            parts = filename.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            filename = String.Empty;

            var metadata = "[";

            parts.ToList().ForEach((part) => 
            {
                part = part.Trim("[]".ToCharArray());

                if (this.AllowedMetaData.Contains(part.ToLower()))
                {
                    metadata += part + " ";
                }
                else
                {
                    filename += part + " ";
                }
            });

            metadata = metadata.Trim() + "]";

            var newFilename = _textInfo.ToTitleCase(filename.Substring(0, filename.LastIndexOf(')') + 1)) + (metadata == "[]" ? String.Empty : " " + metadata);
            filename = String.IsNullOrEmpty(newFilename) ? filename : newFilename;

            return filename.Trim() + extension;
        }


        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.MediaFileList.Count == 0)
                {
                    MessageBox.Show(Application.Current.MainWindow, "Nothing to do.", "Nothing To Do", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.None);
                    return;
                }

                this.MediaFileList.ForEach((file) =>
                {
                    var oldName = file.FolderPath + "\\" + file.OriginalName;
                    var newName = file.FolderPath + "\\" + file.NewName;

                    if (oldName != newName)
                    {
                        File.Move(oldName, newName);
                    }
                });

                MessageBox.Show(Application.Current.MainWindow, "Done!", "Operation Complete", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.None);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Application.Current.MainWindow, ex.Message, "ERROR!", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.None);
            }
        }


        private void ClearList_Click(object sender, RoutedEventArgs e)
        {
            this.MediaFileList.Clear();
            this.PopulateGrid();
        }


        protected void PopulateGrid()
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(new DataTable());
            ds.Tables[0].Columns.Add("Original Name");
            ds.Tables[0].Columns.Add("New Name");

            this.MediaFileList.ToList().ForEach(file => ds.Tables[0].Rows.Add(file.OriginalName, file.NewName));
            this.DataContext = ds.Tables[0].DefaultView;

            this.grid.Columns[0].Width = 424;
            this.grid.Columns[1].Width = 424;
        }


        private void Grid_PreviewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == DataGrid.DeleteCommand)
            {
                var mediaFile = this.MediaFileList.SingleOrDefault(file => file.OriginalName == ((sender as DataGrid).SelectedValue as DataRowView).Row[0].ToString());
                if (mediaFile != null)
                {
                    this.MediaFileList.Remove(mediaFile);
                    this.PopulateGrid();
                }
            }
        }


        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.OriginalString));
            e.Handled = true;
        }
    }


}
