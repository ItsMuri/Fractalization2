using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Server
{
    class Fraktal
    {
        public enum Farbe
        {
            Gelb,Rot,Schwarz
        }
        public float Koordinaten { get; set; } //Welche Koordinaten sollen verwendet werden???
        public int Iteration { get; set; } //Bei welcher Iteration sind wir???

        private int iterationCounter=0;

        public Fraktal(int iterationsCount)
        {
            Iteration = iterationsCount;
        }

        

    }
}
