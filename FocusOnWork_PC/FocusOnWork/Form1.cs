using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Windows.Storage.Streams;

namespace FocusOnWork {
    public partial class Form1 : Form {
        bool connectFlg = false;
        public Form1() {
            InitializeComponent();

    }

        private void InitializeComponent() {
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            int[] btn_size = new int[] { 200, 100, 280 };
            int[] form_size = new int[] { 800, 500 };
            this.SuspendLayout();

            // 
            // button1
            // 
            this.button1.Location = new Point((int)(form_size[0] / 2 - btn_size[0] * 1.3), ((form_size[1] / 2) - (btn_size[1] / 2)));
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(btn_size[0], btn_size[1]);
            this.button1.TabIndex = 0;
            this.button1.Text = "開始";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(button1_Click);

            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point((int)(form_size[0] / 2 + btn_size[0] * 0.3), (form_size[1] / 2) - (btn_size[1] / 2));
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(btn_size[0], btn_size[1]);
            this.button2.TabIndex = 1;
            this.button2.Text = "終了";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point((int)(form_size[0]/2-btn_size[2]*0.5), 400);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(btn_size[2], 40);
            this.button3.TabIndex = 2;
            this.button3.Text = "ペアリング";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(button3_Click);

            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(800, 500);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "FocusOnWork";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }
        private void Form1_Load(object sender, EventArgs e) {

        }

        private async void button1_Click(object sender, EventArgs ex) {
            if (connectFlg) {
                return;
            }
            /* 音量を取得 */
            GetVolume getvol = new GetVolume();
            ConnectBle cnt = new ConnectBle(this.button2);
            StaticValues.writer = await cnt.Connect();
            GetProcess getoprocess = new GetProcess();
            if (StaticValues.writer == null && !connectFlg) {
                MessageBox.Show("デバイスが見つかりません\nBluetoothを確認してください", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }else {
                button1.Text = "接続中";
                connectFlg = true;
                    /* 送信開始 */
                    cnt.Send_start(StaticValues.writer);
                while (true) {
                    /* 送信終了していたら終わらせる */
                    if (StaticValues.writer == null) {
                        button1.Text = "開始";
                        connectFlg = false;
                        break;
                    }

                    /* スピーカーの状態のフラグを受け取る 1:using, 0:not using */
                    int resultVolume = await getvol.GetVolumeFlg();
                    Debug.WriteLine(resultVolume.ToString());
                    /* プロセスの取得 */
                    getoprocess.GetTask();
                    int result = await getoprocess.EvaluateScoreOfProcess(resultVolume);
                    Debug.WriteLine(result.ToString());
                 
                    /* 30秒間に1回送信 */
                    cnt.SendCommand(result.ToString(), StaticValues.writer);
                    /* return null writerに */

                }
            }
        }


        private void button2_Click(object sender, EventArgs ex) {
            /* 切断 */
            ConnectBle cnt = new ConnectBle(this.button2);
            cnt.Disconnect(StaticValues.writer);
            this.Close();
        }


        private void button3_Click(object sender, EventArgs ex) {
            var f = new Form2();
            f.ShowDialog();

        }

        public static class StaticValues {
            private static DataWriter _writer;
            public static DataWriter writer {
                get {
                    return _writer;
                }
                set {
                    _writer = value;
                }
            }
        }

    }
}

