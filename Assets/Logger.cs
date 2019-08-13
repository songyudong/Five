using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public partial class Five
{
    string logFolder;
    string logFilename;
    string path;
    FileInfo m_logFileInfo;

    public void InitLogger()
    {
        logFolder = Application.dataPath + "/..";
        path = logFolder + "/FiveLog.txt";
        
    }

    public void ClearLogger()
    {
        if (File.Exists(path))
            File.Delete(path);
        m_logFileInfo = new FileInfo(path);
    }

    public void Output(string log)
    {
        var sw = m_logFileInfo.AppendText();
        sw.WriteLine(log);
        sw.Close();
    }
}