﻿// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Reflection;
using System.Windows.Forms;
using HtmlRenderer.Demo.WinForms.Properties;
using HtmlRenderer.WinForms;

namespace HtmlRenderer.Demo.WinForms
{
    public partial class GenerateImageForm : Form
    {
        private readonly string _html;
        private readonly Bitmap _background;

        public GenerateImageForm(string html)
        {
            _html = html;
            InitializeComponent();

            Icon = Icon.FromHandle(Resources.image.GetHicon());

            _background = new Bitmap(10, 10);
            using (var g = Graphics.FromImage(_background))
            {
                g.Clear(Color.White);
                g.FillRectangle(SystemBrushes.Control, new Rectangle(0, 0, 5, 5));
                g.FillRectangle(SystemBrushes.Control, new Rectangle(5, 5, 5, 5));
            }

            foreach (var color in GetColors())
            {
                if (color != Color.Transparent)
                    _backgroundColorTSB.Items.Add(color.Name);
            }
            _backgroundColorTSB.SelectedItem = Color.White.Name;

            foreach (var hint in Enum.GetNames(typeof(TextRenderingHint)))
            {
                _textRenderingHintTSCB.Items.Add(hint);
            }
            _textRenderingHintTSCB.SelectedItem = TextRenderingHint.AntiAlias.ToString();
        }

        private void OnUseGdiPlus_Click(object sender, EventArgs e)
        {
            _useGdiPlusTSB.Checked = !_useGdiPlusTSB.Checked;
            _textRenderingHintTSCB.Visible = _useGdiPlusTSB.Checked;
            _backgroundColorTSB.Visible = !_useGdiPlusTSB.Checked;
            _toolStripLabel.Text = _useGdiPlusTSB.Checked ? "Text Rendering Hint:" : "Background:";
            GenerateImage();
        }

        private void OnBackgroundColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            GenerateImage();
        }

        private void _textRenderingHintTSCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            GenerateImage();
        }

        private void OnGenerateImage_Click(object sender, EventArgs e)
        {
            GenerateImage();
        }

        private void GenerateImage()
        {
            if (_backgroundColorTSB.SelectedItem != null && _textRenderingHintTSCB.SelectedItem != null)
            {
                var backgroundColor = Color.FromName(_backgroundColorTSB.SelectedItem.ToString());
                TextRenderingHint textRenderingHint;
                Enum.TryParse(_textRenderingHintTSCB.SelectedItem.ToString(), out textRenderingHint);
                var img = _useGdiPlusTSB.Checked
                    ? HtmlRender.RenderToImageGdiPlus(_html, _pictureBox.ClientSize, textRenderingHint, null, HtmlRenderingHelper.OnStylesheetLoad, HtmlRenderingHelper.OnImageLoad)
                    : HtmlRender.RenderToImage(_html, _pictureBox.ClientSize, backgroundColor, null, HtmlRenderingHelper.OnStylesheetLoad, HtmlRenderingHelper.OnImageLoad);
                _pictureBox.Image = img;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            using (var b = new TextureBrush(_background, WrapMode.Tile))
            {
                e.Graphics.FillRectangle(b, ClientRectangle);
            }
        }

        private static List<Color> GetColors()
        {
            const MethodAttributes attributes = MethodAttributes.Static | MethodAttributes.Public;
            PropertyInfo[] properties = typeof(Color).GetProperties();
            List<Color> list = new List<Color>();
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo info = properties[i];
                if (info.PropertyType == typeof(Color))
                {
                    MethodInfo getMethod = info.GetGetMethod();
                    if ((getMethod != null) && ((getMethod.Attributes & attributes) == attributes))
                    {
                        list.Add((Color)info.GetValue(null, null));
                    }
                }
            }
            return list;
        }
    }
}