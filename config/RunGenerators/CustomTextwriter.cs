using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunGenerators
{
    public class CustomTextwriter : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;

        private TextWriter ConsoleWriter;
        private TextWriter LogWriter;
        public CustomTextwriter(TextWriter logWriter,TextWriter consoleWriter)
        {
            
            ConsoleWriter = consoleWriter;
            LogWriter = logWriter;
        }
        public override void WriteLine(string? value)
        {
            ConsoleWriter.WriteLine(value);
            LogWriter.WriteLine(value);
        }
        public override void Close()
        {
            ConsoleWriter.Close();
            LogWriter.Close();
        }
    }
}
