using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SGen_Tiler
{
    class Parallax
    {
        /// <summary>
        /// Значение смещения по горизонтали
        /// </summary>
        public float X;
        /// <summary>
        /// Значение смещения по вертикали
        /// </summary>
        public float Y;

        /// <summary>
        /// Конструктор пустого значения. Создаются значения 1x1, например для каркаса
        /// </summary>
        public Parallax()
        {
            X = 1;
            Y = 1;
        }

        /// <summary>
        /// Конструктор нового значения c одинаковым смещением по горизонтали и вертикали
        /// </summary>
        /// <param name="x">Смещение по горизонтали и вертикали</param>
        public Parallax(float x)
        {
            X = x;
            Y = x;
        }

        /// <summary>
        /// Конструктор нового значения c разным смещением по горизонтали и вертикали
        /// </summary>
        /// <param name="x">Смещение по горизонтали</param>
        /// <param name="y">Смещение по вертикали</param>
        public Parallax(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
}
