using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Новая_папка
{
    public static class Profiler
    {
        public struct Sample
        {
            public string name;
            public Stopwatch watch;
            public int callCount;
            public double AverageTime_Ms
            {
                get
                {
                    return callCount > 0 ? watch.Elapsed.TotalMilliseconds / callCount : 0;
                }
            }
        }
        public static Dictionary<string, Sample> samples = new Dictionary<string, Sample>(0);

        public static void StartSample(string name)
        {
            if(samples.TryGetValue(name, out Sample sample))
            {
                sample.watch.Start();
                sample.callCount++;
            }
            else
            {
                samples.Add(name, new Sample() { name=name,callCount=1,watch=Stopwatch.StartNew()});
            }
        }
        public static void StopSample(string name)
        {
            if (samples.TryGetValue(name, out Sample sample))
            {
                sample.watch.Stop();
               // sample.callCount++;
            }

        }
        public static void Draw()
        {
            if (samples.Count <= 0) return;
            ImGui.Begin("Profiler");
            ImGui.End();
        }
    }
}
