using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Five
{
    enum ProfilerFunction
    {
        GET_BLANK_LIST = 0,
        EVALUATE = 1,
        CAL_SCORE = 2,
        COUNT = 3,
    }

    class Profiler
    {
        public List<float> profiler = new List<float>();
        public List<float> enter = new List<float>();
        public List<int> count = new List<int>();
        public void Init()
        {
            for (int i = 0; i < (int)ProfilerFunction.COUNT; i++)
            {
                profiler.Add(0);
                enter.Add(0);
                count.Add(0);
            }
        }
        public void Reset()
        {
            for (int i = 0; i < (int)ProfilerFunction.COUNT; i++)
            {
                profiler[i] = 0;
                enter[i] = 0;
                count[i] = 0;
            }

        }

        public void Enter(ProfilerFunction func)
        {
            enter[(int)func] = System.DateTime.Now.Millisecond/1000.0f;
        }

        public void Leave(ProfilerFunction func)
        {
            count[(int)func]++;
            profiler[(int)func] += System.DateTime.Now.Millisecond/1000.0f - enter[(int)func];
        }
    }

    Profiler profiler = new Profiler();
}