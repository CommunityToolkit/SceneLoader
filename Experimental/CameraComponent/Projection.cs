using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Composition;

namespace CameraComponent
{
    public interface Projection : INotifyPropertyChanged
    {
        float Near { get; set; }
        float Far { get; set; }
        Matrix4x4 CreateNormalizingMatrix();
        void CreateExpressionAnimation(CompositionPropertySet toAnimate, string propertyName);
    }
}
