﻿using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SGen_Tiler
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Main : Game
    {
        const int LabelsSize = 22;
        enum Modes { Edit, SelectTool, SelectStamp, Tiling }
        enum EditModes { Layers, Carcase }
        GraphicsDeviceManager graphics;
        public static SpriteBatch spriteBatch;
        public static Texture2D WhitePixel; //Просто белый пиксель, используем для заливки областей
        int Cursorcolor = 0;    //И его цвет
        Texture2D Checkbox;
        Texture2D Cursor;
        Texture2D CursorMove;
        Texture2D Stamp;
        Texture2D SmallDigits;
        Texture2D WALL;
        Texture2D Karkas;
        Texture2D Background;
        Texture2D Front;
        Texture2D Tiling;
        SpriteFont Font;
        Modes Mode = Modes.Edit;
        EditModes EditMode = EditModes.Layers;
        bool MoveMode;  //Режим перемещения средней кнопкой
        Tool ToolL = new Tool();    //Инструмент для слоёв
        Tool ToolC = new Tool();    //Инструмент для каркаса
        int TimerLayers = 255; //Таймер на отображение титров
        int TimerLabels = 0;   //Таймер для подписей
        string Label;
        int LabelSize;
        byte TimerLayer = 0;    //Таймер на подсветку слоя
        int Wheel = 0;  //Последняя позиция колеса мышки (для вычисления инкремента)
        Vector2 OldMouse; //Последнее положение мышки (для скрола средней кнопкой)
        bool RightClickHold = false;    //Держится ли до сих пор правая кнопка (чтобы корректно менять режим)
        bool KeyHold;   //Зажата ли какая-нибудь клавиша
        bool ZHold;     //Зажат ли Z
        bool YHold;     //Зажат ли Y
        public bool Actived = true; //Так мы будем узнавать акнивно ли главное окно или нет, раз уш так получилось, то будем извращаться :-(
        FormMenu form = new FormMenu();
        byte StampNum = 0;  //Номер штампа, если 0 - значит не выбран
        byte ScrollSlowDown = 0;    //Замедлитель для скролла инструментов
        #region Инициализация
        public Main(string[] arg)
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.Title = System.Windows.Forms.Application.ProductName;
            if (Properties.Settings.Default.FirstStart)
            {
                System.Windows.Forms.MessageBox.Show("Похоже, программа запущена в первый раз.\n" +
                    "Нажмите Esc, что бы вызвать главное меню, " +
                    "в котором выберите файлы текстур и карту или создайте новую.", System.Windows.Forms.Application.ProductName);
                Properties.Settings.Default.FirstStart = false;
                Properties.Settings.Default.Save();        
            }

            Project.NewMap();
            if (arg.Length > 0)
            {
                Project.Load(arg[0]);
                ChangeWindowSize();
            }
            graphics.PreferredBackBufferWidth = Project.ScreenWidth;
            graphics.PreferredBackBufferHeight = Project.ScreenHeight;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            WhitePixel = new Texture2D(GraphicsDevice, 1, 1);
            Color[] color = { new Color(255, 255, 255, 255) };
            WhitePixel.SetData(color);
            Cursor = Content.Load<Texture2D>("Cursor");
            CursorMove = Content.Load<Texture2D>("Move");
            SmallDigits = Content.Load<Texture2D>("SmallDigits");
            Stamp = Content.Load<Texture2D>("Stamp");
            Tiling = Content.Load<Texture2D>("Tiling");
            Checkbox = Content.Load<Texture2D>("Checkbox");
            Font = Content.Load<SpriteFont>("Font");
            InitialTextures();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            IsMouseVisible = true;
        }
        #endregion

        #region update
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (!Actived) return;
            if (!IsActive) return;
            Tool t = ToolL;
            int l = Editor.Layer;
            if (EditMode == EditModes.Carcase) { t = ToolC; l = 0; }
            //Возвращаем мышку в полном экране, если она убежала за границу видимого
            if (graphics.IsFullScreen)
            {
                int x = Mouse.GetState().X;
                int y = Mouse.GetState().Y;
                bool ch = false;
                if (x >= Project.ScreenWidth) { x = Project.ScreenWidth - 1; ch = true; }
                if (y >= Project.ScreenHeight) { y = Project.ScreenHeight - 1; ch = true; }
                if (ch) Mouse.SetPosition(x, y);
            }
            //Вычисляем какая ячейка находится под мышкой
            int cx = (int)(Mouse.GetState().X + Editor.X * Project.Px[l].X) / Project.ScaledSize;
            int cy = (int)(Mouse.GetState().Y + Editor.Y * Project.Px[l].Y) / Project.ScaledSize;
            //Узнаём, свободна ли клавиатура, нажат ли какой-нибудь Alt, Ctrl Или Shift
            bool AltKey = Keyboard.GetState().IsKeyDown(Keys.LeftAlt) | Keyboard.GetState().IsKeyDown(Keys.RightAlt);
            bool ControlKey = Keyboard.GetState().IsKeyDown(Keys.LeftControl) | Keyboard.GetState().IsKeyDown(Keys.RightControl);
            bool ShiftKey = Keyboard.GetState().IsKeyDown(Keys.LeftShift) | Keyboard.GetState().IsKeyDown(Keys.RightShift);
            if (Keyboard.GetState().GetPressedKeys().Length == 0) KeyHold = false;
            if (Keyboard.GetState().IsKeyUp(Keys.Z)) ZHold = false;
            if (Keyboard.GetState().IsKeyUp(Keys.Y)) YHold = false;
            //Переключение в полный экран и обратно
            if (AltKey & Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                graphics.ToggleFullScreen();
                IsMouseVisible = !graphics.IsFullScreen;
                //Чтоб небыло нежелательного скролла помешаем мышку в центр
                if (graphics.IsFullScreen) Mouse.SetPosition(Project.ScreenWidth / 2, Project.ScreenHeight / 2);
            }
            //Обрабатываем анимацию
            if (Animation.Enable) foreach (Animation anim in Project.Animations) anim.Action();
            //Переход в главное меню
            if (Keyboard.GetState().IsKeyDown(Keys.Escape) & !KeyHold)
            {
                if (graphics.IsFullScreen)
                {
                    graphics.ToggleFullScreen();
                    IsMouseVisible = true;
                }
                KeyHold = true;
                Actived = false;
                TimerLayers = 0;
                form.ShowDialog(); Actived = true;
            }
            //Обработка режимов редактирования
            if (Mode == Modes.Edit)
            {
                //Движение мышкой средней кнопкой
                if (MoveMode) //Кнопка уже была нажата перемешаем экран на разницу мышки от старого положения
                {
                    Editor.X += (int)OldMouse.X - Mouse.GetState().Position.X;
                    Editor.Y += (int)OldMouse.Y - Mouse.GetState().Position.Y;
                }
                OldMouse = Mouse.GetState().Position.ToVector2();
                if (Mouse.GetState().MiddleButton == ButtonState.Pressed)
                {
                    MoveMode = true;
                    if (!graphics.IsFullScreen) IsMouseVisible = false;
                    //OldMouse = Mouse.GetState().Position.ToVector2();
                }
                else
                {
                    MoveMode = false;
                    if (!graphics.IsFullScreen) IsMouseVisible = true;
                }
                //Проверка на тыканье мышкой
                if (Mouse.GetState().LeftButton == ButtonState.Pressed & !MoveMode)
                {
                    for (int i = 0; i < t.Width; i++)
                        for (int j = 0; j < t.Height; j++)
                            Project.Put(l, cx + i, cy + j, t.M[i, j]);
                    Hystory.AddRecord();
                }
                int inc = 16;
                if (ControlKey) inc = 128;
                //Прочие способы скола, если средняя кнопка не нажата
                if (!MoveMode)
                {
                    //Движение карты клавишами
                    if (Keyboard.GetState().IsKeyUp(Keys.LeftShift) & Keyboard.GetState().IsKeyUp(Keys.RightShift) &
                        Keyboard.GetState().IsKeyDown(Keys.Right))
                        Editor.X += inc;
                    if (Keyboard.GetState().IsKeyUp(Keys.LeftShift) & Keyboard.GetState().IsKeyUp(Keys.RightShift) &
                        Keyboard.GetState().IsKeyDown(Keys.Left))
                        Editor.X -= inc;
                    if (Keyboard.GetState().IsKeyUp(Keys.LeftShift) & Keyboard.GetState().IsKeyUp(Keys.RightShift) &
                        Keyboard.GetState().IsKeyDown(Keys.Down))
                        Editor.Y += inc;
                    if (Keyboard.GetState().IsKeyUp(Keys.LeftShift) & Keyboard.GetState().IsKeyUp(Keys.RightShift) &
                        Keyboard.GetState().IsKeyDown(Keys.Up))
                        Editor.Y -= inc;
                    //Движение карты мышкой у края экрана
                    if (Mouse.GetState().Position.X < 3 & Mouse.GetState().Position.X > -20 &
                        Mouse.GetState().Position.Y >= 0 & Mouse.GetState().Position.Y <= Project.ScreenHeight)
                        Editor.X -= inc / 2;
                    if (Mouse.GetState().Position.Y < 3 & Mouse.GetState().Position.Y > -20 &
                        Mouse.GetState().Position.X >= 0 & Mouse.GetState().Position.X <= Project.ScreenWidth)
                        Editor.Y -= inc / 2;
                    if (Mouse.GetState().Position.X > Project.ScreenWidth - 3 & Mouse.GetState().Position.X < Project.ScreenWidth + 20 &
                        Mouse.GetState().Position.Y >= 0 & Mouse.GetState().Position.Y <= Project.ScreenHeight)
                        Editor.X += inc / 2;
                    if (Mouse.GetState().Position.Y > Project.ScreenHeight - 3 & Mouse.GetState().Position.Y < Project.ScreenHeight + 20 &
                        Mouse.GetState().Position.X >= 0 & Mouse.GetState().Position.X <= Project.ScreenWidth)
                        Editor.Y += inc / 2;
                }
                //Переход в режим выбора инструмента
                if (Mouse.GetState().RightButton != ButtonState.Pressed) RightClickHold = false;
                if (Mouse.GetState().RightButton == ButtonState.Pressed & !RightClickHold & !MoveMode)
                {
                    TimerLayers = 0;
                    RightClickHold = true;
                    Mode = Modes.SelectTool;
                    TimerLabels = 0;
                }
                //Переключение слоёв
                if (Keyboard.GetState().IsKeyDown(Keys.PageUp) & !KeyHold && Editor.Layer > 1)
                {
                    Editor.Layer--;
                    TimerLayer = 16;
                    PopUp();
                }
                if (Keyboard.GetState().IsKeyDown(Keys.PageDown) & !KeyHold && Editor.Layer < Project.Layers)
                {
                    Editor.Layer++;
                    TimerLayer = 16;
                    PopUp();
                }
                //Переключение видимости слоёв
                if (Keyboard.GetState().IsKeyDown(Keys.Enter) & !KeyHold)
                {
                    Editor.ShowOnlyCurrentLayer ^= true;
                    if (!Editor.ShowOnlyCurrentLayer) PopUp("Отображаются все слои", 320);
                    else PopUp("Отображается только текущий слой", 320);
                    TimerLayers = 255;
                    KeyHold = true;
                }
                //Включение/выключение каркаса
                if (Keyboard.GetState().IsKeyDown(Keys.Space) & !KeyHold)
                {
                    if (EditMode == EditModes.Layers) EditMode = EditModes.Carcase; else EditMode = EditModes.Layers;
                    PopUp();
                }
                //Переключение в режим выбора штампа
                if (AltKey & !KeyHold)
                {
                    Mode = Modes.SelectStamp;
                    PopUp("Выберите область для создания штампа", 360);
                    TimerLayers = 0;
                }
                //Переключение в режим тайлинга
                if (ShiftKey & !KeyHold)
                {
                    Mode = Modes.Tiling;
                    Select.Active = false;
                    PopUp("Режим тайлинга", 140);
                    TimerLayers = 0;
                }
                //Вызов штампа
                if (!ControlKey & EditMode == EditModes.Layers)
                {
                    bool load = false;
                    if (Keyboard.GetState().IsKeyDown(Keys.D1)) { StampNum = 1; t.CopyBy(Project.Stamps[1]); load = true; }
                    if (Keyboard.GetState().IsKeyDown(Keys.D2)) { StampNum = 2; t.CopyBy(Project.Stamps[2]); load = true; }
                    if (Keyboard.GetState().IsKeyDown(Keys.D3)) { StampNum = 3; t.CopyBy(Project.Stamps[3]); load = true; }
                    if (Keyboard.GetState().IsKeyDown(Keys.D4)) { StampNum = 4; t.CopyBy(Project.Stamps[4]); load = true; }
                    if (Keyboard.GetState().IsKeyDown(Keys.D5)) { StampNum = 5; t.CopyBy(Project.Stamps[5]); load = true; }
                    if (Keyboard.GetState().IsKeyDown(Keys.D6)) { StampNum = 6; t.CopyBy(Project.Stamps[6]); load = true; }
                    if (Keyboard.GetState().IsKeyDown(Keys.D7)) { StampNum = 7; t.CopyBy(Project.Stamps[7]); load = true; }
                    if (Keyboard.GetState().IsKeyDown(Keys.D8)) { StampNum = 8; t.CopyBy(Project.Stamps[8]); load = true; }
                    if (Keyboard.GetState().IsKeyDown(Keys.D9)) { StampNum = 9; t.CopyBy(Project.Stamps[9]); load = true; }
                    if (Keyboard.GetState().IsKeyDown(Keys.D0)) { StampNum = 0; t.Reset(); }
                    if (load)
                        if (StampNum > 0) PopUp("Выбран штамп " + StampNum, 150);
                        else PopUp("Инструмент сброшен", 200);
                }
                //Включение/выключение показа кодов
                if (Keyboard.GetState().IsKeyDown(Keys.C) & !KeyHold)
                {
                    Editor.Codes ^= true;
                    if (Editor.Codes) PopUp("Отображение кодов включено", 300);
                    else PopUp("Отображение кодов выключено", 300);
                    KeyHold = true;
                }
                //Включение/выключение анимации
                if (Keyboard.GetState().IsKeyDown(Keys.A) & !KeyHold)
                {
                    Animation.Enable ^= true;
                    if (Animation.Enable) PopUp("Анимация включена", 190);
                    else PopUp("Анимация выключена", 190);
                    KeyHold = true;
                }
                //Включение/выключение рандома
                if (Keyboard.GetState().IsKeyDown(Keys.R) & !KeyHold)
                {
                    RandomTile.Enable ^= true;                   
                    if (RandomTile.Enable) PopUp("Рандом включен", 160);
                    else PopUp("Рандом выключен", 160);
                    KeyHold = true;
                }
                //Включение/выключение автозаполнения
                if (Keyboard.GetState().IsKeyDown(Keys.K) & !KeyHold)
                {
                    AutoRule.Enable ^= true;
                    KeyHold = true;
                    if (AutoRule.Enable) PopUp("Автозаполнение включено", 250);
                    else PopUp("Автозаполнение выключено", 250);
                }
                //Переключение режима отображения фантомного слоя
                if (Keyboard.GetState().IsKeyDown(Keys.F) & !KeyHold)
                {
                    Editor.Phantom++;
                    if (Editor.Phantom > 2) Editor.Phantom = 0;
                    PopUp("Видимость фантомного слоя: " + Editor.Phantom * 50 + "%", 310);
                    KeyHold = true;
                }
                //Быстрый доступ к слоям
                if (Keyboard.GetState().IsKeyDown(Keys.NumPad0) & !KeyHold) { EditMode = EditModes.Carcase; PopUp(); }
                if (Keyboard.GetState().IsKeyDown(Keys.NumPad1) & !KeyHold)
                { EditMode = EditModes.Layers; Editor.Layer = 1; PopUp(); TimerLayer = 16; }
                if (Keyboard.GetState().IsKeyDown(Keys.NumPad2) & !KeyHold & Project.Layers >= 2)
                { EditMode = EditModes.Layers; Editor.Layer = 2; PopUp(); TimerLayer = 16; }
                if (Keyboard.GetState().IsKeyDown(Keys.NumPad3) & !KeyHold & Project.Layers >= 3)
                { EditMode = EditModes.Layers; Editor.Layer = 3; PopUp(); TimerLayer = 16; }
                if (Keyboard.GetState().IsKeyDown(Keys.NumPad4) & !KeyHold & Project.Layers >= 4)
                { EditMode = EditModes.Layers; Editor.Layer = 4; PopUp(); TimerLayer = 16; }
                if (Keyboard.GetState().IsKeyDown(Keys.NumPad5) & !KeyHold & Project.Layers >= 5)
                { EditMode = EditModes.Layers; Editor.Layer = 5; PopUp(); TimerLayer = 16; }
                if (Keyboard.GetState().IsKeyDown(Keys.NumPad6) & !KeyHold & Project.Layers >= 6)
                { EditMode = EditModes.Layers; Editor.Layer = 6; PopUp(); TimerLayer = 16; }
                if (Keyboard.GetState().IsKeyDown(Keys.NumPad7) & !KeyHold & Project.Layers >= 7)
                { EditMode = EditModes.Layers; Editor.Layer = 7; PopUp(); TimerLayer = 16; }
                if (Keyboard.GetState().IsKeyDown(Keys.NumPad8) & !KeyHold & Project.Layers >= 8)
                { EditMode = EditModes.Layers; Editor.Layer = 8; PopUp(); TimerLayer = 16; }
                //Изменением масштаба
                float oldscale = Editor.Scale;
                float x = (Mouse.GetState().X + Editor.X) / Project.ScaledSize;
                float y = (Mouse.GetState().Y + Editor.Y) / Project.ScaledSize;
                if ((Wheel - Mouse.GetState().ScrollWheelValue > 0 | Keyboard.GetState().IsKeyDown(Keys.Subtract)) && Editor.Scale>0.1f) Editor.Scale -= 0.1f;
                if ((Wheel - Mouse.GetState().ScrollWheelValue < 0 | Keyboard.GetState().IsKeyDown(Keys.Add)) && Editor.Scale<2) Editor.Scale += 0.1f;
                if (Keyboard.GetState().IsKeyDown(Keys.Multiply)) Editor.Scale = 1;
                if (oldscale != Editor.Scale)
                {
                    PopUp("Масштаб " + (Editor.Scale).ToString("0%"), 130);
                    Editor.X = (int)x * Project.ScaledSize - Mouse.GetState().X;
                    Editor.Y = (int)y * Project.ScaledSize - Mouse.GetState().Y;
                }
                //Корректируем позицию камеры на карте
                if (Editor.X > Project.Width * Project.ScaledSize - Project.ScreenWidth)
                    Editor.X = Project.Width * Project.ScaledSize - Project.ScreenWidth;
                if (Editor.X < 0) Editor.X = 0;
                if (Editor.Y > Project.Height * Project.ScaledSize - Project.ScreenHeight)
                    Editor.Y = Project.Height * Project.ScaledSize - Project.ScreenHeight;
                if (Editor.Y < 0) Editor.Y = 0;
                //Сохранение штампа
                if (ControlKey)
                {
                    bool sav = false;
                    if (Keyboard.GetState().IsKeyDown(Keys.D1)) { StampNum = 1; Project.Stamps[1].CopyBy(t); sav = true; }
                    if (Keyboard.GetState().IsKeyDown(Keys.D2)) { StampNum = 2; Project.Stamps[2].CopyBy(t); sav = true; }
                    if (Keyboard.GetState().IsKeyDown(Keys.D3)) { StampNum = 3; Project.Stamps[3].CopyBy(t); sav = true; }
                    if (Keyboard.GetState().IsKeyDown(Keys.D4)) { StampNum = 4; Project.Stamps[4].CopyBy(t); sav = true; }
                    if (Keyboard.GetState().IsKeyDown(Keys.D5)) { StampNum = 5; Project.Stamps[5].CopyBy(t); sav = true; }
                    if (Keyboard.GetState().IsKeyDown(Keys.D6)) { StampNum = 6; Project.Stamps[6].CopyBy(t); sav = true; }
                    if (Keyboard.GetState().IsKeyDown(Keys.D7)) { StampNum = 7; Project.Stamps[7].CopyBy(t); sav = true; }
                    if (Keyboard.GetState().IsKeyDown(Keys.D8)) { StampNum = 8; Project.Stamps[8].CopyBy(t); sav = true; }
                    if (Keyboard.GetState().IsKeyDown(Keys.D9)) { StampNum = 9; Project.Stamps[9].CopyBy(t); sav = true; }
                    if (sav) PopUp("Штамп сохранен в ячейку " + StampNum, 240);
                }
                //Правка
                if (ControlKey & Keyboard.GetState().IsKeyDown(Keys.Z) & !ZHold) { Hystory.Undo(); ZHold = true; }
                if (ControlKey & Keyboard.GetState().IsKeyDown(Keys.Y) & !YHold) { Hystory.Redo(); YHold = true; } 
            }
            //Режим выбора инструмента
            if (Mode == Modes.SelectTool) //Тут только для выбора большого инструмента
            {
                Select.End(Tool.Xcalc(Mouse.GetState().X), Mouse.GetState().Y / Project.TileSize);
                //Выход из режима выбора спрайта
                if (Mouse.GetState().RightButton != ButtonState.Pressed) RightClickHold = false;
                if (Mouse.GetState().RightButton == ButtonState.Pressed & !RightClickHold) { RightClickHold = true; Mode = Modes.Edit; }
                if (Mouse.GetState().LeftButton == ButtonState.Pressed & !Select.Active)
                    Select.Start(Tool.Xcalc(Mouse.GetState().X), Mouse.GetState().Y / Project.TileSize);
                if (Mouse.GetState().LeftButton != ButtonState.Pressed & Select.Active)
                {
                    //Высчитываем выделение
                    t.Width = Select.Width;
                    t.Height = Select.Height;
                    for (int i = 0; i < Select.Width; i++)
                        for (int j = 0; j < Select.Height; j++)
                            t.M[i, j] = (ushort)(Select.Left + i + (Select.Top + j + t.Scroll) * (Project.ScreenWidth / Project.TileSize));
                    Select.Active = false;
                    Mode = Modes.Edit;
                    StampNum = 0;
                }
                //Прокрутка
                t.Scroll += (Wheel - Mouse.GetState().ScrollWheelValue) / 40;
                if (Mouse.GetState().Position.Y < 3 & Mouse.GetState().Position.Y > -20 &
                    Mouse.GetState().Position.X >= 0 & Mouse.GetState().Position.X <= Project.ScreenWidth)
                {
                    ScrollSlowDown++;
                    if (ScrollSlowDown > 5)
                    {
                        t.Scroll--;
                        ScrollSlowDown = 0;
                    }
                }
                if (Mouse.GetState().Position.Y > Project.ScreenHeight - 3 & Mouse.GetState().Position.Y < Project.ScreenHeight + 20 &
                    Mouse.GetState().Position.X >= 0 & Mouse.GetState().Position.X <= Project.ScreenWidth)
                {
                    ScrollSlowDown++;
                    if (ScrollSlowDown > 5)
                    {
                        t.Scroll++;
                        ScrollSlowDown = 0;
                    }
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Up)) t.Scroll--;
                if (Keyboard.GetState().IsKeyDown(Keys.Down)) t.Scroll++;
                //Скорректируем, если надо курсор
                int Rows = SpritesCount() / (Project.ScreenWidth / Project.TileSize);
                if (t.Scroll > Rows - (Project.ScreenHeight / Project.TileSize)) t.Scroll = Rows - (Project.ScreenHeight / Project.TileSize);
                if (t.Scroll < 0) t.Scroll = 0;
                //Смена фона
                if (Keyboard.GetState().IsKeyDown(Keys.Space) & !KeyHold)
                {
                    KeyHold = true;
                    Project.ToolBackground++;
                    if (Project.ToolBackground > 2) Project.ToolBackground = 0;
                    Project.Saved = false;
                }
            }
            //Режим выбора штампа
            if (Mode == Modes.SelectStamp)
            {
                Select.End(cx, cy);
                if (!AltKey) Mode = Modes.Edit;
                if (Mouse.GetState().LeftButton == ButtonState.Pressed & !Select.Active) Select.Start(cx, cy);
                if (Mouse.GetState().LeftButton != ButtonState.Pressed & Select.Active)
                {
                    Select.End(cx, cy);
                    //Создаём штамп с имеющейся карты
                    t.Width = Select.Width;
                    t.Height = Select.Height;
                    for (int i = 0; i < t.Width; i++)
                        for (int j = 0; j < t.Height; j++)
                            t.M[i, j] = Project.Get(l, Select.Left + i, Select.Top + j);
                    Select.Active = false;
                    Mode = Modes.Edit;
                    KeyHold = true;
                    StampNum = 0;
                }
            }
            //Режим тайлинга
            if (Mode == Modes.Tiling)
            {
                Select.End(cx, cy);
                if (!Keyboard.GetState().IsKeyDown(Keys.LeftShift) & !Keyboard.GetState().IsKeyDown(Keys.RightShift)) Mode = Modes.Edit;
                if (Mouse.GetState().LeftButton == ButtonState.Pressed & !Select.Active) Select.Start(cx, cy);
                if (Mouse.GetState().LeftButton != ButtonState.Pressed & Select.Active)
                {
                    Select.End(cx, cy);
                    //Тайлим выделенную область
                    int ti = 0;
                    for (int i = 0; i < Select.Width; i++)
                    {
                        int tj = 0;
                        for (int j = 0; j < Select.Height; j++)
                        {
                            Project.Put(l, Select.Left + i, Select.Top + j, t.M[ti, tj]);
                            tj++; if (tj >= t.Height) tj = 0;
                        }
                        ti++; if (ti >= t.Width) ti = 0;
                    }
                    Select.Active = false;
                    KeyHold = true;
                    Hystory.AddRecord();
                }
            }
            base.Update(gameTime);
            Wheel = Mouse.GetState().ScrollWheelValue; //Делаем это независимо от режима, а то получается что прокрутка работает даже при другом режиме
        }
        #endregion

        #region draw
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            int al = Cursorcolor + 64;
            if (Cursorcolor > 127) al = 319 - Cursorcolor;
            Tool t = ToolL;
            int l = Editor.Layer;
            Texture2D tex = WALL;
            Color col = Color.FromNonPremultiplied(0, 128, 255, al);
            if (EditMode == EditModes.Carcase)
            {
                t = ToolC;
                l = 0;
                tex = Karkas;
                col = Color.FromNonPremultiplied(128, 128, 128, al);
            }
            int s = Project.ScaledSize;
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            if (Mode == Modes.Edit)
            {
                DrawLayers();
                //Курсор
                float kx = Editor.X * Project.Px[l].X;
                float ky = Editor.Y * Project.Px[l].Y;
                if (!MoveMode)
                {
                    int x = (int)(Mouse.GetState().X + kx) / s * s - (int)(kx);
                    int y = (int)(Mouse.GetState().Y + ky) / s * s - (int)(ky);
                    spriteBatch.Draw(WhitePixel, new Rectangle(x, y, t.Width * s, t.Height * s), new Rectangle(0, 0, 1, 1), col);
                    //Прозрачно рисуем инструмент на курсоре
                    for (int i = 0; i < t.Width; i++)
                        for (int j = 0; j < t.Height; j++)
                            spriteBatch.Draw(tex, new Rectangle(x + i * s, y + j * s, s, s),
                                SpriteByNum(tex, t.M[i, j], false), Color.FromNonPremultiplied(255, 255, 255, al));
                    //Код штампа, если используется
                    if (StampNum > 0) DrawSmallNum(x, y, StampNum, Color.White);
                }
                //Табличка слоёв
                if (TimerLayers > 1)
                {
                    al = 255;
                    if (TimerLayers < 128) al = (byte)(TimerLayers * 2); //Это даёт красивый эффект, затухание происходит не линейно, а с паузой. Круть :-)
                    spriteBatch.Draw(WhitePixel, new Rectangle(0, 0, 90, 20 * Project.Layers + 10), Color.FromNonPremultiplied(0, 0, 0, al / 2));
                    for (int i = 0; i < Project.Layers; i++)
                    {
                        if (!Editor.ShowOnlyCurrentLayer | i + 1 == l)
                            spriteBatch.Draw(Checkbox, new Vector2(0, i * 20), new Rectangle(0, 0, 20, 20), Color.FromNonPremultiplied(255, 255, 255, al));
                        else
                            spriteBatch.Draw(Checkbox, new Vector2(0, i * 20), new Rectangle(0, 20, 20, 20), Color.FromNonPremultiplied(255, 255, 255, al));
                        byte br = 92;
                        if (i + 1 == l & EditMode != EditModes.Carcase) br = 255;
                        spriteBatch.DrawString(Font,"Слой " + (i + 1),new Vector2(20,i*20), Color.FromNonPremultiplied(br, br, 255, al));
                    }
                    TimerLayers -= 4;
                }
                if (!MoveMode) InfoPanel(l, (int)(Mouse.GetState().X + kx) / s, (int)(Mouse.GetState().Y + ky) / s);
            }
            //Режим выбора инструментов
            if (Mode == Modes.SelectTool)
            {
                if (Project.ToolBackground == 1) GraphicsDevice.Clear(Color.FromNonPremultiplied(0, 30, 50, 255));
                if (Project.ToolBackground == 2 && Background != null)
                    spriteBatch.Draw(Background, new Rectangle(0, 0, Project.ScreenWidth, Project.ScreenHeight),
                        new Rectangle(0, 0, Background.Width, Background.Height), Color.White);
                //Рисуем сетку инструментов
                int n = t.Scroll * (Project.ScreenWidth / Project.TileSize);
                for (int j = 0; j < Project.ScreenHeight / Project.TileSize; j++)
                    for (int i = 0; i < Project.ScreenWidth / Project.TileSize; i++)
                    {
                        if (n < SpritesCount())
                        {
                            spriteBatch.Draw(tex, new Vector2(i * Project.TileSize, j * Project.TileSize), SpriteByNum(tex, n, false), Color.White);
                            DrawSmallNum(i * Project.TileSize, j * Project.TileSize, n, Color.White);
                        }
                        n++;
                    }
                spriteBatch.Draw(WhitePixel, new Rectangle(Select.Left * Project.TileSize, Select.Top * Project.TileSize,
                    Select.Width * Project.TileSize, Select.Height * Project.TileSize), new Rectangle(0, 32, 32, 32), col);
            }
            if (Mode == Modes.SelectStamp)
            {
                DrawLayers();
                spriteBatch.End();
                //Курсор
                if (!MoveMode)
                {
                    float kx = Select.Left * s - Editor.X * Project.Px[l].X;
                    float ky = Select.Top * s - Editor.Y * Project.Px[l].Y;
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearWrap,
                        DepthStencilState.Default, RasterizerState.CullNone);
                    spriteBatch.Draw(Stamp, new Vector2(kx, ky),
                        new Rectangle(0, 0, Select.Width * s, Select.Height * s),
                        col, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    spriteBatch.End();
                    spriteBatch.Begin();
                }
                InfoPanel(l, Select.Left, Select.Top, Select.Width, Select.Height);
            }
            if (Mode == Modes.Tiling)
            {
                DrawLayers();
                spriteBatch.End();
                //Курсор
                if (!MoveMode)
                {
                    float kx = Select.Left * s - Editor.X * Project.Px[l].X;
                    float ky = Select.Top * s - Editor.Y * Project.Px[l].Y;
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearWrap,
                        DepthStencilState.Default, RasterizerState.CullNone);
                    spriteBatch.Draw(Tiling, new Vector2(kx, ky),
                        new Rectangle(0, 0, Select.Width * s, Select.Height * s),
                        col, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    spriteBatch.End();
                    spriteBatch.Begin();
                    InfoPanel(l, Select.Left, Select.Top, Select.Width, Select.Height);
                }
            }
            //Поп-ап сообщение
            DrawPopUp();
            //Указатель
            if (graphics.IsFullScreen & !MoveMode) spriteBatch.Draw(Cursor, Mouse.GetState().Position.ToVector2(), Color.White);
            //Курсор перемещений
            if (MoveMode)
                spriteBatch.Draw(CursorMove, Mouse.GetState().Position.ToVector2() - new Vector2(12, 12), Color.White);
            //Мерцание курсора
            Cursorcolor += 8;
            if (Cursorcolor > 255) Cursorcolor = 0;
            spriteBatch.End();
            base.Draw(gameTime);
        }
        #endregion

        #region Всякое разное
        /// <summary>
        /// Панель состояния
        /// </summary>
        void InfoPanel(int l, int x, int y, int xx, int yy)
        {
            spriteBatch.Draw(WhitePixel, new Rectangle(0, Project.ScreenHeight - LabelsSize, 300, LabelsSize), Color.FromNonPremultiplied(0, 0, 0, 128));
            spriteBatch.DrawString(Font, l.ToString("0 x ") + x.ToString("0 x ") + y.ToString(),
                new Vector2(0, Project.ScreenHeight - 20), Color.White);
            spriteBatch.DrawString(Font, "Область: " + xx.ToString("0 x ") + yy.ToString(),
                new Vector2(150, Project.ScreenHeight - 20), Color.White);
        }

        /// <summary>
        /// Панель состояния
        /// </summary>
        void InfoPanel(int l, int x, int y)
        {
            spriteBatch.Draw(WhitePixel, new Rectangle(0, Project.ScreenHeight - LabelsSize, 150, LabelsSize), Color.FromNonPremultiplied(0, 0, 0, 128));
            spriteBatch.DrawString(Font, l.ToString("0 x ") + x.ToString("0 x ") +y.ToString(),
                new Vector2(0, Project.ScreenHeight - 20), Color.White);
        }

        /// <summary>
        /// Новое поп-ап сообщение сообщением об слое
        /// </summary>
        void PopUp()
        {
            if (EditMode == EditModes.Layers) PopUp("Выбран слой " + Editor.Layer, 130);
            else PopUp("Выбран каркас ", 130);
            TimerLayers = 255;
            KeyHold = true;
        }

        /// <summary>
        /// Новое поп-ап сообщение
        /// </summary>
        /// <param name="label"></param>
        /// <param name="width"></param>
        void PopUp(string label, int width)
        {
            Label = label;
            LabelSize = width;
            TimerLabels = 255;
        }

        void DrawPopUp()
        {
            if (TimerLabels > 0)
            {
                int br = 255;
                if (TimerLabels < 128) br = TimerLabels * 2;
                spriteBatch.Draw(WhitePixel, new Rectangle(Project.ScreenWidth / 2 - LabelSize / 2 - 15, 0, LabelSize + 30, LabelsSize),
                    Color.FromNonPremultiplied(0, 0, 0, br/2));
                spriteBatch.DrawString(Font, Label, new Vector2(Project.ScreenWidth / 2 - LabelSize / 2, 0),
                    Color.FromNonPremultiplied(255, 255, 255, br));
                TimerLabels -= 4;
            }
        }

        /// <summary>
        /// Рисование слоёв
        /// </summary>
        void DrawLayers()
        {
            int s = Project.ScaledSize;
            //Рисуем задник
            if (!Editor.ShowOnlyCurrentLayer && Background != null)
                spriteBatch.Draw(Background, new Rectangle(0, 0, Project.ScreenWidth, Project.ScreenHeight),
                    new Rectangle(0, 0, Background.Width, Background.Height), Color.White);
            //Рисуем видимые слои текстурные
            for (int l = 1; l <= Project.Layers; l++)
            {
                if (!Editor.ShowOnlyCurrentLayer | (Editor.ShowOnlyCurrentLayer & l == Editor.Layer))
                {
                    Color col = Color.White;
                    if (l == Project.Phantom && !Editor.ShowOnlyCurrentLayer & EditMode == EditModes.Layers)
                        col = new Color(Editor.Phantom * 128, Editor.Phantom * 128, Editor.Phantom * 128, Editor.Phantom * 128);
                    if (l == Editor.Layer && !Editor.ShowOnlyCurrentLayer & EditMode == EditModes.Layers & TimerLayer > 0)
                    {
                        TimerLayer--;
                        col = new Color(256 - TimerLayer * 16, 256 - TimerLayer * 16, 256 - TimerLayer * 16, 256 - TimerLayer * 16);
                    }
                    //Вывод слоя
                    int kamx = (int)(Editor.X * Project.Px[l].X); //Вычисление камеры для конкретного слоя с учётом коэффициентов смещения
                    int kamy = (int)(Editor.Y * Project.Px[l].Y);
                    int leftTile = kamx / Project.ScaledSize;
                    int topTile = kamy / Project.ScaledSize;
                    int rightTile = leftTile + Project.ScreenWidth / s + 1;
                    int bottomTile = topTile + Project.ScreenHeight / s + 1;
                    for (int i = leftTile; i <= rightTile; i++)
                    {
                        for (int j = topTile; j <= bottomTile; j++)
                        {
                            int x = i * s - kamx;
                            int y = j * s - kamy;
                            if (i < Project.MaxWidth & j < Project.MaxHeight && Project.M[l, i, j] > 0)
                            {
                                spriteBatch.Draw(WALL, new Rectangle(x, y, s, s), SpriteByNum(WALL, Project.M[l, i, j], true), col);
                                if (l == Editor.Layer & EditMode == EditModes.Layers & Editor.Codes) DrawSmallNum(x, y, Project.M[l, i, j], col);
                            }
                        }
                    }
                }
            }
            //Рисуем передник
            if (!Editor.ShowOnlyCurrentLayer && Front != null)
                spriteBatch.Draw(Front, new Rectangle(0, 0, Project.ScreenWidth, Project.ScreenHeight),
                    new Rectangle(0, 0, Front.Width, Front.Height), Color.White);
            //Затем рисуем, если видно каркасный слой
            if (EditMode == EditModes.Carcase)
            {
                int leftTile = Editor.X / s;
                int topTile = Editor.Y / s;
                int rightTile = leftTile + Project.ScreenWidth / s + 1;
                int bottomTile = topTile + Project.ScreenHeight / s + 1;
                for (int i = leftTile; i <= rightTile; i++)
                    for (int j = topTile; j <= bottomTile; j++)
                    {
                        int x = i * Project.ScaledSize - Editor.X;
                        int y = j * Project.ScaledSize - Editor.Y;
                        if (i < Project.MaxWidth & j < Project.MaxHeight)
                            spriteBatch.Draw(Karkas, new Rectangle(x, y, s, s), SpriteByNum(Karkas, Project.M[0, i, j], false), Color.White);
                        if (EditMode == EditModes.Carcase & Editor.Codes) DrawSmallNum(x, y, Project.M[0, i, j], Color.White);
                    }
            }
        }

        /// <summary>
        /// Вычисление количества спрайт в текстуре
        /// </summary>
        /// <returns></returns>
        public int SpritesCount()
        {
            if (EditMode==EditModes.Layers) return (WALL.Width / Project.TileSize) * (WALL.Height / Project.TileSize);
            else return (Karkas.Width / Project.TileSize) * (Karkas.Height / Project.TileSize);
        }

        /// <summary>
        /// Рисование маленького числа
        /// </summary>
        /// <param name="x">Координата X</param>
        /// <param name="y">Координата Y</param>
        /// <param name="num">Число</param>
        void DrawSmallNum(int x, int y, int num, Color color)
        {
            string str = num.ToString();
            for (int i = 0; i < str.Length; i++)
                spriteBatch.Draw(SmallDigits, new Vector2(x + i * 6, y), new Rectangle((str[i] - 48) * 6, 0, 6, 8), color);
        }

        /// <summary>
        /// Изменение размеров экрана
        /// </summary>
        public void ChangeWindowSize()
        {
            graphics.PreferredBackBufferWidth = Project.ScreenWidth;
            graphics.PreferredBackBufferHeight = Project.ScreenHeight;
            graphics.ApplyChanges();
        }

        /// <summary>
        /// Загрузка текстур
        /// </summary>
        public void InitialTextures()
        {
            //Тайлы
            try
            {
                FileStream file = new FileStream(TextureFile(Project.FileTexture), FileMode.Open);
                WALL = Texture2D.FromStream(GraphicsDevice, file);
                file.Dispose(); //Освобождаем файл вручную, ибо иначе его больше нельзя будет выбрать в этом сеансе (тупо, чё сказать...)
            }
            catch { WALL = new Texture2D(GraphicsDevice, 640, 480); }
            //Каркас
            try
            {
                FileStream file = new FileStream(TextureFile(Project.FileKarkas), FileMode.Open);
                Karkas = Texture2D.FromStream(GraphicsDevice, file);
                file.Dispose(); //Освобождаем файл вручную, ибо иначе его больше нельзя будет выбрать в этом сеансе (тупо, чё сказать...)
            }
            catch { Karkas = new Texture2D(GraphicsDevice, 640, 480); }
            //Задник
            try
            {
                FileStream file = new FileStream(TextureFile(Project.FileBackground), FileMode.Open);
                Background = Texture2D.FromStream(GraphicsDevice, file);
                file.Dispose(); //Освобождаем файл вручную, ибо иначе его больше нельзя будет выбрать в этом сеансе (тупо, чё сказать...)
            }
            catch { }
            //Передник
            try
            {
                FileStream file = new FileStream(TextureFile(Project.FileFront), FileMode.Open);
                Front = Texture2D.FromStream(GraphicsDevice, file);
                file.Dispose(); //Освобождаем файл вручную, ибо иначе его больше нельзя будет выбрать в этом сеансе (тупо, чё сказать...)
            }
            catch { }
        }

        /// <summary>
        /// Подготовка имени файла для текстуры (если файл не имеет пути, ему добавляется путь файла карты)
        /// </summary>
        /// <param name="file">Файл (ваш КЭП)</param>
        /// <returns></returns>
        string TextureFile(string file)
        {
            if (file == "") return "";
            if (Path.GetDirectoryName(file) == "") return Path.GetDirectoryName(Project.FileName) + "\\" + file;
            else return file;
        }

        /// <summary>
        /// Очень важная функция, вырезающая нужный квадратик из текстуры
        /// </summary>
        /// <param name="texture">Текстура</param>
        /// <param name="Num">Номер спрайта</param>
        /// <param name="withAnimation">Применять ли анимацию к этому тайлу</param>
        /// <returns></returns>
        Rectangle SpriteByNum(Texture2D texture, int Num, bool withAnimation)
        {
            if (withAnimation & Animation.Enable)
            {
                int index = Project.Animations.FindIndex(Animation => Animation.Included(Num));
                if (index >= 0) Num = Project.Animations[index].GetFrame(Num);
            }
            //Определяем количество спрайтов по горизонтали
            int WSprites = texture.Width / Project.TileSize;
            //находим строку
            int str = Num / WSprites;
            //Находим колонку
            int tab = Num % WSprites;
            return new Rectangle(tab * Project.TileSize, str * Project.TileSize, Project.TileSize, Project.TileSize);
        }
        #endregion
    }
}
