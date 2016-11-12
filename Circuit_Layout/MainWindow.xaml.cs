using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Circuit_Layout
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Determination
        public MainWindow()
        {
            InitializeComponent();
            Layout layout = Layout.GetInstance();
            this.DataContext = layout;
            clcLayout.Owner = this;
        }
        #endregion

        private void Window_Loaded( object sender, RoutedEventArgs e )
        {
            Layout layout = Layout.GetInstance();

            var galv = layout.CreateElement( typeof( Galvanometr ), 100, 50 ) as Galvanometr;

            var res1 = layout.CreateElement( typeof( Resistor ), 200, 100 );
            var res2 = layout.CreateElement( typeof( Resistor ), 200, 150 );
            var res3 = layout.CreateElement( typeof( Resistor ), 200, 200 );
            var bat = layout.CreateElement( typeof( Battery ), 200, 250 );
            layout.ConnectNodes( res1.NodeA, res2.NodeA, new Connector() );
            layout.ConnectNodes( res2.NodeA, res3.NodeA, new Connector() );
            layout.ConnectNodes( bat.NodeA, res3.NodeA, new Connector() );
            layout.ConnectNodes( res1.NodeB, res2.NodeB, new Connector() );
            layout.ConnectNodes( res2.NodeB, res3.NodeB, new Connector() );
            layout.ConnectNodes( bat.NodeB, res3.NodeB, new Connector() );
        }

        //private void btnGetExperimentData_Click( object sender, RoutedEventArgs e )
        //{
        //    var experimentsData = edExperimentData.GetData();
        //    //todo: write to xls file
        //}

        private void tbInstructions_Loaded( object sender, RoutedEventArgs e )
        {
            string path = "./Data/instructions.txt";

            try
            {
                string[] t = System.IO.File.ReadAllLines( path );
                tbInstructions.Text = string.Join( " ", t );
            }
            catch ( System.IO.FileNotFoundException )
            {
                System.IO.File.Create( path );
            }
        }

        private void btnResultCheck_Click( object sender, RoutedEventArgs e )
        {
            
        }

        private void Window_KeyDown( object sender, KeyEventArgs e )
        {
            clcLayout.Layout_KeyDown( sender, e );
        }


    }
}
