using ConsoleApp1_Pet.Architecture;
using ConsoleApp1_Pet.Render;
using ConsoleApp1_Pet.Scripts.DebugScripts;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Scripts
{
    public static class ScriptManager
    {
        public static List<IScript> AllScripts = new List<IScript>(150);
        public static List<IUpdatable> UpdateSctipts = new List<IUpdatable>(100);
        public static void AddScript<T>(T script) where T : IScript
        {
            AllScripts.Add(script);
            if(script is IUpdatable u)
            {
                int order = 0;
                if (script is IOrderImportant ord) order = ord.ScriptExecutionOrder;
                UpdateSctipts.Add(u);
                UpdateSctipts = UpdateSctipts.OrderBy(x =>
                {
                    int order = 0;
                    if (x is IOrderImportant ord) order = ord.ScriptExecutionOrder;
                    return order;
                }
                ).ToList();
            }
        }
        
        public static void TickUpdate(float dt)
        {
            var Values = UpdateSctipts;
           // var span = CollectionsMarshal.AsSpan(Values);
            for (int i = Values.Count-1; i >= 0; i--)
            {
                if ((Values[i] is IScript scr))
                {
                    if (scr.IsActive)
                        Values[i].Update();
                }
                else
                {
                    Values[i].Update();
                }
            }
        }


        public static void Initialize()
        {
            //GameObject box = new GameObject($"TestCube");
            //var resMat = Game.instance.RockMaterial;
            //var rr3 = new RenderComponent(Game.instance.CubeMesh, resMat);
            //box.AddComponent(rr3);
            //var Cnst = new CameraCursorGridSnap();
            //box.AddComponent(Cnst);
            //Game.instance.renderer.AddToRender(rr3 );
            GameObject box = DebugDrawer.GetBoundingBox(new DebugDrawer.Bounds(Vector3.Zero, Vector3.One), 0.0256f).parent;
            for (int i = 1; i < 4; i++)
            {
                GameObject box2 = DebugDrawer.GetBoundingBox(new DebugDrawer.Bounds(Vector3.Zero+Vector3.One*i, 2), 0.0256f).parent;
            }
            var Cnst = new CameraCursorGridSnap();
            box.AddComponent(Cnst);
        }
    }

    public interface IUpdatable
    {
        public void Update();
    }
    public interface IOrderImportant
    {
        public int ScriptExecutionOrder { get; }
    }
    public interface IOnStart
    {
        public void OnStart();
    }
    public interface IScript
    {
        public bool IsActive { get; set; }
    }
    public class MonoBehavior : Component, IScript
    {
        bool IScript.IsActive { get; set; } = true;
    }

}
