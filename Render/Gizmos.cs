using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Render
{
    public static class Gizmos
    {
        public static LineDrawer3D lineDrawer =new LineDrawer3D();
        public static void DrawLine(Vector3 from, Vector3 to, int thickness = 1)
        {
            lineDrawer.DrawLine(from, to, thickness);
        }
    }
    //public class LineDrawer
    //{
    //    private int _vertexArrayObject;
    //    private int _vertexBufferObject;
    //    private int _shaderProgram;

    //    public LineDrawer()
    //    {
    //        // Create and compile shaders
    //        _shaderProgram = CreateShaderProgram();

    //        // Generate and bind vertex array object
    //        _vertexArrayObject = GL.GenVertexArray();
    //        GL.BindVertexArray(_vertexArrayObject);

    //        // Generate and bind vertex buffer object
    //        _vertexBufferObject = GL.GenBuffer();
    //        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

    //        // Set up vertex attribute pointer
    //        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0);
    //        GL.EnableVertexAttribArray(0);

    //        // Unbind buffer
    //        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    //        GL.BindVertexArray(0);
    //    }

    //    // Draw a line between two points with specified thickness
    //    public void DrawLine(Vector3 start, Vector3 end, float thickness)
    //    {
    //        // Use shader program
    //        GL.UseProgram(_shaderProgram);

    //        // Bind vertex array object
    //        GL.BindVertexArray(_vertexArrayObject);

    //        // Update vertex buffer data
    //        float[] vertices = { start.X, start.Y, end.X, end.Y };
    //        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
    //        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StreamDraw);

    //        // Set line width
    //        GL.LineWidth(thickness);

    //        // Draw line
    //        GL.DrawArrays(PrimitiveType.Lines, 0, 2);
    //        GL.LineWidth(0);
    //        // Unbind vertex array object
    //        GL.BindVertexArray(0);
    //    }

    //    // Create and compile shader program
    //    private int CreateShaderProgram()
    //    {
    //        // Vertex shader
    //        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
    //        string vertexShaderSource = @"
    //            #version 330 core
    //            layout (location = 0) in vec2 aPos;

    //            void main()
    //            {
    //                gl_Position = vec4(aPos, 0.0, 1.0);
    //            }
    //        ";
    //        GL.ShaderSource(vertexShader, vertexShaderSource);
    //        GL.CompileShader(vertexShader);

    //        // Fragment shader
    //        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
    //        string fragmentShaderSource = @"
    //            #version 330 core
    //            out vec4 FragColor;

    //            void main()
    //            {
    //                FragColor = vec4(1.0f, 1.0f, 1.0f, 1.0f);
    //            }
    //        ";
    //        GL.ShaderSource(fragmentShader, fragmentShaderSource);
    //        GL.CompileShader(fragmentShader);

    //        // Create shader program
    //        int shaderProgram = GL.CreateProgram();
    //        GL.AttachShader(shaderProgram, vertexShader);
    //        GL.AttachShader(shaderProgram, fragmentShader);
    //        GL.LinkProgram(shaderProgram);

    //        // Delete shaders
    //        GL.DeleteShader(vertexShader);
    //        GL.DeleteShader(fragmentShader);

    //        return shaderProgram;
    //    }
    //}
    public class LineDrawer3D
    {
        private int _vertexArrayObject;
        private int _vertexBufferObject;
        private int _shaderProgram;

        private Matrix4 _viewMatrix;
        private Matrix4 _projectionMatrix;

        public LineDrawer3D()
        {
            // Create and compile shaders
            _shaderProgram = CreateShaderProgram();

            // Generate and bind vertex array object
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            // Generate and bind vertex buffer object
            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

            // Set up vertex attribute pointer
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(0);

            // Unbind buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            // Set up perspective camera
            _viewMatrix = Matrix4.LookAt(new Vector3(0.0f, 0.0f, 3.0f), Vector3.Zero, Vector3.UnitY);
            _projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), 800f / 600f, 0.1f, 100.0f);
        }

        // Draw a line between two points in 3D space with specified thickness
        public void DrawLine(Vector3 start, Vector3 end, float thickness)
        {
            // Use shader program
            GL.UseProgram(_shaderProgram);

            // Bind vertex array object
            GL.BindVertexArray(_vertexArrayObject);

            // Update vertex buffer data
            float[] vertices = { start.X, start.Y, start.Z, end.X, end.Y, end.Z };
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StreamDraw);

            // Set line thickness in shader (uniform)
            int thicknessLoc = GL.GetUniformLocation(_shaderProgram, "thickness");
            GL.Uniform1(thicknessLoc, thickness);

            // Set view and projection matrices
            int viewLoc = GL.GetUniformLocation(_shaderProgram, "view");
            GL.UniformMatrix4(viewLoc, false, ref _viewMatrix);

            int projLoc = GL.GetUniformLocation(_shaderProgram, "projection");
            GL.UniformMatrix4(projLoc, false, ref _projectionMatrix);

            // Draw line
            GL.DrawArrays(PrimitiveType.Lines, 0, 2);

            // Unbind vertex array object
            GL.BindVertexArray(0);
        }

        // Create and compile shader program
        private int CreateShaderProgram()
        {
            // Vertex shader
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            string vertexShaderSource = @"
                #version 330 core
                layout (location = 0) in vec3 aPos;

                uniform mat4 view;
                uniform mat4 projection;

                void main()
                {
                    gl_Position = projection * view * vec4(aPos, 1.0);
                }
            ";
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);

            // Geometry shader
            int geometryShader = GL.CreateShader(ShaderType.GeometryShader);
            string geometryShaderSource = @"
                #version 330 core
                layout (lines) in;
                layout (triangle_strip, max_vertices = 4) out;

                uniform float thickness;

                void main() {
                    vec3 lineDir = gl_in[1].gl_Position.xyz - gl_in[0].gl_Position.xyz;
                    vec3 lineNormal = normalize(cross(lineDir, vec3(0.0, 1.0, 0.0)));

                    // Calculate offset points for thicker line
                    vec3 offset1 = lineNormal * thickness / 2.0;
                    vec3 offset2 = -offset1;

                    // Output the four vertices for the thicker line
                    gl_Position = gl_in[0].gl_Position + vec4(offset1, 0.0);
                    EmitVertex();
                    gl_Position = gl_in[1].gl_Position + vec4(offset1, 0.0);
                    EmitVertex();
                    gl_Position = gl_in[0].gl_Position + vec4(offset2, 0.0);
                    EmitVertex();
                    gl_Position = gl_in[1].gl_Position + vec4(offset2, 0.0);
                    EmitVertex();
                    EndPrimitive();
                }
            ";
            GL.ShaderSource(geometryShader, geometryShaderSource);
            GL.CompileShader(geometryShader);

            // Fragment shader
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            string fragmentShaderSource = @"
                #version 330 core
                out vec4 FragColor;

                void main()
                {
                    FragColor = vec4(1.0f, 1.0f, 1.0f, 1.0f);
                }
            ";
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);

            // Create shader program
            int shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, geometryShader);
            GL.AttachShader(shaderProgram, fragmentShader);
            GL.LinkProgram(shaderProgram);

            // Delete shaders
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(geometryShader);
            GL.DeleteShader(fragmentShader);

            return shaderProgram;
        }
    }
}
