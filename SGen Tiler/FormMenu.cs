using System;
using System.IO;
using System.Windows.Forms;

namespace SGen_Tiler
{
    public partial class FormMenu : Form
    {
        const string FilterPNG = "Изображение(*.png)|*.png|Все файлы|*.*";
        public const string FilterMAP = "Карты(*.map)|*.map|Все файлы|*.*";
        /// <summary>
        /// Пользователь ли меняет данные на форме или программа?
        /// Нужно для того что бы пересчёт делался только если именно пользователь меняет данные, а не программа
        /// </summary>
        bool UserChange = true;

        public FormMenu()
        {
            InitializeComponent();
        }

        private void FormMenu_Load(object sender, EventArgs e)
        {
            FormRefresh();
        }

        /// <summary>
        /// Обновление формы
        /// </summary>
        void FormRefresh()
        {
            UserChange = false;
            //Основные параметры
            button_tiletexture.Text = Project.FileTexture;
            button_carcasetexture.Text = Project.FileKarkas;
            button_backtexture.Text = Project.FileBackground;
            button_fronttexture.Text = Project.FileFront;
            numericUpDown_tilesize.Value = Project.TileSize;
            numericUpDown_resx.Value = Project.ScreenWidth;
            numericUpDown_rexy.Value = Project.ScreenHeight;
            numericUpDown_width.Maximum = Project.MaxWidth;
            numericUpDown_height.Maximum = Project.MaxHeight;
            numericUpDown_width.Value = Project.Width;
            numericUpDown_height.Value = Project.Height;
            numericUpDown_layers.Value = Project.Layers;
            //Параметры параллакса
            numericUpDown_shiftx1.Value = (decimal)Project.Px[1].X;
            numericUpDown_shifty1.Value = (decimal)Project.Px[1].Y;
            numericUpDown_shiftx2.Value = (decimal)Project.Px[2].X;
            numericUpDown_shifty2.Value = (decimal)Project.Px[2].Y;
            numericUpDown_shiftx3.Value = (decimal)Project.Px[3].X;
            numericUpDown_shifty3.Value = (decimal)Project.Px[3].Y;
            numericUpDown_shiftx4.Value = (decimal)Project.Px[4].X;
            numericUpDown_shifty4.Value = (decimal)Project.Px[4].Y;
            numericUpDown_shiftx5.Value = (decimal)Project.Px[5].X;
            numericUpDown_shifty5.Value = (decimal)Project.Px[5].Y;
            numericUpDown_shiftx6.Value = (decimal)Project.Px[6].X;
            numericUpDown_shifty6.Value = (decimal)Project.Px[6].Y;
            numericUpDown_shiftx7.Value = (decimal)Project.Px[7].X;
            numericUpDown_shifty7.Value = (decimal)Project.Px[7].Y;
            numericUpDown_shiftx8.Value = (decimal)Project.Px[8].X;
            numericUpDown_shifty8.Value = (decimal)Project.Px[8].Y;
            //Автозаполнение
            checkBox_avtoEnable.Checked = AutoRule.Enable;
            numericUpDown_main.Value = Project.Main;
            numericUpDown_phantom.Value = Project.Phantom;
            RefreshRulesList();
            //Анимация
            checkBox_animation.Checked = Animation.Enable;
            comboBox_animation_type.SelectedIndex = 0;
            RefreshAnimationList();
            //Рандом
            checkBox_random.Checked = RandomTile.Enable;
            RefreshRandomList();
            //Имя файла
            Text = Project.Label();
            UserChange = true;
        }

        #region Работа с файлом

        /// <summary>
        /// Кнопка Новый
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_new_Click(object sender, EventArgs e)
        {
            DialogResult rslt = DialogResult.No;
            if (!Project.Saved) rslt = MessageBox.Show("Сохранить карту перед созданием новой?",
                Application.ProductName, MessageBoxButtons.YesNoCancel);
            if (rslt == DialogResult.Cancel) return;
            if (rslt == DialogResult.Yes && !Save(false)) return;
            Project.NewMap();
            Project.FileName = "";
            FormRefresh();
        }

        /// <summary>
        /// Кнопка открыть
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_open_Click(object sender, EventArgs e)
        {
            DialogResult rslt = DialogResult.No;
            if (!Project.Saved) rslt = MessageBox.Show("Сохранить карту перед открытием другой?", 
                Application.ProductName, MessageBoxButtons.YesNoCancel);
            if (rslt == DialogResult.Cancel) return;
            if (rslt == DialogResult.Yes && !Save(false)) return;
            //Диалог открытия
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = FilterMAP;
            if (open.ShowDialog() == DialogResult.Cancel) return;
            Project.FileName = open.FileName;
            Project.Load();
            Program.game.InitialTextures();
            FormRefresh();
        }

        DialogResult Ask()
        {
            DialogResult rslt = DialogResult.No;
            if (!Project.Saved) rslt = MessageBox.Show("Сохранить перед этим текущий проект?", 
                Application.ProductName, MessageBoxButtons.YesNoCancel);
            return rslt;
        }

        /// <summary>
        /// Кнопка сохранить
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_save_Click(object sender, EventArgs e) { Save(false); }

        /// <summary>
        /// Кнопка Сохранить как
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_saveas_Click(object sender, EventArgs e) { Save(true); }

        /// <summary>
        /// Сохранить как
        /// </summary>
        /// <returns></returns>
        bool Save(bool AS)
        {
            if (Project.FileName == "" | AS)
            {
                //Диалог сохранения
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = FilterMAP;
                if (save.ShowDialog() == DialogResult.Cancel) return false;
                Project.FileName = save.FileName;
            }
            Project.Save(false);
            FormRefresh();
            return true;
        }

        /// <summary>
        /// Сохранение шаблона
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = FilterMAP;
            if (save.ShowDialog() == DialogResult.Cancel) return;
            Project.FileNameTemplate = save.FileName;
            Project.Save(true);
        }
        #endregion

        #region Основные параметры

        //Выбор файлов текстур
        private void button_tiletexture_Click(object sender, EventArgs e)
        {
            TextureSelect(ref Project.FileTexture);
            button_tiletexture.Text = Path.GetFileName(Project.FileTexture);
            //Program.game.InitialTextures();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            TextureSelect(ref Project.FileKarkas);
            button_carcasetexture.Text = Path.GetFileName(Project.FileKarkas);
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            TextureSelect(ref Project.FileBackground);
            button_backtexture.Text = Path.GetFileName(Project.FileBackground);
        }

        private void button_fronttexture_Click(object sender, EventArgs e)
        {
            TextureSelect(ref Project.FileFront);
            button_fronttexture.Text = Path.GetFileName(Project.FileFront);
        }

        /// <summary>
        /// Выыбор файла текстуры
        /// </summary>
        /// <param name="file"></param>
        void TextureSelect(ref string file)
        {
            OpenFileDialog load = new OpenFileDialog();
            load.Filter = FilterPNG;
            if (load.ShowDialog() == DialogResult.Cancel) return;
            file = load.FileName;
            Program.game.InitialTextures();
        }

        /// <summary>
        /// Настройки размеров
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Project.TileSize = (int)numericUpDown_tilesize.Value;
            label10.Text = Program.game.SpritesCount().ToString();
            CalculateVisibleTiles();
            Change();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            Project.ScreenWidth = (int)numericUpDown_resx.Value;
            CalculateVisibleTiles();
            Program.game.ChangeWindowSize();
            Change();
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            Project.ScreenHeight = (int)numericUpDown_rexy.Value;
            CalculateVisibleTiles();
            Program.game.ChangeWindowSize();
            Change();
        }

        /// <summary>
        /// Выбор ширины карты
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            Project.Width = (int)numericUpDown_width.Value;
            Change();
        }

        /// <summary>
        /// Выбор высоты карты
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            Project.Height = (int)numericUpDown_height.Value;
            Change();
        }

        /// <summary>
        /// Расчёт количества видимых тайлов на экране
        /// </summary>
        void CalculateVisibleTiles()
        {
            label_visibletiles.Text = (numericUpDown_resx.Value / Project.TileSize).ToString("0.00") + " x " +
                                      (numericUpDown_rexy.Value / Project.TileSize).ToString("0.00");
        }

        /// <summary>
        /// Выбор количества слоёв
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            byte n = (byte)numericUpDown_layers.Value;
            bool v = n > 1; label14.Visible = v; label22.Visible = v; numericUpDown_shiftx2.Visible = v; numericUpDown_shifty2.Visible = v;
            v = n > 2; label15.Visible = v; label23.Visible = v; numericUpDown_shiftx3.Visible = v; numericUpDown_shifty3.Visible = v;
            v = n > 3; label16.Visible = v; label24.Visible = v; numericUpDown_shiftx4.Visible = v; numericUpDown_shifty4.Visible = v;
            v = n > 4; label17.Visible = v; label25.Visible = v; numericUpDown_shiftx5.Visible = v; numericUpDown_shifty5.Visible = v;
            v = n > 5; label18.Visible = v; label26.Visible = v; numericUpDown_shiftx6.Visible = v; numericUpDown_shifty6.Visible = v;
            v = n > 6; label19.Visible = v; label27.Visible = v; numericUpDown_shiftx7.Visible = v; numericUpDown_shifty7.Visible = v;
            v = n > 7; label20.Visible = v; label28.Visible = v; numericUpDown_shiftx8.Visible = v; numericUpDown_shifty8.Visible = v;
            Project.Layers = n;
            if (Editor.Layer >= n) Editor.Layer = n;
            numericUpDown_main.Maximum = Project.Layers;
            numericUpDown_phantom.Maximum = Project.Layers;
            if (numericUpDown_phantom.Value == Project.Main)
                numericUpDown_phantom.Value = 0;
            Change();
        }

        /// <summary>
        /// Выбор главного слоя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDown_avtoMain_ValueChanged_1(object sender, EventArgs e)
        {
            Project.Main = (byte)numericUpDown_main.Value;
            Change();
            ChechParallax();
        }

        /// <summary>
        /// Выбор фантомного слоя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDown_phantom_ValueChanged(object sender, EventArgs e)
        {
            Project.Phantom = (byte)numericUpDown_phantom.Value;
            label_error1.Visible = Project.Phantom <= Project.Main & Project.Phantom != 0;
            Change();
            ChechParallax();
        }
        #endregion

        #region Паралакс
        private void numericUpDown8_ValueChanged(object sender, EventArgs e)
            { Project.Px[1].X = (float)numericUpDown_shiftx1.Value; Change(); ChechParallax(); }
        private void numericUpDown9_ValueChanged(object sender, EventArgs e)
            { Project.Px[1].Y = (float)numericUpDown_shifty1.Value; Change(); ChechParallax(); }
        private void numericUpDown10_ValueChanged(object sender, EventArgs e)
            { Project.Px[2].X = (float)numericUpDown_shiftx2.Value; Change(); ChechParallax(); }
        private void numericUpDown11_ValueChanged(object sender, EventArgs e)
            { Project.Px[2].Y = (float)numericUpDown_shifty2.Value; Change(); ChechParallax(); }
        private void numericUpDown12_ValueChanged(object sender, EventArgs e)
            { Project.Px[3].X = (float)numericUpDown_shiftx3.Value; Change(); ChechParallax(); }
        private void numericUpDown13_ValueChanged(object sender, EventArgs e)
            { Project.Px[3].Y = (float)numericUpDown_shifty3.Value; Change(); ChechParallax(); }
        private void numericUpDown14_ValueChanged(object sender, EventArgs e)
            { Project.Px[4].X = (float)numericUpDown_shiftx4.Value; Change(); ChechParallax(); }
        private void numericUpDown15_ValueChanged(object sender, EventArgs e)
            { Project.Px[4].Y = (float)numericUpDown_shifty4.Value; Change(); ChechParallax(); }
        private void numericUpDown16_ValueChanged(object sender, EventArgs e)
            { Project.Px[5].X = (float)numericUpDown_shiftx5.Value; Change(); ChechParallax(); }
        private void numericUpDown17_ValueChanged(object sender, EventArgs e)
            { Project.Px[5].Y = (float)numericUpDown_shifty5.Value; Change(); ChechParallax(); }
        private void numericUpDown18_ValueChanged(object sender, EventArgs e)
            { Project.Px[6].X = (float)numericUpDown_shiftx6.Value; Change(); ChechParallax(); }
        private void numericUpDown19_ValueChanged(object sender, EventArgs e)
            { Project.Px[6].Y = (float)numericUpDown_shifty6.Value; Change(); ChechParallax(); }
        private void numericUpDown20_ValueChanged(object sender, EventArgs e)
            { Project.Px[7].X = (float)numericUpDown_shiftx7.Value; Change(); ChechParallax(); }
        private void numericUpDown21_ValueChanged(object sender, EventArgs e)
            { Project.Px[7].Y = (float)numericUpDown_shifty7.Value; Change(); ChechParallax(); }
        private void numericUpDown22_ValueChanged(object sender, EventArgs e)
            { Project.Px[8].X = (float)numericUpDown_shiftx8.Value; Change(); ChechParallax(); }
        private void numericUpDown23_ValueChanged(object sender, EventArgs e)
            { Project.Px[8].Y = (float)numericUpDown_shifty8.Value; Change(); ChechParallax(); }

        /// <summary>
        /// Проверка параметров параллакса
        /// </summary>
        private void ChechParallax()
        {
            label_error2.Visible = Project.Px[Project.Main].X != 1 |
                                   Project.Px[Project.Main].Y != 1 |
                                   Project.Px[Project.Phantom].X != 1 |
                                   Project.Px[Project.Phantom].Y != 1;
        }
        #endregion

        #region Автозаполнение
        private void button_avtoAdd_Click(object sender, EventArgs e)
        {
            AutoRule rule = new AutoRule((ushort)numericUpDown_avtoCode.Value,
                (ushort)numericUpDown_avtoFrom.Value, (ushort)numericUpDown_avtoTo.Value);
            Project.AutoRules.Add(rule);
            RefreshRulesList();
            Change();
        }

        private void checkBox_avtoEnable_CheckedChanged(object sender, EventArgs e) { AutoRule.Enable = checkBox_avtoEnable.Checked; Change(); }

        void RefreshRulesList()
        {
            listView_avto.Items.Clear();
            foreach (AutoRule rule in Project.AutoRules)
            {
                ListViewItem item = new ListViewItem(rule.Code.ToString());
                item.SubItems.Add(rule.From.ToString());
                item.SubItems.Add(rule.To.ToString());
                listView_avto.Items.Add(item);
            }
        }

        private void button_avtoDel_Click(object sender, EventArgs e)
        {
            if (listView_avto.SelectedIndices.Count == 0) return;
            if (listView_avto.SelectedIndices[0] == 0) MessageBox.Show("Первое правило удалить нельзя", Application.ProductName);
            if (listView_avto.SelectedIndices[0] > 0)
            {
                Project.AutoRules.RemoveAt(listView_avto.SelectedIndices[0]);
                listView_avto.Items.RemoveAt(listView_avto.SelectedIndices[0]);
                Change();
            }
        }

        /// <summary>
        /// Очистка правил автозаполнения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_avtoClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Уверены что хотити этого?", Application.ProductName, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Project.AutoRulesClear();
                RefreshRulesList();
                Change();
            }
        }

        /// <summary>
        /// Пересборка каркаса
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_remakecarcase_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Этот процесс перерисует каркас с учётом введёных правил. Продолжить?", Application.ProductName, MessageBoxButtons.YesNo)
                == DialogResult.No)
                return;
            for (int i = 0; i < Project.Width; i++)
                for (int j = 0; j < Project.Height; j++)
                    foreach (AutoRule rule in Project.AutoRules)
                        if (rule.In(Project.M[Project.Main, i, j]))
                            Project.Put(0, i, j, rule.Code);
            Hystory.AddRecord();
            Change();
            MessageBox.Show("Каркас пересобран", Application.ProductName);
        }

        /// <summary>
        /// Заполнение полей автозаполнения из выбранного правила
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_avto_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView_avto.SelectedIndices.Count == 0) return;
            if (listView_avto.SelectedIndices[0] == 0) return;
            numericUpDown_avtoCode.Value = Project.AutoRules[listView_avto.SelectedIndices[0]].Code;
            numericUpDown_avtoFrom.Value = Project.AutoRules[listView_avto.SelectedIndices[0]].From;
            numericUpDown_avtoTo.Value = Project.AutoRules[listView_avto.SelectedIndices[0]].To;
        }
        #endregion

        #region Анимация
        private void checkBox_animation_CheckedChanged(object sender, EventArgs e)
        {
            Animation.Enable = checkBox_animation.Checked;
            Change();
        }

        private void button_animationadd_Click(object sender, EventArgs e)
        {
            foreach (Animation anim in Project.Animations)
                if (anim.Code == numericUpDown_animation_code.Value)
                {
                    MessageBox.Show("Анимация для этого кода уже существует.", Application.ProductName);
                    return;
                }
            Project.Animations.Add(new Animation((ushort)numericUpDown_animation_code.Value, (byte)numericUpDown_animation_frames.Value,
                (byte)numericUpDown_animation_time.Value, (Animation.Types)comboBox_animation_type.SelectedIndex));
            RefreshAnimationList();
            Change();
        }

        /// <summary>
        /// Обновление списка анимаций
        /// </summary>
        void RefreshAnimationList()
        {
            listView_animation.Items.Clear();
            foreach (Animation anim in Project.Animations)
            {
                ListViewItem item = new ListViewItem(anim.Code.ToString());
                item.SubItems.Add(anim.Frames.ToString());
                item.SubItems.Add(anim.Time.ToString());
                item.SubItems.Add(anim.TypeString());
                listView_animation.Items.Add(item);
            }
        }

        private void button_animation_del_Click(object sender, EventArgs e)
        {
            if (listView_animation.SelectedIndices.Count == 0) return;
            if (listView_animation.SelectedIndices[0] >= 0)
            {
                Project.Animations.RemoveAt(listView_animation.SelectedIndices[0]);
                RefreshAnimationList();
                Change();
            }
        }

        /// <summary>
        /// Очистка правил анимации
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_animation_clear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Уверены что хотити этого?", Application.ProductName, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Project.Animations.Clear();
                listView_animation.Items.Clear();
                Change();
            }
        }

        /// <summary>
        /// Заполнение полей анимации из правила
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_animation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView_animation.SelectedIndices.Count == 0) return;
            numericUpDown_animation_code.Value = Project.Animations[listView_animation.SelectedIndices[0]].Code;
            numericUpDown_animation_frames.Value = Project.Animations[listView_animation.SelectedIndices[0]].Frames;
            numericUpDown_animation_time.Value = Project.Animations[listView_animation.SelectedIndices[0]].Time;
            comboBox_animation_type.SelectedIndex = (int)Project.Animations[listView_animation.SelectedIndices[0]].Type;
        }
        #endregion

        #region Рандом
        private void button5_Click(object sender, EventArgs e)
        {
            Project.Randoms.Add(new RandomTile((ushort)numericUpDown_rnd_code.Value, (ushort)numericUpDown_rnd_rdntile.Value, 
                (byte)numericUpDown_rnd_persent.Value));
            RefreshRandomList();
            Change();
        }

        /// <summary>
        /// Обновление списка рандомов
        /// </summary>
        void RefreshRandomList()
        {
            listView_random.Items.Clear();
            foreach (RandomTile rand in Project.Randoms)
            {
                ListViewItem item = new ListViewItem(rand.Code.ToString());
                item.SubItems.Add(rand.Tile.ToString());
                item.SubItems.Add(rand.Persent.ToString());
                listView_random.Items.Add(item);
            }
        }

        private void checkBox_random_CheckedChanged(object sender, EventArgs e)
        {
            RandomTile.Enable = checkBox_random.Checked;
            Change();
        }

        /// <summary>
        /// Удаление строки рандома
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            if (listView_random.SelectedIndices.Count == 0) return;
            if (listView_random.SelectedIndices[0] >= 0)
            {
                Project.Randoms.RemoveAt(listView_random.SelectedIndices[0]);
                RefreshRandomList();
                Change();
            }
        }

        /// <summary>
        /// Очистка правил рандома
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click_1(object sender, EventArgs e)
        {
            if (MessageBox.Show("Уверены что хотити этого?", Application.ProductName, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Project.Randoms.Clear();
                listView_random.Items.Clear();
                Change();
            }
        }

        /// <summary>
        /// Заполнение полей рандома из правила
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_random_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView_random.SelectedIndices.Count == 0) return;
            numericUpDown_rnd_code.Value = Project.Randoms[listView_random.SelectedIndices[0]].Code;
            numericUpDown_rnd_rdntile.Value = Project.Randoms[listView_random.SelectedIndices[0]].Tile;
            numericUpDown_rnd_persent.Value = Project.Randoms[listView_random.SelectedIndices[0]].Persent;
        }
        #endregion

        /// <summary>
        /// Регистрация изменений
        /// </summary>
        void Change()
        {
            if (UserChange)
            {
                Project.Saved = false;
                Text = Project.Label();
            }
        }

        /// <summary>
        /// Ссылка "Эбаут"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FormAbout fa = new FormAbout();
            fa.ShowDialog();
        }

    }
}
