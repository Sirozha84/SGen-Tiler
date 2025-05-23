﻿using System;
using System.Windows.Forms;

namespace SGen_Tiler
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        public static Main game; //Вот как мне пришлось изъебнуться чтобы получить доступ к нестатичным полям класса game из других классов
        public const string Version = "Версия: 3.3 - 18 мая 2016 года";
        public const string Autor = "Автор: Сергей Гордеев";
        public const string Web = "http://www.sg-software.ru";
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] arg)
        {
            Application.EnableVisualStyles();
            game = new Main(arg);
            game.Run();
            if (!Project.Saved && MessageBox.Show("Сохранить карту перед выходом?",
                Application.ProductName, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if (Project.FileName == "")
                {
                    //Диалог сохранения
                    SaveFileDialog save = new SaveFileDialog();
                    save.Filter = FormMenu.FilterMAP;
                    if (save.ShowDialog() == DialogResult.Cancel) return;
                    Project.FileName = save.FileName;
                }
                Project.Save(false);
            }
        }
    }
#endif
}
