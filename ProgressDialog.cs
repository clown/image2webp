/* ------------------------------------------------------------------------- */
/*
 *  ProgressDialog.cs
 *  
 *  This program is derived from DOBON.NET.
 *  See also: http://dobon.net/vb/dotnet/programing/progressdialog.html
 *  
 *  Last-modified: Sat 02 Oct 2010 01:22:00 JST
 */
/* ------------------------------------------------------------------------- */
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Clown {
    /// <summary>
    /// ProgressForm の概要の説明です。
    /// </summary>
    public class ProgressForm : System.Windows.Forms.Form {
        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.ProgressBar ProgressBar1;
        internal System.Windows.Forms.Button Button1;
        private PictureBox pictureBox1;
        /// <summary>
        /// 必要なデザイナ変数です。
        /// </summary>
        private System.ComponentModel.Container components = null;

        public ProgressForm() {
            //
            // Windows フォーム デザイナ サポートに必要です。
            //
            InitializeComponent();
            var image = new Bitmap(32, 32);
            var g = Graphics.FromImage(image);
            g.DrawIcon(SystemIcons.Information, 0, 0);
            g.Dispose();
            this.pictureBox1.Image = image;

            //
            // TODO: InitializeComponent 呼び出しの後に、コンストラクタ コードを追加してください。
            //
        }

        /// <summary>
        /// 使用されているリソースに後処理を実行します。
        /// </summary>
        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (components != null) {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナで生成されたコード
        /// <summary>
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent() {
            this.Label1 = new System.Windows.Forms.Label();
            this.ProgressBar1 = new System.Windows.Forms.ProgressBar();
            this.Button1 = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // Label1
            // 
            this.Label1.Location = new System.Drawing.Point(53, 9);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(329, 32);
            this.Label1.TabIndex = 0;
            // 
            // ProgressBar1
            // 
            this.ProgressBar1.Location = new System.Drawing.Point(10, 48);
            this.ProgressBar1.Name = "ProgressBar1";
            this.ProgressBar1.Size = new System.Drawing.Size(291, 23);
            this.ProgressBar1.TabIndex = 1;
            // 
            // Button1
            // 
            this.Button1.Location = new System.Drawing.Point(307, 48);
            this.Button1.Name = "Button1";
            this.Button1.Size = new System.Drawing.Size(75, 23);
            this.Button1.TabIndex = 2;
            this.Button1.Text = "キャンセル";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(15, 10);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(32, 32);
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // ProgressForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
            this.ClientSize = new System.Drawing.Size(394, 82);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.Button1);
            this.Controls.Add(this.ProgressBar1);
            this.Controls.Add(this.Label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProgressForm";
            this.ShowInTaskbar = false;
            this.Text = "ProgressForm";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion
    }

    /// <summary>
    /// 進行状況ダイアログを表示するためのクラス
    /// </summary>
    public class ProgressDialog : IDisposable {
        //キャンセルボタンがクリックされたか
        private volatile bool _canceled = false;
        //ダイアログフォーム
        private volatile ProgressForm form;
        //フォームが表示されるまで待機するための待機ハンドル
        private System.Threading.ManualResetEvent startEvent;
        //フォームが一度表示されたか
        private bool showed = false;
        //フォームをコードで閉じているか
        private volatile bool closing = false;
        //オーナーフォーム
        private Form ownerForm;

        //別処理をするためのスレッド
        private System.Threading.Thread thread;

        //フォームのタイトル
        private volatile string _title = "進行状況";
        //ProgressBarの最小、最大、現在の値
        private volatile int _minimum = 0;
        private volatile int _maximum = 100;
        private volatile int _value = 0;
        //表示するメッセージ
        private volatile string _message = "";

        /// <summary>
        /// ダイアログのタイトルバーに表示する文字列
        /// </summary>
        public string Title {
            set {
                _title = value;
                if (form != null)
                    form.Invoke(new MethodInvoker(SetTitle));
            }
            get {
                return _message;
            }
        }

        /// <summary>
        /// プログレスバーの最小値
        /// </summary>
        public int Minimum {
            set {
                _minimum = value;
                if (form != null)
                    form.Invoke(new MethodInvoker(SetProgressMinimum));
            }
            get {
                return _minimum;
            }
        }

        /// <summary>
        /// プログレスバーの最大値
        /// </summary>
        public int Maximum {
            set {
                _maximum = value;
                if (form != null)
                    form.Invoke(new MethodInvoker(SetProgressMaximun));
            }
            get {
                return _maximum;
            }
        }

        /// <summary>
        /// プログレスバーの値
        /// </summary>
        public int Value {
            set {
                _value = value;
                if (form != null)
                    form.Invoke(new MethodInvoker(SetProgressValue));
            }
            get {
                return _value;
            }
        }

        /// <summary>
        /// ダイアログに表示するメッセージ
        /// </summary>
        public string Message {
            set {
                _message = value;
                if (form != null)
                    form.Invoke(new MethodInvoker(SetMessage));
            }
            get {
                return _message;
            }
        }

        /// <summary>
        /// キャンセルされたか
        /// </summary>
        public bool Canceled {
            get { return _canceled; }
        }

        /// <summary>
        /// ダイアログを表示する
        /// </summary>
        /// <param name="owner">
        /// ownerの中央にダイアログが表示される
        /// </param>
        /// <remarks>
        /// このメソッドは一回しか呼び出せません。
        /// </remarks>
        public void Show(Form owner) {
            if (showed)
                throw new Exception("ダイアログは一度表示されています。");
            showed = true;

            _canceled = false;
            startEvent = new System.Threading.ManualResetEvent(false);
            ownerForm = owner;

            //スレッドを作成
            thread = new System.Threading.Thread(
                new System.Threading.ThreadStart(Run));
            thread.IsBackground = true;
            //this.thread.ApartmentState =
            //    System.Threading.ApartmentState.STA;
            this.thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();

            //フォームが表示されるまで待機する
            startEvent.WaitOne();
        }
        public void Show() {
            Show(null);
        }

        //別スレッドで処理するメソッド
        private void Run() {
            //フォームの設定
            form = new ProgressForm();
            form.Text = _title;
            form.Button1.Click += new EventHandler(Button1_Click);
            form.Closing += new CancelEventHandler(form_Closing);
            form.Activated += new EventHandler(form_Activated);
            form.ProgressBar1.Minimum = _minimum;
            form.ProgressBar1.Maximum = _maximum;
            form.ProgressBar1.Value = _value;
            //フォームの表示位置をオーナーの中央へ
            if (ownerForm != null) {
                form.StartPosition = FormStartPosition.Manual;
                form.Left =
                    ownerForm.Left + (ownerForm.Width - form.Width) / 2;
                form.Top =
                    ownerForm.Top + (ownerForm.Height - form.Height) / 2;
            }
            //フォームの表示
            form.ShowDialog();

            form.Dispose();
        }

        /// <summary>
        /// ダイアログを閉じる
        /// </summary>
        public void Close() {
            closing = true;
            form.Invoke(new MethodInvoker(form.Close));
        }

        public void Dispose() {
            form.Invoke(new MethodInvoker(form.Dispose));
        }

        private void SetProgressValue() {
            if (form != null && !form.IsDisposed)
                form.ProgressBar1.Value = _value;
        }

        private void SetMessage() {
            if (form != null && !form.IsDisposed)
                form.Label1.Text = _message;
        }

        private void SetTitle() {
            if (form != null && !form.IsDisposed)
                form.Text = _title;
        }

        private void SetProgressMaximun() {
            if (form != null && !form.IsDisposed)
                form.ProgressBar1.Maximum = _maximum;
        }

        private void SetProgressMinimum() {
            if (form != null && !form.IsDisposed)
                form.ProgressBar1.Minimum = _minimum;
        }

        private void Button1_Click(object sender, EventArgs e) {
            _canceled = true;
        }

        private void form_Closing(object sender, CancelEventArgs e) {
            if (!closing) {
                e.Cancel = true;
                _canceled = true;
            }
        }

        private void form_Activated(object sender, EventArgs e) {
            form.Activated -= new EventHandler(form_Activated);
            startEvent.Set();
        }
    }
}