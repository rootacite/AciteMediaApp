using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace AciteLightApp.Controllers;
using System.IO;

[ApiController]
[Route("api/[controller]")]
public class ImageController : Controller
{
    private static readonly string ImageFolder = Path.Combine("/opt", "Images");

    [HttpGet("collections")]
    public IActionResult GetCollection()
    {
        var imageCollections = Directory.GetDirectories(ImageFolder, "*");
        return Ok(imageCollections.Select(x => Path.GetFileName(x)).ToList());
    }

    [HttpGet("meta")]
    public IActionResult GetList(string collection)
    {
        var imageFiles = Directory.GetFiles(Path.Combine(ImageFolder, collection), "*.jpeg");
        var metaData = System.IO.File.ReadAllText(Path.Combine(ImageFolder, collection, "summary.json"));
        var imageUrls = imageFiles.Select(Path.GetFileName).ToList();
        
        JObject comicJson = JObject.Parse(metaData);
        JArray jArray = new JArray(imageUrls);
        comicJson["pages"] = jArray;
        
        return Ok(comicJson.ToString());
    }

    [HttpGet("file")]
    public IActionResult GetImage(string collection, string file)
    {
        return File(System.IO.File.Open(Path.Combine(ImageFolder, collection, file), FileMode.Open, FileAccess.Read), $"image/{file.Split('.')[^1]}");
    }
}