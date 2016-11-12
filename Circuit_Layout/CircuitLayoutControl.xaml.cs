using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для CircuitLayoutControl.xaml
    /// </summary>
    public partial class CircuitLayoutControl : UserControl
    {
        #region Determination
        public CircuitLayoutControl()
        {
            InitializeComponent();
        }
        #endregion
        #region ContextMenu
        #region isContextMenuShown
        private bool isContextMenuShown = false;
        private void ContextMenu_ContextMenuOpening( object sender, ContextMenuEventArgs e )
        {
            if ( (bool)cbRun.IsChecked )
            {
                e.Handled = true;
                return;
            }
            isContextMenuShown = true;
        }

        private void ContextMenu_ContextMenuClosing( object sender, ContextMenuEventArgs e )
        {
            isContextMenuShown = false;
        }
        #endregion
        #region ContextMenuItems
        private void ContextMenu_Edit( object sender, RoutedEventArgs e )
        {
            isContextMenuShown = false;
            MenuItem menuItem = sender as MenuItem;
            ContextMenu cmenu = menuItem.Parent as ContextMenu;
            Grid grid = cmenu.PlacementTarget as Grid;
            Element element = grid.DataContext as Element;

            Window window = new EditWindow()
            {
                Tag = element,
                Owner = this.Owner,
            };

            window.ShowDialog();
        }

        private void ContextMenu_Remove( object sender, RoutedEventArgs e )
        {
            isContextMenuShown = false;
            MenuItem menuItem = sender as MenuItem;
            ContextMenu cmenu = menuItem.Parent as ContextMenu;
            Grid grid = cmenu.PlacementTarget as Grid;

            Layout layout = Layout.GetInstance();

            if ( grid.DataContext is Element )
            {
                Element element = grid.DataContext as Element;
                layout.RemoveElement( element );
            }
            else if ( grid.DataContext is Connector )
            {
                Connector connector = grid.DataContext as Connector;
                layout.RemoveConnector( connector );
            }
        }
        #endregion
        #endregion
        #region Element
        #region CreateElement
        private void Layout_MouseDown( object sender, MouseButtonEventArgs e )
        {
            if ( (bool)cbRun.IsChecked )
                return;

            if ( e.LeftButton == MouseButtonState.Pressed && !isContextMenuShown )             // создание элемента
            {
                Layout layout = Layout.GetInstance();
                Point pos = e.GetPosition( icLayout );

                Type[] types = new Type[] {
                typeof(Resistor),
                typeof(Battery),
                typeof(Galvanometr),
                typeof(Reohord),
                typeof(ResistorX),
                };
                Element element;
                try                                                                             //проверка на существование Rx
                {
                    element = layout.CreateElement( types[lbElements.SelectedIndex], (int)pos.X, (int)pos.Y );
                    movingElement = element;
                }
                catch ( Layout.ResistorXPlacedException ex )
                {
                    MessageBox.Show( "ERROR: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error );
                }

            }
            if ( e.RightButton == MouseButtonState.Pressed )
            {
                if ( movingElement != null )                              // удаление перетаскиваемого элемента
                {
                    Layout layout = Layout.GetInstance();
                    layout.RemoveElement( movingElement );

                    movingElement = null;
                    if ( selectedNode != null )
                    {
                        selectedNode.IsSelected = false;
                        selectedNode = null;
                    }
                }
            }
        }
        #endregion
        #region SelectElement
        private Element movingElement;
        private void Element_MouseDown( object sender, MouseButtonEventArgs e )
        {
            if ( (bool)cbRun.IsChecked )
                return;

            Grid grid = sender as Grid;
            Element element = grid.DataContext as Element;

            if ( e.LeftButton == MouseButtonState.Pressed )
            {
                movingElement = movingElement == null ? element : null;
                e.Handled = true;
            }
        }
        #endregion
        #region ElementMove
        private void Layout_MouseMove( object sender, MouseEventArgs e )
        {
            if ( movingElement == null )
                return;

            Point position = e.GetPosition( icLayout );

            double width = Math.Abs( movingElement.OffsetB * Math.Cos( movingElement.Angle * Math.PI / 180 ) );
            double height = Math.Abs( movingElement.OffsetB * Math.Sin( movingElement.Angle * Math.PI / 180 ) );

            if ( position.X > width + 5 && position.X < icLayout.ActualWidth - width - 5 )
                movingElement.X = (int)position.X;
            if ( position.Y > height + 25 && position.Y < icLayout.ActualHeight - height - 10 )
                movingElement.Y = (int)position.Y;
        }
        #endregion
        #endregion
        #region Node
        Node selectedNode;
        private void Node_MouseDown( object sender, MouseButtonEventArgs e )
        {
            if ( (bool)cbRun.IsChecked )
                return;

            Canvas canv = sender as Canvas;
            Node node = canv.DataContext as Node;
            Layout layout = Layout.GetInstance();

            if ( e.LeftButton == MouseButtonState.Pressed )
            {
                if ( selectedNode == null )
                {
                    selectedNode = node;
                    node.IsSelected = true;
                }
                else
                {
                    if ( selectedNode != node )
                    {
                        layout.ConnectNodes( selectedNode, node, new Connector() );
                    }
                    selectedNode.IsSelected = false;
                    selectedNode = null;
                }
            }
            if ( e.RightButton == MouseButtonState.Pressed )
            {
                //todo Context Menu!
                layout.RemoveNode( node );
            }
            e.Handled = true;
        }
        #endregion
        #region Keyboard
        public void Layout_KeyDown( object sender, KeyEventArgs e )
        {
            //MessageBox.Show( e.Key.ToString() );
            switch ( e.Key )
            {
                case Key.OemPlus:
                case Key.Add:
                    if ( movingElement != null )
                        movingElement.Angle += 15;
                    break;
                case Key.OemMinus:
                case Key.Subtract:
                    if ( movingElement != null )
                        movingElement.Angle -= 15;
                    break;

            }
        }

        #endregion
        #region Run
        private void ButtonRUN_Click( object sender, RoutedEventArgs e )
        {
            Button button = sender as Button;

            cbRun.IsChecked = !cbRun.IsChecked;

            if ( (bool)cbRun.IsChecked )
            {
                button.Content = "Stop Simulation";
                movingElement = null;
                if ( selectedNode != null )
                {
                    selectedNode.IsSelected = false;
                    selectedNode = null;
                }
            }
            else
            {
                button.Content = "Run Simulation";
            }

            Layout layout = Layout.GetInstance();
            string log = "";
            layout.Update( ref log );
            tbLog.Text = log;
        }
        #endregion
        #region Reohord
        private void ReohordSlider_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
        {
            Slider slider = sender as Slider;
            Reohord reohord = slider.DataContext as Reohord;
            reohord.LengthAC = slider.Value;
        }
        #endregion
        #region Properties
        public Window Owner { get; set; }
        #endregion
        private void DebugButton_Click( object sender, RoutedEventArgs e )
        {
            Layout layout = Layout.GetInstance();

            var t = layout.GetResistorXResistance();
        }

        private void slDebug_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
        {
            Layout layout = Layout.GetInstance();
            Galvanometr galv = layout.Connectors.FirstOrDefault( i => i is Galvanometr ) as Galvanometr;
            if ( galv != null )
                galv.ElectricCurrent = e.NewValue;
        }
        public double icLayoutX { get; set; }
        public double icLayoutY { get; set; }
        private void vbLayout_PreviewMouseWheel( object sender, MouseWheelEventArgs e )
        {
            icLayout.Width -= icLayout.Width - e.Delta < 1500 && icLayout.Width - e.Delta > 200 ? e.Delta / 10d : 0;
            icLayout.Height -= icLayout.Height - e.Delta < 1500 && icLayout.Height - e.Delta > 200 ? e.Delta / 10d : 0;
        }
    }
}
