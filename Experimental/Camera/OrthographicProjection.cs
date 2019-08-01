using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Experimental
{
    public sealed class OrthographicProjection : Projection
    {
        private float height, width, near, far;

        public OrthographicProjection()
        {
            height = 1000f;
            width = 1000f;

            near = 1f;
            far = 1000f;
        }

        public float Height
        {
            get => height;
            set
            {
                height = value;
                RaisePropertyChanged("Height");
            }
        }
        public float Width
        {
            get => width;
            set
            {
                width = value;
                RaisePropertyChanged("Width");
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

        public Matrix4x4 CreateNormalizingMatrix()
        {
            float near = -Near;
            float far = -Far;

            Matrix4x4 matNormalize = Matrix4x4.Identity;
            matNormalize.M11 = 1 / width;
            matNormalize.M22 = 1 / height;
            matNormalize.M33 = 1 / far - near;

            return matNormalize;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
