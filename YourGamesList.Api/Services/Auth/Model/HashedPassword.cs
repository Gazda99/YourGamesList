namespace YourGamesList.Api.Services.Auth.Model;

public class HashedPassword
{
    public string HashString { get; }
    public byte[] Salt { get; }

    public HashedPassword(string hashString, byte[] salt)
    {
        HashString = hashString;
        Salt = salt;
    }
}