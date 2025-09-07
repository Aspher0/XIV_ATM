namespace XIVATM.Structs;

public class MessageToSend
{
    public bool ShouldSendMessage { get; set; }
    public string Message { get; set; }

    public MessageToSend(bool shouldSend = false, string message = "")
    {
        Message = message;
        ShouldSendMessage = shouldSend;
    }
}
