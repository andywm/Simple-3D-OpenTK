using Labs.Utility;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
namespace Labs.Lab3
{
    public class Lab3Window : GameWindow
    {
        public Lab3Window()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Lab 3 Lighting and Material Properties",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                3, // major
                3, // minor
                GraphicsContextFlags.ForwardCompatible
                )
        {
        }

        private int[] mVBO_IDs = new int[5];
        private int[] mVAO_IDs = new int[3];
        private ShaderUtility mShader;
        //private ModelUtility mSphereModelUtility;
        private ModelUtility mPedestalModelUtility;
        private ModelUtility mSupriseModelUtility;
        private Matrix4 mView, mGroundModel, mPedestal, mSupriseModel; //mSphereModel
        private Vector4[] lightOrigins = { new Vector4(1, -3, -12, 1), new Vector4(-1, -3, -12, 1), new Vector4(0, -3, -11, 1) };
        private Vector4[] lightPosition = new Vector4[3];
        int uAmbientLightLocation, uDiffuseLightLocation, uSpecularLightLocation, uAmbientLightReflect, uDiffuseLightReflect, uSpecularLightReflect, uShine;
        int[] uLightPositionLocation = new int[3];
        int uEyePos;

        protected override void OnLoad(EventArgs e)
        {
            // Set some GL state
            GL.ClearColor(Color4.White);
            GL.Enable(EnableCap.CullFace);

            //Shader Initialisation/Lighting
            mShader = new ShaderUtility(@"Lab3/Shaders/vLighting.vert", @"Lab3/Shaders/fLighting.frag");
            GL.UseProgram(mShader.ShaderProgramID);

            mView = Matrix4.CreateTranslation(0, -5, 2);

            Vector3[] colour = { new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1) };
            
            for (int i = 0; i < 3; i++)
            {
                uLightPositionLocation[i] = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[" + i + "].Position");
                //Vector3 colour = new Vector3(0.1f, 0.0f, 0.0f);
                lightPosition[i] = lightOrigins[i];// new Vector4(2+ i, 4 - i, -8.5f, 1);
                lightPosition[i] = Vector4.Transform(lightPosition[i], mView);
                GL.Uniform4(uLightPositionLocation[i], lightPosition[i]);

                uAmbientLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[" + i + "].AmbientLight");
                GL.Uniform3(uAmbientLightLocation, colour[i]/10);
                uDiffuseLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[" + i + "].DiffuseLight");
                GL.Uniform3(uDiffuseLightLocation, colour[i]);
                uSpecularLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[" + i + "].SpecularLight");
                GL.Uniform3(uSpecularLightLocation, colour[i]);
            }
            uAmbientLightReflect = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.AmbientReflectivity");
            Vector3 reflection = new Vector3(0.3f, 0.3f, 0.3f);
            GL.Uniform3(uAmbientLightReflect, reflection);
            uDiffuseLightReflect = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.DiffuseReflectivity");
            GL.Uniform3(uDiffuseLightReflect, reflection);
            uSpecularLightReflect = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.SpecularReflectivity");
            GL.Uniform3(uSpecularLightReflect, reflection);
            uShine = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.Shininess");
            GL.Uniform1(uShine, 0.2f);

            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            int vNormal = GL.GetAttribLocation(mShader.ShaderProgramID, "vNormal");
            uEyePos = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition"); 
            Console.WriteLine(mView);
            GL.Uniform4(uEyePos, mView.Row3);

            GL.GenVertexArrays(mVAO_IDs.Length, mVAO_IDs);
            GL.GenBuffers(mVBO_IDs.Length, mVBO_IDs);
            //Ground verticies?
            float[] vertices = new float[] {-10, 0, -10,0,1,0,
                                             -10, 0, 10,0,1,0,
                                             10, 0, 10,0,1,0,
                                             10, 0, -10,0,1,0,};
            //Ground...
            GL.BindVertexArray(mVAO_IDs[0]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            int size;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.EnableVertexAttribArray(vNormal);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.VertexAttribPointer(vNormal, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));
            /*
            mSphereModelUtility = ModelUtility.LoadModel(@"Utility/Models/sphere.bin"); 

            GL.BindVertexArray(mVAO_IDs[1]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[1]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mSphereModelUtility.Vertices.Length * sizeof(float)), mSphereModelUtility.Vertices, BufferUsageHint.StaticDraw);           
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[2]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mSphereModelUtility.Indices.Length * sizeof(float)), mSphereModelUtility.Indices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mSphereModelUtility.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mSphereModelUtility.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.EnableVertexAttribArray(vNormal);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.VertexAttribPointer(vNormal, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));
            */
            //PEDESTAL
            mPedestalModelUtility = ModelUtility.LoadModel(@"Utility/Models/cylinder.bin"); 

            GL.BindVertexArray(mVAO_IDs[1]);
            //Model Verticies
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[1]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mPedestalModelUtility.Vertices.Length * sizeof(float)), mPedestalModelUtility.Vertices, BufferUsageHint.StaticDraw);       
            //Model Indicies
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[2]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mPedestalModelUtility.Indices.Length * sizeof(float)), mPedestalModelUtility.Indices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mPedestalModelUtility.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mPedestalModelUtility.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.EnableVertexAttribArray(vNormal);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0); //Read in 3 from 0 of 6
            GL.VertexAttribPointer(vNormal, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));//Read in 3 from 3 of 6
            //MODEL
            mSupriseModelUtility = ModelUtility.LoadModel(@"Utility/Models/model.bin");

            GL.BindVertexArray(mVAO_IDs[2]);
            //Model Verticies
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[3]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mSupriseModelUtility.Vertices.Length * sizeof(float)), mSupriseModelUtility.Vertices, BufferUsageHint.StaticDraw);
            //Model Indicies
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[4]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mSupriseModelUtility.Indices.Length * sizeof(float)), mSupriseModelUtility.Indices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mSupriseModelUtility.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mSupriseModelUtility.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.EnableVertexAttribArray(vNormal);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0); //Read in 3 from 0 of 6
            GL.VertexAttribPointer(vNormal, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));//Read in 3 from 3 of 6


            GL.BindVertexArray(0);    
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);

            mGroundModel = Matrix4.CreateTranslation(0, 0, -5f);
            //mSphereModel = Matrix4.CreateTranslation(0, 1, -5f);        
            mPedestal = Matrix4.CreateTranslation(0, 1, -5f); //X.Y.Z
            mSupriseModel = Matrix4.CreateTranslation(0, 2, -5f);

            base.OnLoad(e);
            
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(this.ClientRectangle);
            if (mShader != null)
            {
                int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
                Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, (float)ClientRectangle.Width / ClientRectangle.Height, 0.5f, 25);
                GL.UniformMatrix4(uProjectionLocation, true, ref projection);
            }
        }

        public void Camera()
        { 
        
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            int uView;
            char key = e.KeyChar;
            uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            //if(key == 'w' || key == 'a' || key == 's' || key == 'd')
            {
                switch(key)
                {
                    case 'w':
                        mView = mView * Matrix4.CreateTranslation(0.0f, 0.0f, 0.05f);
                        break;
                    case 's':
                        mView = mView * Matrix4.CreateTranslation(0.0f, 0.0f, -0.05f);
                        break;
                    case 'a':
                        mView = mView * Matrix4.CreateRotationY(-0.025f);
                        break;
                    case 'd':
                        mView = mView * Matrix4.CreateRotationY(0.025f);
                        break;
                }
                for (int i = 0; i < 3; i++)
                {
                    lightPosition[i] = Vector4.Transform(lightOrigins[i], mView); //replace lightPos with a fixed start value
                    GL.Uniform4(uLightPositionLocation[i], lightPosition[i]);
                }
            }
            GL.UniformMatrix4(uView, true, ref mView);
            //GL.Uniform4(uLightPositionLocation, lightPosition);
            GL.Uniform4(uEyePos, mView.Row3);
            Console.Clear();
            Vector3 t = mGroundModel.ExtractTranslation();
            //Vector3 st = mSphereModel.ExtractTranslation();
            Vector3 cyT = mPedestal.ExtractTranslation();
            Vector3 mdT = mSupriseModel.ExtractTranslation();
            if (e.KeyChar == 'z')
            { 
                    Matrix4 translation = Matrix4.CreateTranslation(t);
                    Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                    mGroundModel = mGroundModel * inverseTranslation * Matrix4.CreateRotationY(-0.025f) * translation;
            }
            else if(e.KeyChar == 'x')
            {
                    Matrix4 translation = Matrix4.CreateTranslation(t);
                    Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t); 
                    mGroundModel = mGroundModel * inverseTranslation * Matrix4.CreateRotationY(0.025f) * translation;
            }
            if (e.KeyChar == 'c')
            {
                Matrix4 translation = Matrix4.CreateTranslation(cyT);
                Matrix4 inverseTranslation = Matrix4.CreateTranslation(-cyT);
                //mSphereModel = mSphereModel * inverseTranslation * Matrix4.CreateRotationY(-0.025f) * translation;
                mPedestal = mPedestal * inverseTranslation * Matrix4.CreateRotationY(-0.025f) * translation;
               // translation = Matrix4.CreateTranslation(mdT);
                //inverseTranslation = Matrix4.CreateTranslation(-mdT);
                mSupriseModel = mSupriseModel * inverseTranslation * Matrix4.CreateRotationY(-0.025f) * translation;
            }
            else if (e.KeyChar == 'v')
            {
                Matrix4 translation = Matrix4.CreateTranslation(cyT);
                Matrix4 inverseTranslation = Matrix4.CreateTranslation(-cyT);
                //mSphereModel = mSphereModel * inverseTranslation * Matrix4.CreateRotationY(0.025f) * translation;
                mPedestal = mPedestal * inverseTranslation * Matrix4.CreateRotationY(-0.025f) * translation;
                //translation = Matrix4.CreateTranslation(mdT);
                //inverseTranslation = Matrix4.CreateTranslation(-mdT);
                mSupriseModel = mSupriseModel * inverseTranslation * Matrix4.CreateRotationY(0.025f) * translation;
            }
         
        }
        public void set_Brass(Vector3 col)
        {
           // GL.Uniform3(uAmbientLightLocation, col);
           // GL.Uniform3(uDiffuseLightLocation, col);
            //GL.Uniform3(uSpecularLightLocation, col);

            GL.Uniform3(uAmbientLightReflect, new Vector3(0.329412f,0.223529f,0.027451f));
            GL.Uniform3(uDiffuseLightReflect, new Vector3(0.780392f,0.568627f,0.113725f));
            GL.Uniform3(uSpecularLightReflect, new Vector3(0.992157f,0.94176f,0.807843f));
            GL.Uniform1(uShine, (0.21794872f * 128));
        }
        public void set_Default()
        {
            Vector3 col = new Vector3(0.5f, 0.6f, 0.6f);
            //GL.Uniform3(uAmbientLightLocation, col);
            //GL.Uniform3(uDiffuseLightLocation, col);
            //GL.Uniform3(uSpecularLightLocation, col);


            Vector3 reflect = new Vector3(0.3f, 0.3f, 0.3f);
            GL.Uniform3(uAmbientLightReflect, reflect);
            GL.Uniform3(uDiffuseLightReflect, reflect);
            GL.Uniform3(uSpecularLightReflect, reflect);
            GL.Uniform1(uShine, 0.2f);
        }
        public void set_White_Plastic()
        {
            Vector3 col = new Vector3(1f, 1f, 1f);
          //  GL.Uniform3(uAmbientLightLocation, col);
            //GL.Uniform3(uDiffuseLightLocation, col);
           // GL.Uniform3(uSpecularLightLocation, col);

            GL.Uniform3(uAmbientLightReflect, new Vector3(0.1f, 0.1f, 0.1f));
            GL.Uniform3(uDiffuseLightReflect, new Vector3(0.55f, 0.55f, 0.55f));
            GL.Uniform3(uSpecularLightReflect, new Vector3(0.7f, 0.7f, 0.7f));
            GL.Uniform1(uShine, 0.25f);
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);

            int uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref mGroundModel);  

            GL.BindVertexArray(mVAO_IDs[0]);
            Vector3 colour = new Vector3(0f, 0.6f, 1.0f);
            Vector3 reflection = new Vector3(0.3f, 0.3f, 0.3f);
            set_Default();

            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);

            //Matrix4 m = mSphereModel * mGroundModel;
            Matrix4 m = mPedestal * mGroundModel;
            uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref m);

            GL.BindVertexArray(mVAO_IDs[1]);
            GL.DrawElements(PrimitiveType.Triangles, mPedestalModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);
            m = mSupriseModel * mGroundModel;
            GL.UniformMatrix4(uModel, true, ref m);

            set_Brass(new Vector3(0.4f,0.4f,0.4f));

            GL.BindVertexArray(mVAO_IDs[2]);
            GL.DrawElements(PrimitiveType.Triangles, mSupriseModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);

            
            GL.BindVertexArray(0);
            this.SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.DeleteBuffers(mVBO_IDs.Length, mVBO_IDs);
            GL.DeleteVertexArrays(mVAO_IDs.Length, mVAO_IDs);
            mShader.Delete();
            base.OnUnload(e);
        }
    }
}
