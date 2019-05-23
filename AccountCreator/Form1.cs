using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace AccountCreator
{
    public partial class Form1 : Form
    {
        private SemaphoreSlim semaphoreSlim;
        private Creator creator;
        private Thread RegThread;

        public Form1()
        {
            InitializeComponent();
            semaphoreSlim = new SemaphoreSlim(1);
            semaphoreSlim.Wait();
            
            creator = new Creator(semaphoreSlim, this);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text.Equals("Start"))
            {
                this.button1.Enabled = false;
                this.numericUpDown1.ReadOnly = true;

                this.button1.Text = "Stop";
                this.textBox1.Text = "";
                this.textBox2.Text = "";
                this.textBox3.Text = "";
                this.progressBar1.Value = 0;
                this.progressBar1.Maximum = Convert.ToInt32(this.numericUpDown1.Value);
                this.label4.Text = "0/" + Convert.ToInt32(this.numericUpDown1.Value);

                RegThread = new Thread(() => creator.RegistrationThread(Convert.ToInt32(this.numericUpDown1.Value)));
                RegThread.Start();

                this.button1.Enabled = true;
            }
            else //stop
            {
                this.button1.Enabled = false;

                RegThread.Abort();

                this.button1.Text = "Start";
                this.numericUpDown1.ReadOnly = false;
                this.button1.Enabled = true;
            }
            
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == (char)Keys.Enter)
            {
                semaphoreSlim.Release();
                e.Handled = true;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(RegThread != null)
                RegThread.Abort();
        }
    }
}
