﻿using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using JubaUniversity.Data;
using JubaUniversity.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JubaUniversity.Pages.Courses
{
    public class Edit : PageModel
    {
        private readonly IMediator _mediator;

        [BindProperty]
        public Command Data { get; set; }

        public Edit(IMediator mediator) => _mediator = mediator;

        public async Task OnGetAsync(Query query) => Data = await _mediator.Send(query);

        public async Task<IActionResult> OnPostAsync()
        {
            await _mediator.Send(Data);

            return this.RedirectToPageJson(nameof(Index));
        }

        public class Query : IRequest<Command>
        {
            public int? Id { get; set; }
        }

        public class QueryValidator : AbstractValidator<Query>
        {
            public QueryValidator()
            {
                RuleFor(m => m.Id).NotNull();
            }
        }

        public class QueryHandler : IRequestHandler<Query, Command>
        {
            private readonly SchoolContext _db;
            private readonly IConfigurationProvider _configuration;

            public QueryHandler(SchoolContext db, IConfigurationProvider configuration)
            {
                _db = db;
                _configuration = configuration;
            }

            public Task<Command> Handle(Query message, CancellationToken token) =>
                _db.Courses
                    .Where(c => c.Id == message.Id)
                    .ProjectTo<Command>(_configuration)
                    .SingleOrDefaultAsync(token);
        }

        public class Command : IRequest
        {
            [Display(Name = "Number")]
            public int Id { get; set; }
            public string Title { get; set; }
            public int? Credits { get; set; }
            public Department Department { get; set; }
        }

        public class MappingProfile : Profile
        {
            public MappingProfile() => CreateMap<Course, Command>().ReverseMap();
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(m => m.Title).NotNull().Length(3, 50);
                RuleFor(m => m.Credits).NotNull().InclusiveBetween(0, 5);
            }
        }

        public class CommandHandler : IRequestHandler<Command, Unit>
        {
            private readonly SchoolContext _db;
            private readonly IMapper _mapper;

            public CommandHandler(SchoolContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                var course = await _db.Courses.FindAsync(request.Id);

                _mapper.Map(request, course);

                return default;
            }
        }
    }
}