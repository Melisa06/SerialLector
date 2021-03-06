<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//Libreria para la lectura del puerto serial. 
using System.IO.Ports;
using System.IO;
using System.Threading;


namespace BunthiV5
{
    public partial class Form1 : Form
    {

        //Variables a usar 
        //Este es un list privado para que siempre tenga los valores sea cual sea la acción que se realice
        //O al menos en esta clase
        private List<Datos> datosTxt = new List<Datos>();
        //Variable para dar acceso a la pantalla de Bunthi 
        private delegate void DelegadoAcceso(string accion);
        //Variables para la entrada (In) y salida (Out) de datos del serial 
        private string strBufferIn;
        private string strBufferOut;
        //Variable para la fecha actual
        private DateTime fecha = DateTime.Now;
        //Variable para el tiempo a comparar 
        private TimeSpan aumentoTiempo = new TimeSpan(0, 0, 0);
        //Variables para asignar la ruta donde se guardará el archivo a generar. 
        FolderBrowserDialog folderBrowse = new FolderBrowserDialog();
        private string ruta;
        //Variables de Tiempo
        private string tiempoEspera;
        private TimeSpan time = new System.TimeSpan();

        /*
         * Contructor 
         */
        public Form1()
        {
            InitializeComponent();
            //tiempos de espera para tomar los datos
            cmbTiempo.Items.Add(time = new TimeSpan(0, 0, 1));
            cmbTiempo.Items.Add(time = new TimeSpan(0, 0, 5));
            cmbTiempo.Items.Add(time = new TimeSpan(0, 0, 10));
            cmbTiempo.Items.Add(time = new TimeSpan(0, 0, 20));
            cmbTiempo.Items.Add(time = new TimeSpan(0, 0, 30));
            cmbTiempo.Items.Add(time = new TimeSpan(0, 0, 40));
            cmbTiempo.Items.Add(time = new TimeSpan(0, 0, 50));
            cmbTiempo.Items.Add(time = new TimeSpan(0, 1, 0));
            cmbTiempo.Items.Add(time = new TimeSpan(0, 5, 0));
            cmbTiempo.Items.Add(time = new TimeSpan(0, 10, 0));




        }

        //-----------------GET´S Y SET´S 
        public class Datos
        {
            public string Caja1 { get; set; }
            public string Caja2 { get; set; }
            public string Caja3 { get; set; }
            public string Caja4 { get; set; }
            public string Temp { get; set; }
            public string Lum { get; set; }
            public string Fecha { get; set; }
            public string Hora { get; set; }
        }

        //-------------------------METODOS A UTILIZAR ---------------------------
        /*
        *Este metodo nos sirve para comparar los minutos asignados en aumentoTiempo
        *contra los que estan pasando en tiempo real 
        */
        public bool ComparacionTiempo(TimeSpan aumento)
        {

            if ((fecha + time).ToString("mm:ss") == DateTime.Now.ToString("mm:ss"))
            {

                fecha = fecha + time;

                return true;
            }

            else return false;
        }//cierra Metodo ComparacionTiempo

        /*
         *Primer metodo para dar acceso al 
         *form
         *En este metodo además se harála separación de 
         *datos por mediante de comas 
         */
        private void AccesoForm(string accion)
        {
            if (ComparacionTiempo(time))
            {
                try
                {
                    strBufferIn = accion;
                    //En esta parte se hara la separación de datos. 

                    string caja1, caja2, caja3, caja4, temp, lum;

                    //Se hace un array el cual se separa con la (,)para eso funciona la herramienta split 
                    //(Puede ser cualquier parametro desde una palabra hasta caracteres especiales en un orden especifico)

                    string[] subStrings = accion.Split(',');
                    caja1 = subStrings[0];
                    caja2 = subStrings[1];
                    caja3 = subStrings[2];
                    caja4 = subStrings[3];
                    temp = subStrings[4];
                    lum = subStrings[5];
                    //Hacemos que el txtDatoRecibido tome los valores del buffer que entra  
                    txtDatoRecibido.Text = accion;
                    //Después se limpia el bufferin 
                    serialPort1.DiscardInBuffer();

                    Datos datosNuevos = new Datos();

                    datosNuevos.Caja1 = caja1;
                    datosNuevos.Caja2 = caja2;
                    datosNuevos.Caja3 = caja3;
                    datosNuevos.Caja4 = caja4;
                    datosNuevos.Temp = temp;
                    datosNuevos.Lum = lum;
                    datosNuevos.Fecha = DateTime.Now.ToShortDateString();
                    datosNuevos.Hora = DateTime.Now.ToLongTimeString();
                    LeerDatos(datosNuevos);
                    //Este metodo ingresa datos al gridView y lo agrega, no se puede poner un objeto, se tiene que poner separado por comas
                    //Asi separamos el objeto actual en los elementos separados
                    dataGridView1.Rows.Add(datosNuevos.Caja1, datosNuevos.Caja2, datosNuevos.Caja3, datosNuevos.Caja4, datosNuevos.Temp, datosNuevos.Lum
                      + datosNuevos.Fecha, datosNuevos.Hora);
                }

                catch (Exception ex) { MessageBox.Show(ex.Message.ToString()); }
            }

        }

        /*
        * Metodo para asignar datos en el form 
       */

        private void AccesoInterrumpcion(string accion)
        {

            try
            {
                DelegadoAcceso var_Da;
                var_Da = new DelegadoAcceso(AccesoForm);

                object[] arg = { accion };
                base.Invoke(var_Da, arg);

            }
            catch (Exception e) { MessageBox.Show(e.Message.ToString()); }
        }

        /*
                * Metodo para leer datos e insertarlos en el TXT 
                * Este metodo es uno de los más importantes 
                */
        public void LeerDatos(Datos datoRecibido)
        {
            //a la variable ruta le asignamos 
            //el valor de lo que tome el txt 

            ruta = txtUbicaciónArchivo.Text;
            //Lo primero que hace este metodo es agregar el dato a la lista. 
            datosTxt.Add(datoRecibido);
            //Después la lista se pasa en reversa 
            //Para ver el dato actual al principio 
            datosTxt.Reverse();
            //El streamWriter es para generar el archivo txt y entre los 
            //parentecis va  la ruta que se deleccione en la variable ruta
            StreamWriter sw = new StreamWriter(ruta);
            //Creamos el encabezado
            sw.WriteLine("+-------------+-------------+-------------+-------------+-------------+-------------+------------+-------------+");
            sw.WriteLine("|Caja 1       | Caja 2      |  Caja 3     |  Caja 4     | TEMPERATURA | LUMINOSIDAD |    FECHA   |    HORA     |");
            sw.WriteLine("+-------------+-------------+-------------+-------------+-------------+-------------+------------+-------------+");
            //Creamos un foreach para recorrer la lista inversa y separar los datos del mismo
            foreach (var dato in datosTxt)
            {

                string espaciado = "";
                //Caada valor separado agrega  auna linea en el archivo
                for (int i = 0; i < 13 - (dato.Temp.Length + 1); i++)
                {
                    espaciado = espaciado + "";
                    sw.WriteLine("|" + espaciado + dato.Caja1 + "|" + espaciado
                         + dato.Caja2 + "|" + espaciado + dato.Caja3 + "|" + espaciado + dato.Caja4 + "|"
                         + espaciado + dato.Temp + " |" + espaciado + dato.Lum + "," + "|" + dato.Fecha + "," + "|" + dato.Hora + "|");
                }
                sw.WriteLine("+-------------+-------------+-------------+-------------+-------------+-------------+-------------+-------------+");
                //Cerramos el motor para que se guarden los cambios en el archivo
                sw.Close();
                //Volvemos a invertir la lista para que el siguiente dato sigua la linea de la lista correctamente
                datosTxt.Reverse();
            }
        }

        //--------------------FUNCIONES DEL FORM Y BOTONES-------------------------- 
        private void Form1_Load(object sender, EventArgs e)
        {
            //el botón de se inhabilita al principio 
            btnConectar.Enabled = false;
        }

        //Función del botón buscar puertos
        private void btnBuscarPuertos_Click(object sender, EventArgs e)
        {
            //En un arreglo se guardan los puertos disponibles 

            string[] PuertosDisponibles = SerialPort.GetPortNames();
            cmbBuscarPuertos.Items.Clear();

            foreach (string puerto in PuertosDisponibles)
            {
                //Al comboBox de Puertos le agregamos los intems encontrados por elprimer 
                //SerialPort.GetPortNames()
                cmbBuscarPuertos.Items.Add(puerto);
            }

            if (cmbBuscarPuertos.Items.Count > 0)
            {

                cmbBuscarPuertos.SelectedIndex = 0;
                MessageBox.Show("Selecciona el puerto");
                //En cuanto se seleccione un item del comboBox 
                //El botpn de conectar será habilitado. 
                btnConectar.Enabled = true;
            }
            //si no se encuentra ningún puerto activo 
            //hace lo siguiente 
            else
            {
                MessageBox.Show("Ningún Puerto Seleccionado");
                cmbBuscarPuertos.Items.Clear();
                cmbBuscarPuertos.Text = "";
                btnConectar.Enabled = false;

            }
        }//cierra funciones del botón de buscar puertos 
        //------FUNCIÓN DEL BOTÓN CONECTAR 
        private void btnConectar_Click(object sender, EventArgs e)
        {
            if (btnConectar.Text == "CONECTAR")
            {
                //datos necesarios para hacer conexión con el puerto serial
                serialPort1.BaudRate = 9600;
                serialPort1.DataBits = 8;
                serialPort1.Parity = Parity.None;
                serialPort1.StopBits = StopBits.One;
                serialPort1.Encoding = Encoding.ASCII;
                serialPort1.PortName = cmbBuscarPuertos.Text;
                //Después establecemos un intervalo de tiempo para que 
                //comiencen algunas funciones 
                timer1.Interval = 10000;
                timer1.Enabled = true;
                //Después de establecer todos esos parametros se abre el puerto
                // serial 
                serialPort1.Open();
                //Después de conectar hacemos que se mande el número 1 
                // para recibir el primer dato
                serialPort1.Write("1");

                //Mandamos llamar el metodo de accesoInterrumpción 
                //para recibir el primer dato tomado por el puerto serial
                AccesoInterrumpcion(serialPort1.ReadExisting());
                //Posteriormente generamos un try y un catch en 
                //donde se hacen otra acciones 
                try
                {
                    btnConectar.Text = "DESCONECTAR";
                    //Lo que se hace es tomar toda la fecha, dia/mes/año hh:mm:ss por cualquier cosa que quieras hacer despues
                    //Luego lo que hago es separar esa fecha en solo los minutos para comenzar a hacer el contador 
                    fecha = DateTime.Now;
                }
                catch (Exception ex) { MessageBox.Show(ex.Message.ToString()); }
            }
            else if (btnConectar.Text == "DESCONECTAR")
            {
                serialPort1.Close();
                btnConectar.Text = "CONECTAR";
            }
        }//cierra función del boton conectar 
        //Función del puerto serial 
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                /*
                 Con el thread sleep lo que se hace es que el puerto 
                 * serial se duerma por un rato para que reciba todos los 
                 * datos que contiene el buffer 
                 */
                Thread.Sleep(2000);
                string dato = serialPort1.ReadExisting();
                AccesoInterrumpcion(dato);
            }
            catch (Exception EX) { MessageBox.Show(EX.Message.ToString()); }
        }//cierra funciones delpuerto serial 

        //función para agregar el timer 
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                timer1.Enabled = true;
                lblTiempo.Text = DateTime.Now.ToLongTimeString();

                timer1.Start();
                if ((fecha + aumentoTiempo).ToString("mm:ss") == DateTime.Now.ToString("mm:ss"))
                {

                    serialPort1.Write("1");
                    serialPort1.DiscardOutBuffer();

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Truena por el minuto" + ex);
            }

        }

        //-----------Función del botón guardar archivo 
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        //-------------------Función del saveFileDialog1
        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            txtUbicaciónArchivo.Text = saveFileDialog1.FileName;
        }

    }
}

=======
﻿//Librerias utilizadas 
using System;
using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Linq;

//nombre del proyecto 
namespace SerialReceive
{
    public partial class PROYECTO_SERIAL : System.Windows.Forms.Form
    {


        //Este es un list privado para que siempre tenga los valores sea cual sea la acción que se realice
        //O al menos en esta clase
        private List<Datos> datosDeTxt = new List<Datos>();
        //Variable para dar acceso al form 
        private delegate void DelegadoAcceso(string accion);
        //Variables para la lectura de datos
        private string strBufferIn;
        private string strBufferOut;

        //Esta es para tomar la fecha, hora al momento de conectarse al puerto
        private DateTime fecha = DateTime.Now;
        //Es el contador que vamos a usar por mientras
        //private int aumentoTiempo = 0;
        //Es el contador que vamos a usar por mientras
        private TimeSpan aumentoTiempo = new TimeSpan(0, 0, 0);
        //Variable para abrir ventana de busqueda de archivos 
        FolderBrowserDialog fbd = new FolderBrowserDialog();
        private string ruta;
        //Variable para asignar el tiempo 
        //private string time; 
        //Inicio de la clase del diseño  
        private TimeSpan time = new System.TimeSpan();

        public PROYECTO_SERIAL()
        {
            InitializeComponent();
            //tiempos de espera para tomar los datos
            cmbTiempo.Items.Add(time = new TimeSpan(0, 0, 1));
            cmbTiempo.Items.Add(time = new TimeSpan(0, 0, 5));
            cmbTiempo.Items.Add(time = new TimeSpan(0, 0, 10));
            cmbTiempo.Items.Add(time = new TimeSpan(0, 0, 20));
            cmbTiempo.Items.Add(time = new TimeSpan(0, 0, 30));
            cmbTiempo.Items.Add(time = new TimeSpan(0, 0, 40));
            cmbTiempo.Items.Add(time = new TimeSpan(0, 0, 50));
            cmbTiempo.Items.Add(time = new TimeSpan(0, 1, 0));
            cmbTiempo.Items.Add(time = new TimeSpan(0, 5, 0));
            cmbTiempo.Items.Add(time = new TimeSpan(0, 10, 0));
            
        }


        /// <summary>
        /// Esta función es para validar si entra o no en el rango de tiempo
        /// </summary>
        /// <param name="aumento">es el aumento en minutos que se tendra que sumar a la hora anterior</param>
        public bool MandarDatos(TimeSpan aumento)
        {

            //Se usa para validar el minuto actual 
            System.Diagnostics.Debug.WriteLine("Minuto anterior: " + fecha.ToString("mm:ss") + "  Minuto actual: " + DateTime.Now.ToString("mm:ss"));

            if ((fecha + time).ToString("mm:ss") == DateTime.Now.ToString("mm:ss"))
            {
                fecha = fecha + time;
                //MessageBox.Show("aqui voy");
                return true;
            }

            else
                return false;

        }
        //---------------------------Metodo para leer dato e insertar en un .txt-------------

        public void LeerDatos(Datos datoRecibido)
        {
            ruta = txtBrowse.Text;
            //Primero se agrega el dato a la lista
            datosDeTxt.Add(datoRecibido);

            //Luego la lista se cambia a el sentido contrario, esto para que el ultimo dato ingresado salga al inicio del txt
            datosDeTxt.Reverse();

            //Iniciamos el StreamWriter que es el motor para crear el archivo, se especifica la ruta donde se almacenara
            // System.IO.StreamWriter stre = new System.IO.StreamWriter(Server.MapPath("C:/Datos/datos.txt") + "datos.txt", true); 
            StreamWriter sw = new StreamWriter(ruta);

            //Creamos el encabezado
            sw.WriteLine("+-------------+-------------+-------------+-------------+-------------+-------------+------------+-------------+");
            sw.WriteLine("|Caja 1       | Caja 2      |  Caja 3     |  Caja 4     | TEMPERATURA | LUMINOSIDAD |    FECHA   |    HORA     |");
            sw.WriteLine("+-------------+-------------+-------------+-------------+-------------+-------------+------------+-------------+");

            //Creamos un foreach para recorrer la lista inversa y separar los datos del mismo
            foreach (var dato in datosDeTxt)
            {
                string espaciado = "";
                //Cada valor separado se agrega a una linea en el archivo

                for (int i = 0; i < 13 - (dato.Temp.Length + 1); i++)
                    espaciado = espaciado + " ";

                sw.WriteLine("|" + espaciado + dato.Caja1 + "|" + espaciado + dato.Caja2 + "|" + espaciado + dato.Caja3 + "|" + espaciado + dato.Caja4 + "|" + espaciado + dato.Temp + " |" + espaciado + dato.Lum + "," + "|" + dato.Fecha + "," + "|" + dato.Hora + "|");
            }
            sw.WriteLine("+-------------+-------------+-------------+-------------+-------------+-------------+-------------+-------------+");

            //Cerramos el motor para que se guarden los cambios en el archivo
            sw.Close();

            //Volvemos a invertir la lista para que el siguiente dato sigua la linea de la lista correctamente
            datosDeTxt.Reverse();

        }//cierra metodo leer dato 

        //metodo para dar acceso al form
        private void AccesoForm(string accion)
        {
            //--------------------- tener  acceso a los datos que se ingresan desde el puerto serial

            //El numero 10 es provisional

            if (MandarDatos(time))
            {
                try
                {
                    strBufferIn = accion;


                    //Aqui es donde se hara la separación de datos

                    System.Diagnostics.Debug.WriteLine(accion);

                    string caja1, caja2, caja3, caja4, temp, lum;
                    //Se hace un array el cual se separa por la coma para eso funciona la herramienta split 
                    //(Puede ser cualquier parametro desde una palabra hasta caracteres especiales en un orden especifico)
                    string[] subStrings = accion.Split(',');

                    caja1 = subStrings[0];
                    caja2 = subStrings[1];
                    caja3 = subStrings[2];
                    caja4 = subStrings[3];
                    temp = subStrings[4];
                    lum = subStrings[5];

                    txtDatoRecibido.Text = accion;
                    spPuertoSerial.DiscardInBuffer();

                    Datos datosNuevos = new Datos();
                    datosNuevos.Caja1 = caja1;
                    datosNuevos.Caja2 = caja2;
                    datosNuevos.Caja3 = caja3;
                    datosNuevos.Caja4 = caja4;
                    datosNuevos.Temp = temp;
                    datosNuevos.Lum = lum;
                    datosNuevos.Fecha = DateTime.Now.ToShortDateString();
                    datosNuevos.Hora = DateTime.Now.ToLongTimeString();

                    LeerDatos(datosNuevos);
                    //Este metodo ingresa datos al gridView y lo agrega, no se puede poner un objeto, se tiene que poner separado por comas
                    //Asi separamos el objeto actual en los elementos separados
                    dataGridView1.Rows.Add(datosNuevos.Caja1, datosNuevos.Caja2, datosNuevos.Caja3, datosNuevos.Caja4, datosNuevos.Temp, datosNuevos.Lum, datosNuevos.Fecha, datosNuevos.Hora);
                    //spPuertoSerial.DiscardOutBuffer();

                    //System.Diagnostics.Debug.WriteLine("1");

                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message.ToString());
                }
            }
        }




        //Metodo para estar guardando los datos 
        private void AccesoInterrupcion(string accion)
        {
            try
            {
                DelegadoAcceso var_DA;
                var_DA = new DelegadoAcceso(AccesoForm);


                object[] arg = { accion };

                base.Invoke(var_DA, arg);
            }

            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("No toma el valor o supera el buffer asignado");
            }

        }

        //Metodo para poder leer datos del serial 
        private void PROYECTO_SERIAL_Load(object sender, EventArgs e)
        {
            strBufferIn = "";
            strBufferOut = "";
            btnConectar.Enabled = false;
        }

        //Metodo  y acción para buscar los puertos disponibles en la pc 
        private void btnBuscar_Puertos_Click(object sender, EventArgs e)
        {
            // en un arreglo se guardarn los datos disponobles 
            string[] PuertosDisponibles = SerialPort.GetPortNames();
            cmbPuertos.Items.Clear();

            foreach (string puerto in PuertosDisponibles)
            {
                // en el combo box puertos 
                // se guardaran los puertos disponibles 
                cmbPuertos.Items.Add(puerto);

            }
            //si el cmbPuertos es mayor que 0 se mostrará  el siguiente mensaje 
            if (cmbPuertos.Items.Count > 0)
            {
                cmbPuertos.SelectedIndex = 0;
                //-------------------------------Mensaje para seleccionar el puerto 
                // Esto nos indica que el metodo para encontrar los seriales esta funcionando 
                MessageBox.Show("Seleccionar el puerto");

                btnConectar.Enabled = true;
                //------------------------------------------------------------------------------
            }
            //si no se encuentra ningun puerto pasara al siguiente else 
            else
            {
                MessageBox.Show("Ningun puerto seleccionado");
                cmbPuertos.Items.Clear();
                cmbPuertos.Text = "                       ";
                strBufferIn = "";
                strBufferOut = "";
                btnConectar.Enabled = false;
            }
        }

        /*
         *En el botón conectar le pasaremos los 
         * parametros que necesita para recibir los datos 
         * 
         */
        private void btnConectar_Click(object sender, EventArgs e)
        {
            // La función y el metodo para conectra con el serial
            try
            {
                if (btnConectar.Text == "CONECTAR")
                {
                    System.Diagnostics.Debug.WriteLine("1");
                    //datos para poder conectar con el serial 
                    spPuertoSerial.BaudRate = 9600;
                    spPuertoSerial.DataBits = 8;
                    spPuertoSerial.Parity = Parity.None;
                    spPuertoSerial.StopBits = StopBits.One;
                    spPuertoSerial.Encoding = Encoding.ASCII;
                    //spPuertoSerial.Handshake = Handshake.None;// para que sirve?
                    // No lo se eso lo puse del video 
                    //este es otro emulador 

                    spPuertoSerial.PortName = cmbPuertos.Text;

                    try
                    {
                        //Línea de código para abrir el puerto serial 
                        spPuertoSerial.Open();
                        spPuertoSerial.DiscardOutBuffer();

                        strBufferIn = "";
                        strBufferOut = "";
                        /*
                         * Cuando se conecte al serial 
                         * este mandara un dato de 
                         * para concer que si esta concetado 
                         * 
                         */
                        spPuertoSerial.DiscardInBuffer();
                        spPuertoSerial.DiscardOutBuffer();
                        AccesoInterrupcion(spPuertoSerial.ReadExisting());
                        strBufferIn = "";
                        strBufferOut = "";
                        btnConectar.Text = "DESCONECTAR";

                        //Lo que se hace es tomar toda la fecha, dia/mes/año hh:mm:ss por cualquier cosa que quieras hacer despues
                        //Luego lo que hago es separar esa fecha en solo los minutos para comenzar a hacer el contador 
                        fecha = DateTime.Now;
                        // aumentoTiempo = Int32.Parse(fecha.ToString("mm"));
                    }
                    catch (Exception EXC)
                    {
                        MessageBox.Show(EXC.Message.ToString());
                    }
                }
                //EL else if es para cambiar el botn de conectar por las palabras "desconectar"
                else if (btnConectar.Text == "DESCONECTAR")
                {
                    spPuertoSerial.Close();
                    btnConectar.Text = "CONECTAR";
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message.ToString());
            }
        }

        //*Metodo para recibir los datos del serial 

        private void spPuertoSerial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                Thread.Sleep(5000);
                string dato = spPuertoSerial.ReadExisting();
                AccesoInterrupcion(dato);
            }

            catch (Exception ec)
            {
                MessageBox.Show(ec.Message.ToString());
            }

        }

        private void lbListaDatos_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        //
        private void txtDatoRecibido_TextChanged(object sender, EventArgs e)
        {


        }

        private void cmbPuertos_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        //función para agregar el timer 
        private void tTime_Tick(object sender, EventArgs e)
        {
            try
            {
                tTime.Enabled = true;
                lblTiempo.Text = DateTime.Now.ToLongTimeString();

                tTime.Start();
                if ((fecha + time).ToString("mm:ss") == DateTime.Now.ToString("mm:ss"))
                {
                    //spPuertoSerial.DiscardInBuffer();
                    //spPuertoSerial.DiscardOutBuffer();
                    spPuertoSerial.Write("1");
                    //Thread.Sleep(10000);
                    spPuertoSerial.DiscardOutBuffer();


                }



            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Truena por el minuto" + ex);
            }

        }

      

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        //Metodo para seleccionar donde guardar el archivo generado por el sistema 

        private void btnArchivos_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        public class Datos
        {
            public string Caja1 { get; set; }
            public string Caja2 { get; set; }
            public string Caja3 { get; set; }
            public string Caja4 { get; set; }
            public string Temp { get; set; }
            public string Lum { get; set; }
            public string Fecha { get; set; }
            public string Hora { get; set; }
        }

        private void saveFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            txtBrowse.Text = saveFileDialog1.FileName;
        }

        private void lblTiempo_Click(object sender, EventArgs e)
        {

        }

       
    }
}

>>>>>>> 61800a0f41c8193df526b0339b64360b7906f560
