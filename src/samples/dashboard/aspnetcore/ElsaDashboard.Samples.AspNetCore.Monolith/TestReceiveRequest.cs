using Elsa;
using Elsa.Activities.Http;
using Elsa.Activities.Http.Bookmarks;
using Elsa.Attributes;
using Elsa.Events;
using Elsa.Services.Bookmarks;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ElsaDashboard.Samples.AspNetCore.Monolith
{
    public class CustomizeTestReceiveRequest : INotificationHandler<DescribingActivityType>
    {
        // hiding some properties from UI
        public Task Handle(DescribingActivityType notification, CancellationToken cancellationToken)
        {
            var descriptor = notification.ActivityDescriptor;

            if (descriptor.Type != nameof(TestReceiveRequest))
                return Task.CompletedTask;

            var hiddenProperties = descriptor.InputProperties.Where(x => x.Name == "ReadContent" || x.Name == "TargetType" || x.Name == "Methods");

            foreach (var hiddenProperty in hiddenProperties)
                hiddenProperty.IsBrowsable = false;

            return Task.CompletedTask;
        }
    }

    [Trigger(
        Category = "Test",
        DisplayName = "Receive Request",
        Description = "Wait for an test request.",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class TestReceiveRequest : HttpEndpoint
    {
        // here to specify default value
        [ActivityInput(DefaultValue = true)]
        public new bool ReadContent { get; set; } = true;

        // here to specify default value
        [ActivityInput(DefaultValue = typeof(Dictionary<string, string>))]
        public new Type? TargetType { get; set; } = typeof(Dictionary<string, string>);
    }

    public class TestReceiveRequestBookmarkkProvider : BookmarkProvider<HttpEndpointBookmark, TestReceiveRequest>
    {
        public override async ValueTask<IEnumerable<BookmarkResult>> GetBookmarksAsync(BookmarkProviderContext<TestReceiveRequest> context, CancellationToken cancellationToken)
        {
            var path = ToLower(await context.ReadActivityPropertyAsync<HttpEndpoint, PathString>(x => x.Path, cancellationToken))!;
            var methods = new[] { "post" };

            BookmarkResult CreateBookmark(string method) => Result(new(path, method), nameof(HttpEndpoint));
            return methods.Select(CreateBookmark);
        }

        private static string ToLower(string? s) => s?.ToLowerInvariant();
    }
}
