using ConsoleApp1_Pet.Architecture;
using ConsoleApp1_Pet.Meshes;
using ConsoleApp1_Pet.Render;
using ConsoleApp1_Pet.Server;
using ConsoleApp1_Pet.Новая_папка;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ConsoleApp1_Pet.Server.TcpServer;

namespace ConsoleApp1_Pet.Scripts.DebugScripts
{
    public class NetworkTest
    {
        public void Initialize(bool IsServer)
        {
            if (IsServer)
            {
                TcpServer serv = new TcpServer();
                serv.Start(45334);
            }
            else
            {
                TcpClientSide client = new TcpClientSide();
                client.Connect("localhost", 45334).GetAwaiter().OnCompleted(() =>
                {
                    client.StartPingSender();
                });
            }

            GameObject gg = new GameObject("NetworkTest");
            RenderComponent rc = new RenderComponent(Cube.Generate(2), MainGameWindow.instance.RockMaterial);
            gg.AddComponent(rc);
            NetworkIdentity ni = new NetworkIdentity();
            gg.AddComponent(ni);
            
            if (IsServer)
            {
                MainGameWindow.instance.OnBeforeScriptsRun += () =>
                {
                    gg.transform.position = Vector3.Lerp(gg.transform.position, Vector3.UnitX * MathF.Sin(Time.time * 2) * 4 + Vector3.UnitZ * MathF.Sin(Time.time / 2) * 6, 2.4f * Time.deltaTime);
                };
            }

        }
    }
}
