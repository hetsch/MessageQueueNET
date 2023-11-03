namespace MessageQueueNET.Client.Models.Authentication;

public class BasicAuthentication : IAuthentication
{
    public BasicAuthentication(string username, string password)
    {
        Username = username;
        Password = password;
    }

    public string Username { get; }
    public string Password { get; }
}
