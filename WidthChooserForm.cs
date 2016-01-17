using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace MinhNguyen.InkTalk
{
	/// <summary>
	/// Summary description for WidthChooserForm.
	/// </summary>
	public class WidthChooserForm : System.Windows.Forms.Form
	{
		
		#region Variables
		
		// Windows forms variables
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.TrackBar widthTrack;
		private Pen blackPen = new Pen(Color.Black,1);
		private System.Windows.Forms.Label smallLabel;
		private System.Windows.Forms.Label largeLabel;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the ink width used for the tracker
		/// </summary>
		public float InkWidth 
		{
			get 
			{
				return widthTrack.Value;
			}
			set 
			{
				if (value < 1)
					widthTrack.Value = 1;
				else if (value > widthTrack.Maximum)
					widthTrack.Value = widthTrack.Maximum;
				else 
					widthTrack.Value = Convert.ToInt32(value);
			}
		}

		#endregion

		#region Methods

		#region event-handlers

		/// <summary>
		/// User clicks on cancel - closes window with cancel result
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void cancelButton_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}


		/// <summary>
		/// User clicks on OK - closes window with OK result
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void okButton_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		#endregion

		#region Windows-related methods

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="version">Version of this program</param>
		public WidthChooserForm(string version)
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
			this.widthTrack = new System.Windows.Forms.TrackBar();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.smallLabel = new System.Windows.Forms.Label();
			this.largeLabel = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.widthTrack)).BeginInit();
			this.SuspendLayout();
			// 
			// widthTrack
			// 
			this.widthTrack.Location = new System.Drawing.Point(8, 32);
			this.widthTrack.Maximum = 300;
			this.widthTrack.Minimum = 1;
			this.widthTrack.Name = "widthTrack";
			this.widthTrack.Size = new System.Drawing.Size(248, 42);
			this.widthTrack.TabIndex = 0;
			this.widthTrack.TickFrequency = 15;
			this.widthTrack.Value = 1;
			// 
			// okButton
			// 
			this.okButton.Location = new System.Drawing.Point(96, 80);
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 1;
			this.okButton.Text = "&OK";
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Location = new System.Drawing.Point(176, 80);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 2;
			this.cancelButton.Text = "&Cancel";
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// smallLabel
			// 
			this.smallLabel.Location = new System.Drawing.Point(8, 8);
			this.smallLabel.Name = "smallLabel";
			this.smallLabel.Size = new System.Drawing.Size(64, 23);
			this.smallLabel.TabIndex = 3;
			this.smallLabel.Text = "Small";
			// 
			// largeLabel
			// 
			this.largeLabel.Location = new System.Drawing.Point(200, 8);
			this.largeLabel.Name = "largeLabel";
			this.largeLabel.Size = new System.Drawing.Size(56, 23);
			this.largeLabel.TabIndex = 4;
			this.largeLabel.Text = "Large";
			this.largeLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// WidthChooserForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(264, 111);
			this.Controls.Add(this.largeLabel);
			this.Controls.Add(this.smallLabel);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.widthTrack);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "WidthChooserForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "InkTalk";
			((System.ComponentModel.ISupportInitialize)(this.widthTrack)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion




		#endregion

		#endregion
	}
}
