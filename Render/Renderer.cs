using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using ImGuiNET;
using ConsoleApp1_Pet.Materials;
using ConsoleApp1_Pet.Shaders;
using ConsoleApp1_Pet.Новая_папка;
using ConsoleApp1_Pet.Meshes;
using Profiling;
using ConsoleApp1_Pet.Renovation;
using System.Diagnostics;

namespace ConsoleApp1_Pet.Render
{
    public class Renderer
    {
        private const float CullingRadius = 3.87f;
        private static readonly Stack<List<Matrix4>> MatrixPool = new();
        private static readonly Stack<Dictionary<Material, Dictionary<Mesh, List<Matrix4>>>> BatchCache = new (12);
        private static readonly Stack<Dictionary<Material, Dictionary<Mesh, List<Matrix4>>>> FrameUsed = new (12);

        private readonly List<RenderComponent> renderObjects = new();
        private readonly Dictionary<Camera, Dictionary<Material, Dictionary<Mesh, List<Matrix4>>>> frustumCache = new(8);

        // New pipeline system
        private readonly List<RenderStep> renderPipeline = new();
        private readonly Dictionary<string, RenderStep> stepLookup = new();

        private Material currentMaterial;
        private Mesh currentMesh;
        public static bool useFrustumCulling;
        public void AddToRender(RenderComponent component) => renderObjects.Add(component);
        public void OnFrameStart() { }
        public void OnFrameEnd() => FrameCleanup();
        private List<RenderComponent> RenderScene_visibleObjects = new List<RenderComponent>(1000);
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public RenderPassResult RenderScene(RenderSceneCommand cmd)
        {
            var result = new RenderPassResult();
            var cam = cmd.cam;

            Profiler.BeginSample("Render Pass");
            FrustumCulling.Initialize(cam.ViewProjectionMatrix);

            using (var bind = cmd.Target.Bind())
            {
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

                var viewProj = cam.ViewProjectionMatrix;
                var view = cam.ViewMatrix;
                var projection = cam.ProjectionMatrix;
                var invViewProj = viewProj.Inverted();

                Dictionary<Material, Dictionary<Mesh, List<Matrix4>>> batches;
                if (useFrustumCulling && frustumCache.TryGetValue(cam, out batches))
                {
                    RenderBatches(batches, view, projection, viewProj, invViewProj, cam, ref result);
                }
                else
                {
                    Profiler.BeginSample("Frustum Culling");
                    RenderScene_visibleObjects.Clear();
                    foreach (var obj in CollectionsMarshal.AsSpan(renderObjects))
                    {
                        if (FrustumCulling.IsSphereInside(obj.transform.position, CullingRadius))
                            RenderScene_visibleObjects.Add(obj);
                    }
                    Profiler.EndSample("Frustum Culling");
                    batches = BatchObjects(RenderScene_visibleObjects);
                    if (useFrustumCulling) frustumCache[cam] = batches;


                    RenderBatches(batches, view, projection, viewProj, invViewProj, cam, ref result);
                }

                ImGui.Text($"{cmd.name}: Objects: {renderObjects.Count}, DrawCalls: {result.DrawCalls}, Verts: {result.VerticesDrawn}");
                currentMaterial = null;
                currentMesh = null;

                Profiler.EndSample("Render Pass");
            }
            return result;
        }
        public class RenderPassResult
        {
            public int TotalObjectsRendered;
            public string PassName;
            public TimeSpan Duration;
            public int DrawCalls;
            public long VerticesDrawn;
            public int ObjectsCulled;
            public int TotalObjects;
            public int MaterialsUsed;
            public int MeshesUsed;
            public long MemoryUsage;

            public override string ToString() =>
                $"{PassName}: {DrawCalls} DC, {VerticesDrawn:N0} Verts, {ObjectsCulled:N0} Culled";
        }

        public struct RenderStep
        {
            public string Name;
            public Func<Camera> Camera;
            public FrameBuffer Target;
            public Action<RenderStepContext> PreExecute;
            public Action<RenderStepContext> PostExecute;
            public RenderPass PassType;
            public bool Enabled =true;

            public RenderStep()
            {
            }
        }

        public struct RenderStepContext
        {
            public Renderer Renderer;
            public RenderStep Step;
            public RenderPassResult Result;
        }

        // Pipeline management
        public void AddRenderStep(RenderStep step, int? order = null)
        {
            if (order.HasValue)
                renderPipeline.Insert(order.Value, step);
            else
                renderPipeline.Add(step);

            stepLookup[step.Name] = step;
        }

        public void EnableStep(string name) => UpdateStepState(name, true);
        public void DisableStep(string name) => UpdateStepState(name, false);

        private void UpdateStepState(string name, bool state)
        {
            if (stepLookup.TryGetValue(name, out var step))
            {
                step.Enabled = state;
                stepLookup[name] = step;
            }
        }

        // Modified main render method
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public List<RenderPassResult> ExecuteRenderPipeline()
        {
            var results = new List<RenderPassResult>();

            foreach (var step in renderPipeline.Where(s => s.Enabled))
            {
                var result = ExecuteRenderStep(step);
                results.Add(result);
            }

            return results;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private RenderPassResult ExecuteRenderStep(RenderStep step)
        {
            var result = new RenderPassResult
            {
                PassName = step.Name,
                TotalObjects = renderObjects.Count
            };

            var sw = Stopwatch.StartNew();
            var context = new RenderStepContext { Renderer = this, Step = step, Result = result };

            try
            {
                step.PreExecute?.Invoke(context);
                using (new ProfilerScope(step.Name))
                {
                    var cmd = new RenderSceneCommand(
                        step.Name,
                        step.Camera(),
                        step.PassType,
                        step.Target
                    );

                    var passResult = RenderScene(cmd);
                    UpdateResult(ref result, passResult);
                }

                step.PostExecute?.Invoke(context);
            }
            finally
            {
                result.Duration = sw.Elapsed;
                result.ObjectsCulled = result.TotalObjects - result.TotalObjectsRendered;
            }

            return result;
        }

        private void UpdateResult(ref RenderPassResult dest, RenderPassResult source)
        {
            dest.DrawCalls = source.DrawCalls;
            dest.VerticesDrawn = source.VerticesDrawn;
            dest.TotalObjectsRendered = source.TotalObjectsRendered;
            dest.MaterialsUsed = source.MaterialsUsed;
            dest.MeshesUsed = source.MeshesUsed;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void RenderBatches(
            Dictionary<Material, Dictionary<Mesh, List<Matrix4>>> batches,
            Matrix4 view, Matrix4 projection, Matrix4 viewProj, Matrix4 invViewProj, Camera cam,
            ref RenderPassResult result)
        {
            foreach (var materialBatch in  batches)
            {
                var material = materialBatch.Key;
                result.MeshesUsed += materialBatch.Value.Count;
                if (currentMaterial != material)
                {
                    currentMaterial = material;
                    material.Use();
                    material.Shader.SetUniform("view".GetHashCode(), view);
                    material.Shader.SetUniform("projection".GetHashCode(), projection);
                    material.Shader.SetUniform("viewProjection".GetHashCode(), viewProj);
                    material.Shader.SetUniform("mainCameraVP".GetHashCode(), cam);
                    material.Shader.SetUniform("invMainCameraVP".GetHashCode(), invViewProj);
                    // material.shader.SetTexture(Shader.CameraDepth, MainGameWindow.instance.depthBuffer);
                }

                foreach (var meshBatch in materialBatch.Value)
                {
                    var mesh = meshBatch.Key;
                    if (currentMesh != mesh)
                    {
                        currentMesh = mesh;
                        mesh.FillBuffers();
                        GL.BindVertexArray(mesh.VAO);
                    }

                    foreach (var matrix in meshBatch.Value)
                    {
                        material.Shader.SetMatrix(0, matrix);
                        GL.DrawElements(PrimitiveType.Triangles, mesh.triangles.Length, DrawElementsType.UnsignedInt, 0);
                        result.DrawCalls++;
                        result.VerticesDrawn += mesh.vertices.Length;
                        result.TotalObjectsRendered++;
                    }
                    
                    //MatrixPool.Push(meshBatch.Value);
                }
            }
           if(!FrameUsed.Contains(batches)) FrameUsed.Push(batches);
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        static Dictionary<Material, Dictionary<Mesh, List<Matrix4>>> BatchObjects(List<RenderComponent> toRender)
        {
            Profiler.BeginSample("HyperBatching");


            //var gr = toRender.GroupBy(x => x.material);

            if(!BatchCache.TryPop(out var res))
            {
                res = new Dictionary<Material, Dictionary<Mesh, List<Matrix4>>>(64, ReferenceEqualityComparer.Instance);
            }

            // Phase 2: ID-based processing with direct memory access
            var span = CollectionsMarshal.AsSpan(toRender);
            ref var start = ref MemoryMarshal.GetReference(span);

            for (int i = 0; i < span.Length; i++)
            {
                ref readonly var item = ref Unsafe.Add(ref start, i);
                var material = item.material;
                var mesh = item.mesh;

                // Tier 1: Material lookup
                ref var meshDict = ref CollectionsMarshal.GetValueRefOrAddDefault(
                    res,
                    material,
                    out bool materialExists
                );

                if (!materialExists)
                {
                    meshDict = new Dictionary<Mesh, List<Matrix4>>(
                        4,
                        ReferenceEqualityComparer.Instance
                    );
                }

                // Tier 2: Mesh lookup
                ref var matrixList = ref CollectionsMarshal.GetValueRefOrAddDefault(
                    meshDict!,
                    mesh,
                    out bool meshExists
                );

                if (!meshExists)
                {
                    matrixList = MatrixPool.TryPop(out var pooledList)
                        ? pooledList
                        : new List<Matrix4>(64);
                    matrixList.Clear();
                }


                matrixList!.Add(item.transform);
            }

            Profiler.EndSample("HyperBatching");
            return res;
        }

        public void FrameCleanup()
        {
            Profiler.BeginSample("Frame Cleanup");

            frustumCache.Clear();
            
            foreach (var v in FrameUsed)
                BatchCache.Push(v);
            FrameUsed.Clear();
            foreach (var bb in BatchCache)
            {
                foreach (var materialEntry in bb.Values)
                {
                    foreach (var (mesh, matrices) in materialEntry)
                    {
                        if (matrices.Capacity >= 64 && matrices.Capacity <= 4096)
                        {
                            matrices.Clear();
                            MatrixPool.Push(matrices);
                        }
                    }
                    materialEntry.Clear();
                }
                bb.Clear();
            }

            Profiler.EndSample("Frame Cleanup");
        }

        //public struct RenderPassResult
        //{
        //    public int TotalObjectsRendered;
        //    public int DrawCalls;
        //    public long VerticesDrawn;
        //}

        public enum RenderPass { main, depth }

        public struct RenderSceneCommand
        {
            public string name;
            public Camera cam;
            public RenderPass pass;
            public FrameBuffer Target;

            public RenderSceneCommand(string name, Camera cam, RenderPass pass, FrameBuffer target = null)
            {
                this.name = name;
                this.cam = cam;
                this.pass = pass;
                Target = target;
            }
        }
    }
}