using OnkelMato.BlogEngine.Core.Model;

namespace OnkelMato.BlogEngine.Pages.PostAdmin;

/// <summary>
/// Provides extension methods for converting between ShowState and ShowStateModel enumeration values.
/// </summary>
/// <remarks>These methods enable seamless mapping between the ShowState and ShowStateModel types, facilitating
/// interoperability when working with different representations of show state within the application.</remarks>
public static class ShowStateExtensions
{
    public static ShowStateModel FromShowState(this ShowState showState)
    {
        return showState switch
        {
            ShowState.None => ShowStateModel.None,
            ShowState.Blog => ShowStateModel.Blog,
            ShowState.Menu => ShowStateModel.Menu,
            ShowState.Footer => ShowStateModel.Footer,

            ShowState.BlogAndMenu => ShowStateModel.BlogAndMenu,
            ShowState.BlogAndFooter => ShowStateModel.Blog,
            ShowState.LinkAndMenu => ShowStateModel.LinkAndMenu,
            ShowState.LinkAndFooter => ShowStateModel.LinkAndFooter,

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
            ShowStateModel.Footer => ShowState.Footer,

            ShowStateModel.BlogAndMenu => ShowState.BlogAndMenu,
            ShowStateModel.BlogAndFooter => ShowState.BlogAndFooter,
            ShowStateModel.LinkAndMenu => ShowState.LinkAndMenu,
            ShowStateModel.LinkAndFooter => ShowState.LinkAndFooter,

            _ => throw new ArgumentOutOfRangeException(nameof(showState), showState, null)
        };
    }
}