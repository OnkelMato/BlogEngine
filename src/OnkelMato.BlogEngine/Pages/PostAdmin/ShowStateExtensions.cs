using OnkelMato.BlogEngine.Database;

namespace OnkelMato.BlogEngine.Pages.PostAdmin;

public static class ShowStateExtensions
{
    public static ShowStateModel ToShowStateModel(this ShowState showState)
    {
        return showState switch
        {
            ShowState.None => ShowStateModel.None,
            ShowState.Blog => ShowStateModel.Blog,
            ShowState.Menu => ShowStateModel.Menu,
            ShowState.BlogAndMenu => ShowStateModel.BlogAndMenu,
            ShowState.Link => ShowStateModel.Link,
            _ => throw new ArgumentOutOfRangeException(nameof(showState), showState, null)
        };
    }

    public static ShowState ToShowState(this ShowStateModel showState)
    {
        return showState switch
        {
            ShowStateModel.None => ShowState.None,
            ShowStateModel.Blog => ShowState.Blog,
            ShowStateModel.Menu => ShowState.Menu,
            ShowStateModel.BlogAndMenu => ShowState.BlogAndMenu,
            ShowStateModel.Link => ShowState.Link,
            _ => throw new ArgumentOutOfRangeException(nameof(showState), showState, null)
        };
    }
}