using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    [DataContract]
    public class FraktalTask
    {
        [DataMember]
        public int XCoordinates { get; set; }

        [DataMember]
        public int YCoordinates { get; set; }

        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public Bitmap partialFraktal { get; set; }
    }
}
