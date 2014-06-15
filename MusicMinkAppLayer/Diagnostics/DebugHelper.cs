using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MusicMinkAppLayer.Diagnostics
{
    /// <summary>
    /// Logs assertion failures and provides helpful assertion patterns like CastAndAssert and Alert
    /// </summary>
    public static class DebugHelper
    {
        public static void Alert(CallerInfo callerInfo, string message)
        {
            Assert(callerInfo, false, message);
        }

        public static void Alert(CallerInfo callerInfo, string message, params Object[] args)
        {
            Assert(callerInfo, false, string.Format(message, args));
        }

        public static bool Assert(CallerInfo callerInfo, bool assertion)
        {
            return Assert(callerInfo, assertion, "Assertion Failed");
        }

        public static bool Assert(CallerInfo callerInfo, bool assertion, string message, params Object[] args)
        {
            if (!assertion)
            {
                Debugger.Break();
                Logger.Current.Log(callerInfo, LogLevel.Error, string.Format(message, args));
            }

            Debug.Assert(assertion, message);

            return assertion;
        }

        public static T CastAndAssert<T>(object obj, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            T objAsAT = default(T);

            try
            {
                objAsAT = (T)obj;
            }
            catch (Exception ex)            
            {
                if (ex is NullReferenceException || ex is InvalidCastException)
                {
                    Assert(new CallerInfo(memberName, sourceFilePath, sourceLineNumber), false, "CastAndAssert encoutered a invalid cast");
                }

                else throw;
            }

            return objAsAT;
        }
    }
}
