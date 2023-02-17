using System.Diagnostics;

namespace AutoColoriserNet48
{
    class SupressSeleniumLogs : ConsoleTraceListener
    {
        public override void Write(string message) {}
        public override void WriteLine(string message) {}
    }
}