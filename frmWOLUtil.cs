using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Globalization;
using System.IO;
/*
 * Wake On Lan Utility
 * List out all mac address stotred in txt file "public_file.txt"
 * Allow user to select (check) and send WOL packet to the mac address specified in the txt file.
 * For specific part sending the packet, refer: 
 * 
 * private void WakeFunction(string MAC_ADDRESS)
 
 
 */
namespace WOL
{
    public partial class frmWOLUtil : Form
    {
        CheckBox checkboxHeader = new CheckBox();
        
        public List<string> maclist = new List<string>();

        //Config file location
        public string appath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\WFUtil\";
        
        public frmWOLUtil()
        {
            InitializeComponent();
          
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)))
            {

                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            }
            if (!Directory.Exists(appath))
            {
                Directory.CreateDirectory(appath);
            }
       
        }
        private void frmWOLUtil_Load(object sender, EventArgs e)
        {
            loadList();
            show_chkBox();
            checkboxHeader.Checked = true;
        }
        private void show_chkBox()
        {
            Rectangle rect = dataGridView1.GetCellDisplayRectangle(1, -1, true);
            // set checkbox header to center of header cell. +1 pixel to position 
            rect.Y = 2;
            rect.X = rect.Location.X +(rect.Width-36);
            checkboxHeader.Name = "checkboxHeader";
            checkboxHeader.Size = new Size(18, 18);
            checkboxHeader.Location = rect.Location;
            checkboxHeader.CheckedChanged += new EventHandler(checkboxHeader_CheckedChanged);
            dataGridView1.Controls.Add(checkboxHeader);
        }
        private void checkboxHeader_CheckedChanged(object sender, EventArgs e)
        {

            CheckBox headerBox = ((CheckBox)dataGridView1.Controls.Find("checkboxHeader", true)[0]);
            int index = 0;
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                dataGridView1.Rows[i].Cells[1].Value = headerBox.Checked;
            }
        }
     
        private void writefile()
        {

            try
            {
                StreamWriter s = new StreamWriter(appath + "public_file.txt");
                s.WriteLine("Write the MAC Addresses below:");


                foreach (string line in maclist)
                {

                    s.WriteLine(line);

                }


                s.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);


            }


        }
        private void loadList()
        {


            try
            {

                if (!File.Exists(appath + "public_file.txt"))
                {

                    writefile();

                }

                StreamReader file = new StreamReader(appath + "public_file.txt");
                string line = "";
                while ((line = file.ReadLine()) != null)
                {
                    if (!line.Trim().Contains(" "))
                    {
                        maclist.Add(line);

                    }
                }

                file.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);


            }
            this.dataGridView1.Rows.Clear();


            foreach (string line in maclist)
            {

                this.dataGridView1.Rows.Add(line, true);

            }


        }


        private void WakeFunction(string MAC_ADDRESS)
        {
            WOLClass client = new WOLClass();
            client.Connect(new
               IPAddress(0xffffffff),  //255.255.255.255  i.e broadcast
               0x2fff); // port=12287 default WOL port
 
            client.SetClientToBrodcastMode();
            
            //set sending bites
            int counter = 0;
            
            //buffer to be send
            byte[] bytes = new byte[1024];   // more than enough :-)
            
            //first 6 bytes should be 0xFF
            for (int y = 0; y < 6; y++)
                bytes[counter++] = 0xFF;
            
            //now repeate sneding MAC 16 times
            for (int y = 0; y < 16; y++)
            {
                int i = 0;
                for (int z = 0; z < 6; z++)
                {
                    bytes[counter++] =
                        byte.Parse(MAC_ADDRESS.Substring(i, 2),
                        NumberStyles.HexNumber);
                    i += 2;
                }
            }

            //now send wake up packet
            int reterned_value = client.Send(bytes, 1024);
        }

        private void btnReload_Click(object sender, System.EventArgs e)
        {

            loadList();
        }

        private void btnEdit_Click(object sender, System.EventArgs e)
        {
            if (!File.Exists(appath + "public_file.txt"))
            {


                writefile();

            }
            System.Diagnostics.Process.Start(appath + "public_file.txt");
          
        }

     

        private void btnSend_Click(object sender, System.EventArgs e)
        {
            checkboxHeader.Checked = true;


            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                if ((bool)dataGridView1.Rows[i].Cells[1].Value)
                {

                    Thread t1 = new Thread(turnon);
                    t1.Start(dataGridView1.Rows[i].Cells[1].Value.ToString());
                    dataGridView1.Rows[i].Cells[1].Value = false;

                }
            }
        }
        //@Called by btnSend_Click
        void turnon(object mac) 
        {
            try
            {
                WakeFunction(mac.ToString());
            }
            catch(Exception ex)
            {

                MessageBox.Show(ex.Message);
            
            }
        }

      
    }
    public class WOLClass : UdpClient
    {
        public WOLClass()
            : base()
        { }
        //this is needed to send broadcast packet
        public void SetClientToBrodcastMode()
        {
            if (this.Active)
                this.Client.SetSocketOption(SocketOptionLevel.Socket,
                                          SocketOptionName.Broadcast, 0);
        }
    }
}
