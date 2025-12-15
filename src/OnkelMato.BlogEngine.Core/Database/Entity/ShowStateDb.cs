namespace OnkelMato.BlogEngine.Core.Database.Entity;

public enum ShowStateDb
{
    None = 0,
    Blog = 1,
    Menu = 2,
    //Link = 4, // Only used in combination
    Footer = 8,

    BlogAndMenu = 3, // Blog == 1, Menu == 2, BlogAndMenu == 3
    BlogAndFooter = 9, // Blog == 1, Footer == 8, BlogAndFooter == 9
    LinkAndMenu = 6, // Link == 4, Menu == 2, LinkAndMenu == 6
    LinkAndFooter = 12, // Link == 4, Footer == 8, LinkAndFooter == 12
}