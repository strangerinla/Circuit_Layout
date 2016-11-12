using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Circuit_Layout
{
    class Layout
    {
        #region Singleton
        private static Layout instance;
        public static Layout GetInstance()
        {
            if ( instance == null )
                instance = new Layout();
            return instance;
        }
        #endregion
        #region Determination
        private Layout()
        {
            Nodes = new ObservableCollection<Node>();
            Connectors = new ObservableCollection<Connector>();
        }
        #endregion
        #region Properties
        public ObservableCollection<Node> Nodes { get; set; }
        public ObservableCollection<Connector> Connectors { get; set; }
        #endregion
        #region Methods
        #region Elements
        public Element CreateElement( Type type, int x, int y )
        {
            if ( type.BaseType.Name == "Element" )
            {
                if ( type.Name == "ResistorX" && Connectors.Count( i => i is ResistorX ) > 0 )
                    throw new ResistorXPlacedException();

                var element = Activator.CreateInstance( type ) as Element;              //создание элемента

                Node pina = new Node();                                                 //создание пинов
                Node pinb = new Node();

                if ( type.Name == "Reohord" )                                           //реохорд
                {
                    Node pinc = new Node();

                    Reohord reohord = element as Reohord;
                    reohord.ResistorAC = new NoDrawConnector() { Resistance = 100 };    //создание соединителей
                    reohord.ResistorBC = new NoDrawConnector();
                    ConnectNodes( pina, pinc, reohord.ResistorAC );                     //соединение пинов при помощи коннекторов
                    ConnectNodes( pinb, pinc, reohord.ResistorBC );
                    reohord.NodeA = pina;                                               //привязка ножек к нодам
                    reohord.NodeB = pinb;
                    reohord.NodeC = pinc;
                    Nodes.Add( reohord.NodeC );                                         //отрисовка нода C
                    Connectors.Add( element );                                          //отрисовка реохорда
                }
                else
                {
                    ConnectNodes( pina, pinb, element );                                //соединение пинов при помощи элемента
                }
                Nodes.Add( element.NodeA );                                             //отрисовка нодов
                Nodes.Add( element.NodeB );

                element.X = x;                                                          //перемещение по координатам (учитывая обновление свойств X и Y у пинов)
                element.Y = y;
                return element;
            }
            return null;
        }

        public void ConnectNodes( Node nodea, Node nodeb, Connector connector )
        {
            if ( ( nodea.Connections.Keys.Count( i => i == nodeb ) > 0 ) || ( nodeb.Connections.Keys.Count( i => i == nodea ) > 0 ) )
                return;

            nodea.Connections.Add( nodeb, connector );
            nodeb.Connections.Add( nodea, connector );
            connector.NodeA = nodea;
            connector.NodeB = nodeb;

            Connectors.Add( connector );
        }
        public void RemoveElement( Element element )
        {
            Node nodea = element.NodeA;
            Node nodeb = element.NodeB;
            if ( element is Reohord )
            {
                Reohord reohord = element as Reohord;
                Node nodec = reohord.NodeC;
                RemoveConnector( reohord.ResistorAC );
                RemoveConnector( reohord.ResistorBC );
                Connectors.Remove( reohord );
                Nodes.Remove( nodec );
            }
            else
            {
                RemoveConnector( element );
            }

            Nodes.Remove( nodea );                                                      //удаление пинов из отрисовки
            Nodes.Remove( nodeb );

            while ( nodea.Connections.Count > 0 )// ( Connector item in nodea.Connections.Values )
            {
                RemoveConnector( nodea.Connections.Values.First() );
            }
            while ( nodeb.Connections.Count > 0 )//foreach ( Connector item in nodeb.Connections.Values )
            {
                RemoveConnector( nodeb.Connections.Values.First() );
            }
        }
        public void RemoveConnector( Connector connector )
        {
            Node nodea = connector.NodeA;
            Node nodeb = connector.NodeB;

            Connectors.Remove( connector );                                             //удаление из отрисовки

            nodea.Connections.Remove( nodeb );                                          //отвязка соединителя
            nodeb.Connections.Remove( nodea );
        }
        #endregion
        #region Nodes
        public void CreateNode( Connector connector, Point position )
        {
            Node node = new Node( (int)position.X, (int)position.Y );
            ConnectNodes( node, connector.NodeA, new Connector() );
            ConnectNodes( node, connector.NodeB, new Connector() );

            RemoveConnector( connector );

            Nodes.Add( node );
        }
        public void RemoveNode( Node node )
        {
            if ( node.Connections.Values.Count( i => i is Element ) > 0 )
                return;

            while ( node.Connections.Count > 0 )
            {
                RemoveConnector( node.Connections.Values.First() );
            }
            Nodes.Remove( node );
        }
        #endregion
        #region Update
        private void ResetNodesState()
        {
            for ( int i = 0; i < Nodes.Count; i++ )
            {
                Nodes[i].Number = i;
                Nodes[i].Visited = false;
            }
        }
        public void Update( ref string log )  //КОСТЫЛЬ!
        {
            ResetNodesState();                                                                      // Сброс состояния нод, присвоение им номеров

            List<List<Node>> Chains = GetNodesChains();                                             // Список цепей (от одного узла до другого)
            List<List<Element>> ElementsInChains = Chains                                           // Список цеаей (по элементам)
                .Select( i => GetElementsIn( i ) )
                .ToList();

            List<List<Node>> Contours = GetNodesContours();                                         // Список замкнутых контуров (по нодам)
            List<List<Element>> ElementsInContours = Contours                                        // Список замкнутых контуров (по элементам)
                .Select( i => GetElementsIn( i ) )

                .ToList();

            int matrixSize = Chains.Count;                                                          // Размер матрицы (количество неизвестных)
            int rule1Count = Nodes.Count( i => i.Connections.Count > 2 ) - 1;                       // Количество уравнений по первому правилу
            int rule2Count = matrixSize - rule1Count;                                             // Количество уравнений по второму правилу

            double[][] countMatrix = new double[matrixSize]                                         // Матрица для вычисления токов
                .Select( i => new double[matrixSize + 1] )
                .ToArray();

            for ( int i = 0; i < matrixSize; i++ )
            {
                for ( int j = 0; j < matrixSize; j++ )
                {
                    if ( i < rule1Count )
                    {
                        countMatrix[i][j] = 0;
                    }
                    else
                    {

                        countMatrix[i][j] = ElementsInChains[j].Sum( el => el.Resistance );
                    }
                }
                countMatrix[i][matrixSize] = i < rule1Count ? 0 : ElementsInChains[i]
                    .Where( el => el is Battery )
                    .Sum( el => ( el as Battery ).Eds * ( Chains[i].FirstOrDefault( pin => pin == el.NodeA || pin == el.NodeB ) == el.NodeA ? -1 : 1 ) );
            }

            // -=[DEBUG]=- //

            //log += string.Join( "|", Contours.Select( i => string.Join( ", ", i.Select( j => j.Number ) ) ) );
            log += string.Join( "|", Chains.Select( i => string.Join( ", ", i.Select( j => j.Number ) ) ) );

            //log += string.Join( "|", ElementsInContours.Select( i => string.Join( ", ", i.Select( j => j.Name ) ) ) );
            //log += string.Join( "|", ElementsInChains.Select( i => string.Join( ", ", i.Select( j => j.Name ) ) ) );

            log += "\n" + string.Join( " ", matrixSize, rule1Count, rule2Count );

            log += "\n" + string.Join( "\n", countMatrix.Select( i => string.Join( " ", i ) ) );
        }
        #endregion
        #region GetNodesChains
        private List<List<Node>> GetNodesChains()
        {
            ResetNodesState();                                                                              // Сбрасываем состояние нод

            List<List<Node>> chains = new List<List<Node>>();                                               // Список цепей

            foreach ( Node node in Nodes.Where( node => node.Connections.Count > 2 ) )                      // Для каждого узла
            {
                foreach ( Node nextNode in node.Connections.Keys.Where( i => !i.Visited ) )                 // Для каждой непосещенной ноды из смежных
                {
                    List<Node> chain = GetNodesChain( nextNode, node );                                     // Залезаем в цепочку
                    if ( chain != null )                                                                    // "Есть чё?"
                        chains.Add( chain );                                                                // Есть? Ну тогда "Добро пожаловать в стаю!"
                }
                node.Visited = true;                                                                        // Узел пройден
            }
            return chains;
        }
        private List<Node> GetNodesChain( Node node, Node prevNode, bool hasElement = false )
        {
            List<Node> chain = new List<Node>();
            chain.Add( prevNode );                                                                          // Добавляем текущую ноду в цепь
            hasElement |= node.Connections[prevNode] is Element;                                            // Нашли элемент?

            if ( node.Connections.Count > 2 )                                                               // Если у ноды >2 ног - ЛИВАЕМ! МУТАНТЫ!
            {
                chain.Add( node );
                return hasElement ? chain : null;                                                           // Если ничего ценного нет - бросаем всё найденное!
            }

            if ( node.Connections.Count < 2 )                                                               // Если меньше двух ног - ВАЛИМ, БРОСАЯ ВСЁ!
                return null;                                                                                // (она не догонит... но вдруг догонит та, что у этой ногу оттяпала)

            node.Visited = true;                                                                            // Хлебные крошки 

            Node nextnode = node.Connections.Keys.FirstOrDefault( i => i != prevNode );                     // Следующая нода
            if ( nextnode == null )
                return null;

            List<Node> nextNodeChains = GetNodesChain( nextnode, node, hasElement );                        // Уходим глубже

            return nextNodeChains == null ? null : chain.Concat( nextNodeChains ).ToList();                 // Если удираем бросив всё, то продолжаем удирать. 
        }
        #endregion
        
        #region GetNodesContours
        private List<List<Node>> GetNodesContours()
        {
            List<Node> temp = new List<Node>();
            List<List<Node>> contours = new List<List<Node>>();

            ResetNodesState();

            temp.Add( Nodes[0] );
            bool done = false;

            while ( !done )
            {
                while ( temp.Count > 0 )
                {
                    GetContoursInternal( temp.Last(), ref contours, ref temp );
                }

                done = true;
                foreach ( Node node in Nodes )
                {
                    if ( !node.Visited )
                    {
                        temp.Add( node );
                        done = false;
                        break;
                    }
                }
            }
            return contours;
        }
        private void GetContoursInternal( Node node, ref List<List<Node>> contours, ref List<Node> temp )
        {
            node.Visited = true;

            foreach ( Node connectedNode in node.Connections.Keys )
            {
                if ( !connectedNode.Visited )
                {
                    temp.Add( connectedNode );
                    GetContoursInternal( connectedNode, ref contours, ref temp );
                }
                else if ( temp.IndexOf( connectedNode ) != -1 )
                {
                    List<Node> newPath = new List<Node>();
                    int startIndex = temp.IndexOf( connectedNode );

                    for ( int i = startIndex; i < temp.Count; i++ )
                    {
                        newPath.Add( temp[i] );
                    }
                    if ( newPath.Count > 2 )            // исключаем одиночные элементы
                        contours.Add( newPath );
                }
            }
            temp.Remove( node );
        }
        #endregion
        #region GetElementsIn
        private List<Element> GetElementsIn( List<Node> contour )
        {
            List<Element> elements = new List<Element>();
            for ( int i = 0; i < contour.Count - 1; i++ )
            {
                if ( contour[i].Connections[contour[i + 1]] is Element )
                    elements.Add( contour[i].Connections[contour[i + 1]] as Element );
            }

            return elements;
        }
        #endregion
        #region GetValue
        public double GetResistorXResistance()
        {
            ResistorX resistorx = Connectors.FirstOrDefault( i => i is ResistorX ) as ResistorX;
            return resistorx == null ? double.NaN : resistorx.Resistance;
        }
        #endregion
        #endregion
        #region Exceptions
        [Serializable]
        public class ResistorXPlacedException : Exception           // Rx уже существует
        {
            public ResistorXPlacedException() : base( "X-Resistor is already on the layout" ) { }
            public ResistorXPlacedException( string message ) : base( message ) { }
            public ResistorXPlacedException( string message, Exception inner ) : base( message, inner ) { }
            protected ResistorXPlacedException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context )
                : base( info, context ) { }
        }
        [Serializable]
        public class WrongConnectionsException : Exception
        {
            public WrongConnectionsException() { }
            public WrongConnectionsException( string message ) : base( message ) { }
            public WrongConnectionsException( string message, Exception inner ) : base( message, inner ) { }
            protected WrongConnectionsException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context )
                : base( info, context ) { }
        }
        #endregion
    }
}
