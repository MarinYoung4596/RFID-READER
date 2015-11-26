using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DopplerVisualization
{
    class VisualizationTest
    {
        static FormRealTime DSForm;


        static void Main(string[] args)
        {
            DSForm = new FormRealTime();
            Application.Run(DSForm);

            //DSForm.updateTaginfo(row);
        }

    }
}
