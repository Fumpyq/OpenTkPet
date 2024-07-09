using ConsoleApp1_Pet.Materials;
using ConsoleApp1_Pet.Meshes;
using ConsoleApp1_Pet.Render;
using ConsoleApp1_Pet.Shaders;
using ConsoleApp1_Pet.Textures;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using System;
using System.Collections.Generic;
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

        public Game(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title }) { instance = this; }

        Shader3d shader;
        Texture texture;
        int VertexArrayObject;
        int VertexBufferObject;
        int ElementBufferObject;
        float speed = 8f;
        float dt = 0;

        Vector3 position = new Vector3(0.0f, 0.0f, -3.0f);
        Vector3 front = new Vector3(0.0f, 0.0f, 1.0f);
        Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            KeyboardState input = KeyboardState;

            //...
            var speed = this.speed * dt;
            if (input.IsKeyDown(Keys.W))
            {
                position += front * speed; //Forward 
            }

            if (input.IsKeyDown(Keys.S))
            {
                position -= front * speed; //Backwards
            }

            if (input.IsKeyDown(Keys.A))
            {
                position -= Vector3.Normalize(Vector3.Cross(front, up)) * speed; //Left
            }

            if (input.IsKeyDown(Keys.D))
            {
                position += Vector3.Normalize(Vector3.Cross(front, up)) * speed; //Right
            }

            if (input.IsKeyDown(Keys.Space))
            {
                position += up * speed; //Up 
            }

            if (input.IsKeyDown(Keys.LeftShift))
            {
                position -= up * speed; //Down
            }
            
            if (input.IsAnyKeyDown)
            {
                view = Matrix4.LookAt(position, position + front, up);

                viewProjection = view* projection;
            }
            if (KeyboardState.IsKeyDown(Keys.Escape))
            {

                //Test code
                

    

                Close();
            }
        }
        protected override void OnLoad()
        {
            base.OnLoad();
            this.Context.MakeCurrent();

           // ImGui.CreateContext();
            ImGui.SetCurrentContext(this.Context.WindowPtr);
            
            
            GL.Enable(EnableCap.DepthTest);
            //GL.CullFace(CullFaceMode.Front);
            //view = Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f);
            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f),this.Size.X  /(float) this.Size.Y, 0.1f, 100.0f);
            view = Matrix4.LookAt(position, position + front, up);
            shader = new Shader3d();
            shader.Compile();
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
            var mat = new TexColorMaterial(shader, texture);

            rr = new RenderObject(mesh, mat);
            TestRender.Add(rr);
            var N = 40;
            float[] arrr = new float[N * N * N];
            var MM =fn2.GenUniformGrid3D(arrr,0,0,0, N, N, N,0.012f,152);
            var middle = (MM.min  - MM.max) / 2;
            int ll = arrr.Length;
            for (int i = ll - 1; i > 0; i--)
            {
                if (arrr[i] >= middle)
                {
                    rr = new RenderObject(mesh, mat);
                    int x = i % N;
                    int y = (i / N) % N;
                    int z = i / (N*N);
                    rr.transform.position = new Vector3(x,y,z);
                    TestRender.Add(rr);
                }
            }
            //Code goes here
        }
        List<RenderObject> TestRender = new List<RenderObject>();
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            _stopwatch.Start();
            var start = _stopwatch.Elapsed;
            _frameCount++;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

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
            var viewProj = view * projection;
            FrustumCalling.Initialize(viewProj);
            foreach (var rr in TestRender) {
                if (Vector3.Dot(front, rr.transform.position - position) < 0) continue;
                if (FrustumCalling.IsSphereInside(rr.transform.position,0.26f)) continue;
                if (rr.material != materialInUse)
                {
                    materialInUse = rr.material;
                    rr.material.Use();
                    //var mtrx = transform * view * project;
                    // var model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(Game.instance._stopwatch.Elapsed.TotalSeconds * 35));
                    // model *= Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(Game.instance._stopwatch.Elapsed.TotalSeconds * 25));
                 
                    rr.material.shader.SetMatrix(1, view);
                    rr.material.shader.SetMatrix(2, projection);
                    rr.material.shader.SetMatrix(3, viewProj);
                    //material.shader.SetMatrix(3, mtrx);

                }
                if (meshInUse != rr.mesh)
                {
                    meshInUse = rr.mesh;
                    meshInUse.FillBuffers();
                    GL.BindVertexArray(meshInUse.VAO);

                }
                materialInUse.shader.SetMatrix(0, rr.transform);
                
                GL.DrawElements(PrimitiveType.Triangles, meshInUse.triangles.Length, DrawElementsType.UnsignedInt, 0);
               // GL.BindVertexArray(0);
                //rr.DirectDraw(view, projection);
            }
            //ImGui.ShowDemoWindow();
            //ImGui.NewFrame();

            //// Your ImGui UI code goes here...
            //ImGui.Text("Hello, ImGui!");
            //ImGui.EndFrame();
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
