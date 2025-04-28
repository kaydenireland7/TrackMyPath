using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;

using PathAPI.Controllers;
using PathAPI.Models;

namespace ApiUnitTests
{

    public class LocationsControllerTests
    {

        private readonly Mock<TrackMyPathContext> _mockContext;
        private readonly Mock<DbSet<Location>> _mockDbSet;

        public LocationsControllerTests()
        {
            _mockDbSet = new Mock<DbSet<Location>>();

            _mockContext = new Mock<TrackMyPathContext>();
            _mockContext.Setup(m => m.Locations).Returns(_mockDbSet.Object);
        }



        [Fact]
        public async Task GetLocation_ReturnsNotFound_WhenLocationNotFound()
        {
            // Arrange
            _mockDbSet.Setup(d => d.FindAsync(It.IsAny<int>())).ReturnsAsync((Location)null);

            var controller = new LocationsController(_mockContext.Object);

            // Act
            var result = await controller.GetLocation(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
        }



        [Fact]
        public async Task GetLocation_ReturnsLocation_WhenLocationExists()
        {
            // Arrange
            var location = new Location
            {
                Id = 1,
                TripId = 1,
                Timestamp = DateTime.UtcNow,
                Latitude = 0,
                Longitude = 0
            };
            _mockDbSet.Setup(d => d.FindAsync(It.IsAny<int>())).ReturnsAsync(location);

            var controller = new LocationsController(_mockContext.Object);

            // Act
            var result = await controller.GetLocation(1);

            // Assert
            var okResult = Assert.IsType<ActionResult<Location>>(result);
            var locationResult = Assert.IsType<Location>(okResult.Value);
            Assert.Equal(1, locationResult.Id);
        }

        [Fact]
        public async Task PostLocation_CreatesLocation()
        {
            // Arrange
            var location = new Location
            {
                Id = 1,
                TripId = 1,
                Timestamp = DateTime.UtcNow,
                Latitude = 0,
                Longitude = 0,
                Accuracy = 0,
                Speed = 0
            };

            _mockDbSet.Setup(d => d.AddAsync(It.IsAny<Location>(), It.IsAny<CancellationToken>()))
                      .Returns((Location t, CancellationToken _) => new ValueTask<EntityEntry<Location>>(EntityEntryMock(t)));

            var controller = new LocationsController(_mockContext.Object);

            // Act
            var result = await controller.PostLocation(location);

            // Assert
            var createdAtActionResult = Assert.IsType<ActionResult<Location>>(result).Result as CreatedAtActionResult;
            Assert.NotNull(createdAtActionResult);

            var createdLocation = Assert.IsType<Location>(createdAtActionResult.Value);
            Assert.Equal(0, createdLocation.Id);
        }

        [Fact]
        public async Task PutLocation_ReturnsBadRequest_WhenIdsDoNotMatch()
        {
            // Arrange
            var location = new Location
            {
                Id = 1,
                TripId = 1,
                Timestamp = DateTime.UtcNow,
                Latitude = 0,
                Longitude = 0
            };

            var controller = new LocationsController(_mockContext.Object);

            // Act
            var result = await controller.PutLocation(2, location);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeleteLocation_ReturnsNotFound_WhenLocationNotFound()
        {
            // Arrange
            _mockDbSet.Setup(d => d.FindAsync(It.IsAny<int>())).ReturnsAsync((Location)null);

            var controller = new LocationsController(_mockContext.Object);

            // Act
            var result = await controller.DeleteLocation(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteLocation_DeletesLocation()
        {
            // Arrange
            var location = new Location
            {
                Id = 1,
                TripId = 1,
                Timestamp = DateTime.UtcNow,
                Latitude = 0,
                Longitude = 0,
                Accuracy = 0,
                Speed = 0
            };

            _mockDbSet.Setup(d => d.FindAsync(It.IsAny<int>())).ReturnsAsync(location);

            var controller = new LocationsController(_mockContext.Object);

            // Act
            var result = await controller.DeleteLocation(1);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
        }

        private EntityEntry<Location> EntityEntryMock(Location location)
        {
            var mockEntry = new Mock<EntityEntry<Location>>();
            mockEntry.Setup(e => e.Entity).Returns(location);
            mockEntry.Setup(e => e.State).Returns(EntityState.Unchanged);
            return mockEntry.Object;
        }

    }
}
