// André Betz
// http://www.andrebetz.de
using System;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using System.Data;
using System.IO;

namespace ErrorCorrection
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class ECCGui : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private ECC m_Ecc = new ECC(3);
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ECCGui()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(16, 16);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(112, 23);
			this.button1.TabIndex = 0;
			this.button1.Text = "ECC generate";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(152, 16);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(112, 23);
			this.button2.TabIndex = 1;
			this.button2.Text = "ECC Correct";
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// ECCGui
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(282, 63);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ECCGui";
			this.Text = "ErrorCorrectionCode  www.AndreBetz.de";
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new ECCGui());
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			OpenFileDialog opfd = new OpenFileDialog();
			opfd.Filter = "Binary File (*.*)|*.*" ;
			opfd.FilterIndex = 1 ;
			opfd.InitialDirectory = Application.StartupPath;
			opfd.RestoreDirectory = false ;
			opfd.Title = "Load binary File";
			DialogResult res = opfd.ShowDialog(this);
			if(res==DialogResult.OK)
			{
				string DatafileName = opfd.FileName;
				m_Ecc.Encode(DatafileName);
			}
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			OpenFileDialog opfd = new OpenFileDialog();
			opfd.Filter = "Binary File (*.ecc)|*.ecc" ;
			opfd.FilterIndex = 1 ;
			opfd.InitialDirectory = Application.StartupPath;
			opfd.RestoreDirectory = false ;
			opfd.Title = "Load ECC File";
			DialogResult res = opfd.ShowDialog(this);
			if(res==DialogResult.OK)
			{
				string DatafileName = opfd.FileName;
				m_Ecc.Decode(DatafileName);
			}		
		}
	}
}
