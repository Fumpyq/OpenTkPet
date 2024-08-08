using BepuPhysics;
using BepuPhysics.Collidables;
using ConsoleApp1_Pet.Architecture;
using ConsoleApp1_Pet.Новая_папка;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Physics
{
    public interface IOnPhysicsUpdate
    {
        public void OnFixedUpdate();
    }
    public class SimpleRigidBody<T> : MonoBehavior, IOnPhysicsUpdate where T : unmanaged, IConvexShape
    {
        private BodyReference _ref;

        public SimpleRigidBody(GameObject gg, T  shape,float mass)
        {
            _ref = SimpleSelfContainedDemo.Simple_TEST_AddRigidBody(shape,mass , gg.transform.position.Swap(), this);
            gg.AddComponent(this);
        }
        public SimpleRigidBody(BodyReference refference)
        {
            _ref = refference;
        }

        public void OnFixedUpdate()
        {
            //transform.position = 
                ;
           // transform.rotation = 
                ;
            transform.SetPositionAndRotation(_ref.Pose.Position.Swap(), _ref.Pose.Orientation.Swap());
        }
    }
}
