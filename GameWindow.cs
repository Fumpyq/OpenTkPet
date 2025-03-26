using BepuPhysics;
using BepuPhysics.Collidables;
using ConsoleApp1_Pet.Architecture;
using ConsoleApp1_Pet.Editor;
using ConsoleApp1_Pet.Materials;
using ConsoleApp1_Pet.Meshes;
using ConsoleApp1_Pet.Physics;
using ConsoleApp1_Pet.Render;
using ConsoleApp1_Pet.Scripts;
using ConsoleApp1_Pet.Scripts.DebugScripts;
using ConsoleApp1_Pet.Shaders;
using ConsoleApp1_Pet.Textures;
using ConsoleApp1_Pet.Новая_папка;
using ConsoleApp1_Pet.Новая_папка.ChunkSystem;
using Dear_ImGui_Sample;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Profiling;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using static ConsoleApp1_Pet.Render.Renderer;
using Mesh = ConsoleApp1_Pet.Meshes.Mesh;
using Random = ConsoleApp1_Pet.Новая_папка.Random;
//using ImGuiNET;
//using static ImGuiNET.ImGuiNative;

namespace ConsoleApp1_Pet
{
    public class MainGameWindow : GameWindow
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
        public static MainGameWindow instance;
        public ImGuiController _controller;
        public MainGameWindow(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title }) { instance = this; }

        //Shader3d shader;
        Texture texture;
        Texture RealTexture;
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
        public ResourceManager resources;
        public List<Camera> allCameras = new List<Camera>();
        private int _cameraIndex;
        public DirectLight light;
        public Material ImageDisplayMat;
        public Material PP_BloomMat;
        public Material SunFlareMat;
        public Material FogMat;
        public bool ShowDebugTexture;
        public FrameBuffer depthBuffer;
        public FrameBuffer prePostProcessingGBuffer;
        public Material sss;
        private bool InitState;
        private bool WasFocused;
        public FrameBuffer OutPutBuffer;
        private Channel<Action> _runOnMainThread;
        public event Action OnBeforeScriptsRun;

        public event Action OnAfterScriptsRun;
        public void RunOnMainThread(Action act)
        {
           while(! _runOnMainThread.Writer.TryWrite(act));
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            KeyboardState input = KeyboardState;

            //...
            var speed = this.speed * dt;
        
            if (this.IsFocused)
            {

                var mouse = MouseState;
                var mouseDeltaX = mouse.X - _lastMousePos.X;
                var mouseDeltaY = mouse.Y - _lastMousePos.Y;
               if (CursorState == CursorState.Grabbed) RotateCamera(mainCamera, mouseDeltaX, mouseDeltaY);
               
                _lastMousePos.X = mouse.X;
                _lastMousePos.Y = mouse.Y;

                //OpenTK.Input.Mouse.SetPosition(x, y);
                // Update last mouse position

            }
            if (MouseState.WasButtonDown(MouseButton.Left) && MouseState.IsButtonReleased(MouseButton.Left))
            {
                SimpleSelfContainedDemo.MakePiu(mainCamera);
            }
            if (input.IsKeyDown(Keys.LeftShift))
            {
                //mainCamera.transform.position -= Vector3.UnitY * speed; //Down
                speed *= 2.25f;
            }
            if (input.IsKeyDown(Keys.LeftControl) || input.IsKeyDown(Keys.RightControl))
            {
                mainCamera.transform.position -= Vector3.UnitY * speed; //Down
                //speed *= 2.25f;
            }
            WasFocused = this.IsFocused;
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
            if (input.IsKeyPressed(Keys.H))
            {
                if (CursorState == CursorState.Normal) { CursorState = CursorState.Grabbed;  }
                else
                {
                    
                    CursorState = CursorState.Normal;
                }
            }
            if (input.IsKeyPressed(Keys.F))
            {
                Physic.IsSimulationEnabled = !Physic.IsSimulationEnabled;
            }
            if (input.IsKeyPressed(Keys.V))
            {
                if (this.VSync == VSyncMode.Off) { this.VSync =VSyncMode.On; }
                else
                {

                    this.VSync = VSyncMode.Off;
                }
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


            
            if (input.IsAnyKeyDown)
            {
                //mainCamera.transform.Invalidate();
               //view = Matrix4.LookAt(position, position + front, up);

                viewProjection = view* projection;
            }
            if (KeyboardState.IsKeyDown(Keys.Escape))
            {

                //Test code
                

    

                Close();
            }
        }

       
        
        float TornadoSpeed;
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
            depthBuffer.Resize(ClientSize.X, ClientSize.Y);
            // Tell ImGui of the new size
            _controller.WindowResized(ClientSize.X, ClientSize.Y);
            mainCamera.Resize(ClientSize.X, ClientSize.Y);
            //light.cam.Resize(ClientSize.X, ClientSize.Y);
            //light.depthBuffer.Resize(ClientSize.X, ClientSize.Y);
            prePostProcessingGBuffer.Resize(ClientSize.X, ClientSize.Y);
            OutPutBuffer.Resize(ClientSize.X, ClientSize.Y);
        }
        protected override void OnLoad()
        {
            base.OnLoad();
            this.Context.MakeCurrent();
            resources = new ResourceManager(AppDomain.CurrentDomain.BaseDirectory);
            _runOnMainThread = Channel.CreateUnbounded<Action>();
            SimpleSelfContainedDemo.Setup();

            FullScreenSquad.Initialize();
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Blend);
            // ImGui.CreateContext();
            //ImGui.SetCurrentContext(this.Context.WindowPtr);
            _controller = new ImGuiController(ClientSize.X, ClientSize.Y);
            mainCamera = new Camera(new Vector3(0, 5, -3), new Vector3(0, 0, 0), 45);
            mainCamera.name = "MainCamera";
            //depthBuffer = new DepthBuffer("MainCameraDepth", ClientSize.X, ClientSize.Y);
            depthBuffer = FrameBufferPresets.CreateShadowOrDepthMap(ClientSize.X, ClientSize.Y);
            //depthBuffer = new FrameBuffer("MainCameraDepth", ClientSize.X, ClientSize.Y);
            //prePostProcessingBuffer = new ScreenBuffer("final prePostProcessing Texture", ClientSize.X, ClientSize.Y);
            prePostProcessingGBuffer = FrameBufferPresets.CreateGBuffer(ClientSize.X, ClientSize.Y);
            renderer = new Renderer();
            light = new DirectLight(new Vector3(-6, -15, 8), Vector3.Zero);

     
            PP_BloomMat = new Material(MainGameWindow.instance.resources.
                CreateShader("PostProcessing_Bloom", @"Shaders\Code\DepthTextureDisplay_vert.glsl", @"Shaders\Code\BloomFrag.glsl"))
                    .SetUniform("bloomThreshold", 0.1f)
                    .SetUniform("bloomIntensity", 1.0f)
                    .SetFrameBufferAttachment("uSceneColor", prePostProcessingGBuffer);

            sss = new Material(MainGameWindow.instance.resources.
                CreateShader("PostProcessing_Bloom", @"Shaders\Code\DepthTextureDisplay_vert.glsl", @"Shaders\Code\BloomFrag.glsl"))
            .AddDynamicUniform<Matrix4>("lightCameraVP", () => light.cam.ViewProjectionMatrix)
            .AddDynamicUniform<Matrix4>("invLightCameraVP", () => light.cam.ViewProjectionMatrix.Inverted())
            .AddDynamicUniform<Matrix4>("mainCameraVP", () => Camera.main.ViewProjectionMatrix)
            .AddDynamicUniform<Matrix4>("mainCameraView", () => Camera.main.ViewMatrix)
            .AddDynamicUniform<Matrix4>("invMainCameraVP", () => Camera.main.ViewProjectionMatrix.Inverted())
            .SetFrameBufferAttachment("lightDepth", light.depthBuffer, AttachmentType.Depth)
            .SetFrameBufferAttachment("sceneDepth", prePostProcessingGBuffer, AttachmentType.Depth);
            var invLightCameraVP = light.cam.ViewProjectionMatrix;

            // light.transform.parent = mainCamera.transform;
            // ShaderManager.CompileShader(@"DepthTextureDisplay_vert.glsl",@"DepthTextureDisplay_frag.glsl");


            var s2d= resources.CreateShader("ImgDisplay", @"Shaders\Code\DepthTextureDisplay_vert.glsl", @"Shaders\Code\DepthTextureDisplay_frag.glsl");
            // var sd = new Shader_Old();
            //   sd.Id = s2d.Id;
            //var s2d = new OnScreenTextureShader();
            //s2d.Compile();
            ImageDisplayMat = new Material(s2d)
                .SetFrameBufferAttachment("lightDepth", light.depthBuffer, AttachmentType.Depth)
                ;
            SunFlareMat = new Material(MainGameWindow.instance.resources.CreateShader("PostProcessing_SunFlare", @"Shaders\Code\DepthTextureDisplay_vert.glsl", @"Shaders\Code\SunFlare_frag.glsl"))
            .AddDynamicUniform<Vector3>("sunPosition", () => light.transform.position)
            .SetFrameBufferAttachment("lightDepth", light.depthBuffer, AttachmentType.Depth);
            
            FogMat = new Material(MainGameWindow.instance.resources.CreateShader("PostProcessing_Fog", @"Shaders\Code\DepthTextureDisplay_vert.glsl", @"Shaders\Code\Simple Fog.glsl"))
                            .SetFrameBufferAttachment(Shader.ScreenTexture, prePostProcessingGBuffer, AttachmentType.Color)
                        .SetFrameBufferAttachment(Shader.CameraDepth, prePostProcessingGBuffer, AttachmentType.Depth);


            
           // light.transform.Forward = -light.transform.position.Normalized();

            GL.Enable(EnableCap.DepthTest);

            //GL.CullFace(CullFaceMode.Front);
            //view = Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f);
            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f),this.Size.X  /(float) this.Size.Y, 0.1f, 100.0f);
            view = mainCamera.ViewMatrix;
            // shader = new Shader3d();
            // shader.Compile();
            Default3dShader = resources.CreateShader("Default3dShader",@"Shaders\Code\Basic3d_vert.glsl", @"Shaders\Code\SimpleTexture_frag.glsl");
            var shd = Default3dShader;
            texture = resources.CreateTexture("NoneTexture", "");
                
           // OutPutBuffer = new ScreenBuffer("final prePostProcessing Texture", ClientSize.X, ClientSize.Y);
            OutPutBuffer = FrameBufferPresets.CreatePostProcessing(ClientSize.X, ClientSize.Y);
            RealTexture = resources.CreateTexture("Textures\\Textures\\photo_2024-05-03_14-01-22.jpg");
          var  RealTexture2 = resources.CreateTexture("Textures\\Textures\\silk25-square-grass.jpg");
          var  RealTexture3 = resources.CreateTexture("Textures\\Textures\\square-rock.png");
          var  RealTexture4 = resources.CreateTexture("Textures\\Textures\\greenishRockTexture.jpg");


            
      

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


            CubeMesh = Cube.Generate(1);
            var mat = new Material(shd).SetTexture(RealTexture4);
            RockMaterial = new Material(shd).SetTexture(RealTexture3);

            resources.RegisterMaterial("default", mat);


            rr = new RenderComponent(CubeMesh, mat).WithSelfGamobject();

            
           

            //renderer.AddToRender(rr);


            var rr31 = new RenderComponent(CubeMesh, mat).WithSelfGamobject();

            rr31.transform.scale = new Vector3(50, 1, 50);

            //renderer.AddToRender(rr31);

            var N = 2;
            float[] arrr = new float[N * N * N];
            var MM = TerrainNoise.GenUniformGrid3D(arrr, 0, 0, 0, N, N, N, 0.01f, 121);
            var middle =  (MM.max - MM.min) / 2 + MM.min;
            int ll = arrr.Length;
            var N2 = N * N;
            for (int i = ll - 1; i > 0; i--)
            {
                if (IsBlock(i) && false)// Temporary disabled
                {    
                    if (!IsEnclosed(i) && !IsBotFree(i))
                    {
                        var resMat = System.Random.Shared.Next(0, 2) == 1 ? RockMaterial : mat;
                        rr = new RenderComponent(CubeMesh,resMat).WithSelfGamobject();
                        
                        int x = i % N;
                        int y = (i / N) % N;
                        int z = i / (N * N);
                        rr.transform.position = new Vector3(x-N/2, y-(N*0.8f), z+4 );
                        rr.gameObject.name = rr.transform.position.ToString();
                        renderer.AddToRender(rr);
                    }
                }
            }
            rr = new RenderComponent(CubeMesh, mat).WithSelfGamobject();
            
            rr.transform.position = new Vector3(0, 0,-5);
            //renderer.AddToRender(rr);
            var asd = rr.transform.worldSpaceModel;
            rr.transform.parent = mainCamera.transform;
            asd = rr.transform.worldSpaceModel;
            FollowTest = rr;
            bool IsBlock(int ind)
            {
                int x = ind % N;
                int y = (ind / N) % N;
                int z = ind / (N * N);
                return arrr[ind] <= middle;
            }
            bool IsTopFree(int ind)
            {

                //if (!((ind - N2) > 0 && IsBlock(ind - N2))) return false;
                if (((ind + N) < arrr.Length && IsBlock(ind + N))) return false;

                return true;
            }
            bool IsBotFree(int ind)
            {

                //if (!((ind - N2) > 0 && IsBlock(ind - N2))) return false;
                if (((ind - N) >0 && IsBlock(ind - N))) return false;

                return true;
            }
            bool IsEnclosed(int ind)
            {
                int x = ind % N;
                int y = (ind / N) % N;
                int z = ind / (N * N);
                //int z =  ();
                var v = (ind/N2);
                var bb = (v==0 || v%(N-1)==0) ;
                if (bb)
                {
                    if(z!=0 && z !=N-1)
                    {

                    }
                }
                if (!((ind-1)>0 &&  N% ind != 0 && IsBlock(ind - 1))) return false;   
                if (!((ind + 1) < arrr.Length &&  N % ind != 0 && IsBlock(ind + 1)))return false;

                if (!((ind - N) > 0 && IsBlock(ind - N)) ) return false;
                if (!((ind + N) < arrr.Length && IsBlock(ind + N))) return false;

                if (!((ind - N2) > 0 && IsBlock(ind - N2)) && !bb) return false;
                if (!((ind + N2) < arrr.Length && IsBlock(ind + N2)) && !bb) return false;



                return true;
            }
            //Code goes here

           
           // t.Resize(512, 512, false);
            var Noise = new float[512 * 512];
            var mm= CelluarNoise.GenUniformGrid2D(Noise, 0, 0, 512, 512, 0.01f, 123);
            //148,148,141 med
            //168,168,162 hig
            //23,36,28 low

            var total = mm.max - mm.min;
            var hig = total * 0.65f + mm.min;
            var med = total * 0.3f + mm.min;
            int ind = 0;

            var gradientSource = new (float, Rgba32)[]
  { 
      (mm.min,new Rgba32(148, 155, 212, 255) ),
      //(med,new Rgba32(43, 56, 38, 255)),
      (total/2+ mm.min,new Rgba32(148, 148, 141, 255)),
      //(hig,new Rgba32(168, 168, 162, 255)),
      (mm.max,new Rgba32(23, 36, 28, 255)),
  };

            // Get the color at value 0.25.
           

            object asyncLock = new object();
            var MaxInd = 0;
            var MexTarget = Noise.Length;
            Texture t = resources.RegisterTexture("NoneTexture",TextureGenerator.CreateProcedural(512, 512, (x, y, ind) =>
            {

                var n = Noise[ind];
                var color = gradientSource.GetPiorityLerpColor(n, 0.5f);
                MaxInd = ind;
                return color;

            })); ;
      
            //t.GenerateFromCode(512,512, (x,y,ind) =>
            //{
               
            //            var n = Noise[ind];
            //    var color = gradientSource.GetPiorityLerpColor(n,0.5f);
            //    MaxInd = ind;
            //    return color;
                
            //});
            var mat3 = new Material(shd).SetTexture(t);
            centreObject = new RenderComponent(CubeMesh, mat).WithSelfGamobject();
           // renderer.AddToRender(centreObject);
            var mats = new List<Material>()
            {
                RockMaterial,
                mat,
                mat3
            };
          //  centreObject.transform.scale *= 2;
            for (int i = 0; i < 11; i++)
            {
                var pos = Random.InsideSphere(10, 35);
                var resMat = mats[System.Random.Shared.Next(0, 3)] ;
                rr = new RenderComponent(CubeMesh, resMat).WithSelfGamobject();

                rr.transform.position = pos;
                rr.transform.parent = centreObject.transform;
                rr.transform.scale *= 0.25f;
                //renderer.AddToRender(rr);
            }


            var style = new DarkishRedImGuiTheme();
            style.Apply();


            //Pyramid
            int pyramidSize = 40;
            for (int i = 0; i < pyramidSize; i++)
            {
                // Calculate the number of boxes on this layer
                int boxesOnLayer = pyramidSize - i;

                // Calculate the offset for the layer
                float offset = (pyramidSize - boxesOnLayer) / 2.0f;

                // Loop through each box on this layer
                for (int j = 0; j < boxesOnLayer; j++)
                {
                    // Loop through each box on this layer
                    //for (int k = 0; k < boxesOnLayer; k++)
                    //{
                        // Calculate the position of the box
                       // Vector3 scale = Random.InsideSphere(
                        Vector3 position = new Vector3(offset + j, 1 + i, 
                            //offset + k
                            0
                            );

                        // Instantiate the box at the calculated position
                        GameObject box = new GameObject($"Pyramid {position.ToStringShort()}", position, Vector3.Zero);
                        var resMat = mats[System.Random.Shared.Next(0, 3)];
                        var rr3 = new RenderComponent(CubeMesh, resMat);
                        box.AddComponent(rr3);
                        var Rb = new SimpleRigidBody<Box>(box, new Box(1,1,1), System.Random.Shared.Next(5,1000));
                      //  renderer.AddToRender(rr3);
                  //  }
                }
            }
          //  Renderer.useFrustumCalling = true;

            Chunk.blockProperties.Add(1, new BlockProperties() { mesh = CubeMesh });

            cg = new ChunkGen(TerrainNoise, 02531, 0.02f);
            ExpirementalChunk.Run();
            ScriptManager.Initialize();

            //NetworkTest nt = new NetworkTest();
            //nt.Initialize(true);
            //TcpServer serv= new TcpServer();
            //TcpClientSide client = new TcpClientSide();
            //serv.Start(45334);
            //client.Connect("localhost", 45334).GetAwaiter().OnCompleted(() =>
            //{
            //    client.StartPingSender();
            //});

            var gbufferStep = new RenderStep
            {
                Name = "GBufferPass",
                PassType = RenderPass.main,
                Camera = ()=> mainCamera,
                Target = prePostProcessingGBuffer,
                PreExecute = ctx =>
                {
                    //ctx.Step.Target.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                    // gbufferShader.Use();
                }
            };
            var shadowStep = new RenderStep
            {
                Name = "ShadowPass",
                PassType = RenderPass.depth,
                Camera = ()=> light.cam,
                Target = light.depthBuffer,
                PreExecute = ctx =>
                {
                    //ctx.Step.Target.Clear(ClearBufferMask.DepthBufferBit);
                    //shadowShader.Use();
                }
            };
            renderer.AddRenderStep(shadowStep, 0);
            renderer.AddRenderStep(gbufferStep, 1);
        }
        public ChunkGen cg;
        RenderComponent FollowTest;
        RenderComponent centreObject;
        List<RenderComponent> TestRender = new List<RenderComponent>();
        List<RenderComponent> DrawThisFrame = new List<RenderComponent>();
        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);


            _controller.PressChar((char)e.Unicode);
        }
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            if (e.OffsetY > 0)
            {
                if (TornadoSpeed > 0) TornadoSpeed -= e.OffsetY;
            }
            else
            {
                TornadoSpeed -= e.OffsetY;
            }
            _controller.MouseScroll(e.Offset);
        }
        BodyReference brr;
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            {
                int WasLight = light.depthBuffer.Width;
                Time.deltaTime = (float)e.Time;
                Time.time += Time.deltaTime;
                _stopwatch.Start();
                _controller.Update(this, (float)e.Time);
                OnBeforeScriptsRun?.Invoke();
                ScriptManager.TickUpdate((float)e.Time);
                OnAfterScriptsRun?.Invoke();
                while (_runOnMainThread.Reader.TryRead(out var act))
                {
                    act?.Invoke();
                }
                ShaderManager.OnFrameStart();
                
                Physic.Update(Time.deltaTime);

                var start = _stopwatch.Elapsed;
                _frameCount++;


               // GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

                Profiler.BeginSample("All Render");
                Profiler.BeginSample("T2");
                var model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(35) * dt * TornadoSpeed / 20);
                model *= Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(25) * dt * TornadoSpeed / 20);
                centreObject.transform.rotation *= model.ExtractRotation();
                //centreObject.transform.rotation.Normalize();


                 renderer.ExecuteRenderPipeline();

                //light.depthBuffer.Use();
                //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
                //var res2 = renderer.RenderScene(new RenderSceneCommand("Light", light.cam, Renderer.RenderPass.depth, light.depthBuffer));

                ////depthBuffer.Use();
                ////GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
                ////var res3 = renderer.RenderScene(new RenderSceneCommand("CameraDepth", mainCamera, Renderer.RenderPass.depth, depthBuffer));

                //prePostProcessingGBuffer.Use();
                //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
                //GL.Enable(EnableCap.DepthTest);
                //GL.Viewport(0, 0, this.ClientSize.X, this.ClientSize.Y);
                //var res4 = renderer.RenderScene(new RenderSceneCommand("PrePostProcessing", mainCamera, Renderer.RenderPass.main, prePostProcessingGBuffer));

                //var shadowStep = new RenderStep
                //{
                //    Name = "Light",
                //    PassType = RenderPass.depth,
                //    Camera = ()=> light.cam,
                //    Target = light.depthBuffer,
                //    PreExecute = ctx =>
                //    {
                //        ctx.Step.Target.Clear(ClearBufferMask.DepthBufferBit);
                //        shadowShader.Use();
                //    }
                //};


                Profiler.EndSample("T2");


                // ImageDisplayMat.mainColor = light.depthBuffer[0].Texture;
                //GL.DepthFunc(DepthFunction.Never);
                //if (ShowDebugTexture)
                //    FullScreenSquad.Render(ImageDisplayMat);
                using (prePostProcessingGBuffer.Bind())
                {
                    
                    FullScreenSquad.Render(sss);
                    FullScreenSquad.Render(FogMat);
                }

                Gizmos.DrawLine(new Vector3(-2, -2, -2), new Vector3(25, 25, 25), 0.05f);
                //GL.DepthFunc(DepthFunction.Notequal);


                //GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
               if (true)  using (OutPutBuffer.Bind())
                {
                    //GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

                    //var res = renderer.RenderScene(mainCamera, Renderer.RenderPass.main);
                    GL.Enable(EnableCap.DepthTest);
                    FullScreenSquad.Render(PP_BloomMat);
                    FullScreenSquad.Render(SunFlareMat);

                    ImGui.Begin("Scene");
                    ImGui.Image(OutPutBuffer[0].Texture.id, ImGui.GetWindowSize(), new System.Numerics.Vector2(0, 1), new System.Numerics.Vector2(1, 0));
                    ImGui.End();
                        ImGui.Begin("Gbuf");
                        ImGui.Image(prePostProcessingGBuffer[0].Texture.id, ImGui.GetWindowSize(), new System.Numerics.Vector2(0, 1), new System.Numerics.Vector2(1, 0));
                        ImGui.End();
                        ImGui.Begin("Depth");
                        ImGui.Image(this.depthBuffer[0].Texture.id, ImGui.GetWindowSize(), new System.Numerics.Vector2(0, 1), new System.Numerics.Vector2(1, 0));
                        ImGui.End();
                        Profiler.EndSample("All Render");

                    // var res = renderer.RenderScene(mainCamera, Renderer.RenderPass.main);
                    ImGui.ShowDemoWindow();
                    //ImGui.NewFrame();

                    //// Your ImGui UI code goes here...
                    //ImGui.Text("Hello, ImGui!");
                    //ImGui.EndFrame();
                    //ImGui.DockSpaceOverViewport();

                    //ImGui.ShowDemoWindow();

                    // ImGui.Text(FollowTest.transform.worldSpaceModel.ToTransformString());
                    //ImGui.Text(FollowTest.transform.localSpaceModel.ToTransformString());
                    if (ImGui.Button("Launch as server"))
                    {
                        NetworkTest nt = new NetworkTest();
                        nt.Initialize(true);
                    }
                    if (ImGui.Button("Launch as client"))
                    {
                        NetworkTest nt = new NetworkTest();
                        nt.Initialize(false);
                    }
                    ImGui.TextWrapped($"cam: {mainCamera.transform.position}");
                    ImGui.TextWrapped($"camT: {mainCamera.transform}");

                    ImGui.SliderInt($"ShadowRes:", ref light.depthBuffer.Width, 512, 16384);
                    ImGui.Checkbox("Frostum calling", ref Renderer.useFrustumCulling);
                    ImGui.Checkbox("Hierarchy", ref Hierarchy.DrawHierarchyWindow);
                    //Profiler.BeginSample("DragWindow");


                    if (Hierarchy.DrawHierarchyWindow)
                    {
                        Inspector.instance.DrawWindow(Inspector.instance.DrawedObject);

                        //Profiler.EndSample("DragWindow");
                        Hierarchy.Draw();
                    }


                    //ImGui.TextWrapped($"ren: {res.TotalObjectsRendered}");
                    ImGui.End();

                    Profiler.Draw();
                    ResourceDrawer.Draw();
                }
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

                //var res = renderer.RenderScene(mainCamera, Renderer.RenderPass.main);
                GL.Enable(EnableCap.DepthTest);
                _controller.Render();

                Profiler.BeginSample("FrameEnd");
                ImGuiController.CheckGLError("End of frame");
                Profiler.BeginSample("SwapBuffers");
                //this.VSync = VSyncMode.Off;
                SwapBuffers();
                Profiler.EndSample("SwapBuffers");
                renderer.OnFrameEnd();
                Profiler.EndSample("FrameEnd"); 
                
                
                dt = (float)(_stopwatch.Elapsed - start).TotalSeconds;
                TimeSpan elapsed = _stopwatch.Elapsed - _lastUpdate;
                if (elapsed >= TimeSpan.FromSeconds(1))
                {
                    _fps = (float)(_frameCount / elapsed.TotalSeconds);
                    _frameCount = 0;
                    _lastUpdate = _stopwatch.Elapsed;
                    //if (_fps > 90f)
                    //{
                    //    Renderer.useFrustumCalling = false;
                    //}
                    //if (_fps < 40f)
                    //{
                    //    Renderer.useFrustumCalling = true;
                    //}
                    //Console.WriteLine($"FPS: {_fps:F2}");
                    this.Title = $"FPS: {_fps:F2}";
                }
                int NowLight = light.depthBuffer.Width;
                if (WasLight != NowLight)
                {
                    light.Resize(NowLight, NowLight);
                }
               // Profiler.EndSample("Main Thread");
            }
         
            //SwapBuffers();
        }
        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);

            // Update last mouse position
            lastMousePosition = e.Position;
        }
        private void RotateCamera(Camera cam,float deltaX, float deltaY)
        {
            // Sensitivity for rotation
            const float sensitivity = 1f;

            // Calculate quaternion rotations
            var rot = cam.transform.rotation;
            var xRotation = Quaternion.FromAxisAngle(Vector3.UnitY, -deltaX * sensitivity / 100f);
            var yRotation = Quaternion.FromAxisAngle(Vector3.UnitX, deltaY * sensitivity / 100f);
            //rot.Y += ;
            //rot.X += ;
            // Apply rotations to camera's current rotation
            //xRotation. cam.transform.Forward

            //  cam.transform.rotation = Quaternion.FromEulerAngles(rot);
            cam.transform.rotation = xRotation * rot.Normalized() * yRotation;
            // Normalize quaternion
            cam.transform.rotation.Normalize();
        }

        private Vector2 _lastMousePos;

        private Vector2 lastMousePosition;
        private bool isMouseDragging = false;
        private const float mouseSensitivity = 0.1f;

        public Stopwatch _stopwatch = new Stopwatch();
        private int _frameCount = 0;
        private float _fps = 0;
        private TimeSpan _lastUpdate = TimeSpan.Zero;
        private Matrix4 view;

        public Shader Default3dShader { get; private set; }

        private Matrix4 projection;
        private Matrix4 viewProjection;

        public Material RockMaterial { get; private set; }

        private RenderComponent rr;
         private FastNoise2 CaveNoise = FastNoise2.FromEncodedNodeTree("GgABEQACAAAAAADgQBAAAACIQR8AFgABAAAACwADAAAAAgAAAAMAAAAEAAAAAAAAAD8BFAD//wAAAAAAAD8AAAAAPwAAAAA/AAAAAD8BFwAAAIC/AACAPz0KF0BSuB5AEwAAAKBABgAAj8J1PACamZk+AAAAAAAA4XoUPw==");
         private FastNoise2 CelluarNoise = FastNoise2.FromEncodedNodeTree("CwABAAAAAAAAAAEAAAAAAAAAAOxROL8=");
         private FastNoise2 SquaresNoise = FastNoise2.FromEncodedNodeTree("CwABAAAAAAAAAAEAAAAAAAAAAI/Cdb0=");
        private FastNoise2 TerrainNoise = FastNoise2.FromEncodedNodeTree("EQACAAAAAAAgQBAAAAAAQBkAEwDD9Sg/DQAEAAAAAAAgQAkAAGZmJj8AAAAAPwEEAAAAAAAAAEBAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAM3MTD4AMzMzPwAAAAA/");

        public Material materialInUse;
        public Mesh meshInUse;

        public Mesh CubeMesh { get; private set; }
    }

}
