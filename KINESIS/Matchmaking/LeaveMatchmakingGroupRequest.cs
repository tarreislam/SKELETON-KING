namespace KINESIS.Matchmaking;

public class LeaveMatchmakingGroupRequest : ProtocolRequest<ConnectedClient>
{
    public static LeaveMatchmakingGroupRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        updatedOffset = offset;
        return new();
    }

    public override void HandleRequest(IDbContextFactory<BountyContext> dbContextFactory, ConnectedClient connectedClient)
    {
        MatchmakingGroup? oldMatchmakingGroup = connectedClient.ReplaceMatchmakingGroup(null);
        if (oldMatchmakingGroup != null)
        {
            oldMatchmakingGroup.RemoveParticipant(connectedClient);
        }
    }
}

