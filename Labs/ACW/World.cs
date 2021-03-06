﻿using System;
using System.Collections.Generic;
using Labs.Utility;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Labs.ACW
{
    //Management Class for Objects that Exist in the World.
    public static class World
    {
        public static List<SpatialRegion> Region = new List<SpatialRegion>();
        public static List<Shape> GlobalStaticObjects = new List<Shape>();
        public static List<Ball> Balls = new List<Ball>();
        public static Physics gamePhysics = new Physics();

        public static int boxesDrawn = 0;
        private static Vector3 v3;
        private static Random rand = new Random();

        public static void InitRegions()
        {
            SpatialRegion.AllRegions.Add(new EmitterRegion(ref boxesDrawn));
            SpatialRegion.AllRegions.Add(new CylinderRegion(ref boxesDrawn, CylinderRegion.CylinderArrangement.axisAligned));
            SpatialRegion.AllRegions.Add(new CylinderRegion(ref boxesDrawn, CylinderRegion.CylinderArrangement.nonAxisAligned));
            SpatialRegion.AllRegions.Add(new DoomRegion(ref boxesDrawn));
            makeCeil();
            makeFloor();
        }
        
        //Spawning System.
        static float g_time=0;
        public static void RandomBall(float time)
        {
            g_time += time;
            if (g_time > 1)
            {
                int x = rand.Next(-40, 40);
                int y = rand.Next(-40, 40);
                int col = rand.Next(1, 5);
                if (col >= 4)
                {
                    Generate_DodgerBlueBall(x, y);
                }
                else
                {
                    Generate_OrangeBall(x, y);
                }
                Console.WriteLine(x + ", " + y);
                g_time = 0;
            }
            
        }  
        public static void Generate_DodgerBlueBall(float x, float z)
        {
            float radius = 7; //cm
            float density = 0.0012f; //kg/cm^3
            v3.X = 20; v3.Y = 35; v3.Z = 0;
            if (x > -43 && x < 43)
            {
                v3.X = x;
            }
            if (z > -43 && z < 43)
            {
                v3.Z = z;
            }

            Balls.Add(new Ball(ref v3, radius, ref density, ref Aesthetics.DodgerBlue_Matte));
            Balls[Balls.Count - 1].velocity.Y = 0f;
            Balls[Balls.Count - 1].velocity.X = 0.2f;
            SpatialRegion.OverheadController_AllBalls.Add(Balls[Balls.Count - 1]);
        }
        public static void Generate_OrangeBall(float x, float z)
        {
            float radius = 5; //cm
            float density = 0.001f; //kg/cm^3
            v3.X = -40; v3.Y = 44; v3.Z = 0;
            if (x > -45 && x < 45)
            {
                v3.X = x;
            }
            if (z > -45 && z < 45)
            {
                v3.Z = z;
            }

            Balls.Add(new Ball(ref v3, radius, ref density, ref Aesthetics.Orange_Shinny));
            Balls[Balls.Count - 1].velocity.Y = 0f;
            Balls[Balls.Count - 1].velocity.X = 0.2f;
            SpatialRegion.OverheadController_AllBalls.Add(Balls[Balls.Count - 1]);
        }
        public static void makeFloor()
        {
            Vector3 v3 = new Vector3(0, (boxesDrawn*-100)+50, 0);
            float size = 50;
            GlobalStaticObjects.Add(new Surface(ref v3, ref size, ref Aesthetics.MidGrey));
        }
        public static void makeCeil()
        {
            Vector3 v3 = new Vector3(0, 50, 0);
            float size = 50;
            GlobalStaticObjects.Add(new Surface(ref v3, ref size, ref Aesthetics.MidGrey, true));
        }
        public static int numberOfBoxes()
        {
            return boxesDrawn;
        }
        //Render Static Objects.
        public static void DrawStages(ref ShaderUtility mShader)
        {
            foreach (Shape obj in GlobalStaticObjects)
            {
                obj.Draw(ref mShader);
            }
        }
    }
    //Spatial Partitioning System.
    public abstract class SpatialRegion
    {
        public static List<Ball> OverheadController_AllBalls = new List<Ball>();
        
        public static List<SpatialRegion> AllRegions = new List<SpatialRegion>();
        protected Box Boundary;
        public List<Ball> BallsInRegion = new List<Ball>();
        protected float regionCentreY;
        
        public void DoCollisionChecks()
        {
            List<Ball> RegionalSubset = new List<Ball>();
            foreach (Ball b in BallsInRegion)
            {
                RegionalSubset.Add(b);
            }
            while(RegionalSubset.Count !=0)
            {
                Ball b1 = RegionalSubset[0];
                RegionalSubset.Remove(b1);
                
                foreach(Ball b2 in RegionalSubset)
                {
                    ballByBall(b1, b2);
                    //Console.WriteLine(b1.vPosition);
                }
                ballAlone(b1); //For the last unpaired ball...

            }

        }
        protected virtual void ballByBall(Ball b1, Sphere s1)
        {
            World.gamePhysics.CheckFor_SphereSphere_Collision(b1, s1);
        }
        protected virtual void ballAlone(Ball b1)
        {
            
        }
        protected void AddBallToRegion(Ball ball)
        {
            this.BallsInRegion.Add(ball);
        }
        protected void RemBallFrRegion(Ball ball)
        {
            this.BallsInRegion.Remove(ball);
        }
        public static void SpatialPartitioningController()
        {
            foreach (Ball b in OverheadController_AllBalls)
            { 
               //Cross Region Boundary Checks and Updates.
                float vY = b.vPosition.Y;
                float rad = b.radius * ACWWindow.Centimetre;
                float upperCoord = vY + rad;
                float lowerCoord = vY - rad;
                foreach (SpatialRegion r in AllRegions)
                {
                    float regionHigh = r.regionCentreY * ACWWindow.Centimetre + 10;
                    float regionLow = r.regionCentreY * ACWWindow.Centimetre - 10;
                    if(!(b.IsInRegion(r)))
                    {
                        if ((upperCoord < regionHigh) && (lowerCoord > regionLow))//Refactor later '5'...
                        {
                            b.CollisionCheckRegion.Add(r);
                            r.AddBallToRegion(b);
                        }
                    }
                    else
                    {
                        if ((upperCoord > regionHigh) || (lowerCoord < regionLow))//Refactor later '5'...
                        {
                            b.CollisionCheckRegion.Remove(r);
                            r.RemBallFrRegion(b);
                        }
                    }
                }
            }
        }
    }
    public class EmitterRegion : SpatialRegion
    {
        public EmitterRegion(ref int boxesDrawn)
        {
            Vector3 v3 = new Vector3(0, 0, 0);
            float cubeSize = 100;
            v3.Y = boxesDrawn * -100;
            regionCentreY = v3.Y;
            boxesDrawn++;
            Vector3 rotation = new Vector3((float)Math.PI / 2, 0, 0);

            Boundary = new Box(ref v3, ref cubeSize, ref Aesthetics.MidGrey);
            World.GlobalStaticObjects.Add(Boundary);
        }
    }
    public class CylinderRegion : SpatialRegion
    {
        public enum CylinderArrangement
        { 
            axisAligned,
            nonAxisAligned
        }
        List<Cylinder> Cylinders = new List<Cylinder>();
        public CylinderRegion(ref int boxesDrawn, CylinderArrangement cr)
        {
            if (cr == CylinderArrangement.axisAligned)
            {
                MakeAxisAligned(ref boxesDrawn);
            }
            else
            {
                MakeNonAxisAligned(ref boxesDrawn);
            }
        }
        private void MakeAxisAligned(ref int boxesDrawn)
        {
            Vector3 v3 = new Vector3(0, 0, 0);
            float cubeSize = 100;
            float radius = 7.5f;
            float length = 100;

            v3.Y = boxesDrawn * -100;
            regionCentreY = v3.Y;
            boxesDrawn++;
            Vector3 rotation = new Vector3((float)Math.PI / 2, 0, 0);

            Boundary = new Box(ref v3, ref cubeSize, ref Aesthetics.MidGrey);
            World.GlobalStaticObjects.Add(Boundary);
            v3.X = 25;
            Cylinders.Add(new Cylinder(ref v3, ref radius, ref length, ref rotation, ref Aesthetics.MidGrey));
            World.GlobalStaticObjects.Add(Cylinders[Cylinders.Count-1]);
            v3.X = -25;
            Cylinders.Add(new Cylinder(ref v3, ref radius, ref length, ref rotation, ref Aesthetics.MidGrey));
            World.GlobalStaticObjects.Add(Cylinders[Cylinders.Count - 1]);
            v3.X = 0;
            rotation.Y = (float)(Math.PI / 2);//(float)Math.PI;
            v3.Z = 23;
            Cylinders.Add(new Cylinder(ref v3, ref radius, ref length, ref rotation, ref Aesthetics.MidGrey));
            World.GlobalStaticObjects.Add(Cylinders[Cylinders.Count - 1]);
            radius = 15;
            v3.Z = -17;
            Cylinders.Add(new Cylinder(ref v3, ref radius, ref length, ref rotation, ref Aesthetics.MidGrey));
            World.GlobalStaticObjects.Add(Cylinders[Cylinders.Count - 1]);
        }
        private void MakeNonAxisAligned(ref int boxesDrawn)
        {
            Vector3 v3 = new Vector3(0, 0, 0);
            float cubeSize = 100;
            float radius = 15f;
            float length = 100;
            v3.Y = boxesDrawn * -100;
            regionCentreY = v3.Y;
            boxesDrawn++;

            Vector3 rotation = new Vector3((float)Math.PI / 2, -(float)(Math.PI / 4), 0);
            v3.X = 0; v3.Z = 0;
            Boundary = new Box(ref v3, ref cubeSize, ref Aesthetics.MidGrey);
            World.GlobalStaticObjects.Add(Boundary);
            Cylinders.Add(new Cylinder(ref v3, ref radius, ref length, ref rotation, ref Aesthetics.MidGrey));
            World.GlobalStaticObjects.Add(Cylinders[Cylinders.Count - 1]);
            rotation.Y = (float)(Math.PI / 4);//(float)Math.PI;
            radius = 10f;
            Cylinders.Add(new Cylinder(ref v3, ref radius, ref length, ref rotation, ref Aesthetics.MidGrey));
            World.GlobalStaticObjects.Add(Cylinders[Cylinders.Count - 1]);
        }

        protected override void ballByBall(Ball b1, Sphere s1)
        {
            foreach (Cylinder c in Cylinders)
            {
                World.gamePhysics.CheckFor_SphereCylinder_Collision(c, b1, ACWWindow.mShader);
            }
            base.ballByBall(b1, s1);
        }
        protected override void ballAlone(Ball b1)
        {
            foreach (Cylinder c in Cylinders)
            {
                World.gamePhysics.CheckFor_SphereCylinder_Collision(c, b1, ACWWindow.mShader);
            }
            base.ballAlone(b1);
        }
    }
    public class DoomRegion : SpatialRegion
    {
        Doom BallOfDoom;
        public DoomRegion(ref int boxesDrawn)
        {
            Vector3 v3 = new Vector3(0, 0, 0);
            float cubeSize = 100;
            float radius = 25f;

            v3.Y = boxesDrawn * -100;
            regionCentreY = v3.Y;
            boxesDrawn++;
         
            Boundary = new Box(ref v3, ref cubeSize, ref Aesthetics.MidGrey);
            World.GlobalStaticObjects.Add(Boundary);
            BallOfDoom = new Doom(ref v3, radius, ref Aesthetics.Crimson);
            World.GlobalStaticObjects.Add(BallOfDoom);
        }
        protected override void ballByBall(Ball b1, Sphere s1)
        {
            World.gamePhysics.CheckFor_SphereSphere_Collision(b1, BallOfDoom);
            base.ballByBall(b1, s1);
        }
        protected override void ballAlone(Ball b1)
        {
            World.gamePhysics.CheckFor_SphereSphere_Collision(b1, BallOfDoom);
            base.ballAlone(b1);
        }
    }
}
