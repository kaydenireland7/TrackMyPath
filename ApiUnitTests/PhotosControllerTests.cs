using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;

using PathAPI.Controllers;
using PathAPI.Models;

namespace ApiUnitTests
{
    public class PhotosControllerTests
    {
        private readonly Mock<TrackMyPathContext> _mockContext;
        private readonly Mock<DbSet<Photo>> _mockDbSet;

        public PhotosControllerTests()
        {
            _mockDbSet = new Mock<DbSet<Photo>>();

            _mockContext = new Mock<TrackMyPathContext>();
            _mockContext.Setup(m => m.Photos).Returns(_mockDbSet.Object);
        }

        [Fact]
        public async Task GetPhoto_ReturnsNotFound_WhenPhotoNotFound()
        {
            // Arrange
            _mockDbSet.Setup(d => d.FindAsync(It.IsAny<int>())).ReturnsAsync((Photo)null);

            var controller = new PhotosController(_mockContext.Object);

            // Act
            var result = await controller.GetPhoto(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetPhoto_ReturnsPhoto_WhenPhotoExists()
        {
            // Arrange
            var photo = new Photo
            {
                Id = 0,
                LocationId = 1,
                FileUrl = "http://example.com/photo.jpg",
                Caption = "Test Photo"
            };
            _mockDbSet.Setup(d => d.FindAsync(It.IsAny<int>())).ReturnsAsync(photo);

            var controller = new PhotosController(_mockContext.Object);

            // Act
            var result = await controller.GetPhoto(1);

            // Assert
            var okResult = Assert.IsType<ActionResult<Photo>>(result);
            var photoResult = Assert.IsType<Photo>(okResult.Value);
            Assert.Equal(0, photoResult.Id);
        }

        [Fact]
        public async Task PostPhoto_CreatesPhoto()
        {
            // Arrange
            var photo = new Photo
            {
                Id = 0,
                LocationId = 1,
                FileUrl = "http://example.com/photo.jpg",
                Caption = "Test Photo"
            };

            _mockDbSet.Setup(d => d.AddAsync(It.IsAny<Photo>(), It.IsAny<CancellationToken>()))
                      .Returns((Photo p, CancellationToken _) => new ValueTask<EntityEntry<Photo>>(EntityEntryMock(p)));

            var controller = new PhotosController(_mockContext.Object);

            // Act
            var result = await controller.CreatePhoto(photo);

            // Assert
            var createdAtActionResult = Assert.IsType<ActionResult<Photo>>(result).Result as CreatedAtActionResult;
            Assert.NotNull(createdAtActionResult);

            var createdPhoto = Assert.IsType<Photo>(createdAtActionResult.Value);
            Assert.Equal(0, createdPhoto.Id);
        }

        [Fact]
        public async Task PutPhoto_ReturnsBadRequest_WhenIdsDoNotMatch()
        {
            // Arrange
            var photo = new Photo
            {
                Id = 0,
                LocationId = 1,
                FileUrl = "http://example.com/photo.jpg",
                Caption = "Test Photo"
            };

            var controller = new PhotosController(_mockContext.Object);

            // Act
            var result = await controller.UpdatePhoto(2, photo);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DeletePhoto_ReturnsNotFound_WhenPhotoNotFound()
        {
            // Arrange
            _mockDbSet.Setup(d => d.FindAsync(It.IsAny<int>())).ReturnsAsync((Photo)null);

            var controller = new PhotosController(_mockContext.Object);

            // Act
            var result = await controller.DeletePhoto(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeletePhoto_DeletesPhoto()
        {
            // Arrange
            var photo = new Photo
            {
                Id = 0,
                LocationId = 1,

                FileUrl = "http://example.com/photo.jpg",
                Caption = "Test Photo"
            };

            _mockDbSet.Setup(d => d.FindAsync(It.IsAny<int>())).ReturnsAsync(photo);

            var controller = new PhotosController(_mockContext.Object);

            // Act
            var result = await controller.DeletePhoto(1);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
        }

        private EntityEntry<Photo> EntityEntryMock(Photo photo)
        {
            var mockEntry = new Mock<EntityEntry<Photo>>();
            mockEntry.Setup(e => e.Entity).Returns(photo);
            mockEntry.Setup(e => e.State).Returns(EntityState.Unchanged);
            return mockEntry.Object;
        }
    }
}
