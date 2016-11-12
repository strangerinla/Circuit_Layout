using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Circuit_Layout
{
    class Resistor : Element
    {
        #region Determination
        public Resistor()
        {
            Resistance = 100;
            Name = "R";
        }
        #endregion
    }
    class ResistorX : Element
    {
        #region Determination
        public ResistorX()
        {
            Random r = new Random();
            Resistance = r.Next( 10, 100000 ) / 100d;
            Name = "Rx";
        }
        #endregion
    }
    class NoDrawConnector : Connector
    {

    }
    class Reohord : Element
    {
        #region Determination
        public Reohord()
        {
            Resistance = 100;
            OffsetA = -100;
            OffsetB = 100;
            OffsetC = -30;
            Name = "Rh";
            DisplayLength = true;
        }
        #endregion
        #region Properties
        #region Values
        private double lengthac;
        private bool displayLength;
        public System.Windows.Visibility LengthVisibility
        {
            get
            {
                return displayLength ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
        }
        public System.Windows.Visibility ResistanceVisibility
        {
            get
            {
                return displayLength ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            }
        }
        public bool DisplayLength
        {
            get
            {
                return displayLength;
            }
            set
            {
                if ( displayLength != value )
                {
                    displayLength = value;
                    Fire( "LengthVisibility", "ResistanceVisibility" );
                }
            }
        }
        #endregion
        #region Pins
        public Node NodeC { get; set; }
        public double LengthAC
        {
            get
            {
                return lengthac;
            }
            set
            {
                if ( lengthac != value )
                {
                    lengthac = value;
                    ResistorAC.Resistance = value / 100 * Resistance;
                    ResistorBC.Resistance = ( 100 - value ) / 100 * Resistance;
                    Fire( "LengthAC", "LengthBC" );
                }
            }
        }
        public double LengthBC
        {
            get { return 100 - lengthac; }
        }
        public Connector ResistorAC { get; set; }
        public Connector ResistorBC { get; set; }
        public int OffsetC { get; protected set; }
        #endregion
        #endregion
        #region Methods
        protected override void PlacePins()
        {
            base.PlacePins();
            if ( NodeC != null )
            {
                NodeC.X = (int)( X + OffsetC * Math.Cos( ( Angle + 90 ) * Math.PI / 180 ) );
                NodeC.Y = (int)( Y + OffsetC * Math.Sin( ( Angle + 90 ) * Math.PI / 180 ) );
            }
        }
        #endregion
    }
    class Battery : Element
    {
        #region Determination
        public Battery()
        {
            Resistance = 1;
            Eds = 1;
            Name = "Gb";
        }
        #endregion
        #region Properties
        public double Eds { get; set; }
        #endregion
    }
    class Galvanometr : Element
    {
        #region Determination
        public Galvanometr()
        {
            OffsetA = -40;
            OffsetB = 40;
            Resistance = double.PositiveInfinity;
            Division = 1;
            Name = "G";
        }
        #endregion
        #region Properties
        private double[] divisions;
        private double electricCurrent, division;
        public double Division
        {
            get
            {
                return division;
            }
            set
            {
                if ( division != value )
                {
                    division = value;
                    divisions = new double[5]
                    .Select( ( i, j ) =>
                    {
                        j++;
                        return ( j - 3 ) * Division / 2d;
                    } )
                    .ToArray();
                    Fire( "Division", "Divisions" );
                }
            }
        }
        public double ElectricCurrent
        {
            get
            {
                return electricCurrent;
            }
            set
            {
                if ( electricCurrent != value )
                {
                    electricCurrent = value;
                    Fire( "ElectricCurrent", "ArrowAngle" );
                }
            }
        }
        public double ArrowAngle
        {
            get
            {
                double a = ElectricCurrent * 10 / Division;
                return a > 100 ? 100 : a < -100 ? -100 : a;
            }
        }
        public double[] Divisions
        {
            get
            {
                return divisions;
            }
        }
        #endregion
    }
}
