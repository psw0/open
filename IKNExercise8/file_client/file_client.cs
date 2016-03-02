using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace tcp
{
	class file_client
	{
		/// <summary>
		/// The PORT.
		/// </summary>
		const int PORT = 9000;
		/// <summary>
		/// The BUFSIZE.
		/// </summary>
		const int BUFSIZE = 1000;

		/// <summary>
		/// Initializes a new instance of the <see cref="file_client"/> class.
		/// </summary>
		/// <param name='args'>
		/// The command-line arguments. First ip-adress of the server. Second the filename
		/// </param>
		private file_client (string[] args)
		{
			// Make sure there's any args
			if (args.Length == 0)
			{
				Console.WriteLine ("Please specify host, and file");
			}
			else if (args.Length == 1)
			{
				Console.WriteLine ("Please specify file");
			}
			else
			{
				string server_address = args [0];
				string file_name = args [1];

				TcpClient client = new TcpClient (server_address, PORT);
				byte[] bytes = new byte[BUFSIZE];
				bytes = System.Text.Encoding.ASCII.GetBytes (file_name);

				// open stream
				Socket TCPsocket = client.Client;

				// write filename
				TCPsocket.Send(bytes);

				// read response
				int bytes_read = TCPsocket.Receive(bytes);

				// write to console
				Console.WriteLine ("{0}",System.Text.Encoding.ASCII.GetString(bytes, 0, bytes_read));

			}

		}

		/// <summary>
		/// Receives the file.
		/// </summary>
		/// <param name='fileName'>
		/// File name.
		/// </param>
		/// <param name='io'>
		/// Network stream for reading from the server
		/// </param>
		private void receiveFile (String fileName, NetworkStream io)
		{
			// TO DO Your own code
		}

		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name='args'>
		/// The command-line arguments.
		/// </param>
		public static void Main (string[] args)
		{
			Console.WriteLine ("Client starts...");
			new file_client(args);
		}
	}
}
