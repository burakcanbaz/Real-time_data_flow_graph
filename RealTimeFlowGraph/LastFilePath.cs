using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace LastFilePath
{
    public class LastCreatedFilePath
    {
        private string _newPath;
        public string NewPath
        {
            get
            {
                return _newPath;
            }
            set
            {
                _newPath = value;
            }
        }
        public string FullTxtName { get; set; }
        public string TxtName { get; set; }
        public string LastFileCreated { get; set; }
        public string NPath { get; set; }
        public LastCreatedFilePath()
        {
            NPath = @"C:\Users\Burak\Desktop\CsvFileCreator\database\";
            LastFileCreated = GetLastFile(NPath);
            LastFileCreated = LastFileCreated.Split(' ')[0];
            TxtName = ReadTxtFile();
            FullTxtName = LastFileCreated + "\\" + TxtName;
            string FullTxtNameCsv = LastFileCreated + "\\" + TxtName + ".csv";
            _newPath = @"C:\Users\Burak\Desktop\CsvFileCreator\database\" + FullTxtNameCsv;
        }

        private string ReadTxtFile()
        {
            string Fullpath = @"C:/Users/Burak/Desktop/CsvFileCreator/temp/tempadate.txt";
            using (var fs = new FileStream(Fullpath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs, Encoding.UTF8))
            {
                string txt = sr.ReadLine();
                return txt;
            }

        }

        private string GetLastFile(string npath)
        {

            DateTime LastFile = new DateTime(1900, 1, 1);
            string highDir;
            foreach (string subdir in Directory.GetDirectories(npath))
            {
                DateTime created = Directory.GetLastWriteTime(subdir);

                if (created > LastFile)
                {
                    highDir = subdir;
                    LastFile = created;
                }
            }
            return LastFile.ToString("yy-MM-dd", CultureInfo.InvariantCulture);
        }
    }
}