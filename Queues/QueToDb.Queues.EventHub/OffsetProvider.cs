using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace QueToDb.Queues.EventHub
{
    public class OffsetProvider
    {
        private static string _storageFilePath;
        private static long _offset;

        public static long Offset
        {
            get { return GetOffset(); }
            set { StoreOffset(value); }
        }
        public static string StorageFilePath
        {
            get
            {
                if (String.IsNullOrEmpty(_storageFilePath))
                    _storageFilePath = @"OffsetStorage.txt";
                return _storageFilePath;
            }
            set { _storageFilePath = value; }
        }

        [STAThread]
        private static long GetOffset()
        {
            try
            {
                var sr = new StreamReader(StorageFilePath, Encoding.ASCII);
                _offset = Convert.ToInt64(sr.ReadLine());
                sr.Close();
            }
            catch (FileNotFoundException ex)
            { // it might be the file is not created yet, so it is not an error
                _offset = 0;
            }
            catch (DirectoryNotFoundException ex)
            { // it might be the file is not created yet, so it is not an error
                 _offset = 0;
            }
             return _offset;

        }

        [STAThread]
        private static void StoreOffset(long value)
        {
            var sw = new StreamWriter(StorageFilePath, false, Encoding.ASCII);
            sw.WriteLine(value.ToString());
            sw.Close();
        }
    }
}