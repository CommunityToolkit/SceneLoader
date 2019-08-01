using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Experimental
{
    public interface Projection : INotifyPropertyChanged
    {
        float Near { get; set; }
        float Far { get; set; }
        Matrix4x4 CreateNormalizingMatrix();
    }
}
