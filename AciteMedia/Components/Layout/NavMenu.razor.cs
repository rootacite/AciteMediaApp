
using Microsoft.AspNetCore.Components;

namespace AciteMedia.Components.Layout;

public partial class NavMenu : ComponentBase
{
    protected readonly List<string> Videos = new();
    protected override async Task OnInitializedAsync()
    {
        var b = await http.GetFromJsonAsync<List<string>>($"api/video/collections");
        if (b == null) return;
                
        Videos.AddRange(b.Select(v => v.Split("/")[0]).Distinct());
        StateHasChanged();
    }
}