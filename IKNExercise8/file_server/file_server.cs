
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace tcp
{
	class file_server
	{
		/// <summary>
		/// The PORT
		/// </summary>
		const int PORT = 9000;
		/// <summary>
		/// The BUFSIZE
		/// </summary>
		const int BUFSIZE = 1000;

		/// <summary>
		/// The request count. The amount of requests serviced.
		/// </summary>
		int requestCount;

		/// <summary>
		/// Initializes a new instance of the <see cref="file_server"/> class.
		/// Opretter en socket.
		/// Venter på en connect fra en klient.
		/// Modtager filnavn
		/// Finder filstørrelsen
		/// Kalder metoden sendFile
		/// Lukker socketen og programmet
 		/// </summary>
		private file_server ()
		{
			// Select IP to listen on
			IPAddress server_IP = choose_ip();

			// Start TCPserver
			TcpListener TCPserver = new TcpListener(server_IP, PORT);
			requestCount = 0;
			TCPserver.Start ();

			// Main loop

			Console.WriteLine ("Waiting for connection ...");
			while (!Console.KeyAvailable) {
				if (TCPserver.Pending ()) {
					try {
						// Blocking until connection
						TcpClient client = TCPserver.AcceptTcpClient ();
						Console.WriteLine ("Connected...!");

						// Setup network stream
						NetworkStream stream = client.GetStream ();

						// Receive file name
						string received_filename = receive_filename (stream);
						Console.WriteLine ("Received file name: {0}", received_filename);

						// Send file
						sendFile (received_filename, stream);

						Console.WriteLine ("Total number of requests: {0}", requestCount);

					} catch (Exception ex) {
						Console.WriteLine (ex.ToString ());
					}
				}
			}
            TCPserver.Stop();
			Console.WriteLine ("Server shutting down, requests: {0}", requestCount);
		}

		/// <summary>
		/// Sends the file.
		/// </summary>
		/// <param name='fileName'>
		/// The filename.
		/// </param>
		/// <param name='fileSize'>
		/// The filesize.
		/// </param>
		/// <param name='io'>
		/// Network stream for writing to the client.
		/// </param>
		private void sendFile (string fileName, NetworkStream stream)
		{
			// increment requestCount to indicated serviced requests
			requestCount = requestCount + 1;

			// buffer for reading file
			byte[] buffer;
			int byteAmount = 0;

			if (file_exists (fileName)) 
			{
				// Send length of file to client
				long file_length = get_file_length (fileName);
				buffer = System.Text.Encoding.ASCII.GetBytes ("<" + file_length.ToString() + ">");
				stream.Write (buffer, 0, buffer.Length);

				// Open the file
				using (var s = new BinaryReader(new FileStream(fileName, FileMode.Open)))
				{
					// Keep sending data until the file is exhausted
					while ((buffer = s.ReadBytes(1000)).Length > 0)
					{
						byteAmount = byteAmount + buffer.Length;
						stream.Write(buffer, 0, buffer.Length);
					}
				}
				stream.Flush();
				Console.WriteLine ("Bytes sent: {0}", byteAmount);
			}
			else
			{
				// TODO: refactor at some point
				buffer = System.Text.Encoding.ASCII.GetBytes ("<-1>");
				stream.Write(buffer, 0, 4);
				Console.WriteLine("File not found");
			}
		}

		// Receives filename as null terminated string from stream, blocking
		private string receive_filename(NetworkStream stream)
		{
			//TODO: Cleanup
			string file_name = "";
			do
			{
				int read_byte = stream.ReadByte();
				if(read_byte != -1){
					file_name = file_name + (char)read_byte;
				}
			}while(file_name[file_name.Length-1] != '\0');

			return file_name.Substring (0,file_name.Length-1);
		}

		// Returns length of file in bytes
		private long get_file_length(string fileName)
		{
			FileInfo info = new FileInfo (fileName);
			return info.Length;
		}

		// Returns whether a file exists or not
		private bool file_exists(string filename)
		{
			FileInfo info = new FileInfo (filename);
			return info.Exists;
		}

		// Generates dialog to select IP and returns selected IPAddress
		private static IPAddress choose_ip ()
		{
			IPAddress[] server_IPs = Dns.GetHostEntry (Dns.GetHostName ()).AddressList;
			int selected_ip;
			while (true) {
				Console.WriteLine ("Select IP to listen on");
				for (int i = 0; i < server_IPs.Length; ++i) {
					Console.WriteLine (i + ": " + server_IPs [i].ToString ());
				}
				Console.Write ("IP to use: ");
				selected_ip = (int)(Console.Read () - '0');
				Console.WriteLine ("");
				if (selected_ip >= 0 && selected_ip < server_IPs.Length) {
					break;
				}
				else {
					Console.WriteLine ("Index out of range");
					Console.WriteLine ("");
				}
			}
			return server_IPs[selected_ip];
		}


		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name='args'>
		/// The command-line arguments.
		/// </param>
		public static void Main (string[] args)
		{
			Console.WriteLine ("Server starts...");
			new file_server();
		}
	}
}