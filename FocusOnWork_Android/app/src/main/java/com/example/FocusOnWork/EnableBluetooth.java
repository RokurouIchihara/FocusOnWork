package com.example.FocusOnWork;

import android.bluetooth.BluetoothAdapter;
import android.content.Intent;

import static com.example.FocusOnWork.BtServerThread.REQUEST_ENABLE_BLUETOOTH;

public class EnableBluetooth extends Thread{
    /* MainActivityを使う用 */
    MainActivity mainActivity;
    public EnableBluetooth(MainActivity m){
        mainActivity = m;
    }
    public void run(){
        Intent btOn = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
        //mainActivity.startActivity(btOn);
        mainActivity.startActivityForResult(btOn, REQUEST_ENABLE_BLUETOOTH);

    }
}

