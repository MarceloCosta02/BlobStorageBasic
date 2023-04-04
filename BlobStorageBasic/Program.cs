using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Reflection.Metadata;

public static class Program
{
	public static void Main(string[] args)
	{
		Console.WriteLine("Iniciando aplicação");

		Console.WriteLine("\nPor favor forneça o nome do storage account");
		string storageaccount = Console.ReadLine();

		Console.WriteLine("\nPor favor forneça a connection string");
		string connectionString = Console.ReadLine();

		Console.WriteLine("\nPor favor forneça o shared access uri");
		string sharedAccessURI = Console.ReadLine();

		Console.WriteLine("\nPor favor forneça o nome do container");
		string container = Console.ReadLine();

		Console.WriteLine("\nListando todos os blobs do " + container + " no Storage Account " + storageaccount);
		Console.WriteLine();

		ListarBlobs(connectionString, container);

		Console.WriteLine("\nInforme o nome do arquivo que está no diretório e que será enviado ao blob storage:");
		Console.WriteLine();

		string filename = Console.ReadLine();

		Console.WriteLine("\nEnviando o arquivo " + filename + " para o container " + container + " da Storage Account " + storageaccount);

		Upload(connectionString, container, filename);

		Console.WriteLine("\nListando todos os blobs do " + container + " no Storage Account " + storageaccount + " após o upload");
		Console.WriteLine();

		ListarBlobs(connectionString, container);

		Console.WriteLine("\nLendo o arquivo " + filename + " do container " + container + " na Storage Account " + storageaccount);
		Console.WriteLine();

		VisualizarDados(sharedAccessURI, container, filename).GetAwaiter().GetResult();

		Console.WriteLine("\nExecução completa");
	}


	// Método para fazer upload de um arquivo para um Blob.
	private static void Upload(string connectionString, string container, String filename)
	{
		BlobContainerClient blobcontainer = new(connectionString, container);
		
		if (blobcontainer.GetBlobClient(filename).Exists())
		{
			blobcontainer.DeleteBlob(filename);
		}

		if (File.Exists(filename))
		{
			BlobClient blob = blobcontainer.GetBlobClient(filename);
			using (FileStream file = File.OpenRead(filename))
			{
				blob.Upload(file);
				Console.WriteLine("O arquivo " + filename + " está sendo carregado");
			}
			Console.WriteLine("O arquivo " + filename + "foi carregado com sucesso");
		}
		else
		{
			Console.WriteLine("O arquivo não existe, por favor, forneça um nome de arquivo que exista nos dados da pasta");
		}
	}

	// Método para listar todos os Objetos Blob no contêiner usando a string de conexão.
	private static void ListarBlobs(string connectionString, string container)
	{
		BlobContainerClient containerClient = new BlobContainerClient(connectionString, container);

		containerClient.CreateAsync();
		foreach (BlobItem blob in containerClient.GetBlobs())
		{
			Console.WriteLine(blob.Name);
		}
	}

	// Tarefa Assíncrona para ler os dados de um blob usando a Shared Access Signature
	public static async Task VisualizarDados(string uri, string container, String filename)
	{
		UriBuilder sasURI = new(uri);
		BlobServiceClient service = new(sasURI.Uri);
		BlobClient blobClient = service.GetBlobContainerClient(container).GetBlobClient(filename);

		if (await blobClient.ExistsAsync())
		{
			var blobs = await blobClient.DownloadAsync();
			using (var streamReader = new StreamReader(blobs.Value.Content))
			{
				while (!streamReader.EndOfStream)
				{
					var line = await streamReader.ReadLineAsync();
					Console.WriteLine(line);
				}
			}
		}
	}
}
