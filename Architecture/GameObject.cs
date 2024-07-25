using ConsoleApp1_Pet.Render;
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
        private List<MonoBehavior> _components = new List<MonoBehavior>();
        private List<IUpdatable> _updateComponents = new List<IUpdatable>();


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
        public void AddComponent<T>() where T : MonoBehavior, new()
        {
            T component = new T();
            component.gameObject = this;
            component.transform = this.transform;
            _components.Add(component);
            if (component is IUpdatable c)
            {
                _updateComponents.Add(c);
            }
        }
        public void AddComponent(MonoBehavior cmp) 
        {
            var component = cmp;
            component.gameObject = this;
            component.transform = this.transform;
            _components.Add(component);
            if (component is IUpdatable c)
            {
                _updateComponents.Add(c);
            }
        }
        // Gets a component of a specific type
        public T GetComponent<T>() where T : MonoBehavior
        {
            foreach (MonoBehavior component in _components)
            {
                if (component is T)
                {
                    return (T)component;
                }
            }
            return null;
        }
    }
    public interface IUpdatable
    {
        public void Update();
    }
    public class MonoBehavior
    {
        public GameObject gameObject { get; set; }
        public Transform transform { get; set; }

    }
}
