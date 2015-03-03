using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The data model defined by this file serves as a representative example of a strongly-typed
// model.  The property names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs. If using this model, you might improve app 
// responsiveness by initiating the data loading task in the code behind for App.xaml when the app 
// is first launched.

namespace HierarchicalNavTemplate.Data
{
    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class PortalDataItem
    {
        public PortalDataItem(String uniqueId, String title, String folder, String webURL, String subtitle, String imagePath, String description, String content)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Folder = folder;
            this.WebURL = webURL;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Content = content;
            
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Folder { get; private set; }
        public string WebURL { get; private set; }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public string Content { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class PortalDataGroup
    {
        public PortalDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Items = new ObservableCollection<PortalDataItem>();
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public ObservableCollection<PortalDataItem> Items { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with content read from a static json file.
    /// 
    /// PortalDataSource initializes with data read from a static json file included in the 
    /// project.  This provides portal data at both design-time and run-time.
    /// </summary>
    public sealed class PortalDataSource
    {
        private static PortalDataSource _portalDataSource = new PortalDataSource();

        private ObservableCollection<PortalDataGroup> _groups = new ObservableCollection<PortalDataGroup>();
        public ObservableCollection<PortalDataGroup> Groups
        {
            get { return this._groups; }
        }

        public static async Task<IEnumerable<PortalDataGroup>> GetGroupsAsync()
        {
            await _portalDataSource.GetPortalDataAsync();

            return _portalDataSource.Groups;
        }

        public static async Task<PortalDataGroup> GetGroupAsync(string uniqueId)
        {
            await _portalDataSource.GetPortalDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _portalDataSource.Groups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static async Task<PortalDataItem> GetItemAsync(string uniqueId)
        {
            await _portalDataSource.GetPortalDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _portalDataSource.Groups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        private async Task GetPortalDataAsync()
        {
            if (this._groups.Count != 0)
                return;

            Uri dataUri = new Uri("ms-appx:///DataModel/PortalData.json");

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
            string jsonText = await FileIO.ReadTextAsync(file);
            JsonObject jsonObject = JsonObject.Parse(jsonText);
            JsonArray jsonArray = jsonObject["Groups"].GetArray();

            foreach (JsonValue groupValue in jsonArray)
            {
                JsonObject groupObject = groupValue.GetObject();
                PortalDataGroup group = new PortalDataGroup(groupObject["UniqueId"].GetString(),
                                                            groupObject["Title"].GetString(),
                                                            groupObject["Subtitle"].GetString(),
                                                            groupObject["ImagePath"].GetString(),
                                                            groupObject["Description"].GetString());

                foreach (JsonValue itemValue in groupObject["Items"].GetArray())
                {
                    JsonObject itemObject = itemValue.GetObject();
                    group.Items.Add(new PortalDataItem(itemObject["UniqueId"].GetString(),
                                                       itemObject["Title"].GetString(),
                                                       itemObject["Folder"].GetString(),
                                                       itemObject["WebURL"].GetString(),
                                                       itemObject["Subtitle"].GetString(),
                                                       itemObject["ImagePath"].GetString(),
                                                       itemObject["Description"].GetString(),
                                                       itemObject["Content"].GetString()));
                }
                this.Groups.Add(group);
            }
        }
    }
}