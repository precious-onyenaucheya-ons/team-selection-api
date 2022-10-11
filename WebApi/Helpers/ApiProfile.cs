using AutoMapper;
using WebApi.DTOs;
using WebApi.Entities;

namespace WebApi.Helpers
{
    public class ApiProfile:Profile
    {
        public ApiProfile()
        {
            CreateMap<PlayerSkillDTO, PlayerSkill>();
            CreateMap<PlayerDTO, Player>()
                .ForMember(dest => dest.PlayerSkills,opt => opt.MapFrom(src => src.PlayerSkills))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src=>src.Position.ToLower()));
        }
    }
}