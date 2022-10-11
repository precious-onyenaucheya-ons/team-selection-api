using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs
{
    public class PlayerDTO
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Position { get; set; }
        public List<PlayerSkillDTO> PlayerSkills { get; set; }
    }

    public class PlayerSkillDTO
    {
        public string Skill { get; set; }
        public int Value { get; set; }
    }
}
