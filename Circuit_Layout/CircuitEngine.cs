﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Circuit_Layout
{
    class Node : INotifyPropertyChanged
    {
        #region Determination
        public Node()
        {
            Connections = new Dictionary<Node, Connector>();
        }

        public Node( int x, int y )
        {
            Connections = new Dictionary<Node, Connector>();
            X = x;
            Y = y;
        }
        #endregion
        #region Properties
        public Dictionary<Node, Connector> Connections { get; set; }
        private int x, y;
        public int X
        {
            get
            {
                return x;
            }
            set
            {
                if ( x != value )
                {
                    x = value;
                    Fire( "X" );
                }
            }
        }
        public int Y
        {
            get
            {
                return y;
            }
            set
            {
                if ( y != value )
                {
                    y = value;
                    Fire( "Y" );
                }
            }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                if ( isSelected != value )
                {
                    isSelected = value;
                    Fire( "StrokeThickness" );
                }
            }
        }
        public int StrokeThickness
        {
            get
            {
                return IsSelected ? 3 : 0;
            }
        }
        public bool Visited { get; set; }
        #region graph
        private int number;
        public int Number
        {
            get { return number; }
            set
            {
                if ( number != value )
                {
                    number = value;
                    Fire( "Number" );
                }
            }
        }
        #endregion
        #endregion
        #region Fire
        private void Fire( params string[] names )
        {
            if ( PropertyChanged != null )
                foreach ( var name in names )
                    PropertyChanged( this, new PropertyChangedEventArgs( name ) );
        }
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
    class Connector : INotifyPropertyChanged
    {
        #region Properties
        private double resistance;
        public Node NodeA { get; set; }
        public Node NodeB { get; set; }
        public double Resistance
        {
            get { return resistance; }
            set
            {
                if (resistance != value)
                {
                    resistance = value;
                    Fire( "Resistance" );
                }
            }
        }
        #endregion
        #region Fire
        protected void Fire( params string[] names )
        {
            if ( PropertyChanged != null )
                foreach ( var name in names )
                    PropertyChanged( this, new PropertyChangedEventArgs( name ) );
        }
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
    class Element : Connector
    {
        #region Determination
        public Element()
        {
            X = 0;
            Y = 0;
            OffsetA = -25;
            OffsetB = 25;
        }
        #endregion
        #region Properties
        private int x, y, angle;
        private string name;
        public int X
        {
            get
            {
                return x;
            }
            set
            {
                if ( x != value )
                {
                    x = value;
                    PlacePins();
                    Fire( "X" );
                }
            }
        }
        public int Y
        {
            get
            {
                return y;
            }
            set
            {
                if ( y != value )
                {
                    y = value;
                    PlacePins();
                    Fire( "Y" );
                }
            }
        }
        public int OffsetA { get; protected set; }
        public int OffsetB { get; protected set; }
        public int Angle
        {
            get
            {
                return angle;
            }
            set
            {
                if ( angle != value )
                {
                    angle = value;
                    PlacePins();
                    Fire( "Angle" );
                }
            }
        }
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if ( name != value )
                {
                    name = value;
                    Fire( "Name" );
                }
            }
        }
        #endregion
        #region Methods
        protected virtual void PlacePins()
        {
            if ( NodeA != null )
            {
                NodeA.X = (int)( x + OffsetA * Math.Cos( Angle * Math.PI / 180 ) );
                NodeA.Y = (int)( y + OffsetA * Math.Sin( Angle * Math.PI / 180 ) );
            }
            if ( NodeB != null )
            {
                NodeB.X = (int)( x + OffsetB * Math.Cos( Angle * Math.PI / 180 ) );
                NodeB.Y = (int)( y + OffsetB * Math.Sin( Angle * Math.PI / 180 ) );
            }
        }
        #endregion
    }
}