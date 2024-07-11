using ConsoleApp1_Pet.Materials;
using ConsoleApp1_Pet.Meshes;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
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

                    rr.material.shader.SetMatrix(1, view);
                    rr.material.shader.SetMatrix(2, projection);
                    rr.material.shader.SetMatrix(3, viewProj);


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
