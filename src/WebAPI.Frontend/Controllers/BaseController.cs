namespace WebAPI.Frontend.Controllers;

/// <summary>
/// The base controller for the API. All other controllers should inherit from this.
/// </summary>
/// <remarks>
/// This controller sets up common behaviors for all controllers, such as routing and response types.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class BaseController : ControllerBase
{
}