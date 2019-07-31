using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TestViewer
{
    public class OrthographicProjection : Projection
    {
        private float top, bottom, left, right;

        public OrthographicProjection()
        {
            top = 10f;
            bottom = 10f;
            left = 10f;
            right = 10f;
        }

        public float Top
        {
            get => top;
            set
            {
                top = value;
                RaisePropertyChanged("Top");
            }
        }
        public float Bottom
        {
            get => bottom;
            set
            {
                bottom = value;
                RaisePropertyChanged("Bottom");
            }
        }
        public float Left
        {
            get => left;
            set
            {
                left = value;
                RaisePropertyChanged("Left");
            }
        }
        public float Right
        {
            get => right;
            set
            {
                right = value;
                RaisePropertyChanged("Right");
            }
        }

        public override Matrix4x4 CreateProjectionMatrix(Stretch stretch, Vector2 size)
        {
            switch(stretch)
            {
                case Stretch.None:
                    break;
                case Stretch.Fill:
                    break;
                case Stretch.FixX:
                    break;
                case Stretch.FixY:
                    break;
                case Stretch.Uniform:
                    break;
                case Stretch.UniformToFill:
                    break;
            }

            return Matrix4x4.Identity;
        }
    }
}
