namespace SGen_Tiler
{
    class Editor
    {
        /// <summary>
        /// Текущий слой
        /// </summary>
        public static int Layer;
        /// <summary>
        /// Позиция камеры по координате X
        /// </summary>
        public static int X;
        /// <summary>
        /// Позиция камеры по координате Y
        /// </summary>
        public static int Y;
        /// <summary>
        /// Показывать только текущий слой
        /// </summary>
        public static bool ShowOnlyCurrentLayer;
        /// <summary>
        /// Показывать ли коды тайлов
        /// </summary>
        public static bool Codes;
        /// <summary>
        /// Сброс переменных для работы с новой карты
        /// </summary>
        public static void Reset()
        {
            X = 0;
            Y = 0;
            Layer = 1;
            ShowOnlyCurrentLayer = false;
            Codes = false;
        }
    }
}