using Threads.Api.IntegrationTests.Blocks.TestSupport;
using Threads.Api.IntegrationTests.Comments.TestSupport;
using Threads.Api.IntegrationTests.Follows.TestSupport;
using Threads.Api.IntegrationTests.Likes.TestSupport;
using Threads.Api.IntegrationTests.Posts.TestSupport;
using Threads.Api.IntegrationTests.Users.TestSupport;

namespace Threads.Api.IntegrationTests.Infrastructure;

public sealed class TestScenarios
{
    public BlockRelationshipScenarioSeeder BlockRelationships { get; }
    public CommentListScenarioSeeder CommentList { get; }
    public FollowRelationshipScenarioSeeder FollowRelationships { get; }
    public LikeListScenarioSeeder LikeList { get; }
    public PostVisibilityScenarioSeeder PostVisibility { get; }
    public PostPaginationScenarioSeeder PostPagination { get; }
    public PostEngagementVisibilityScenarioSeeder PostEngagementVisibility { get; }
    public UserDeletionScenarioSeeder UserDeletion { get; }

    public TestScenarios(TestSeeders seeders)
    {
        BlockRelationships = new BlockRelationshipScenarioSeeder(
            seeders.Users,
            seeders.Blocks,
            seeders.Follows
        );
        CommentList = new CommentListScenarioSeeder(
            seeders.Users,
            seeders.Posts,
            seeders.Comments,
            seeders.Blocks
        );
        FollowRelationships = new FollowRelationshipScenarioSeeder(
            seeders.Users,
            seeders.Follows,
            seeders.Blocks
        );
        LikeList = new LikeListScenarioSeeder(
            seeders.Users,
            seeders.Posts,
            seeders.Likes,
            seeders.Blocks
        );
        PostVisibility = new PostVisibilityScenarioSeeder(
            seeders.Users,
            seeders.Posts,
            seeders.Follows
        );
        PostPagination = new PostPaginationScenarioSeeder(seeders.Users, seeders.Posts);
        PostEngagementVisibility = new PostEngagementVisibilityScenarioSeeder(
            PostVisibility,
            seeders.Blocks,
            seeders.Likes,
            seeders.Comments
        );
        UserDeletion = new UserDeletionScenarioSeeder(
            seeders.Users,
            seeders.Posts,
            seeders.Follows,
            seeders.Blocks,
            seeders.Likes,
            seeders.Comments,
            seeders.Notifications,
            seeders.RefreshTokens
        );
    }
}
