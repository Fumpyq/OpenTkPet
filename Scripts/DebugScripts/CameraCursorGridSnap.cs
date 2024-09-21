using ConsoleApp1_Pet.Render;
using ConsoleApp1_Pet.Новая_папка;
using ConsoleApp1_Pet.Новая_папка.ChunkSystem;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Scripts.DebugScripts
{
    public class CameraCursorGridSnap : MonoBehavior, IUpdatable
    {
        
        private Vector3 Target;
        public float SnappingSpeed=10;

        public CameraCursorGridSnap( float snappingSpeed = 10)
        {
            SnappingSpeed = snappingSpeed;
        }


        public void Update()
        {
            Target = Camera.main.transform.position + Camera.main.transform.Forward*7;
            Target = Target.SnapToGrid(1);
            transform.position = Vector3.Lerp(transform.position, Target, SnappingSpeed * Time.deltaTime);
        }
    }
    
}
