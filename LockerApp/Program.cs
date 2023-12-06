using System;
using System.Windows.Forms;

namespace LockerApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new StartForm()); // Start with a login form
        }
    }
}
