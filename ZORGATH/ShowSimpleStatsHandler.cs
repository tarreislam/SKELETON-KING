﻿namespace ZORGATH;

public record ShowSimpleStatsData(
    int TotalLevel,
    int TotalExperience,
    int NumberOfHeroesOwned,
    int TotalMatchesPlayed,
    CombinedPlayerAwardSummary CombinedPlayerAwardSummary,
    ICollection<string> SelectedUpgradeCodes,
    ICollection<string> UnlockedUpgradeCodes,
    int AccountId,
    int SeasonId,
    SeasonShortSummary SeasonNormal,
    SeasonShortSummary SeasonCasual);

public class ShowSimpleStatsHandler : IRequesterHandler
{
    private const int NumberOfHeroesTheGameHas = 139;

    // In Heroes of Newerth, there is a slightly different logic when computing stats for Seasons <= 6 and Seasons > 6.
    // Additionally, season id must be < 1000. 22 was chosen somewhat arbitrarily and primarily because revival started
    // in the year 2022, but anything > 6 should work just as well.
    private const int CurrentSeason = 22;

    public async Task<IActionResult> HandleRequest(ControllerContext controllerContext, Dictionary<string, string> formData)
    {
        using var bountyContext = controllerContext.HttpContext.RequestServices.GetRequiredService<BountyContext>();
        string cookie = formData["cookie"];

        // Validate cookie.
        if (!await bountyContext.Accounts.AnyAsync(a => a.Cookie == cookie))
        {
            // Invalid cookie.
            return new UnauthorizedResult();
        }

        string nickname = formData["nickname"];
        ShowSimpleStatsData? data = await bountyContext.Accounts
            .Where(account => account.Name == nickname)
            .Select(account => new ShowSimpleStatsData(
                /* TotalLevel: */ 0,
                /* TotalExperience: */ 0,
                /* NumberOfHeroesOwned: */ NumberOfHeroesTheGameHas,
                /* TotalMatchesPlayed: */ 
                    // Public
                    account.PlayerSeasonStatsPublic.Wins + account.PlayerSeasonStatsPublic.Losses +
                    // Ranked
                    account.PlayerSeasonStatsRanked.Wins + account.PlayerSeasonStatsRanked.Losses +
                    // Casual
                    account.PlayerSeasonStatsRankedCasual.Wins + account.PlayerSeasonStatsRankedCasual.Losses +
                    // Mid Wars
                    account.PlayerSeasonStatsMidWars.Wins + account.PlayerSeasonStatsMidWars.Losses,
                /* PlayerAwardSummary: */ CombinedPlayerAwardSummary.AddUp(account.PlayerSeasonStatsPublic.PlayerAwardSummary, account.PlayerSeasonStatsRanked.PlayerAwardSummary, account.PlayerSeasonStatsRankedCasual.PlayerAwardSummary, account.PlayerSeasonStatsMidWars.PlayerAwardSummary),
                /* SelectedUpgrades: */ account.SelectedUpgradeCodes,
                /* UnlockedUpgradeCodes: */ account.User.UnlockedUpgradeCodes,
                /* AccountId: */ account.AccountId,
                /* SeasonId: */ CurrentSeason,
                /* SeasonNormal: */ new SeasonShortSummary(account.PlayerSeasonStatsRanked.Wins + account.PlayerSeasonStatsMidWars.Wins, account.PlayerSeasonStatsRanked.Losses + account.PlayerSeasonStatsMidWars.Losses, 0, 0),
                /* SeasonCasual: */ new SeasonShortSummary(account.PlayerSeasonStatsRankedCasual.Wins, account.PlayerSeasonStatsRankedCasual.Losses, 0, 0))
            )
            .FirstOrDefaultAsync();

        if (data is null)
        {
            return new NotFoundResult();
        }

        // TODO: include awards once we have stats.
        ShowSimpleStatsResponse showSimpleStatsResponse = new(
            nickname,
            data.TotalLevel,
            data.TotalExperience,
            data.NumberOfHeroesOwned,
            data.UnlockedUpgradeCodes.Count(upgrade => upgrade.StartsWith("aa.")),
            data.TotalMatchesPlayed,
            data.CombinedPlayerAwardSummary.MVP,
            data.SelectedUpgradeCodes,
            data.AccountId,
            data.SeasonId,
            data.SeasonNormal,
            data.SeasonCasual,
            data.CombinedPlayerAwardSummary.Top4Names,
            data.CombinedPlayerAwardSummary.Top4Nums
        );
        return new OkObjectResult(PHP.Serialize(showSimpleStatsResponse));
    }
}
