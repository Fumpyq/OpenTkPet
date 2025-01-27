using ImGuiNET;
using OpenTK.Graphics.ES11;
using PostSharp.Aspects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static BepuPhysics.Collidables.CompoundBuilder;
using static ConsoleApp1_Pet.Новая_папка.Profiler;

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
            public long startMemoryUsage;
            public long endMemoryUsage;
            public int callCount;
            public long MemoryBefore;
            public long MemoryAfter;
            public double AverageTime_Ms
            {
                get
                {
                    return callCount > 0 ? watch.Elapsed.TotalMilliseconds / callCount : 0;
                }
            }
            public long AllocationInBytes
            {
                get
                {
                    return endMemoryUsage - startMemoryUsage;
                }
            }
            public HashSet<Sample> innerSamples = new HashSet<Sample>();
        }
        public class ProfilerThreadFrame
        {
            public Dictionary<string, Sample> samples = new Dictionary<string, Sample>();
            public Queue<Sample> sampQue = new Queue<Sample>();
            public int thread;

            public ProfilerThreadFrame(int thread)
            {
                this.thread = thread;
            }
            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            public void BeginSample(string name)
            {
                if (samples.TryGetValue(name, out var sample))
                {
                    sampQue.Enqueue(sample);
                    sample.watch.Start();
                    sample.startMemoryUsage = GC.GetTotalMemory(false);
                    sample.callCount++;
                }
                else
                {


                    var smpl = new Sample() { name = name, callCount = 1, watch = Stopwatch.StartNew(), threadId = Thread.CurrentThread.ManagedThreadId };
                    if (sampQue.TryPeek(out var ps))
                    {
                        ps.innerSamples.Add(smpl); smpl.parent = ps;
                    }
                    //else
                    samples.TryAdd(name, smpl);
                    sampQue.Enqueue(smpl);
                    smpl.startMemoryUsage = GC.GetTotalMemory(false);
                    //allSamples.Add(smpl);



                }
            }
            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            public void EndSample()
            {
                if (sampQue.Count > 0)
                {
                    var smpl = sampQue.Dequeue();
                    smpl.watch.Stop();
                    smpl.endMemoryUsage = GC.GetTotalMemory(false);
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
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
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
                    var d = new ProfilerThreadFrame(Thread.CurrentThread.ManagedThreadId);
                    d.BeginSample(name);
                    samples.TryAdd(Thread.CurrentThread.ManagedThreadId, d);
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
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
        public static List<List<ProfilerThreadFrame>> sampleHistory= new List<List<ProfilerThreadFrame>>();
        public static int HistoryStackSize=120;
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
            sampleHistory.Add(ThreadMap.Select(x=>x.Value).ToList());
            //samples.Clear();
            //string[] Labels = new string[ThreadMap.Length];
            //float[] values = new float[ThreadMap.Length];
            var size = new System.Numerics.Vector2(240, 12);

            var MainThreadID = Environment.CurrentManagedThreadId;
            
            //sampleHistory
            for (int i = 0; i < ThreadMap.Length;i++)//Foreach thread
            {
                var el = ThreadMap[i];
               
                if (ImGui.TreeNodeEx($"{el.Key}", ImGuiTreeNodeFlags.CollapsingHeader, $"{(el.Key == MainThreadID?"Main Thread":"Thread 1")}: {el.Value.samples.Where(x=>x.Value.parent==null).Sum(x=>x.Value.watch.Elapsed.TotalMilliseconds).ToString("f2")} ms"))
                {


                    var Max = el.Value.samples.Max(s => s.Value.watch.Elapsed.TotalMilliseconds);
                    foreach (var snap in ThreadMap[i].Value.samples.Values)//Foreach thread
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
            while(sampleHistory.Count > HistoryStackSize)
            {
                sampleHistory.RemoveAt(0);
            }
            DrawHistoryPlot(CollectionsMarshal.AsSpan(sampleHistory));
            ImGui.End();
        }
        class SamplesSumamry
        {
            public int Count;
            public TimeSpan TotalTime;
            public long totalMemory;
            public List<Sample> Samples = new List<Sample>(4);
            public SamplesSumamry Clear()
            {
                Count = 0;
                TotalTime = TimeSpan.Zero;
                totalMemory = 0;
                Samples.Clear();
                return this;
            }
        }
        private static Queue<SamplesSumamry> SamplesSumamryPool = new Queue<SamplesSumamry>();
        private static SamplesSumamry GetSS()
        {
            if(SamplesSumamryPool.TryDequeue(out SamplesSumamry s))return s.Clear();
            else return new SamplesSumamry();
        }
        public static void DrawHistoryPlot(Span<List<ProfilerThreadFrame>> hist)
        {
            Dictionary<string, SamplesSumamry> map = new Dictionary<string, SamplesSumamry>();
            Dictionary<string, HashSet<SamplesSumamry>> ParentMap = new Dictionary<string, HashSet<SamplesSumamry>>();
            SamplesSumamry MainThread = GetSS();
            var MainThreadId = Thread.CurrentThread.ManagedThreadId;
            foreach (var ThreadFrames in hist)
            {
                foreach (var ThreadFrame in ThreadFrames)
                {
                    foreach (var s in ThreadFrame.samples)
                    {
                        if (!map.TryGetValue(s.Key, out SamplesSumamry sample)) // Если
                        {
                            sample = GetSS();
                            map.Add(s.Key, sample);
                        }


                        sample.Count += s.Value.callCount;
                        sample.TotalTime += s.Value.watch.Elapsed;
                        sample.totalMemory += s.Value.AllocatedBytes;
                        sample.Samples.Add(s.Value);
                        if(ThreadFrame.thread == MainThreadId)
                        {
                            MainThread.Count++;
                            sample.TotalTime += s.Value.watch.Elapsed;
                        }
                   
                        if(s.Value.parent != null)
                        {
                            if (!ParentMap.TryGetValue(s.Value.parent.name, out var Childs))
                            {
                                ParentMap.Add(s.Value.parent.name.ToString(), new HashSet<SamplesSumamry>() { sample});
                            }
                            else
                            {
                                Childs.Add(sample);
                            }
                        }
                        else
                        {
                            if (!ParentMap.TryGetValue(s.Value.name, out var Childs))
                            {
                                ParentMap.Add(s.Value.name.ToString(), new HashSet<SamplesSumamry>());
                            }
                        }
                    }
                }
            }
            ImGui.SliderInt("Sample history", ref HistoryStackSize, 10, 1000);
            //float[] data = childSamples.Samples.Select(x => (float)x.AverageTime_Ms).ToArray();
            //ImGui.TextDisabled(ss.Key);
            //ImGui.SameLine();
            //ImGui.PlotLines(childSamples.Samples[0].name, ref data[0], data.Length, 0, (childSamples.TotalTime.TotalMilliseconds / childSamples.Count).ToString("f2"));

            foreach (var ss in ParentMap)
            {
                if (ss.Value.Count > 0)
                {
                    foreach (var childSamples in ss.Value)
                    {
                        float[] data = childSamples.Samples.Select(x => (float)x.AverageTime_Ms).ToArray();
                        ImGui.TextDisabled(ss.Key);
                        ImGui.SameLine();
                        ImGui.PlotLines(childSamples.Samples[0].name + $"  | {(childSamples.totalMemory/1024.0f).ToString("f2")}kb  {childSamples.TotalTime.TotalMilliseconds.ToString("f2")} x {childSamples.Count}", ref data[0], data.Length, 0, 
                            (childSamples.TotalTime.TotalMilliseconds / childSamples.Count).ToString("f2")+$", {(childSamples.totalMemory / 1024.0f / childSamples.Count).ToString("f2")}kb");
                    }
                }
                else
                {
                    if(map.TryGetValue(ss.Key.ToString(), out var me))
                    {

                        float[] data = me.Samples.Select(x => (float)x.AverageTime_Ms).ToArray();
                        ImGui.PlotLines(ss.Key, ref data[0], data.Length, 0, (me.TotalTime.TotalMilliseconds / me.Count).ToString("f2"));
                    }
                    
                }
                //if (map.TryGetValue(ss.Key,out var childSamples))
                //{

                // }

                //  ImGui.SameLine();
            }
        }
        public static void DrawProfilerRow(Sample snap, double MaxFrameTime)
        {
            var size = new System.Numerics.Vector2(240, 12);
            if (snap.innerSamples.Count > 0)
            {
                if (ImGui.TreeNodeEx($"{snap.name}", ImGuiTreeNodeFlags.CollapsingHeader,$"{snap.name} {snap.watch.Elapsed.TotalMilliseconds.ToString("f2")} ms {snap.AverageTime_Ms.ToString("f2")} x{snap.callCount} GC {snap.AllocationInBytes}"))
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
