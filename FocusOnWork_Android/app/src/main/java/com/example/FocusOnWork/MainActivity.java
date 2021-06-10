package com.example.FocusOnWork;


import android.app.NotificationManager;
import android.bluetooth.BluetoothAdapter;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.media.AudioManager;
import android.os.Build;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;

import static com.example.FocusOnWork.BtServerThread.REQUEST_ENABLE_BLUETOOTH;


public class MainActivity extends AppCompatActivity {

    /* グローバル変数 */
    BtServerThread btThread = null;
    GetPermissionOfSilentMode get_permis = null;
    Toast tst;
    TextView t1;
    BluetoothAdapter adapter;
    NotificationManager notificationManager;
    EnableBluetooth enble;
    public boolean flg = false;
    Button btn_s;
    Button btn_p;
    AlertDialog.Builder alertDialog;
    private boolean btnFlg = true;
    boolean swth = true;

    @Override
    /* 作成時の動作 */
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        /* トースト用変数 */
        tst = Toast.makeText(this, "LENGTH_SHORT", Toast.LENGTH_SHORT);
        /* テキスト用変数 */
        t1 = findViewById(R.id.textView1);
        t1.setText("");
        /* ボタン */
        btn_s = (Button)findViewById(R.id.start_btn);
        btn_p = (Button)findViewById(R.id.pairing_btn);

        /* 通知の許可を取るクラスのインスタンス生成 */
        get_permis = new GetPermissionOfSilentMode(this);

        /* bluetoothを使うためのインスタンス生成 */
        adapter = BluetoothAdapter.getDefaultAdapter();
        btThread = new BtServerThread(tst, t1, adapter, this, btn_s, btn_p);

        /* 権限がないとき取得 */
        notificationManager = (NotificationManager) getSystemService(Context.NOTIFICATION_SERVICE);
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M && !notificationManager.isNotificationPolicyAccessGranted()) {
              /* 通知許可の確認のダイアログを表示 */
            Dialog();
        }
        else{
            /* bluetoothをONにする */
            enble = new EnableBluetooth(this);
            Check_bluetooth(adapter, tst);

        }
    }

    @Override
    protected void onResume(){
        super.onResume();
        if (flg) {
            /* アプリをリスタート */
            startActivity(new Intent(this, MainActivity.class));
            flg = false;
            finish();
        }
    }


    @Override
    protected void onDestroy() {
        super.onDestroy();
        btThread.cancel();

    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        /* bluetoothがONになっていなかったらもう一度確認画面を */
        if (requestCode == REQUEST_ENABLE_BLUETOOTH ) {
            if (resultCode != RESULT_OK) {
                startActivity(new Intent(this, MainActivity.class));
                finish();
            }
            else{
                Check_bluetooth(adapter, tst);
            }
        }
    }

    /* bluetoothの状態を確認 */
    private void Check_bluetooth(BluetoothAdapter adapter, Toast tst){
        if (adapter.equals(null)){
            tst.setText("Bluetooth非対応機種です．");
            tst.show();
            try {
                Thread.sleep(3000);
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
            /* アクティビティの終了 */
            this.finish();
        }
        /* BluetoothがONか確認 */
        if(!adapter.isEnabled()){
            enble.start();
            try {
                enble.join();
            } catch (InterruptedException e) {
                /* 例外処理 */
                e.printStackTrace();
            }
        }else{
            tst.setText("BluetoothはONです");
            tst.show();
            ChangeMode changemode = new ChangeMode(tst, this, (AudioManager)getSystemService(Context.AUDIO_SERVICE));
            /* 接続＆送受信を始める */
            btThread.SetupAudioManager(changemode);

            alertDialog = new AlertDialog.Builder(this);
            /* ダイアログの表示 */
            alertDialog.setTitle("注意！");
            alertDialog.setMessage("アプリ終了時にサイレントモードがONになったままの場合があります．\n終了後は確認してください．");
            alertDialog.setPositiveButton("OK", new DialogInterface.OnClickListener() {
                public void onClick(DialogInterface dialog, int whichButton) {
                    ;
                }
            });
            alertDialog.show();
            SetBtn();
        }
    }

    /* ダイアログを出力する関数 */
    public void Dialog(){
        UpdateTextView("ERROR:通知の許可をしてください\n", (float) 20.0);
        AlertDialog.Builder builder = new AlertDialog.Builder(this);
        builder.setMessage("通知の許可をお願いします")
                .setPositiveButton("設定へ", new DialogInterface.OnClickListener() {
                         public void onClick(DialogInterface dialog, int id) {
                    }
                })
               .setOnDismissListener(new DialogInterface.OnDismissListener() {
                @Override
                public void onDismiss(DialogInterface dialog) {
                    /* ダイアログが閉じられた際の処理 */
                    StartGetpermiss();
                 }
                }).show();
    }


    /* パーミッションを得るために設定を開く */
    public void StartGetpermiss(){
        try {
            Thread.sleep(100);
        } catch (InterruptedException e) {
            e.printStackTrace();
        }
        get_permis.start();

        /* Threadの処理が終了するまで待機の指示 */
        try {
            get_permis.join();
        } catch (InterruptedException e) {
            /* 例外処理 */
            e.printStackTrace();
        }

    }

    /* TextViewを更新する関数 */
    public void UpdateTextView(String text, float size){
        t1 = findViewById(R.id.textView1);
        t1.setText(text);
        t1.setTextSize(size);
    }

    /* ボタンを初期化する関数 */
    public void InitBtThread(){
        //SetBtnEvent setBtn;
        //setBtn = new SetBtnEvent(this, btThread, tst, (Button)findViewById(R.id.start_btn));
        //setBtn.Set();
        btThread = new BtServerThread(tst, t1, adapter, this, btn_s, btn_p);
        ChangeMode changemode = new ChangeMode(tst, this, (AudioManager)getSystemService(Context.AUDIO_SERVICE));
        btThread.SetupAudioManager(changemode);
    }

    /* ボタンにイベントを定義する関数 */
    public void SetBtn(){
        View.OnClickListener event = new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                switch (v.getId()){
                    case R.id.start_btn:
                        /*if(swth && !btThread.connectFlg){
                            btn_s.setText("Reconnect");
                        }*/
                        /* 通信開始 */
                        if(btnFlg){
                            btnFlg = false;
                            tst.setText("接続待機中");
                            tst.show();
                            btThread.start();
                        }
                        else{
                            if(!btThread.connectFlg){
                                // findViewById(R.id.restart_btn).performClick();
                                // findViewById(R.id.start_btn).performClick();
                                btThread.cancel();
                                try {
                                    btThread.join();
                                } catch (InterruptedException e) {
                                    // 例外処理
                                    e.printStackTrace();
                                }
                                Log.e("sad","asf");
                                /* BtThreadを初期化 */
                                InitBtThread();
                                tst.setText("接続待機中");
                                tst.show();
                                btThread.start();

                            }
                        }
                        break;
                    case R.id.pairing_btn:
                        /* 接続していないとき */
                        if(!btThread.connectFlg){
                            swth = false;
                            findViewById(R.id.start_btn).performClick();
                            /* 検出可能にする */
                            Intent intent = new Intent(BluetoothAdapter.ACTION_REQUEST_DISCOVERABLE);
                            /* 検出時間の設定(30秒) */
                            intent.putExtra(BluetoothAdapter.EXTRA_DISCOVERABLE_DURATION, 30);
                            startActivity(intent);
                        }
                        break;

                    case R.id.restart_btn:
                        try {
                            Thread.sleep(500);
                        } catch (InterruptedException e) {
                            e.printStackTrace();
                        }
                        /* アプリをリスタート */
                        startActivity(new Intent(getApplicationContext(), MainActivity.class));
                        finish();
                        break;
                }
            }
        };
        /* ボタンをリスナーへ登録 */
        findViewById(R.id.start_btn).setOnClickListener(event);
        findViewById(R.id.pairing_btn).setOnClickListener(event);
        findViewById(R.id.restart_btn).setOnClickListener(event);
    }
}

