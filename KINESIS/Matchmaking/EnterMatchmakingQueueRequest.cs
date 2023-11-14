namespace KINESIS.Matchmaking;

public class EnterMatchmakingQueueRequest : ProtocolRequest<ConnectedClient>
{
    private readonly byte _readyStatus;
    private readonly byte _gameType;

    public EnterMatchmakingQueueRequest(byte readyStatus, byte gameType)
    {
        _readyStatus = readyStatus;
        _gameType = gameType;
    }

    public static EnterMatchmakingQueueRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        EnterMatchmakingQueueRequest enterMatchmakingQueueRequest = new EnterMatchmakingQueueRequest(
            readyStatus: ReadByte(data, offset, out offset),
            gameType: ReadByte(data, offset, out offset)
        );
        updatedOffset = offset;
        return enterMatchmakingQueueRequest;
    }

    public override void HandleRequest(IDbContextFactory<BountyContext> dbContextFactory, ConnectedClient connectedClient)
    {
        if (_readyStatus == 0)
        {
            // TODO: does this happen? should we leave the queue?
            return;
        }

        MatchmakingGroup? matchmakingGroup = connectedClient.MatchmakingGroup;
        if (matchmakingGroup != null)
        {
            matchmakingGroup.EnterQueue(dbContextFactory);
        }
    }
}
