using Inventory_Managment.Models;
using Inventory_Managment.Models.Dto;
using Inventory_Managment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Managment.Controllers
{
    public class DiscussionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public DiscussionController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Posts(int inventoryId, int after = 0)
        {
            var userId = _userManager.GetUserId(User);

            var posts = await _context.DiscussionPosts
                .Where(p => p.InventoryId == inventoryId && p.Id > after)
                .OrderBy(p => p.Id)
                .Select(p => new PostDto
                {
                    Id = p.Id,
                    UserName = p.User.Name,
                    UserId = p.UserId,
                    Text = p.Text,
                    CreatedAt = p.CreatedAt,
                    LikeCount = p.Likes.Count,
                    LikedByMe = p.Likes.Any(l => l.UserId == userId)
                })
                .ToListAsync();

            return Ok(posts);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddPost([FromBody] AddPostDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Text))
                return BadRequest("Post text cannot be empty.");

            var userId = _userManager.GetUserId(User)!;
            var post = new DiscussionPost
            {
                InventoryId = dto.InventoryId,
                UserId = userId,
                Text = dto.Text.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.DiscussionPosts.Add(post);
            await _context.SaveChangesAsync();

            var user = await _userManager.GetUserAsync(User);
            return Ok(new PostDto
            {
                Id = post.Id,
                UserName = user!.Name,
                UserId = userId,
                Text = post.Text,
                CreatedAt = post.CreatedAt,
                LikeCount = 0,
                LikedByMe = false
            });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ToggleLike([FromBody] int postId)
        {
            var userId = _userManager.GetUserId(User)!;
            var existing = await _context.PostLikes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (existing != null)
                _context.PostLikes.Remove(existing);
            else
                _context.PostLikes.Add(new PostLike { PostId = postId, UserId = userId });

            await _context.SaveChangesAsync();

            var count = await _context.PostLikes.CountAsync(l => l.PostId == postId);
            return Ok(new { liked = existing == null, count });
        }
    }
}
