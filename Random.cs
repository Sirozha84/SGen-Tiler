using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SGen_Tiler
{
    class Random
    {
        /// <summary>
        /// Включен ли рандом
        /// </summary>
        public static bool Enable = true;
        /// <summary>
        /// Код
        /// </summary>
        public ushort Code;
        /// <summary>
        /// Случайый тайл
        /// </summary>
        public ushort Tile;
        /// <summary>
        /// Процент вероятности
        /// </summary>
        public byte Persent;
        /// <summary>
        /// Конструктор правила
        /// </summary>
        /// <param name="code">Код</param>
        /// <param name="tile">Случайный тайл</param>
        /// <param name="persent">Процент вероятности</param>
        public Random(ushort code, ushort tile, byte persent)
        {
            Code = code;
            Tile = tile;
            Persent = persent;
        }
    }
}
