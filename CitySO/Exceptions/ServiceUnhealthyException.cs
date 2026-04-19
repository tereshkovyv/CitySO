using VkNet.Model;

namespace CitySO.Exceptions;

public class ServiceUnhealthyException(string message) : Exception
{
    public override string Message => message;
}