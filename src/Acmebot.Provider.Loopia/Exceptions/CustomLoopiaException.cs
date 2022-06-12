using System;

namespace Acmebot.Provider.Loopia.Exceptions;

public class CustomLoopiaException : Exception
{
    public CustomLoopiaException(string message) : base(message)
    {

    }
}