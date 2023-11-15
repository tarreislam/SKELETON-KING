using ZORGATH.Upgrades;

namespace ZORGATH;

public class GetProductsHandler: IClientRequesterHandler
{
    private readonly IUpgradeRepository _upgradeRepository;

    public GetProductsHandler(IUpgradeRepository upgradeRepository)
    {
        _upgradeRepository = upgradeRepository;
    }
    public Task<IActionResult> HandleRequest(ControllerContext controllerContext, Dictionary<string, string> formData)
    {
        // maybe even now introduce a repository for accounts, these 3 lines will be called many times. Also, why even do this check here? We should protect with middlewares instead. That will also keep the handlers nice and tidy
        // using BountyContext bountyContext = controllerContext.HttpContext.RequestServices.GetRequiredService<BountyContext>();
        // Account? account = await bountyContext.Accounts.FirstOrDefaultAsync(a => a.Cookie == formData["cookie"]);
        //if (account is null)
        //{
        //     return new UnauthorizedResult();
        //}
        
        var res = _upgradeRepository.GetProductsForClient();
        
        return Task.FromResult<IActionResult>(new OkObjectResult(PhpSerialization.Serialize(res)));
    }
}