using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace tcp
{
	class file_client
	{
		// Progress bar length:
		private const int PROGBARLEN = 20;
		// TCP port
		const int PORT = 9000;
		// Buffer size
		const int BUFSIZE = 1000;
		// Save file path
		const string PATH = "./";


		private IPAddress _ipAddress = null;
		private string _fileName = "";
		TcpClient _client;

		/// <summary>
		/// Initializes a new instance of the <see cref="file_client"/> class.
		/// </summary>
		/// <param name='args'>
		/// The command-line arguments. First ip-adress of the server. Second the filename
		/// </param>
		private file_client (string[] args)
		{
			//Validate input arguments, prints relevant info
			if (validateInputParameters (args)) {
				_ipAddress = IPAddress.Parse(args [0]);
				_fileName = args [1];
			} 
			else {
				Environment.Exit (exitCode: -1);
			}

			try
			{
			    // Open the Tcp connection
			    _client = new TcpClient (_ipAddress.ToString(), PORT); 
			}
			catch (SocketException ex) {
					Console.WriteLine ("Error establishing connection: {0}", ex.Message);
					Environment.Exit(exitCode:-1);
			}

			Console.WriteLine ("Fetching {0} from {1}", Path.GetFileName(_fileName), _ipAddress.ToString());


			// TODO: refactor
			// Send file name
			//Preparing buffer
			byte[] bytes = new byte[BUFSIZE];
			bytes = System.Text.Encoding.ASCII.GetBytes (_fileName + "\0");

			// Get stream object
			NetworkStream stream = _client.GetStream();

			// write filename to server
			stream.Write(bytes, 0, bytes.Length);
			// End send file name


			// receive file from server
			try
			{
				ReceiveFile(_fileName, stream);
			}
			catch(FileNotFoundException e)
			{
				Console.WriteLine ("Error: {0}", e.Message);
			}
			// TODO: Catch too big file exception here
		}

		private bool validateInputParameters (string[] args)
		{
			if (args.Length == 2) {
				return ValidateIP (args[0]) && ValidateFilePath (args[1]);
			} else {
				PrintFormattingInformation ();
				return false;
			}
		}

		bool ValidateFilePath (string filepath)
		{
			// Check if file already exists on client
			if (File.Exists (PATH + Path.GetFileName (filepath))) {
				Console.WriteLine ("Error: {0} already exists", Path.GetFileName (filepath));
				return false;
			} else {
				return true;
			}
		}

		private bool ValidateIP (string ip)
		{
			IPAddress dummy;
			if (IPAddress.TryParse (ip, out dummy)) {
				return true;
			}
			else{
				Console.WriteLine ("Error: IP invalid");
				return false;
			}
		}

	    /// <summary>
	    /// Receives the file.
	    /// </summary>
	    /// <param name="fileName">File name.</param>
	    /// <param name="stream">Stream.</param>
	    private void ReceiveFile (string fileName, NetworkStream stream)
		{
			//TODO: Cleanup
			// buffer for reading file
			byte[] buffer = new byte[BUFSIZE];

			// Get size of file
			int totalSize = receiveSizeHeader (stream);
			if (totalSize == -1)
			{
				throw new FileNotFoundException ("File not found");
			}

			// How many bytes are written/need to be written
			int amountRead = 0;
			int amountToRead = totalSize;

			// Open the file
			using (var s = new BinaryWriter(new FileStream(PATH + Path.GetFileName(fileName), FileMode.CreateNew)))
			{
				// Keep receiving data until amount of bytes
				// received is correct
				do
				{
				    int i = stream.Read(buffer, 0, buffer.Length);
				    amountToRead = amountToRead - i;
				    amountRead = amountRead + i;
				    s.Write(buffer, 0, i);

				    // Update progress bar
				    Console.Write(progress_bar(amountRead, totalSize, PROGBARLEN) + '\r');

					if(amountRead < 0)
					{
						throw new Exception("Received more bytes than anticipated");
					}
				} while (amountToRead != 0);
			}

			Console.WriteLine ("\nFile received: {0}", Path.GetFileName(fileName));
			Console.WriteLine ("Bytes: {0}", amountRead);
		}

		void PrintFormattingInformation ()
		{
			Console.WriteLine ("Wrong syntax, terminating... \nCorrect syntax is:");
			Console.WriteLine ("./file_client.exe <ip-address> <path>");
		}

		// Receives size header, in the format '<'<SIZE>'>'
		private int receiveSizeHeader(NetworkStream stream)
		{	
			// Keep reading bytes until header is done
			// Should have used regex or something
			string sizeHeader = "";
			do
			{
				int readByte = stream.ReadByte();
				sizeHeader = sizeHeader + (char)readByte;
			} while (sizeHeader[sizeHeader.Length-1] != '>');
				
			// total size in bytes from header
			return Int32.Parse(sizeHeader.Substring(1, sizeHeader.Length - 2));

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


		// Function for creating a progress bar
		// amount = How many.                           amount > 0
		// total = total amount.                        total  > amount
		// length = How many characters are inside bar. length > 0
		static private string progress_bar(int amount, int total, int length)
		{
			// percentage done
			float done = (float)amount / (float)total;

			// How many full characters to print
			int fullAmount = (int)(done * length);

			// How many empty characters to print
			int emptyAmount = length - fullAmount;

			// Return string with first character
			string retstr = "{";

			// For loop for full characters
			for (int i = 0; i < fullAmount; ++i)
			{
				retstr += "#";
			}

			// For loop for empty characters
			for (int i = 0; i < emptyAmount; ++i)
			{
				retstr += "_";
			}

			// Terminate bar
			retstr += "} ";

			// Add percentage
			retstr += ((int)(done * 100)) + "%";

			return retstr;
		}

	}
}