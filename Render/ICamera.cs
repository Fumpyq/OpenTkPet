using OpenTK.Mathematics;

namespace ConsoleApp1_Pet.Render
{
    public interface ICamera
    {
        public Vector3 position { get ; }
        Matrix4 ProjectionMatrix { get; }
        Matrix4 ViewMatrix { get; }
        Matrix4 ViewProjectionMatrix { get; }
    }
}