using System.Collections.Generic;
using System.Threading.Tasks;
using CosmicBackend.Core;
using CosmicBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CosmicBackend.Controllers
{
    [ApiController]
    public class AccessController : ControllerBase
    {
        [NoAuthorizationRequired]
        [HttpGet("/datarouter/api/v1/public/data")]
        public IActionResult DatarouterHandler() => NoContent();

        [HttpGet("/eulatracking/api/public/agreements/fn/account/*")]
        public IActionResult EulaHandler() => Ok();

        [HttpGet("/waitingroom/api/waitingroom")]
        public IActionResult WaitingRoomHandler() => NoContent();

        [HttpGet("/fortnite/api/v2/versioncheck/Windows")]
        public IActionResult VersionCheckHandler() => Ok(new JsonResult(new { type = "NO_UPDATE" }).Value);

        [HttpGet("/lightswitch/api/service/bulk/status")]
        public IActionResult StatusHandler()
        {
            var status = new
                             {
                                 serviceInstanceId = "fortnite",
                                 status = "UP",
                                 maintenanceUri = (string)null,
                                 overrideCatalogIds = new string[1] { "a7f138b2e51945ffbfdacc1af0541053" },
                                 allowedActions = new string[2] { "PLAY", "DOWNLOAD" },
                                 banned = false,
                                 launcherInfoDTO = new
                                                       {
                                                           appName = "Fortnite",
                                                           catalogItemId = "4fe75bbc5a674f4f9b356b5c90567da5",
                                                           @namespace = "fn"
                                                       },
                             };
            return Ok(new JsonResult(status).Value);
        }

        [HttpGet("/fortnite/api/cloudstorage/system")]
        public IActionResult CloudstorageHandler()
        {
            List<CloudstorageFile> cloudstorageFiles = new();
            return Ok(new JsonResult(cloudstorageFiles).Value);
        }

        [HttpGet("/fortnite/api/cloudstorage/system/{file}")]
        public async Task<IActionResult> CloudstorageFileHandler()
        {
            string url = Request.Path.Value;
            string file = url[(url.LastIndexOf('/') + 1)..];
            if (!System.IO.File.Exists(file))
            {
                Response.Headers["Content-Type"] = "application/json";
                return NotFound(Utilities.SetError("The server couldn't find the file you requested.","cosmic.cloudstorage.file_not_found", 404).Value);
            }

            Response.Headers["Content-Type"] = "application/octet-stream";
            await Response.WriteAsync(await System.IO.File.ReadAllTextAsync(file).ConfigureAwait(false))
                          .ConfigureAwait(false);
            return Ok();
        }
    }
}