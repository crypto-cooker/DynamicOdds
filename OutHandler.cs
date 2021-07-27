using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Xml.Serialization;

namespace DOFeed
{
    internal class OutHandler
    {
        private string datafile;

        public OutHandler(string v)
        {
            this.datafile = v;
        }

        internal void Write(string xmlData)
        {
            if (xmlData != null)
            {
                WriteOut(xmlData);
            }
            else
            {
                WriteFail("No data received");
            }
        }

        private void WriteFail(string error)
        {
            TextWriter writer = null;
            bool isValidFile = ValidateFileName(datafile);
            if (!isValidFile)
            {
                return;
            }
            try
            {
                XmlSerializer formatter = new XmlSerializer(typeof(string));
                writer = new StreamWriter(datafile, false);
                formatter.Serialize(writer, error); ;
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
            }
        }

        private void WriteOut(string xmlData)
        {
            bool isValidFile = ValidateFileName(datafile);
            if (!isValidFile)
            {
                return;
            }
            try
            {
                FileIOPermission f = new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.Write, datafile);
                try
                {
                    f.Demand();
                }
                catch (SecurityException)
                {
                    return;
                }
                System.IO.File.WriteAllText(datafile, xmlData);

            }
            catch
            {
                WriteFail("Exception occurred - no data");
            }
        }

        private bool ValidateFileName(string outfile)
        {
            System.IO.FileInfo fi = null;
            try
            {
                fi = new System.IO.FileInfo(outfile);
            }
            catch (ArgumentException) { }
            catch (System.IO.PathTooLongException) { }
            catch (NotSupportedException) { }
            if (ReferenceEquals(fi, null))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}