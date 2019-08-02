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
    public sealed class PerspectiveProjection : Projection
    {
        private float xfov, yfov, near, far;
        private CompositionPropertySet propertySet;

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
        public void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        public void CreateExpressionAnimation(CompositionPropertySet toAnimate, string propertyName)
        {
            propertySet = Window.Current.Compositor.CreatePropertySet();
            propertySet.InsertScalar("XFov", xfov);
            propertySet.InsertScalar("YFov", yfov);
            propertySet.InsertScalar("Near", near);
            propertySet.InsertScalar("Far", far);

            var matNorm =
                "Matrix4x4(" +
                "1 / Tan(PerspProj.XFov / 2), 0, 0, 0, " +
                "0, 1 / Tan(PerspProj.YFov / 2), 0, 0, " +
                "0, 0, (PerspProj.Far - PerspProj.Near) / -(PerspProj.Far + PerspProj.Near), -1, " +
                "0, 0, (-2 * PerspProj.Far * PerspProj.Near) / -(PerspProj.Far + PerspProj.Near), 1)";

            var projExpression = Window.Current.Compositor.CreateExpressionAnimation();
            projExpression.Expression = matNorm;
            projExpression.SetReferenceParameter("PerspProj", propertySet);

            toAnimate.StartAnimation(propertyName, projExpression);
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
