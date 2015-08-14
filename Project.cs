using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SGen_Tiler
{
    class Project
    {
        const string ErrorFileIntegrity = "Ошибка целостности файла, либо файл не поддерживается.";
        static Random RND = new Random();
        /// <summary>
        /// Редактируемый файл
        /// </summary>
        public static string FileName = "";
        /// <summary>
        /// Максимальное количество слоёв
        /// </summary>
        public const int MaxLayers = 8;
        /// <summary>
        /// Максимальная ширина карты
        /// </summary>
        public const int MaxWidth = 5000;
        /// <summary>
        /// Максимальная высота карты
        /// </summary>
        public const int MaxHeight = 5000;
        /// <summary>
        /// Сохранён ли проект (при изменении вызывать Project.Change();)
        /// </summary>
        public static bool Saved;
        /// <summary>
        /// Размер тайла в пикселях
        /// </summary>
        public static int TileSize;
        /// <summary>
        /// Ширина экрана
        /// </summary>
        public static int ScreenWidth;
        /// <summary>
        /// Высота экрана
        /// </summary>
        public static int ScreenHeight;
        /// <summary>
        /// Матрица карты [Слой, X, Y]
        /// </summary>
        public static ushort[,,] M;
        /// <summary>
        /// Ширина карты
        /// </summary>
        public static int Width;
        /// <summary>
        /// Высота карты
        /// </summary>
        public static int Height;
        /// <summary>
        /// Количество слоёв
        /// </summary>
        public static byte Layers;
        /// <summary>
        /// Смещение слоёв
        /// </summary>
        public static Parallax[] Px;
        /// <summary>
        /// Текстура тайлов
        /// </summary>
        public static string FileTexture = "";
        /// <summary>
        /// Текстура каркаса
        /// </summary>
        public static string FileKarkas = "";
        /// <summary>
        /// Текстура задника
        /// </summary>
        public static string FileBackground = "";
        /// <summary>
        /// Текстура передника
        /// </summary>
        public static string FileFront = "";
        /// <summary>
        /// Список анимаций
        /// </summary>
        public static List<Animation> Animations = new List<Animation>();
        /// <summary>
        /// Список правил автозаполнения
        /// </summary>
        public static List<AutoRule> AutoRules = new List<AutoRule>();
        /// <summary>
        /// Список правил случайного заполнения
        /// </summary>
        public static List<RandomTile> Randoms = new List<RandomTile>();
        /// <summary>
        /// Инструменты для быстрого доступа
        /// </summary>
        public static Tool[] Stamps = new Tool[10];

        /// <summary>
        /// Создание новой дефолтной карты
        /// </summary>
        public static void NewMap()
        {
            M = null; //Хм, интересно, но если не сделать предварительную очистку, возникает перегрузка памяти, 2015 год, а гигабайт - максимум :-(
            M = new ushort[MaxLayers + 1, MaxWidth, MaxHeight];
            TileSize = 32;
            ScreenWidth = 640;
            ScreenHeight = 480;
            //CalculateTiles();
            Width = 20;
            Height = 15;
            Layers = MaxLayers;
            Px= new Parallax[MaxLayers + 1];
            Px[0] = new Parallax();
            Px[1] = new Parallax(0);
            Px[2] = new Parallax(0.2f);
            Px[3] = new Parallax(0.4f);
            Px[4] = new Parallax(0.6f);
            Px[5] = new Parallax(0.8f);
            Px[6] = new Parallax(1);
            Px[7] = new Parallax(1);
            Px[8] = new Parallax(1.2f);
            Editor.Reset();
            AnimationClear();
            AutoRulesClear();
            RandomClear();
            StampesClear();
            Hystory.Clear();
            Saved = true;
        }

        /// <summary>
        /// Очистка штампов
        /// </summary>
        static void StampesClear()
        {
            for (int i = 0; i < 10; i++)
                Stamps[i] = new Tool();
        }

        /// <summary>
        /// Очистка правил случайного заполнения
        /// </summary>
        static void RandomClear()
        {
            RandomTile.Enable = true;
            Randoms.Clear();
        }

        /// <summary>
        /// Очистка правил анимации
        /// </summary>
        static void AnimationClear()
        {
            Animation.Enable = true;
            Animations.Clear();
        }

        /// <summary>
        /// Очистка правил автозаполнения и создание правила по умолчанию
        /// </summary>
        public static void AutoRulesClear()
        {
            AutoRule.Enable = false;
            AutoRule.Layer = 6;
            AutoRules.Clear();
            AutoRules.Add(new AutoRule(0, 0, 0));
        }

        /// <summary>
        /// Открытие заданного файла
        /// </summary>
        /// <param name="filename">Файл</param>
        public static void Load(string filename)
        {
            FileName = filename;
            Load();
        }

        /// <summary>
        /// Открытие текущего файла
        /// </summary>
        public static void Load()
        {
            //Открываем
            try
            {
                NewMap();
                BinaryReader file = new BinaryReader(new FileStream(FileName, FileMode.Open));

                if (file.ReadString() != Program.Name) throw new Exception("Файл не поддерживается.");  //Читаем маркер о формате
                int count;
                TileSize = file.ReadInt16();
                ScreenWidth = file.ReadInt16();
                ScreenHeight = file.ReadInt16();
                Layers = file.ReadByte();
                Width = file.ReadInt16();
                Height = file.ReadInt16();
                for (byte l = 0; l <= Layers; l++)
                {
                    if (file.ReadString() != "Layer" + l.ToString()) throw new Exception("Слой " + l.ToString() + " не обнаружен."); //Маркер слоя
                    bool OK = true;
                    do
                    {
                        int j = file.ReadUInt16();
                        if (j != 0xFFFF)
                        {
                            //Битность
                            byte bpt = file.ReadByte();
                            if (bpt == 1)
                            {
                                //Однобайтовая версия
                                byte p = 0;
                                for (int i = 0; i < Width; i++)
                                {
                                    byte n = file.ReadByte();
                                    if (n != 0xFF)
                                    {
                                        p = n;
                                        M[l, i, j] = p;
                                    }
                                    else
                                    {
                                        int c = file.ReadUInt16();
                                        i--;
                                        for (int s = 1; s < c; s++) M[l, i + s, j] = p;
                                        i += c - 1;
                                    }
                                }
                            }
                            else
                            {
                                //Двухбайтовая версия
                                ushort p = 0;
                                for (int i = 0; i < Width; i++)
                                {
                                    ushort n = file.ReadUInt16();
                                    if (n != 0xFFFF)
                                    {
                                        p = n;
                                        M[l, i, j] = p;
                                    }
                                    else
                                    {
                                        i--;
                                        int c = file.ReadUInt16();
                                        for (int s = 1; s < c; s++) M[l, i + s, j] = p;
                                        i += c - 1;
                                    }
                                }
                            }
                        }
                        else OK = false;
                    } while (OK);
                    Px[l] = new Parallax(file.ReadSingle(), file.ReadSingle());
                }
                //Текстуры
                if (file.ReadString() != "Textures") throw new Exception();
                FileBackground = file.ReadString();
                FileTexture = file.ReadString();
                FileFront = file.ReadString();
                FileKarkas = file.ReadString();
                //Правила анимации
                if (file.ReadString() != "Animation") throw new Exception(ErrorFileIntegrity); //Маркер параметров анимации
                count = file.ReadInt32();
                for (int i = 0; i < count; i++)
                    Animations.Add(new Animation(file.ReadUInt16(), file.ReadByte(), file.ReadByte(), (Animation.Types)file.ReadByte()));
                //Правила автозаполнения каркаса
                if (file.ReadString() != "AutoRules") throw new Exception(); //Маркер правил автозаполнения
                AutoRule.Layer = file.ReadByte();
                AutoRules.Clear();
                count = file.ReadInt32();
                for (int i = 0; i < count; i++) AutoRules.Add(new AutoRule(file.ReadUInt16(), file.ReadUInt16(), file.ReadUInt16()));
                //Параметры рандома
                if (file.ReadString() != "Random") throw new Exception();
                count = file.ReadInt32();
                for (int i = 0; i < count; i++) Randoms.Add(new RandomTile(file.ReadUInt16(), file.ReadUInt16(), file.ReadByte()));
                //Штампы
                if (file.ReadString() != "Stamps") throw new Exception();
                for (int s = 0; s < 10; s++)
                {
                    Stamps[s].Width = file.ReadByte();
                    Stamps[s].Height = file.ReadByte();
                    for (int i = 0; i < Stamps[s].Width; i++)
                        for (int j = 0; j < Stamps[s].Height; j++)
                            Stamps[s].M[i, j] = file.ReadUInt16();
                }
                if (file.ReadString() != "Options") throw new Exception();
                Editor.Codes = file.ReadBoolean();
                AutoRule.Enable = file.ReadBoolean();
                Animation.Enable = file.ReadBoolean();
                RandomTile.Enable = file.ReadBoolean();
                file.Close();
                Saved = true;
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Произошла ошибка при открытии файла.\n" + e.Message, Program.Name);
                NewMap();
                FileName = "";
            }
        }

        /// <summary>
        /// Сохранение проекта
        /// </summary>
        public static void Save()
        {
            try
            {
                BinaryWriter file = new BinaryWriter(new FileStream(FileName, FileMode.Create));
                file.Write(Program.Name);
                file.Write((short)TileSize);
                file.Write((short)ScreenWidth);
                file.Write((short)ScreenHeight);
                file.Write(Layers);
                file.Write((short)Width);
                file.Write((short)Height);
                for (int l = 0; l <= Layers; l++)
                {
                    file.Write("Layer" + l.ToString()); //Ставим маркер в файле для удобной визуализации
                    for (short j = 0; j < Height; j++)
                    {
                        //Проверяем какой максимум у строки
                        int max = 0;
                        for (int i = 0; i < Width; i++)
                            if (M[l, i, j] > max)
                                max = M[l, i, j];
                        //И, если строка содержит что-то больше ноля - значит она не пустая, пишем строку
                        if (max > 0)
                        {
                            //Пишем номер строки
                            file.Write(j);
                            //Задаём битность для строки
                            byte bpt = 2;
                            if (max < 255) bpt = 1;
                            file.Write(bpt);
                            if (bpt == 1)
                            {
                                //Однобайтная версия
                                for (int i = 0; i < Width; i++)
                                {
                                    byte n = (byte)M[l, i, j];
                                    ushort s = 1;
                                    file.Write(n);
                                    while (i + s < Width && M[l, i + s, j] == n) s++;
                                    if (s > 4)
                                    {
                                        file.Write((byte)0xFF);
                                        file.Write(s);
                                        i += s - 1;
                                    }
                                }
                            }
                            else
                            {
                                //Двухбайтная версия
                                for (int i = 0; i < Width; i++)
                                {
                                    ushort n = M[l, i, j];
                                    ushort s = 1;
                                    file.Write(n);
                                    while (i + s < Width && M[l, i + s, j] == n) s++;
                                    if (s > 3)
                                    {
                                        file.Write((ushort)0xFFFF);
                                        file.Write(s);
                                        i += s - 1;
                                    }
                                }
                            }
                        }
                    }
                    file.Write((UInt16)0xFFFF);
                    file.Write(Px[l].X);
                    file.Write(Px[l].Y);
                }
                file.Write("Textures");
                file.Write(FileBackground);
                file.Write(FileTexture);
                file.Write(FileFront);
                file.Write(FileKarkas);
                file.Write("Animation");
                file.Write(Animations.Count);
                foreach (Animation anim in Animations)
                {
                    file.Write(anim.Code);
                    file.Write(anim.Frames);
                    file.Write(anim.Time);
                    file.Write((byte)anim.Type);
                }
                file.Write("AutoRules");
                file.Write((byte)AutoRule.Layer);
                file.Write(AutoRules.Count);
                foreach (AutoRule rule in AutoRules)
                {
                    file.Write(rule.Code);
                    file.Write(rule.From);
                    file.Write(rule.To);
                }
                file.Write("Random");
                file.Write(Randoms.Count);
                foreach (RandomTile rand in Randoms)
                {
                    file.Write(rand.Code);
                    file.Write(rand.Tile);
                    file.Write(rand.Persent);
                }
                file.Write("Stamps");
                for (int s = 0; s < 10; s++)
                {
                    file.Write((byte)Stamps[s].Width);
                    file.Write((byte)Stamps[s].Height);
                    for (int i = 0; i < Stamps[s].Width; i++)
                        for (int j = 0; j < Stamps[s].Height; j++)
                            file.Write(Stamps[s].M[i, j]);
                }
                file.Write("Options");
                file.Write(Editor.Codes);
                file.Write(AutoRule.Enable);
                file.Write(Animation.Enable);
                file.Write(RandomTile.Enable);
                file.Close();
                Saved = true;
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Произошла ошибка при записи файла. Файл не сохранён.", Program.Name);
                FileName = "";
            }
        }

        /// <summary>
        /// Размещение ячейки на карте
        /// </summary>
        /// <param name="l">Номер слоя</param>
        /// <param name="x">Координата X</param>
        /// <param name="y">Координата Y</param>
        /// <param name="c">Код</param>
        public static void Put(int l, int x, int y, ushort c)
        {
            if (x < 0 | y < 0 | x >= Width | y >= Height) return;

            //Узнаём, есть ли правила рандома
            List<RandomTile> rnds = Randoms.FindAll(RandomTile => RandomTile.Code == c);
            if (rnds.Count > 0)
                foreach (RandomTile rnd in rnds)
                    if (RND.Next(100) > rnd.Persent) c = rnd.Tile;
            //Запоминаем что было в этой ячейке
            Hystory.AddRecord(l, x, y, M[l, x, y], c);
            //"Путим" ячейку
            M[l, x, y] = c;
            //Далее обрабатываем автозаполнение
            if (AutoRule.Enable & l == AutoRule.Layer & Px[l].X == 1 & Px[l].Y == 1)
                foreach (AutoRule rule in AutoRules)
                    if (rule.In(c))
                    {
                        //Запоминаем что было в этой ячейке
                        Hystory.AddRecord(0, x, y, M[0, x, y], rule.Code);
                        M[0, x, y] = rule.Code;
                    }
            Saved = false;
        }

        /// <summary>
        /// Опрос кода в ячейке
        /// </summary>
        /// <param name="l">Номер слоя</param>
        /// <param name="x">Координата X</param>
        /// <param name="y">Координата Y</param>
        /// <returns>Код</returns>
        public static ushort Get(int l, int x, int y)
        {
            if (x < 0 | y < 0 | x >= Width | y >= Height) return 0;
            return M[l, x, y];
        }

        /// <summary>
        /// Заголовок для окна
        /// </summary>
        public static string Label()
        {
            string name = "Новая карта";
            string star = "";
            if (!Saved) star = "*";
            if (FileName != "") name = Path.GetFileNameWithoutExtension(FileName);
            return name + star + " - " + Program.Name;
        }

        public static int ScaledSize { get { return (int)(TileSize * Editor.Scale); } }
    }
}
