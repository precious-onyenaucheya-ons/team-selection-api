// /////////////////////////////////////////////////////////////////////////////
// YOU CAN FREELY MODIFY THE CODE BELOW IN ORDER TO COMPLETE THE TASK
// /////////////////////////////////////////////////////////////////////////////

namespace WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApi.Helpers;
using WebApi.Entities;
using WebApi.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class PlayerController : ControllerBase
{
    private readonly DataContext Context;
    private readonly IMapper _mapper;
    public PlayerController(DataContext context, IMapper mapper)
    {
        Context = context;
        _mapper = mapper;   
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Player>>> GetAll()
    {
        try
        {
            var response = await Task.Run(() => Context.Players);
            foreach (var player in response)
            {
                player.PlayerSkills = Context.PlayerSkills.Where(x => x.PlayerId == player.Id).ToList();
            }
            return Ok(response);            
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Error retrieving data from the database");
        }       
    }
    

    [HttpPost]
    public async Task<ActionResult<Player>> PostPlayer([FromBody] PlayerDTO player)
    {     try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse("Validation Error"));
            }
            if (player == null)
                return BadRequest();
            var player1 = _mapper.Map<Player>(player);
            await Context.Players.AddAsync(player1);
            await Context.SaveChangesAsync();
            return Ok(player1);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ErrorResponse("Error creating new employee record"));
        }
    }

    [HttpPut("{playerId}")]
    public async Task<IActionResult> PutPlayer(int playerId, PlayerDTO player)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse("Validation Error"));

            var checkplayer = Context.Players.FindAsync(playerId);

            if (await checkplayer == null)
                return NotFound(new ErrorResponse($"Employee with Id = {playerId} not found"));

            //find record based on playerId and update
            var updateplayer = await Context.Players.Include(z => z.PlayerSkills).Where(y => y.Id == playerId).FirstOrDefaultAsync();
            updateplayer.Name = player.Name;
            updateplayer.Position = player.Position;
            Context.SaveChanges();
            for(int i =0; i< player.PlayerSkills.Count; i++)
            {
                //update playerSkill based on skillId
                updateplayer.PlayerSkills[i].Skill = player.PlayerSkills[i].Skill;
                updateplayer.PlayerSkills[i].Value = player.PlayerSkills[i].Value;
            }
            Context.Attach(updateplayer);
            Context.Entry(updateplayer).State = EntityState.Modified;
            Context.SaveChanges();
            return Ok(updateplayer);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ErrorResponse("Error updating data"));
        }
    }

    [Authorize]
    [HttpDelete("{playerId}")]
    public async Task<ActionResult<Player>> DeletePlayer(int playerId)
    {
        try
        {
            var playerToDelete = Context.Players.FindAsync(playerId);

            if (await playerToDelete == null)
            {
                return NotFound(new ErrorResponse($"Employee with Id = {playerId} not found"));
            }
            //remove  record in players table
            Context.Players.Remove(await Context.Players.FindAsync(playerId));
            Context.SaveChanges();
            //remove records in skills table
            var skills = Context.PlayerSkills.Where(s => s.PlayerId == playerId);
            Context.PlayerSkills.RemoveRange(skills);
            Context.SaveChanges();
            return Ok();
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ErrorResponse("Error deleting data"));
        }
    }
}