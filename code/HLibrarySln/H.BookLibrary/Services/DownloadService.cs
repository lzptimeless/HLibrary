﻿using H.Book;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace H.BookLibrary
{
    public class DownloadService
    {
        public event EventHandler<BookEventArgs> DownloadBookStart;
        public event EventHandler<BookEventArgs> DownloadBookStop;
        public event EventHandler<BookEventArgs> DownloadBookResume;
        public event EventHandler<BookEventArgs> DownloadBookCompleted;
        public event EventHandler<PageEventArgs> DownloadPageCompleted;
        public event EventHandler<BookEventArgs> DownloadPreviewBookStart;
        public event EventHandler<BookEventArgs> DownloadPreviewBookStop;
        public event EventHandler<BookEventArgs> DownloadPreviewBookResume;
        public event EventHandler<BookEventArgs> DownloadPreviewBookCompleted;
        public event EventHandler<BookEventArgs> DownloadPreviewPageCompleted;

        public Task<IHBookHeader> GetDownloadingBook()
        {
            throw new NotImplementedException();
        }

        public Task<BooksResult> GetBooksAsync()
        {
            throw new NotImplementedException();
        }

        public Task<BitmapImage> GetCoverThumbnailAsync(Guid bookID)
        {
            throw new NotImplementedException();
        }

        public Task<BitmapImage> GetPageThumbnailAsync(Guid bookID, Guid pageID)
        {
            throw new NotImplementedException();
        }

        public Task DownloadAsync()
        {
            throw new NotImplementedException();
        }

        public Task StopAsync()
        {
            throw new NotImplementedException();
        }

        public Task ResumeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
