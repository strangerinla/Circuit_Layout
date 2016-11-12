using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace Circuit_Layout
{
    /// <summary>
    /// Логика взаимодействия для EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        #region Determination
        public EditWindow()
        {
            InitializeComponent();
            tbEds.CommandBindings.Add( new CommandBinding( ApplicationCommands.Paste, OnPasteCommand ) );
            tbResistance.CommandBindings.Add( new CommandBinding( ApplicationCommands.Paste, OnPasteCommand ) );
            tbDivision.CommandBindings.Add( new CommandBinding( ApplicationCommands.Paste, OnPasteCommand ) );
        }
        #endregion
        #region Load
        private void Window_Loaded( object sender, RoutedEventArgs e )
        {
            Element element = Tag as Element;

            this.Title = "Edit " + ( element is ResistorX ? "X-Resistor" : element.GetType().Name );

            tbName.Text = element.Name;

            if ( element is Resistor )
            {
                spResistance.Visibility = System.Windows.Visibility.Visible;
                tbResistance.Text = element.Resistance.ToString();
                Height = 130;
            }
            else if ( element is Reohord )
            {
                spResistance.Visibility = System.Windows.Visibility.Visible;
                spReohordDisplayMode.Visibility = System.Windows.Visibility.Visible;
                Reohord reohord = element as Reohord;
                tbResistance.Text = reohord.Resistance.ToString();
                cbReohordDisplayMode.SelectedIndex = reohord.DisplayLength ? 0 : 1;
                Height = 160;
            }
            else if ( element is Battery )
            {
                spEds.Visibility = System.Windows.Visibility.Visible;
                spResistance.Visibility = System.Windows.Visibility.Visible;
                Battery battery = element as Battery;
                tbEds.Text = battery.Eds.ToString();
                tbResistance.Text = battery.Resistance.ToString();
                Height = 160;
            }
            else if ( element is Galvanometr )
            {
                spDivision.Visibility = System.Windows.Visibility.Visible;
                Galvanometr galvanometr = element as Galvanometr;
                tbDivision.Text = galvanometr.Division.ToString();
                Height = 130;
            }
        }
        #endregion
        #region Methods/Buttons
        private void ButtonOK_Click( object sender, RoutedEventArgs e )
        {
            SaveData();
            this.Close();
        }
        private void SaveData()
        {
            Element element = Tag as Element;
            element.Name = tbName.Text;

            if ( element is Resistor )
            {
                Resistor resistor = element as Resistor;
                resistor.Resistance = double.Parse( tbResistance.Text );
            }
            else if ( element is Reohord )
            {
                Reohord reohord = element as Reohord;
                reohord.Resistance = double.Parse( tbResistance.Text );
                reohord.DisplayLength = cbReohordDisplayMode.SelectedIndex == 0;
            }
            else if ( element is Battery )
            {
                Battery battery = element as Battery;
                battery.Resistance = double.Parse( tbResistance.Text );
                battery.Eds = double.Parse( tbEds.Text );
            }
            else if ( element is Galvanometr )
            {
                Galvanometr galvanometr = element as Galvanometr;
                galvanometr.Division = double.Parse( tbDivision.Text );
            }
        }
        private void ButtonCancel_Click( object sender, RoutedEventArgs e )
        {
            this.Close();
        }

        private void Window_KeyDown( object sender, KeyEventArgs e )
        {
            if ( e.Key == Key.Enter )
            {
                SaveData();
                this.Close();
            }
            else if ( e.Key == Key.Escape )
            {
                this.Close();
            }
        }
        #endregion
        #region Poka-yoke
        private void DoubleTextBox_PreviewTextInput( object sender, TextCompositionEventArgs e )
        {
            TextBox tb = sender as TextBox;
            e.Handled = "0123456789,".IndexOf( e.Text ) < 0 || (e.Text=="," && tb.Text.Count(i=>i==',') > 0);
        }
        public void OnPasteCommand( object sender, ExecutedRoutedEventArgs e )
        {
            TextBox textbox = sender as TextBox;

            string t = textbox.Text.Remove( textbox.SelectionStart, textbox.SelectionLength );
            t = t.Insert( textbox.SelectionStart, Clipboard.GetText() );

            double parseResult;
            if ( double.TryParse( t, out parseResult ) )
            {
                textbox.Text = parseResult.ToString();
                textbox.SelectionStart = textbox.Text.Length;
            }
        }
        #endregion
    }
}
