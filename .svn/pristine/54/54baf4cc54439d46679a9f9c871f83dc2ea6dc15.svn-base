using System;
using Labs.Utility;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Labs.Lab4
{
    public class Lab4_1Window : GameWindow
    {
        private int[] mVertexArrayObjectIDArray = new int[2];
        private int[] mVertexBufferObjectIDArray = new int[2];
        private ShaderUtility mShader;
        private Matrix4 mSquareMatrix, mSquareMatrix2;
        private Vector3[] mCirclePosition = new Vector3[2];
        private Vector3[] mCirclePreviousPos = new Vector3[2];
        private Vector3[] mCircleVelocity = new Vector3[2];
        private float[] mCircleRadius = new float[2];
        private Timer mTimer;
        private float[] TimeSinceLastCollision = { 20, 20 };

        private Vector3 colLine;

        public Lab4_1Window()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Lab 4_1 Simple Animation and Collision Detection",
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
            GL.ClearColor(Color4.AliceBlue);

            mShader = new ShaderUtility(@"Lab4/Shaders/vLab4.vert", @"Lab4/Shaders/fLab4.frag");
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            GL.UseProgram(mShader.ShaderProgramID);

            float[] vertices = new float[] 
            { 
                   -1f, -1f,
                   1f, -1f,
                   1f, 1f,
                   -1f, 1f
            };

            mTimer = new Timer(); 
            mTimer.Start();
            mCircleVelocity[0] = new Vector3(4f, 0, 0);
            mCircleRadius[0] = 0.1f;
            mCircleVelocity[1] = new Vector3(1f, 0, 0);
            mCircleRadius[1] = 0.1f;

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

            mSquareMatrix = Matrix4.CreateScale(3f, 2f, 1f) * Matrix4.CreateRotationZ(0.5f) * Matrix4.CreateTranslation(0.5f, 0.5f, 0);
            mSquareMatrix2 = Matrix4.CreateScale(1f) * Matrix4.CreateRotationZ(0.4f) * Matrix4.CreateTranslation(0.0f, 0.0f, 0);
            mCirclePosition[0].X = -2;
            mCirclePosition[0].Y = 1f;

            mCirclePosition[1].X = 2;
            mCirclePosition[1].Y = 1f;
            base.OnLoad(e);
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

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(this.ClientRectangle);
            SetCamera();
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            Vector3 Point1 = new Vector3(-1, 1, 0);
            Vector3 Point2 = new Vector3(-1, -1, 0);
            Vector3 Point3 = new Vector3(1, 1, 0);
            Vector3 Point4 = new Vector3(1, -1, 0);
           
            base.OnUpdateFrame(e);

            float timestep = mTimer.GetElapsedSeconds();

            //Essentially, for each physics object (ball) check collsions.
            //Note to self, this solution is not scallable. Improve it.
            for (int i = 0; i < 2; i++)
            {
                //Pos set.
                mCirclePreviousPos[i] = mCirclePosition[i];
                mCirclePosition[i] = mCirclePosition[i] + mCircleVelocity[i] * timestep;

                //Translate for square space.
                Vector3 circleInSquareSpace = Vector3.Transform(mCirclePosition[i], mSquareMatrix.Inverted());

                //Check Boundary collisions.
                if (circleInSquareSpace.X > (1 - (mCircleRadius[i] / mSquareMatrix.ExtractScale().X)) || circleInSquareSpace.X < -(1 - (mCircleRadius[i] / mSquareMatrix.ExtractScale().X)))
                {
                    mCirclePosition[i] = mCirclePreviousPos[i];
                    Vector3 normal = Vector3.Transform(mCircleVelocity[i].Normalized(), mSquareMatrix.ExtractRotation());
                    mCircleVelocity[i] = mCircleVelocity[i] - 2 * Vector3.Dot(normal, mCircleVelocity[i]) * normal;
                }
                if (circleInSquareSpace.Y > (1 - (mCircleRadius[i] / mSquareMatrix.ExtractScale().Y)) || circleInSquareSpace.Y < -(1 - (mCircleRadius[i] / mSquareMatrix.ExtractScale().Y)))
                {
                    mCirclePosition[i] = mCirclePreviousPos[i];
                    Vector3 normal = Vector3.Transform(mCircleVelocity[i].Normalized(), mSquareMatrix.ExtractRotation());
                    mCircleVelocity[i] = mCircleVelocity[i] - 2 * Vector3.Dot(normal, mCircleVelocity[i]) * normal;
                }

                //Check for line collisions. (doesn't check for further line collisions if one passed)
                //Note it would be a good idea to better encapsulate the object data. (group all sphere data together, and all square data)
                //...would make this type of thing easier. And would allow this to be scalled beyond a single square. (think ACW)
                
                if (!LineCollision(i, ref Point1, ref Point2))
                {
                    if (!LineCollision(i, ref Point1, ref Point3))
                    {
                        if (!LineCollision(i, ref Point3, ref Point4))
                        {
                            LineCollision(i, ref Point2, ref Point4);
                        }
                    }
                }
                CornerCollision(i, ref Point1);
                CornerCollision(i, ref Point2);
                CornerCollision(i, ref Point3);
                CornerCollision(i, ref Point4);
            }
            ballCollision(); //Do the two circles interact with each other?
        }

        private bool LineCollision(int circleID, ref Vector3 Point1, ref Vector3 Point2)
        {
            float modifierPercentage = mCircleRadius[circleID] + mCircleRadius[circleID] / 2;

            Vector3 vPoint1 = Vector3.Transform(Point1, mSquareMatrix2);
            Vector3 vPoint2 = Vector3.Transform(Point2, mSquareMatrix2);

            Vector3 vP2toCentre = mCirclePosition[circleID] - vPoint2;
            Vector3 vP2P1_normal = (vPoint1 - vPoint2).Normalized();
            Vector3 adjSide = Vector3.Dot(vP2toCentre, vP2P1_normal) * vP2P1_normal;
            float len = adjSide.Length;

            //If normal is pointing in the oposite direction to line, then invert len range check.
            //If len is within a finite range, do collision checks.

            Vector3 vCentreToEdge = vPoint2 + (adjSide - mCirclePosition[circleID]);
            if (vCentreToEdge.Length < mCircleRadius[circleID] || vCentreToEdge.Length < 0) //(vCentreToEdge.Length < mCircleRadius[circleID])
            {
                if ((len > 0 && adjSide.Normalized() == vP2P1_normal) && len < (vPoint1 - vPoint2).Length) //(vCentreToEdge.Length < mCircleRadius[circleID])
                {
                    Vector3 iPoint = vPoint2 + adjSide;
                    //Extrapolate position correction.

                    mCirclePosition[circleID] = iPoint + iPoint.Normalized() * modifierPercentage; 
                    Vector3 normal = mCirclePosition[circleID] - vCentreToEdge;
                    normal = normal.Normalized();
                    colLine = Vector3.Transform(iPoint, mSquareMatrix2.Inverted());

                    mCircleVelocity[circleID] = mCircleVelocity[circleID] - 2 * Vector3.Dot(normal, mCircleVelocity[circleID]) * normal;
                    //mCircleVelocity[circleID] = new Vector3(0,0,0);
                    return true;
                }
            }
            return false;
        }

        private bool CornerCollision(int circleID, ref Vector3 vCorner)
        {
            Vector3 corner= Vector3.Transform(vCorner, mSquareMatrix2);
            float modifierPercentage = mCircleRadius[circleID] + mCircleRadius[circleID] / 2;
            if ((corner - mCirclePosition[circleID]).Length < mCircleRadius[circleID])
            {
                Console.WriteLine(circleID + " Corner Collide");
                mCirclePosition[circleID] = mCirclePreviousPos[circleID];
                Vector3 normal = mCircleVelocity[circleID].Normalized(); //cheat for now. Maybe use the corner in future, once that is well... More easily ascertainable.
                colLine = vCorner;
                mCirclePosition[circleID] = corner + -normal * modifierPercentage;
                mCircleVelocity[circleID] = mCircleVelocity[circleID] - 2 * Vector3.Dot(normal, mCircleVelocity[circleID]) * normal;
                return true;
            }
            return false;
        }
        private bool ballCollision() 
        {
            Vector3 tVec = (mCirclePosition[0] - mCirclePosition[1]);
            if (tVec.Length < mCircleRadius[0] + mCircleRadius[1])
            {
                tVec = (mCirclePreviousPos[0] - mCirclePreviousPos[1]);
                Vector3 normal = tVec.Normalized();
                mCirclePosition[0] = mCirclePreviousPos[0];
                mCirclePosition[1] = mCirclePreviousPos[1];
                mCircleVelocity[0] = mCircleVelocity[0] - 2 * Vector3.Dot(normal, mCircleVelocity[0]) * normal;
                mCircleVelocity[1] = mCircleVelocity[1] - 2 * Vector3.Dot(-normal, mCircleVelocity[1]) * -normal;
                Console.WriteLine("Ball Collide");
                return true;
            }
            return false;
        }

        //private bool BoundaryCollision() 
        //{ 
        
        //}

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

            GL.UniformMatrix4(uModelMatrixLocation, true, ref mSquareMatrix2);
            GL.BindVertexArray(mVertexArrayObjectIDArray[0]);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 4);

            GL.UniformMatrix4(uModelMatrixLocation, true, ref mSquareMatrix2);
            GL.Uniform4(uColourLocation, Color4.Orange);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(colLine);
            //GL.Vertex3((-colLine) + (colLine).Normalized() * 2);
            GL.End();

            Matrix4[] circleMatrix = new Matrix4[2];

            for (int i = 0; i < 2; i++)
            {
                GL.Uniform4(uColourLocation, Color4.DodgerBlue);
                circleMatrix[i] = Matrix4.CreateScale(mCircleRadius[i]) * Matrix4.CreateTranslation(mCirclePosition[i]);
                GL.UniformMatrix4(uModelMatrixLocation, true, ref circleMatrix[i]);
                GL.BindVertexArray(mVertexArrayObjectIDArray[1]);
                GL.DrawArrays(PrimitiveType.LineLoop, 0, 100);

                GL.Uniform4(uColourLocation, Color4.Green);
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(0, 0, 0);
                mCircleVelocity[i].Normalized();
                GL.Vertex3(mCircleVelocity[i].Normalized()*5);
                GL.End(); 
            }

             


            GL.Uniform4(uColourLocation, Color4.Red);
            Matrix4 m = mSquareMatrix * mSquareMatrix.Inverted();
            GL.UniformMatrix4(uModelMatrixLocation, true, ref m);
            GL.BindVertexArray(mVertexArrayObjectIDArray[0]); 
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 4);

            m = mSquareMatrix2 * mSquareMatrix.Inverted();
            GL.UniformMatrix4(uModelMatrixLocation, true, ref m);
            GL.BindVertexArray(mVertexArrayObjectIDArray[0]);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 4);

            for (int i = 0; i < 2; i++)
            {
                m = (Matrix4.CreateScale(mCircleRadius[i]) * Matrix4.CreateTranslation(mCirclePosition[i])) * mSquareMatrix.Inverted();
                GL.UniformMatrix4(uModelMatrixLocation, true, ref m);
                GL.BindVertexArray(mVertexArrayObjectIDArray[1]);
                GL.DrawArrays(PrimitiveType.LineLoop, 0, 100);

                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(0, 0, 0);
                mCircleVelocity[i].Normalized();
                GL.Vertex3(mCircleVelocity[i].Normalized() * 5);
                GL.End();
            }

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