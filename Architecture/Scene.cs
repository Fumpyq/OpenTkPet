using ConsoleApp1_Pet.Editor;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace ConsoleApp1_Pet.Architecture
{
    public class Scene
    {
        private static Scene _instance;
        public static Scene instance { get => _instance ??= new Scene(); }

        public List<GameObject> objects = new List<GameObject>();
        public void RegisterObject(GameObject obj)
        {
            objects.Add(obj);
        }
    }
}
