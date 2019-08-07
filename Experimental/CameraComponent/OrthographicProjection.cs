using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Composition;
using Windows.UI.Xaml;

namespace CameraComponent
{
    public sealed class OrthographicProjection : Projection
    {
        private float height, width, near, far;
        private CompositionPropertySet propertySet;

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
        public void CreateExpressionAnimation(CompositionPropertySet toAnimate, string propertyName)
        {
            propertySet = Window.Current.Compositor.CreatePropertySet();
            propertySet.InsertScalar("Height", height);
            propertySet.InsertScalar("Width", width);
            propertySet.InsertScalar("Near", near);
            propertySet.InsertScalar("Far", far);

            var matNorm =
                "Matrix4x4(" +
                "1 / OrthoProj.Width, 0, 0, 0, " +
                "0, 1 / OrthoProj.Height, 0, 0, " +
                "0, 0, 1 / (OrthoProj.Far - OrthoProj.Near), 0, " +
                "0, 0, 0, 1)";

            var projExpression = Window.Current.Compositor.CreateExpressionAnimation();
            projExpression.Expression = matNorm;
            projExpression.SetReferenceParameter("OrthoProj", propertySet);

            toAnimate.StartAnimation(propertyName, projExpression);
        }
        public Matrix4x4 CreateNormalizingMatrix()
        {
            Matrix4x4 matNormalize = Matrix4x4.Identity;
            matNormalize.M11 = 1 / width;
            matNormalize.M22 = 1 / height;
            matNormalize.M33 = 1 / (Far - Near);

            return matNormalize;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
