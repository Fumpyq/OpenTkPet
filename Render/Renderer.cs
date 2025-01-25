using BepuUtilities.TaskScheduling;
using ConsoleApp1_Pet.Materials;
using ConsoleApp1_Pet.Meshes;
using ConsoleApp1_Pet.Shaders;
using ConsoleApp1_Pet.Textures;
using ConsoleApp1_Pet.Новая_папка;
using ImGuiNET;
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
        public void OnFrameEnd() { FrostumCullingCash.Clear();SceneDrawTemp.Clear(); }

        public Dictionary<Material, InstanceRenderBatch>  MaterialBatching = new Dictionary<Material, InstanceRenderBatch>  ();

        public void AddToRender(RenderComponent rr)
        {
            renderObjects.Add(rr);
        }
        public struct RenderPassResult
        {
            public int TotalObjectsRendered;
        }
        Dictionary<Camera, List<RenderBatch>> FrostumCullingCash = new Dictionary<Camera, List<RenderBatch>>(8);
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
        public struct RenderBatch
        {
            public Material material;
            public List<MeshRenderBatch> meshBatches;

            public RenderBatch(Material material) : this()
            {
                this.material = material;
                meshBatches = new List<MeshRenderBatch>(2);
            }

            public RenderBatch(Material material, Mesh mesh,Matrix4 matr)
            {
                this.material = material;
                this.meshBatches = new List<MeshRenderBatch>() { new MeshRenderBatch(mesh, matr) };
            }
        }
        public struct MeshRenderBatch
        {
            public Mesh mesh;
            public List<Matrix4> matrices;

            public MeshRenderBatch(Mesh mesh,Matrix4 mat)
            {
                this.mesh = mesh;
                matrices = new List<Matrix4>() { mat };
            }

            public MeshRenderBatch(Mesh mesh, List<Matrix4> matrices)
            {
                this.mesh = mesh;
                this.matrices = matrices;
            }
        }
        List<RenderComponent> FrostumCallingTmp = new List<RenderComponent>(4000);
        Matrix4[] instancedDrawing = new Matrix4[10000];
        private int view            = "view".GetHashCode();
        private int projection      = "projection".GetHashCode();
        private int viewProjection  = "viewProjection".GetHashCode();
        private int mainCameraVP    = "mainCameraVP".GetHashCode();
        private int invMainCameraVP = "invMainCameraVP".GetHashCode();
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public RenderPassResult RenderScene(RenderSceneCommand cmd)
        {     
            
           
        RenderSceneCommands++;
            TotalRenderObjectProceded += renderObjects.Count;
            Profiler.BeginSample("Render Pass");
            Camera cam = cmd.cam;
    
            if (cmd.pass == RenderPass.depth)
            {
               // GL.ColorMask(false, false, false, false);
            }
            int PredictedSize = (int)(TotalRenderObjectProceded / (float)RenderSceneCommands) + 3;
            if (instancedDrawing.Length< PredictedSize)Array.Resize(ref instancedDrawing,PredictedSize);
            //var toRenderSpan = CollectionsMarshal.AsSpan(renderObjects);
            //for (int i = toRenderSpan.Length - 1; i > 0; i--)
            //{
            //    var rr = toRenderSpan[i];
                
            //}
                int DrawCall = 0;
            long VertCount =0;
            FrustumCalling.Initialize(cam.ViewProjectionMatrix);
            var view = cam.ViewMatrix;
            var projection = cam.ProjectionMatrix;
            var viewProj = cam.ViewProjectionMatrix;
            Matrix4 InvCamera = viewProj.Inverted();
            
            var res = new RenderPassResult();
            var toRender = renderObjects;
            [MethodImpl(MethodImplOptions.AggressiveOptimization)]

            void Render(List<RenderBatch> RenBatchList)
            {
                foreach (var rb in RenBatchList)
                {
                    // var rb = MaterialsSpan[i];
                    Profiler.BeginSample("DrawBatch");



                    res.TotalObjectsRendered += 1;
                  
                    if (rb.material != materialInUse)
                    {
                        Profiler.BeginSample("Send material");
                        materialInUse = rb.material;
                        rb.material.Use();
                        Profiler.BeginSample("Send material unifs");
              
                        rb.material.shader.SetUniform(this.view, view);
                        rb.material.shader.SetUniform(this.projection, projection);
                        rb.material.shader.SetUniform(this.viewProjection, viewProj);
                        rb.material.shader.SetUniform(this.mainCameraVP, cam);
                        rb.material.shader.SetUniform(this.invMainCameraVP, InvCamera);
                        rb.material.shader.SetTexture(Shader.CameraDepth, MainGameWindow.instance.depthBuffer);
                        Profiler.EndSample("Send material unifs");
                        Profiler.EndSample("Send material");
                    }
                    
                    foreach (var mb in rb.meshBatches)
                    {
                        Profiler.BeginSample("Send mesh");
                        if (meshInUse != mb.mesh)
                        {
                            meshInUse = mb.mesh;
                            meshInUse.FillBuffers();
                            GL.BindVertexArray(meshInUse.VAO);

                        }
                        Profiler.EndSample("Send mesh");
                        var sp = CollectionsMarshal.AsSpan(mb.matrices);
                        for (int i = sp.Length- 1;i>=0;i--)
                       // foreach (var mt in mb.matrices)
                        {
                            Profiler.BeginSample("DrawCall");
                            materialInUse.shader.SetMatrix(0, mb.matrices[i]);
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
                   foreach (var item in renderObjects) { 
                        // Your calculation goes here
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
                List<RenderBatch> RenBatchList = DoBatching(toRender);
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
            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            static List<RenderBatch> DoBatching(IEnumerable<RenderComponent> toRender)
            {
                Profiler.BeginSample("Batching");
                var RenBatchList = new List<RenderBatch>(2);
                var MatGroup = toRender.GroupBy(x => x.material);
                foreach (var mg in MatGroup)
                {
                    var v1 = new RenderBatch(mg.Key);
                    var MeshGroup = mg.GroupBy(x => x.mesh);
                    foreach (var mesh in MeshGroup)
                    {
                        v1.meshBatches.Add(new MeshRenderBatch(mesh.Key, mesh.Select(x => (Matrix4)x.transform).ToList()));
                    }
                    RenBatchList.Add(v1);
                }
                Profiler.EndSample("Batching");
                return RenBatchList;
            }
            //Console.Title = $"DrawCalls: {DrawCall}, total:{renderObjects}";
        }
        public enum RenderPass { 
            main,
            depth
        }
    }
}
