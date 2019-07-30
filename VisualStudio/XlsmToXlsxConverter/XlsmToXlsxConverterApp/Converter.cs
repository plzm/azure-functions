using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;

namespace XlsmToXlsxConverterApp
{
	public static class Converter
	{
		[FunctionName("Converter")]
		public static void Run([BlobTrigger("files-stage/xlsm/{name}.xlsm")]Stream xlsmBlob, string name, ILogger log, [Blob("files-stage/files/{name}.xlsx", FileAccess.Write)]Stream xlsxBlob)
		{
			log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {xlsmBlob.Length} Bytes");

			using (MemoryStream interim = new MemoryStream())
			{
				xlsmBlob.CopyTo(interim);

				using (SpreadsheetDocument spreadsheetDoc = SpreadsheetDocument.Open(interim, true))
				{
					spreadsheetDoc.DeletePartsRecursivelyOfType<VbaDataPart>();
					spreadsheetDoc.DeletePartsRecursivelyOfType<VbaProjectPart>();

					// Change from template type to workbook type
					spreadsheetDoc.ChangeDocumentType(SpreadsheetDocumentType.Workbook);
				}

				var byteArray = interim.ToArray();

				xlsxBlob.Write(byteArray, 0, byteArray.Length);
			}
		}
	}
}
