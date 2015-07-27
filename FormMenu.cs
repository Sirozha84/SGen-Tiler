﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SGen_Tiler
{
    public partial class FormMenu : Form
    {
        const string FilterPNG = "Изображение(*.png)|*.png|Все файлы|*.*";
        const string FilterMAP = "Карты(*.map)|*.map|Все файлы|*.*";
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
            label_texture.Text = Config.FileTexture;
            label_karkas.Text = Config.FileKarkas;
            checkBox_savetexture.Checked = Project.AttachTexture;
            checkBox_savekarkas.Checked = Project.AttachCarcase;
            numericUpDown_tilesize.Value = Project.TileSize;
            numericUpDown_resx.Value = Project.ScreenWidth;
            numericUpDown_rexy.Value = Project.ScreenHeight;
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
            numericUpDown_avtoMain.Value = AutoRule.Layer;
            RefreshRulesList();
            //Анимация
            checkBox_animation.Checked = Animation.Enable;
            comboBox_animation_type.SelectedIndex = 0;
            RefreshAnimationList();
            //Рандом
            checkBox_random.Checked = RandomTile.Enable;
            RefreshRandomList();
            //Имя файла
            RefreshName();
            UserChange = true;
        }

        /// <summary>
        /// Оновлени заголовка окна
        /// </summary>
        void RefreshName()
        {
            string star = "";
            string name = "Новый";
            if (!Project.Saved) star = "*";
            if (Project.FileName != "") name = System.IO.Path.GetFileNameWithoutExtension(Project.FileName);
            Text = name + star + " - " + Program.Name;
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
            if (!Project.Saved) rslt = MessageBox.Show("Сохранить карту перед созданием новой?", Program.Name, MessageBoxButtons.YesNoCancel);
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
            if (!Project.Saved) rslt = MessageBox.Show("Сохранить карту перед открытием другой?", Program.Name, MessageBoxButtons.YesNoCancel);
            if (rslt == DialogResult.Cancel) return;
            if (rslt == DialogResult.Yes && !Save(false)) return;
            //Диалог открытия
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = FilterMAP;
            if (open.ShowDialog() == DialogResult.Cancel) return;
            Project.FileName = open.FileName;
            Project.Load();
            FormRefresh();
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
            Project.Save();
            FormRefresh();
            return true;
        }
        #endregion

        #region Основные параметры

        //Выбор файлов текстур и галочки
        private void button_tiletexture_Click(object sender, EventArgs e)
        {
            OpenFileDialog load = new OpenFileDialog();
            load.Filter = FilterPNG;
            if (load.ShowDialog() == DialogResult.Cancel) return;
            Config.FileTexture = load.FileName;
            label_texture.Text = Config.FileTexture;
            Program.game.InitialTextures();
            numericUpDown1_ValueChanged(null, null);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog load = new OpenFileDialog();
            load.Filter = FilterPNG;
            if (load.ShowDialog() == DialogResult.Cancel) return;
            Config.FileKarkas = load.FileName;
            label_karkas.Text = Config.FileKarkas;
            Program.game.InitialTextures();
        }

        private void checkBox_savetexture_CheckedChanged(object sender, EventArgs e)
        {
            Project.AttachTexture = checkBox_savetexture.Checked;
            Change();
        }

        private void checkBox_savekarkas_CheckedChanged(object sender, EventArgs e)
        {
            Project.AttachCarcase = checkBox_savekarkas.Checked;
            Change();
        }
        //Настройки размеров
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

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            Project.Width = (int)numericUpDown_width.Value;
            Change();
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            Project.Height = (int)numericUpDown_height.Value;
            Change();
        }


        private void button2_Click(object sender, EventArgs e)
        {
            Project.Save();
        }

        private void numericUpDown8_ValueChanged(object sender, EventArgs e) { Project.Px[1].X = (float)numericUpDown_shiftx1.Value; Change(); }
        private void numericUpDown9_ValueChanged(object sender, EventArgs e) { Project.Px[1].Y = (float)numericUpDown_shifty1.Value; Change(); }
        private void numericUpDown10_ValueChanged(object sender, EventArgs e) { Project.Px[2].X = (float)numericUpDown_shiftx2.Value; Change(); }
        private void numericUpDown11_ValueChanged(object sender, EventArgs e) { Project.Px[2].Y = (float)numericUpDown_shifty2.Value; Change(); }
        private void numericUpDown12_ValueChanged(object sender, EventArgs e) { Project.Px[3].X = (float)numericUpDown_shiftx3.Value; Change(); }
        private void numericUpDown13_ValueChanged(object sender, EventArgs e) { Project.Px[3].Y = (float)numericUpDown_shifty3.Value; Change(); }
        private void numericUpDown14_ValueChanged(object sender, EventArgs e) { Project.Px[4].X = (float)numericUpDown_shiftx4.Value; Change(); }
        private void numericUpDown15_ValueChanged(object sender, EventArgs e) { Project.Px[4].Y = (float)numericUpDown_shifty4.Value; Change(); }
        private void numericUpDown16_ValueChanged(object sender, EventArgs e) { Project.Px[5].X = (float)numericUpDown_shiftx5.Value; Change(); }
        private void numericUpDown17_ValueChanged(object sender, EventArgs e) { Project.Px[5].Y = (float)numericUpDown_shifty5.Value; Change(); }
        private void numericUpDown18_ValueChanged(object sender, EventArgs e) { Project.Px[6].X = (float)numericUpDown_shiftx6.Value; Change(); }
        private void numericUpDown19_ValueChanged(object sender, EventArgs e) { Project.Px[6].Y = (float)numericUpDown_shifty6.Value; Change(); }
        private void numericUpDown20_ValueChanged(object sender, EventArgs e) { Project.Px[7].X = (float)numericUpDown_shiftx7.Value; Change(); }
        private void numericUpDown21_ValueChanged(object sender, EventArgs e) { Project.Px[7].Y = (float)numericUpDown_shifty7.Value; Change(); }
        private void numericUpDown22_ValueChanged(object sender, EventArgs e) { Project.Px[8].X = (float)numericUpDown_shiftx8.Value; Change(); }
        private void numericUpDown23_ValueChanged(object sender, EventArgs e) { Project.Px[8].Y = (float)numericUpDown_shifty8.Value; Change(); }

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
            numericUpDown_avtoMain.Maximum = numericUpDown_layers.Value;
            Change();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FormAbout fa = new FormAbout();
            fa.ShowDialog();
        }

        /// <summary>
        /// Расчёт количества видимых тайлов на экране
        /// </summary>
        void CalculateVisibleTiles()
        {
            label_visibletiles.Text = (numericUpDown_resx.Value / Project.TileSize).ToString("0.00") + " x " +
                                      (numericUpDown_rexy.Value / Project.TileSize).ToString("0.00");
        }
        #endregion

        #region Автозаполнение
        private void numericUpDown_avtoMain_ValueChanged(object sender, EventArgs e)
        {
            AutoRule.Layer = (int)numericUpDown_avtoMain.Value;
            Change();
        }

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
            listBox_avto.Items.Clear();
            foreach (AutoRule rule in Project.AutoRules)
                listBox_avto.Items.Add(rule.Code + " - от " + rule.From + " до " + rule.To);
        }

        private void button_avtoDel_Click(object sender, EventArgs e)
        {
            if (listBox_avto.SelectedIndex == 0) MessageBox.Show("Первое правило удалить нельзя", Program.Name);
            if (listBox_avto.SelectedIndex > 0)
            {
                Project.AutoRules.RemoveAt(listBox_avto.SelectedIndex);
                listBox_avto.Items.RemoveAt(listBox_avto.SelectedIndex);
                Change();
            }
        }

        private void button_avtoClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Уверены что хотити этого?", Program.Name, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Project.AutoRulesClear();
                RefreshRulesList();
                Change();
            }
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
                    MessageBox.Show("Анимация для этого кода уже существует.", Program.Name);
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
            listBox_animation.Items.Clear();
            foreach (Animation anim in Project.Animations)
                listBox_animation.Items.Add(anim.Code + " - Кадров: " + anim.Frames + ", Время: " + anim.Time + ", тип: " + anim.TypeString());
        }

        private void button_animation_del_Click(object sender, EventArgs e)
        {
            if (listBox_animation.SelectedIndex >= 0)
            {
                Project.Animations.RemoveAt(listBox_animation.SelectedIndex);
                RefreshAnimationList();
                Change();
            }
        }

        private void button_animation_clear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Уверены что хотити этого?", Program.Name, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Project.Animations.Clear();
                listBox_animation.Items.Clear();
                Change();
            }
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
            listBox_random.Items.Clear();
            foreach (RandomTile rand in Project.Randoms)
                listBox_random.Items.Add(rand.Code + " - Случайный тайл: " + rand.Tile + ", Вероятность: " + rand.Persent + "%");
        }

        private void checkBox_random_CheckedChanged(object sender, EventArgs e)
        {
            RandomTile.Enable = checkBox_random.Checked;
            Change();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox_random.SelectedIndex >= 0)
            {
                Project.Randoms.RemoveAt(listBox_random.SelectedIndex);
                RefreshRandomList();
                Change();
            }
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            if (MessageBox.Show("Уверены что хотити этого?", Program.Name, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Project.Randoms.Clear();
                listBox_random.Items.Clear();
                Change();
            }
        }
        #endregion

        /// <summary>
        /// Регистрация изменений
        /// </summary>
        void Change()
        {
            if (UserChange)
            {
                Project.Change();
                RefreshName();
            }
        }
    }
}
