using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Experimental
{
    public abstract class Projection : INotifyPropertyChanged
    {
        private float near, far;

        protected Projection()
        {
            near = 1f;
            far = 1000f;
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

        public abstract Matrix4x4 CreateProjectionMatrix(Stretch stretch, Vector2 size);
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
