using BepuUtilities.TaskScheduling;
using ConsoleApp1_Pet.Materials;
using ConsoleApp1_Pet.Meshes;
using ConsoleApp1_Pet.Shaders;
using ConsoleApp1_Pet.Новая_папка;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        Dictionary<Camera, List<RenderComponent>> FrostumCullingCash = new Dictionary<Camera, List<RenderComponent>>(8);
        List<RenderComponent> SceneDrawTemp = new List<RenderComponent>(2500);

        private ConcurrentQueue<RenderComponent> ParallelFrustumCalling = new ConcurrentQueue<RenderComponent> ();
        public int RenderSceneCommands;
        public int TotalRenderObjectProceded;
        public struct RenderSceneCommand
        {
            public string name;
            public Camera cam;
            public RenderPass pass;

            public RenderSceneCommand(string name, Camera cam, RenderPass pass)
            {
                this.name = name;
                this.cam = cam;
                this.pass = pass;
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
            FrustumCalling.Initialize(cam.ViewProjectionMatrix);
            var view = cam.ViewMatrix;
            var projection = cam.ProjectionMatrix;
            var viewProj = cam.ViewProjectionMatrix;var res = new RenderPassResult();
            var toRender = renderObjects;
            if (useFrustumCalling)
            {
                if (FrostumCullingCash.TryGetValue(cam, out toRender))
                {
                    var toRenderSpan = CollectionsMarshal.AsSpan(toRender);
                    for (int i = toRenderSpan.Length - 1; i >= 0; i--)
                    {
                        var rr = toRenderSpan[i];
                        Profiler.BeginSample("DrawCall");
                        res.TotalObjectsRendered++;
                        if (rr.material != materialInUse)
                        {
                            materialInUse = rr.material;
                            rr.material.Use();

                            rr.material.shader.SetUniform("view", view);
                            rr.material.shader.SetUniform("projection", projection);
                            rr.material.shader.SetUniform("viewProjection", viewProj);
                            rr.material.shader.SetUniform("mainCameraVP", cam);
                            rr.material.shader.SetUniform("invMainCameraVP", InvCamera);
                            rr.material.shader.SetTexture(Shader.CameraDepth, Game.instance.depthBuffer);

                        }
                        if (meshInUse != rr.mesh)
                        {
                            meshInUse = rr.mesh;
                            meshInUse.FillBuffers();
                            GL.BindVertexArray(meshInUse.VAO);

                        }
                        materialInUse.shader.SetMatrix(0, rr.transform);
                        //GL.MultiDrawElements

                        GL.DrawElements(PrimitiveType.Triangles, meshInUse.triangles.Length, DrawElementsType.UnsignedInt, 0);
                        DrawCall++;
                        Profiler.EndSample("DrawCall");
                    }
                }
                else
                {
                    if (renderObjects.Count > 5000)
                    {
                        //Profiler.BeginSample("Occlusion");

                        // Task.Run(() =>
                        //{
#if selfHandled || false
                           List<Task> ts = new List<Task>();
                           ;foreach (var v in renderObjects.Chunk(1024))
                           {
                               Task tsk = Task.Run(() =>
                               {
                                   for (int i = v.Length - 1; i >= 0; i--)
                                   {
                                       var rr = v[i];
                                       if (FrustumCalling.IsSphereInside(rr.transform.position, 0.87f))
                                       {
                                           ParallelFrustumCalling.Enqueue(rr);

                                       }
                                   }
                               });
                               ts.Add(tsk);
                           }
#endif
#if ParallelFr || true
                        //var toRenderSpan = CollectionsMarshal.AsSpan(renderObjects);
                        Parallel.ForEach<RenderComponent>(renderObjects, new ParallelOptions() {MaxDegreeOfParallelism=4 }, item =>
                           {
                               // Your calculation goes here
                               if (FrustumCalling.IsSphereInside(item.transform.position, 0.87f))
                               {
                                   ParallelFrustumCalling.Enqueue(item);

                               }
                           });
                          
#endif
#if selfHandled || false
                           async Task job(RenderComponent[] data)
                           {
                               for (int i = data.Length - 1; i >= 0; i--)
                               {
                                   var rr = data[i];
                                   if (FrustumCalling.IsSphereInside(rr.transform.position, 0.87f))
                                   {
                                       ParallelFrustumCalling.Enqueue(rr);

                                   }
                               }
                           }
                           var Tasks = renderObjects.Chunk(512).Select(job).ToArray();
                           Task.WaitAll(Tasks);
#endif
                        // Task.WaitAll(ts.ToArray());
                        // }).GetAwaiter().GetResult();
                        // Profiler.EndSample("Occlusion");
                        while (ParallelFrustumCalling.TryDequeue(out var rr))
                       // var toRenderSpan = CollectionsMarshal.AsSpan(renderObjects);
                       // for (int i = toRenderSpan.Length - 1; i > 0; i--)
                        {
                           // var rr = toRenderSpan[i];

                            if (!FrustumCalling.IsSphereInside(rr.transform.position, 0.87f)) continue;
                            SceneDrawTemp.Add(rr);
                            res.TotalObjectsRendered++;
                            if (rr.material != materialInUse)
                            {
                                materialInUse = rr.material;
                                rr.material.Use();

                                rr.material.shader.SetUniform("view", view);
                                rr.material.shader.SetUniform("projection", projection);
                                rr.material.shader.SetUniform("viewProjection", viewProj);
                                rr.material.shader.SetUniform("mainCameraVP", cam);
                                rr.material.shader.SetUniform("invMainCameraVP", InvCamera);
                                rr.material.shader.SetTexture(Shader.CameraDepth, Game.instance.depthBuffer);

                            }
                            if (meshInUse != rr.mesh)
                            {
                                meshInUse = rr.mesh;
                                meshInUse.FillBuffers();
                                GL.BindVertexArray(meshInUse.VAO);

                            }
                            materialInUse.shader.SetMatrix(0, rr.transform);

                            GL.DrawElements(PrimitiveType.Triangles, meshInUse.triangles.Length, DrawElementsType.UnsignedInt, 0);
                            DrawCall++;
                        }
                    }
                    else
                    {
                        var toRenderSpan = CollectionsMarshal.AsSpan(renderObjects);
                        for (int i = toRenderSpan.Length - 1; i >= 0; i--)
                        {
                            var rr = toRenderSpan[i];


                            if (!FrustumCalling.IsSphereInside(rr.transform.position, 0.87f)) continue;
                            SceneDrawTemp.Add(rr);
                            res.TotalObjectsRendered++;
                            if (rr.material != materialInUse)
                            {
                                materialInUse = rr.material;
                                rr.material.Use();

                                rr.material.shader.SetUniform("view", view);
                                rr.material.shader.SetUniform("projection", projection);
                                rr.material.shader.SetUniform("viewProjection", viewProj);
                                rr.material.shader.SetUniform("mainCameraVP", cam);
                                rr.material.shader.SetUniform("invMainCameraVP", InvCamera);
                                rr.material.shader.SetTexture(Shader.CameraDepth, Game.instance.depthBuffer);

                            }
                            if (meshInUse != rr.mesh)
                            {
                                meshInUse = rr.mesh;
                                meshInUse.FillBuffers();
                                GL.BindVertexArray(meshInUse.VAO);

                            }
                            materialInUse.shader.SetMatrix(0, rr.transform);

                            GL.DrawElements(PrimitiveType.Triangles, meshInUse.triangles.Length, DrawElementsType.UnsignedInt, 0);
                            DrawCall++;
                        }
                    }
                    FrostumCullingCash.Add(cam, new List<RenderComponent>(SceneDrawTemp));
                    SceneDrawTemp.Clear();
                }
            }
            else
            {
                var toRenderSpan = CollectionsMarshal.AsSpan(toRender);
                for (int i = toRenderSpan.Length - 1; i >= 0; i--)
                {
                    var rr = toRenderSpan[i];
                    Profiler.BeginSample("DrawCall");
                    res.TotalObjectsRendered++;
                    if (rr.material != materialInUse)
                    {
                        materialInUse = rr.material;
                        rr.material.Use();

                        rr.material.shader.SetUniform("view", view);
                        rr.material.shader.SetUniform("projection", projection);
                        rr.material.shader.SetUniform("viewProjection", viewProj);
                        rr.material.shader.SetUniform("mainCameraVP", cam);
                        rr.material.shader.SetUniform("invMainCameraVP", InvCamera);
                        rr.material.shader.SetTexture(Shader.CameraDepth, Game.instance.depthBuffer);

                    }
                    if (meshInUse != rr.mesh)
                    {
                        meshInUse = rr.mesh;
                        meshInUse.FillBuffers();
                        GL.BindVertexArray(meshInUse.VAO);

                    }
                    materialInUse.shader.SetMatrix(0, rr.transform);

                    GL.DrawElements(PrimitiveType.Triangles, meshInUse.triangles.Length, DrawElementsType.UnsignedInt, 0);
                    DrawCall++;
                    Profiler.EndSample("DrawCall");
                }
            }
            ImGui.Text($"{cmd.name}: T: {renderObjects.Count} , DC: {DrawCall}");
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
}
