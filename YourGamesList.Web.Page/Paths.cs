namespace YourGamesList.Web.Page;

public static class Paths
{
    public const string Home = "";
    public const string SearchGames = "searchGames";
    public const string User = "user";
    public const string Login = "login";
    public const string Register = "register";
    public const string Lists = "lists";

    public static string ViewList(string listId) => $"{Lists}/{listId}";
}