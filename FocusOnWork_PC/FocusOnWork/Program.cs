using System;
using System.Windows.Forms;

namespace FocusOnWork {
    static class Program {

        [STAThread]
        static void Main() {
            ApplicationManage.SetApplication();
        }

        public static class ApplicationManage {
            static Form form = new Form1();
            public static void SetApplication() {
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(form);
               
            }

        }
    }
 }
    //�A�v�����Ő؂�ꂽ�Ƃ��̗�O�����ǉ�

