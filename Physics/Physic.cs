
using ConsoleApp1_Pet.Новая_папка;
using Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Physics
{
    public static class Physic
    {
        private static float fixedDeltaTime = 1.0f / 50.0f; // 50 time per sec
        private static float MaxDeltaTime = 1.0f/20.0f; // 10 time per sec
        private static float accumulator = 0.0f;
        public static bool IsSimulationEnabled = false;
        public static void Update(float deltaTime)
        {
            Profiler.BeginSample("physics");
            deltaTime = Math.Clamp (deltaTime,0f, MaxDeltaTime);
            accumulator += deltaTime;
          
            while (accumulator >= fixedDeltaTime)
            {
                if(IsSimulationEnabled)
                    SimpleSelfContainedDemo.PhysicsTick(fixedDeltaTime);
                        accumulator -= fixedDeltaTime;
            }
            Profiler.EndSample("physics");
        }
    }
}
