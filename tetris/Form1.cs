using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace tetris
{
    public partial class Form1 : Form
    {
        Label[,] tablero = new Label[20, 10]; // tablero visible
        bool[,] atrastablero = new bool[20, 10]; // tablero funcional
        bool[,] Figura = new bool[4, 4];// matriz donde haremos las figuras
        int figuraX = 0; // posicion  inicial en la fila 0 
        int figuraY = 3;//poscicion  inicial en la columna 3
        Thread gameThread; // hilo de juego
        bool running = true; // me indica si he perdido
        int puntaje = 0; // aqui sumaremos nuestros puntajes
        Label puntajetexto=new Label(); // caja de texto donde se vera el puntaje
        Color figuraColor; // variable del color de la figura
        int incre = 0; // sera nuestro incremento de tiempo
        public Form1()
        {
            InitializeComponent();

            // aqui podras encontrar las captura del evento del teclado en las flechas 
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(Direccion);
             // inicializar juego llamando a los metodos inicializar tablero e iniciando el hilo
            InicializarTablero();
            Figura = ObtenerNuevaFigura();

            gameThread = new Thread(Jugar);
            gameThread.Start();

            ActualizarTablero();
        }

        // metodo para iniciar tablero
        private void InicializarTablero()
        {
            //recooremos el tablero
            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    tablero[i, j] = new Label(); //llenamos el tablero visible de labels los cuales tendran un color gris
                    tablero[i, j].Size = new Size(35, 35);
                    tablero[i, j].Location = new Point(j * 35, i * 35);
                    tablero[i, j].Font = new Font("Arial", 36);
                    this.Controls.Add(tablero[i, j]);
                    tablero[i, j].BackColor = Color.Gray;
                    tablero[i, j].BorderStyle = BorderStyle.Fixed3D;
                }
            }
            puntajetexto.Text = "Puntaje: "+puntaje; // aqui mostrsremos nuestro puntaje en la parte baja de nuestro tetris
            puntajetexto.Size = new Size(150, 60);
            puntajetexto.Font = new Font("Arial", 17);
            puntajetexto.Location = new Point(10, 730);
            this.Controls.Add(puntajetexto);
            this.Size = new Size(10 * 35 + 20, 20 * 40 + 40); //aqui definimos el tamaño del form
        }

        private bool[,] ObtenerNuevaFigura()
        {
            // en este metodo escogeremos una figrura de forma aleatoria entre las 7 figuras que hay a traves de un switch case
            Random rdm = new Random();  
            int opc = rdm.Next(1, 8);
            bool[,] nuevaFigura = new bool[4, 4];
            switch (opc)
            {
                case 1: // ficha de palo
                    for (int j = 0; j < 4; j++) { nuevaFigura[0, j] = true; }
                    break;
                case 2: // ficha L invertida
                    nuevaFigura[1, 0] = nuevaFigura[1, 1] = nuevaFigura[1, 2] = nuevaFigura[0, 2] = true;
                    break;
                case 3: // ficha L
                    nuevaFigura[0, 0] = nuevaFigura[0, 1] = nuevaFigura[0, 2] = nuevaFigura[1, 2] = true;
                    break;
                case 4: // ficha Cuadrado
                    nuevaFigura[0, 0] = nuevaFigura[0, 1] = nuevaFigura[1, 0] = nuevaFigura[1, 1] = true;
                    break;
                case 5: // Z invertida
                    nuevaFigura[0, 1] = nuevaFigura[0, 2] = nuevaFigura[1, 0] = nuevaFigura[1, 1] = true;
                    break;
                case 6: // Z normal
                    nuevaFigura[0, 0] = nuevaFigura[0, 1] = nuevaFigura[1, 1] = nuevaFigura[1, 2] = true;
                    break;
                case 7: // ficha T
                    nuevaFigura[0, 0] = nuevaFigura[0, 1] = nuevaFigura[0, 2] = nuevaFigura[1, 1] = true;
                    break;
            }
            // Asignar un color aleatorio a la figura
            figuraColor = Color.FromArgb(rdm.Next(256), rdm.Next(256), rdm.Next(256));
            figuraX = 0; // reiniciamos posición inicial de la ficha
            figuraY = 3;// reiniciamos posición inicial de la ficha
            return nuevaFigura;
        }

        // metodo que define si las figuras se pueden mover con las teclas del teclado
        private void Direccion(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left && PuedeMover(0, -1)) // tecla de rayita izquierda 
            {
                figuraY--; // ir a izquierda
            }
            else if (e.KeyCode == Keys.Right && PuedeMover(0, 1))// tecla rayita derecha 
            {
                figuraY++;// ir a derecha
            }
            else if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Space) // si toca espacio o tecla rayita arriba
            {
                Rotar(); // llamamos el metodo rotar
            }
            ActualizarTablero(); // llamamos el metodo para actualizar el tablero
        }


        // metodo que me define los limites en los que me puedo mover
        private bool PuedeMover(int dx, int dy)
        {
            for (int i = 0; i < 4; i++) // recorremos la figura
            {
                for (int j = 0; j < 4; j++)
                {
                    if (Figura[i, j])
                    {
                        int nuevaX = figuraX + i + dx; //antes de dar el paso evaluo si se puede
                        int nuevaY = figuraY + j + dy;//antes de dar el paso evaluo si se puede
                        if (nuevaX < 0 || nuevaX >= 20 || nuevaY < 0 || nuevaY >= 10 || atrastablero[nuevaX, nuevaY]) // se puede mover si  esta en los limites de atras tablero 
                        {
                            return false; // no me muevo
                        }
                    }
                }
            }
            return true; // sino esta en los limites me muevo
        }


        // metodo que me permite rotar una figura
        private void Rotar()
        {
            // co´pia de nuestra figura
            bool[,] figuraAux = new bool[4, 4];
            Array.Copy(Figura, figuraAux, Figura.Length);

            // Dimensiones de la matriz de la ficha
            int m = Figura.GetLength(0);
            int n = Figura.GetLength(1);

            // Girar la ficha 90 grados  en su misma posición en una nueva matriz 
            bool[,] figuraRotada = new bool[n, m];
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    figuraRotada[j, m - 1 - i] = figuraAux[i, j];
                }
            }

            if (PuedeRotar(figuraRotada)) // llamamos al metodo puede rotar a ver si si podemos rotarla nuevamente
            {
                Figura = figuraRotada; // si es así la figura rota
            }
        }

        //metodo para saber si LA FIGURA PUEDE rotar
        private bool PuedeRotar(bool[,] figuraRotada)
        {
            for (int i = 0; i < 4; i++) // recorremos la figura
            {
                for (int j = 0; j < 4; j++)
                {
                    if (figuraRotada[i, j])   
                    {
                        // evaluamos donde esta la figura ahora
                        int nuevaX = figuraX + i;
                        int nuevaY = figuraY + j;
                        if (nuevaX < 0 || nuevaX >= 20 || nuevaY < 0 || nuevaY >= 10 || atrastablero[nuevaX, nuevaY])//Verificamos si alguna de las rotaciones se sale de los limites
                        {
                            return false; // si entro significa que no lo podemos rotar
                        }
                    }
                }
            }
            return true;
        }

        // metodo para actualizar el tablero 
        private void ActualizarTablero()
        {
            // si  esta en nuestro hilo invoke  actualizamos 
            if (InvokeRequired)
            {
                Invoke(new Action(ActualizarTablero));
                return;
            }

            // actualizaremos el tablero sin afectar la fichas que estan fijas
            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (atrastablero[i, j])
                    {
                        tablero[i, j].BackColor = Color.Blue; // celda ocupada se pone color azul
                    }
                    else
                    {
                        tablero[i, j].BackColor = Color.Gray; // pintar de gris el color del tablero porque la celda esta vacia
                    }
                }
            }

            // dibujar la figura  en el  tablero con un color aleatorio
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (Figura[i, j])
                    {
                        int fila = figuraX + i;
                        int columna = figuraY + j;
                        if (fila >= 0 && fila < 20 && columna >= 0 && columna < 10)
                        {
                            tablero[fila, columna].BackColor = figuraColor;
                        }
                    }
                }
            }
        }
 
        // metodo para eliminar una fila 
        private bool Eliminarfila()
        {
            bool filaEliminada = false; // nuestra fila comienza en false puesto que aun no se ha eliminado

            for (int i = 19; i >= 0; i--) // recorreremos la matriz de abajo a arriba
            {
                bool filaCompleta = true; // booleano que me indica si la matriz se lleno
                for (int j = 0; j < 10; j++) // recorremos la fila seleccionada de derecha a izquierda
                {
                    //si encuentra un falso en el atras tablero ( tablero funcional) significa que la fila no esta completa y no lo podre eliminar
                    if (!atrastablero[i, j]) 
                    {
                        filaCompleta = false; 
                        break;
                    }
                }

                // Si la fila está completamente llena (todos true), proceder a eliminarla 
                if (filaCompleta)
                {
                    // Desplaza todas las filas de arriba hacia abajo
                    for (int k = i; k > 0; k--)
                    {
                        for (int j = 0; j < 10; j++)
                        {// Mueve la fila superior a la posición actual
                            atrastablero[k, j] = atrastablero[k - 1, j];
                        }
                    }
                    // Vacía la fila superior después de haber desplazado las demás
                    for (int j = 0; j < 10; j++)
                    {
                        atrastablero[0, j] = false;
                    }

                    i++; // Revisar la misma fila nuevamente

                    filaEliminada = true;// Marca que se ha eliminado una fila
                    puntaje = puntaje + 10; // si se elimina la fila sumamos el puntaje
                     // Actualiza el texto del puntaje en la interfaz 
                    this.Invoke((MethodInvoker)delegate
                    {
                        // mostramos el puntaje actualizado
                        puntajetexto.Text = "Puntaje: " + puntaje.ToString();
                    }
                );
                    // si eliminamos nuestra fila nuestra velocidad de caida de las fichas aumenta
                    incre += 10;
                }
            }

            return filaEliminada;
        }
        private void Jugar()
        {
            // Mientras el usuario no haya perdido cuando se genere una nueva figura este medodo la va a ir bajando
            while (running)
            {
                Thread.Sleep(500-incre); //Inicialmente cada medio segundo baja una casilla peroa a medida que elimina filas el tiempo se reduce
                Eliminarfila();//Cada vez que baja una casilla se verifica si se puede eliminar una fila
                if (PuedeMover(1, 0)) // verificamos que no haya nada debajo de la ficha que esta cayendo y bajamos una posicion
                {
                    figuraX++;
                }
                else
                {
                    //Si no podemos baja mas, fijamos la figura, y obtenemos una nueva figura
                    FijarFigura(); 
                    Figura = ObtenerNuevaFigura();
                    if (!PuedeMover(0, 0)) // si ya no podemos movernos mas significa que perdimos, entonces finalizamos el ciclo while y mostramos que perdimos
                    {
                        // perder
                        running = false;
                        MessageBox.Show("Game Over");
                        break;
                    }
                }
                ActualizarTablero(); // Actualizamos el tablero en cada movimiento
            }
        }
        //Este metodo fija una figura en un lugar
        private void FijarFigura()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (Figura[i, j])
                    {
                        atrastablero[figuraX + i, figuraY + j] = true; //Fija la figura en la posicion en la que detecta que ya no piede bajar mas
                    }
                }
            }
        }

        // definimos la clase 
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            //Si se cierra el Formulario entonces cierra el ciclo while y el thread
            running = false;
            gameThread.Join();
            base.OnFormClosing(e);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
    }

}