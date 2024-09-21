using ConsoleApp1_Pet.Render;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Scripts.DebugScripts
{
    public class PositionConstraint : MonoBehavior,IUpdatable
    {
        public Transform Target;
        public Vector3 RelativeOffset;

        public PositionConstraint(Transform target, Vector3 positionOffest)
        {
            Target = target;
            RelativeOffset = positionOffest;
        }

        public void Update()
        {
            Vector3 offest = transform.Forward * RelativeOffset;
            Target.position = transform.position + offest;
        }
    }
}
