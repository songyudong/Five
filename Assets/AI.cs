using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public partial class Five
{
    Thread aiThread;
    Position aiPos;

    private void StartAIThread()
    {        
        aiThread = new Thread(RunAIThread);
        aiThread.Start();
    }
    
    private bool ai_start_flag = false;
    private bool ai_finish_flag = false;
    private bool ai_thread_end_flag = false;

    private void RunAIThread()
    {
        while(ai_thread_end_flag==false)
        {
            if (ai_start_flag)
            {
                aiPos = AI();
                ai_start_flag = false;
                ai_finish_flag = true;
            }
        }
        
    }

    public void AIReset()
    {
        search_count = 0;
        cut_count = 0;

        if (aiThread != null)
        {
            ai_thread_end_flag = true;
            while (aiThread.ThreadState != ThreadState.Stopped)
            {
                Thread.Sleep(100);
            }
        }

        ai_start_flag = false;
        ai_finish_flag = false;
        ai_thread_end_flag = false;
        StartAIThread();
    }

    private void OnApplicationQuit()
    {
        if (aiThread != null)
            ai_thread_end_flag = true;
    }
}