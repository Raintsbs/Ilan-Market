using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Models;
using AdvertisementApp.Dtos.Platform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace AdvertisementApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableRateLimiting("write")]
    public class MessagesController : ControllerBase
    {
        private readonly IPlatformService _platform;

        public MessagesController(IPlatformService platform) => _platform = platform;

        [HttpGet("threads")]
        [DisableRateLimiting]
        public async Task<IActionResult> GetThreads()
        {
            var items = await _platform.GetThreadsAsync(GetUserId());
            return Ok(ApiResponse<List<MessageThreadDto>>.Ok(items));
        }

        [HttpGet("threads/{threadId:int}")]
        [DisableRateLimiting]
        public async Task<IActionResult> GetMessages(int threadId)
        {
            var items = await _platform.GetMessagesAsync(GetUserId(), threadId);
            return Ok(ApiResponse<List<MessageDto>>.Ok(items));
        }

        [HttpPost]
        public async Task<IActionResult> Send([FromBody] SendMessageDto dto)
        {
            var msg = await _platform.SendMessageAsync(GetUserId(), dto);
            if (msg == null)
                return BadRequest(ApiResponse.Fail("Mesaj gönderilemedi."));
            return Ok(ApiResponse<MessageDto>.Ok(msg, "Mesaj gönderildi."));
        }

        [HttpPost("threads/{threadId:int}")]
        public async Task<IActionResult> SendToThread(int threadId, [FromBody] ThreadReplyDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Body))
                return BadRequest(ApiResponse.Fail("Mesaj metni gerekli."));
            var msg = await _platform.SendThreadMessageAsync(GetUserId(), threadId, dto.Body);
            if (msg == null)
                return BadRequest(ApiResponse.Fail("Mesaj gönderilemedi."));
            return Ok(ApiResponse<MessageDto>.Ok(msg, "Mesaj gönderildi."));
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
