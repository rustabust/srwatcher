using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sr_watcher.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthorTotals",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Author = table.Column<string>(type: "TEXT", nullable: true),
                    TotalUps = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorTotals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RedditApiBookmarks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    ApiBaseUrl = table.Column<string>(type: "TEXT", nullable: true),
                    BookmarkValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedditApiBookmarks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RedditApiPosts",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", nullable: false),
                    subreddit = table.Column<string>(type: "TEXT", nullable: true),
                    selftext = table.Column<string>(type: "TEXT", nullable: true),
                    author_fullname = table.Column<string>(type: "TEXT", nullable: true),
                    author = table.Column<string>(type: "TEXT", nullable: true),
                    title = table.Column<string>(type: "TEXT", nullable: true),
                    name = table.Column<string>(type: "TEXT", nullable: true),
                    ups = table.Column<int>(type: "INTEGER", nullable: false),
                    selftext_html = table.Column<string>(type: "TEXT", nullable: true),
                    subreddit_id = table.Column<string>(type: "TEXT", nullable: true),
                    created_utc = table.Column<double>(type: "REAL", nullable: false),
                    url = table.Column<string>(type: "TEXT", nullable: true),
                    permalink = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedditApiPosts", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorTotals");

            migrationBuilder.DropTable(
                name: "RedditApiBookmarks");

            migrationBuilder.DropTable(
                name: "RedditApiPosts");
        }
    }
}
