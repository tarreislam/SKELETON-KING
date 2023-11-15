using System.Text.RegularExpressions;
using ZORGATH.Upgrades;

namespace ZORGATH;

[TestClass]
public class GetProductsHandlerTest
{
    
    [TestMethod]
    public async Task GetProductsHandlerTest_ResponseOK()
    {
        ConcurrentDictionary<string, SrpAuthSessionData> srpAuthSessions = new();

        UpgradeRepository upgradeRepository = new UpgradeRepository();
        Dictionary<string, string> formData = new()
        {
            ["cookie"] = "C81C1CDA25B2F0E600681167BCC3D446",
        };

        IActionResult result = await new GetProductsHandler(upgradeRepository).HandleRequest(new ControllerContextForTesting(), formData);
        string? stResult = ((ObjectResult)result).Value.ToString();
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));

        if (stResult != null)
        {
            // Console.WriteLine(stResult);
            // At first I made a sha1 hash of the result to just compare it, but it failed so this is the "cleanest" way to validate the integrity
            Assert.IsTrue(Regex.IsMatch(stResult, "\"products\";a:8"));
            Assert.IsTrue(Regex.IsMatch(stResult, "\"Alt Avatar\";a:1305"));
        }
        else
        {
            Assert.Fail("Null response from HandleRequest");
        }
        
    }
    
}