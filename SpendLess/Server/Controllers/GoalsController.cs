using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendLess.Shared;


namespace SpendLess.Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class GoalsController : ControllerBase
	{
		private readonly SpendLessContext _context;

		public GoalsController(SpendLessContext context) 
		{
			_context = context;
		}

		[HttpGet("GetGoals")]
		public async Task<ActionResult<List<Goal>>> GetGoals()
		{
			var goals = await _context.Goals.ToListAsync();
			return Ok(goals);
		}

		[HttpPost("AddGoal")]
		public async Task<ActionResult<int?>> AddGoal([FromBody] Goal goal)
		{
			var header = Request.Headers.FirstOrDefault(h => h.Key.Equals("Authorization"));
			_context.Goals.Add(goal);
			await _context.SaveChangesAsync();

			return Ok(goal.Id);
		}

        [HttpPut("PutGoal")]
        public async Task UpdateGoal(Goal goal)
        {
            _context.Goals.Attach(goal);
            _context.Goals.Update(goal);
            await _context.SaveChangesAsync();
        }

    }
}
