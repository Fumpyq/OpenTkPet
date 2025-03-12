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
using static Profiling.Profiler;


namespace Profiling
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

            public TimeSpan ExclusiveDuration { get => watch.Elapsed - TimeSpan.FromTicks(innerSamples.Sum(x => x.watch.Elapsed.Ticks)); }

            public HashSet<Sample> innerSamples = new HashSet<Sample>();
        }
        public class ProfilerThreadFrame
        {
            public Dictionary<string, Sample> samples = new Dictionary<string, Sample>();
            public Stack<Sample> sampQue = new Stack<Sample>();
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
                    sampQue.Push(sample);
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
                    sampQue.Push(smpl);
                    smpl.startMemoryUsage = GC.GetTotalMemory(false);
                    //allSamples.Add(smpl);



                }
            }
            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            public void EndSample()
            {
                if (sampQue.TryPop(out var smpl))
                {
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
                        //var sample = ..Value;

                        // Labels[i] =
                        if (snap.parent == null)
                            DrawProfilerRow(snap, Max);
                        //if (ImGui.TreeNodeEx($"{sample.name} {sample.watch.Elapsed.TotalMilliseconds.ToString("f2")} ms avg:{sample.AverageTime_Ms.ToString("f2")} x{sample.callCount}", ImGuiTreeNodeFlags.CollapsingHeader))
                        //{

                        //    ImGui.TreePop();
                        //}
                        //ImGui.ProgressBar((float)(sample.watch.Elapsed.TotalMilliseconds / Max),
                        //       size,
                        //       $"{sample.name} {sample.watch.Elapsed.TotalMilliseconds.ToString("f2")} ms avg:{sample.AverageTime_Ms.ToString("f2")} x{sample.callCount}"
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
        class SamplesSummary
        {
            public string Name;
            public SamplesSummary parent;
            public int ChildDepth;
            public int Count;
            public TimeSpan TotalTime;
            public long totalMemory;
            public List<Sample> Samples = new List<Sample>(4);

            public void SetParent(SamplesSummary parent)
            {
                this.parent = parent;
                ChildDepth = parent?.ChildDepth + 1 ?? 0;
            }

            public SamplesSummary Clear()
            {
                Count = 0;
                TotalTime = TimeSpan.Zero;
                totalMemory = 0;
                Samples.Clear();
                return this;
            }
        }

        private static Queue<SamplesSummary> SamplesSummaryPool = new Queue<SamplesSummary>();
        private static SamplesSummary GetSS() =>
            SamplesSummaryPool.TryDequeue(out SamplesSummary s) ? s.Clear() : new SamplesSummary();

        // Plot style variants



        private static void DrawParentChildHierarchy(SamplesSummary node,
            Dictionary<string, (SamplesSummary Parent, HashSet<SamplesSummary> Children)> parentMap,
            int depth)
        {
            // Calculate statistics
            double avgTime = node.TotalTime.TotalMilliseconds / node.Count;
            double totalTime = node.TotalTime.TotalMilliseconds;
            double avgMemory = node.totalMemory / 1024.0 / node.Count;
            double totalMemory = node.totalMemory / 1024.0;

            // Create overlay text
            string overlay = $"Avg: {avgTime:f2}ms, {avgMemory:f2}kb | Total: {totalTime:f2}ms, {totalMemory:f2}kb";

            // Draw the plot
            string indent = new string(' ', depth * 2);
            string label = $"{indent}{node.Name}";
            float[] data = node.Samples.Select(x => (float)x.AverageTime_Ms).ToArray();

            

            ImGui.PlotHistogram(label, ref data[0], data.Length, 0, overlay);

            // Draw children
            if (parentMap.TryGetValue(node.Name, out var children))
            {
                foreach (var child in children.Children.OrderBy(c => c.Name))
                {
                    DrawParentChildHierarchy(child, parentMap, depth + 2);
                }
            }
        }

        private static void DrawThreadSummary(IOrderedEnumerable<SamplesSummary> map, int threadId)
        {
            int maxSamples = map.Max(x => x.Samples.Count);
            // Calculate average across all samples in the history interval
            double totalAvgTime = map.Sum(x => x.TotalTime.TotalMilliseconds) / maxSamples;
            double totalAvgMemory = map.Sum(x => x.totalMemory) / 1024.0 / maxSamples;

            // Create summary data
            float[] summaryData = new float[maxSamples];
            if (map.Any())
            {
              
                for (int i = 0; i < maxSamples; i++)
                {
                    summaryData[i] = (float)map
                        .Where(x => x.ChildDepth==0)
                        .Select(x => x.Samples[i].AverageTime_Ms)
                        .DefaultIfEmpty()
                        .Average();
                }
            }

            // Draw summary
            ImGui.Separator();
            ImGui.TextColored(new System.Numerics.Vector4(0, 1, 0, 1), $"Thread {threadId} Summary");
            if (summaryData.Length > 0)
            {
                string overlay = $"Avg: {totalAvgTime:f2}ms, {totalAvgMemory:f2}kb";
              
                ImGui.PlotHistogram("##summary", ref summaryData[0], summaryData.Length, 0, overlay);
            }
        }

        public static void DrawHistoryPlot(Span<List<ProfilerThreadFrame>> hist)
        {
            var threadData = new Dictionary<int, (
                SamplesSummary Root,
                Dictionary<string, SamplesSummary> Map,
                Dictionary<string, (SamplesSummary Parent, HashSet<SamplesSummary> Children)> ParentMap
            )>();

            // Data collection phase
            foreach (var threadFrames in hist)
            {
                foreach (var threadFrame in threadFrames)
                {
                    int threadId = threadFrame.thread;

                    if (!threadData.TryGetValue(threadId, out var data))
                    {
                        data = (
                            Root: GetSS(),
                            Map: new Dictionary<string, SamplesSummary>(),
                            ParentMap: new Dictionary<string, (SamplesSummary, HashSet<SamplesSummary>)>()
                        );
                        threadData.Add(threadId, data);
                    }

                    foreach (var s in threadFrame.samples)
                    {
                        if (!data.Map.TryGetValue(s.Key, out SamplesSummary sample))
                        {
                            sample = GetSS();
                            sample.Name = s.Key;
                            data.Map.Add(s.Key, sample);
                        }

                        sample.Count += s.Value.callCount;
                        sample.TotalTime += s.Value.watch.Elapsed;
                        sample.totalMemory += s.Value.AllocationInBytes;
                        sample.Samples.Add(s.Value);

                        // Handle parent relationships within the same thread
                        if (s.Value.parent != null)
                        {
                            var parName = s.Value.parent.name;
                            if (data.Map.TryGetValue(parName, out SamplesSummary parentSample))
                            {
                                if (!data.Item3.TryGetValue(parName, out var children))
                                {
                                    children = (parentSample, new HashSet<SamplesSummary>());
                                    data.Item3.Add(parName, children);
                                }
                                sample.SetParent(parentSample);
                                children.Item2.Add(sample);
                            }
                        }
                        else
                        {
                            // Root sample for this thread
                            sample.SetParent(data.Root);
                        }
                    }
                }
            }

            // Visualization phase
            ImGui.SliderInt("Sample history", ref HistoryStackSize, 10, 1000);

            foreach (var (threadId, (root, map, parentMap)) in threadData)
            {
                ImGui.Separator();
                ImGui.TextColored(new System.Numerics.Vector4(1, 0.5f, 0, 1), $"Thread {threadId}");

                // Get root-level parents (direct children of thread root)
                var topLevelParents = map.Values
                    .Where(s => s.parent == root)
                    .OrderBy(s => s.Name);

                foreach (var parent in topLevelParents)
                {
                    DrawParentChildHierarchy(parent, parentMap, 0);
                }

                // Draw thread summary
                DrawThreadSummary(topLevelParents, threadId);
            }
        }
        public static void DrawProfilerRow(Sample sample, double MaxFrameTime)
        {
            var size = new System.Numerics.Vector2(240, 12);
            if (sample.innerSamples.Count > 0)
            {
                var label = $"{sample.name} || {sample.ExclusiveDuration.TotalMilliseconds:F2}ms " +
                $"(Total: {sample.watch.Elapsed.TotalMilliseconds:F2}ms) " +
                $"x{sample.callCount} " +
                $"Gc: {sample.AllocationInBytes / 1024:F2}KB";

                //var label = $"{sample.name} {sample.watch.Elapsed.TotalMilliseconds.ToString("f2")} ms {sample.AverageTime_Ms.ToString("f2")} x{sample.callCount} GC {sample.AllocationInBytes}";
                if (ImGui.TreeNodeEx($"{sample.name}", ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.Selected, label))
                {
                    foreach (var s in sample.innerSamples)
                    {
                        DrawProfilerRow(s, MaxFrameTime);
                       
                    }
                    ImGui.TreePop();

                }
            }
            else
            {
                ImGui.ProgressBar((float)(sample.watch.Elapsed.TotalMilliseconds / MaxFrameTime),
                  size,
                  $"{sample.name} {sample.watch.Elapsed.TotalMilliseconds.ToString("f2")} ms avg:{sample.AverageTime_Ms.ToString("f2")} x{sample.callCount}"
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
