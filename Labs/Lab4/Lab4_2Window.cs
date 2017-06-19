using System;
using Labs.Utility;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Labs.Lab4
{
    public class Lab4_2Window : GameWindow
    {
        private int[] mVertexArrayObjectIDArray = new int[2];
        private int[] mVertexBufferObjectIDArray = new int[2];
        private ShaderUtility mShader;
        private Matrix4 mSquareMatrix;
        private Vector3 mCirclePosition, mCirclePosition2;
        private Vector3 mCircleVelocity, mCircleVelocity2;
        Vector3 oldPosition1, oldPosition2 = new Vector3(0,0,0); 
        private float mCircleRadius, mCircleRadius2;
        float density1, density2;
        float volume1, volume2;
        float mass1, mass2;
        float e_ = 0.5f;
        private Timer mTimer;
        Vector3 accelerationDueToGravity = new Vector3(0, -9.81f, 0);
        public Lab4_2Window()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Lab 4_2 Physically Based Simulation",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                3, // major
                3, // minor
                GraphicsContextFlags.ForwardCompatible
                )
        {
        }
        bool fail = false;
        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(Color4.AliceBlue);

            mShader = new ShaderUtility(@"Lab4/Shaders/vLab4.vert", @"Lab4/Shaders/fLab4.frag");
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            GL.UseProgram(mShader.ShaderProgramID);

            float[] vertices = new float[] { 
                   -1f, -1f,
                   1f, -1f,
                   1f, 1f,
                   -1f, 1f
            };

            GL.GenVertexArrays(mVertexArrayObjectIDArray.Length, mVertexArrayObjectIDArray);
            GL.GenBuffers(mVertexBufferObjectIDArray.Length, mVertexBufferObjectIDArray);

            GL.BindVertexArray(mVertexArrayObjectIDArray[0]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBufferObjectIDArray[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            int size;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            if (vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

            vertices = new float[200];

            for (int i = 0; i < 100; ++i)
            {
                vertices[2 * i] = (float)Math.Cos(MathHelper.DegreesToRadians(i * 360.0 / 100));
                vertices[2 * i + 1] = (float)Math.Cos(MathHelper.DegreesToRadians(90.0 + i * 360.0 / 100));
            }

            GL.BindVertexArray(mVertexArrayObjectIDArray[1]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBufferObjectIDArray[1]);

            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            if (vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

            int uViewLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            Matrix4 m = Matrix4.CreateTranslation(0, 0, 0);
            GL.UniformMatrix4(uViewLocation, true, ref m);

            mCircleRadius = 0.2f;
            mCircleRadius2 = 0.252f;

            mCirclePosition = new Vector3(-2.5f, 2f, 0);
            mCircleVelocity = new Vector3(1, 0, 0);
            mCirclePosition2 = new Vector3(0, 2, 0);
            mCircleVelocity2 = new Vector3(-1, 0, 0);

            density1 = 7.8f;
            density2 = 7.8f;

            volume1 = (float)((4.0 / 3.0) * Math.PI * Math.Pow(mCircleRadius,3));
            volume2 = (float)((4.0 / 3.0) * Math.PI * Math.Pow(mCircleRadius2, 3));
            mass1 = density1 * volume1;
            mass2 = density2 * volume2;
            mSquareMatrix = Matrix4.CreateScale(4f) * Matrix4.CreateRotationZ(0.0f) * Matrix4.CreateTranslation(0, 0, 0);
            

            base.OnLoad(e);

            mTimer = new Timer();
            mTimer.Start();
        }

        private void SetCamera()
        {
            float height = ClientRectangle.Height;
            float width = ClientRectangle.Width;
            if (mShader != null)
            {
                Matrix4 proj;
                if (height > width)
                {
                    if (width == 0)
                    {
                        width = 1;
                    }
                    proj = Matrix4.CreateOrthographic(10, 10 * height / width, 0, 10);
                }
                else
                {
                    if (height == 0)
                    {
                        height = 1;
                    }
                    proj = Matrix4.CreateOrthographic(10 * width / height, 10, 0, 10);
                }
                int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
                GL.UniformMatrix4(uProjectionLocation, true, ref proj);
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            float timestep = mTimer.GetElapsedSeconds();
           
            base.OnUpdateFrame(e);


            //Essentially, for each physics object (ball) check collsions.
            //Note to self, this solution is not scallable. Improve it.

            BoundaryCheck(ref mCirclePosition, ref mCircleVelocity, timestep ,ref oldPosition1, true);
            BoundaryCheck(ref mCirclePosition2, ref mCircleVelocity2, timestep ,ref oldPosition2, false);
            if (Math.Abs((mCirclePosition - mCirclePosition2).Length) < mCircleRadius + mCircleRadius2 && !fail)
            {
                {
                    mCirclePosition = oldPosition1;
                    mCirclePosition2 = oldPosition2;

                    Vector3 normal = mCirclePosition2 - mCirclePosition;
                    normal.Normalize();

                    Console.WriteLine("Original Velocities: " + mCircleVelocity.Length + ":" + mCircleVelocity2.Length);

                    float SumMass = mass1+mass2;
                    float M1negM2 = mass1-mass2;
                    float M2negM1 = mass2 -mass1;
                    float x2Mass1 = 2*mass1;
                    float x2Mass2 = 2*mass2;


                    Vector3 velocityInDirectionOfCollision_C1 = Vector3.Dot(mCircleVelocity, normal) * normal;// +((x2Mass2 / SumMass) * Vector3.Dot(mCircleVelocity, -normal) * -normal);
                    Vector3 velocityInDirectionOfCollision_C2 = Vector3.Dot(mCircleVelocity2, -normal) * -normal;// + ((x2Mass1/SumMass) * Vector3.Dot(mCircleVelocity, normal) * normal);

                    
                    Vector3 adjustedC3 = ((mass1 * velocityInDirectionOfCollision_C1) + (mass2 * velocityInDirectionOfCollision_C2) + e_* (mass2 *(velocityInDirectionOfCollision_C2 - velocityInDirectionOfCollision_C1))) / SumMass;
                    Vector3 adjustedC4 = ((mass1 * velocityInDirectionOfCollision_C1) + (mass2 * velocityInDirectionOfCollision_C2) + e_*(mass1 * (velocityInDirectionOfCollision_C1 - velocityInDirectionOfCollision_C2))) / SumMass;

                    //Vector3 adjustedC1 = ((M1negM2 / SumMass) * velocityInDirectionOfCollision_C1) + ((x2Mass2 / SumMass) * velocityInDirectionOfCollision_C2);
                    //Vector3 adjustedC2 = ((M2negM1 / SumMass) * velocityInDirectionOfCollision_C2) + ((x2Mass1 / SumMass) * velocityInDirectionOfCollision_C1);

                    Vector3 velocityPerpendicularToCollision_C1 = mCircleVelocity - velocityInDirectionOfCollision_C1;
                    Vector3 velocityPerpendicularToCollision_C2 = mCircleVelocity2 - velocityInDirectionOfCollision_C2;

                    mCircleVelocity = velocityPerpendicularToCollision_C1 + adjustedC3;
                    mCircleVelocity2 = velocityPerpendicularToCollision_C2 + adjustedC4;

                    Console.WriteLine("New Velocities: " + mCircleVelocity.Length + ":" + mCircleVelocity2.Length);
                    Console.WriteLine("Momentum in System: " + (mass1 + mass2) * (mCircleVelocity.Length + mCircleVelocity2.Length));
                } 
            }

        }

        private void BoundaryCheck(ref Vector3 CirclePosition, ref Vector3 CircleVelocity, float timestep ,ref Vector3 oldPosition, bool after)
        {
            oldPosition = CirclePosition;
            if (after)
            {
                CirclePosition = CirclePosition + CircleVelocity * timestep;
                CircleVelocity = CircleVelocity + accelerationDueToGravity * timestep;
            }
            else
            {
                CircleVelocity = CircleVelocity + accelerationDueToGravity * timestep;
                CirclePosition = CirclePosition + CircleVelocity * timestep;  
            }


            Vector3 Point1 = new Vector3(-1, 1, 0);
            Vector3 Point2 = new Vector3(-1, -1, 0);
            Vector3 Point3 = new Vector3(1, 1, 0);
            Vector3 Point4 = new Vector3(1, -1, 0);         

            //Translate for square space.
            Vector3 circleInSquareSpace = Vector3.Transform(CirclePosition, mSquareMatrix.Inverted());

            //Check Boundary collisions.
            if (circleInSquareSpace.X > (1 - (mCircleRadius / mSquareMatrix.ExtractScale().X)) || circleInSquareSpace.X < -(1 - (mCircleRadius / mSquareMatrix.ExtractScale().X)))
            {
                CirclePosition = oldPosition;
                Vector3 normal = Vector3.Transform(CircleVelocity.Normalized(), mSquareMatrix.ExtractRotation());
                CircleVelocity = CircleVelocity - (1+e_) * Vector3.Dot(normal, CircleVelocity) * normal;
            }
            if (circleInSquareSpace.Y > (1 - (mCircleRadius / mSquareMatrix.ExtractScale().Y)) || circleInSquareSpace.Y < -(1 - (mCircleRadius / mSquareMatrix.ExtractScale().Y)))
            {
                CirclePosition = oldPosition;
                Vector3 normal = Vector3.Transform(CircleVelocity.Normalized(), mSquareMatrix.ExtractRotation());
                CircleVelocity = CircleVelocity - (1+e_) * Vector3.Dot(normal, CircleVelocity) * normal;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(this.ClientRectangle);
            SetCamera();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            int uModelMatrixLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            int uColourLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uColour");

            GL.Uniform4(uColourLocation, Color4.DodgerBlue);

            GL.UniformMatrix4(uModelMatrixLocation, true, ref mSquareMatrix);
            GL.BindVertexArray(mVertexArrayObjectIDArray[0]);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 4);

            Matrix4 circleMatrix = Matrix4.CreateScale(mCircleRadius) * Matrix4.CreateTranslation(mCirclePosition);

            GL.UniformMatrix4(uModelMatrixLocation, true, ref circleMatrix);
            GL.BindVertexArray(mVertexArrayObjectIDArray[1]);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 100);

            GL.Uniform4(uColourLocation, Color4.Red);

            Matrix4 circleMatrix2 = Matrix4.CreateScale(mCircleRadius2) * Matrix4.CreateTranslation(mCirclePosition2);

            GL.UniformMatrix4(uModelMatrixLocation, true, ref circleMatrix2);
            GL.BindVertexArray(mVertexArrayObjectIDArray[1]);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 100);

            this.SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            GL.DeleteBuffers(mVertexBufferObjectIDArray.Length, mVertexBufferObjectIDArray);
            GL.DeleteVertexArrays(mVertexArrayObjectIDArray.Length, mVertexArrayObjectIDArray);
            GL.UseProgram(0);
            mShader.Delete();
        }
    }
}