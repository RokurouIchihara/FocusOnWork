using System;
using System.Windows.Forms;

namespace FocusOnWork {
    public partial class Form3 : Form {
        PairingDevice pairingDevice;
        public Form3(PairingDevice p) {
            pairingDevice = p;
            InitializeComponent();
            this.Text = "PINの入力";
        }


        private void button1_Click(object sender, EventArgs e) {
            pairingDevice.inputText = null;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e) {
            pairingDevice.inputText = textBox1.Text;
            this.Close();
        }

        private void label1_Click(object sender, EventArgs e) {

        }

        private void textBox1_TextChanged(object sender, EventArgs e) {

        }
    }
}
