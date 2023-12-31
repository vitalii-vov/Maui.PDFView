﻿using System.Reflection;

namespace Example.Business.Services
{
    internal class RepositoryService
    {
        private readonly Queue<string> _pdfs = new();

        public RepositoryService()
        {
            _pdfs.Enqueue("pdf2.pdf");
            _pdfs.Enqueue("pdf1.pdf");
        }

        public string GetPdfSource()
        {
            var name = NextPdfName();

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

            var fileName = Path.Combine(Path.GetTempPath(), name);
            File.WriteAllBytes(fileName, bytes);

            return fileName;
        }

        private string NextPdfName()
        {
            var name = _pdfs.Dequeue();
            _pdfs.Enqueue(name);
            return name;
        }
    }
}
