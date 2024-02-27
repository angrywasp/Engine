// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Engine.Tools
{
    public class ExternalTool
    {
        public static int Run(string command, string arguments, out string stdout, out string stderr, string stdin = null)
        {
            var fullPath = command;
            if (string.IsNullOrEmpty(fullPath))
                throw new Exception(string.Format("Couldn't locate external tool '{0}'.", command));

            var stdoutTemp = string.Empty;
            var stderrTemp = string.Empty;

            var processInfo = new ProcessStartInfo
            {
                Arguments = arguments,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                ErrorDialog = false,
                FileName = fullPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
            };

            using (var process = new Process())
            {
                process.StartInfo = processInfo;

                process.Start();

                var stdoutThread = new Thread(new ThreadStart(() =>
                {
                    var memory = new MemoryStream();
                    process.StandardOutput.BaseStream.CopyTo(memory);
                    var bytes = new byte[memory.Position];
                    memory.Seek(0, SeekOrigin.Begin);
                    memory.Read(bytes, 0, bytes.Length);
                    stdoutTemp = System.Text.Encoding.ASCII.GetString(bytes);
                }));

                var stderrThread = new Thread(new ThreadStart(() =>
                {
                    var memory = new MemoryStream();
                    process.StandardError.BaseStream.CopyTo(memory);
                    var bytes = new byte[memory.Position];
                    memory.Seek(0, SeekOrigin.Begin);
                    memory.Read(bytes, 0, bytes.Length);
                    stderrTemp = System.Text.Encoding.ASCII.GetString(bytes);
                }));

                stdoutThread.Start();
                stderrThread.Start();

                if (stdin != null)
                    process.StandardInput.Write(System.Text.Encoding.ASCII.GetBytes(stdin));

                process.StandardInput.Close();

                process.WaitForExit();

                stdoutThread.Join();
                stderrThread.Join();

                stdout = stdoutTemp;
                stderr = stderrTemp;

                return process.ExitCode;
            }
        }
    }
}
