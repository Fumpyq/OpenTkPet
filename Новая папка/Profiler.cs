using ImGuiNET;
using OpenTK.Graphics.ES11;
using PostSharp.Aspects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApp1_Pet.Новая_папка
{
    public static class Profiler
    {
        public class Sample
        {
            public Sample parent;
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
            public HashSet<Sample> innerSamples = new HashSet<Sample>();
            public Dictionary<string, Sample> innerSamplesMap = new Dictionary<string, Sample>();
        }
        public class ProfilerThreadFrame
        {
            public List<Sample> samples = new List<Sample>(); 
            public Stack<Sample> sampQue = new Stack<Sample>();

            public void BeginSample(string name)
            {
                if (sampQue.TryPeek(out var ps))
                {
                    if (ps.innerSamplesMap.TryGetValue(name, out var sample))
                    {
                        sampQue.Push(sample);
                        sample.watch.Start();
                        sample.callCount++;
                    }
                    else
                    {
                        var smpl = new Sample() { name = name, callCount = 1, watch = Stopwatch.StartNew(), threadId = Thread.CurrentThread.ManagedThreadId };
                        ps.innerSamples.Add(smpl); smpl.parent = ps;ps.innerSamplesMap.Add(name, smpl);
                        sampQue.Push(smpl);
                    }
                }
                else
                {
                    var smpl = new Sample() { name = name, callCount = 1, watch = Stopwatch.StartNew(), threadId = Thread.CurrentThread.ManagedThreadId };
                    sampQue.Push(smpl);
                    samples.Add(smpl);
                }
                    
            }
            public void EndSample()
            {
                if (sampQue.Count > 0)
                {
                    var smpl = sampQue.Pop();
                    smpl.watch.Stop();
                }
                else
                {
                    Console.WriteLine("There is more EndSamples then Begin sample !");
                }

            }
        }
        public static object SampleLock = new object();
        public static ConcurrentDictionary<int, ProfilerThreadFrame> samples = new ConcurrentDictionary<int, ProfilerThreadFrame>();
       // public static ConcurrentBag<Sample> allSamples = new ConcurrentBag<Sample>();
        public static void BeginSample(string name)
        {
            lock (SampleLock){ 
                if(samples.TryGetValue(Thread.CurrentThread.ManagedThreadId, out var ts))
                {
                    ts.BeginSample(name);
                }
                else
                {
                    //var smpl = new Sample() { name = name, callCount = 1, watch = Stopwatch.StartNew(), threadId = Thread.CurrentThread.ManagedThreadId };
                    //allSamples.Add(smpl);
                    var d = new ProfilerThreadFrame();
                    d.BeginSample(name);
                    samples.TryAdd(Thread.CurrentThread.ManagedThreadId, d);
                }
            }
        }
        public static void EndSample(string name)
        {
            lock (SampleLock)
            {
                if (samples.TryGetValue(Thread.CurrentThread.ManagedThreadId, out var ts))
                {
                    ts.EndSample();
                }
            }
        }
        public static List<Dictionary<int, ConcurrentDictionary<string,Sample>>> sampleHistory= new List<Dictionary<int, ConcurrentDictionary<string, Sample>>>();
        public const int HistoryStackSize=4;
        public static void Draw()
        {
            if (samples.Count <= 0) return;
            ImGui.Begin("Profiler");

            // Positive Async!
            KeyValuePair<int, ProfilerThreadFrame>[] ThreadMap;
            lock (SampleLock)
            {
                ThreadMap = samples.ToArray();
                //KeyValuePair<int, ConcurrentDictionary<string, Sample>>[] saf = samples.ToArray();
                samples.Clear();
            }
            //samples.Clear();
            //string[] Labels = new string[ThreadMap.Length];
            //float[] values = new float[ThreadMap.Length];
            var size = new System.Numerics.Vector2(240, 12);

            var MainThreadID = Environment.CurrentManagedThreadId;
            //sampleHistory
            for (int i = 0; i < ThreadMap.Length;i++)//Foreach thread
            {
                var el = ThreadMap[i];
                if (ImGui.TreeNodeEx($"{el.Key}", ImGuiTreeNodeFlags.CollapsingHeader, $"{(el.Key == MainThreadID?"Main Thread":"Thread 1")}: {el.Value.samples.Sum(x=>x.watch.Elapsed.TotalMilliseconds).ToString("f2")} ms"))
                {
                    ImGui.TreePush("a");

                    var Max = el.Value.samples.Max(s => s.watch.Elapsed.TotalMilliseconds);
                    foreach (var snap in ThreadMap[i].Value.samples)//Foreach thread
                    {
                        //var snap = ..Value;

                        // Labels[i] =
                        if (snap.parent == null)
                            DrawProfilerRow(snap, Max);
                        //if (ImGui.TreeNodeEx($"{snap.name} {snap.watch.Elapsed.TotalMilliseconds.ToString("f2")} ms avg:{snap.AverageTime_Ms.ToString("f2")} x{snap.callCount}", ImGuiTreeNodeFlags.CollapsingHeader))
                        //{

                        //    ImGui.TreePop();
                        //}
                        //ImGui.ProgressBar((float)(snap.watch.Elapsed.TotalMilliseconds / Max),
                        //       size,
                        //       $"{snap.name} {snap.watch.Elapsed.TotalMilliseconds.ToString("f2")} ms avg:{snap.AverageTime_Ms.ToString("f2")} x{snap.callCount}"
                        //       );
                        //values[i] = 
                        ;
                    }
                    ImGui.TreePop();
                }
            }
           // ImGui.PlotHistogram("Profiler", ref values[0],values.Length);
            ImGui.End();
        }
        public static void DrawProfilerRow(Sample snap, double MaxFrameTime)
        {
            var size = new System.Numerics.Vector2(240, 12);
            if (snap.innerSamples.Count > 0)
            {
                if (ImGui.TreeNodeEx($"{snap.name}", ImGuiTreeNodeFlags.Bullet, $"{snap.name} {snap.watch.Elapsed.TotalMilliseconds.ToString("f2")} ms avg:{snap.AverageTime_Ms.ToString("f2")} x{snap.callCount}"))
                {
                    foreach (var s in snap.innerSamples)
                    {
                        DrawProfilerRow(s, MaxFrameTime);
                       
                    }
                    ImGui.TreePop();
                }
            }
            else
            {
                ImGui.ProgressBar((float)(snap.watch.Elapsed.TotalMilliseconds / MaxFrameTime),
                  size,
                  $"{snap.name} {snap.watch.Elapsed.TotalMilliseconds.ToString("f2")} ms avg:{snap.AverageTime_Ms.ToString("f2")} x{snap.callCount}"
                  );
                
            }

        }
    }

    [Serializable]
    public class TimingAspect : OnMethodBoundaryAspect
    {
        

        public override void OnEntry(MethodExecutionArgs args)
        {
            Profiler.BeginSample(args.Method.Name);
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            Profiler.EndSample(args.Method.Name);

        }
    }
}
