using BepuUtilities.TaskScheduling;
using ConsoleApp1_Pet.Materials;
using ConsoleApp1_Pet.Meshes;
using ConsoleApp1_Pet.Scripts.Core;
using ConsoleApp1_Pet.Shaders;
using ConsoleApp1_Pet.Textures;
using ConsoleApp1_Pet.Новая_папка;
using ImGuiNET;
using Newtonsoft.Json.Linq;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static ConsoleApp1_Pet.Render.Renderer;
using Task = System.Threading.Tasks.Task;

namespace ConsoleApp1_Pet.Render
{
    public class Renderer
    {
        public class InstanceRenderBatch
        {
            public Material instanceMat;
            public List<RenderComponent> objects;
            //public Transform[] transforms;
        }

        private const float CallingSphereRadiusDefaultValue = 3.87f;
        public List<RenderComponent> renderObjects = new List<RenderComponent>();
        public Material materialInUse;
        public Mesh meshInUse;
        public static bool useFrustumCalling;

        public void OnFrameStart()
        {

        }
        public void OnFrameEnd() {FrameCleanup();  }

        public Dictionary<Material, InstanceRenderBatch> MaterialBatching = new Dictionary<Material, InstanceRenderBatch>();

        public void AddToRender(RenderComponent rr)
        {
            renderObjects.Add(rr);
        }
        public struct RenderPassResult
        {
            public int TotalObjectsRendered;
        }
        Dictionary<Camera, Dictionary<Material, Dictionary<Mesh, List<Matrix4>>>> FrostumCullingCash = new Dictionary<Camera, Dictionary<Material, Dictionary<Mesh, List<Matrix4>>>>(8);
        List<RenderComponent> SceneDrawTemp = new List<RenderComponent>(2500);


        public int RenderSceneCommands;
        public int TotalRenderObjectProceded;
        public struct RenderSceneCommand
        {
            public string name;
            public Camera cam;
            public RenderPass pass;
            public FrameBuffer Target;

            public RenderSceneCommand(string name, Camera cam, RenderPass pass)
            {
                this.name = name;
                this.cam = cam;
                this.pass = pass;
            }

            public RenderSceneCommand(string name, Camera cam, FrameBuffer target) : this()
            {
                this.name = name;
                this.cam = cam;
                Target = target;
            }

            public RenderSceneCommand(string name, Camera cam, RenderPass pass, FrameBuffer target) : this(name, cam, pass)
            {
                Target = target;
            }
        }
        //public struct RenderBatch
        //{
        //    public Material material;
        //    public Dictionary<Mesh,MeshRenderBatch> meshBatches;

        //    public RenderBatch(Material material) : this()
        //    {
        //        this.material = material;
        //        meshBatches = new Dictionary<Mesh, MeshRenderBatch>(2);
        //    }

        //    public RenderBatch(Material material, Mesh mesh, Matrix4 matr)
        //    {
        //        this.material = material;
        //        this.meshBatches = new Dictionary<Mesh, MeshRenderBatch>() { { mesh, new MeshRenderBatch(mesh) } };
        //    }
        //}
        //public struct MeshRenderBatch
        //{
        //    public List<Matrix4> matrices;

        //    public MeshRenderBatch(Matrix4 mat)
        //    {
        //        matrices = new List<Matrix4>() { mat };
        //    }

        //    public MeshRenderBatch(List<Matrix4> matrices)
        //    {
        //        this.matrices = matrices;
        //    }
        //}
        List<RenderComponent> FrostumCallingTmp = new List<RenderComponent>(4000);
        Matrix4[] instancedDrawing = new Matrix4[10000];
        private int view = "view".GetHashCode();
        private int projection = "projection".GetHashCode();
        private int viewProjection = "viewProjection".GetHashCode();
        private int mainCameraVP = "mainCameraVP".GetHashCode();
        private int invMainCameraVP = "invMainCameraVP".GetHashCode();

        private Octree<RenderComponent> renderOctree = new Octree<RenderComponent>(10);
        public RenderPassResult RenderScene(RenderSceneCommand cmd)
        {


            RenderSceneCommands++;
            TotalRenderObjectProceded += renderObjects.Count;
            Profiler.BeginSample("Render Pass");
            //Profiler.BeginSample("Build Octree");
            //if (renderOctree.root == null) renderOctree.Rebuild(renderObjects);
            //Profiler.EndSample("Build Octree");
            Camera cam = cmd.cam;

            if (cmd.pass == RenderPass.depth)
            {
                // GL.ColorMask(false, false, false, false);
            }
            int PredictedSize = (int)(TotalRenderObjectProceded / (float)RenderSceneCommands) + 3;
            if (instancedDrawing.Length < PredictedSize) Array.Resize(ref instancedDrawing, PredictedSize);
            //var toRenderSpan = CollectionsMarshal.AsSpan(renderObjects);
            //for (int i = toRenderSpan.Length - 1; i > 0; i--)
            //{
            //    var rr = toRenderSpan[i];

            //}
            int DrawCall = 0;
            long VertCount = 0;
            FrustumCalling.Initialize(cam.ViewProjectionMatrix);
            var view = cam.ViewMatrix;
            var projection = cam.ProjectionMatrix;
            var viewProj = cam.ViewProjectionMatrix;
            Matrix4 InvCamera = viewProj.Inverted();

            var res = new RenderPassResult();
            var toRender = renderObjects;
            [MethodImpl(MethodImplOptions.AggressiveOptimization)]

            void Render(Dictionary<Material, Dictionary<Mesh, List<Matrix4>>> RenBatchList)
            {
                foreach (var rb in RenBatchList)
                {
                    // var rb = MaterialsSpan[i];
                    Profiler.BeginSample("DrawBatch");



                    res.TotalObjectsRendered += 1;
                    var material = rb.Key;
                    if (material != materialInUse)
                    {
                        Profiler.BeginSample("Send material");
                        materialInUse = material;
                        material.Use();
                        Profiler.BeginSample("Send material unifs");

                        material.shader.SetUniform(this.view, view);
                        material.shader.SetUniform(this.projection, projection);
                        material.shader.SetUniform(this.viewProjection, viewProj);
                        material.shader.SetUniform(this.mainCameraVP, cam);
                        material.shader.SetUniform(this.invMainCameraVP, InvCamera);
                        material.shader.SetTexture(Shader.CameraDepth, MainGameWindow.instance.depthBuffer);
                        Profiler.EndSample("Send material unifs");
                        Profiler.EndSample("Send material");
                    }
                    var meshBatches = rb.Value;
                    foreach (var mb in meshBatches)
                    {
                        Profiler.BeginSample("Send mesh");
                        if (meshInUse != mb.Key)
                        {
                            meshInUse = mb.Key;
                            meshInUse.FillBuffers();
                            GL.BindVertexArray(meshInUse.VAO);

                        }
                        Profiler.EndSample("Send mesh");
                        var sp = CollectionsMarshal.AsSpan(mb.Value);
                        for (int i = sp.Length - 1; i >= 0; i--)
                        // foreach (var mt in mb.matrices)
                        {
                            Profiler.BeginSample("DrawCall");
                            materialInUse.shader.SetMatrix(0, mb.Value[i]);
                            //GL.DrawElements

                            //GL.MultiDrawElements

                            GL.DrawElements(PrimitiveType.Triangles, meshInUse.triangles.Length, DrawElementsType.UnsignedInt, 0);
                            DrawCall++;
                            VertCount += meshInUse.vertices.LongLength;
                            Profiler.EndSample("DrawCall");
                        }
                    }
                    Profiler.EndSample("DrawBatch");
                }
            }

            if (useFrustumCalling)
            {
                if (FrostumCullingCash.TryGetValue(cam, out var RenBatchList))
                {

                    //var MaterialsSpan = CollectionsMarshal.AsSpan(RenBatchList);
                    Render(RenBatchList);

                }
                else
                {
                    Profiler.BeginSample("FrostumCalling");
                    FrostumCallingTmp.Clear();
                    // RenBatchList = new List<RenderBatch>(2);
                    // ConcurrentQueue<RenderComponent> ParallelFrustumCalling = new ConcurrentQueue<RenderComponent>();
                    //  Queue<RenderComponent> ParallelFrustumCalling = new Queue<RenderComponent>();
                    //Span<RenderComponent> ParallelFrustumCalling = stackalloc RenderComponent[renderObjects.Count];
                    //Parallel.ForEach<RenderComponent>(renderObjects, new ParallelOptions() { MaxDegreeOfParallelism = 2 }, item =>
                    // {
                    foreach (var item in renderObjects)
                    {
                        // Your calculation goes here
                        //renderOctree.root.DoFrostumCall()

                        if (FrustumCalling.IsSphereInside(item.transform.position, CallingSphereRadiusDefaultValue))
                        {
                            FrostumCallingTmp.Add(item);

                        }
                    }
                    // });

                    RenBatchList = DoBatching(FrostumCallingTmp);
                    FrostumCullingCash.Add(cam, RenBatchList);
                    Profiler.EndSample("FrostumCalling");
                    Render(RenBatchList);

                }
            }
            else
            {
                var RenBatchList = DoBatching(toRender);
                Render(RenBatchList);
            }
            ImGui.Text($"{cmd.name}: T: {renderObjects.Count} , DC: {DrawCall} , V:{VertCount}");
            materialInUse = null;
            meshInUse = null;
            if (cmd.pass == RenderPass.depth)
            {
                // GL.ColorMask(true, true, true, true);
            }
            Profiler.EndSample("Render Pass");
            return res;
        }
        public enum RenderPass
        {
            main,
            depth
        }
        // Memory pool for matrix lists
        private static readonly Stack<List<Matrix4>> _matrixPool = new();

        // Primary storage (persists between frames)
        private static readonly Dictionary<Material, Dictionary<Mesh, List<Matrix4>>> _resultCache
            = new(64, ReferenceEqualityComparer.Instance);

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        static Dictionary<Material, Dictionary<Mesh, List<Matrix4>>> DoBatching(List<RenderComponent> toRender)
        {
            Profiler.BeginSample("HyperBatching");

            

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
                    _resultCache,
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
                    matrixList = _matrixPool.TryPop(out var pooledList)
                        ? pooledList
                        : new List<Matrix4>(64);
                    matrixList.Clear();
                }

                
                matrixList!.Add(item.transform);
            }

            Profiler.EndSample("HyperBatching");
            return _resultCache;
        }

        // Call this after rendering completes
        public void FrameCleanup()
        {
            Profiler.BeginSample("Render-FrameCleanUp");
            FrostumCullingCash.Clear(); 
            SceneDrawTemp.Clear(); 
            renderOctree.Clear();
            foreach (var meshDict in _resultCache.Values)
            {
                foreach (var (mesh, list) in meshDict)
                {
                    if (list.Capacity >= 64 && list.Capacity <= 4096)
                    {
                        list.Clear();
                        _matrixPool.Push(list);
                    }
                    //Remove(mesh);
                }
                meshDict.Clear();
            }

            Profiler.EndSample("Render-FrameCleanUp");
        }
    }

    //public class Renderer
    //{
    //    // Improved data structures
    //    private readonly Dictionary<Material, Dictionary<Mesh, List<Matrix4>>> _batchDictionary = new();
    //    private RenderComponent[] _renderComponents = new RenderComponent[1000];
    //    private int _componentCount;

    //    // Camera UBO for uniform sharing
    //    private int _cameraUBO;
    //    private bool _uboInitialized;

    //    // Instance buffers
    //    private int _instanceVBO;
    //    private Matrix4[] _instanceMatrices = new Matrix4[10000];
    //    private int _instanceCount;
    //    internal static bool useFrustumCalling;

    //    public void Initialize()
    //    {
    //        // Set up instance buffer
    //        _instanceVBO = GL.GenBuffer();
    //        GL.BindBuffer(BufferTarget.ArrayBuffer, _instanceVBO);
    //        GL.BufferData(BufferTarget.ArrayBuffer, _instanceMatrices.Length * sizeof(float) * 16,
    //                    IntPtr.Zero, BufferUsageHint.StreamDraw);

    //        // Create camera UBO
    //        _cameraUBO = GL.GenBuffer();
    //        GL.BindBuffer(BufferTarget.UniformBuffer, _cameraUBO);
    //        GL.BufferData(BufferTarget.UniformBuffer, 3 * sizeof(float) * 16, IntPtr.Zero, BufferUsageHint.DynamicDraw);
    //        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, _cameraUBO);
    //    }

    //    public void AddToRender(RenderComponent rr)
    //    {
    //        if (_componentCount >= _renderComponents.Length)
    //            Array.Resize(ref _renderComponents, _renderComponents.Length * 2);

    //        _renderComponents[_componentCount++] = rr;
    //    }
    //    public struct RenderPassResult
    //    {
    //        public int TotalObjectsRendered;
    //    }
    //    public RenderPassResult RenderScene(RenderSceneCommand cmd)
    //    {
    //        Profiler.BeginSample("RenderScene");

    //        UpdateCameraUBO(cmd.cam);
    //        FrustumCulling(cmd.cam);
    //        BuildBatches();
    //        RenderBatches();

    //        Profiler.EndSample("RenderScene");
    //        return new RenderPassResult();
    //    }

    //    private void UpdateCameraUBO(Camera cam)
    //    {
    //        if (!_uboInitialized)
    //        {
    //            Initialize();
    //            _uboInitialized = true;
    //        }

    //        var viewProj = cam.ViewProjectionMatrix;
    //        var invViewProj = viewProj.Inverted();

    //        GL.BindBuffer(BufferTarget.UniformBuffer, _cameraUBO);

    //        var ViewMat = cam.ViewMatrix;
    //        var ProjectionMatrix = cam.ProjectionMatrix;

    //        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero,
    //            Matrix4x4SizeByte, ref ViewMat);
    //        GL.BufferSubData(BufferTarget.UniformBuffer, Matrix4x4SizeByte,
    //            Matrix4x4SizeByte, ref ProjectionMatrix);
    //        GL.BufferSubData(BufferTarget.UniformBuffer, 2 * Matrix4x4SizeByte,
    //            Matrix4x4SizeByte, ref viewProj);
    //    }

    //    private void FrustumCulling(Camera cam)
    //    {

    //    }

    //    private void BuildBatches()
    //    {
    //        _batchDictionary.Clear();

    //        for (int i = 0; i < _componentCount; i++)
    //        {
    //            var rc = _renderComponents[i];
    //            if (!_batchDictionary.TryGetValue(rc.material, out var meshDict))
    //            {
    //                meshDict = new Dictionary<Mesh, List<Matrix4>>();
    //                _batchDictionary[rc.material] = meshDict;
    //            }

    //            if (!meshDict.TryGetValue(rc.mesh, out var matrices))
    //            {
    //                matrices = new List<Matrix4>();
    //                meshDict[rc.mesh] = matrices;
    //            }

    //            matrices.Add(rc.transform.WorldMatrix);
    //        }
    //    }

    //    private void RenderBatches()
    //    {
    //        foreach (var materialEntry in _batchDictionary)
    //        {
    //            materialEntry.Key.Use();
    //            materialEntry.Key.shader.SetUniformBlock("CameraData", 0);

    //            foreach (var meshEntry in materialEntry.Value)
    //            {
    //                var mesh = meshEntry.Key;
    //                var matrices = meshEntry.Value;

    //                mesh.Bind();
    //                UploadInstanceData(matrices);
    //                SetInstanceAttributes(mesh);

    //                GL.DrawElementsInstanced(
    //                    PrimitiveType.Triangles,
    //                    mesh.vertices.Length,
    //                    DrawElementsType.UnsignedInt,
    //                    IntPtr.Zero,
    //                    matrices.Count
    //                );
    //            }
    //        }
    //        _batchDictionary.Clear();
    //    }

    //    private void UploadInstanceData(List<Matrix4> matrices)
    //    {
    //        if (_instanceMatrices.Length < matrices.Count)
    //            Array.Resize(ref _instanceMatrices, matrices.Count);

    //        matrices.CopyTo(_instanceMatrices);
    //        GL.BindBuffer(BufferTarget.ArrayBuffer, _instanceVBO);
    //        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero,
    //            matrices.Count * Matrix4x4SizeByte, _instanceMatrices);
    //    }
    //    const int Vector4SizeByte = 4 * 4;
    //    const int Matrix4x4SizeByte = Vector4SizeByte * 4;
    //    private void SetInstanceAttributes(Mesh mesh)
    //    {
    //        mesh.Bind();

    //        // Enable and set up the mat4 attribute
    //        int location = 3; // Assuming vertex attributes 0-2 are used for position, normal, texCoord
    //        GL.EnableVertexAttribArray(location);
    //        GL.VertexAttribPointer(location, 4, VertexAttribPointerType.Float, false, Matrix4x4SizeByte, 0);
    //        GL.EnableVertexAttribArray(location + 1);
    //        GL.VertexAttribPointer(location + 1, 4, VertexAttribPointerType.Float, false, Matrix4x4SizeByte, Vector4SizeByte);
    //        GL.EnableVertexAttribArray(location + 2);
    //        GL.VertexAttribPointer(location + 2, 4, VertexAttribPointerType.Float, false, Matrix4x4SizeByte, 2 * Vector4SizeByte);
    //        GL.EnableVertexAttribArray(location + 3);
    //        GL.VertexAttribPointer(location + 3, 4, VertexAttribPointerType.Float, false, Matrix4x4SizeByte, 3 * Vector4SizeByte);

    //        // Set divisor for instanced rendering
    //        GL.VertexAttribDivisor(location, 1);
    //        GL.VertexAttribDivisor(location + 1, 1);
    //        GL.VertexAttribDivisor(location + 2, 1);
    //        GL.VertexAttribDivisor(location + 3, 1);
    //    }

    //    internal void OnFrameEnd()
    //    {

    //    }

    //    public enum RenderPass
    //    {
    //        main,
    //        depth
    //    }


    //    public struct RenderSceneCommand
    //    {
    //        public string name;
    //        public Camera cam;
    //        public RenderPass pass;
    //        public FrameBuffer Target;

    //        public RenderSceneCommand(string name, Camera cam, RenderPass pass)
    //        {
    //            this.name = name;
    //            this.cam = cam;
    //            this.pass = pass;
    //        }

    //        public RenderSceneCommand(string name, Camera cam, FrameBuffer target) : this()
    //        {
    //            this.name = name;
    //            this.cam = cam;
    //            Target = target;
    //        }

    //        public RenderSceneCommand(string name, Camera cam, RenderPass pass, FrameBuffer target) : this(name, cam, pass)
    //        {
    //            Target = target;
    //        }
    //    }

    //}
}
