using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;

using PathAPI.Controllers;
using PathAPI.Models;

namespace ApiUnitTests
{
    public class UsersControllerTests
    {
        private readonly Mock<TrackMyPathContext> _mockContext;
        private readonly Mock<DbSet<User>> _mockDbSet;

        public UsersControllerTests()
        {
            _mockDbSet = new Mock<DbSet<User>>();

            _mockContext = new Mock<TrackMyPathContext>();
            _mockContext.Setup(m => m.Users).Returns(_mockDbSet.Object);
        }

        [Fact]
        public async Task GetUser_ReturnsNotFound_WhenUserNotFound()
        {
            // Arrange
            _mockDbSet.Setup(d => d.FindAsync(It.IsAny<int>())).ReturnsAsync((User)null);

            var controller = new UsersController(_mockContext.Object);

            // Act
            var result = await controller.GetUser(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetUser_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var user = new User
            {
                Id = 0,
                Email = "test@example.com",
                PasswordHash = "hashedpassword"
            };
            _mockDbSet.Setup(d => d.FindAsync(It.IsAny<int>())).ReturnsAsync(user);

            var controller = new UsersController(_mockContext.Object);

            // Act
            var result = await controller.GetUser(1);

            // Assert
            var okResult = Assert.IsType<ActionResult<User>>(result);
            var userResult = Assert.IsType<User>(okResult.Value);
            Assert.Equal(0, userResult.Id);
        }

        [Fact]
        public async Task PostUser_CreatesUser()
        {
            // Arrange
            var user = new User
            {
                Id = 0,
                Email = "test@example.com",
                PasswordHash = "hashedpassword"
            };

            _mockDbSet.Setup(d => d.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                      .Returns((User t, CancellationToken _) => new ValueTask<EntityEntry<User>>(EntityEntryMock(t)));

            var controller = new UsersController(_mockContext.Object);

            // Act
            var result = await controller.CreateUser(user);

            // Assert
            var createdAtActionResult = Assert.IsType<ActionResult<User>>(result).Result as CreatedAtActionResult;
            Assert.NotNull(createdAtActionResult);

            var createdUser = Assert.IsType<User>(createdAtActionResult.Value);
            Assert.Equal(0, createdUser.Id);
        }

        [Fact]
        public async Task PutUser_ReturnsBadRequest_WhenIdsDoNotMatch()
        {
            // Arrange
            var user = new User
            {
                Id = 0,
                Email = "test@example.com",
                PasswordHash = "hashedpassword"
            };

            var controller = new UsersController(_mockContext.Object);

            // Act
            var result = await controller.UpdateUser(2, user);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DeleteUser_ReturnsNotFound_WhenUserNotFound()
        {
            // Arrange
            _mockDbSet.Setup(d => d.FindAsync(It.IsAny<int>())).ReturnsAsync((User)null);

            var controller = new UsersController(_mockContext.Object);

            // Act
            var result = await controller.DeleteUser(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteUser_DeletesUser()
        {
            // Arrange
            var user = new User
            {
                Id = 0,
                Email = "test@example.com",
                PasswordHash = "hashedpassword"
            };

            _mockDbSet.Setup(d => d.FindAsync(It.IsAny<int>())).ReturnsAsync(user);

            var controller = new UsersController(_mockContext.Object);

            // Act
            var result = await controller.DeleteUser(1);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
        }

        private EntityEntry<User> EntityEntryMock(User user)
        {
            var mockEntry = new Mock<EntityEntry<User>>();
            mockEntry.Setup(e => e.Entity).Returns(user);
            mockEntry.Setup(e => e.State).Returns(EntityState.Unchanged);
            return mockEntry.Object;
        }
    }
}
