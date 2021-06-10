using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

namespace FocusOnWork {
    public class PairingDevice {
        
        /*　ラベルとか */
        Label label1;
        ListView listView1;
        ListViewItem item = new ListViewItem();
        Form form2;

        /* 取得情報 */
        DeviceInformationCollection infomations;

        string[] notPairedDeviceName = new string[0];
        string[] loading = new string[] { ".", ".o", ".oO", "" };
        
        /* ロ―ディングの表示切替用 */
        int count = 0;
        public bool endflg = true;
        
        public string inputText;
        public string deviceName = "";

        /* インスタンス */
        public PairingDevice(Label label, ListView list, Form f) {
            label1 = label;
            listView1 = list;
            form2 = f;
            ColumnHeader header1 = new ColumnHeader();
            header1.Text = "デバイス一覧";
            header1.TextAlign = HorizontalAlignment.Left;
            header1.Width = 700;
            listView1.Columns.Add(header1);

        }

      
        private async void ChangeLabelText() {
            while (endflg) {
                label1.Text = "検索中" + loading[count];
                count = ++count % 4;
                await Task.Delay(700);
            }
            label1.Text = "検索終了";
        }


        /* デバイスを見つける関数 */
        private async Task<DeviceInformationCollection> FindDevice(Boolean flg) {
         return await DeviceInformation.FindAllAsync(BluetoothDevice.GetDeviceSelectorFromPairingState(flg));
        }

        /* ペアリングの関数 */
        public async Task<bool> Pairing() {
            notPairedDeviceName = new string[0];
            endflg = true;
            ChangeLabelText();
            /* デバイスを探す */
            infomations = await FindDevice(false);
            endflg = false;
            /* 名前取得 */
            notPairedDeviceName = GetDeviceName(notPairedDeviceName);
            /* ListViewに表示 */
            AddListView(notPairedDeviceName);
            return true;
        }

        /* イベントハンドラ */
        private void PairingRequestedHandler(DeviceInformationCustomPairing sender, DevicePairingRequestedEventArgs args) {
            switch (args.PairingKind) {
                case DevicePairingKinds.ProvidePin:
                    /* PIN入力が必要なケース */
                    var collectPinDeferral = args.GetDeferral();

                    /* 入力フォームを立ち上げる */
                    var f3 = new Form3(this);
                    f3.ShowDialog();
                    /* 入力結果を取得 */
                    string pinFromUser = inputText;
                    /* 認証 */
                    if (!string.IsNullOrEmpty(pinFromUser)) {
                        args.Accept(pinFromUser);
                    }
                    collectPinDeferral.Complete();
                    break;

                case DevicePairingKinds.ConfirmPinMatch:
                    /* PINが一致しているか確認 */
                    collectPinDeferral = args.GetDeferral();
                    inputText = args.Pin;
                    /* PIN表示 */
                    var f4 = new Form4(this, args);
                    f4.ShowDialog();
                    collectPinDeferral.Complete();
                    break;
                
            }
        }

        public async Task<int> Connect(string name) {
            /* 初期化 */
            inputText = "";
            deviceName = "";
            if(notPairedDeviceName.Length == 0) {
                return -1;
            }
            foreach (DeviceInformation infomation in infomations) {
                /* ペアリング */
                if (infomation.Name == name) {
                    deviceName = infomation.Name;
                    /* 準備 */
                    DeviceInformationCustomPairing pair = infomation.Pairing.Custom;
                    pair.PairingRequested += this.PairingRequestedHandler;
                    /* 接続 */
                    DevicePairingResult result = await pair.PairAsync(
                            DevicePairingKinds.ProvidePin, DevicePairingProtectionLevel.EncryptionAndAuthentication);
                    if (result.Status == DevicePairingResultStatus.Paired || result.Status == DevicePairingResultStatus.AlreadyPaired) {
                        MessageBox.Show("ペアリング成功", "情報", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                      
                        return 0;
                    } else {
                        MessageBox.Show("ペアリング失敗", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return 1;
                    }               
                }
            }
            return -2;
        }

        /* デバイスの名前取得 */
        private string[] GetDeviceName(string[] list) {
            foreach (DeviceInformation infomation in infomations) {
                Debug.WriteLine(infomation.Name);
                /* リサイズ */
                Array.Resize(ref list, list.Length + 1);
                list[list.Length - 1] = infomation.Name;
            }
            return list;
        }

        /* listviewに追加 */
        private void AddListView(string[] list) {
            foreach (string text in list) {
                listView1.Items.Add(new ListViewItem(text));
            }
        }

        public string GetSelectedView() {
            int index = 0;
            if(listView1.SelectedItems.Count > 0) {
                index = listView1.SelectedItems[0].Index;
                return notPairedDeviceName[index];
            }
            return null;
        }

    }
}
