using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FractalLibrary
{
    [DataContract]
    public class PropsOfFractal : ICloneable
    {
        public PropsOfFractal(int iterationsCount)
        {
            IterationsCount = iterationsCount;
        }
        [DataMember]
        public int IterationsCount { get; set; }
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public int ClientCount { get; set; }
        [DataMember]
        public Bitmap NeededBitmap { get; set; }
        [DataMember]
        public double ImgWidth { get; set; }
        [DataMember]
        public double ImgHeight { get; set; }

        public object Clone()
        {
            var item = new PropsOfFractal(IterationsCount)
            {
                ClientCount = ClientCount,
                Id = Id,
                ImgHeight = ImgHeight,
                ImgWidth = ImgWidth,
                IterationsCount = IterationsCount,
                NeededBitmap = NeededBitmap
            };

            return item;
        }
    }
}