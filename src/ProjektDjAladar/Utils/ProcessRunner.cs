using System;
using System.Diagnostics;
using System.Text;

namespace ProjektDjAladar
{
    public static class ProcessRunner
    {
        private static StringBuilder _output;

        public static string RunProcess(string processName, string arguments = "")
        {
            _output = new StringBuilder();
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = processName,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            process.OutputDataReceived += ProcessOutputHandler;
            process.ErrorDataReceived += ProcessOutputHandler;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            return _output.ToString();
        }

        private static void ProcessOutputHandler(object sendingProcess, DataReceivedEventArgs eventArgs)
        {
            if (!string.IsNullOrEmpty(eventArgs.Data))
            {
                if (_output.Length != 0)
                    _output.Append(Environment.NewLine);
                _output.Append(eventArgs.Data);
            }
        }
    }
}