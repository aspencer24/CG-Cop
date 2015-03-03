using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace CGCop
{
    public class SiteCacher
    {
        private string url;
        private string datetimeFolder;
        private string defaultFolder;

        public string responseFromServer { get; private set; }

        //List<HtmlNode> imageNodes;

        public SiteCacher(String url, String defaultFolder)
        {
            this.url = url;
            DateTime now = DateTime.UtcNow;
            //int year = now.Year;
            //int month = now.Month;
            //int day = now.Day;
            //int hours = now.Hour;
            //int minutes = now.Minute;
            //this.storedFileName = "" + year + month + day + hours + minutes; //storedFileName;
            this.datetimeFolder = now.ToString("yyyyMMddHHmm");
            this.defaultFolder = defaultFolder;
        }

        public async void getResponse()
        {
            WebRequest request = WebRequest.Create(url);

            WebResponse response = (WebResponse)await request.GetResponseAsync();

            //Debug
            //TODO: Handle Bad Response
            //Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            using (Stream dataStream = response.GetResponseStream())
            {

                using (StreamReader reader = new StreamReader(dataStream))
                {
                    // Read the content
                    this.responseFromServer = reader.ReadToEnd();

                }
            }

            cacheSite();

        }

        public async void cacheSite()
        {
            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;

            folder = await folder.CreateFolderAsync(defaultFolder, CreationCollisionOption.OpenIfExists);

            folder = await folder.CreateFolderAsync(datetimeFolder, CreationCollisionOption.OpenIfExists);

            StorageFile file = await folder.CreateFileAsync("index.html", CreationCollisionOption.ReplaceExisting);


            await Windows.Storage.FileIO.WriteTextAsync(file, responseFromServer);
            //File.WriteAllText(storedFileName, responseFromServer);

            HtmlDocument doc = new HtmlDocument();

            doc.LoadHtml(responseFromServer);

            // Now, using LINQ to get all Images
            //List<HtmlNode> imageNodes = null;
            var imageNodes = doc.DocumentNode.DescendantsAndSelf("img")
                              .Where(p => p.GetAttributeValue("src", null) != null);
            //.ToList();
            // where node.Name == "img"

            //select node).ToList();

            foreach (HtmlNode node in imageNodes)
            {

                downloadImageFromUrl(node.Attributes["src"].Value, this.url);
            }
        }

        public void downloadImage(String imageSrc, String url)
        {
            /*
            //TODO: catch exceptions
            String imageDirectory = Path.GetDirectoryName(imageSrc);
            if (!String.IsNullOrEmpty(imageDirectory))
                Directory.CreateDirectory(imageDirectory);


            //Filename
            //Console.WriteLine(Path.GetFileName(imageSrc));

            String imageUrl = url + "/" + imageSrc;


            using (WebClient Client = new WebClient())
            {
                Client.DownloadFile(imageUrl, imageSrc);
            } */
        }

        public async void downloadImageFromUrl(String imageSrc, String url)
        {
            StorageFolder myfolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            myfolder = await myfolder.CreateFolderAsync(defaultFolder, CreationCollisionOption.OpenIfExists);

            myfolder = await myfolder.CreateFolderAsync(datetimeFolder, CreationCollisionOption.OpenIfExists);

            string[] words = imageSrc.Split('/');
            foreach (string word in words)
            {
                if (word.Contains('.'))
                    break;
                else
                {
                    myfolder = await myfolder.CreateFolderAsync(word, CreationCollisionOption.OpenIfExists);
                    //myfolder.CreateFolderAsync(word, )
                }
            }

            try
            {
                HttpClient client = new HttpClient();

                //Remove file name like "something.html" from off of url
                int sizeOfFileName = Path.GetFileName(url).Count();
                url = url.Substring(0, url.Count() - sizeOfFileName);


                System.Uri imageUri = new System.Uri(url + imageSrc);

                HttpResponseMessage message = await client.GetAsync(imageUri);

                //StorageFolder myfolder = Windows.Storage.ApplicationData.Current.LocalFolder;

                imageSrc.Replace('/', '\\');

                String imageFileName = System.IO.Path.GetFileName(imageSrc);

                StorageFile sampleFile = await myfolder.CreateFileAsync(imageFileName, CreationCollisionOption.ReplaceExisting);

                IBuffer buffer = await message.Content.ReadAsBufferAsync();



                //Windows.Storage.Streams.Buffer buffer = (Windows.Storage.Streams.Buffer)await message.Content.ReadAsBufferAsync();

                await Windows.Storage.FileIO.WriteBufferAsync(sampleFile, buffer);

                //byte[] buffer; 

                //var buf = await FileIO.WriteBytesAsync(sampleFile, buffer);

                //DataWriter writer = new DataWriter()
                //var bytes = new byte[buf.Lenght]

                //var files = await myfolder.GetFilesAsync();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }





        }
    }
}
