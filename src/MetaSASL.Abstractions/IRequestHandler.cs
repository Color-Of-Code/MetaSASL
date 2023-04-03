namespace Sasl.Abstractions;

public interface IRequestHandler
{
    void HandleRequest(ISocket s);
}
