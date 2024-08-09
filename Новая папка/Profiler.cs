using ImGuiNET;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Новая_папка
{
    public static class Profiler
    {
        public class Sample
        {
            public int threadId;
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
        public static ConcurrentDictionary<int, ConcurrentDictionary<string, Sample>> samples = new ConcurrentDictionary<int, ConcurrentDictionary<string, Sample>>();
        public static ConcurrentBag<Sample> allSamples = new ConcurrentBag<Sample>();
        public static void BeginSample(string name)
        {
            if(samples.TryGetValue(Thread.CurrentThread.ManagedThreadId, out var ts))
            {
                if (ts.TryGetValue(name, out var sample))
                {
                    sample.watch.Start();
                    sample.callCount++;
                }
                else
                {
                    var smpl = new Sample() { name = name, callCount = 1, watch = Stopwatch.StartNew(), threadId = Thread.CurrentThread.ManagedThreadId };
                    allSamples.Add(smpl);
                    ts.TryAdd(name,smpl);
                }
            }
            else
            {
                var smpl = new Sample() { name = name, callCount = 1, watch = Stopwatch.StartNew(), threadId = Thread.CurrentThread.ManagedThreadId };
                allSamples.Add(smpl);
                var d = new ConcurrentDictionary<string, Sample>();
                d.TryAdd(name, smpl);
                samples.TryAdd(Thread.CurrentThread.ManagedThreadId, d);
            }
        }
        public static void EndSample(string name)
        {
            if (samples.TryGetValue(Thread.CurrentThread.ManagedThreadId, out var ts))
            {
                if (ts.TryGetValue(name, out Sample sample))
                {
                    sample.watch.Stop();
                    // sample.callCount++;
                }
            }
        }
        public static void Draw()
        {
            if (samples.Count <= 0) return;
            ImGui.Begin("Profiler");

            // Positive Async!
            Sample[] snapShot= allSamples.ToArray();
            allSamples.Clear();
            samples.Clear();
            //string[] Labels = new string[snapShot.Length];
            //float[] values = new float[snapShot.Length];
            var size = new System.Numerics.Vector2(240, 12);
            var Max = snapShot.Max(s => s.watch.Elapsed.TotalMilliseconds);
            for (int i = 0; i < snapShot.Length;i++)
            { 
                var snap = snapShot[i];
               // Labels[i] =
                   ImGui.ProgressBar((float)(snap.watch.Elapsed.TotalMilliseconds/Max),
                       size,
                       $"{snap.name} {snap.watch.Elapsed.TotalMilliseconds.ToString("f2")} ms avg:{snap.AverageTime_Ms.ToString("f2")} x{snap.callCount}"
                       );
                //values[i] = 
                    ;
            }
           // ImGui.PlotHistogram("Profiler", ref values[0],values.Length);
            ImGui.End();
        }
    }
}
