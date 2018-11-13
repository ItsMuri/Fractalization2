using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SerializedFraktal
{
    [DataContract]
    public class PropsOfFractal
    {
        [DataMember]
        public int IterationsCount { get; set; }
        public PropsOfFractal(int iterationsCount)
        {
            IterationsCount = iterationsCount;
        }
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public Bitmap NeededBitmap { get; set; }
        [DataMember]
        public double imgWidth { get; set; }
        [DataMember]
        public double imgHeight { get; set; }
    }
}
