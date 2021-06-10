package com.example.FocusOnWork;

import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothServerSocket;
import android.bluetooth.BluetoothSocket;
import android.content.Context;
import android.media.AudioManager;
import android.util.Log;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.UUID;

public class BtServerThread extends Thread{
    /*---------- 変数準備 ----------*/
    /* bluetoothの状態をチェックする変数 */
    public static  int REQUEST_ENABLE_BLUETOOTH = 0;
    BluetoothSocket bluetoothsocket;
    /* ソケット */
    private BluetoothServerSocket mSocket;
    /* UUID */
    static final String UUID_PCtoAndroid = "6f974e3c-9236-487d-84ba-0bdb31c56870";
    /* bluetoothのadapter */
    BluetoothAdapter adapter;
    /* 入出力ストリーム */
    InputStream inputStream;
    OutputStream outputStream;
    // OutputStream outputStream;
    /* トースト用変数 */
    Toast tst;
    /* テキスト用変数 */
    TextView text1;
    /* MainActivityを使う用 */
    MainActivity mainActivity;
    /* マナーモード変更 */
    ChangeMode changemode;
    boolean connectFlg = false;
    Button btn_s;
    Button btn_p;
    int nowRigerMode;
    AudioManager am;


    public BtServerThread(Toast t, TextView t1, BluetoothAdapter adp, MainActivity m, Button bs, Button bp) {
        tst = t;
        text1 = t1;
        adapter = adp;
        mainActivity = m;
        btn_s = bs;
        btn_p = bp;
        am = (AudioManager)mainActivity.getSystemService(Context.AUDIO_SERVICE);
    }

    public void SetupAudioManager(ChangeMode ch){
        changemode = ch;
    }

    public void run(){
        BluetoothServerSocket tmp = null;
        /* 接続処理 */
        try{
            while(true){
                if(Thread.interrupted()){
                    break;
                }
                String receiveData = "";
                try{
                    /* UUIDで接続 */
                    mSocket = adapter.listenUsingRfcommWithServiceRecord("BT_SERVER_ANDROID", UUID.fromString(UUID_PCtoAndroid));
                    /* ソケット受信*/
                    Log.e("BtServer", "--------------Connecting--------------");
                    bluetoothsocket = mSocket.accept();
                    Log.e("BtServer", "---------------Connected--------------");
                    /* ソケットをクローズ */
                    mSocket.close();
                    tst.setText("接続成功");
                    tst.show();
                    if(am.getRingerMode() == 0){
                        changemode.MuteSounds(0);
                        changemode.MuteSounds(2);
                    }
                    connectFlg = true;
                    btn_s.setText("Connected");
                    btn_p.setText("PAIRED");
                    /* 入出力ストリームの作成 */
                    inputStream = bluetoothsocket.getInputStream();
                    outputStream = bluetoothsocket.getOutputStream();
                    while(true) {
                        if (Thread.interrupted()) {
                            break;
                        }
                        /* 受信 */
                        byte[] incomingBuff = new byte[64];
                        receiveData = new String(incomingBuff, 0, inputStream.read(incomingBuff));
                        if(receiveData.length() <= 5){
                            receiveData = receiveData.replace("*", "");
                            /*tst.setText(receiveData);
                            tst.show();*/
                        }else{
                            while(receiveData.length() > 5){
                                /* データ出力 */
                                String text = receiveData;
                                receiveData = receiveData.substring(0, 4);
                                receiveData = receiveData.replace("*", "");
                                Log.e("INPUT", receiveData);
                                /*tst.setText(receiveData);
                                  tst.show();*/
                                // text1.setText(text1.getText().toString() + receiveData + "\n");
                                if(text.length() > 5){
                                    receiveData = text.substring(5);
                                }
                            }
                        }
                        Log.e("s",receiveData);
                        if(!receiveData.equals("start") && !receiveData.equals("end")){
                            changemode.MuteSounds(Integer.parseInt(receiveData));
                            nowRigerMode = am.getRingerMode();
                        }
                        /* 終了のキーを受信 */
                        else if(receiveData.equals("end")){
                            text1.setText("");
                            changemode.RestoreSounds();
                            SendText("---");
                            mainActivity.finish();
                            break;
                        }
                        else if(receiveData.equals("start")){
                            nowRigerMode = am.getRingerMode();
                        }
                        SendText("---");
                        ChangeTextView(nowRigerMode);
                    }
                } catch (IOException e) {
                    Log.e("BtServer", "SocketConnectError");
                }
                if(bluetoothsocket !=null || "end".equals(receiveData)){
                    try{
                        bluetoothsocket.close();
                        bluetoothsocket = null;
                        break;
                    }catch (IOException e){
                        Log.e("BtServer", "BluetoothSocketCloseError");
                    }
                }

                /* sleep */
                Thread.sleep(1000);
            }
            Log.e("BtServer", "EndBluetooth");
        } catch (InterruptedException e) {
            Log.e("BtThread", "Cancelled ServerThread");
        }
        Log.e("Btserver", "ThreadSuccess");
    }

    /* 終了 */
    public void cancel() {
        if (mSocket != null) {
            try {
                /* 接続解除後，終わりの合図*/
                if(connectFlg == true){
                    SendText("-*-");
                }
                Log.e("BtServer", "Could not close the connect socket1");
                mSocket.close();
                mSocket = null;
            } catch (IOException e) { }
        }
        this.interrupt();
        Log.e("BtServer", "Could not close the connect socket");
    }

    /* viewTextを更新 */
    private void ChangeTextView(int ringermode){
        String text = "";
        switch (ringermode){
            case 0:
                text = "サイレントモード";
                break;
            case 1:
                text = "バイブレーションモード";
                break;
            case 2:
                text = "ノーマルモード";

        }
        text1.setText("現在のモード\n"+ text);
    }

    private void SendText(String text){
        byte[] bytes = {};
        bytes = text.getBytes();
        try {
            outputStream.write(bytes);
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

}
