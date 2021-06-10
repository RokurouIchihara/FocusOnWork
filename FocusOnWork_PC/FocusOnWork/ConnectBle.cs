using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;


namespace FocusOnWork {
    public class ConnectBle {


        /*  uuidの指定  */
        Guid ServerUUID = new Guid("6f974e3c-9236-487d-84ba-0bdb31c56870");
        private RfcommDeviceService rfcommService;
        private StreamSocket socket;
        private DataWriter dataWriterObject;
        private DataReader dataReaderObject;
        private Button btn;

        public ConnectBle(Button b) {
            init_var();
            btn = b;
        }

        private void init_var() {
            rfcommService = null;
            socket = null;
            dataWriterObject = null;
            dataReaderObject = null;
        }


        /* 接続 */
        public async Task<DataWriter> Connect() {
          /* uuidから検索 */
                string selector = RfcommDeviceService.GetDeviceSelector(RfcommServiceId.FromUuid(ServerUUID));
                DeviceInformationCollection collection = await DeviceInformation.FindAllAsync(selector);

                if (collection.Count > 0) {
                    try {
                        DeviceInformation info = collection.First();
                        rfcommService = await RfcommDeviceService.FromIdAsync(info.Id);

                        /* ソケット作成 */
                        socket = new StreamSocket();
                        while (true) {
                            await socket.ConnectAsync(rfcommService.ConnectionHostName, rfcommService.ConnectionServiceName);
                            dataWriterObject = new DataWriter(socket.OutputStream);
                            dataReaderObject = new DataReader(socket.InputStream);

                        }
                    } catch (Exception ) {
                        /* エラー処理 */
                        if (dataWriterObject == null) {
                            return null;
                        } else { return dataWriterObject; }  /* データライターを返す */
                    }
                } else {
                    /* 見つからなかったとき */
                    return null;
                }

        }

        public void Send_start(DataWriter writer) {
            /* 開始のキーを送信 */
            SendCommand("start",writer);
        }

        public void Disconnect(DataWriter writer) {
            /* 切断する */
            /* 接続状態のとき終わりのキーを送信 */
            if(writer != null) {
                SendCommand("end", writer);
                writer.Dispose();
            }
            Debug.WriteLine("Disposed");
        }

        /* 送信の関数 */
        public async void SendCommand(string text, DataWriter writer) {
            Debug.WriteLine("W:" + text);
            while (text.Length < 5) {
               text += "*";
            }
                writer.WriteString(text);
            if (text.Equals("start")) {
                ReceiveCommand(dataReaderObject);

            }
            if (writer != null) {
                await writer.StoreAsync();
            }
        }
        public async void ReceiveCommand(DataReader reader) {
            while (true) {
                Debug.WriteLine("await");
                await reader.LoadAsync(3);
                try {
                    string text = reader.ReadString(3);
                    Debug.WriteLine(text);
                    if (text.Equals("-*-")) {
                        Debug.WriteLine("Doend");
                        Form1.StaticValues.writer = null;
                        btn.PerformClick();
                    }
                } catch (System.Exception) {
                    Debug.WriteLine("Doend");
                    Form1.StaticValues.writer = null;
                    btn.PerformClick();
                    break;
                }
                
            }

        }
    }
}
