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

namespace ConsoleApp1_Pet.Render
{
    public class Renderer
    {
        private const float CullingRadius = 3.87f;
        private static readonly Stack<List<Matrix4>> MatrixPool = new();
        private static readonly Dictionary<Material, Dictionary<Mesh, List<Matrix4>>> BatchCache = new(64, ReferenceEqualityComparer.Instance);

        private readonly List<RenderComponent> renderObjects = new();
        private readonly Dictionary<Camera, Dictionary<Material, Dictionary<Mesh, List<Matrix4>>>> frustumCache = new(8);

        private Material currentMaterial;
        private Mesh currentMesh;
        public static bool useFrustumCulling;
        public void AddToRender(RenderComponent component) => renderObjects.Add(component);
        public void OnFrameStart() { }
        public void OnFrameEnd() => FrameCleanup();

        public RenderPassResult RenderScene(RenderSceneCommand cmd)
        {
            var result = new RenderPassResult();
            var cam = cmd.cam;

            Profiler.BeginSample("Render Pass");
            FrustumCulling.Initialize(cam.ViewProjectionMatrix);

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
                var visibleObjects = new List<RenderComponent>();
                foreach (var obj in renderObjects)
                {
                    if (FrustumCulling.IsSphereInside(obj.transform.position, CullingRadius))
                        visibleObjects.Add(obj);
                }
                batches = BatchObjects(visibleObjects);
                if (useFrustumCulling) frustumCache[cam] = batches;
                Profiler.EndSample("Frustum Culling");

                RenderBatches(batches, view, projection, viewProj, invViewProj, cam, ref result);
            }

            ImGui.Text($"{cmd.name}: Objects: {renderObjects.Count}, DrawCalls: {result.DrawCalls}, Verts: {result.VerticesDrawn}");
            currentMaterial = null;
            currentMesh = null;

            Profiler.EndSample("Render Pass");
            return result;
        }

        private void RenderBatches(
            Dictionary<Material, Dictionary<Mesh, List<Matrix4>>> batches,
            Matrix4 view, Matrix4 projection, Matrix4 viewProj, Matrix4 invViewProj, Camera cam,
            ref RenderPassResult result)
        {
            foreach (var materialBatch in batches)
            {
                var material = materialBatch.Key;
                if (currentMaterial != material)
                {
                    currentMaterial = material;
                    material.Use();
                    material.shader.SetUniform("view".GetHashCode(), view);
                    material.shader.SetUniform("projection".GetHashCode(), projection);
                    material.shader.SetUniform("viewProjection".GetHashCode(), viewProj);
                    material.shader.SetUniform("mainCameraVP".GetHashCode(), cam);
                    material.shader.SetUniform("invMainCameraVP".GetHashCode(), invViewProj);
                    material.shader.SetTexture(Shader.CameraDepth, MainGameWindow.instance.depthBuffer);
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
                        material.shader.SetMatrix(0, matrix);
                        GL.DrawElements(PrimitiveType.Triangles, mesh.triangles.Length, DrawElementsType.UnsignedInt, 0);
                        result.DrawCalls++;
                        result.VerticesDrawn += mesh.vertices.Length;
                        result.TotalObjectsRendered++;
                    }
                }
            }
        }

        private static Dictionary<Material, Dictionary<Mesh, List<Matrix4>>> BatchObjects(List<RenderComponent> objects)
        {
            Profiler.BeginSample("Batching");

            foreach (var obj in CollectionsMarshal.AsSpan(objects))
            {
                ref var materialDict = ref CollectionsMarshal.GetValueRefOrAddDefault(BatchCache, obj.material, out var materialExists);
                if (!materialExists) materialDict = new Dictionary<Mesh, List<Matrix4>>(4, ReferenceEqualityComparer.Instance);

                ref var matrixList = ref CollectionsMarshal.GetValueRefOrAddDefault(materialDict, obj.mesh, out var meshExists);
                if (!meshExists) matrixList = MatrixPool.TryPop(out var list) ? list : new List<Matrix4>(64);

                matrixList.Add(obj.transform);
            }

            Profiler.EndSample("Batching");
            return BatchCache;
        }

        public void FrameCleanup()
        {
            Profiler.BeginSample("Frame Cleanup");

            frustumCache.Clear();

            foreach (var materialEntry in BatchCache.Values)
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

            Profiler.EndSample("Frame Cleanup");
        }

        public struct RenderPassResult
        {
            public int TotalObjectsRendered;
            public int DrawCalls;
            public long VerticesDrawn;
        }

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