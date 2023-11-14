namespace KINESIS.Matchmaking;

public class RefreshMatchmakingSettingsRequest : ProtocolRequest<ConnectedClient>
{
    public static RefreshMatchmakingSettingsRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        updatedOffset = offset;
        return new();
    }

    public override void HandleRequest(IDbContextFactory<BountyContext> dbContextFactory, ConnectedClient connectedClient)
    {
        connectedClient.SendResponse(ChatServer.MatchmakingSettingsResponse);
    }
}
