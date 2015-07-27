using System.Collections.Generic;
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
        enum Modes { Edit, SelectTool, SelectStamp, Tiling }
        enum EditModes { Layers, Carcase }
        GraphicsDeviceManager graphics;
        public static SpriteBatch spriteBatch;
        public static Texture2D WhitePixel; //Просто белый пиксель, используем для заливки областей
        public static Texture2D ControlPanel; //Текстура панели управления
        int Cursorcolor = 0;    //И его цвет
        Texture2D Checkbox;
        Texture2D Cursor;
        Texture2D SmallDigits;
        Texture2D WALL;
        Texture2D Karkas;
        Texture2D Labels;
        Texture2D LabelStamp;
        Texture2D LabelTiling;
        Texture2D Tiling;
        Modes Mode = Modes.Edit;
        EditModes EditMode = EditModes.Layers;
        Tool ToolL = new Tool();    //Инструмент для слоёв
        Tool ToolC = new Tool();    //Инструмент для каркаса
        byte TimerTitles = 255; //Таймер на отображение титров
        byte TimerLayer = 0;    //Таймер на подсветку слоя
        int Wheel = 0;  //Последняя позиция колеса мышки (для вычисления инкремента)
        //bool MouseHold = false; //Зажатие кнопки для выбора большого инструмента (и не только)
        bool RightClickHold = false;    //Держится ли до сих пор правая кнопка (чтобы корректно менять режим)
        bool KeyHold = true;    //В релизе сделать false, чтобы окно настроек не появлялось сразу же, хотя надо ещё подумать как удобней
        public bool Actived = true; //Так мы будем узнавать акнивно ли главное окно или нет, раз уш так получилось, то будем извращаться :-(
        FormMenu form = new FormMenu();

        #region Инициализация
        public Main(string[] arg)
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.Title = Program.Name;
            graphics.PreferredBackBufferWidth = Project.ScreenWidth;
            graphics.PreferredBackBufferHeight = Project.ScreenHeight;
            Config.Load();
            Project.NewMap();

            //arg = new string[0];
            if (arg.Length > 0)
            {
                Project.Load(arg[0]);
                ChangeWindowSize();
            }
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
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            WhitePixel = Content.Load<Texture2D>("WhitePixel");
            WhitePixel = new Texture2D(GraphicsDevice, 1, 1);
            Color[] color = { new Color(255, 255, 255, 255) };
            WhitePixel.SetData<Color>(color);
            ControlPanel = Content.Load<Texture2D>("ControlPanel");
            SmallDigits = Content.Load<Texture2D>("SmallDigits");
            Cursor = Content.Load<Texture2D>("Cursor");
            Labels = Content.Load<Texture2D>("Labels");
            LabelStamp = Content.Load<Texture2D>("LabelStamp");
            LabelTiling = Content.Load<Texture2D>("LabelTiling");
            Tiling = Content.Load<Texture2D>("Tiling");
            Checkbox = Content.Load<Texture2D>("Checkbox");
            InitialTextures();
            if (Project.FileName == "")
            {
                form.ShowDialog();
                KeyHold = true; //Для того, что бы не вызывать меню повторно когда нажат ESC для выхода из него
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
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
            //Вычисляем какая ячейка находится под мышкой
            int cx = (int)(Mouse.GetState().X + Editor.X * Project.Px[l].X) / Project.TileSize;
            int cy = (int)(Mouse.GetState().Y + Editor.Y * Project.Px[l].Y) / Project.TileSize;
            
            if (Keyboard.GetState().GetPressedKeys().Length == 0) KeyHold = false;
            if ((Mouse.GetState().LeftButton == ButtonState.Pressed | Mouse.GetState().RightButton == ButtonState.Pressed) & Project.FileName == "")
            {
                System.Windows.Forms.MessageBox.Show("Для начала откройте или создайте новую карту.", Program.Name);
                form.ShowDialog();
                KeyHold = true; //Для того, что бы не вызывать меню повторно когда нажат ESC для выхода из него
                return;
            }
            //Обработка режимов редактирования
            if (Mode == Modes.Edit)
            {
                //Проверка на тыканье мышкой
                if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    for (int i = 0; i < t.Width; i++)
                        for (int j = 0; j < t.Height; j++)
                            Project.Put(l, cx + i, cy + j, t.M[i, j]);
                }
                //Движение карты клавишами
                if (Keyboard.GetState().IsKeyUp(Keys.LeftShift) & Keyboard.GetState().IsKeyUp(Keys.RightShift) & Keyboard.GetState().IsKeyDown(Keys.Right))
                    Editor.X += Project.TileSize;
                if (Keyboard.GetState().IsKeyUp(Keys.LeftShift) & Keyboard.GetState().IsKeyUp(Keys.RightShift) & Keyboard.GetState().IsKeyDown(Keys.Left))
                    Editor.X -= Project.TileSize;
                if (Keyboard.GetState().IsKeyUp(Keys.LeftShift) & Keyboard.GetState().IsKeyUp(Keys.RightShift) & Keyboard.GetState().IsKeyDown(Keys.Down))
                    Editor.Y += Project.TileSize;
                if (Keyboard.GetState().IsKeyUp(Keys.LeftShift) & Keyboard.GetState().IsKeyUp(Keys.RightShift) & Keyboard.GetState().IsKeyDown(Keys.Up))
                    Editor.Y -= Project.TileSize;
                //Корректируем позицию камеры на карте
                if (Editor.X > Project.Width * Project.TileSize - Project.ScreenWidth)
                    Editor.X = Project.Width * Project.TileSize - Project.ScreenWidth;
                if (Editor.X < 0) Editor.X = 0;
                if (Editor.Y > Project.Height * Project.TileSize - Project.ScreenHeight)
                    Editor.Y = Project.Height * Project.TileSize - Project.ScreenHeight;
                if (Editor.Y < 0) Editor.Y = 0;
                //Переход в режим выбора спрайтов
                if (Mouse.GetState().RightButton != ButtonState.Pressed) RightClickHold = false;
                if (Mouse.GetState().RightButton == ButtonState.Pressed & !RightClickHold)
                {
                    RightClickHold = true;
                    Mode = Modes.SelectTool;
                }
                //Переход в главное меню
                if (Keyboard.GetState().IsKeyDown(Keys.Escape) & !KeyHold)
                {
                    KeyHold = true;
                    Actived = false;
                    try { form.ShowDialog(); Actived = true; }
                    catch { }
                }
                //Переключение слоёв
                if (Keyboard.GetState().IsKeyDown(Keys.PageUp) & !KeyHold && Editor.Layer > 1)
                {
                    Editor.Layer--;
                    KeyHold = true;
                    TimerTitles = 255;
                    TimerLayer = 16;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.PageDown) & !KeyHold && Editor.Layer < Project.Layers)
                {
                    Editor.Layer++;
                    KeyHold = true;
                    TimerTitles = 255;
                    TimerLayer = 16;
                }
                //Переключение видимости слоёв
                if (Keyboard.GetState().IsKeyDown(Keys.Enter) & !KeyHold)
                {
                    Editor.ShowOnlyCurrentLayer ^= true;
                    KeyHold = true;
                    TimerTitles = 255;
                }
                //Включение/выключение каркаса
                if (Keyboard.GetState().IsKeyDown(Keys.Space) & !KeyHold)
                {
                    if (EditMode == EditModes.Layers) EditMode = EditModes.Carcase; else EditMode = EditModes.Layers;
                    KeyHold = true;
                    TimerTitles = 255;
                }
                //Включение/выключение показа кодов
                if (Keyboard.GetState().IsKeyDown(Keys.C) & !KeyHold) { Editor.Codes ^= true; KeyHold = true; }
                //Переключение в режим выбора штампа
                if ((Keyboard.GetState().IsKeyDown(Keys.LeftControl) | Keyboard.GetState().IsKeyDown(Keys.RightControl)) & !KeyHold) Mode = Modes.SelectStamp;
                //Переключение в режим тайлинга
                if ((Keyboard.GetState().IsKeyDown(Keys.LeftShift) | Keyboard.GetState().IsKeyDown(Keys.RightShift)) & !KeyHold)
                { Mode = Modes.Tiling; Select.Active = false; }
            }
            if (Mode == Modes.SelectTool) //Тут только для выбора большого инструмента
            {
                Select.End(Mouse.GetState().X / Project.TileSize, Mouse.GetState().Y / Project.TileSize);
                //Выход из режима выбора спрайта
                if (Mouse.GetState().RightButton != ButtonState.Pressed) RightClickHold = false;
                if (Mouse.GetState().RightButton == ButtonState.Pressed & !RightClickHold) { RightClickHold = true; Mode = Modes.Edit; }
                if (Mouse.GetState().LeftButton == ButtonState.Pressed & !Select.Active)
                    Select.Start(Mouse.GetState().X / Project.TileSize, Mouse.GetState().Y / Project.TileSize);
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
                }
                //Прокрутка
                t.Scroll += (Wheel - Mouse.GetState().ScrollWheelValue) / 120;
            }
            if (Mode == Modes.SelectStamp)
            {
                Select.End(cx, cy);
                if (!Keyboard.GetState().IsKeyDown(Keys.LeftControl) & !Keyboard.GetState().IsKeyDown(Keys.RightControl)) Mode = Modes.Edit;
                if (Mouse.GetState().LeftButton == ButtonState.Pressed & !Select.Active) Select.Start(cx, cy);
                if (Mouse.GetState().LeftButton != ButtonState.Pressed & Select.Active)
                {
                    Select.End(cx, cy);
                    //Создаём штамп с имеющейся карты!
                    t.Width = Select.Width;
                    t.Height = Select.Height;
                    for (int i = 0; i < t.Width; i++)
                        for (int j = 0; j < t.Height; j++)
                            t.M[i, j] = Project.Get(l, Select.Left + i, Select.Top + j);
                    Select.Active = false;
                    Mode = Modes.Edit;
                    KeyHold = true;
                }
            }
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
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            if (Mode == Modes.Edit)
            {
                DrawLayers();
                //Курсор
                int s = Project.TileSize;
                float kx = Editor.X * Project.Px[l].X;
                float ky = Editor.Y * Project.Px[l].Y;
                spriteBatch.Draw(WhitePixel, new Rectangle(
                    (int)(Mouse.GetState().X + kx) / s * s - (int)(kx), (int)(Mouse.GetState().Y + ky) / s * s - (int)(ky),
                    t.Width * Project.TileSize, t.Height * Project.TileSize), new Rectangle(0, 0, 1, 1), col); //Ебическая сила
                //Табличка слоёв
                if (TimerTitles > 1)
                {
                    al = 255;
                    if (TimerTitles < 128) al = (byte)(TimerTitles * 2); //Это даёт красивый эффект, затухание происходит не линейно, а с паузой. Круть :-)
                    spriteBatch.Draw(WhitePixel, new Rectangle(0, 0, 90, 20 * Project.Layers + 10), Color.FromNonPremultiplied(0, 0, 0, (int)(al*0.75f)));
                    for (int i = 0; i < Project.Layers; i++)
                    {
                        if (!Editor.ShowOnlyCurrentLayer | i + 1 == l)
                            spriteBatch.Draw(Checkbox, new Vector2(0, i * 20), new Rectangle(0, 0, 20, 20), Color.FromNonPremultiplied(255, 255, 255, al));
                        else
                            spriteBatch.Draw(Checkbox, new Vector2(0, i * 20), new Rectangle(0, 20, 20, 20), Color.FromNonPremultiplied(255, 255, 255, al));
                        byte br = 92;
                        if (i + 1 == l & EditMode != EditModes.Carcase) br = 255;
                        spriteBatch.Draw(Labels, new Vector2(20, i * 20), new Rectangle(0, i * 18, 60, 18), Color.FromNonPremultiplied(br, br, 255, al));
                    }
                    TimerTitles -= 2;
                }
            }
            //Режим выбора инструментов
            if (Mode == Modes.SelectTool)
            {
                //Посчитаем сколько у нас получается спрайтов
                int Nums = SpritesCount();
                int Rows = Nums / (Project.ScreenWidth / Project.TileSize);
                //Скорректируем, если надо курсор
                if (t.Scroll > Rows - (Project.ScreenHeight / Project.TileSize)) t.Scroll = Rows - (Project.ScreenHeight / Project.TileSize);
                if (t.Scroll < 0) t.Scroll = 0;
                //Ну и, собственно, нарисуем сетку спрайтов
                for (int i = 0; i < Project.ScreenWidth / Project.TileSize; i++)
                    for (int j = 0; j < Project.ScreenHeight / Project.TileSize; j++)
                    {
                        int n = i + (j + t.Scroll) * Project.ScreenWidth / Project.TileSize;
                        if (n <= Nums)
                        {
                            spriteBatch.Draw(tex, new Vector2(i * Project.TileSize, j * Project.TileSize), SpriteByNum(n), Color.White);
                            DrawSmallNum(i * Project.TileSize, j * Project.TileSize, n, Color.White);
                        }
                    }
                //Курсор
                spriteBatch.Draw(WhitePixel, new Rectangle(Select.Left * Project.TileSize, Select.Top * Project.TileSize,
                    Select.Width * Project.TileSize, Select.Height * Project.TileSize), new Rectangle(0, 32, 32, 32), col);
            }
            if (Mode == Modes.SelectStamp)
            {
                DrawLayers();
                spriteBatch.End();
                //Курсор
                float kx = Select.Left * Project.TileSize - Editor.X * Project.Px[l].X;
                float ky = Select.Top * Project.TileSize - Editor.Y * Project.Px[l].Y;
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearWrap,
                    DepthStencilState.Default, RasterizerState.CullNone);
                spriteBatch.Draw(Cursor, new Vector2(kx, ky),
                    new Rectangle(0, 0, Select.Width * Project.TileSize, Select.Height * Project.TileSize),
                    col, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                spriteBatch.End();
                //Табличка режима выделения
                spriteBatch.Begin();
                spriteBatch.Draw(WhitePixel, new Rectangle(Project.ScreenWidth / 2 - 120, 0, 240, 25), Color.FromNonPremultiplied(0, 0, 0, 192));
                spriteBatch.Draw(LabelStamp, new Vector2(Project.ScreenWidth / 2 - 110, 0), Color.FromNonPremultiplied(92, 92, 255, 255));
                Select.End(Mouse.GetState().X / Project.TileSize, Mouse.GetState().Y / Project.TileSize);
            }
            if (Mode == Modes.Tiling)
            {
                DrawLayers();
                spriteBatch.End();
                //Курсор
                float kx = Select.Left * Project.TileSize - Editor.X * Project.Px[l].X;
                float ky = Select.Top * Project.TileSize - Editor.Y * Project.Px[l].Y;
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearWrap,
                    DepthStencilState.Default, RasterizerState.CullNone);
                spriteBatch.Draw(Tiling, new Vector2(kx, ky),
                    new Rectangle(0, 0, Select.Width * Project.TileSize, Select.Height * Project.TileSize),
                    col, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                spriteBatch.End();
                //Табличка режима тайлинга
                spriteBatch.Begin();
                spriteBatch.Draw(WhitePixel, new Rectangle(Project.ScreenWidth / 2 - 105, 0, 200, 25), Color.FromNonPremultiplied(0, 0, 0, 192));
                spriteBatch.Draw(LabelTiling, new Vector2(Project.ScreenWidth / 2 - 95, 0), Color.FromNonPremultiplied(92, 92, 255, 255));
                Select.End(Mouse.GetState().X / Project.TileSize, Mouse.GetState().Y / Project.TileSize);
            }
            //Мерцание курсора
            Cursorcolor += 8;
            if (Cursorcolor > 255) Cursorcolor = 0;
            spriteBatch.End();
            base.Draw(gameTime);
        }
        #endregion

        #region Всякое разное
        /// <summary>
        /// Рисование слоёв
        /// </summary>
        public void DrawLayers()
        {
            //Сначала рисуем видимые слои текстурные
            for (int l = 1; l <= Project.Layers; l++)
                if (!Editor.ShowOnlyCurrentLayer | (Editor.ShowOnlyCurrentLayer & l == Editor.Layer))
                {
                    int kamx = (int)(Editor.X * Project.Px[l].X); //Вычисление камеры для конкретного слоя с учётом коэффициентов смещения
                    int kamy = (int)(Editor.Y * Project.Px[l].Y);
                    int leftTile = kamx / Project.TileSize;
                    int topTile = kamy / Project.TileSize;
                    Color col = Color.White;
                    if (l == Editor.Layer && !Editor.ShowOnlyCurrentLayer & EditMode == EditModes.Layers & TimerLayer > 0)
                    {
                        TimerLayer--;
                        col = new Color(256 - TimerLayer * 16, 256 - TimerLayer * 16, 256 - TimerLayer * 16, 256 - TimerLayer * 16);
                    }
                    for (int i = leftTile; i < leftTile + Project.TilesWidth; i++)
                        for (int j = topTile; j < topTile + Project.TilesHeight; j++)
                        {
                            int x = i * Project.TileSize - kamx;
                            int y = j * Project.TileSize - kamy;
                            if (Project.M[l, i, j] > 0)
                            {
                                spriteBatch.Draw(WALL, new Vector2(x, y), SpriteByNum(Project.M[l, i, j]), col);
                                if (l == Editor.Layer & EditMode == EditModes.Layers & Editor.Codes) DrawSmallNum(x, y, Project.M[l, i, j], col);
                            }
                        }
                }
            //Затем рисуем, если видно каркасный слой
            if (EditMode == EditModes.Carcase)
            {
                int leftTile = Editor.X / Project.TileSize;
                int topTile = Editor.Y / Project.TileSize;
                for (int i = leftTile; i < leftTile + Project.TilesWidth; i++)
                    for (int j = topTile; j < topTile + Project.TilesHeight; j++)
                    {
                        int x = i * Project.TileSize - Editor.X;
                        int y = j * Project.TileSize - Editor.Y;
                        spriteBatch.Draw(Karkas, new Vector2(x, y), SpriteByNum(Project.M[0, i, j]), Color.White);
                        if (EditMode == EditModes.Carcase & Editor.Codes) DrawSmallNum(x, y, Project.M[0, i, j], Color.White);
                    }
            }
        }

        /// <summary>
        /// Вычисление количества спрайт в текстуре
        /// </summary>
        /// <returns></returns>
        public int SpritesCount() { return (WALL.Width / Project.TileSize) * (WALL.Height / Project.TileSize); }

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
                spriteBatch.Draw(SmallDigits, new Vector2(x + i * 5, y), new Rectangle((str[i] - 48) * 5, 0, 5, 7), color);
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
            //Загружаем текстуру
            try
            {
                System.IO.FileStream file = new System.IO.FileStream(Project.FileTexture, System.IO.FileMode.Open);
                WALL = Texture2D.FromStream(GraphicsDevice, file);
                file.Dispose(); //Освобождаем файл вручную, ибо иначе его больше нельзя будет выбрать в этом сеансе (тупо, чё сказать...)
            }
            catch
            {
                //Загрузка обернулась фэйлом, создаём пустую текстуру
                WALL = new Texture2D(GraphicsDevice, 640, 480); //Над размерами надо ещё подумать
            }
            //Загружаем графику
            try
            {
                System.IO.FileStream file = new System.IO.FileStream(Project.FileKarkas, System.IO.FileMode.Open);
                Karkas = Texture2D.FromStream(GraphicsDevice, file);
                file.Dispose(); //Освобождаем файл вручную, ибо иначе его больше нельзя будет выбрать в этом сеансе (тупо, чё сказать...)
            }
            catch
            {
                //Загрузка обернулась фэйлом, создаём пустую текстуру
                Karkas = new Texture2D(GraphicsDevice, 640, 480); //Над размерами надо ещё подумать
            }
        }

        /// <summary>
        /// Очень важная функция, вырезающая нужный квадратик из текстуры
        /// </summary>
        /// <param name="Num">Номер спрайта</param>
        /// <returns></returns>
        Rectangle SpriteByNum(int Num)
        {
            //Определяем количество спрайтов по горизонтали
            int WSprites = WALL.Width / Project.TileSize;
            //находим строку
            int str = Num / WSprites;
            //Находим колонку
            int tab = Num % WSprites;
            return new Rectangle(tab * Project.TileSize, str * Project.TileSize, Project.TileSize, Project.TileSize);
        }
        #endregion
    }
}
