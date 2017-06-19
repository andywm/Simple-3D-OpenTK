﻿using System;
using System.Collections.Generic;
using Labs.Utility;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Labs.Lab4;

namespace Labs.ACW
{
    public class Physics
    {  
        //Physics Settings
        public bool showPhysicsData = false;
        bool integrationBefore = true;
        bool limitless = false;
        float restitutionDelta=0.05f, restitutionMax=1, restituionMin=0, restitutionCoefficent = 0.9f;
        Vector4 acceleration_g = new Vector4(0,-1f,0,0); //World Scalling is a 10nth. So 1 here represents 10m\sE2
        //House Keeping
        List<Ball> cleanupList = new List<Ball>();
        public void SimulateFrame(float timestep, ShaderUtility mShader)
        {
            foreach (Ball b in SpatialRegion.OverheadController_AllBalls)
            {
                b.vOldPosition = b.vPosition;
                PositionUpdate(b, timestep);

                CheckFor_SpherePlane_Collision(b);
            }
            SpatialRegion.SpatialPartitioningController();
            foreach (SpatialRegion space in SpatialRegion.AllRegions)
            {
                space.DoCollisionChecks(); //Each Spatial Segment Handles it's own collision detection calls.
            }
            //Remove dead objects from simlation. Go on Garbage Collector, do your thing.
            while (cleanupList.Count > 0)
            {
                cleanupList[0].KillRegions();
                SpatialRegion.OverheadController_AllBalls.Remove(cleanupList[0]);
                cleanupList.Remove(cleanupList[0]);
            }
        }
        public void PositionUpdate(Ball ball, float timestep)
        {
            ball.vOldPosition = ball.vPosition;
            if (integrationBefore)
            {
                ball.velocity += acceleration_g * timestep;
                ball.vPosition += (ball.velocity * ACWWindow.Centimetre);
            }
            else
            {
                ball.vPosition += (ball.velocity * ACWWindow.Centimetre);
                ball.velocity += acceleration_g * timestep;
            }
        }
        //Update Settings
        public float setCollisionRestituionCoefficent(int direction)
        {
            //Clamp 0 - 1
            float newValue = (restitutionCoefficent + (direction * restitutionDelta));
            if (direction > 0)
            { 
                return (restitutionCoefficent = newValue>restitutionMax? restitutionMax : newValue);
            }
            return (restitutionCoefficent = newValue<restituionMin? restituionMin : newValue);

        }
        public bool setEulerIntegration()
        {
            return (integrationBefore = !integrationBefore); 
        }
        public bool setShowPhysics()
        {
            return (showPhysicsData = !showPhysicsData);
        }
        public bool setLimitless()
        {
            return (limitless = !limitless);
        }
        //Collision Detection.
        public bool CheckFor_SphereSphere_Collision(Ball b1, Sphere b2)
        {
            bool doomMode = false;
            if (b2 is Doom)
            { 
                doomMode = true;
            }

            float separation = Math.Abs((b1.vPosition - b2.vPosition).Length);
            #region visualisation_stuff

            if (separation < ((Ball)b1).DistanceToNearestObject)
            {
                b1.DistanceToNearestObject = separation * 2;
                b1.NearestContact = b2.vPosition-b1.vPosition;
            }
            if (doomMode == false && separation < ((Ball)b2).DistanceToNearestObject)
            {
                ((Ball)b2).DistanceToNearestObject = separation * 2;
                ((Ball)b2).NearestContact = b1.vPosition - b2.vPosition;
            }
            #endregion

            //are the balls further apart than the sum of their radii?
            if (separation < (b1.radius + b2.radius)  * ACWWindow.Centimetre) 
            {
                if (doomMode)
                {
                    Vector4 cNorm = b2.vPosition - b1.vPosition;
                    Vector3 Norm = new Vector3(cNorm.X, cNorm.Y, cNorm.Z);
                    React_SphereDoom(ref b1, separation, Norm);
                }
                else
                {
                    b1.vPosition = b1.vOldPosition;
                    b2.vPosition = ((Ball)b2).vOldPosition;
                    React_SphereSphere(b1, (Ball)b2);
                }
            }
            return true;
        }
        public bool CheckFor_SphereCylinder_Collision(Cylinder cylinder, Ball ball, ShaderUtility mShader)
        {
            Vector4 vPoint1, centre, vPoint2;
            float cylinderRadius, clen; //Radius and Length of the Cylinder.
            Matrix4 mObjRotation; //Cylinder Transformation data.
            
            cylinder.GetCollisionData(out centre, out clen, out mObjRotation, out cylinderRadius);
            vPoint1 = centre + new Vector4(0, ACWWindow.Centimetre * (clen / 2), 0, 0);
            vPoint2 = centre - new Vector4(0, ACWWindow.Centimetre * (clen / 2), 0, 0);

            float modifierPercentage = ball.radius + ball.radius / 2; //for extrapolating the previous position.

            vPoint1 = Vector4.Transform((vPoint1), mObjRotation);
            vPoint2 = Vector4.Transform((vPoint2), mObjRotation);

            #region horrible_horribe_hacky_thing_oh_please_fix_this
            if (Math.Abs(vPoint1.X) - Math.Abs(vPoint1.Z) > 1 || Math.Abs(vPoint1.Z) - Math.Abs(vPoint1.X) > 1)
            {
                if (Math.Abs(vPoint1.X) > Math.Abs(vPoint1.Z))
                {
                    vPoint1.Z = centre.Z;
                    vPoint2.Z = centre.Z;
                }
                else
                {
                   vPoint1.X = centre.X;
                   vPoint2.X = centre.X;
                }
            }
            vPoint1.Y = centre.Y;
            vPoint2.Y = centre.Y;
            #endregion
            
            //Line Collision Algorith
            Vector4 vP2toCentre = ball.vPosition - vPoint2;
            Vector4 vP2P1_normal = (vPoint1 - vPoint2).Normalized();
            Vector4 adjSide = Vector4.Dot(vP2toCentre, vP2P1_normal) * vP2P1_normal;
            float len = adjSide.Length;

            Vector4 vCentreToEdge = vPoint2 + (adjSide - ball.vPosition);
           
            Vector4 pointOfImpact = vPoint2 + adjSide;
            #region visualisation_stuff
            if (vCentreToEdge.Length < ball.DistanceToNearestObject)
            {
                ball.DistanceToNearestObject = vCentreToEdge.Length;
                ball.NearestContact = (pointOfImpact - ball.vPosition).Normalized() * ball.DistanceToNearestObject*2;//Vector4.Transform(iPoint, mObjRotation.Inverted());
            }
            #endregion

            if (vCentreToEdge.Length < ((ball.radius + cylinderRadius) * ACWWindow.Centimetre) || vCentreToEdge.Length < 0) //(vCentreToEdge.Length < mCircleRadius[circleID])
            {
                //if (len > 0 && len < (vPoint1 - vPoint2).Length) //Check not really needed if we're not considering anything outside the box...
                {
                    ball.vPosition = ball.vOldPosition;
                    Vector4 normal =  - vCentreToEdge;
                    normal = normal.Normalized();
                    React_SphereImmovableObject(ball, normal);
                    return true;
                   
                }
            }
            return false;
        }
        public void CheckFor_SpherePlane_Collision(Ball b)
        {
            float modifier = ACWWindow.Centimetre;
            
            //Collapse Problem to a 2D plane
            Vector2 CollapsedPosition = new Vector2(b.vPosition.X * modifier, b.vPosition.Z * modifier);

            //Check each boundary in turn, reflect ball velocity if impact.
            if (CollapsedPosition.X > 0.5 - b.radius * (modifier*modifier))
            {
                Vector4 normal = new Vector4(1, 0, 0, 0);
                React_SphereImmovableObject(b, normal);
            }
            if (CollapsedPosition.X < -0.5+b.radius * (modifier*modifier))
            {
                Vector4 normal = new Vector4(-1, 0, 0, 0);
                React_SphereImmovableObject(b, normal);
            }
            if (CollapsedPosition.Y > 0.5 - b.radius * (modifier * modifier))
            {
                Vector4 normal = new Vector4(0, 0, 1, 0);
                React_SphereImmovableObject(b, normal);
            }
            if (CollapsedPosition.Y < -0.5 + b.radius * (modifier * modifier))
            {
                Vector4 normal = new Vector4(0, 0, -1, 0);
                React_SphereImmovableObject(b, normal);
            }
            if (limitless) return;
            if (b.vPosition.Y * modifier > (0.5 - (b.radius*modifier*modifier)))
            {
                Vector4 normal = new Vector4(0, -1, 0, 0);
                React_SphereImmovableObject(b, normal);
                if (b.vPosition.Y * modifier > 1) //Remove if significantly above.
                {
                    cleanupList.Add(b); 
                }
            }
            if (b.vPosition.Y * modifier < (-World.boxesDrawn + 0.5f + (b.radius * modifier * modifier)))
            {
                cleanupList.Add(b);
            }
        }
        //Physical Reaction.
        private void React_SphereSphere(Ball b1, Ball b2)
        {
                Vector4 normal = b2.vPosition - b1.vPosition;

                b1.vPosition = b1.vOldPosition;
                b2.vPosition = b2.vOldPosition;
                normal.Normalize();
                float conversionConst = 1E2f;
                float mass1 = b1.GetMass() * conversionConst;
                float mass2 = b2.GetMass() * conversionConst;
                float SumMass = mass1 + mass2;
              

                Vector4 velocityInDirectionOfCollision_C1 = Vector4.Dot(b1.velocity, normal) * normal;
                Vector4 velocityInDirectionOfCollision_C2 = Vector4.Dot(b2.velocity, -normal) * -normal;

                Vector4 retainC1 = ((mass1 * velocityInDirectionOfCollision_C1) + (mass2 * velocityInDirectionOfCollision_C2) + restitutionCoefficent * (mass2 * (velocityInDirectionOfCollision_C2 - velocityInDirectionOfCollision_C1))) / SumMass;
                Vector4 retainC2 = ((mass1 * velocityInDirectionOfCollision_C1) + (mass2 * velocityInDirectionOfCollision_C2) + restitutionCoefficent * (mass1 * (velocityInDirectionOfCollision_C1 - velocityInDirectionOfCollision_C2))) / SumMass;

                Vector4 velocityPerpendicularToCollision_C1 = b1.velocity - velocityInDirectionOfCollision_C1;
                Vector4 velocityPerpendicularToCollision_C2 = b2.velocity - velocityInDirectionOfCollision_C2;

                b1.velocity = velocityPerpendicularToCollision_C1 + retainC1;
                b2.velocity = velocityPerpendicularToCollision_C2 + retainC2;
        }        
        private void React_SphereImmovableObject(Ball b, Vector4 normal)
        {
            b.vPosition = b.vOldPosition;
            b.velocity = b.velocity - (1 + restitutionCoefficent) * Vector4.Dot(normal, b.velocity) * normal;
        }
        private void React_SphereDoom(ref Ball impactingBall, float cullAmount, Vector3 collisionNormal)
        {
                Vector3 cVelocity = new Vector3(impactingBall.velocity.X, impactingBall.velocity.Y, impactingBall.velocity.Z);
                Vector3 B = Vector3.Cross(collisionNormal, cVelocity);
                Vector3 Trans = Vector3.Cross(B, cVelocity);
                Vector4 Translate = new Vector4(Trans, 0);
                Translate.Normalize();
                float cull = cullAmount * ACWWindow.Centimetre;
                impactingBall.radius -= cull;
                if (Vector3.Dot(cVelocity, collisionNormal) > Math.PI / 4)
                {
                    impactingBall.radius -= cull / 2;
                    impactingBall.vPosition += (Translate * (cull / 2) * ACWWindow.Centimetre);
                }
                else
                {
                    impactingBall.radius -= cull;
                }
                impactingBall.updateScale();
            if (impactingBall.radius  < 0)
            {
                cleanupList.Add(impactingBall);
            }
            
        }
    }
}
