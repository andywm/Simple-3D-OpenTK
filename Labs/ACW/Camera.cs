﻿using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Labs.Lab4;

namespace Labs.ACW
{
    class Camera
    {
        public bool enableFreeCam = true;
        public bool hal9000 = false;
        private bool easterEgg = false;
        private Matrix4 mFixed_Doom, mFixed_S1, mFixed_S2, mUser;
        public bool setEasterEgg()
        {
            hal9000 = false;
            return easterEgg = !easterEgg;
        }
        public Camera(Matrix4 user)
        {
            mUser = user;

            mFixed_S1 = Matrix4.CreateTranslation(0, 0, 0);
            mFixed_S1 *= Matrix4.CreateRotationX((float)(Math.PI / 2));
            mFixed_S2 = Matrix4.CreateTranslation(0, 10, 0);
            mFixed_S2 *= Matrix4.CreateRotationX((float)(Math.PI/2));
            mFixed_Doom = Matrix4.CreateTranslation(0, 19, 0);
            mFixed_Doom *= Matrix4.CreateRotationX((float)(Math.PI / 2));
        }
        public Matrix4 user()
        {
            enableFreeCam = true;
            return mUser;
        }
        public Matrix4 fixedDoom()
        {
            enableFreeCam = false;
            if(easterEgg)
            hal9000 = true;
            return mFixed_Doom;
        }
        public Matrix4 fixedS1()
        {
            enableFreeCam = false;
            return mFixed_S1;
        }
        public Matrix4 fixedS2()
        {
            enableFreeCam = false;
            return mFixed_S2;
        }
    }
}
