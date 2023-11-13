using KINESIS.Client;
using KINESIS.Gamefinder;
using KINESIS.Matchmaking;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace KINESIS;

public class ChatServer
{
    private readonly IDbContextFactory<BountyContext> _dbContextFactory;
    private readonly IProtocolRequestFactory<ConnectedClient> _clientProtocolRequestFactory;
    private readonly TcpListener _clientListener;

    public static readonly ConcurrentDictionary<int, ConnectedClient> ConnectedClientsByAccountId = new();
    public static readonly ConcurrentDictionary<int, ChatChannel> ChatChannelsByChannelId = new();
    public static readonly ConcurrentDictionary<string, ChatChannel> ChatChannelsByUpperCaseName = new();

    public static MatchmakingSettingsResponse MatchmakingSettingsResponse = null!;

    public ChatServer(IDbContextFactory<BountyContext> dbContextFactory, IConfiguration configuration)
    {
        _dbContextFactory = dbContextFactory;

        // TODO: make ports configurable.
        _clientListener = new TcpListener(IPAddress.Any, 11031);
        _clientProtocolRequestFactory = new ClientProtocolRequestFactory();

        MatchmakingSettingsResponse = CreateMatchmakingSettingsResponse(configuration);
    }

    public void Start()
    {
        _clientListener.Start();
        _clientListener.BeginAcceptSocket(AcceptClientSocketCallback, this);
    }

    public void Stop()
    {
        _clientListener.Stop();
    }

    public static void AcceptClientSocketCallback(IAsyncResult result)
    {
        ChatServer chatServer = (result.AsyncState as ChatServer)!;

        TcpListener clientListener = chatServer._clientListener;
        Socket socket;
        try
        {
            socket = clientListener.EndAcceptSocket(result);

            ConnectedClient client = new(socket, chatServer._clientProtocolRequestFactory, chatServer._dbContextFactory);
            client.Start();
        }
        finally
        {
            // Prepare for another connection.
            clientListener.BeginAcceptSocket(AcceptClientSocketCallback, chatServer);
        }
    }

    private static MatchmakingSettingsResponse CreateMatchmakingSettingsResponse(IConfiguration configuration)
    {
        string BasePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "Matchmaking");
        MatchmakingConfiguration matchmakingConfiguration = System.Text.Json.JsonSerializer.Deserialize<MatchmakingConfiguration>(File.ReadAllText(Path.Combine(BasePath, "MatchmakingConfiguration.JSON")))!;

        MatchmakingMapConfiguration caldavar = matchmakingConfiguration.Caldavar;
        MatchmakingMapConfiguration midwars = matchmakingConfiguration.MidWars;

        string enabledRegions = string.Join('|', caldavar.Regions);

        // This list appears to only be used by the old UI and must match
        // the maps/modes enabled below.
        List<TMMGameType> enabledGameTypes = new()
            {
                TMMGameType.MIDWARS,
                TMMGameType.CAMPAIGN_NORMAL
            };

        List<string> enabledMaps = new();
        if (caldavar.Modes.Length != 0)
        {
            enabledMaps.Add("caldavar");
        }
        if (midwars.Modes.Length != 0)
        {
            enabledMaps.Add("midwars");
        }

        HashSet<string> enabledGameModes = new();
        foreach (string mode in caldavar.Modes)
        {
            enabledGameModes.Add(mode);
        }
        foreach (string mode in midwars.Modes)
        {
            enabledGameModes.Add(mode);
        }

        List<string> unsupportedCombinations = new();
        foreach (string mode in enabledGameModes)
        {
            if (!midwars.Modes.Contains(mode))
            {
                unsupportedCombinations.Add("con->" + mode);
            }
            if (!midwars.Modes.Contains(mode))
            {
                unsupportedCombinations.Add("midwars->" + mode);
            }
        }

        int numberOfEnabledRegions = 0;
        if (enabledRegions.Length != 0)
        {
            numberOfEnabledRegions = enabledRegions.Count(c => c == '|') + 1;
        }

        return new MatchmakingSettingsResponse(
            matchmakingAvailability: numberOfEnabledRegions == 0 ? (byte)0 : (byte)1,
            availableMaps: string.Join("|", enabledMaps),
            gameTypes: string.Join("|", enabledGameTypes),
            gameModes: string.Join("|", enabledGameModes),
            availableRegions: enabledRegions,
            disabledGameModesByGameType: "",
            disabledGameModesByRankType: "",
            disabledGameModesByMap: string.Join("|", unsupportedCombinations),
            restrictedRegions: "",
            clientCountryCode: "",
            legend: "maps:modes:regions:",
            popularityByGameMap: new byte[enabledMaps.Count],
            popularityByGameType: new byte[enabledGameTypes.Count],
            popularityByGameMode: new byte[enabledGameModes.Count],
            popularityByRegion: new byte[numberOfEnabledRegions],
            customMapRotationTime: 0
        );
    }
}
