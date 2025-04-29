using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using AciteMediaApp.Models;
using AciteMediaApp.Services.Interfaces;
using Newtonsoft.Json.Linq;
using SQLite;

namespace AciteMediaApp.Services
{
    class MetadataService : IMetadataService
    {
        private readonly HttpClient _httpClient;
        private readonly SQLiteAsyncConnection _database;
        private readonly AlphanumComparator alphanumComparator = new();
        private readonly ConnectivityService _connectivityService;
        public static bool IsOffLine { get; private set; } = false;

        public static string DatabasePath => Path.Combine(FileSystem.AppDataDirectory, "metadata.db");
        public static string CachePath => Path.Combine(FileSystem.AppDataDirectory, "Cache");

        public static readonly string BaseUrl = "https://192.168.1.10/";
        public static string BuildUrlWithParameters(string baseUrl, Dictionary<string, string> parameters)
        {
            var uriBuilder = new UriBuilder(BaseUrl + baseUrl);
            var query = HttpUtility.ParseQueryString(string.Empty, Encoding.UTF8);
            foreach (var param in parameters)
            {
                query[param.Key] = param.Value;
            }

            uriBuilder.Query = query.ToString();
            return uriBuilder.ToString();
        }

        public async Task<ComicMetadata> ResolveMetadataAsyc(string name)
        {
            var sq = await _database.Table<ComicMetadata>().Where(n => n.Name == name).ToArrayAsync();
            if (sq.Length >= 1) return sq[0];

            if(IsOffLine) throw new IOException("Offline Status meets remote Comic!");

            var furl = BuildUrlWithParameters("api/image/meta", new Dictionary<string, string>
            {
                ["collection"] = name
            });

            var response = await _httpClient.GetStringAsync(furl);
            JObject resJson = JObject.Parse(response);

            ////
            var pages = resJson["pages"]?.ToObject<string[]>();
            var marksJson = resJson["bookmarks"]?.ToObject<JObject[]>();
            var marks = marksJson?.Select(x => $"{x["name"]}:{x["page"]}").ToArray();
            ////

            if (pages == null || pages.Length == 0 || marks == null) throw new IOException("Invaild Comic!");

            pages.Sort((a, b) => alphanumComparator.Compare(a, b));

            ComicMetadata n = new ComicMetadata() { 
                Name = name, Pages = string.Join('\n', pages),
                TotalPages = pages.Length, Progress = 0, Marks = string.Join('\n', marks) };
            
            await _database.InsertOrReplaceAsync(n);
            return n;
        }

        public async Task<string[]> GetCollectionsAsync()
        {
            var connective = await _connectivityService.CheckUrlWithTimeoutAsync(BaseUrl);
            IsOffLine = !connective.IsConnected;

            List<string> names = [];
            try
            {
                if (!IsOffLine)
                {
                    var response = await _httpClient.GetAsync("api/image/collections");
                    response.EnsureSuccessStatusCode();
                    var c = await response.Content.ReadFromJsonAsync<List<string>>();
                    if (c is not null)
                        names.AddRange(c);
                }
            }
            catch (Exception) { }

            names.AddRange(Directory.EnumerateDirectories(CachePath).Select(x => x.Split(Path.DirectorySeparatorChar)[^1]));
            names = names.Distinct().ToList();

            return names.ToArray();
        }

        public async Task UpdateProgressAsync(string name, int progress)
        {
            await _database.ExecuteAsync("UPDATE ComicMetadata SET Progress = ? WHERE Name = ?", progress, name);
        }

        public async Task<string[]> GetVideoClasses()
        {
            var r = await _httpClient.GetFromJsonAsync<string[]>("api/video/collections");
            if (r == null)
                return [];
            return r.Select(x => x.Split('/')[0]).Distinct().ToArray();
        }

        public async Task<string[]> GetVideos(string vclass)
        {
            var r = await _httpClient.GetFromJsonAsync<string[]>("api/video/collections");

            if (r == null)
                return [];

            var fr = r.Where(x => x.Split('/')[0] == vclass || vclass == "").ToList();
            fr.Sort((a, b) => alphanumComparator.Compare(a, b));         

            return fr.ToArray();
        }

        public async Task<Video> ResloveVideosAsync(string v)
        {
            var u = BuildUrlWithParameters("api/video/file", new Dictionary<string, string>
            {
                ["file"] = Regex.Replace(v, @"\.[^\.]*$", ".jpg")
            }).Replace("https", "http").Replace(":443", "");

            Console.WriteLine(u);
            try
            {
                byte[] img = await _httpClient.GetByteArrayAsync(u);
                if (img == null) throw new Exception($"Error Video {v}");

                return new Video()
                {
                    Name = Regex.Replace(v.Split("/")[^1], @"\.[^\.]*$", ""),
                    Uri = BuildUrlWithParameters("api/video/file", new Dictionary<string, string>
                    {
                        ["file"] = v
                    }).Replace("https", "http").Replace(":443", ""),
                    Cover = ImageSource.FromStream(() => new MemoryStream(img)) as StreamImageSource,
                    Resolvable = v
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(u);
                return new Video() { Name = "", Cover = "", Uri = "", Resolvable = "" };
            }
        }

        public MetadataService(HttpClient httpClient, ConnectivityService connectivityService)
        {
            _httpClient = httpClient;
            _connectivityService = connectivityService;

            _database = new SQLiteAsyncConnection(DatabasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);

            _database.CreateTableAsync<ComicMetadata>().Wait();

            if (!Directory.Exists(CachePath))
                Directory.CreateDirectory(CachePath);
        }
    }
}
