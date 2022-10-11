// /////////////////////////////////////////////////////////////////////////////
// YOU CAN FREELY MODIFY THE CODE BELOW IN ORDER TO COMPLETE THE TASK
// /////////////////////////////////////////////////////////////////////////////

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using WebApi.DTOs;
using WebApi.Entities;
using WebApi.Helpers;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeamController : ControllerBase
    {
        private readonly DataContext Context;

        public TeamController(DataContext context)
        {
            Context = context;
        }

        [HttpPost("process")]
        public async Task<ActionResult<List<Player>>> Process(List<TeamProcessDTO> teams)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponse("Validation Error"));
                }
                // Check for repeated positions
                if ((teams.Select(x=>x.Position).Count() != teams.Select(x => x.Position).Distinct().Count())) 
                    { return BadRequest(new ErrorResponse("The position of the player should not be repeated!!")); }
                    var result = new List<Player>();
                foreach (var team in teams)
                {
                    // Check for insuffient players with a position
                    if (Context.Players.Where(x => x.Position == team.Position).Count() < Convert.ToInt32(team.NumberOfPlayers))
                    {
                        return BadRequest(new ErrorResponse($"Insufficient number of players for position: {team.Position}"));
                    }
                    
                    else
                    {
                        // Get players with the position and skill required

                        var addPlayer = Context.Players.Where(x => x.Position == team.Position).Where(x=> x.PlayerSkills
                         .Any(x => x.Skill == team.MainSkill)).OrderByDescending(x=>x.PlayerSkills.First(x=>x.Skill == team.MainSkill).Value)
                         .Include(x => x.PlayerSkills.Where(x => x.Skill == team.MainSkill))
                         .Take(Convert.ToInt32(team.NumberOfPlayers)).ToList();

                        if (addPlayer.Count < int.Parse(team.NumberOfPlayers))
                        {
                            // Check for incomplete players
                            addPlayer = addPlayer.Concat(Context.Players.Where(x => x.Position == team.Position).Include(x => x.PlayerSkills
                            .Where(x => x.Skill != team.MainSkill).OrderByDescending(x=>x.Value).Take(1)).
                                OrderByDescending(x => x.PlayerSkills.Where(x=>x.Skill != team.MainSkill).Max(x => x.Value)).
                                Take(int.Parse(team.NumberOfPlayers)- addPlayer.Count)).ToList();
                        }

                        if (addPlayer.Count == 0)
                        {
                            // if players have the position but no required skill, get the highest other skill 
                            addPlayer = Context.Players.Where(x => x.Position == team.Position).Include(x => x.PlayerSkills
                            .Where(x => x.Skill != team.MainSkill).OrderByDescending(x => x.Value).Take(1))
                                .OrderByDescending(x => x.PlayerSkills.Max(x=> x.Value)).
                                Take(Convert.ToInt32(team.NumberOfPlayers)).ToList();
                        }

                        foreach (var player in addPlayer)
                        {
                            result.Add(player);
                        }
                    }
                }
                if(result.Count ==0)
                {
                    return BadRequest(new ErrorResponse("Players not Found"));
                }
                    return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse("Error retrieving data from the database"));
            }

        }
    }
}
