using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using System.Data;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using System.IO;
using Microsoft.Ink;

/*
  
 InkTalk 1.1
 ==================

 by Minh T. Nguyen
 minh@minh.org
 http://www.enderminh.com

*/

namespace MinhNguyen.InkTalk
{
	/// <summary>
	/// Main form for InkTalk
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
	{
		#region Variables/Delegates

		// delegates
		delegate void AddInkMessageDelegate(Ink ink);
		delegate void SetMainFormVisibilityDelegate(bool visible);
		delegate void SetStatusBarTextDelegate(string message);
		delegate void SetSendButtonStateDelegate(bool enable);

		#region private variables

		// ink-related controls/variables
		private Microsoft.Ink.InkCollector inputInkCollector = null;
		private Microsoft.Ink.InkOverlay chatInkOverlay = null;
		private System.Drawing.Drawing2D.Matrix unmodifiedMatrix = null;		

		// forms
		private StartupForm startupForm = null;
		private WaitingForm waitingForm = null;		

		// network variables
		private StreamReader inputStream = null;
		private StreamWriter outputStream = null;
		private TcpClient tcpClient = null;

		// Threads
		private Thread listenForMessagesThread = null;
		private Thread listenForConnectionThread = null;
		private Thread connectingToClientThread = null;

		// delegates
		private AddInkMessageDelegate addInkMessageDelegate = null;
		private SetMainFormVisibilityDelegate setMainFormVisibilityDelegate = null;
		private SetStatusBarTextDelegate setStatusBarTextDelegate = null;
		private SetSendButtonStateDelegate setSendButtonStateDelegate = null;
		
		// other private variables
		private StringBuilder recognizedString = new StringBuilder();
		private string version;
		private string connectionIPAddress;
		private int connectionPort;
		private bool bSupportsTextRecognition = true;
		private bool bFirstStartup = true;

		#endregion

		#region Windows forms variables

		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.VScrollBar chatVScroll;
		private System.Windows.Forms.HScrollBar chatHScroll;
		private System.Windows.Forms.TextBox inputBox;
		private System.Windows.Forms.Button sendButton;
		private System.Windows.Forms.Splitter horizontalSplitter;
		private System.Windows.Forms.Panel bottomPanel;
		private System.Windows.Forms.Panel chatPanel;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem hyphenMenu;
		private System.Windows.Forms.ColorDialog colorDialog1;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.Button colorButton;
		private System.Windows.Forms.Button widthButton;
		private System.Windows.Forms.MenuItem saveAsMenu;
		private System.Windows.Forms.MenuItem exitMenu;
		private System.Windows.Forms.MenuItem helpMenu;
		private System.Windows.Forms.MenuItem aboutMenu;
		private System.Windows.Forms.MenuItem connectMenu;
		private System.Windows.Forms.MenuItem copyMenu;
		private System.Windows.Forms.MenuItem clearChat;
		private System.Windows.Forms.MenuItem clearInput;
		private System.Windows.Forms.MenuItem editMenu;
		private System.Windows.Forms.MenuItem pasteMenu;
		private System.Windows.Forms.MenuItem hyphenMenu3;
		private System.Windows.Forms.StatusBar mainStatusBar;
		private System.Windows.Forms.MenuItem contentsMenu;
		
		#endregion

		#endregion

		#region Methods

		#region Important business logic related methods

		/// <summary>
		/// Main form loads - sets up UI for drawing and starts connection wizard
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MainForm_Load(object sender, System.EventArgs e)
		{
			int iPenWidth = 1;

			// obtain and show version
			version = Application.ProductVersion;
			version = version.Substring(0,version.LastIndexOf("."));
			version = version.Substring(0,version.LastIndexOf("."));
			this.Text = "InkTalk " + version;

			try 
			{
				// read configuration file variables
				connectionPort = Convert.ToInt32(ConfigurationSettings.AppSettings["Port"]);
				connectionIPAddress = ConfigurationSettings.AppSettings["IP"];
				iPenWidth = Convert.ToInt32(ConfigurationSettings.AppSettings["PenWidth"]);
			} 
			catch (Exception ex) 
			{
				MessageBox.Show(this,"Missing or invalid configuration file: " + Environment.NewLine + ex.Message,"InkTalk " + version, MessageBoxButtons.OK, MessageBoxIcon.Error);
				Application.Exit();
			}

			// create start up form
			startupForm = new StartupForm(version, connectionIPAddress);
			
			// set delegate
			addInkMessageDelegate = new AddInkMessageDelegate(addInkMessage);
			setMainFormVisibilityDelegate = new SetMainFormVisibilityDelegate(setMainFormVisibility);
			setStatusBarTextDelegate = new SetStatusBarTextDelegate(setStatusBarText);
			setSendButtonStateDelegate = new SetSendButtonStateDelegate(setSendButtonState);

			// set up input ink collector
			inputInkCollector = new InkCollector(inputBox.Handle);	
			inputInkCollector.DefaultDrawingAttributes.Width = iPenWidth;
			inputInkCollector.Enabled = true;
			
			// set up chat ink output
			chatInkOverlay = new InkOverlay(chatPanel.Handle);
			chatInkOverlay.Renderer.GetViewTransform(ref unmodifiedMatrix);
			chatInkOverlay.EditingMode = InkOverlayEditingMode.Select;
			chatInkOverlay.Enabled = true;

			// start connection wizard
			newConnectionWizard();
		}


		/// <summary>
		/// User closes window - aborts all running threads
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try 
			{
				// close input stream
				if (inputStream != null)
					inputStream.Close();

				// close output stream
				if (outputStream != null)
					outputStream.Close();

				// close socket
				if (tcpClient != null)
					tcpClient.Close();

				// terminate all threads
				terminateAllRunningThreads();
			} 
			catch (Exception ex) 
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
		}

		/// <summary>
		/// Aborts all running threads
		/// </summary>
		private void terminateAllRunningThreads() 
		{
			// in case threads haven't terminated, close them here
			if ((listenForMessagesThread != null) && (listenForMessagesThread.IsAlive)) 
			{
				listenForMessagesThread.Abort();
				listenForMessagesThread.Join(3000);
			}
			
			// in case threads haven't terminated, close them here
			if ((listenForConnectionThread != null) && (listenForConnectionThread.IsAlive)) 
			{
				listenForConnectionThread.Abort();
				listenForConnectionThread.Join(3000);
			}

			// in case threads haven't terminated, close them here
			if ((connectingToClientThread != null) && (connectingToClientThread.IsAlive)) 
			{
				connectingToClientThread.Abort();
				connectingToClientThread.Join(3000);
			}
		}

		/// <summary>
		/// User clicks on the send button - sends ink message to other client and to yourself
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void sendButton_Click(object sender, System.EventArgs e)
		{
			if (inputInkCollector.Ink.Strokes.Count > 0) 
			{
				// add message to your own chat
				addInkMessage(inputInkCollector.Ink);

				// send ink to the other client
				sendInkMessage(inputInkCollector.Ink);

				// delete input ink
				inputInkCollector.Ink.DeleteStrokes();
				inputBox.Invalidate();
				inputBox.Clear();
			}
		}

		/// <summary>
		/// User clicks on width button - displays width chooser
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void widthButton_Click(object sender, System.EventArgs e)
		{
			// set up window chooser form
			WidthChooserForm widthChooserForm = new WidthChooserForm(version);

			// set up width
			widthChooserForm.InkWidth = inputInkCollector.DefaultDrawingAttributes.Width;

			// show width dialog
			if (widthChooserForm.ShowDialog(this) == DialogResult.OK)
				inputInkCollector.DefaultDrawingAttributes.Width = widthChooserForm.InkWidth;

			// close window
			widthChooserForm.Close();
			widthChooserForm.Dispose();
		}


		/// <summary>
		/// Adds a single ink message to the main chat ink window
		/// </summary>
		/// <param name="ink">Ink to be added to the chat ink window</param>
		private void addInkMessage(Ink ink) 
		{
			// calculate the bounds of the input and current chat panel
			Rectangle chatBounds = chatInkOverlay.Ink.GetBoundingBox();
			Rectangle inputBounds = ink.GetBoundingBox();

			int yPosition = chatBounds.Y + chatBounds.Height;

			// let's start drawing a little bit further down on the first ink
			if (yPosition == 0)
				yPosition = 20;

			try 
			{
				// add strokes from the input message to the chat panel
				if (ink.Strokes.Count > 0)
					chatInkOverlay.Ink.AddStrokesAtRectangle(ink.Strokes, new Rectangle(inputBounds.X, yPosition,inputBounds.Width, inputBounds.Height));

				// redraw chat panel
				redrawChatPanel(true);

				if (bSupportsTextRecognition) 
				{
					try 
					{
						// try recognizing string
						string sRecognizedString = ink.Strokes.ToString();

						// append string
						if (sRecognizedString != string.Empty)
							recognizedString.Append(sRecognizedString + Environment.NewLine);
					} 
					catch (Exception) 
					{
						bSupportsTextRecognition = false;
					}
				}
			} 
			catch (Exception ex) 
			{
				this.Invoke(setStatusBarTextDelegate, new object[] { "Error: " + ex.Message });
			}
		}

		
		#region Connection-related methods
		
		/// <summary>
		/// Starts a new connection wizard
		/// </summary>
		private void newConnectionWizard() 
		{
			// ask for connection mode
			if (startupForm.ShowDialog(this) == DialogResult.OK) 
			{
				// not first start up anymore
				bFirstStartup = false;

				// kill all running threads
				terminateAllRunningThreads();

				if (startupForm.IsClientMode) 
				{
					// connect as a client
					connectAsClient();
				} 
				else 
				{
					// listen as a server
					listenAsServer();
				}
			} 
			else if (bFirstStartup) 
			{
				// kill all running threads
				terminateAllRunningThreads();

				// close program
				Application.Exit();
			} 
		}


		/// <summary>
		/// Connects to a server as a client
		/// </summary>
		private void connectAsClient() 
		{
			// prepare listen thread
			connectingToClientThread = new Thread(new ThreadStart(connectingToClient));
			connectingToClientThread.Name = "ConnectingToClient";

			// prepare "Waiting..." form
			waitingForm = new WaitingForm(listenForConnectionThread, version,"Connecting to " + startupForm.ClientIP + ":" + connectionPort + "...");

			// show Waiting form
			waitingForm.TopLevel = true;
			waitingForm.Show();
			
			// hide main window
			this.Hide();

			// start to attempt to connect
			connectingToClientThread.Start();
		}


		/// <summary>
		/// Attempts to connect to client
		/// </summary>
		private void connectingToClient() 
		{
			tcpClient = null;

			// hide main form
			this.Invoke(setMainFormVisibilityDelegate, new object[] { false });

			try 
			{
				// attempt to connect
				tcpClient = new TcpClient(startupForm.ClientIP.ToString(), connectionPort);

				// set up IO streams
				NetworkStream oSocketStream = tcpClient.GetStream();
				outputStream = new StreamWriter(oSocketStream); 
				inputStream = new StreamReader(oSocketStream);	
				outputStream.AutoFlush = true;

				// enable send button
				this.Invoke(setSendButtonStateDelegate, new object[] {true});
				this.Invoke(setStatusBarTextDelegate, new object[] {"Connection established."});

				// start to listen to incoming messages
				listenForMessagesThread = new Thread(new ThreadStart(listenToIncomingMessages));
				listenForMessagesThread.Name = "ListenForMessages";
				listenForMessagesThread.Start();
			} 
			catch (ThreadAbortException) 
			{
				this.Invoke(setStatusBarTextDelegate, new object[] {"Connecting process was aborted."});
			} 
			catch (SocketException) 
			{
				this.Invoke(setStatusBarTextDelegate, new object[] {"Connection to client could not have been established."});
			} 
			finally 
			{
				// close the waiting form if necessary
				if (waitingForm != null) 
				{
					waitingForm.Close();
					waitingForm.Dispose();
					waitingForm = null;
				}

				// show main form
				this.Invoke(setMainFormVisibilityDelegate, new object[] { true });
			}
		}


		/// <summary>
		/// Listens as a server for incoming connection
		/// </summary>
		private void listenAsServer() 
		{
			tcpClient = null;

			// prepare listen thread
			listenForConnectionThread = new Thread(new ThreadStart(listenToIncomingConnection));
			listenForConnectionThread.Name = "ListenForConnection";

			// prepare "Waiting..." form
			waitingForm = new WaitingForm(listenForConnectionThread, version,"Waiting for incoming connection on " + startupForm.ServerIP.ToString() + ":" + connectionPort + " ...");

			// show Waiting form
			waitingForm.TopLevel = true;
			waitingForm.Show();
			
			// hide main window
			this.Hide();

			// start listen for connection thread
			listenForConnectionThread.Start();
		}


		/// <summary>
		/// Listens for one incoming connection
		/// </summary>
		private void listenToIncomingConnection()
		{

			// hide main form
			this.Invoke(setMainFormVisibilityDelegate, new object[] { false });

			try 
			{

				// sets up socket listener
				TcpListener listener = new TcpListener(startupForm.ServerIP,connectionPort);

				// starts to listen to specified port
				listener.Start();

				// wait until there is an incoming connection
				
				// I usually don't do this "busy-loop", but I wanted to be
				// responsive to the Cancel button, so I need to sleep this thread
				// every once in a while
				while (!listener.Pending()) 
					System.Threading.Thread.Sleep(1000);

				// accept the server connection
				tcpClient = listener.AcceptTcpClient();
				
				// set up IO streams
				outputStream = new StreamWriter(tcpClient.GetStream());
				inputStream = new StreamReader(tcpClient.GetStream());
				outputStream.AutoFlush = true;

				// enable send button
				this.Invoke(setSendButtonStateDelegate, new object[] { true });

				// start to listen to incoming messages
				listenForMessagesThread = new Thread(new ThreadStart(listenToIncomingMessages));
				listenForMessagesThread.Start();
			} 
			catch (ThreadAbortException) 
			{
				this.Invoke(setStatusBarTextDelegate, new object[] {"Listening process was aborted."});
			} 
			finally 
			{
				// close the waiting form if necessary
				if (waitingForm != null) 
				{
					waitingForm.Close();
					waitingForm.Dispose();
					waitingForm = null;
				}

				// show main form
				this.Invoke(setMainFormVisibilityDelegate, new object[] { true });
			}
		}


		/// <summary>
		/// Listens to incoming messages and adds them to chat panel
		/// </summary>
		private void listenToIncomingMessages() 
		{
			string message = null;
			string sErrorMessage = "Unknown";

			// we are connected at this point
			this.Invoke(setStatusBarTextDelegate, new object[] {"Connection established."});

			do 
			{
				try 
				{
					// reads in a single Base64 line of a single ink
					message = inputStream.ReadLine();

					// see if connection was closed
					if (message == null)
						break;

					// create new incomingInk
					Ink incomingInk = new Ink();

					// load data from base64 string
					incomingInk.Load(System.Text.Encoding.UTF8.GetBytes(message));

					// add deserialized ink to chat window
					this.Invoke(addInkMessageDelegate,new object[] {incomingInk});
				} 
				catch (Exception ex) 
				{
					sErrorMessage = ex.Message;
					break;
				}					
			} while (message != null);

			// disable button
			this.Invoke(setStatusBarTextDelegate, new object[] {"Connection lost: " + sErrorMessage});
			this.Invoke(setSendButtonStateDelegate, new object[] { false });
		}


		/// <summary>
		/// Sends a single ink to the other client
		/// </summary>
		/// <param name="ink">Ink to be sent</param>
		private void sendInkMessage(Ink ink) 
		{
			byte[] base64ISF_bytes;
			string base64ISF_string;

			if (outputStream != null) 
			{

				#region serialize ink into base64 string

				// The following code snippet was taken from the Microsoft Tablet PC
				// Ink Serialization code example provided in the Table PC Developer SDK 1.5

				// Get the base64 encoded ISF
				base64ISF_bytes = ink.Save(PersistenceFormat.Base64InkSerializedFormat);

				// Ink.Save returns a null terminated byte array. The encoding of the null
				// character generates a control sequence when it is UTF8 encoded. This
				// sequence is invalid in XML. Therefore, it is necessary to remove the 
				// null character before UTF8 encoding the array.
				// The following loop finds the index of the first non-null byte in the byte 
				// array returned by the Ink.Save method.
				int countOfBytesToConvertIntoString = base64ISF_bytes.Length - 1;
				for(; countOfBytesToConvertIntoString >= 0; --countOfBytesToConvertIntoString)
				{
					// Break the loop if the byte at the index is non-null.
					if(0 != base64ISF_bytes[countOfBytesToConvertIntoString])
						break;
				}

				// Convert the index into count by incrementing it.
				countOfBytesToConvertIntoString++;

				// Convert it to a String
				base64ISF_string = System.Text.Encoding.UTF8.GetString(base64ISF_bytes, 0, countOfBytesToConvertIntoString);

				#endregion

				// send base64 string
				outputStream.WriteLine(base64ISF_string);	
			}
		}
		

		#endregion

		#region drawing-related methods

		/// <summary>
		/// Redraws the chat panel including ink and scrollbars
		/// </summary>
		/// <param name="scrollToBottom">If set to true, will scroll to the bottom</param>
		private void redrawChatPanel(bool scrollToBottom) 
		{
			// find out the bounds of the current inks in the chat
			Rectangle chatBounds = chatInkOverlay.Ink.GetBoundingBox();

			// update the vertical scroll bar
			displayScrollBars(chatBounds, scrollToBottom);

			// reset to ink overlay back to the original coordinates
			chatInkOverlay.Renderer.SetViewTransform(unmodifiedMatrix);

			// calculates the ratio of where we are currently located
			double xRatio = (chatHScroll.Value * 1.0/(chatHScroll.Maximum));
			double yRatio = (chatVScroll.Value * 1.0/(chatVScroll.Maximum));

			// offset the ink overlay by the amount specified by the scroll bars
			chatInkOverlay.Renderer.Move(0-Convert.ToInt64((chatBounds.Width)* xRatio), 0- Convert.ToInt32((chatBounds.Height) * yRatio));

			// force paint-event
			chatPanel.Invalidate();
		}


		/// <summary>
		/// Redisplays the scroll bards based on the dimension of the chat ink strokes
		/// and the actual chat panel
		/// </summary>
		/// <param name="chatBounds">Bounds of the chat panel</param>
		/// <param name="scrollToBottom">If set to true will scroll to the bottom</param>
		private void displayScrollBars(Rectangle chatBounds, bool scrollToBottom) 
		{	
			int iLeft, iTop, iWidth, iHeight;

			// convert coordinates from ink-space to pixel-space
			getBoundsFromInk(out iLeft, out iTop, out iWidth, out iHeight, chatInkOverlay);

			// display vertical scroll bar if necessary
			if (iHeight > chatPanel.ClientSize.Height) 
			{
				chatVScroll.Visible = true;
				chatVScroll.LargeChange = chatPanel.ClientSize.Height;
				chatVScroll.Maximum = iHeight + 50;

				// scroll to the bottom if necessary
				if (scrollToBottom)
					chatVScroll.Value = chatVScroll.Maximum - chatVScroll.LargeChange;
			}
			else
				chatVScroll.Visible = false;

			// display horizontal scroll bar if necessary
			if (iWidth > chatPanel.ClientSize.Width) 
			{
				chatHScroll.Visible = true;
				chatHScroll.LargeChange = chatPanel.ClientSize.Width;
				chatHScroll.Maximum = iWidth + 100;
			} else
				chatHScroll.Visible = false;
		}
		

		/// <summary>
		/// User moves vertical scroll bar - repaint chat window
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void chatVScroll_Scroll(object sender, System.Windows.Forms.ScrollEventArgs e)
		{
			redrawChatPanel(false);
		}


		/// <summary>
		/// User moves horizontal scroll back - repaint chat window
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void chatHScroll_Scroll(object sender, System.Windows.Forms.ScrollEventArgs e)
		{
			redrawChatPanel(false);
		}


		/// <summary>
		/// User clicks on the color button - shows ColorChooser
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void colorButton_Click(object sender, System.EventArgs e)
		{
			// displays normal color chooser
			if (colorDialog1.ShowDialog(this) == DialogResult.OK) 
			{
				// save color into the button
				colorButton.BackColor = colorDialog1.Color;

				// and the current pen
				inputInkCollector.DefaultDrawingAttributes.Color = colorDialog1.Color;

				// lose focus (otherwise we have the selection in the button which looks ugly)
				inputBox.Focus();
			}
		}


		#endregion

		#region menu-events

		/// <summary>
		/// User clicks on connect menu - starts new connection wizard
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void connectMenu_Click(object sender, System.EventArgs e)
		{
			// starts new connection wizard
			newConnectionWizard();
		}


		/// <summary>
		/// User clicks on save - allows user to save current chat
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void saveAsMenu_Click(object sender, System.EventArgs e)
		{
			// clear save file name
			saveFileDialog1.FileName = string.Empty;

			// display save as text option only if you have real text data
			if (recognizedString.Length != 0)
				saveFileDialog1.Filter = "Gif File (*.gif)|*.gif|Text File (*.txt)|*.txt";
			else
				saveFileDialog1.Filter = "Gif File (*.gif)|*.gif";

			// show save as dialog
			if (saveFileDialog1.ShowDialog(this) == DialogResult.OK) 
			{
				if (saveFileDialog1.FilterIndex == 1) 
				{
					 byte[] gifBytes;

					// save gif
					using (FileStream gifFile = File.OpenWrite(saveFileDialog1.FileName))
					{
				    
						// Generate the fortified GIF represenation of the ink
						gifBytes = chatInkOverlay.Ink.Save(PersistenceFormat.Gif);

						// Write and close the gif file
						gifFile.Write(gifBytes, 0, gifBytes.Length);
					}
				} 
				else 
				{
					// save text
					using(TextWriter tw = File.CreateText(saveFileDialog1.FileName))
						tw.WriteLine(recognizedString.ToString());
				}
			}
		}

		/// <summary>
		/// User clicks on clear chat - clears the entire chat window
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void clearChat_Click(object sender, System.EventArgs e)
		{
			// clear all strokes
			chatInkOverlay.Ink.DeleteStrokes();

			// redraws chat
			redrawChatPanel(false);
		}


		/// <summary>
		/// User clicks on clear input - clears the input window
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void clearInput_Click(object sender, System.EventArgs e)
		{
			// clear all strokes
			inputInkCollector.Ink.DeleteStrokes();

			// refresh input screen
			inputBox.Invalidate();
		}

		/// <summary>
		/// User clicks on copy menu item - copies selected/all ink to clipboard
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void copyMenu_Click(object sender, System.EventArgs e)
		{
			InkClipboardFormats format = InkClipboardFormats.Bitmap
				| InkClipboardFormats.EnhancedMetafile
				| InkClipboardFormats.InkSerializedFormat
				| InkClipboardFormats.Metafile
				| InkClipboardFormats.TextInk;

			// copies either selected or all
			if (chatInkOverlay.Selection.Count > 0)
				chatInkOverlay.Ink.ClipboardCopy(chatInkOverlay.Selection, format,InkClipboardModes.Copy);
		}

		/// <summary>
		/// User clicks on edit menu - enables/disables copy/paste menu items
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void editMenu_Popup(object sender, System.EventArgs e)
		{
			pasteMenu.Enabled = inputInkCollector.Ink.CanPaste();
			copyMenu.Enabled = chatInkOverlay.Selection.Count > 0;
		}
		

		/// <summary>
		/// User clicks on the paste menu item - pastes ink to the input window
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void pasteMenu_Click(object sender, System.EventArgs e)
		{
			if (inputInkCollector.Ink.CanPaste()) 
			{
				
				// paste into input in the middle
				inputInkCollector.Ink.ClipboardPaste(new Point(30,30));

				// refresh input panel
				inputBox.Invalidate();
			}
		}



		/// <summary>
		/// User clicks on exit - closes the program
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void exitMenu_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}


		/// <summary>
		/// User clicks on the contents menu item - displays the readme file
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void contentsMenu_Click(object sender, System.EventArgs e)
		{
			(new ContentsForm(version)).ShowDialog(this);
		}


		/// <summary>
		/// User clicks on the about menu item - shows author information
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void aboutMenu_Click(object sender, System.EventArgs e)
		{
			MessageBox.Show(this, "InkTalk " + version + " written by Minh T. Nguyen (nguyentriminh@yahoo.com)." + Environment.NewLine + Environment.NewLine
				+ "http://www.enderminh.com/minh/inktalk.aspx","InkTalk " + version, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}


		#endregion

		#region general UI-related methods 

		/// <summary>
		/// User resizes the form - will adjust controls dimensions
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MainForm_Resize(object sender, System.EventArgs e)
		{
			adjustControlSizes();
		}


		/// <summary>
		/// User moves the horizontal splitter - will adjust controls dimensions
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void horizontalSplitter_SplitterMoved(object sender, System.Windows.Forms.SplitterEventArgs e)
		{
			adjustControlSizes();
		}


		/// <summary>
		/// Adjusts the controls dimension based on the main window's dimension
		/// </summary>
		private void adjustControlSizes() 
		{
			inputBox.Width = chatPanel.Width - 95;
			inputBox.Height = this.Height - horizontalSplitter.Top - 86;
			
			sendButton.Left = chatPanel.Width - 82;
			sendButton.Height = this.Height - horizontalSplitter.Top - 125;

			colorButton.Left = chatPanel.Width - 82;
			widthButton.Left = chatPanel.Width - 40;
			
		}


		/// <summary>
		/// Sets this main form to be visible or invisible
		/// </summary>
		/// <param name="visible">If true will set this main form to be visible, otherwise it's going to be invisible</param>
		private void setMainFormVisibility(bool visible) 
		{
			this.Visible = visible;
			this.Opacity = 100;
		}


		/// <summary>
		/// Sets the status bar text
		/// </summary>
		/// <param name="message">Message to be set for status bar</param>
		private void setStatusBarText(string message) 
		{
			mainStatusBar.Text = message;
		}


		/// <summary>
		/// Sets the send button state to enabled/disabled
		/// </summary>
		/// <param name="enabled">If true, will enable the send button, otherwise it will disable it</param>
		private void setSendButtonState(bool enabled) 
		{
			sendButton.Enabled = enabled;
		}


		#endregion

		#endregion

		#region utility functions 

		/// <summary>
		/// This C# example gets the bounds of all of the ink in the Ink object in pixel space, returning them in the out parameters as left, top, width, and height.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="top"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="theIC"></param>
		/// <param name="theHandle"></param>
		/// <remarks>From MSDN: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/tpcsdk10/html/reference/tbmthinkspacetopixel.asp </remarks>
		public void getBoundsFromInk(out int left, out int top, out int width, out int height, InkOverlay theIO)
		{
    
			// Copy the bounding rectangle in ink space dimensions
			System.Drawing.Rectangle inkRect = theIO.Ink.GetBoundingBox();

			// Load the top left and bottom right points
			System.Drawing.Point ptTL, ptBR;
			ptTL = ptBR = inkRect.Location;
			ptBR += inkRect.Size;

			Graphics g = CreateGraphics();

			// Find the edges of the bounding box in pixel space
			theIO.Renderer.InkSpaceToPixel(g, ref ptTL);
			theIO.Renderer.InkSpaceToPixel(g, ref ptBR);

			g.Dispose();

			// Copy the out parameters
			left = ptTL.X;
			top = ptTL.Y;
			width = ptBR.X - ptTL.X;
			height = ptBR.Y - ptTL.Y;
		}


		#endregion

		#region windows-related methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public MainForm()
		{
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
			this.chatVScroll = new System.Windows.Forms.VScrollBar();
			this.chatPanel = new System.Windows.Forms.Panel();
			this.chatHScroll = new System.Windows.Forms.HScrollBar();
			this.horizontalSplitter = new System.Windows.Forms.Splitter();
			this.bottomPanel = new System.Windows.Forms.Panel();
			this.colorButton = new System.Windows.Forms.Button();
			this.widthButton = new System.Windows.Forms.Button();
			this.sendButton = new System.Windows.Forms.Button();
			this.inputBox = new System.Windows.Forms.TextBox();
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.connectMenu = new System.Windows.Forms.MenuItem();
			this.saveAsMenu = new System.Windows.Forms.MenuItem();
			this.hyphenMenu = new System.Windows.Forms.MenuItem();
			this.exitMenu = new System.Windows.Forms.MenuItem();
			this.editMenu = new System.Windows.Forms.MenuItem();
			this.copyMenu = new System.Windows.Forms.MenuItem();
			this.pasteMenu = new System.Windows.Forms.MenuItem();
			this.hyphenMenu3 = new System.Windows.Forms.MenuItem();
			this.clearChat = new System.Windows.Forms.MenuItem();
			this.clearInput = new System.Windows.Forms.MenuItem();
			this.helpMenu = new System.Windows.Forms.MenuItem();
			this.contentsMenu = new System.Windows.Forms.MenuItem();
			this.aboutMenu = new System.Windows.Forms.MenuItem();
			this.colorDialog1 = new System.Windows.Forms.ColorDialog();
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.mainStatusBar = new System.Windows.Forms.StatusBar();
			this.chatPanel.SuspendLayout();
			this.bottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// chatVScroll
			// 
			this.chatVScroll.Dock = System.Windows.Forms.DockStyle.Right;
			this.chatVScroll.Location = new System.Drawing.Point(644, 0);
			this.chatVScroll.Name = "chatVScroll";
			this.chatVScroll.Size = new System.Drawing.Size(16, 296);
			this.chatVScroll.TabIndex = 5;
			this.chatVScroll.Visible = false;
			this.chatVScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.chatVScroll_Scroll);
			// 
			// chatPanel
			// 
			this.chatPanel.BackColor = System.Drawing.Color.Lavender;
			this.chatPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.chatPanel.Controls.Add(this.chatHScroll);
			this.chatPanel.Controls.Add(this.chatVScroll);
			this.chatPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.chatPanel.Location = new System.Drawing.Point(0, 0);
			this.chatPanel.Name = "chatPanel";
			this.chatPanel.Size = new System.Drawing.Size(664, 300);
			this.chatPanel.TabIndex = 6;
			// 
			// chatHScroll
			// 
			this.chatHScroll.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.chatHScroll.Location = new System.Drawing.Point(0, 280);
			this.chatHScroll.Name = "chatHScroll";
			this.chatHScroll.Size = new System.Drawing.Size(644, 16);
			this.chatHScroll.TabIndex = 6;
			this.chatHScroll.Visible = false;
			this.chatHScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.chatHScroll_Scroll);
			// 
			// horizontalSplitter
			// 
			this.horizontalSplitter.Cursor = System.Windows.Forms.Cursors.HSplit;
			this.horizontalSplitter.Dock = System.Windows.Forms.DockStyle.Top;
			this.horizontalSplitter.Location = new System.Drawing.Point(0, 300);
			this.horizontalSplitter.MinExtra = 100;
			this.horizontalSplitter.MinSize = 100;
			this.horizontalSplitter.Name = "horizontalSplitter";
			this.horizontalSplitter.Size = new System.Drawing.Size(664, 3);
			this.horizontalSplitter.TabIndex = 7;
			this.horizontalSplitter.TabStop = false;
			this.horizontalSplitter.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.horizontalSplitter_SplitterMoved);
			// 
			// bottomPanel
			// 
			this.bottomPanel.BackColor = System.Drawing.SystemColors.Control;
			this.bottomPanel.Controls.Add(this.colorButton);
			this.bottomPanel.Controls.Add(this.widthButton);
			this.bottomPanel.Controls.Add(this.sendButton);
			this.bottomPanel.Controls.Add(this.inputBox);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.bottomPanel.DockPadding.Top = 1;
			this.bottomPanel.Location = new System.Drawing.Point(0, 303);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = new System.Drawing.Size(664, 162);
			this.bottomPanel.TabIndex = 8;
			// 
			// colorButton
			// 
			this.colorButton.BackColor = System.Drawing.Color.Black;
			this.colorButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.colorButton.Location = new System.Drawing.Point(583, 5);
			this.colorButton.Name = "colorButton";
			this.colorButton.Size = new System.Drawing.Size(34, 34);
			this.colorButton.TabIndex = 7;
			this.colorButton.TabStop = false;
			this.colorButton.Click += new System.EventHandler(this.colorButton_Click);
			// 
			// widthButton
			// 
			this.widthButton.Location = new System.Drawing.Point(624, 5);
			this.widthButton.Name = "widthButton";
			this.widthButton.Size = new System.Drawing.Size(34, 34);
			this.widthButton.TabIndex = 6;
			this.widthButton.Text = "__";
			this.widthButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.widthButton.Click += new System.EventHandler(this.widthButton_Click);
			// 
			// sendButton
			// 
			this.sendButton.Enabled = false;
			this.sendButton.Location = new System.Drawing.Point(582, 43);
			this.sendButton.Name = "sendButton";
			this.sendButton.Size = new System.Drawing.Size(76, 88);
			this.sendButton.TabIndex = 4;
			this.sendButton.Text = "&Send";
			this.sendButton.Click += new System.EventHandler(this.sendButton_Click);
			// 
			// inputBox
			// 
			this.inputBox.BackColor = System.Drawing.Color.LightSteelBlue;
			this.inputBox.Location = new System.Drawing.Point(4, 5);
			this.inputBox.Multiline = true;
			this.inputBox.Name = "inputBox";
			this.inputBox.ReadOnly = true;
			this.inputBox.Size = new System.Drawing.Size(568, 128);
			this.inputBox.TabIndex = 3;
			this.inputBox.Text = "";
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem1,
																					  this.editMenu,
																					  this.helpMenu});
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 0;
			this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.connectMenu,
																					  this.saveAsMenu,
																					  this.hyphenMenu,
																					  this.exitMenu});
			this.menuItem1.Text = "&File";
			// 
			// connectMenu
			// 
			this.connectMenu.Index = 0;
			this.connectMenu.Text = "&Connect ...";
			this.connectMenu.Click += new System.EventHandler(this.connectMenu_Click);
			// 
			// saveAsMenu
			// 
			this.saveAsMenu.Index = 1;
			this.saveAsMenu.Text = "&Save As...";
			this.saveAsMenu.Click += new System.EventHandler(this.saveAsMenu_Click);
			// 
			// hyphenMenu
			// 
			this.hyphenMenu.Index = 2;
			this.hyphenMenu.Text = "-";
			// 
			// exitMenu
			// 
			this.exitMenu.Index = 3;
			this.exitMenu.Text = "E&xit";
			this.exitMenu.Click += new System.EventHandler(this.exitMenu_Click);
			// 
			// editMenu
			// 
			this.editMenu.Index = 1;
			this.editMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.copyMenu,
																					 this.pasteMenu,
																					 this.hyphenMenu3,
																					 this.clearChat,
																					 this.clearInput});
			this.editMenu.Text = "&Edit";
			this.editMenu.Popup += new System.EventHandler(this.editMenu_Popup);
			// 
			// copyMenu
			// 
			this.copyMenu.Index = 0;
			this.copyMenu.Text = "&Copy";
			this.copyMenu.Click += new System.EventHandler(this.copyMenu_Click);
			// 
			// pasteMenu
			// 
			this.pasteMenu.Index = 1;
			this.pasteMenu.Text = "&Paste";
			this.pasteMenu.Click += new System.EventHandler(this.pasteMenu_Click);
			// 
			// hyphenMenu3
			// 
			this.hyphenMenu3.Index = 2;
			this.hyphenMenu3.Text = "-";
			// 
			// clearChat
			// 
			this.clearChat.Index = 3;
			this.clearChat.Text = "Clear c&hat";
			this.clearChat.Click += new System.EventHandler(this.clearChat_Click);
			// 
			// clearInput
			// 
			this.clearInput.Index = 4;
			this.clearInput.Text = "Clear &input";
			this.clearInput.Click += new System.EventHandler(this.clearInput_Click);
			// 
			// helpMenu
			// 
			this.helpMenu.Index = 2;
			this.helpMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.contentsMenu,
																					 this.aboutMenu});
			this.helpMenu.Text = "&Help";
			// 
			// contentsMenu
			// 
			this.contentsMenu.Index = 0;
			this.contentsMenu.Text = "&Contents...";
			this.contentsMenu.Click += new System.EventHandler(this.contentsMenu_Click);
			// 
			// aboutMenu
			// 
			this.aboutMenu.Index = 1;
			this.aboutMenu.Text = "&About InkTalk...";
			this.aboutMenu.Click += new System.EventHandler(this.aboutMenu_Click);
			// 
			// saveFileDialog1
			// 
			this.saveFileDialog1.Filter = "GIF File (*.gif)|*.gif|Text File (*.txt)|*.txt";
			// 
			// mainStatusBar
			// 
			this.mainStatusBar.Location = new System.Drawing.Point(0, 443);
			this.mainStatusBar.Name = "mainStatusBar";
			this.mainStatusBar.Size = new System.Drawing.Size(664, 22);
			this.mainStatusBar.TabIndex = 9;
			// 
			// MainForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(664, 465);
			this.Controls.Add(this.mainStatusBar);
			this.Controls.Add(this.bottomPanel);
			this.Controls.Add(this.horizontalSplitter);
			this.Controls.Add(this.chatPanel);
			this.Menu = this.mainMenu1;
			this.Name = "MainForm";
			this.Opacity = 0;
			this.Text = "InkTalk";
			this.Resize += new System.EventHandler(this.MainForm_Resize);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.MainForm_Closing);
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.chatPanel.ResumeLayout(false);
			this.bottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new MainForm());
		}


		#endregion

		#endregion

	}
}
