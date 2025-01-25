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
        public List<GOComponent> Components = new List<GOComponent>();

        public event Action<GameObject, GOComponent> OnComponentAdded;
        public event Action<GameObject, GOComponent> OnComponentRemoved;

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
        public void AddComponent<T>() where T : GOComponent, new()
        {
            T component = new T();
            _addComponent(component);

        }
        public void AddComponent(GOComponent cmp)
        {
            var component = cmp;
            _addComponent(component);
        }

        private void _addComponent(GOComponent component)
        {
            component.gameObject = this;
            component.transform = this.transform;
            component.OnInit(this);
            OnComponentAdded?.Invoke(this, component);
            Components.Add(component);

            if (component is IScript sc)
                ScriptManager.AddScript(sc);
        }

        // Gets a component of a specific type
        public T GetComponent<T>() where T : GOComponent
        {
            foreach (GOComponent component in Components)
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
            var resMat = new Materials.TextureMaterial(MainGameWindow.instance.Default3dShader,null);
            var rr3 = new RenderComponent(MainGameWindow.instance.CubeMesh, resMat);
            box.AddComponent(rr3);
           // MainGameWindow.instance.renderer.AddToRender(rr3);
            return box;
        }
    }
    public static class ComponentIdIncrement
    {
        private static int _nextId=-10000; // Backing field for the ID

        public static int GetNextId()
        {
            return Interlocked.Increment(ref _nextId);
        }
    }
    public class GOComponent
    {
        public int ID { get; private set; } = ComponentIdIncrement.GetNextId();
        public GameObject gameObject { get; set; }
        public Transform transform { get; set; }
        /// <summary>
        /// this method called then component added to go, but not yet in go component list;
        /// </summary>
        /// <param name="go"></param>
        public virtual void OnInit(GameObject go) { 
            //Do nothing
        }
        
    }
}
