using System.ComponentModel.DataAnnotations;

namespace OnkelMato.BlogEngine.Pages.PostAdmin;

public enum ShowStateModel
{
    [Display(Name = "Unpublished")] None = 0,
    [Display(Name = "Show in Blog")] Blog = 1,
    [Display(Name = "Show in Menu")] Menu = 2,
    [Display(Name = "Show in Footer")] Footer = 8,

    [Display(Name = "Show in Blog an Menu")] BlogAndMenu = 3,
    [Display(Name = "Show in Blog and Footer")] BlogAndFooter = 9,
    [Display(Name = "Show in Link and Menu")] LinkAndMenu = 6,
    [Display(Name = "Show in Link and Footer")] LinkAndFooter = 12,
}