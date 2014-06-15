using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MusicMinkAppLayer.Diagnostics
{
    /// <summary>
    /// Used with Logger and DebugHelper to extract the caller info so that the source function can be properly logged
    /// </summary>
    public class CallerInfo
    {
        private static char[] splitParts = new char[] { '\\' };

        public string FunctionName { get; private set; }
        public string FileName { get; private set; }
        public int LineNumber { get; private set; }

        public CallerInfo([CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            FunctionName = memberName;
            LineNumber = sourceLineNumber;
            FileName = sourceFilePath.Split(splitParts, StringSplitOptions.RemoveEmptyEntries).Last();
        }
    }
}
