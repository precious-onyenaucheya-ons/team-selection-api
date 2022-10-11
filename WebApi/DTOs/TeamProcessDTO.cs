using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs
{
    public class TeamProcessDTO
    {
        [Required]
        public string Position { get; set; }
        [Required]
        public string MainSkill { get; set; }
        [Required]
        public string NumberOfPlayers { get; set; }
    }
}
