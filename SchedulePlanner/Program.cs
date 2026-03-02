using System;
using System.Windows.Forms;
using SchedulePlanner.UI;

namespace SchedulePlanner
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}