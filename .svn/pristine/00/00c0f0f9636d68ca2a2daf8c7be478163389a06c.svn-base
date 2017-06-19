using OpenTK;
using System;
using OpenTK.Graphics;
using Labs.Utility;
using OpenTK.Graphics.OpenGL;

namespace Labs.Lab2
{
    public class Lab2_2Window : GameWindow
    {
        public Lab2_2Window()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Lab 2_2 Understanding the Camera",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                3, // major
                3, // minor
                GraphicsContextFlags.ForwardCompatible
                )
        {
        }

        private int[] mVBO_IDs = new int[2];
        private int mVAO_ID;
        private ShaderUtility mShader;
        private ModelUtility mModel;
        private static Matrix4 mView;

        protected override void OnLoad(EventArgs e)
        {
            // Set some GL state
            GL.ClearColor(Color4.DodgerBlue);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            mModel = ModelUtility.LoadModel(@"Utility/Models/lab22model.sjg");    
            mShader = new ShaderUtility(@"Lab2/Shaders/vLab22.vert", @"Lab2/Shaders/fSimple.frag");
            GL.UseProgram(mShader.ShaderProgramID);
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            int vColourLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vColour");
            //mView = Matrix4.Identity;
            mView = Matrix4.CreateTranslation(0, 0, -2);
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);

            if (mShader != null)
            {
                int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
                //int windowHeight = this.ClientRectangle.Height;
                //int windowWidth = this.ClientRectangle.Width;

                Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, (float)ClientRectangle.Width / ClientRectangle.Height, 0.5f, 5);
            }
                /*
                if (windowHeight > windowWidth)
                {
                    if (windowWidth < 1) 
                    {
                        windowWidth = 1;
                    }
                    float ratio = (float) windowHeight / windowWidth;
                    Console.Write(ratio);
                    Matrix4 projection = Matrix4.CreateOrthographic(ratio * 10, 10, -1, 1); 

                    GL.UniformMatrix4(uProjectionLocation, true, ref projection);
                }
                else
                {
                    if (windowHeight < 1) 
                    {
                        windowHeight = 1; 
                    }
                    float ratio = (float) windowWidth / windowHeight;
                    Console.Write(ratio + ":" + windowHeight + ":" + windowWidth);
                    Matrix4 projection = Matrix4.CreateOrthographic(10, ratio * 10, -1, 1);
                    GL.UniformMatrix4(uProjectionLocation, true, ref projection);
                }
            }*/

            mVAO_ID = GL.GenVertexArray();
            GL.GenBuffers(mVBO_IDs.Length, mVBO_IDs);
            
            GL.BindVertexArray(mVAO_ID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mModel.Vertices.Length * sizeof(float)), mModel.Vertices, BufferUsageHint.StaticDraw);           
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[1]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mModel.Indices.Length * sizeof(float)), mModel.Indices, BufferUsageHint.StaticDraw);

            int size;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mModel.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mModel.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(vColourLocation);
            GL.VertexAttribPointer(vColourLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

            GL.BindVertexArray(0);

            base.OnLoad(e);
            
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Console.Clear();
            GL.Viewport(this.ClientRectangle);
            if (mShader != null)
            {
                int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
                int windowHeight = this.ClientRectangle.Height;
                int windowWidth = this.ClientRectangle.Width;

                if (windowHeight > windowWidth)
                {
                    if (windowWidth < 1)
                    {
                        windowWidth = 1;
                    }
                    float ratio = (float)windowWidth / windowHeight;
                   // float ratio = (float)windowHeight / windowWidth;
                    Console.Write(ratio);
                    //Matrix4 projection = Matrix4.CreateOrthographic(10, 10/ratio, -1, 1);
                    Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, (float)ClientRectangle.Width / ClientRectangle.Height, 0.5f, 5);
                    GL.UniformMatrix4(uProjectionLocation, true, ref projection);
                }
                else
                {
                    if (windowHeight < 1)
                    {
                        windowHeight = 1;
                    }
                    float ratio = (float)windowHeight / windowWidth;
                    //float ratio = (float)windowWidth / windowHeight;
                    Console.Write(ratio + ":" + windowHeight + ":" + windowWidth);
                    //Matrix4 projection = Matrix4.CreateOrthographic(10, ratio * 10, -1, 1);
                    Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, (float)ClientRectangle.Width / ClientRectangle.Height, 0.5f, 5);
                    GL.UniformMatrix4(uProjectionLocation, true, ref projection);
                }
            }

        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
           // GL.UseProgram(mShader.ShaderProgramID);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            int uModelLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.BindVertexArray(mVAO_ID);
            for (int x = -5; x < 5; ++x)
            {
                for (int y = -5; y < 5; ++y)
                {
                    for (int z = -5; z < 5; ++z)
                    {
                        Matrix4 m1 = Matrix4.CreateTranslation((float)x/2, (float)y/2, (float)z/2);
                        GL.UniformMatrix4(uModelLocation, true, ref m1);
                        GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);
                    }
                }
            }




            //Matrix4 m1 = Matrix4.CreateTranslation(1, 0, 0);
            //Matrix4 m1 = Matrix4.Identity;
           // m1 = m1 * Matrix4.CreateRotationZ(0.8f);
           // GL.BindVertexArray(mVAO_ID);
           // GL.UniformMatrix4(uModelLocation, true, ref m1);
          //  GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);
            //m1 = Matrix4.Identity;
           // m1 = m1 * Matrix4.CreateRotationZ(0.8f);
            //GL.UniformMatrix4(uModelLocation, true, ref m1);
           // GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);
            this.SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.DeleteBuffers(mVBO_IDs.Length, mVBO_IDs);
            GL.DeleteVertexArray(mVAO_ID);
            mShader.Delete();
            base.OnUnload(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            int migrateX = 0;
            int migrateY = 0;
            int migrateZ = 0;
            float crX=0.0f, crY=0.0f, crZ=0.0f;
            switch(e.KeyChar)  
            {
                case 'a':
                    migrateX = -1;
                    break;
                case 'd':
                    migrateX = 1;
                    break;
                case 'w':
                    migrateZ = 1;
                    break;
                case 's':
                    migrateZ = -1;
                    break;
                case 'q':
                    migrateY = 1;
                    break;
                case 'e':
                    migrateY = -1;
                    break;
            }
            switch (e.KeyChar)
            {
                case 'r':
                    crY = -0.1f;
                    break;
                case 't':
                    crY = 0.1f;
                    break;
                case 'f':
                    crZ = 0.1f;
                    break;
                case 'g':
                    crZ = -0.1f;
                    break;
                case 'v':
                    crX = 0.1f;
                    break;
                case 'b':
                    crX = -0.1f;
                    break;
            }
            cameraMove2D(migrateX, migrateY ,migrateZ, crX, crY, crZ);
        }
        void cameraMove2D(int x, int y, int z, float crX, float crY, float crZ)
        {
            float camSpeed = 0.1f;
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            //mView = mView * Matrix4.CreateTranslation(x * camSpeed, y * camSpeed, 0);
            mView = mView * Matrix4.CreateTranslation(x*camSpeed, y*camSpeed ,z*camSpeed);
            mView = mView * Matrix4.CreateRotationX(crX);
            mView = mView * Matrix4.CreateRotationY(crY);
            mView = mView * Matrix4.CreateRotationZ(crZ);
            uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);
        }
    }
}
