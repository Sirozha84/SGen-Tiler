using System;

namespace SGen_Tiler
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        public static Main game; //Вот как мне пришлось изъебнуться чтобы получить доступ к нестатичным полям класса game из других классов
        public const string Name = "SGen Tiler";
        public const string Version = "Версия: 2.2 - 18 июля 2015 года";
        public const string Autor = "Автор: Сергей Гордеев";
        public const string Web = "http://www.sg-software.ru";
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] arg)
        {
            //using (var game = new Game1())
            //    game.Run();
            //arg = new string[1];
            //arg[0] = @"c:\Users\sg\Dropbox\Проекты\map.map";

            game = new Main(arg);
            game.Run();
            if (!Project.Saved && System.Windows.Forms.MessageBox.Show("Сохранить карту перед выходом?", Name,
                System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                Project.Save();
            Config.Save();
        }
    }
#endif
}
