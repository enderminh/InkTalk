using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;

namespace MinhNguyen.InkTalk
{
	/// <summary>
	/// Startup form that provides the user ability to choose between client or server mode
	/// </summary>
	public class StartupForm : System.Windows.Forms.Form
	{

		#region Variables

		// Windows form variables
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.GroupBox connectModeGroupBox;
		private System.Windows.Forms.Label modeLabel;
		private System.Windows.Forms.RadioButton serverRadio;
		private System.Windows.Forms.RadioButton clientRadio;
		private System.Windows.Forms.Label clientIPLabel;
		private System.Windows.Forms.TextBox clientIPBox;
		private System.Windows.Forms.Label serverIPLabel;
		private System.Windows.Forms.TextBox serverIPBox;



		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		#endregion

		#region Properties

		/// <summary>
		/// Gets whether the client mode has been chosen or not
		/// </summary>
		public bool IsClientMode 
		{
			get 
			{
				return clientRadio.Checked;
			}
		}


		/// <summary>
		/// Gets the specified IP address for client
		/// </summary>
		public IPAddress ClientIP
		{
			get 
			{
				try 
				{
					return IPAddress.Parse(clientIPBox.Text);
				} 
				catch (FormatException) 
				{
					return IPAddress.Loopback;
				}
			}
		}


		/// <summary>
		/// Gets IP address of server
		/// </summary>
		public IPAddress ServerIP
		{
			get 
			{
				try 
				{
					return IPAddress.Parse(serverIPBox.Text);
				} 
				catch (FormatException) 
				{
					return IPAddress.Loopback;
				}
			}
		}


		#endregion

		#region Methods

		#region important event-handlers

		/// <summary>
		/// Form loads - puts focus to the client IP textbox
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void StartupForm_Load(object sender, System.EventArgs e)
		{
			clientIPBox.Focus();
		}


		/// <summary>
		/// User clicks OK, closes form with an OK return value
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void okButton_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}


		/// <summary>
		/// User clicks on cancel - closes the form with Cancel return value
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void cancelButton_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}


		/// <summary>
		/// User selects client radio button - enables the client IP textbox
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void clientRadio_CheckedChanged(object sender, System.EventArgs e)
		{
			clientIPBox.Focus();
			clientIPBox.Enabled = true;
			serverIPBox.Enabled = false;
		}


		/// <summary>
		/// User selects server radio button - enables the server IP textbox
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void serverRadio_CheckedChanged(object sender, System.EventArgs e)
		{
			serverIPBox.Focus();
			clientIPBox.Enabled = false;
			serverIPBox.Enabled = true;
		}


		#endregion

		#region Windows-related methods

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="version">Version of this program</param>
		/// <param name="IP">Default IP address to be placed in the textboxes</param>
		public StartupForm(string version, string IP)
		{
			InitializeComponent();
			this.Text = "InkTalk " + version;		
			this.clientIPBox.Text = IP;
			this.serverIPBox.Text = IP;
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
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.connectModeGroupBox = new System.Windows.Forms.GroupBox();
			this.serverIPLabel = new System.Windows.Forms.Label();
			this.serverIPBox = new System.Windows.Forms.TextBox();
			this.modeLabel = new System.Windows.Forms.Label();
			this.clientIPLabel = new System.Windows.Forms.Label();
			this.clientIPBox = new System.Windows.Forms.TextBox();
			this.serverRadio = new System.Windows.Forms.RadioButton();
			this.clientRadio = new System.Windows.Forms.RadioButton();
			this.connectModeGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Location = new System.Drawing.Point(232, 232);
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 5;
			this.okButton.Text = "&OK";
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Location = new System.Drawing.Point(312, 232);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 6;
			this.cancelButton.Text = "&Cancel";
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// connectModeGroupBox
			// 
			this.connectModeGroupBox.Controls.Add(this.serverIPLabel);
			this.connectModeGroupBox.Controls.Add(this.serverIPBox);
			this.connectModeGroupBox.Controls.Add(this.modeLabel);
			this.connectModeGroupBox.Controls.Add(this.clientIPLabel);
			this.connectModeGroupBox.Controls.Add(this.clientIPBox);
			this.connectModeGroupBox.Controls.Add(this.serverRadio);
			this.connectModeGroupBox.Controls.Add(this.clientRadio);
			this.connectModeGroupBox.Location = new System.Drawing.Point(-16, -24);
			this.connectModeGroupBox.Name = "connectModeGroupBox";
			this.connectModeGroupBox.Size = new System.Drawing.Size(552, 248);
			this.connectModeGroupBox.TabIndex = 7;
			this.connectModeGroupBox.TabStop = false;
			this.connectModeGroupBox.Text = "Connect Mode";
			// 
			// serverIPLabel
			// 
			this.serverIPLabel.Location = new System.Drawing.Point(96, 200);
			this.serverIPLabel.Name = "serverIPLabel";
			this.serverIPLabel.Size = new System.Drawing.Size(64, 14);
			this.serverIPLabel.TabIndex = 11;
			this.serverIPLabel.Text = "IP Address";
			// 
			// serverIPBox
			// 
			this.serverIPBox.Enabled = false;
			this.serverIPBox.Location = new System.Drawing.Point(160, 200);
			this.serverIPBox.Name = "serverIPBox";
			this.serverIPBox.Size = new System.Drawing.Size(104, 20);
			this.serverIPBox.TabIndex = 10;
			this.serverIPBox.Text = "127.0.0.1";
			// 
			// modeLabel
			// 
			this.modeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.modeLabel.Location = new System.Drawing.Point(48, 40);
			this.modeLabel.Name = "modeLabel";
			this.modeLabel.Size = new System.Drawing.Size(272, 23);
			this.modeLabel.TabIndex = 9;
			this.modeLabel.Text = "InkTalk Connect Mode";
			// 
			// clientIPLabel
			// 
			this.clientIPLabel.Location = new System.Drawing.Point(96, 120);
			this.clientIPLabel.Name = "clientIPLabel";
			this.clientIPLabel.Size = new System.Drawing.Size(64, 14);
			this.clientIPLabel.TabIndex = 8;
			this.clientIPLabel.Text = "IP Address";
			// 
			// clientIPBox
			// 
			this.clientIPBox.Location = new System.Drawing.Point(160, 120);
			this.clientIPBox.Name = "clientIPBox";
			this.clientIPBox.Size = new System.Drawing.Size(104, 20);
			this.clientIPBox.TabIndex = 7;
			this.clientIPBox.Text = "127.0.0.1";
			// 
			// serverRadio
			// 
			this.serverRadio.Location = new System.Drawing.Point(72, 168);
			this.serverRadio.Name = "serverRadio";
			this.serverRadio.Size = new System.Drawing.Size(264, 24);
			this.serverRadio.TabIndex = 6;
			this.serverRadio.Text = "I am expecting an InkTalk client (Server Mode)";
			this.serverRadio.CheckedChanged += new System.EventHandler(this.serverRadio_CheckedChanged);
			// 
			// clientRadio
			// 
			this.clientRadio.Checked = true;
			this.clientRadio.Location = new System.Drawing.Point(72, 88);
			this.clientRadio.Name = "clientRadio";
			this.clientRadio.Size = new System.Drawing.Size(312, 24);
			this.clientRadio.TabIndex = 5;
			this.clientRadio.TabStop = true;
			this.clientRadio.Text = "I want to connect to another InkTalk client (Client Mode)";
			this.clientRadio.CheckedChanged += new System.EventHandler(this.clientRadio_CheckedChanged);
			// 
			// StartupForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(394, 263);
			this.Controls.Add(this.connectModeGroupBox);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "StartupForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "InkTalk Connect";
			this.Load += new System.EventHandler(this.StartupForm_Load);
			this.connectModeGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion


		#endregion

		#endregion
	}
}
