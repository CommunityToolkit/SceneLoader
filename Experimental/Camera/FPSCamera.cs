using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using Windows.UI.Composition;
using Windows.UI.Composition.Scenes;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace TestViewer
{
    public class FPSCamera : Camera
    {
        private Compositor compositor;

        private Vector3 position;
        private float yaw, pitch, roll;
        private float perspectiveDistance;

        public static bool useExpressionAnimations = false;

        private Projection projection;
        private CompositionPropertySet propertySet;

        

        public FPSCamera()
        {
            this.compositor = Window.Current.Compositor;

            if (useExpressionAnimations)
            {
                CreateExpressionAnimations();
            }
            else
            {
                Yaw = 0;
                Pitch = 0;
                Roll = 0;
                Position = Vector3.Zero;
            }

            projection = new PerspectiveProjection();

            //distFix();
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////
        /// PUBLIC PROPERTIES
        ///////////////////////////////////////////////////////////////////////////////////////////////// 
        public float Yaw
        {
            get
            {
                if (useExpressionAnimations)
                {
                    float curr = 0.0f;
                    return (propertySet.TryGetScalar("Yaw", out curr) == CompositionGetValueStatus.Succeeded) ? curr : yaw;
                }
                else
                {
                    return yaw;
                }
            }
            set
            {
                if (useExpressionAnimations)
                {
                    propertySet.InsertScalar("Yaw", value);
                }
                else
                { 
                    yaw = value;
                    RaisePropertyChanged("Yaw");
                }
            }
        }
        public float Pitch 
        {
            get
            {
                if (useExpressionAnimations)
                {
                    float curr = 0.0f;
                    return (propertySet.TryGetScalar("Pitch", out curr) == CompositionGetValueStatus.Succeeded) ? curr : pitch;
                }
                else
                {
                    return pitch;
                }
            }
            set
            {
                if (useExpressionAnimations)
                {
                    propertySet.InsertScalar("Pitch", value);
                }
                else
                {
                    pitch = value;
                    RaisePropertyChanged("Pitch");
                }
            }
        }
        public float Roll
        {
            get
            {
                if (useExpressionAnimations)
                {
                    float curr = 0.0f;
                    return (propertySet.TryGetScalar("Roll", out curr) == CompositionGetValueStatus.Succeeded) ? curr : roll;
                }
                else
                {
                    return roll;
                }
            }
            set
            {
                if (useExpressionAnimations)
                {
                    propertySet.InsertScalar("Roll", value);
                }
                else
                {
                    roll = value;
                    RaisePropertyChanged("Roll");
                }
            }
        }
        public Vector3 Position
        {
            get
            {
                if(useExpressionAnimations)
                {
                    Vector3 curr = Vector3.Zero;
                    return (propertySet.TryGetVector3("Position", out curr) == CompositionGetValueStatus.Succeeded) ? curr : position;
                }
                else
                {
                    return position;
                }
            }
            set
            {
                if (useExpressionAnimations)
                {
                    propertySet.InsertVector3("Position", value);
                }
                else
                {
                    position = value;
                    RaisePropertyChanged("Position");
                }
            }
        }
        public Vector3 LookDirection
        {
            get => Vector3.Zero;
            set
            {
                Vector3 direction = value;
                if (value != Vector3.Zero)
                {
                    direction = Vector3.Normalize(value);
                }
                
                direction.X *= -1; 
                Yaw = (MathF.PI / 2f) + MathF.Atan2(direction.Z, direction.X);
                Pitch = MathF.Asin(direction.Y);                    
            }
        }
        
        public bool UseAnimations
        {
            get => FPSCamera.useExpressionAnimations;
            set
            {
                // actually switching the value
                if(value != UseAnimations)
                {
                    // turn animations off
                    if (!value)
                    {
                        propertySet.TryGetVector3("Position", out position);
                        propertySet.TryGetScalar("Yaw", out yaw);
                        propertySet.TryGetScalar("Pitch", out pitch);
                        propertySet.TryGetScalar("Roll", out roll);

                        //head.StopAnimation("TransformMatrix");

                        UpdateTransformation();
                    }
                    // turn animations on
                    else
                    {
                        CreateExpressionAnimations(); // relies on property set being set to fields instead of normal default zeros
                    }

                    FPSCamera.useExpressionAnimations = value;
                }
            }
        }

        public Projection Projection
        {
            get => projection;
            set
            {
                projection = value;
                projection.PropertyChanged += PropertyChanged;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        /// PUBLIC FUNCTIONS
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public Matrix4x4 CreateTransformationMatrix()
        {
            Matrix4x4 transformation = Matrix4x4.Identity;
            Matrix4x4 matPos = Matrix4x4.CreateTranslation(-position);
            Matrix4x4 matRoll = Matrix4x4.CreateFromAxisAngle(new Vector3(0, 0, 1), -roll);
            Matrix4x4 matPitch = Matrix4x4.CreateFromAxisAngle(new Vector3(1, 0, 0), -pitch);
            Matrix4x4 matYaw = Matrix4x4.CreateFromAxisAngle(new Vector3(0, 1, 0), -yaw);

            return matPos * matYaw * matPitch * matRoll;
        }

        private void UpdateTransformation()
        {
            Matrix4x4 transformation = Matrix4x4.Identity;
            Matrix4x4 matPos = Matrix4x4.CreateTranslation(-position);
            Matrix4x4 matRoll = Matrix4x4.CreateFromAxisAngle(new Vector3(0, 0, 1), -roll);
            Matrix4x4 matPitch = Matrix4x4.CreateFromAxisAngle(new Vector3(1, 0, 0), -pitch);
            Matrix4x4 matYaw = Matrix4x4.CreateFromAxisAngle(new Vector3(0, 1, 0), -yaw);
            Matrix4x4 matPerspFix = Matrix4x4.CreateTranslation(new Vector3(0, 0, perspectiveDistance));
            Matrix4x4 matPersp = Matrix4x4.Identity;
            matPersp.M34 = -1f / perspectiveDistance;

            transformation = matPos * matYaw * matPitch * matRoll * matPerspFix * matPersp;


            //head.TransformMatrix = transformation;
        }

        public CompositionPropertySet GetPropertySet()
        {
            return propertySet;
        }

        public void StartAnimation(string propertyName, CompositionAnimation animation)
        {
            propertySet.StartAnimation(propertyName, animation);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        /// PRIVATE FUNCTIONS
        ///////////////////////////////////////////////////////////////////////////////////////////////// 

        private void CreateExpressionAnimations()
        {
            propertySet = compositor.CreatePropertySet();
            propertySet.InsertVector3("Position", position); // changed from Vector.Zero
            propertySet.InsertScalar("Yaw", yaw); // changed from 0.0f
            propertySet.InsertScalar("Pitch", pitch); // changed from 0.0f
            propertySet.InsertScalar("Roll", roll); // changed from 0.0f
            //propertySet.InsertMatrix4x4("Transformation", head.TransformMatrix); // change from Matrix4x4.Identity

            var matPos = "Matrix4x4.CreateTranslation(-FPSCamera.Position)";
            var matRoll = "Matrix4x4.CreateFromAxisAngle(Vector3(0, 0, 1), -FPSCamera.Roll)";
            var matPitch = "Matrix4x4.CreateFromAxisAngle(Vector3(1, 0, 0), -FPSCamera.Pitch)";
            var matYaw = "Matrix4x4.CreateFromAxisAngle(Vector3(0, 1, 0), -FPSCamera.Yaw)";
            //var matFOV = ""; // TODO
            //var matScale = ""; // TODO
            //var expression = matPos + "*" + matYaw + "*" + matPitch + "*" + matRoll + "*" + matFOV + "*" + matScale;
            var matPerspFix = "Matrix4x4.CreateTranslation(Vector3(0, 0, 600f))";
            var matPersp = "Matrix4x4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, -1/600f, 0, 0, 0, 1)";
            var expression = matPos + "*" + matYaw + "*" + matPitch + "*" + matRoll + "*" + matPerspFix + "*" + matPersp;


            var transformationExpression = compositor.CreateExpressionAnimation();
            transformationExpression.SetReferenceParameter("FPSCamera", propertySet);
            transformationExpression.Expression = expression;

            //head.StartAnimation("TransformMatrix", transformationExpression);
        }
    }
}
