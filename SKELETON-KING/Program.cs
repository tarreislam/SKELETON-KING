using ZORGATH.Upgrades;

namespace SKELETON_KING;

using Microsoft.EntityFrameworkCore;
using KINESIS;
using PUZZLEBOX;
using System.Collections.Concurrent;
using ZORGATH;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();

        string connectionString = builder.Configuration.GetConnectionString("BOUNTY")!;
        builder.Services.AddDbContextFactory<BountyContext>(options =>
        {
            options.UseSqlServer(connectionString, connection => connection.MigrationsAssembly("SKELETON-KING"));
        });

        ConcurrentDictionary<string, SrpAuthSessionData> srpAuthSessions = new();
        UpgradeRepository upgradeRepo = new UpgradeRepository(); // used in more places (plinko etc), therefore it should be oK to load it here. Also it should be a singleton with an interface to be bound to aspnet controllers later on

        builder.Services.AddSingleton<IReadOnlyDictionary<string, IClientRequesterHandler>>(
            new Dictionary<string, IClientRequesterHandler>()
            {
                // NOTE: Please keep this list alphabetized by the string literal in the key.
                {"autocompleteNicks", new AutoCompleteNicksHandler() },
                {"get_match_stats", new GetMatchStatsHandler(replayServerUrl: "http://api.kongor.online") },
                {"get_products", new GetProductsHandler(upgradeRepo)},
                {"match_history_overview", new MatchHistoryOverviewHandler() },
                {"pre_auth", new PreAuthHandler(srpAuthSessions) },
                {"show_simple_stats", new ShowSimpleStatsHandler() },
                {"srpAuth", new SrpAuthHandler(srpAuthSessions, new(), chatServerUrl: "localhost", icbUrl: "kongor.online") },
            }
        );
        builder.Services.AddSingleton<ChatServer>();

        var app = builder.Build();
        app.MapControllers();

        app.Services.GetRequiredService<ChatServer>().Start();
        app.Run();
    }
}
