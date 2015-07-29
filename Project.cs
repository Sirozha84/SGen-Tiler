﻿using System;
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
        /// Прикреплять ли текстуру к карте
        /// </summary>
        public static bool AttachTexture;
        /// <summary>
        /// Прикреплять ли каркас к карте
        /// </summary>
        public static bool AttachCarcase;
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
                BinaryReader file = new BinaryReader(new FileStream(Project.FileName, FileMode.Open));

                if (file.ReadString() != Program.Name) throw new Exception("Файл не поддерживается.");  //Читаем маркер о формате
                int count;
                TileSize = file.ReadInt16();
                ScreenWidth = file.ReadInt16();
                ScreenHeight = file.ReadInt16();
                Layers = file.ReadByte();
                Width = file.ReadInt16();
                Height = file.ReadInt16();
                for (byte l = 0; l < Layers; l++)
                {
                    if (file.ReadString() != "Layer" + l.ToString()) throw new Exception("Слой " + l.ToString() + " не обнаружен."); //Маркер слоя
                    bool OK = true;
                    do
                    {
                        int j = file.ReadUInt16();
                        if (j != 0xFFFF)
                        {
                            int FirstTile = file.ReadInt16();
                            int LastTile = file.ReadInt16();
                            for (int i = FirstTile; i <= LastTile; i++) M[l, i, j] = file.ReadUInt16();         
                        }
                        else OK = false;
                    } while (OK);
                    Px[l] = new Parallax(file.ReadSingle(), file.ReadSingle());
                }
                //Правила анимации
                if (file.ReadString() != "Animation") throw new Exception(ErrorFileIntegrity); //Маркер параметров анимации
                count = file.ReadInt32();
                for (int i = 0; i < count; i++)
                    Animations.Add(new Animation(file.ReadUInt16(), file.ReadByte(), file.ReadByte(), (Animation.Types)file.ReadByte()));
                file.Close();
                file = new BinaryReader (new FileStream(Project.FileName + " extra", FileMode.Open));
                //Правила автозаполнения каркаса
                if (file.ReadString() != "AutoRules") throw new Exception(ErrorFileIntegrity); //Маркер правил автозаполнения
                AutoRule.Layer = file.ReadByte();
                count = file.ReadInt32();
                AutoRules.Clear();
                for (int i = 0; i < count; i++) AutoRules.Add(new AutoRule(file.ReadUInt16(), file.ReadUInt16(), file.ReadUInt16()));
                //Параметры рандома
                if (file.ReadString() != "Random") throw new Exception(ErrorFileIntegrity);
                count = file.ReadInt32();
                for (int i = 0; i < count; i++) Randoms.Add(new RandomTile(file.ReadUInt16(), file.ReadUInt16(), file.ReadByte()));
                //Прикреплённые файлы (если есть)
                if (file.ReadString() != "Attach") throw new Exception(ErrorFileIntegrity);
                string ft = file.ReadString();
                AttachTexture = ft != "";
                if (AttachTexture) Config.FileTexture = ft;
                ft = file.ReadString();
                AttachCarcase = ft != "";
                if (AttachCarcase) Config.FileKarkas = ft;
                //Штампы
                if (file.ReadString() != "Stamps") throw new Exception(ErrorFileIntegrity);
                for (int s = 0; s < 10; s++)
                {
                    Stamps[s].Width = file.ReadByte();
                    Stamps[s].Height = file.ReadByte();
                    for (int i = 0; i < Stamps[s].Width; i++)
                        for (int j = 0; j < Stamps[s].Height; j++)
                            Stamps[s].M[i, j] = file.ReadUInt16();
                }
                if (file.ReadString() != "Options") throw new Exception(ErrorFileIntegrity);
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
                //BinaryWriter file = new BinaryWriter(new FileStream(@"c:\Users\sg\Dropbox\Проекты\tets.map", FileMode.Create));
                file.Write(Program.Name);
                file.Write((short)TileSize);
                file.Write((short)ScreenWidth);
                file.Write((short)ScreenHeight);
                file.Write(Layers);
                file.Write((short)Width);
                file.Write((short)Height);
                for (int l = 0; l < Layers; l++)
                {
                    file.Write("Layer" + l.ToString()); //Ставим маркер в файле для удобной визуализации
                    for (short j = 0; j < Height; j++)
                    {
                        if (!FreeString(l, j))
                        {
                            file.Write(j);  //Ставим номер строки
                            int FirstTile = 0; while (M[l, FirstTile, j] == 0 & FirstTile + 1 < Width) FirstTile++;
                            int LastTile = Width - 1; while (M[l, LastTile, j] == 0 & LastTile > 0) LastTile--;
                            file.Write((short)FirstTile);
                            file.Write((short)LastTile);
                            for (int i = FirstTile; i <= LastTile; i++) file.Write(M[l, i, j]);
                        }
                    }
                    file.Write((UInt16)0xFFFF);
                    file.Write(Px[l].X);
                    file.Write(Px[l].Y);
                }
                file.Write("Animation");
                file.Write(Animations.Count);
                foreach (Animation anim in Animations)
                {
                    file.Write(anim.Code);
                    file.Write(anim.Frames);
                    file.Write(anim.Time);
                    file.Write((byte)anim.Type);
                }
                file.Close();
                //Сохраняем дополнительные данные в отдельный файл
                file = new BinaryWriter(new FileStream(FileName+" extra", FileMode.Create));
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
                file.Write("Attach");
                if (AttachTexture) file.Write(Config.FileTexture); else file.Write("");
                if (AttachCarcase) file.Write(Config.FileKarkas); else file.Write("");
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
        /// Проверка на пустоту строки
        /// </summary>
        /// <param name="l">Номер слоя</param>
        /// <param name="n">номер строки</param>
        /// <returns></returns>
        static bool FreeString(int l, int n)
        {
            bool free = true;
            for (int i = 0; i < Width; i++)
                if (M[l, i, n] > 0) free = false;
            return free;
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
            //"Путим" ячейку
            M[l, x, y] = c;
            //Далее обрабатываем автозаполнение
            if (AutoRule.Enable & l == AutoRule.Layer & Px[l].X == 1 & Px[l].Y == 1)
                foreach (AutoRule rule in AutoRules) if (rule.In(c)) M[0, x, y] = rule.Code;
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
