using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SGen_Tiler
{
    class Select
    {
        /// <summary>
        /// Начальная координата X
        /// </summary>
        static int StartX;

        /// <summary>
        /// Начальная координата Y
        /// </summary>
        static int StartY;

        /// <summary>
        /// Конечная координата X
        /// </summary>
        static int EndX;

        /// <summary>
        /// Конечная координата Y
        /// </summary>
        static int EndY;

        /// <summary>
        /// Продолжается ли выделение в данный момент?
        /// </summary>
        public static bool Active;

        /// <summary>
        /// Установка начальных координат выделения
        /// </summary>
        /// <param name="x">Координата X</param>
        /// <param name="y">Координата Y</param>
        public static void Start(int x, int y)
        {
            StartX = x;
            StartY = y;
            Active = true;
        }

        /// <summary>
        /// Конец выделения
        /// </summary>
        /// <param name="x">Координата X</param>
        /// <param name="y">Координата Y</param>
        public static void End(int x, int y)
        {
            EndX = x;
            EndY = y;
        }

        /// <summary>
        /// Левый край выделения
        /// </summary>
        /// <returns></returns>
        public static int Left
        {
            get
            {
                if (Active)
                    return StartX <= EndX ? StartX : EndX; //Надо бы запомнить приёмчик, очень удобно и кратко
                else
                    return EndX;
            }
        }

        /// <summary>
        /// Верхний край выделения
        /// </summary>
        /// <returns></returns>
        public static int Top
        {
            get
            {
                if (Active)
                    return StartY <= EndY ? StartY : EndY;
                else
                    return EndY;
            }
        }

        /// <summary>
        /// Ширина выделенного фрагмента
        /// </summary>
        /// <returns></returns>
        public static int Width
        {
            get
            {
                if (Active)
                    return StartX <= EndX ? EndX - StartX + 1 : StartX - EndX + 1;
                else
                    return 1;
            }
        }

        /// <summary>
        /// Высота выделенного фрагмента
        /// </summary>
        /// <returns></returns>
        public static int Height
        {
            get
            {
                if (Active)
                    return StartY <= EndY ? EndY - StartY + 1 : StartY - EndY + 1;
                else
                    return 1;
            }
        }
    }
}