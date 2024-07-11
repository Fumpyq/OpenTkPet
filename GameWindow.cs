using ConsoleApp1_Pet.Materials;
using ConsoleApp1_Pet.Meshes;
using ConsoleApp1_Pet.Render;
using ConsoleApp1_Pet.Shaders;
using ConsoleApp1_Pet.Textures;
using Dear_ImGui_Sample;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static OpenTK.Graphics.OpenGL.GL;
//using ImGuiNET;
//using static ImGuiNET.ImGuiNative;

namespace ConsoleApp1_Pet
{
    public class Game : GameWindow
    {
        //private ImGuiRenderer _renderer;
        //private ImGuiController _controller;

        //private static ImGui.SetWindow _controller;
        float[] vertices =
        {
    //Position          Texture coordinates
     0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // top right
     0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
    -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
    -0.5f,  0.5f, 0.0f, 0.0f, 1.0f  // top left
};
        uint[] indices = {  // note that we start from 0!
    0, 1, 3,   // first triangle
    1, 2, 3    // second triangle
};
        public static Game instance;
        public ImGuiController _controller;
        public Game(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title }) { instance = this; }

        //Shader3d shader;
        Texture texture;
        public Renderer renderer;
        int VertexArrayObject;
        int VertexBufferObject;
        int ElementBufferObject;
        float speed = 8f;
        float dt = 0;

        //Vector3 position = new Vector3(0.0f, 0.0f, -3.0f);
        //Vector3 front = new Vector3(0.0f, 0.0f, 1.0f);
        //Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);
        public Camera mainCamera;
        public List<Camera> allCameras = new List<Camera>();
        private int _cameraIndex;
        public DirectLight light;
        public TextureMaterial ImageDisplayMat;
        public bool ShowDebugTexture;
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            KeyboardState input = KeyboardState;

            //...
            var speed = this.speed * dt;
            if (input.IsKeyDown(Keys.W))
            {
                mainCamera.transform.position += mainCamera.transform.Forward * speed; //Forward 
            }
            if (input.IsKeyPressed(Keys.C))
            {
                _cameraIndex += 1;
                if (_cameraIndex >= allCameras.Count) _cameraIndex = 0;
               mainCamera = allCameras[_cameraIndex];
            }
            if (input.IsKeyPressed(Keys.T))
            {
                ShowDebugTexture = !ShowDebugTexture;
            }
            if (input.IsKeyDown(Keys.S))
            {
                mainCamera.transform.position -= mainCamera.transform.Forward * speed;//Backwards

            }

            if (input.IsKeyDown(Keys.A))
            {
                mainCamera.transform.position -= mainCamera.transform.Right * speed; //Left
            }

            if (input.IsKeyDown(Keys.D))
            {
                mainCamera.transform.position += mainCamera.transform.Right * speed; //Right
            }

            if (input.IsKeyDown(Keys.Space))
            {
                mainCamera.transform.position += Vector3.UnitY * speed; //Up 
            }

            if (input.IsKeyDown(Keys.LeftShift))
            {
                mainCamera.transform.position -= Vector3.UnitY * speed; //Down
            }
            
            if (input.IsAnyKeyDown)
            {
                mainCamera.transform.Invalidate();
               //view = Matrix4.LookAt(position, position + front, up);

                viewProjection = view* projection;
            }
            if (KeyboardState.IsKeyDown(Keys.Escape))
            {

                //Test code
                

    

                Close();
            }
        }
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

            // Tell ImGui of the new size
            _controller.WindowResized(ClientSize.X, ClientSize.Y);
        }
        protected override void OnLoad()
        {
            base.OnLoad();
            this.Context.MakeCurrent();
            FullScreenSquad.Initialize();
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Blend);
            // ImGui.CreateContext();
            //ImGui.SetCurrentContext(this.Context.WindowPtr);
            _controller = new ImGuiController(ClientSize.X, ClientSize.Y);
            mainCamera = new Camera(new Vector3(0, 0, -3), new Vector3(1, 0, 0), 45);
            mainCamera.name = "MainCamera";
            renderer = new Renderer();
            light = new DirectLight(new Vector3(0, 12, 13), Vector3.Zero);
            light.transform.parent = mainCamera.transform;
           // ShaderManager.CompileShader(@"DepthTextureDisplay_vert.glsl",@"DepthTextureDisplay_frag.glsl");
        var s2d= ShaderManager.CompileShader(@"Shaders\Code\DepthTextureDisplay_vert.glsl", @"Shaders\Code\DepthTextureDisplay_frag.glsl");
           // var sd = new Shader_Old();
         //   sd.Id = s2d.Id;
            //var s2d = new OnScreenTextureShader();
            //s2d.Compile();
            ImageDisplayMat = new TextureMaterial(s2d, light.depthBuffer.texture);
            light.transform.Forward = -light.transform.position;

            GL.Enable(EnableCap.DepthTest);

            //GL.CullFace(CullFaceMode.Front);
            //view = Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f);
            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f),this.Size.X  /(float) this.Size.Y, 0.1f, 100.0f);
            view = mainCamera.ViewMatrix;
           // shader = new Shader3d();
           // shader.Compile();
           var shd = ShaderManager.CompileShader(@"Shaders\Code\Basic3d_vert.glsl", @"Shaders\Code\SimpleTexture_frag.glsl");
            texture = new Texture("");
            VertexBufferObject = GL.GenBuffer();

            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);

            ElementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);


            var mesh = Cube.Generate();
            var mat = new TextureMaterial(shd, texture);

            rr = new RenderObject(mesh, mat);



            renderer.AddToRender(rr);
            var N = 45;
            float[] arrr = new float[N * N * N];
            var MM =fn2.GenUniformGrid3D(arrr,0,0,0, N, N, N,0.002f,132);
            var middle = (MM.min  - MM.max) / 2;
            int ll = arrr.Length;
            var N2 = N * N;
            for (int i = ll - 1; i > 0; i--)
            {
                if (IsBlock(i))
                {
                    if (!IsEnclosed(i))
                    {
                        rr = new RenderObject(mesh, mat);
                        int x = i % N;
                        int y = (i / N) % N;
                        int z = i / (N * N);
                        rr.transform.position = new Vector3(x+N/2, y + N / 2, z + N / 2);
                        renderer.AddToRender(rr);
                    }
                }
            }

            //rr.transform.parent = mainCamera.transform;

            bool IsBlock(int ind)
            {
                
               return arrr[ind] >= middle;
            }
            
            bool IsEnclosed(int ind)
            {
                if(!((ind-1)>0 && IsBlock(ind - 1))) return false;   
                if (!((ind + 1) < arrr.Length && IsBlock(ind + 1)))return false;

                if (!((ind - N) > 0 && IsBlock(ind - N))) return false;
                if (!((ind + N) < arrr.Length && IsBlock(ind + N))) return false;

                if (!((ind - N2) > 0 && IsBlock(ind - N2))) return false;
                if (!((ind + N2) < arrr.Length && IsBlock(ind + N2))) return false;

                return true;
            }
            //Code goes here
        }
        List<RenderObject> TestRender = new List<RenderObject>();
        List<RenderObject> DrawThisFrame = new List<RenderObject>();
        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);


            _controller.PressChar((char)e.Unicode);
        }
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            _controller.MouseScroll(e.Offset);
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            _stopwatch.Start();
            _controller.Update(this, (float)e.Time);
            ShaderManager.OnFrameStart();
            // DrawThisFrame = TestRender.Where(x=> (Vector3.Dot(front, x.transform.position - position) >= 0) && FrustumCalling.IsSphereInside(x.transform.position, 0.5f)).ToList();
            //var tt= Parallel.ForEachAsync(
            //TestRender!,
            //cancellationToken: default,
            //(rr, ct) =>
            //{
            //    if (!(Vector3.Dot(front, rr.transform.position - position) < 0))
            //        if (FrustumCalling.IsSphereInside(rr.transform.position, 0.5f))
            //            DrawThisFrame.Add(rr);
            //    return ValueTask.CompletedTask;
            //});


            //light.transform.position

            var start = _stopwatch.Elapsed;
            _frameCount++;

            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
#if false
            // Matrix4 model =Matrix4.Identity* Matrix4.CreateRotationX(MathHelper.DegreesToRadians(-25.0f));
            //model = model * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(-25.0f));
            texture.Use();
            shader.Use();
            GL.Uniform1(0, texture.id);
            var model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(_stopwatch.Elapsed.TotalSeconds * 35));
            model *= Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(_stopwatch.Elapsed.TotalSeconds * 25));
            //shader.SetMatrix("model", model);
            //shader.SetMatrix("view", view);
            //shader.SetMatrix("projection", projection);
            //shader.SetMatrix("viewProjection", viewProjection); 
            viewProjection = view * projection;
            // viewProjection.Transpose();
            //var m1 = model * view * projection;
            //var m2 = model * viewProjection;
            view = Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f);
            view = mainCamera.ViewMatrix;

            FrustumCalling.Initialize(viewProjection);
            FrustumCalling.Initialize(mainCamera);

            var transform = model * view * projection;

            shader.SetMatrix(0, model);
            shader.SetMatrix(1, view);
            shader.SetMatrix(2, projection);
            shader.SetMatrix(3, viewProjection);
            shader.SetMatrix(4, transform);
            //shader.SetMatrix(1, transform);
            //shader.SetMatrix(2, transform);
            //shader.SetMatrix(3, transform);
            //shader.SetMatrix(4, transform);
            // GL.BindVertexArray(VertexArrayObject);
            //uniform mat4 model;
            //uniform mat4 view;
            //uniform mat4 projection;
            //uniform mat4 viewProjection;


            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
            rr.transform.rotation = Quaternion.FromEulerAngles((float)_stopwatch.Elapsed.TotalSeconds * 3, (float)_stopwatch.Elapsed.TotalSeconds * 2, (float)_stopwatch.Elapsed.TotalSeconds);
            rr.transform.scale = Vector3.One * (MathF.Sin((float)_stopwatch.Elapsed.TotalSeconds));
            rr.transform.position = Vector3.One * (MathF.Sin((float)_stopwatch.Elapsed.TotalSeconds) * MathF.Cos((float)_stopwatch.Elapsed.TotalSeconds));
            rr.transform.Invalidate();
            var viewProj = mainCamera.ViewProjectionMatrix;
            // var MinVec3 = 
            //FrustumCalling.Initialize(viewProj);
            //int DrawCall = 0;
            // foreach (var rr in TestRender) {
            ////tt.GetAwaiter().GetResult();
            ////foreach (var rr in DrawThisFrame) {
            //    //if (Vector3.Dot(front, rr.transform.position - position) < 0) continue;
            //    //if (!FrustumCalling.IsSphereInside(rr.transform.position,0.5f)) continue;
            //    if (rr.material != materialInUse)
            //    {
            //        materialInUse = rr.material;
            //        rr.material.Use();
            //        //var mtrx = transform * view * project;
            //        // var model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(Game.instance._stopwatch.Elapsed.TotalSeconds * 35));
            //        // model *= Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(Game.instance._stopwatch.Elapsed.TotalSeconds * 25));

            //        rr.material.shader.SetMatrix(1, view);
            //        rr.material.shader.SetMatrix(2, projection);
            //        rr.material.shader.SetMatrix(3, viewProj);
            //        //material.shader.SetMatrix(3, mtrx);

            //    }
            //    if (meshInUse != rr.mesh)
            //    {
            //        meshInUse = rr.mesh;
            //        meshInUse.FillBuffers();
            //        GL.BindVertexArray(meshInUse.VAO);

            //    }
            //    materialInUse.shader.SetMatrix(0, rr.transform);

            //    GL.DrawElements(PrimitiveType.Triangles, meshInUse.triangles.Length, DrawElementsType.UnsignedInt, 0);
            //    DrawCall++;
            //   // GL.BindVertexArray(0);
            //    //rr.DirectDraw(view, projection);
            //}
            //Console.Title = $"DrawCalls: {DrawCall}, total:{TestRender.Count}";
#endif
           // if (ShowDebugTexture) 
                light.depthBuffer.Use();

            //var res2 = renderer.RenderScene(light.cam, Renderer.RenderPass.depth);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            var res2 = renderer.RenderScene(mainCamera, Renderer.RenderPass.depth);
          //
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            var res = renderer.RenderScene(mainCamera, Renderer.RenderPass.main);

            // ImageDisplayMat.mainColor = texture;
            if (ShowDebugTexture)
                FullScreenSquad.Render(ImageDisplayMat);
           // var res = renderer.RenderScene(mainCamera, Renderer.RenderPass.main);
            //ImGui.ShowDemoWindow();
            //ImGui.NewFrame();

            //// Your ImGui UI code goes here...
            //ImGui.Text("Hello, ImGui!");
            //ImGui.EndFrame();
            //ImGui.DockSpaceOverViewport();

            ImGui.ShowDemoWindow();
            ImGui.Begin("info");
            ImGui.TextWrapped($"cam: {mainCamera.transform.position}");
            //ImGui.TextWrapped($"ren: {res.TotalObjectsRendered}");
            ImGui.End();

            _controller.Render();

            ImGuiController.CheckGLError("End of frame");
            SwapBuffers();
            dt = (float)(_stopwatch.Elapsed- start).TotalSeconds;
            TimeSpan elapsed = _stopwatch.Elapsed - _lastUpdate;
            if (elapsed >= TimeSpan.FromSeconds(1))
            {
                _fps = (float)(_frameCount / elapsed.TotalSeconds);
                _frameCount = 0;
                _lastUpdate = _stopwatch.Elapsed;
                Console.WriteLine($"FPS: {_fps:F2}");
            }

            //SwapBuffers();
        }

        public Stopwatch _stopwatch = new Stopwatch();
        private int _frameCount = 0;
        private float _fps = 0;
        private TimeSpan _lastUpdate = TimeSpan.Zero;
        private Matrix4 view;
        private Matrix4 projection;
        private Matrix4 viewProjection;
        private RenderObject rr;
        private FastNoise2 fn2 = FastNoise2.FromEncodedNodeTree("GgABEQACAAAAAADgQBAAAACIQR8AFgABAAAACwADAAAAAgAAAAMAAAAEAAAAAAAAAD8BFAD//wAAAAAAAD8AAAAAPwAAAAA/AAAAAD8BFwAAAIC/AACAPz0KF0BSuB5AEwAAAKBABgAAj8J1PACamZk+AAAAAAAA4XoUPw==");


        public Material materialInUse;
        public Mesh meshInUse;

    }

}
