package com.example.FocusOnWork;


import android.app.NotificationManager;
import android.content.Context;
import android.content.Intent;
import android.provider.Settings;

public class GetPermissionOfSilentMode extends Thread{
    /* MainActivityを使う用 */
    MainActivity mainActivity;
    int cnt = 0;
    public  GetPermissionOfSilentMode(MainActivity m){
        mainActivity = m;
    }

    public void run(){
        mainActivity.flg = true;
        NotificationManager notificationManager = (NotificationManager) mainActivity.getSystemService(Context.NOTIFICATION_SERVICE);
        Intent setupIntent = new Intent(Settings.ACTION_NOTIFICATION_POLICY_ACCESS_SETTINGS);
        mainActivity.startActivity(setupIntent);
    }
}
