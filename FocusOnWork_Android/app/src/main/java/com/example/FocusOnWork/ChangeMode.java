package com.example.FocusOnWork;

import android.media.AudioManager;
import android.widget.Toast;

public class ChangeMode {
    /* MainActivityをいじる用の変数 */
    MainActivity mainActivity;
    /* AudioManagerをインスタンス生成 */
    AudioManager am;
    private static int ringermode = 0;
    Toast tst;

    public ChangeMode(Toast t, MainActivity m, AudioManager _am){
        am = _am;
        mainActivity = m;
        tst = t;
        /* 起動時のマナーモード状態を取得 */
        ringermode = am.getRingerMode();
    }

    public void PrintMode(int mode){
        tst.setText("RINGERMODE:" + Integer.toString(mode));
        tst.show();
    }

    public void MuteSounds(int change_mode){
        /* ミュート設定をONにする */
        switch (change_mode){
            case 2:
                am.setRingerMode(AudioManager.RINGER_MODE_SILENT);
                break;
            case 1:
                am.setRingerMode(AudioManager.RINGER_MODE_VIBRATE);
                break;
            case 0:
                am.setRingerMode(AudioManager.RINGER_MODE_NORMAL);
                break;
            default:
              break;
        }
        // PrintMode(change_mode);
    }

    public void RestoreSounds(){
        RestoreRingerMode restore_mode = new RestoreRingerMode();
        restore_mode.start();
        try {
            restore_mode.join();
        } catch (InterruptedException e) {
            e.printStackTrace();
        }
        am.setRingerMode(ringermode);

    }

    private class RestoreRingerMode extends Thread{
        public RestoreRingerMode(){ }
        public void run(){
            /* 音量を元に戻す */
            am.adjustStreamVolume(AudioManager.STREAM_MUSIC, AudioManager.ADJUST_RAISE, 0);
            am.adjustStreamVolume(AudioManager.STREAM_MUSIC, AudioManager.ADJUST_LOWER, 0);
            am.adjustStreamVolume(AudioManager.STREAM_NOTIFICATION, AudioManager.ADJUST_RAISE, 0);
            am.adjustStreamVolume(AudioManager.STREAM_NOTIFICATION, AudioManager.ADJUST_LOWER, 0);

            /* マナーモードを元に戻す */
            am.setRingerMode(AudioManager.RINGER_MODE_NORMAL);
        }
    }

}
