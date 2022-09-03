using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapleLib.Helpers
{
    public static class ErrorLogger
    {
        private static readonly List<Error> errorList = new List<Error>();

        public static void Log(ErrorLevel level, string message)
        {
            lock (errorList)
                errorList.Add(new Error(level, message));
        }

        /// <summary>
        /// Returns the numbers of errors currently in the pending queue
        /// </summary>
        /// <returns></returns>
        public static int NumberOfErrorsPresent()
        {
            return errorList.Count;
        }

        /// <summary>
        /// Errors present currently in the pending queue
        /// </summary>
        /// <returns></returns>
        public static bool ErrorsPresent()
        {
            return errorList.Count > 0;
        }

        /// <summary>
        /// Clears all errors currently in the pending queue
        /// </summary>
        public static void ClearErrors()
        {
            lock (errorList)
                errorList.Clear();
        }

        /// <summary>
        /// Logs all pending errors in the queue to file, and clears the queue
        /// </summary>
        /// <param name="filename"></param>
        public static void SaveToFile(string filename)
        {
            if (!ErrorsPresent())
                return;

            using (StreamWriter sw = new(File.Open(filename, FileMode.Append, FileAccess.Write, FileShare.Read)))
            {
                sw.Write("----- Start of the error log. [");
                sw.Write(DateTime.Today.ToString());
                sw.Write("] -----");
                sw.WriteLine();

                List<Error> errorList_;
                lock (errorList)
                {
                    errorList_ = new List<Error>(errorList); // make a copy before writing
                    ClearErrors();
                }

                foreach (Error e in errorList_)
                {
                    sw.Write("[");
                    sw.Write(e.level.ToString());
                    sw.Write("] : ");
                    sw.Write(e.message);

                    sw.WriteLine();
                }
                sw.WriteLine();
            }
        }
    }

    public class Error
    {
        internal ErrorLevel level;
        internal string message;

        internal Error(ErrorLevel level, string message)
        {
            this.level = level;
            this.message = message;
        }
    }

    public enum ErrorLevel
    {
        MissingFeature,
        IncorrectStructure,
        Critical,
        Crash
    }
}
