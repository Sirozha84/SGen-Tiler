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
        /// Масштаб
        /// </summary>
        public static float Scale;
        /// <summary>
        /// Показывать ли коды тайлов
        /// </summary>
        public static bool Codes;
        /// <summary>
        /// Режим отображения фантомного слоя (0 - выкл, 1 - прозрачный, 2 - видимый)
        /// </summary>
        public static byte Phantom;

        /// <summary>
        /// Сброс переменных для работы с новой карты
        /// </summary>
        public static void Reset()
        {
            X = 0;
            Y = 0;
            Layer = 1;
            ShowOnlyCurrentLayer = false;
            Scale = 1;
            Codes = false;
            Phantom = 2;
        }
    }
}