using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SGen_Tiler
{
    class AutoRule
    {
        /// <summary>
        /// Включен ли режим автозаполнения
        /// </summary>
        public static bool Enable;
        /// <summary>
        /// Слой, с которым работает автозаполнение
        /// </summary>
        public static int Layer;
        /// <summary>
        /// Код каркаса
        /// </summary>
        public ushort Code;
        /// <summary>
        /// Начальный тайл
        /// </summary>
        public ushort From;
        /// <summary>
        /// Конечный тайл
        /// </summary>
        public ushort To;
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="code">Код на каркасе</param>
        /// <param name="from">От</param>
        /// <param name="to">До</param>
        public AutoRule(ushort code, ushort from, ushort to)
        {
            Code = code;
            From = from;
            To = to;
        }
        /// <summary>
        /// Подходит ли код под это правило
        /// </summary>
        /// <param name="c">Код</param>
        /// <returns></returns>
        public bool In(ushort c)
        {
            return c >= From & c <= To;
        }
    }
}
