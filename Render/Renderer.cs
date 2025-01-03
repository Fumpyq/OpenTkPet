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
using Vector3 = OpenTK.Mathematics.Vector3;

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

        private const float CallingSphereRadiusDefaultValue = 15.87f;
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
        }
        Matrix4[] instancedDrawing = new Matrix4[10000];
        public RenderPassResult RenderScene(RenderSceneCommand cmd)
        {     
            
           
        RenderSceneCommands++;
            TotalRenderObjectProceded += renderObjects.Count;
            Profiler.BeginSample("Render Pass");
            Camera cam = cmd.cam;
            Matrix4 InvCamera = cam.ViewProjectionMatrix;
            InvCamera.Invert();
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
            var viewProj = cam.ViewProjectionMatrix;var res = new RenderPassResult();
            var toRender = renderObjects;


            void Render(List<RenderBatch> RenBatchList)
            {
                foreach (var rb in RenBatchList)
                {
                    // var rb = MaterialsSpan[i];
                    Profiler.BeginSample("DrawBatch");



                    res.TotalObjectsRendered += 1;
                    if (rb.material != materialInUse)
                    {
                        materialInUse = rb.material;
                        rb.material.Use();

                        rb.material.shader.SetUniform("view", view);
                        rb.material.shader.SetUniform("projection", projection);
                        rb.material.shader.SetUniform("viewProjection", viewProj);
                        rb.material.shader.SetUniform("mainCameraVP", cam);
                        rb.material.shader.SetUniform("invMainCameraVP", InvCamera);
                        rb.material.shader.SetTexture(Shader.CameraDepth, MainGameWindow.instance.depthBuffer);

                    }
                    foreach (var mb in rb.meshBatches)
                    {

                        if (meshInUse != mb.mesh)
                        {
                            meshInUse = mb.mesh;
                            var span = CollectionsMarshal.AsSpan(mb.matrices);
                            meshInUse.FillBuffers(span);
                            GL.BindVertexArray(meshInUse.VAO);

                        }
                        Profiler.BeginSample("DrawCall");
                        try
                        {
                            materialInUse.shader.SetMatrix(0, mb.matrices[0]);
                            GL.DrawElementsInstanced(PrimitiveType.Triangles, meshInUse.triangles.Length, DrawElementsType.UnsignedInt, 0, mb.matrices.Count);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                        Profiler.EndSample("DrawCall");
                        //foreach (var mt in mb.matrices)
                        //{

                        //   materialInUse.shader.SetMatrix(0, mt);
                        //    //GL.DrawElements

                        //    //GL.MultiDrawElements

                        //    GL.DrawElements(PrimitiveType.Triangles, meshInUse.triangles.Length, DrawElementsType.UnsignedInt, 0);
                        //    DrawCall++;
                        //    VertCount += meshInUse.vertices.LongLength;

                        //}
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
                    ConcurrentQueue<RenderComponent> ParallelFrustumCalling = new ConcurrentQueue<RenderComponent>();
                    Parallel.ForEach<RenderComponent>(renderObjects, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, item =>
                    {
                        // Your calculation goes here
                        if (FrustumCalling.IsSphereInside(item.transform.position, CallingSphereRadiusDefaultValue))
                        {
                            ParallelFrustumCalling.Enqueue(item);

                        }
                    });

                    ///ParallelFrustumCalling.GroupBy(x=)

                    Dictionary<Material,RenderBatch> materialMap = new Dictionary<Material,RenderBatch>();
                    Dictionary<Mesh,MeshRenderBatch> meshMap = new Dictionary<Mesh, MeshRenderBatch>();
                    RenBatchList = new List<RenderBatch>(2);
                    while (ParallelFrustumCalling.TryDequeue(out var rc))
                    {
                        if(materialMap.TryGetValue(rc.material,out var batch))
                        {
                            if (meshMap.TryGetValue(rc.mesh,out var mbatch))
                            {
                                mbatch.matrices.Add(rc.transform);
                            }
                            else
                            {
                                mbatch = new MeshRenderBatch(rc.mesh, rc.transform);
                                batch.meshBatches.Add(mbatch);
                                meshMap.Add(rc.mesh, mbatch);
                            }
                        }
                        else
                        {
                            batch = new RenderBatch(rc.material, rc.mesh, rc.transform);
                            materialMap.Add(rc.material, batch);
                            RenBatchList.Add(batch);
                        }
                    }
                    FrostumCullingCash.Add(cam, RenBatchList);
                    Render(RenBatchList);
                }
            }
            else
            {
                Profiler.BeginSample("Batching");
              //  OpenGLInstancing ggg = new OpenGLInstancing();
              //  ggg.SetupAndDraw(cam);
                Dictionary<long, RenderBatch> materialMap = new Dictionary<long, RenderBatch>();
                Dictionary<int, MeshRenderBatch> meshMap = new Dictionary<int, MeshRenderBatch>();
                var RenBatchList = new List<RenderBatch>(2);
                foreach (var rc in toRender)
                {
                   
                    if (materialMap.TryGetValue(rc.material.id, out var batch))
                    {
                        if (meshMap.TryGetValue(rc.mesh.VAO, out var mbatch))
                        {
                            mbatch.matrices.Add(rc.transform);
                        }
                        else
                        {
                            mbatch = new MeshRenderBatch(rc.mesh, rc.transform);
                            batch.meshBatches.Add(mbatch);
                            meshMap.Add(rc.mesh.VAO, mbatch);
                        }
                    }
                    else
                    {
                        batch = new RenderBatch(rc.material, rc.mesh, rc.transform);
                        materialMap.Add(rc.material.id, batch);
                        RenBatchList.Add(batch);
                    }
                }
                Profiler.EndSample("Batching");
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
            //Console.Title = $"DrawCalls: {DrawCall}, total:{renderObjects}";
        }
        public enum RenderPass { 
            main,
            depth
        }
    }


    public class OpenGLInstancing
    {
        private int _vertexArrayObject;
        private int _vertexBufferObject;
        private int _instanceBufferObject;
        private int _shaderProgram;

        private const int InstanceCount = 100;
        private List<Matrix4> _instanceMatrices;

        public unsafe void SetupAndDraw(Camera cam)
        {
            // ------------------ Initialization (Typically in GameWindow's OnLoad) ------------------

            // --- Generate OpenGL Object Ids
            _vertexArrayObject = GL.GenVertexArray();
            _vertexBufferObject = GL.GenBuffer();
            _instanceBufferObject = GL.GenBuffer();

            // --- Define cube vertices data
            float[] cubeVertices = {

        };

            // --- Send the vertices data to GPU
            GL.BindVertexArray(_vertexArrayObject); // bind Vertex Array Object
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject); // bind Vertex Buffer Object
            GL.BufferData(BufferTarget.ArrayBuffer, cubeVertices.Length * sizeof(float), cubeVertices, BufferUsageHint.StaticDraw); // Copy vertex data

            // --- Setup Vertex Attributes
            GL.EnableVertexAttribArray(0); // position attribute
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0); // Vertex position pointer
            GL.EnableVertexAttribArray(1); // TexCoord attribute
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float)); // TexCoord position pointer

            // --- Setup instance matrices.
            _instanceMatrices = new List<Matrix4>();

            var s = Unsafe.SizeOf<Matrix4>();


            // -- Send instance matrix data to GPU
            Span<Matrix4> matrixSpan = CollectionsMarshal.AsSpan(_instanceMatrices);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _instanceBufferObject);
            fixed (Matrix4* bp = &MemoryMarshal.GetReference(matrixSpan))
            {
                GL.BufferData(BufferTarget.ArrayBuffer, matrixSpan.Length * s, (nint)bp, BufferUsageHint.DynamicDraw);
            }
            // Setup the instance matrix attributes
            int matrixSize = s;
            for (int i = 0; i < 4; i++)
            {
                GL.EnableVertexAttribArray(2 + i);
                GL.VertexAttribPointer(2 + i, 4, VertexAttribPointerType.Float, false, matrixSize, i * 16);
                GL.VertexAttribDivisor(2 + i, 1);
            }

            // --- Unbind all ---
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // ------------------ Rendering (Typically in GameWindow's OnRenderFrame) ------------------
            // --- Bind VAO
            GL.BindVertexArray(_vertexArrayObject);

            // --- Draw Instances
            GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 36, InstanceCount);
        }

        public void CleanUp()
        {
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteBuffer(_instanceBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);
            GL.DeleteProgram(_shaderProgram);
        }
    }

}
