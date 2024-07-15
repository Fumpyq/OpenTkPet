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
        public List<RenderObject> renderObjects = new List<RenderObject>();
        public Material materialInUse;
        public Mesh meshInUse;
        public static bool useFrustumCalling;

        public void OnFrameStart()
        {

        }
        public void OnFrameEnd() { }
        public void AddToRender(RenderObject rr)
        {
            renderObjects.Add(rr);
        }
        public struct RenderPassResult
        {
            public int TotalObjectsRendered;
        }
        public RenderPassResult RenderScene(Camera cam, RenderPass pass)
        {
            Matrix4 InvCamera = cam.ViewProjectionMatrix;
            InvCamera.Invert();
            if (pass == RenderPass.depth)
            {
               // GL.ColorMask(false, false, false, false);
            }
            int DrawCall = 0;
            FrustumCalling.Initialize(cam.ViewProjectionMatrix);
            var view = cam.ViewMatrix;
            var projection = cam.ProjectionMatrix;
            var viewProj = cam.ViewProjectionMatrix;var res = new RenderPassResult();
            foreach (var rr in renderObjects)
            {
                if (useFrustumCalling && !FrustumCalling.IsSphereInside(rr.transform.position, 0.87f)) continue;
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
                    rr.material.shader.SetTexture(Shader.CameraDepth,Game.instance.depthBuffer);

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
            ImGui.BulletText($"Total: {renderObjects.Count} , DrawCalls: {DrawCall}");
            materialInUse = null;
            meshInUse = null;
            if (pass == RenderPass.depth)
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
