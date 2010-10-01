/* ------------------------------------------------------------------------- */
/*
 *  MainForm.cs
 *
 *  Copyright (c) 2010 CubeSoft Inc. All rights reserved.
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see < http://www.gnu.org/licenses/ >.
 *
 *  Last-modified: Sat 02 Oct 2010 01:30:00 JST
 */
/* ------------------------------------------------------------------------- */
using System;
using System.Drawing;
using System.Windows.Forms;
using Container = System.Collections.Generic;

namespace Clown {
    /* --------------------------------------------------------------------- */
    /// MainForm
    /* --------------------------------------------------------------------- */
    public partial class MainForm : Form {
        /* ----------------------------------------------------------------- */
        /// Constructor
        /* ----------------------------------------------------------------- */
        public MainForm() {
            InitializeComponent();
            this.ContextMenuStrip = this.MainContextMenuStrip;
        }

        /* ----------------------------------------------------------------- */
        /// 各種イベントハンドラ
        /* ----------------------------------------------------------------- */
        #region Event handlers

        /* ----------------------------------------------------------------- */
        ///
        /// MainForm_MouseMove
        /// 
        /// <summary>
        /// タイトルバーを非表示に設定しているため，ウィンドウの移動を
        /// 自力で実装している．
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void MainForm_MouseMove(object sender, MouseEventArgs e) {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left) {
                this.Location = new Point(this.Location.X + e.X - pos_.X, this.Location.Y + e.Y - pos_.Y);
            }
        }

        /* ----------------------------------------------------------------- */
        /// MainForm_MouseDown
        /* ----------------------------------------------------------------- */
        private void MainForm_MouseDown(object sender, MouseEventArgs e) {
            pos_ = new Point(e.X, e.Y);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// MainForm_DragEnter
        /// 
        /// <summary>
        /// メインフォームに Drag&Drop 可能なファイルを指定する．
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void MainForm_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.All;
            else e.Effect = DragDropEffects.None;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// MainForm_DragDrop
        /// 
        /// <summary>
        /// Drag&Drop されたファイルのうち，*.jpeg, *.png, *.gif, *.bmp
        /// ファイルを WebP フォーマットへ変換する．フォルダが指定された
        /// 場合は，再帰的に展開する．
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void MainForm_DragDrop(object sender, DragEventArgs e) {
            var exec = System.Reflection.Assembly.GetEntryAssembly();
            var dir = System.IO.Path.GetDirectoryName(exec.Location);

            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                var dialog = new ProgressDialog();
                dialog.Title = "jpeg2webp";
                dialog.Minimum = 0;
                dialog.Maximum = 100;
                dialog.Value = 0;
                
                dialog.Show();

                var raw_files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var files = new Container.List<string>();
                foreach (var path in raw_files) this.ExtractImageFiles(path, files);

                foreach (var path in files) {
                    if (dialog.Canceled) break;

                    dialog.Value = dialog.Value + (dialog.Maximum / files.Count);
                    var ext = System.IO.Path.GetExtension(path).ToLower();
                    dialog.Message = path + " を変換中...";
                    try {
                        var proc = new System.Diagnostics.Process();
                        proc.StartInfo.FileName = dir + @"\webpconv.exe";
                        proc.StartInfo.Arguments = path;
                        proc.StartInfo.CreateNoWindow = true;
                        proc.StartInfo.UseShellExecute = false;
                        proc.Start();
                        proc.WaitForExit();
                        proc.Close();
                        proc.Dispose();
                    }
                    catch (Exception /* err */) { }
                }

                dialog.Close();
            }
        }

        /* ----------------------------------------------------------------- */
        /// CloseMenuItem_Click
        /* ----------------------------------------------------------------- */
        private void CloseMenuItem_Click(object sender, EventArgs e) {
            this.Close();
            this.Dispose();
        }

        #endregion

        /* ----------------------------------------------------------------- */
        /// 補助関数群
        /* ----------------------------------------------------------------- */
        #region Utility functions

        /* ----------------------------------------------------------------- */
        ///
        /// ExtractImageFiles
        /// 
        /// <summary>
        /// 指定されたパスから対象となる画像ファイルのパスを抽出する．
        /// src がフォルダの場合は，再帰的に展開する．
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void ExtractImageFiles(string src, Container.List<string> dest) {
            if (System.IO.Directory.Exists(src)) {
                var dir = new System.IO.DirectoryInfo(src);
                foreach (var item in dir.GetFileSystemInfos()) {
                    this.ExtractImageFiles(item.FullName, dest);
                }
            }
            else {
                var ext = System.IO.Path.GetExtension(src).ToLower();
                if (ext == ".bmp" || ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif") dest.Add(src);
            }
        }

        #endregion

        /* ----------------------------------------------------------------- */
        /// メンバ変数
        /* ----------------------------------------------------------------- */
        #region Member variables
        private Point pos_;
        #endregion
    }
}
