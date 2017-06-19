using System;
using Labs.Utility;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Labs.Lab1
{
    public class Lab1Window : GameWindow
    {
        private int[] mVertexBufferObjectIDArray = new int[2];
        private ShaderUtility mShader;

        public Lab1Window()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Lab 1 Hello, Triangle",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                3, // major
                3, // minor
                GraphicsContextFlags.ForwardCompatible
                )
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(Color4.Black);
            GL.Enable(EnableCap.CullFace);

            
            uint[] indices = new uint[] { 0, 15, 20, 
                                          20, 14, 15,
                                          12, 13, 11,
                                          13,21,11,
                                          19,18,21, //
                                          18,10,21,
                                          20,1,16,
                                          1,17,16,
                                          2,3,17,
                                          22,8,9,
                                          17,3,22,
                                          3,8,22,
                                          4,5,6,
                                          7,4,6 };
            
            float[] vertices = new float[] {-0.6f,-0.6f, //bottom left       0
                                            -0.6f,0.2f, //square left top    1
                                            -0.8f,0.2f, //roof bottom left   2
                                            -0.4f,0.6f, //roof top left      3
                                            -0.2f,0.6f, //chimney base, left 4
                                            -0.2f,0.8f, //chimney top left   5
                                            0f,0.8f, //chimney top right     6
                                            0f,0.6f, //chimney base right    7
                                            0.4f,0.6f, //roof top right      8
                                            0.8f,0.2f, //roof bottom right   9
                                            0.6f,0.2f, //square top right    10
                                            0.6f,-0.6f, //bottom right       11
                                            0.4f,-0.6f, //door, lower right  12
                                            0.4f,-0.2f, //door upper right   13
                                            0.2f,-0.2f, //door upper left    14
                                            0.2f,-0.6f, //door lower left    15
                                            -0.4f,-0.2f, //win bottom left   16
                                            -0.4f,0.2f, //window top left    17
                                            0f,0.2f, //window top right      18
                                            0f,-0.2f, //win bottom right     19
                                            -0.6f,-0.2f, //mid point left    20
                                            0.6f,-0.2f, // mid point right   21
                                            0.4f,0.2f  //  roof extra point  22
            
            }; 
                            
          



            GL.GenBuffers(2, mVertexBufferObjectIDArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBufferObjectIDArray[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices,
            BufferUsageHint.StaticDraw);
            int size;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVertexBufferObjectIDArray[1]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(uint)),
            indices, BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (indices.Length * sizeof(uint) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }


            #region Shader Loading Code - Can be ignored for now

            mShader = new ShaderUtility( @"Lab1/Shaders/vSimple.vert", @"Lab1/Shaders/fSimple.frag");

            #endregion

            base.OnLoad(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBufferObjectIDArray[0]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVertexBufferObjectIDArray[1]);


            // shader linking goes here
            #region Shader linking code - can be ignored for now

            GL.UseProgram(mShader.ShaderProgramID);
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

            #endregion

            GL.DrawElements(PrimitiveType.TriangleFan, 4, DrawElementsType.UnsignedInt, 1*sizeof(uint));

            this.SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            GL.DeleteBuffers(2, mVertexBufferObjectIDArray);
            GL.UseProgram(0);
            mShader.Delete();
        }
    }
}
