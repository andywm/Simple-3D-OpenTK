using System;
using System.Collections.Generic;
using Labs.Utility;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Labs.Lab4;

//100cm = Distance 10;

//Note to self: Make Camera Class. Make Light Object Class too, lighting is a little... unorganised.

namespace Labs.ACW
{
    public class ACWWindow : GameWindow
    {
        public const float Centimetre = 0.1f;
        public const int viewDistance = 125;
        public ACWWindow()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Assessed Coursework",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                3, // major
                3, // minor
                GraphicsContextFlags.ForwardCompatible
                )
        {
        }
        public Timer mTimer;
        public static ShaderUtility mShader;
        private Matrix4 mView;
        private Vector4[] lightOrigins = { new Vector4(0, -26f, -0, 1), new Vector4(0, -14, 0, 1), new Vector4(0, 4.4f, 0, 1) };
        private Vector4[] lightPosition = new Vector4[3];
        int uAmbientLightLocation, uDiffuseLightLocation, uSpecularLightLocation;
        int[] uLightPositionLocation = new int[3];
        int uEyePos;
        Camera cam;
        protected override void OnLoad(EventArgs e) 
        {
            mTimer = new Timer();
            mTimer.Start();
            // Set some GL state
            GL.ClearColor(Color4.White);
            GL.Enable(EnableCap.CullFace);
            #region mostly shader stuff, +camera matrix
            //Shader Initialisation/Lighting
            mShader = new ShaderUtility(@"ACW/Shaders/vLighting.vert", @"ACW/Shaders/fLighting.frag");
            GL.UseProgram(mShader.ShaderProgramID);
            
            mView = Matrix4.CreateTranslation(0,3.5f, -10);

            Vector3[] colour = { new Vector3(2f, 0f, 0f), new Vector3(0f, 1.5f, 0f), new Vector3(1f, 1.4f,1.5f) };
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
           
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            int vNormal = GL.GetAttribLocation(mShader.ShaderProgramID, "vNormal");

            uEyePos = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
            GL.Uniform4(uEyePos, mView.Row3);
            #endregion

            Surface.LoadSurface(ref mShader);
            Ball.LoadGeometry(ref mShader);
            Cylinder.LoadCylinder(ref mShader);
            Box.LoadSurface(ref mShader);
            GL.BindVertexArray(0);

            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);
            World.InitRegions();
            cam = new Camera(mView);
            base.OnLoad(e);
        }
        public static ShaderUtility RequestShader()
        {
            return mShader;
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(this.ClientRectangle);
            if (mShader != null)
            {
                int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
                Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, (float)ClientRectangle.Width / ClientRectangle.Height, 0.5f, viewDistance);
                GL.UniformMatrix4(uProjectionLocation, true, ref projection);
                
            }
        }

        /// <summary>
        /// User Interaction Control. 
        /// Note: If this gets any larger, start migrating functionallity.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyPress(KeyPressEventArgs e) 
        {
            base.OnKeyPress(e);
            int uView;
            char key = e.KeyChar;
            uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            //if(key == 'w' || key == 'a' || key == 's' || key == 'd')
            {
                switch (key)
                {
                    case 'w':
                        if (!cam.enableFreeCam) break;
                        mView = mView * Matrix4.CreateTranslation(0.0f, 0.0f, 0.1f);
                        break;
                    case 's':
                        if (!cam.enableFreeCam) break;
                        mView = mView * Matrix4.CreateTranslation(0.0f, 0.0f, -0.1f);
                        break;
                    case 'a':
                        if (!cam.enableFreeCam) break;
                        mView = mView * Matrix4.CreateRotationY(-0.025f);
                        break;
                    case 'd':
                        if (!cam.enableFreeCam) break;
                        mView = mView * Matrix4.CreateRotationY(0.025f);
                        break; 
                    case 'q':
                        if (!cam.enableFreeCam) break;
                        mView = mView * Matrix4.CreateTranslation(0,-1,0);
                        break;
                    case 'e':
                        if (!cam.enableFreeCam) break;
                        mView = mView * Matrix4.CreateTranslation(0, 1, 0);
                        break;
                    case '-':
                        Console.Clear();
                        Console.WriteLine("SET Restituion Coefficient: " + World.gamePhysics.setCollisionRestituionCoefficent(-1));
                        break;
                    case '+':
                        Console.Clear();
                        Console.WriteLine("SET Restituion Coefficient: " + World.gamePhysics.setCollisionRestituionCoefficent(1));
                        break;
                    case '*':
                        Console.Clear();
                        bool val = World.gamePhysics.setEulerIntegration();
                        Console.WriteLine("SET Euler Integrate" + (val == false ? " After " : " Before ") + "Position Update");
                        break;
                    case '/':
                        Console.Clear();
                        Console.WriteLine("Show Physics Data? SET " + World.gamePhysics.setShowPhysics());
                        break;
                    case 'l':
                        Console.Clear();
                        Console.WriteLine("Limitless? SET " + World.gamePhysics.setLimitless());
                        break;
                    case '1':
                        Console.Clear();
                        Console.WriteLine("SET Camera USER");
                        if (cam.hal9000)
                        {
                            Console.WriteLine("I'm sorry, I'm afraid I cannot let you do that.");
                            break;
                        }               
                        mView = cam.user();
                        break;
                    case '2':
                        Console.Clear();
                        Console.WriteLine("SET Camera Focus on Stage 1");
                        if (cam.hal9000)
                        {
                            Console.WriteLine("I'm sorry, I'm afraid I cannot let you do that.");
                            break;
                        }
                        mView = cam.fixedS1();
                        break;
                    case '3':
                        Console.Clear();
                        Console.WriteLine("SET Camera Focus on Stage 2");
                        if (cam.hal9000)
                        {
                            Console.WriteLine("I'm sorry, I'm afraid I cannot let you do that.");
                            break;
                        }
                        mView = cam.fixedS2();
                        break;
                    case '4':
                        Console.Clear();
                        Console.WriteLine("SET Camera Focus on Stage 3");
                        mView = cam.fixedDoom();
                        break;
                    case '0':
                        Console.Clear();
                        Console.WriteLine("Easter Egg? SET " + cam.setEasterEgg());
                        break;
                }

                //Maintain Point Light Location.
                for (int i = 0; i < 3; i++)
                {
                    lightPosition[i] = Vector4.Transform(lightOrigins[i], mView); //replace lightPos with a fixed start value
                    GL.Uniform4(uLightPositionLocation[i], lightPosition[i]);
                }
            }
            GL.UniformMatrix4(uView, true, ref mView);
            GL.Uniform4(uEyePos, mView.Row3);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);

            float timestep = mTimer.GetElapsedSeconds();
            World.RandomBall(timestep);
            World.gamePhysics.SimulateFrame(timestep, mShader);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);

            foreach (Ball b in SpatialRegion.OverheadController_AllBalls)
            { 
                b.Draw(ref mShader);
                b.ShowPhysicsData(ref mShader);
            }
            World.DrawStages(ref mShader);

            GL.BindVertexArray(0);
            this.SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            Surface.UnloadGeometry();
            Sphere.UnloadGeometry();
            Box.UnloadGeometry();
            Cylinder.UnloadGeometry();
            mShader.Delete();
            base.OnUnload(e);
        }
    }
}
