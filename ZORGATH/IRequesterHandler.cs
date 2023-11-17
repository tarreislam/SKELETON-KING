﻿namespace ZORGATH;

/// <summary>
///    Interface for a `client_requester.php` function request handlers.
/// </summary>
public interface IRequesterHandler
{
    /// <summary>
    ///    Handles a `client_requester.php` request with the given `formData`
    ///    and returns the appropriate result.
    /// </summary>
    public Task<IActionResult> HandleRequest(ControllerContext controllerContext, Dictionary<string, string> formData);
}
