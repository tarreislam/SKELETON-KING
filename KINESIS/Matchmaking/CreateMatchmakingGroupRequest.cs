namespace KINESIS.Matchmaking;

public class CreateMatchmakingGroupRequest : ProtocolRequest<ConnectedClient>
{
    private readonly string Version;
    private readonly byte GroupType; // 2 - midwars/riftwars, 3 - botmatch, 4 - ranked.
    private readonly GameFinder.TMMGameType GameType;
    private readonly string MapNames;
    private readonly string GameModes;
    private readonly string Regions;
    private readonly byte Ranked;
    private readonly byte MatchFidelity;
    private readonly byte BotDifficulty;
    private readonly byte RandomizeBots;

    public CreateMatchmakingGroupRequest(string version, byte groupType, GameFinder.TMMGameType gameType, string mapNames, string gameModes, string regions, byte ranked, byte matchFidelity, byte botDifficulty, byte randomizeBots)
    {
        Version = version;
        GroupType = groupType;
        GameType = gameType;
        MapNames = mapNames;
        GameModes = gameModes;
        Regions = regions;
        Ranked = ranked;
        MatchFidelity = matchFidelity;
        BotDifficulty = botDifficulty;
        RandomizeBots = randomizeBots;
    }

    public static CreateMatchmakingGroupRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        CreateMatchmakingGroupRequest createMatchmakingGroupRequest = new CreateMatchmakingGroupRequest(
            version: ReadString(data, offset, out offset),
            groupType: ReadByte(data, offset, out offset),
            gameType: (GameFinder.TMMGameType)ReadByte(data, offset, out offset),
            mapNames: ReadString(data, offset, out offset),
            gameModes: ReadString(data, offset, out offset),
            regions: ReadString(data, offset, out offset),
            ranked: ReadByte(data, offset, out offset),
            matchFidelity: ReadByte(data, offset, out offset),
            botDifficulty: ReadByte(data, offset, out offset),
            randomizeBots: ReadByte(data, offset, out offset)
        );
        updatedOffset = offset;
        return createMatchmakingGroupRequest;
    }

    public override void HandleRequest(IDbContextFactory<BountyContext> dbContextFactory, ConnectedClient connectedClient)
    {
        // See if we need to reconnect to our last game.
        if (connectedClient.ReconnectToLastGame())
        {
            return;
        }

        // TODO: check if the player is temporarily suspended from the matchmaking due to e.g. frequent failures.

        MatchmakingGroup matchmakingGroup = new MatchmakingGroup(GroupType, GameType, GameModes, Regions, Ranked, MatchFidelity, BotDifficulty, RandomizeBots, maxGroupSize: 5);
        MatchmakingFailedToJoinReason failureReason;
        if (matchmakingGroup.AddParticipant(connectedClient, out failureReason))
        {
            MatchmakingGroup? oldMatchmakingGroup = connectedClient.ReplaceMatchmakingGroup(matchmakingGroup);
            if (oldMatchmakingGroup != null)
            {
                oldMatchmakingGroup.RemoveParticipant(connectedClient);
            }
        }
        else
        {
            // Should never happen when joining a new group.
            connectedClient.SendResponse(new MatchmakingFailedToJoinResponse(failureReason));
        }
    }
}
