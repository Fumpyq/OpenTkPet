using ConsoleApp1_Pet.Materials;
using ConsoleApp1_Pet.Meshes;
using ConsoleApp1_Pet.Shaders;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Render
{
    public class Renderer
    {
        public List<RenderComponent> renderObjects = new List<RenderComponent>();
        public Material materialInUse;
        public Mesh meshInUse;
        public static bool useFrustumCalling;

        public void OnFrameStart()
        {

        }
        public void OnFrameEnd() { FrostumCullingCash.Clear();SceneDrawTemp.Clear(); }
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
        public RenderPassResult RenderScene(RenderSceneCommand cmd)
        {
            Camera cam = cmd.cam;
            Matrix4 InvCamera = cam.ViewProjectionMatrix;
            InvCamera.Invert();
            if (cmd.pass == RenderPass.depth)
            {
               // GL.ColorMask(false, false, false, false);
            }
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
                    foreach (var rr in toRender)
                    {
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
                    }
                }
                else
                {
                    foreach (var rr in renderObjects)
                    {


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
                    FrostumCullingCash.Add(cam, new List<RenderComponent>(SceneDrawTemp));
                    SceneDrawTemp.Clear();
                }
            }
            else
            {
                foreach (var rr in toRender)
                {
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
            ImGui.Text($"{cmd.name}: T: {renderObjects.Count} , DC: {DrawCall}");
            materialInUse = null;
            meshInUse = null;
            if (cmd.pass == RenderPass.depth)
            {
               // GL.ColorMask(true, true, true, true);
            }
            return res;
            //Console.Title = $"DrawCalls: {DrawCall}, total:{renderObjects}";
        }
        public enum RenderPass { 
            main,
            depth
        }
    }
}
