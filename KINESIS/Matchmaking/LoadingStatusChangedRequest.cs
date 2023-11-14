namespace KINESIS.Matchmaking;

public class LoadingStatusChangedRequest : ProtocolRequest<ConnectedClient>
{
    private readonly byte _loadingStatus;

    public LoadingStatusChangedRequest(byte loadingStatus)
    {
        _loadingStatus = loadingStatus;
    }

    public static LoadingStatusChangedRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        LoadingStatusChangedRequest matchmakingGroupPlayerReadyStatus = new LoadingStatusChangedRequest(
            loadingStatus: ReadByte(data, offset, out offset)
        );
        updatedOffset = offset;
        return matchmakingGroupPlayerReadyStatus;
    }

    public override void HandleRequest(IDbContextFactory<BountyContext> dbContextFactory, ConnectedClient connectedClient)
    {
        MatchmakingGroup? matchmakingGroup = connectedClient.MatchmakingGroup;
        if (matchmakingGroup != null)
        {
            matchmakingGroup.NotifyLoadingStatusChanged(connectedClient.AccountId, _loadingStatus);
        }
    }
}
