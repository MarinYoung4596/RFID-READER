using System;
using System.Threading;
using System.Data;

namespace DopplerVisualization{
public class Worker
{
    // Volatile is used as hint to the compiler that this data
    // member will be accessed by multiple threads.
    private volatile bool _shouldStop;

    FormRealTime DSForm;
    public Worker (FormRealTime objet)
    {
        DSForm = objet;
    }

    // This method will be called when the thread is started.
    public void DoWork()
    {
        Thread.Sleep(2000);
        int i = 0;
        while (!_shouldStop)
        {
            //float value = (float)(6-i*0.2);
            //DSForm.updateTaginfo(0,value);
            //Thread.Sleep(250);
            //value = (float)(i * 0.2);
            //DSForm.updateTaginfo(1, value);

            DSForm.updateTaginfo(0, (float)((3.14 + 0.01 * i) / 3.14), DateTime.Now);
            Thread.Sleep(1000);
            i++;
        }
        Console.WriteLine("worker thread: terminating gracefully.");
    }
    public void RequestStop()
    {
        _shouldStop = true;
    }
}
}