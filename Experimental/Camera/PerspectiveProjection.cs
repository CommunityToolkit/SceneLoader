using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TestViewer
{
    public class PerspectiveProjection : Projection
    {
        private float xfov, yfov;

        public PerspectiveProjection() : base()
        {
            xfov = MathF.PI / 2;
            yfov = MathF.PI / 2;
        }

        public float xFov
        {
            get => xfov;
            set
            {
                xfov = value;
                RaisePropertyChanged("xFov");
            }
        }
        public float yFov
        {
            get => yfov;
            set
            {
                yfov = value;
                RaisePropertyChanged("yFov");
            }
        }

        public override Matrix4x4 CreateProjectionMatrix(Stretch stretch, Vector2 size)
        {
            float near = -Near;
            float far = -Far;
            bool stretchX;


            Matrix4x4 matScale = Matrix4x4.Identity;

            switch (stretch)
            {
                case Stretch.None:
                    // Do nothing
                    break;
                case Stretch.Fill:
                    matScale.M11 = size.X;
                    matScale.M22 = size.Y;
                    matScale.M33 = (size.X + size.Y) / 2f;
                    break;
                case Stretch.FixX:
                    stretchX = false;
                    matScale = ScaleMatrix(stretchX, size);
                    break;
                case Stretch.FixY:
                    stretchX = true;
                    matScale = ScaleMatrix(stretchX, size);
                    break;
                case Stretch.Uniform:
                    // wide
                    if (size.X >= size.Y)
                    {
                        stretchX = true;
                        matScale = ScaleMatrix(stretchX, size);
                    }
                    // long
                    else
                    {
                        stretchX = false;
                        matScale = ScaleMatrix(stretchX, size);
                    }
                    break;
                case Stretch.UniformToFill:
                    // wide
                    if (size.X >= size.Y)
                    {
                        stretchX = false;
                        matScale = ScaleMatrix(stretchX, size);
                    }
                    // long
                    else
                    {
                        stretchX = true;
                        matScale = ScaleMatrix(stretchX, size);
                    }
                    break;
            }

            float top = MathF.Tan(yfov / 2) * near;
            float right = MathF.Tan(xfov / 2) * near;

            Matrix4x4 matFOV = Matrix4x4.Identity;
            matFOV.M11 = near / right;
            matFOV.M22 = near / top;
            matFOV.M33 = -(far + near) / (far - near);
            matFOV.M34 = -1;
            matFOV.M43 = (-2 * far * near) / (far - near);

            return matFOV * matScale;
        }

        private Matrix4x4 ScaleMatrix(bool stretchX, Vector2 size)
        {
            Matrix4x4 matScale = Matrix4x4.Identity;
            matScale.M11 = matScale.M22 = matScale.M33 = stretchX ? size.X : size.Y;

            return matScale;
        }
    }
}
