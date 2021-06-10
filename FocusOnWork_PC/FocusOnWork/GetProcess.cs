using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

// メモリ量からゲーム検索？

namespace FocusOnWork {
    class GetProcess {
        /* 入出力ストリーム */
        StreamReader sr;
        StreamWriter writer;
        public ExpectedAppsName expectedAppsName = new ExpectedAppsName();
        public ActiveAppData[] activeAppDatas;

        /* バッシュで作るファイル名 */
        string textname = "result.txt";
        /* ゲームの検索に使う読み書きファイル */
        string textForGameSearch = "SearchWordAndResulut.txt";

        /* ウィンドウが最小化されているか見る変数 */
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsIconic(IntPtr hWnd);

        /* exeを実行するプロセス */
        Process procExe;


    public GetProcess() {
            procExe = new Process();
            procExe.StartInfo.FileName = "WikiSearch.exe";
        }

        public void GetTask() {
            // プロセス起動情報の構築
            ProcessStartInfo startInfo = new ProcessStartInfo();
            // コマンドプロンプトを起動
            startInfo.FileName = "cmd.exe";
            // 終了したら閉じる
            startInfo.Arguments = "/c ";
            // 実行するバッチ
            startInfo.Arguments += "getprocess";
            //　シェル機能を使用しない
            startInfo.UseShellExecute = false;
            // コンソール・ウィンドウを開かない
            startInfo.CreateNoWindow = true;
            // バッチを別プロセスとして実行
            var proc = Process.Start(startInfo);
            // バッチ処理が終了するまで待つ
            proc.WaitForExit();
                        
        }
            
        /* テキストのデータをまとめ上書き & データ取得*/
        public async Task GetDataFromFile() {
            /**/
            sr = new StreamReader(textname);
            string text;
            /* アプリ名を取得 */
            string[] procName = new string[0];
            /* 使用メモリ量 */
            string[] procMemory = new string[0];

            /* 1行ずつ読み出し */
            while ((text = await sr.ReadLineAsync()) != null) {
                string[] data = text.Split(" ");
                /* 不要なデータは無視 */
                if (data.Length != 3 || !(data[0].Contains(".exe"))) {
                    continue;
                } else {
                    data[1] = data[1].Replace(",", "");
                    int pos = 0;
                    int memsize = 0;
                    /* 既に読み込んだプロセスか確認 */
                    if ((pos = Array.IndexOf(procName, data[0])) == -1) {
                        /* 未知のとき */
                        /* リサイズ&追加 */
                        Array.Resize(ref procName, procName.Length + 1);
                        procName[procName.Length - 1] = data[0];
                        Array.Resize(ref procMemory, procMemory.Length + 1);
                        procMemory[procMemory.Length - 1] = data[1];

                    } else {
                        /* 既知のとき */
                        memsize = int.Parse(procMemory[pos]) + int.Parse(data[1]);
                        procMemory[pos] = memsize.ToString();
                    }
                }
            }
            sr.Close();

            /* データ書き込み */
            WriteData(procName, procMemory);

            /* アクティブなアプリを取得 */
            activeAppDatas = await ModifyData(procName, procMemory);

        }


        public async Task<int> EvaluateScoreOfProcess(int volumeFlg ) {
            int score = 0;
            string mainProcess = "なし";
            await Task.Run(() => GetDataFromFile());
            foreach (ActiveAppData activeAppData in activeAppDatas) {
                Debug.WriteLine(activeAppData.appName + "," + activeAppData.windowTitle + ", " + activeAppData.usingMemory.ToString());

                /* 点数によってみなくてよい作業は省略する */
                /* 現在の点数が2点以上のときオンライン会議，映画鑑賞の作業を参照 */
                if (score < 2) {
                    /* オンライン or 動画視聴中か判断 */
                    /* まずはスピーカ－が使用中か判定 */
                    if(volumeFlg == 1) {
                        /* 該当アプリ使用中のときスコア更新 */
                        if(CompareProcessName(activeAppData.appName, expectedAppsName.aboutMovieApps)) {
                            score = 2;
                            break;
                        }
                        /* GoogleChromeがひらかれているとき */
                        else if (activeAppData.appName.Equals(expectedAppsName.chrome)) {
                            if(CompareProcessName(activeAppData.windowTitle, expectedAppsName.chromeMovie)) {
                                score = 2;
                                break;
                            }
                        }
                    /* 使用中でなければ終わり */
                    } else {
                        ;
                    }
                }

                /* 現在の点数が1点未満のときはメール，書類作成，ゲーム(オンライン)の作業を参照 */
                if(score < 1) {
                    /* 書類作成のアプリを参照 */
                    if (CompareProcessName(activeAppData.appName, expectedAppsName.officeApps)) {
                        score = 1;
                        continue;
                    }
                    /* メールアプリを参照 */
                    else if (CompareProcessName(activeAppData.appName, expectedAppsName.mailApps)) {
                        score = 1;
                        continue;
                    }
                    /* GoogleChromeがひらかれているとき */
                    else if (activeAppData.appName.Equals(expectedAppsName.chrome)) {
                        if (CompareProcessName(activeAppData.windowTitle, expectedAppsName.chromeMail)) {
                            score = 1;
                            continue;
                        } else {
                            foreach (string exname in expectedAppsName.chromeMail) {
                                if (activeAppData.windowTitle.Contains(exname)){
                                    score = 1;
                                    continue;
                                }
                            }
                        }
                        /* choromeでゲームをやっているかをチェック(メモリ使用量が多いとき) */
                        if (activeAppData.usingMemory >1500000) {
                            if (await IsGame(activeAppData.windowTitle)) {
                                score = 1;
                                continue;
                            }
                        }
                    /* ゲームをチェック(メモリ使用量が多いとき) */
                    } else if(activeAppData.usingMemory > 600000) {
                        if (await IsGame(activeAppData.appName)) {
                            score = 1;
                            continue;
                        }                    
                    }else {
                        ;
                    }
                }

            }

            Debug.WriteLine("");
            return score;
        }

        /* ゲームか検索する関数 */
        private async Task<bool> IsGame(string title) {
            /* ファイルに書き込み＆上書き保存 */
            writer = new StreamWriter(textForGameSearch, false);
            await writer.WriteLineAsync(title.Replace(".exe", "").Replace(".Windows",""));
            Debug.WriteLine(title.Replace(".exe", ""));
            writer.Close();
            /* Wikiでゲーム名を検索 */ 
            procExe.Start();
            procExe.WaitForExit();
            sr = new StreamReader(textForGameSearch);
            string text = await sr.ReadLineAsync();
            Debug.WriteLine(text);
            if (text.Equals("error")){
                text = await sr.ReadLineAsync();
                sr.Close();
                writer = new StreamWriter(textForGameSearch, false);
                await writer.WriteLineAsync(text);
                writer.Close();
                procExe.Start();
                procExe.WaitForExit();
                sr = new StreamReader(textForGameSearch);
                text = await sr.ReadLineAsync();

            }
            sr.Close();
            if (text.Equals("True")) {
                return true;
            } else {
                return false; 
            }
        }

        /* ウィンドウがアクティブならメモリ量を取得し，データを保存 */
        private async Task<ActiveAppData[]> ModifyData(string[] appNames ,string[] memoryData) {
            /* データ格納用変数 */
            ActiveAppData[] activeApp = new ActiveAppData[0];

            /* C#から現在の起動中アプリを取得 */
            /* Zoomがあると読み込めない？ */
            foreach (Process p in Process.GetProcesses()) {
                /* ウィンドウがアクティブならデータ格納 */
                if (!IsIconic(p.MainWindowHandle) && !p.MainWindowTitle.Equals("") && p.MainWindowHandle != IntPtr.Zero) {
                    /* リサイズ */
                    Array.Resize(ref activeApp, activeApp.Length + 1);
                    activeApp[activeApp.Length - 1].appName = p.ProcessName + ".exe";
                    activeApp[activeApp.Length - 1].usingMemory = GetMemoryData(p.ProcessName + ".exe", appNames, memoryData);
                  
                    /* クロームのときメインウィンドウのタイトル取得 */
                    if ("chrome".Equals(p.ProcessName)) {
                        activeApp[activeApp.Length - 1].windowTitle = GetMainTitle(p.MainWindowTitle);
                        /* 特殊なexeで開かれるアプリ */
                    }else if ("WWAHost".Equals(p.ProcessName) || "ApplicationFrameHost".Equals(p.ProcessName)) {
                        activeApp[activeApp.Length - 1].windowTitle = p.MainWindowTitle;
                    } else { activeApp[activeApp.Length - 1].windowTitle = p.MainWindowTitle; }
                } else {
                    /* メモリ消費量が多いときは取得 */
                    if(GetMemoryData(p.ProcessName + ".exe", appNames, memoryData)> 600000 && !"chrome".Equals(p.ProcessName)) {
                        Array.Resize(ref activeApp, activeApp.Length + 1);
                        activeApp[activeApp.Length - 1].appName = p.ProcessName + ".exe";
                        activeApp[activeApp.Length - 1].usingMemory = GetMemoryData(p.ProcessName + ".exe", appNames, memoryData);
                        activeApp[activeApp.Length - 1].windowTitle = p.MainWindowTitle;
                    }
                   
                }
            }
            return activeApp;
        }


        /* 比較してくれる関数 */
        private bool CompareProcessName(string name, string[] expectNames) {
            foreach(string exceptName in expectNames) {
                /* 一致する名前があったとき */
                if (name.Equals(exceptName)) {
                    return true;
                }
            }
            return false;
        }


        /* chrome系のときメインのタイトルのみ取得 */
        private string GetMainTitle(string title) {

            /* - ****** - Google Chrome となってるので*****を取得
             * 先頭のハイフンは : or | or / のときがあるのでそれも考慮 
            */

            /* 先頭の文字 */
            string[] initials = new string[] { "-", ":", "|", "/"};

            /* 先頭の位置 */
            int startPos = 0;
            /* 末尾の位置 */
            int endPos = 0;
            /* *****の部分 */
            string extractedTitle = "";
            /* 便宜上charへ変換 */
            char[] charTitle  = title.ToCharArray();

            /* まず末尾を取得 */
            endPos = title.IndexOf(" - Google Chrome");
            /* 後ろから検索 */
            for (int i = endPos; i > 0; i--) {
                /* 指定文字まで検索 */
                if(initials.Contains(charTitle[i].ToString())) {
                    startPos = i;
                    break;
                }
            }
           

            /* 抽出 */
            /* **** - のとき*/
            if(startPos == 0) {
                extractedTitle = title.Substring(startPos, endPos);
                /* - ***** - のとき */
            } else { extractedTitle = title.Substring(startPos + 2, endPos - (startPos + 2)); }

            return extractedTitle;
        }


        /* 指定されたアプリのメモリ量を取得 */
        private int GetMemoryData(string name, string[] appNames, string[] memoryData) {
            
            int i = 0;
            /* 名前が一致したらメモリ量をリターン */
            foreach(string appName in appNames) {
                if (name.Equals(appName)) {
                    return int.Parse(memoryData[i]);
                }
                i++;
            }

            /* 該当なしのとき */
            return -1;
        } 


        /* データ書き込み */
        private async void  WriteData(string[] data, string[] memory) {
            /* ファイルに書き込み＆上書き保存 */
            writer = new StreamWriter(textname, false);
            for (int i = 0; i < data.Length; i++) {
                if (i != (data.Length - 1)) {
                    await writer.WriteLineAsync(data[i] + " " + memory[i]);
                } else { await writer.WriteAsync(data[i] + " " + memory[i]); }
            }
            writer.Close();

        }

    }

    struct ActiveAppData {
        public string appName;
        public string windowTitle;
        public int usingMemory;
    }


    class ExpectedAppsName {
        /*--------------------- プロセス取得に使う変数 -----------------------*/
        /* 例外のアプリ */
        public string[] exceptApps = new string[]{
            "Video.UI.exe", // 映画 ＆ テレビ 
            "SystemSettings.exe", // VisualStudioを起動してるから？
            "FocusOnWork.exe", // このアプリ
            "WindowsInternal.ComposableShell.Experiences.TextInput.InputApp.exe" //何だろう
        };

        /* Office系 */
        public string[] officeApps = new string[] {
            "WINWORD.exe",
            "EXCEL.exe",
            "POWERPNT.exe"
        };

        /* メール */
        public string[] mailApps = new string[] {
            "OUTLOOK.exe"
        };

        /* 動画系 */
        public string[] aboutMovieApps = new string[] {
            "PrimeVideo.exe",
            "Zoom.exe"
        };

        /* SNS */
        public string[] SNSApps = new string[] {
            "LINE.exe",
            "slack.exe"
        };

        /* WWAHost.exe or ApplicationFrameHost.exeで開かれるもの */
        // "WWAHost.exe"
        //  ApplicationFrameHost.exe",
        public string[] uncommonApps = new string[] {
            "Instagram",
            "Twitter",
            "Netflix",
            "Amazon Prime Video for Windows"
        };

        /* ---------- chrome系 ------------- */
        public string chrome = "chrome.exe";

        //- **** -Google Chromeのかたち　ハイフン前半角スペース 要素が少ないと*** -
        //後ろのハイフンから-or/ or:まで？ 先頭になったらおしまい
        // 検索ってはいってなかったらセーフ？
        /* メール */
        public string[] chromeMail = new string[]{
            "メール",
            "Gmail"
        };
        /* SNS */
        public string[] chromeSNS = new string[]{
            "Instagram",
            "Twitter", //  / *** -
            "Facebook"
        };

        public string[] chromeMovie = new string[]{
            "Prime Video",//: *** -
            "YouTube",
            "Netflix",
            };
        /* ------------------------------- */



        /* ゲーム　*/

        /*
         * 保留
         */

        /*--------------------------------------------------------------------*/
    }
}
