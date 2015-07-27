using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SGen_Tiler
{
    class Tool //Графический инструмент
    {
        /// <summary>
        /// Ширина инстумента
        /// </summary>
        public int Width = 1;

        /// <summary>
        /// Высота инструмента
        /// </summary>
        public int Height = 1;

        /// <summary>
        /// Матрица инструмента
        /// </summary>
        public ushort[,] M = new ushort[100, 100];

        /// <summary>
        /// Позиция прокрутки в режиме выбора тайла
        /// </summary>
        public int Scroll = 0;

        /// <summary>
        /// Вычисляет номер блока по горизонтали из значения мыши
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static int Xcalc(int x)
        {
            int X = x / Project.TileSize;
            if (X < 0) X = 0;
            if (X > Project.ScreenWidth / Project.TileSize - 1) X = Project.ScreenWidth / Project.TileSize - 1;
            return X;
        }
    }
}
