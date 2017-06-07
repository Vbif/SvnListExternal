using System;
using System.Windows.Forms;

namespace SvnListExternal
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(String[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            String path = null;

            if (args.Length == 1)
                path = args[0];
            
            Application.Run(new MainForm() { TargetPath = path });
        }
    }
}
