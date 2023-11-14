namespace KINESIS.Matchmaking;

public class LeaveMatchmakingQueueRequest : ProtocolRequest<ConnectedClient>
{
    public static LeaveMatchmakingQueueRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        updatedOffset = offset;
        return new();
    }

    public override void HandleRequest(IDbContextFactory<BountyContext> dbContextFactory, ConnectedClient connectedClient)
    {
        MatchmakingGroup? matchmakingGroup = connectedClient.MatchmakingGroup;
        if (matchmakingGroup != null)
        {
            matchmakingGroup.LeaveQueue(initiatedByGameFinder: false);
        }
    }
}
