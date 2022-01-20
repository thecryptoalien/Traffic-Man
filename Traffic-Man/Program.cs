using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
//using System.Threading;
//using System.Threading.Tasks;
using System.Windows.Forms;

namespace Traffic_Man
{
    static class Program
    {


        private static Mutex mutex = null;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]

         
        static void Main()
        {

            const string appName = "Traffic-Man";
            bool createdNew;

            mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                string instanceMsgT = "Traffic-Man Already Running!";
                string instanceMsg = "Only One Instance Of Traffic-Man Is Allowed!";
                MessageBox.Show(instanceMsg, instanceMsgT, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

        }
    }
}
