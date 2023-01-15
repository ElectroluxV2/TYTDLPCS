using Microsoft.AspNetCore.Mvc;

namespace TyranoKurwusBot.Controllers;

[ApiController]
[Route("status")]
public class DownloadersStatusController : ControllerBase
{
    [HttpGet(Name = "/")]
    public string Get()
    {
        return "works";
    }
}
