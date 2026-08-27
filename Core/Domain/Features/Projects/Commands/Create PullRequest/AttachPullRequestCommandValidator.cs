using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Commands.Create_PullRequest
{
    public class AttachPullRequestCommandValidator : AbstractValidator<AttachPullRequestCommand>
    {
        public AttachPullRequestCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.PullRequestUrl)
                .NotEmpty().WithMessage("رابط الـ Pull Request مطلوب.")
                .Must(BeAValidUrl).WithMessage("رابط غير صالح.");
        }

        private bool BeAValidUrl(string url) =>
            Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}
