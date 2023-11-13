namespace KINESIS;

public class ChatServerRequest
{
    public const short Connect = 0x0C00;
    public const short JoinChatChannel = 0x001E;
    public const short LeaveChatChannel = 0x0022;
    public const short SendChatChannelMessage = 0x0003;
    public const short RefreshMatchmakingStats = 0x0F07;
    public const short RefreshMatchmakingSettings = 0x0D07;
    public const short TrackPlayerAction = 0x00B9;
}
