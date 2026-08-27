using AutoMapper;
using Core.Domain.Entities;
using Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Mappers
{
    public class CoreMappingProfile : Profile
    {
        public class ProjectMappingProfile : Profile
        {
            public ProjectMappingProfile()
            {
                CreateMap<Project, ProjectResponseDto>();
            }
        }

        public class SprintMappingProfile : Profile
        {
            public SprintMappingProfile()
            {
                CreateMap<Sprint, SprintResponseDto>();   
            }
        }

        public class TaskItemMappingProfile : Profile
        {
            public TaskItemMappingProfile()
            {
                CreateMap<TaskItem, TaskItemResponseDto>()
                    .ForMember(dest => dest.AssigneeName, opt => opt.MapFrom(src => src.Assignee.Name));
            }
        }
    }
}
