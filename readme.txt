
InkTalk 1.1
==================
by Minh T. Nguyen
minh@minh.org
http://www.enderminh.com


Description
-----------
InkTalk is a PowerToy for the Tablet PC that allows you to chat with another InkTalk client over the internet using your natural handwriting. You can simply write your message and the other InkTalk client will see your message in your original handwriting.


Requirements
------------
Microsoft .NET Framework 1.1
http://msdn.microsoft.com/netframework/technologyinfo/howtoget/default.aspx

Microsoft Table PC SDK 1.5 (for compilation only)
http://www.gotdotnet.com/team/tabletpc/


Manual
------
InkTalk is a stand-alone application that connects to another InkTalk client over the internet. When you start InkTalk you will have the option to either connect to another client or to wait for an incoming client connection. 

In order to establish a connection, the first party has to start InkTalk in the server mode where you wait for an incoming connection. Then, the second party connects to the server. Both parties have to specify the IP addresses of the server during the connection wizard. If you are connecting over the internet, be sure you specify your external IP, which you can obtain by visiting http://www.whatismyip.com. It is important that both the client and the server uses the same IP address.

If the server party is behind a firewall or router, the server party must make sure that the firewall is properly configured to allow incoming connections on port 53101 to be routed to the actual server computer. The port 53101 can be changed if necessary in the InkTalk.exe.config file.

Once the connection is established, you can simply write your messages in the input panel below and click on Send to send the message. Two other buttons above the send button allow you to control the color as well as the thickness of your pen. Notice that you can change the area of the input panel and the chatwindow by scrolling the horizontal splitter between the two.

You can also copy and paste ink to and from other ink-enabled applications like Windows Journal. You can only copy ink from the above chat window and paste into the input panel at the bottom.

Lastly, you can save your messages as a GIF file or as text file provided that operating system has a text-recognition engine (a real Tablet PC does, while the Tablet PC SDK does not).


Future Version
--------------
The 1.1 version of this PowerToy only allows one-on-one connections. Potential future versions will allow you to connect an InkTalk-server for chatroom-capability.

