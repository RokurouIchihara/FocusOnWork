package com.example.FocusOnWork;


import androidx.appcompat.app.AppCompatActivity;

import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothServerSocket;
import android.bluetooth.BluetoothSocket;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.widget.Toast;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.Set;
import java.util.UUID;


public class MainActivity extends AppCompatActivity {

    // グローバル変数
    public static  int REQUEST_ENABLE_BLUETOOTH = 0;
    BtServerThread btThread = null;

    @Override
    // 作成時の動作
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        // bluetoothを使うためのインスタンス生成
        BluetoothAdapter adapter = BluetoothAdapter.getDefaultAdapter();

        // トースト用変数
        Toast tst = Toast.makeText(this, "LENGTH_SHORT", Toast.LENGTH_SHORT);

        // bluetoothをONにする
        Check_bluetooth(adapter, tst);

        //デバイスを選択
        final  String ADDRESS = Search_devices(adapter);
        btThread = new BtServerThread(ADDRESS);
        Log.e("MainActivity", "---------------------------------------------------------------");
        btThread.start();
        Log.e("MainActivity", "---------------------------------------------------------------");

    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        btThread.cancel();

    }

    // bluetoothの状態を確認
    public void Check_bluetooth(BluetoothAdapter adapter, Toast tst){
        if (adapter.equals(null)){
            tst.setText("Bluetooth非対応機種です．");
            tst.show();
            // アクティビティの終了
            finishAndRemoveTask();
        }
        // BluetoothがONか確認
        boolean btflg = adapter.isEnabled();
        // BluetoothがONになるまで繰り返し
        while(!(btflg == true)){
            Intent btOn = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
            startActivityForResult(btOn, REQUEST_ENABLE_BLUETOOTH);
            try {
                Thread.sleep(2300);
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
            btflg = adapter.isEnabled();
        }

        tst.setText("BluetoothがON");
        tst.show();
    }

    public String Search_devices(BluetoothAdapter adapter){
        String text = "";
        Set<BluetoothDevice> pairedDevises = adapter.getBondedDevices();
        for(BluetoothDevice device: pairedDevises){
            if(device.getName().equals("DESKTOP-6BA4SJ8")){
                String deviceAddress = device.getAddress();
            }
            text = device.getAddress();
        }
        Log.e("m",text);
        return text;
    }

}

class  BtServerThread extends Thread{
    private final BluetoothServerSocket mSocket;
    static final String UUID_PCtoAndroid = "6f974e3c-9236-487d-84ba-0bdb31c56870";
    BluetoothAdapter adapter = BluetoothAdapter.getDefaultAdapter();
    BluetoothSocket bluetoothsocket;
    InputStream inputStream;
    OutputStream outputStream;


    public BtServerThread(String ADDRESS) {
        BluetoothServerSocket tmp = null;
        // 接続処理
        try{
            // UUID
            tmp = adapter.listenUsingRfcommWithServiceRecord("BT_SERVER_ANDROID",UUID.fromString(UUID_PCtoAndroid));
        } catch (IOException e) {
            Log.e("BtServer", "ConnectionError");
        }
        this.mSocket = tmp;
    }

    public void run(){
        Log.e("BtServer", "~~~~~~~~~~~~~~~~~~~~~~~");
        while(true){
            // ソケット受信
            try{
                bluetoothsocket = mSocket.accept();
            } catch (Exception e) {
                Log.e("BtServer", "ReceiveSocketError");
                break;
            }
            // ソケットを受け取っていたら
            if(bluetoothsocket!=null){
                try {
                    inputStream = bluetoothsocket.getInputStream();
                    outputStream = bluetoothsocket.getOutputStream();
                    byte[] incomingBuff = new byte[64];
                    String receiveData = new String(incomingBuff, 0, inputStream.read(incomingBuff));
                    Log.e("INPUT",receiveData);
                    mSocket.close();
                } catch (IOException e) {
                    Log.e("BtServer", "SocketCloseError");
                }
            }

        }
    }

    // 終了
    public void cancel() {
        try {
            mSocket.close();
        } catch (IOException e) {
            Log.e("BtServer", "Could not close the connect socket");
        }
    }

}


/*
// リスト表示
        List<String> list = new ArrayList<String>();
        ArrayAdapter pairedDevice = new ArrayAdapter(this, R.layout.rowdata, list);
        // 接続履歴のあるデバイスの情報を取得
        Set<BluetoothDevice> pairedDevices = adapter.getBondedDevices();
        if (pairedDevices.size() > 0) {
            //接続履歴のあるデバイスが存在する
            for (BluetoothDevice device : pairedDevices) {
                //接続履歴のあるデバイスの情報を順に取得して表示
                //getName()・・・デバイス名取得メソッド
                //getAddress()・・・デバイスのMACアドレス取得メソッド
                list.add(device.getName() + "\n" + device.getAddress());
            }
            @SuppressLint("WrongViewCast") ListView deviceList = (ListView) findViewById(R.id.array1);
            deviceList.setAdapter(pairedDevice);
        }
 */