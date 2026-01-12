using FluentAssertions;
using NUnit.Framework;
using OnkelMato.BlogEngine.Core.Database.Entity;
using OnkelMato.BlogEngine.Core.Model;
using OnkelMato.BlogEngine.Core.Repository;

namespace OnkelMato.BlogEngine.Core.Tests.Repository;

[TestFixture]
public class DbToModelExtensionTests
{
    #region ShowState Tests

    [TestCase(ShowStateDb.None, ShowState.None)]
    [TestCase(ShowStateDb.Blog, ShowState.Blog)]
    [TestCase(ShowStateDb.Menu, ShowState.Menu)]
    [TestCase(ShowStateDb.Footer, ShowState.Footer)]
    [TestCase(ShowStateDb.BlogAndMenu, ShowState.BlogAndMenu)]
    [TestCase(ShowStateDb.BlogAndFooter, ShowState.BlogAndFooter)]
    [TestCase(ShowStateDb.LinkAndMenu, ShowState.LinkAndMenu)]
    [TestCase(ShowStateDb.LinkAndFooter, ShowState.LinkAndFooter)]
    public void ToModel_ShowStateDb_ConvertsToCorrectShowState(ShowStateDb input, ShowState expected)
    {
        // Act
        var result = input.ToModel();

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void ToModel_ShowStateDb_WithInvalidValue_ReturnsNone()
    {
        // Arrange
        var invalidShowState = (ShowStateDb)999;

        // Act
        var result = invalidShowState.ToModel();

        // Assert
        result.Should().Be(ShowState.None);
    }

    [TestCase(ShowState.None, ShowStateDb.None)]
    [TestCase(ShowState.Blog, ShowStateDb.Blog)]
    [TestCase(ShowState.Menu, ShowStateDb.Menu)]
    [TestCase(ShowState.Footer, ShowStateDb.Footer)]
    [TestCase(ShowState.BlogAndMenu, ShowStateDb.BlogAndMenu)]
    [TestCase(ShowState.BlogAndFooter, ShowStateDb.BlogAndFooter)]
    [TestCase(ShowState.LinkAndMenu, ShowStateDb.LinkAndMenu)]
    [TestCase(ShowState.LinkAndFooter, ShowStateDb.LinkAndFooter)]
    public void FromModel_ShowState_ConvertsToCorrectShowStateDb(ShowState input, ShowStateDb expected)
    {
        // Act
        var result = input.FromModel();

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void FromModel_ShowState_WithInvalidValue_ReturnsNone()
    {
        // Arrange
        var invalidShowState = (ShowState)999;

        // Act
        var result = invalidShowState.FromModel();

        // Assert
        result.Should().Be(ShowStateDb.None);
    }

    #endregion

    #region PostImage Tests

    [Test]
    public void ToModel_PostImageDb_WithValidData_ReturnsPostImageWithCorrectProperties()
    {
        // Arrange
        var uniqueId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var updatedAt = DateTime.UtcNow;
        var imageData = new byte[] { 1, 2, 3, 4, 5 };

        var postImageDb = new PostImageDb
        {
            UniqueId = uniqueId,
            AltText = "Test Alt Text",
            ContentType = "image/jpeg",
            Name = "test-image.jpg",
            Image = imageData,
            IsPublished = true,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Act
        var result = postImageDb.ToModel();

        // Assert
        result.Should().NotBeNull();
        result.UniqueId.Should().Be(uniqueId);
        result.AltText.Should().Be("Test Alt Text");
        result.ContentType.Should().Be("image/jpeg");
        result.Name.Should().Be("test-image.jpg");
        result.Image.Should().BeEquivalentTo(imageData);
        result.IsPublished.Should().BeTrue();
        result.CreatedAt.Should().Be(createdAt);
        result.UpdatedAt.Should().Be(updatedAt);
    }

    [Test]
    public void ToModel_PostImageDb_WithNullAltText_HandlesCorrectly()
    {
        // Arrange
        var postImageDb = new PostImageDb
        {
            UniqueId = Guid.NewGuid(),
            AltText = null,
            ContentType = "image/png",
            Name = "test.png",
            Image = new byte[] { 1, 2, 3 },
            IsPublished = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var result = postImageDb.ToModel();

        // Assert
        result.Should().NotBeNull();
        result.AltText.Should().BeNull();
    }

    #endregion

    #region Post Tests

    [Test]
    public void ToModel_PostDb_WithValidData_ReturnsPostWithCorrectProperties()
    {
        // Arrange
        var uniqueId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddDays(-2);
        var updatedAt = DateTime.UtcNow.AddDays(-1);
        var publishedAt = DateTime.UtcNow;

        var postDb = new PostDb
        {
            UniqueId = uniqueId,
            Title = "Test Post Title",
            MdPreview = "This is a preview",
            MdContent = "# This is the full content",
            ShowState = ShowStateDb.Blog,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            PublishedAt = publishedAt,
            Order = 5,
            HeaderImage = null,
            PostTags = null
        };

        // Act
        var result = postDb.ToModel();

        // Assert
        result.Should().NotBeNull();
        result.UniqueId.Should().Be(uniqueId);
        result.Title.Should().Be("Test Post Title");
        result.MdPreview.Should().Be("This is a preview");
        result.MdContent.Should().Be("# This is the full content");
        result.ShowState.Should().Be(ShowState.Blog);
        result.CreatedAt.Should().Be(createdAt);
        result.UpdatedAt.Should().Be(updatedAt);
        result.PublishedAt.Should().Be(publishedAt);
        result.Order.Should().Be(5);
        result.HeaderImage.Should().BeNull();
        result.PostTags.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public void ToModel_PostDb_WithHeaderImage_IncludesHeaderImageInResult()
    {
        // Arrange
        var headerImageDb = new PostImageDb
        {
            UniqueId = Guid.NewGuid(),
            AltText = "Header Image",
            ContentType = "image/jpeg",
            Name = "header.jpg",
            Image = new byte[] { 1, 2, 3 },
            IsPublished = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var postDb = new PostDb
        {
            UniqueId = Guid.NewGuid(),
            Title = "Test Post",
            MdPreview = "Preview",
            MdContent = "Content",
            ShowState = ShowStateDb.BlogAndMenu,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            PublishedAt = null,
            Order = 1,
            HeaderImage = headerImageDb,
            PostTags = null
        };

        // Act
        var result = postDb.ToModel();

        // Assert
        result.Should().NotBeNull();
        result.HeaderImage.Should().NotBeNull();
        result.HeaderImage!.UniqueId.Should().Be(headerImageDb.UniqueId);
        result.HeaderImage.Name.Should().Be("header.jpg");
    }

    [Test]
    public void ToModel_PostDb_WithPostTags_IncludesPostTagTitlesInResult()
    {
        // Arrange
        var postTags = new List<PostTagDb>
        {
            new PostTagDb { Title = "C#" },
            new PostTagDb { Title = "ASP.NET Core" },
            new PostTagDb { Title = "Razor Pages" }
        };

        var postDb = new PostDb
        {
            UniqueId = Guid.NewGuid(),
            Title = "Test Post",
            MdPreview = "Preview",
            MdContent = "Content",
            ShowState = ShowStateDb.Blog,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            PublishedAt = DateTime.UtcNow,
            Order = 1,
            HeaderImage = null,
            PostTags = postTags
        };

        // Act
        var result = postDb.ToModel();

        // Assert
        result.Should().NotBeNull();
        result.PostTags.Should().NotBeNull();
        result.PostTags.Should().HaveCount(3);
        result.PostTags.Should().BeEquivalentTo(new[] { "C#", "ASP.NET Core", "Razor Pages" });
    }

    [Test]
    public void ToModel_PostDb_WithNullPostTags_ReturnsEmptyList()
    {
        // Arrange
        var postDb = new PostDb
        {
            UniqueId = Guid.NewGuid(),
            Title = "Test Post",
            MdPreview = "Preview",
            MdContent = "Content",
            ShowState = ShowStateDb.Blog,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            PublishedAt = DateTime.UtcNow,
            Order = 1,
            HeaderImage = null,
            PostTags = null
        };

        // Act
        var result = postDb.ToModel();

        // Assert
        result.PostTags.Should().NotBeNull().And.BeEmpty();
    }

    #endregion

    #region Blog Tests

    [Test]
    public void ToModel_BlogDb_WithValidData_ReturnsBlogWithCorrectProperties()
    {
        // Arrange
        var uniqueId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddMonths(-1);
        var updatedAt = DateTime.UtcNow;

        var blogDb = new BlogDb
        {
            UniqueId = uniqueId,
            Title = "My Blog",
            Description = "A blog about programming",
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            CSS = "body { color: red; }",
            Posts = null,
            PostImages = null
        };

        // Act
        var result = blogDb.ToModel();

        // Assert
        result.Should().NotBeNull();
        result.UniqueId.Should().Be(uniqueId);
        result.Title.Should().Be("My Blog");
        result.Description.Should().Be("A blog about programming");
        result.CreatedAt.Should().Be(createdAt);
        result.UpdatedAt.Should().Be(updatedAt);
        result.CSS.Should().Be("body { color: red; }");
        result.Posts.Should().NotBeNull().And.BeEmpty();
        result.PostImages.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public void ToModel_BlogDb_WithPosts_IncludesPostsInResult()
    {
        // Arrange
        var posts = new List<PostDb>
        {
            new PostDb
            {
                UniqueId = Guid.NewGuid(),
                Title = "Post 1",
                MdPreview = "Preview 1",
                MdContent = "Content 1",
                ShowState = ShowStateDb.Blog,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PublishedAt = DateTime.UtcNow,
                Order = 1
            },
            new PostDb
            {
                UniqueId = Guid.NewGuid(),
                Title = "Post 2",
                MdPreview = "Preview 2",
                MdContent = "Content 2",
                ShowState = ShowStateDb.BlogAndMenu,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PublishedAt = DateTime.UtcNow,
                Order = 2
            }
        };

        var blogDb = new BlogDb
        {
            UniqueId = Guid.NewGuid(),
            Title = "My Blog",
            Description = "Description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CSS = null,
            Posts = posts,
            PostImages = null
        };

        // Act
        var result = blogDb.ToModel();

        // Assert
        result.Should().NotBeNull();
        result.Posts.Should().NotBeNull();
        result.Posts.Should().HaveCount(2);
        result.Posts[0].Title.Should().Be("Post 1");
        result.Posts[1].Title.Should().Be("Post 2");
    }

    [Test]
    public void ToModel_BlogDb_WithPostImages_IncludesPostImagesInResult()
    {
        // Arrange
        var postImages = new List<PostImageDb>
        {
            new PostImageDb
            {
                UniqueId = Guid.NewGuid(),
                Name = "image1.jpg",
                ContentType = "image/jpeg",
                Image = new byte[] { 1, 2, 3 },
                IsPublished = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new PostImageDb
            {
                UniqueId = Guid.NewGuid(),
                Name = "image2.png",
                ContentType = "image/png",
                Image = new byte[] { 4, 5, 6 },
                IsPublished = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        var blogDb = new BlogDb
        {
            UniqueId = Guid.NewGuid(),
            Title = "My Blog",
            Description = "Description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CSS = null,
            Posts = null,
            PostImages = postImages
        };

        // Act
        var result = blogDb.ToModel();

        // Assert
        result.Should().NotBeNull();
        result.PostImages.Should().NotBeNull();
        result.PostImages.Should().HaveCount(2);
        result.PostImages[0].Name.Should().Be("image1.jpg");
        result.PostImages[1].Name.Should().Be("image2.png");
    }

    [Test]
    public void ToModel_BlogDb_WithNullCollections_ReturnsEmptyCollections()
    {
        // Arrange
        var blogDb = new BlogDb
        {
            UniqueId = Guid.NewGuid(),
            Title = "My Blog",
            Description = "Description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CSS = null,
            Posts = null,
            PostImages = null
        };

        // Act
        var result = blogDb.ToModel();

        // Assert
        result.Should().NotBeNull();
        result.Posts.Should().NotBeNull().And.BeEmpty();
        result.PostImages.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public void ToModel_BlogDb_WithCompleteData_MapsAllPropertiesCorrectly()
    {
        // Arrange
        var headerImage = new PostImageDb
        {
            UniqueId = Guid.NewGuid(),
            Name = "header.jpg",
            ContentType = "image/jpeg",
            Image = new byte[] { 1, 2, 3 },
            IsPublished = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var postTags = new List<PostTagDb>
        {
            new PostTagDb { Title = "Tag1" },
            new PostTagDb { Title = "Tag2" }
        };

        var post = new PostDb
        {
            UniqueId = Guid.NewGuid(),
            Title = "Full Post",
            MdPreview = "Preview",
            MdContent = "Content",
            ShowState = ShowStateDb.BlogAndMenu,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            PublishedAt = DateTime.UtcNow,
            Order = 1,
            HeaderImage = headerImage,
            PostTags = postTags
        };

        var postImage = new PostImageDb
        {
            UniqueId = Guid.NewGuid(),
            Name = "post-image.jpg",
            ContentType = "image/jpeg",
            Image = new byte[] { 7, 8, 9 },
            IsPublished = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var blogDb = new BlogDb
        {
            UniqueId = Guid.NewGuid(),
            Title = "Complete Blog",
            Description = "Full description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CSS = "body { margin: 0; }",
            Posts = new List<PostDb> { post },
            PostImages = new List<PostImageDb> { postImage }
        };

        // Act
        var result = blogDb.ToModel();

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Complete Blog");
        result.Posts.Should().HaveCount(1);
        result.Posts[0].HeaderImage.Should().NotBeNull();
        result.Posts[0].PostTags.Should().HaveCount(2);
        result.PostImages.Should().HaveCount(1);
    }

    #endregion
}