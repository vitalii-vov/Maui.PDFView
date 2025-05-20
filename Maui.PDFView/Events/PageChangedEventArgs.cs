namespace Maui.PDFView.Events
{
    public class PageChangedEventArgs
    {
        public PageChangedEventArgs(int currentPage, int totalPages)
        {
            CurrentPage = currentPage;
            TotalPages = totalPages;
        }
       
        public int CurrentPage { get; }
        public int TotalPages { get; }
    }
}
