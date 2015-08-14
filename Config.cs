﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SGen_Tiler
{
    class Config
    {
        static string OpFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\SG\\SGen Tiler";
        static string OpFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\SG\\SGen Tiler\\Config.bin";
        /// <summary>
        /// Инициализация параметров
        /// </summary>
        public static void Load()
        {
            try
            {
                BinaryReader file = new BinaryReader(new FileStream(OpFile, FileMode.Open));
                file.ReadString();
                file.Close();
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Похоже, программа запущена в первый раз.\n" +
                    "Нажмите Esc, что бы вызвать главное меню, в котором выберите файлы текстур и карту или создайте новую.", Program.Name);
            }
        }

        /// <summary>
        /// Сохранение параметров
        /// </summary>
        public static void Save()
        {
            try
            {
                Directory.CreateDirectory(OpFolder);
                BinaryWriter file = new BinaryWriter(new FileStream(OpFile, FileMode.Create));
                file.Write("NotFirstStart");
                file.Close();
            }
            catch { }
        }
    }
}
