using System;
using System.Windows.Forms;
using Windows.Devices.Enumeration;

namespace FocusOnWork {
    public partial class Form4 : Form {
        
        PairingDevice pairingDevice;
        DevicePairingRequestedEventArgs args;

        public Form4(PairingDevice p, DevicePairingRequestedEventArgs a) {
            pairingDevice = p;
            args = a;
            InitializeComponent();
            this.Text = "PINの確認";
            label1.Text = pairingDevice.deviceName + "\nに表示されているPINが以下のPIN\nと一致していれば接続を押してください";
            label2.Text = "PIN:" + pairingDevice.inputText;

        }

        private void button1_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e) {    
                args.Accept();
                this.Close();
        }

        private void label1_Click(object sender, EventArgs e) {

        }

       
    }
}
