using System.Diagnostics;

namespace LaptopAsTvBoxApp
{
    class Shell
    {
        public static int ShellExecuteCommand(string cmd, bool closeProcess)
        {
            int res = 0;

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardInput = true;
            startInfo.UseShellExecute = false;
            process.StartInfo = startInfo;
            process.Start();

            using (System.IO.StreamWriter sw = process.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    sw.WriteLine(cmd);
                }
            }

            if (closeProcess == true)
                process.Close();

            return res;
        }
    }
}
