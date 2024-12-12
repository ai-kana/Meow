namespace Meow.Core.Commands.Framework;

public class UserMessageException : Exception
{
    public readonly string PlayerMessage;

    public UserMessageException(string message)
    {
        PlayerMessage = message;
    }
}
