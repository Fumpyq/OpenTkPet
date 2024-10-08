using ConsoleApp1_Pet.Render;
using ConsoleApp1_Pet.Scripts;
using ConsoleApp1_Pet.Scripts.DebugScripts;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Architecture
{
    public class GameObject
    {
        // Unique identifier for the game object
        public int ID { get; private set; }

        // Name of the game object
        public string name { get; set; }

        // Transform component for position, rotation, and scale
        public Transform transform { get; private set; }

        // List of components attached to this game object
        public List<Component> Components = new List<Component>();



        // Static counter for assigning unique IDs
        private static int _nextID = 1;

        // Constructor
        public GameObject(string name)
        {
            ID = _nextID++;
            this.name = name;
            transform = new Transform(this);
            Scene.instance.objects.Add(this);
        }

        public GameObject(string name, Transform transform)
        {
            ID = _nextID++;
            this.name = name;
            this.transform = transform;
            Scene.instance.objects.Add(this);
        }
        public GameObject(string name, Vector3 position, Vector3 rotation)
        {
            ID = _nextID++;
            this.name = name;
            transform = new Transform(position, rotation);
            Scene.instance.objects.Add(this);
        }
        // Adds a component to the game object
        public void AddComponent<T>() where T : Component, new()
        {
            T component = new T();
            component.gameObject = this;
            component.transform = this.transform;
            Components.Add(component);
            if (component is IScript sc)
                ScriptManager.AddScript(sc);

        }
        public void AddComponent(Component cmp) 
        {
            var component = cmp;
            component.gameObject = this;
            component.transform = this.transform;
            Components.Add(component);
            if (cmp is IScript sc)
                ScriptManager.AddScript(sc);
        }
        // Gets a component of a specific type
        public T GetComponent<T>() where T : Component
        {
            foreach (Component component in Components)
            {
                if (component is T)
                {
                    return (T)component;
                }
            }
            return null;
        }
        public enum PrimitiveType
        {
            Cube
        }
        internal static GameObject CreatePrimitive(object cube)
        {
            GameObject box = new GameObject($"TestCube");
            var resMat = new Materials.TextureMaterial(Game.instance.Default3dShader,null);
            var rr3 = new RenderComponent(Game.instance.CubeMesh, resMat);
            box.AddComponent(rr3);
            Game.instance.renderer.AddToRender(rr3);
            return box;
        }
    }

    public class Component
    {
        public GameObject gameObject { get; set; }
        public Transform transform { get; set; }
        
    }
}
