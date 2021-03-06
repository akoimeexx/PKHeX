﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PKHeX
{
    public partial class Util
    {
        // Form Translation
        internal static void TranslateInterface(Control form, string lang)
        {
            // Check to see if a the translation file exists in the same folder as the executable
            string externalLangPath = "lang_" + lang + ".txt";
            string[] rawlist;
            if (File.Exists(externalLangPath))
                rawlist = File.ReadAllLines(externalLangPath);
            else
            {
                object txt = Properties.Resources.ResourceManager.GetObject("lang_" + lang);
                if (txt == null) return; // Translation file does not exist as a resource; abort this function and don't translate UI.
                rawlist = ((string)txt).Split(new[] { "\n" }, StringSplitOptions.None);
                rawlist = rawlist.Select(i => i.Trim()).ToArray(); // Remove trailing spaces
            }

            List<string> stringdata = new List<string>();
            int start = -1;
            for (int i = 0; i < rawlist.Length; i++)
            {
                // Find our starting point
                if (!rawlist[i].Contains("! " + form.Name)) continue;
                start = i;
                break;
            }
            if (start < 0)
                return;

            // Rename Window Title
            string[] WindowName = rawlist[start].Split(new[] { " = " }, StringSplitOptions.None);
            if (WindowName.Length > 1) form.Text = WindowName[1];

            // Fetch controls to rename
            for (int i = start + 1; i < rawlist.Length; i++)
            {
                if (rawlist[i].Length == 0) continue; // Skip Over Empty Lines, errhandled
                if (rawlist[i][0] == '-') continue; // Keep translating if line is a comment line
                if (rawlist[i][0] == '!') // Stop if we have reached the end of translation
                    break;
                stringdata.Add(rawlist[i]); // Add the entry to process later.
            }

            if (stringdata.Count == 0)
                return;

            // Find control then change display Text.
            form.SuspendLayout();
            foreach (string str in stringdata)
            {
                string[] SplitString = str.Split(new[] { " = " }, StringSplitOptions.None);
                if (SplitString.Length < 2)
                    continue;

                object c = FindControl(SplitString[0], form.Controls); // Find control within Form's controls
                if (c == null) // Not found
                    continue;

                string text = SplitString[1]; // Text to set Control.Text to...

                if (c is Control)
                    (c as Control).Text = text;
                else if (c is ToolStripItem)
                    (c as ToolStripItem).Text = text;
            }
            form.ResumeLayout();
        }
        private static object FindControl(string name, Control.ControlCollection c)
        {
            Control control = c.Find(name, true).FirstOrDefault();
            if (control != null)
                return control;
            foreach (MenuStrip menu in c.OfType<MenuStrip>())
            {
                var item = menu.Items.Find(name, true).FirstOrDefault();
                if (item != null)
                    return item;
            }
            foreach (ContextMenuStrip strip in FindContextMenuStrips(c.OfType<Control>()))
            {
                var item = strip.Items.Find(name, true).FirstOrDefault();
                if (item != null)
                    return item;
            }
            return null;
        }
        private static List<ContextMenuStrip> FindContextMenuStrips(IEnumerable<Control> c)
        {
            List<ContextMenuStrip> cs = new List<ContextMenuStrip>();
            foreach (Control control in c)
            {
                if (control.ContextMenuStrip != null)
                    cs.Add(control.ContextMenuStrip);

                else if (control.Controls.Count > 0)
                    cs.AddRange(FindContextMenuStrips(control.Controls.OfType<Control>()));
            }
            return cs;
        }
        internal static void CenterToForm(Control child, Control parent)
        {
            int x = parent.Location.X + (parent.Width - child.Width) / 2;
            int y = parent.Location.Y + (parent.Height - child.Height) / 2;
            child.Location = new Point(Math.Max(x, 0), Math.Max(y, 0));
        }

        // Message Displays
        internal static DialogResult Error(params string[] lines)
        {
            System.Media.SystemSounds.Exclamation.Play();
            string msg = string.Join(Environment.NewLine + Environment.NewLine, lines);
            return MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        internal static DialogResult Alert(params string[] lines)
        {
            System.Media.SystemSounds.Asterisk.Play();
            string msg = string.Join(Environment.NewLine + Environment.NewLine, lines);
            return MessageBox.Show(msg, "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        internal static DialogResult Prompt(MessageBoxButtons btn, params string[] lines)
        {
            System.Media.SystemSounds.Question.Play();
            string msg = string.Join(Environment.NewLine + Environment.NewLine, lines);
            return MessageBox.Show(msg, "Prompt", btn, MessageBoxIcon.Asterisk);
        }

        internal static int getIndex(ComboBox cb)
        {
            return (int)(cb?.SelectedValue ?? 0);
        }

        public static void PanelScroll(object sender, ScrollEventArgs e)
        {
            var p = sender as Panel;
            switch (e.ScrollOrientation)
            {
                case ScrollOrientation.HorizontalScroll:
                    p.HorizontalScroll.Value = e.NewValue;
                    break;
                case ScrollOrientation.VerticalScroll:
                    p.VerticalScroll.Value = e.NewValue;
                    break;
            }
        }
    }
}
