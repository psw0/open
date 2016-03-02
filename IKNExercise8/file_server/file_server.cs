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
			TcpListener TCPserver = new TcpListener(PORT);
			byte[] bytes = new byte[BUFSIZE];
			requestCount = 0;
			TCPserver.Start ();

			while (true)
			{
			    try
			    {
				Console.WriteLine("Waiting for connection, plz");
				// Blocking until connection
				Socket TCPsocket = TCPserver.AcceptSocket();
				Console.WriteLine("Connected, hopefully");

				// modtag fil navn
				int received_bytes = TCPsocket.Receive(bytes);
			        string received_string = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
			        Console.WriteLine("Received bytes: {0}", received_bytes);
			        Console.WriteLine("Received string: {0}", received_string);

			        //TCPsocket.Send(System.Text.Encoding.ASCII.GetBytes("HALLO"));

                    sendFile(received_string, received_bytes, new NetworkStream(TCPsocket));
			    }
			    catch (Exception ex)
			    {
                    Console.WriteLine(ex.ToString());
			    }
			}
            TCPserver.Stop();
		    Console.ReadLine();
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
		private void sendFile (String fileName, long fileSize, NetworkStream io)
		{
			requestCount = requestCount + 1;
			NetworkStream networkStream = io;
			byte[] buffer = new byte[BUFSIZE];
			using (var s = File.OpenRead(fileName))
			{
				int amountRead;
				while ((amountRead= s.Read(buffer, 0, fileSize)) > 0)
				{
					io.Write(buffer, 0, amountRead);
				}
			}
			io.Flush();
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
