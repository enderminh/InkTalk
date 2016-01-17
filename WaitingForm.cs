using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;

namespace MinhNguyen.InkTalk
{
	/// <summary>
	/// Simple Waiting form to display while waiting for incoming connection
	/// </summary>
	public class WaitingForm : System.Windows.Forms.Form
	{
		#region Variables

		// Windows forms variable
		private System.Windows.Forms.Label waitingLabel;
		private System.Windows.Forms.Button cancel;

		// private important variables
		private Thread waitingThread = null;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		#endregion 

		#region Methods

		#region event-handlers

		/// <summary>
		/// User clicks on cancel - will abort the associated listen thread
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void cancel_Click(object sender, System.EventArgs e)
		{
			// if the listen thread is a live, we'll have to abort it
			if (waitingThread.IsAlive)
				waitingThread.Abort();
		}


		#endregion

		#region Windows-related methods

		/// <summary>
		/// Constructors
		/// </summary>
		/// <param name="listenThread">Thread that is associated with this waiting form</param>
		/// <param name="version">Version number of this program</param>
		/// <param name="displayText">Text to be displayed</param>
		public WaitingForm(Thread waitingThread, string version, string displayText)
		{
			InitializeComponent();
			this.waitingThread = waitingThread;
			this.Text = "InkTalk " + version;
			this.waitingLabel.Text = displayText;
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
			this.waitingLabel = new System.Windows.Forms.Label();
			this.cancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// waitingLabel
			// 
			this.waitingLabel.Location = new System.Drawing.Point(8, 16);
			this.waitingLabel.Name = "waitingLabel";
			this.waitingLabel.Size = new System.Drawing.Size(304, 32);
			this.waitingLabel.TabIndex = 0;
			this.waitingLabel.Text = "Waiting for incoming connection...";
			// 
			// cancel
			// 
			this.cancel.Location = new System.Drawing.Point(318, 10);
			this.cancel.Name = "cancel";
			this.cancel.TabIndex = 1;
			this.cancel.Text = "&Cancel";
			this.cancel.Click += new System.EventHandler(this.cancel_Click);
			// 
			// WaitingForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(402, 47);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.waitingLabel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "WaitingForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "InkTalk";
			this.ResumeLayout(false);

		}
		#endregion

		#endregion

		#endregion
	}
}
