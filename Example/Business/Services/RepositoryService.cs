using System.Reflection;
using Example.Business.Collections;

namespace Example.Business.Services
{
    internal class RepositoryService : IRepositoryService
    {
        private readonly ResourcePdfLoader _loader = new();

        public string GetPdfSource()
        {
            //  Get PDF as byte[] from any source.
            //  It can be a file downloaded via REST request, retrieved from user's local disk, etc.,
            //  or as in this example - retrieved from application resources. 
            byte[] bytes = _loader.GetPdfFileContent();

            //  Save the data to a file to get the path to the file.
            //  You can then pass the file path to the library.
            var fileName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            File.WriteAllBytes(fileName, bytes);

            //  Return path to PDF file
            return fileName;
        }
    }

    internal class ResourcePdfLoader
    {
        private readonly LoopedList<string> _pdfs = new();

        public ResourcePdfLoader()
        {
            _pdfs.Add("pdf2.pdf");
            _pdfs.Add("pdf1.pdf");
        }

        public byte[] GetPdfFileContent()
        {
            var name = _pdfs.Next();

            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly
                .GetManifestResourceNames()
                .Single(str => str.EndsWith(name));

            byte[] bytes;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
            }

            return bytes;
        }
    }
}
