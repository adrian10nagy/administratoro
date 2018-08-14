
namespace Toolbox.DocumentGenerator
{
    using System.IO;
    using iTextSharp.text;
    using iTextSharp.text.html.simpleparser;
    using iTextSharp.text.pdf;

    public static class ConverterHelper
    {
        public static byte[] ConvertHtMLtoBytes(string pHtml)
        {
            var ms = new MemoryStream();
            var txtReader = new StringReader(pHtml);

            // 1: create object of a itextsharp document class
            var doc = new Document(PageSize.A4, 25, 25, 25, 25);

            // 2: we create a itextsharp pdfwriter that listens to the document and directs a XML-stream to a file
            var oPdfWriter = PdfWriter.GetInstance(doc, ms);

            // 3: we create a worker parse the document
            var htmlWorker = new HTMLWorker(doc);

            // 4: we open document and start the worker on the document
            doc.Open();
            htmlWorker.StartDocument();

            // 5: parse the html into the document
            htmlWorker.Parse(txtReader);

            // 6: close the document and the worker
            htmlWorker.EndDocument();
            htmlWorker.Close();
            doc.Close();

            var bPdf = ms.ToArray();

            return bPdf;
        }
    }
}
