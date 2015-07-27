using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SGen_Tiler
{
    class Control
    {
        public enum Levels { Normal, MouseOn, Clicked }
        int X;
        int Y;
        int tX;
        int tY;
        int Width;
        int Height;

        /// <summary>
        /// Конструктор контрола
        /// </summary>
        /// <param name="x">Координата X</param>
        /// <param name="y">Координата Y</param>
        /// <param name="tx">Координата X в текстуре</param>
        /// <param name="ty">Координата Y в текстуре</param>
        /// <param name="width">Ширина</param>
        /// <param name="height">Высота</param>
        public Control(int x, int y, int tx, int ty, int width, int height)
        {
            X = x;
            Y = y;
            tX = tx;
            tY = ty;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Проверка расположения мыши над контролом
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>Возвращает true, если мышь над контролом</returns>
        public bool MouseOn(int x, int y)
        {
            return x >= X & x <= X + Width & y >= Y & y <= Y + Height;
        }

        /// <summary>
        /// Рисование контрола
        /// </summary>
        /// <param name="level">Уровень кнопки</param>
        public void Draw(Levels level)
        {
            Color bac = new Color(0, 0, 0, 0);
            Color col = Color.Gray;
            if (level == Levels.MouseOn) { bac = Color.DarkGray; col = Color.White; }
            if (level == Levels.Clicked) { bac = Color.Gray; col = Color.White; }
            Game1.spriteBatch.Draw(Game1.WhitePixel, new Rectangle(X, Y, Width, Height), new Rectangle(0, 0, 1, 1), bac);
            Game1.spriteBatch.Draw(Game1.ControlPanel, new Vector2(X, Y), new Rectangle(tX, tY, Width, Height), col);
        }
    }
}
