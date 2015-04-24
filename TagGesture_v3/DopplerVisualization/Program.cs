using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;

namespace DopplerVisualization
{
    static class Program
    {
        static FormRealTime DSForm;
        static List<String> TagsEPC = new List<String>();
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            InitTagsEPC();
            DSForm = new FormRealTime(TagsEPC);
            // Create the thread object. This does not start the thread.
            Worker workerObject = new Worker(DSForm);
            Thread workerThread = new Thread(workerObject.DoWork);

            // Start the worker thread.
            workerThread.Start();
            Console.WriteLine("main thread: Starting worker thread...");

            Application.Run(DSForm);
            workerObject.RequestStop();


        }
        static void InitTagsEPC()
        {
            TagsEPC.Add("E200 0001");
            TagsEPC.Add("E200 0002");
            //TagsEPC.Add("E200 0003");
            //TagsEPC.Add("E200 0004");
            //TagsEPC.Add("E200 0005");
        }
    }
}
