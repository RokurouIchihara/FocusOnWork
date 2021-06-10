using NAudio.CoreAudioApi;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace FocusOnWork {
    class GetVolume {

        /* デバイスのスピーカー情報を取得 */
         MMDeviceEnumerator devEnum = new MMDeviceEnumerator();
         MMDevice defaultDevice;

        public GetVolume() {
        }

        /* スピーカーを使用しているかを返す */
        public async Task<int> GetVolumeFlg() {

            /* 30秒間のスピーカー状態をとるため，3秒に一回データを取得
             * 30%以上音が出ていたらスピーカー使用中と仮定
             * しかし，後半の9秒音が出ていなかったら使用をやめたとする
             * また，後半の9秒音が出ていなかったら使用を開始したとする
             */

            /* データ取得 */
            int[] flg = new int[30 / 3];
            for (int i = 0; i < 30 / 3; i++) {
                /* 1秒間データを取る */
                flg[i] = await Getvol();
                /* 2秒待つ */
                await Task.Delay(2000);

            }
            for (int i = 0; i < 30 / 3; i++) {
                Debug.Write(flg[i].ToString() + ", ");
            }
            Debug.WriteLine("On is " + (flg.Count(value => value == 1) * 100 / flg.Length).ToString() + "%");
            
            /* 30%以上だったら判定へ */
            if ((flg.Count(value => value == 1) * 100 / flg.Length) > 30) {
                /* 最後の9秒の状態を取得 */
                for(int i = flg.Length - 3;i < flg.Length;i++) {
                    if(flg[i] == 1) {
                        return 1;
                    }
                }
                /* すべて0のとき使用していないとする */
                return 0;

            } else {
                /* 最後の9秒の状態を取得 */
                for (int i = flg.Length - 3; i < flg.Length; i++) {
                    if (flg[i] == 0) {
                        return 0;
                    }
                }
                /* すべて1のとき使用しているとする */
                return 1;
            }

        }


        /* 音量を取得 */
        private async Task<int> Getvol() {
            int[] volume = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            for (int i = 0; i < 10; i++) {
                defaultDevice = devEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                volume[i] = (int)Math.Round(defaultDevice.AudioMeterInformation.MasterPeakValue * 100);
                /* 0.1秒ごとに取得 */
                await Task.Delay(100);
            }
            int result = await Task<int>.Run(() => AnalyzeVol(volume));
            return result;
        }

        private int AnalyzeVol(int[] volume) {
            int use= 0;
            for(int i = 0; i < volume.Length; i++) {
                Debug.Write(volume[i].ToString()+",");
                /* volumeが3以上のとき使用中でカウント */
                if(volume[i] >= 3) {
                    use++;
                }
            }
            Debug.WriteLine("");
            /* 0.3秒以上音が流れていたら */
            if (use >= 3) {
                return 1;
            } else { return 0; }
        }

    }
}

