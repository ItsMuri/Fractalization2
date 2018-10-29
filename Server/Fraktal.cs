﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Server
{
    [DataContract]
    class Fraktal
    {
        //Hier kann ich weder DataMember, noch IgnoreDataMember hinschreiben!!!
        //Der Compiler meldet dass das nicht sein darf.
        public enum Farbe
        {
            Gelb,Rot,Schwarz
        }
        [DataMember]
        public double[] KoordinatenX { get; set; } //X: -2 bis +2
        [DataMember]
        public double[] KoordinatenY { get; set; } //Y: Wurzel aus -1 = i !!! also -i und i werden abgespeichert.
        [DataMember]
        public int Iteration { get; set; } //Bei welcher Iteration sind wir???

        [IgnoreDataMember]
        private int iterationCounter=0;

        public Fraktal(int iterationsCount)
        {
            Iteration = iterationsCount;
        }

        

    }
}