using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;

namespace MinhNguyen.InkTalk
{
	/// <summary>
	/// Form that displays the contents of the readme file
	/// </summary>
	public class ContentsForm : System.Windows.Forms.Form
	{

		#region Variables

		// Windows forms variables
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.TextBox contentsBox;


		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		#endregion

		#region Methods

		#region event-handlers

		/// <summary>
		/// Form loads - reads in the readme.txt and displays it in the textbox
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ContentsForm_Load(object sender, System.EventArgs e)
		{
			if (File.Exists("readme.txt")) 



				// read in readme file
				using(TextReader reader = File.OpenText("readme.txt")) 
				{
					contentsBox.Text = reader.ReadToEnd();
					contentsBox.Select(0,0);
				}
			else
				contentsBox.Text = "Readme.txt file not found. Please re-download InkTalk";
			
		}


		/// <summary>
		/// User clicks on OK - simply closes this form
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void okButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}



		#endregion

		#region Windows-related methods


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="version">Version number of this program</param>
		public ContentsForm(string version)
		{
			InitializeComponent();

			this.Text = "InkTalk " + version;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
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
			this.contentsBox = new System.Windows.Forms.TextBox();
			this.okButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// contentsBox
			// 
			this.contentsBox.BackColor = System.Drawing.Color.White;
			this.contentsBox.Location = new System.Drawing.Point(8, 8);
			this.contentsBox.Multiline = true;
			this.contentsBox.Name = "contentsBox";
			this.contentsBox.ReadOnly = true;
			this.contentsBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.contentsBox.Size = new System.Drawing.Size(456, 320);
			this.contentsBox.TabIndex = 0;
			this.contentsBox.Text = "";
			// 
			// okButton
			// 
			this.okButton.Location = new System.Drawing.Point(384, 336);
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 1;
			this.okButton.Text = "&OK";
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// ContentsForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(472, 365);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.contentsBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ContentsForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "InkTalk";
			this.Load += new System.EventHandler(this.ContentsForm_Load);
			this.ResumeLayout(false);

		}
		#endregion

		#endregion

		#endregion
	}
}
