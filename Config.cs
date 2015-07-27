using System;
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
        /// Файл с текстурой
        /// </summary>
        public static string FileTexture = "";
        /// <summary>
        /// Файл с каркасом
        /// </summary>
        public static string FileKarkas = "";

        /// <summary>
        /// Инициализация параметров
        /// </summary>
        public static void Load()
        {
            try
            {
                BinaryReader file = new BinaryReader(new FileStream(OpFile, FileMode.Open));
                FileTexture = file.ReadString();
                FileKarkas = file.ReadString();
                file.Close();
            }
            catch { }
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
                file.Write(FileTexture);
                file.Write(FileKarkas);
                file.Close();
            }
            catch { }
        }
    }
}
