using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web;

namespace cgcop.Common
{
    public sealed class StreamUriWinRTResolver : IUriToStreamResolver
    {
        public IAsyncOperation<IInputStream> UriToStreamAsync(Uri uri)
        {
            if (uri == null)
            {
                throw new Exception();
            }
            string path = uri.AbsolutePath;
            return GetContent(path).AsAsyncOperation();
        }

        private async Task<IInputStream> GetContent(string URIPath)
        {
            try
            {
                //Uri localUri = new Uri("ms-appdata:///local/content" + URIPath);
                Uri localUri = new Uri("ms-appdata:///local" + URIPath);
                StorageFile f = await StorageFile.GetFileFromApplicationUriAsync(localUri);
                IRandomAccessStream stream = await f.OpenAsync(FileAccessMode.Read);
                return stream.GetInputStreamAt(0);
            }
            catch (Exception)
            {
                throw new Exception("Invalid path");
            }
        }
    }
}
