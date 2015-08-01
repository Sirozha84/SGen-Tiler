using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SGen_Tiler
{
    class Hystory
    {
        static List<ChangeString> List = new List<ChangeString>();
        static int Position;

        /// <summary>
        /// Добавление ключевой записи. Добавляется после внесения всех изменений.
        /// </summary>
        public static void AddRecord()
        {
            ChangeString r = new ChangeString();
            r.Key = true;
            List.Add(r);
            Position = List.Count - 1;
        }

        /// <summary>
        /// Добавление записи об изменении.
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="was"></param>
        /// <param name="became"></param>
        public static void AddRecord(int layer, int x, int y, ushort was, ushort became)
        {
            //Здесь надо сделать очистку всего что правей Position, если такое имеется
            if (Position < List.Count - 1)
                List.RemoveRange(Position + 1, List.Count - Position - 1);
            //А потом уже пишем историю
            ChangeString r = new ChangeString();
            r.Key = false;
            r.Layer = layer;
            r.X = x;
            r.Y = y;
            r.Was = was;
            r.Became = became;
            List.Add(r);
            Position = List.Count - 1;
        }

        /// <summary>
        /// Очистка истории
        /// </summary>
        public static void Clear()
        {
            List.Clear();
            AddRecord();    //Добавляем самый первый ключ.. ещё подумать как лучше сделать
        }

        /// <summary>
        /// Отмена
        /// </summary>
        public static void Undo()
        {
            while (Position > 0)
            {
                Position--;
                if (List[Position].Key) break;
                else Project.M[List[Position].Layer, List[Position].X, List[Position].Y] = List[Position].Was;
            }
        }

        /// <summary>
        /// Повтор
        /// </summary>
        public static void Redo()
        {
            while (Position < List.Count - 1)
            {
                Position++;
                if (List[Position].Key) break;
                else Project.M[List[Position].Layer, List[Position].X, List[Position].Y] = List[Position].Became;
            }
        }
    }

    struct ChangeString
    {
        public bool Key;
        public int Layer;
        public int X;
        public int Y;
        public ushort Was;
        public ushort Became;
    }
}
