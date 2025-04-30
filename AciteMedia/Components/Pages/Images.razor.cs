using System.Collections;
using System.Text;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Collections;
using System.Text;
using Microsoft.JSInterop;
using MudBlazor;
using Newtonsoft.Json.Linq;

namespace AciteMedia.Components.Pages;

public class AlphanumComparator : IComparer
{
    private enum ChunkType
    {
        Alphanumeric,
        Numeric
    };

    private static bool InChunk(char ch, char otherCh)
    {
        var type = ChunkType.Alphanumeric;

        if (char.IsDigit(otherCh))
        {
            type = ChunkType.Numeric;
        }

        return (type != ChunkType.Alphanumeric || !char.IsDigit(ch))
               && (type != ChunkType.Numeric || char.IsDigit(ch));
    }

    public int Compare(object? x, object? y)
    {
        if (x is not string s1 || y is not string s2)
        {
            return 0;
        }

        int thisMarker = 0;
        int thatMarker = 0;

        while ((thisMarker < s1.Length) || (thatMarker < s2.Length))
        {
            if (thisMarker >= s1.Length)
            {
                return -1;
            }
            else if (thatMarker >= s2.Length)
            {
                return 1;
            }

            var thisCh = s1[thisMarker];
            var thatCh = s2[thatMarker];

            var thisChunk = new StringBuilder();
            var thatChunk = new StringBuilder();

            while ((thisMarker < s1.Length) && (thisChunk.Length == 0 || InChunk(thisCh, thisChunk[0])))
            {
                thisChunk.Append(thisCh);
                thisMarker++;

                if (thisMarker < s1.Length)
                {
                    thisCh = s1[thisMarker];
                }
            }

            while ((thatMarker < s2.Length) && (thatChunk.Length == 0 || InChunk(thatCh, thatChunk[0])))
            {
                thatChunk.Append(thatCh);
                thatMarker++;

                if (thatMarker < s2.Length)
                {
                    thatCh = s2[thatMarker];
                }
            }

            var result = 0;
            // If both chunks contain numeric characters, sort them numerically
            if (char.IsDigit(thisChunk[0]) && char.IsDigit(thatChunk[0]))
            {
                var thisNumericChunk = Convert.ToInt32(thisChunk.ToString());
                var thatNumericChunk = Convert.ToInt32(thatChunk.ToString());

                if (thisNumericChunk < thatNumericChunk)
                {
                    result = -1;
                }

                if (thisNumericChunk > thatNumericChunk)
                {
                    result = 1;
                }
            }
            else
            {
                result = String.Compare(thisChunk.ToString(), thatChunk.ToString(), StringComparison.Ordinal);
            }

            if (result != 0)
            {
                return result;
            }
        }

        return 0;
    }
}

public partial class Images : ComponentBase
{
    private ElementReference imageContainer;
    private DotNetObjectReference<Images> dotNetObjRef;
    
    private AlphanumComparator _aa = new AlphanumComparator();
    private List<string>? _collections = [];

    private string _currentCollection = "";

    public string CurrentCollection
    {
        get => _currentCollection;
        set
        {
            _currentCollection = value;
            SetCollection(_currentCollection);
        }
    }

    private List<string>? _images = [];
    
    private int CurrentIndex { get; set; } = 0;
    private string CurrentImageUrl { get; set; } = "";
    private int PageNumber { get; set; } = 0;

    protected override async Task OnInitializedAsync()
    {
        
    }

    void Flush()
    {
        if(_images is null) return;
        PageNumber = CurrentIndex;
        CurrentImageUrl = $"{Http.BaseAddress}api/image/file?file={_images[CurrentIndex]}&collection={CurrentCollection}&t={DateTime.Now.Ticks}";
    }
    
    void NextImage()
    {
        if(_images is null) return;
        if (_images.Any())
        {
            if(CurrentIndex < _images.Count - 1) CurrentIndex++;
            Flush();
        }
    }

    void PrevImage()
    {
        if(_images is null) return;
        if (_images.Any())
        {
            if (CurrentIndex > 0) CurrentIndex--;
            Flush();
        }
    }
    
    [JSInvokable]
    public void HandleImageClick(bool isLeft)
    {
        if (isLeft)
        {
            // 处理左侧点击
            PrevImage();
        }
        else
        {
            // 处理右侧点击
            NextImage();
        }
        StateHasChanged();
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _collections = await Http.GetFromJsonAsync<List<string>>("api/image/collections");
            if (_collections is null || _collections.Count == 0) return;
            _collections.Sort((a, b) => _aa.Compare(a, b));
            CurrentCollection = _collections[0];
            
            dotNetObjRef = DotNetObjectReference.Create(this);
            await JsRuntime.InvokeVoidAsync("registerImageClick", imageContainer, dotNetObjRef);
            StateHasChanged();
        }
    }

    void SetImage()
    {
        if (_images is null) return;
        if (PageNumber >= _images.Count || PageNumber < 0)
        {
            PageNumber = CurrentIndex;
            return;
        }

        CurrentIndex = PageNumber;
        Flush();
    }

    private async Task SetCollection(string v)
    {
        if (CurrentCollection == "") return;
        _images?.Clear();
        var rawMeta = await Http.GetStringAsync($"api/image/meta?collection={CurrentCollection}");
        JObject jsonMeta = JObject.Parse(rawMeta);
        var ll = jsonMeta["pages"]?.ToObject<List<string>>(); 
        if(ll is null) return;
        
        _images = ll;
        if (_images is null) return;
        _images.Sort((a, b) => _aa.Compare(a, b));
        CurrentIndex = 0;
        Flush();
        StateHasChanged();
    }

    private void HandleSelectChange(ChangeEventArgs e)
    {
        
    }
}