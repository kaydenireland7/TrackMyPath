using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;

using PathAPI.Controllers;
using PathAPI.Models;

namespace ApiUnitTests
{
    public class TripsControllerTests
    {
        private readonly Mock<TrackMyPathContext> _mockContext;
        private readonly Mock<DbSet<Trip>> _mockDbSet;

        public TripsControllerTests()
        {
            _mockDbSet = new Mock<DbSet<Trip>>();

            _mockContext = new Mock<TrackMyPathContext>();
            _mockContext.Setup(m => m.Trips).Returns(_mockDbSet.Object);
        }

        /*
        [Fact]
        public async Task GetTrip_ReturnsNotFound_WhenTripNotFound()
        {
            // Arrange
            _mockDbSet.Setup(d => d.FindAsync(It.IsAny<int>())).ReturnsAsync((Trip)null);

            var controller = new TripsController(_mockContext.Object);

            // Act
            var result = await controller.GetTrip(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetTrip_ReturnsTrip_WhenTripExists()
        {
            // Arrange
            var trip = new Trip
            {
                Id = 0,
                UserId = 1,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow,
                TripName = "Sample Trip"
            };
            _mockDbSet.Setup(d => d.FindAsync(It.IsAny<int>())).ReturnsAsync(trip);

            var controller = new TripsController(_mockContext.Object);

            // Act
            var result = await controller.GetTrip(1);

            // Assert
            var okResult = Assert.IsType<ActionResult<Trip>>(result);
            var tripResult = Assert.IsType<Trip>(okResult.Value);
            Assert.Equal(1, tripResult.Id);
        }*/

        [Fact]
        public async Task PostTrip_CreatesTrip()
        {
            // Arrange
            var trip = new Trip
            {
                Id = 1,
                UserId = 1,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow,
                TripName = "Sample Trip"
            };

            _mockDbSet.Setup(d => d.AddAsync(It.IsAny<Trip>(), It.IsAny<CancellationToken>()))
                      .Returns((Trip t, CancellationToken _) => new ValueTask<EntityEntry<Trip>>(EntityEntryMock(t)));

            var controller = new TripsController(_mockContext.Object);

            // Act
            var result = await controller.CreateTrip(trip);

            // Assert
            var createdAtActionResult = Assert.IsType<ActionResult<Trip>>(result).Result as CreatedAtActionResult;
            Assert.NotNull(createdAtActionResult);

            var createdTrip = Assert.IsType<Trip>(createdAtActionResult.Value);
            Assert.Equal(0, createdTrip.Id);
        }

        [Fact]
        public async Task PutTrip_ReturnsBadRequest_WhenIdsDoNotMatch()
        {
            // Arrange
            var trip = new Trip
            {
                Id = 0,
                UserId = 1,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow,
                TripName = "Sample Trip"
            };

            var controller = new TripsController(_mockContext.Object);

            // Act
            var result = await controller.UpdateTrip(2, trip);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DeleteTrip_ReturnsNotFound_WhenTripNotFound()
        {
            // Arrange
            _mockDbSet.Setup(d => d.FindAsync(It.IsAny<int>())).ReturnsAsync((Trip)null);

            var controller = new TripsController(_mockContext.Object);

            // Act
            var result = await controller.DeleteTrip(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteTrip_DeletesTrip()
        {
            // Arrange
            var trip = new Trip
            {
                Id = 0,
                UserId = 1,
                TripName = "Sample Trip",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow
            };

            _mockDbSet.Setup(d => d.FindAsync(It.IsAny<int>())).ReturnsAsync(trip);

            var controller = new TripsController(_mockContext.Object);

            // Act
            var result = await controller.DeleteTrip(1);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
        }

        private EntityEntry<Trip> EntityEntryMock(Trip trip)
        {
            var mockEntry = new Mock<EntityEntry<Trip>>();
            mockEntry.Setup(e => e.Entity).Returns(trip);
            mockEntry.Setup(e => e.State).Returns(EntityState.Unchanged);
            return mockEntry.Object;
        }
    }
}
