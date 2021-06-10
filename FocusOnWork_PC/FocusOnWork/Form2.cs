using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace FocusOnWork {
    public partial class Form2 : Form {

        public Form2() {
            InitializeComponent();
            this.Text = "ペアリング可能端末";
            /* ペアリング */
            Start();
            Debug.WriteLine("paired");
        }

        public async void Start() {
            AddBtnEvent(await pairingDevice.Pairing());

        }

        private async void button1_Click(object sender, EventArgs ex) {
            /* 最初は普通に実行 */
            if (result == 100) {
                string text = pairingDevice.GetSelectedView();
                if(text != null) {
                    result = await pairingDevice.Connect(text);
                }
            }
            switch (result) {
                /* ペアリング成功時 */
                case 0:
                    button1.Text = "終了";
                    this.button1.Click -= new System.EventHandler(this.button1_Click);
                    this.button1.Click += new System.EventHandler(this.button2_Click);
                    break;

                /* ペアリング失敗時 */
                case 1:
                    /* 初期化 */
                    InitPair();
                    break;

                /* デバイスがなかったとき */
                case -1:
                    MessageBox.Show("接続可能なデバイスはありません", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    InitPair();
                    break;

            }

        }

        private void button2_Click(object sender, EventArgs ex) {
            this.Close();
        }
        private void Form2_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            pairingDevice.endflg = false;
        }


        private void InitPair() {
            button1.Text = "再検索";
            result = 100;
            AddBtnEvent(false);
        }

        public void AddBtnEvent(bool flg) {
            if (flg) {
                this.button1.Click += new System.EventHandler(this.button1_Click);
            } else {
                this.button1.Click -= new System.EventHandler(this.button1_Click);
                this.button1.Click += new System.EventHandler(this.button3_Click);
            }

        }

        private async void button3_Click(object sender, EventArgs ex) {
            Debug.WriteLine("re");
            listView1.Items.Clear();
            this.button1.Click -= new System.EventHandler(this.button3_Click);
            /* ペアリング */
            AddBtnEvent(await pairingDevice.Pairing());
            button1.Text = "ペアリング";
        }

    }
}
