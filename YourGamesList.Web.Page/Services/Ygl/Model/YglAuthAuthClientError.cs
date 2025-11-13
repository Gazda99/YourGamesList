namespace YourGamesList.Web.Page.Services.Ygl.Model;

public enum YglAuthAuthClientError
{
    General,
    
    RegisterUserAlreadyExists,
    RegisterWeakPassword,
    
    LoginUserNotFound,
    LoginUnauthorized,
    
    DeleteUserNotFound
    
}