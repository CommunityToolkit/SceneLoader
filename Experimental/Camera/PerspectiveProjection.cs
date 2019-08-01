using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Experimental
{
    public sealed class PerspectiveProjection : Projection
    {
        private float xfov, yfov, near, far;

        public PerspectiveProjection()
        {
            xfov = MathF.PI / 2;
            yfov = MathF.PI / 2;

            near = 1f;
            far = 1000f;
        }

        public float XFov
        {
            get => xfov;
            set
            {
                xfov = value;
                RaisePropertyChanged("xFov");
            }
        }
        public float YFov
        {
            get => yfov;
            set
            {
                yfov = value;
                RaisePropertyChanged("yFov");
            }
        }
        public float Near
        {
            get => near;
            set
            {
                near = value;
                RaisePropertyChanged("Near");
            }
        }
        public float Far
        {
            get => far;
            set
            {
                far = value;
                RaisePropertyChanged("Far");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public Matrix4x4 CreateNormalizingMatrix()
        {
            float near = -Near;
            float far = -Far;
            float top = MathF.Tan(yfov / 2) * near;
            float right = MathF.Tan(xfov / 2) * near;

            Matrix4x4 matNormalize = Matrix4x4.Identity;
            matNormalize.M11 = near / right;
            matNormalize.M22 = near / top;
            matNormalize.M33 = -(far + near) / (far - near);
            matNormalize.M34 = -1;
            matNormalize.M43 = (-2 * far * near) / (far - near);

            return matNormalize;
        }
    }
}
