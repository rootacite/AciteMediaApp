using System.Diagnostics;
using MediaInfo;
using Microsoft.AspNetCore.Mvc;

namespace AciteLightApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideoController(ILogger<VideoController> logger) : Controller
{
    private ILogger<VideoController> _logger = logger;

    private static readonly string VideoFolder = Path.Combine("/opt", "Videos");

    [HttpGet("collections")]
    public IActionResult GetCollection()
    {
        var imageCollections = Directory.GetFiles(VideoFolder, "*.mp4", SearchOption.AllDirectories);
        return Ok(imageCollections.Select(x => Path.GetRelativePath(VideoFolder, x)).ToList());
    }

    [HttpGet("info")]
    public async Task<IActionResult> GetInfo(string file)
    {
        var resolvedPath = Path.Combine(VideoFolder, file);
        var fullPath = Path.GetFullPath(resolvedPath);

        if (!fullPath.StartsWith(VideoFolder + Path.DirectorySeparatorChar))
        {
            return BadRequest("Invalid file path");
        }

        if (!System.IO.File.Exists(fullPath))
        {
            return NotFound();
        }

        TimeSpan info = new();
        
        await Task.Run(() =>
        {
            info = GetVideoDurationWithFFprobe(fullPath);
        });
        
        return Ok(info.ToString());
    }
    
    public static TimeSpan GetVideoDurationWithFFprobe(string filePath)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ffprobe",
                Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{filePath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };
    
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
    
        if (double.TryParse(output, out double seconds))
        {
            return TimeSpan.FromSeconds(seconds);
        }
    
        throw new Exception("无法解析视频时长");
    }
    
    [HttpGet("file")]
    public IActionResult GetVideo(string file)
    {
        // 验证文件路径安全性
        var resolvedPath = Path.Combine(VideoFolder, file);
        var fullPath = Path.GetFullPath(resolvedPath);

        // 防止路径遍历攻击
        if (!fullPath.StartsWith(VideoFolder + Path.DirectorySeparatorChar))
        {
            return BadRequest("Invalid file path");
        }

        if (!System.IO.File.Exists(fullPath))
        {
            return NotFound();
        }

        // 根据扩展名设置Content-Type
        var contentType = file.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) 
            ? "video/mp4" 
            : "image/jpeg";

        // 返回支持范围请求的文件响应
        return PhysicalFile(fullPath, contentType, enableRangeProcessing: true);
    }
}