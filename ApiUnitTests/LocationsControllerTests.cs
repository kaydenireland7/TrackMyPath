using Microsoft.EntityFrameworkCore;
using Moq;

using PathAPI.Controllers;
using PathAPI.Models;

namespace ApiUnitTests
{

    public class LocationsControllerTests
    {

        private readonly Mock<TrackMyPathContext> _mockContext;
        private readonly Mock<DbSet<Location>> _mockDbSet;

        public LocationsControllerTests() { 
            _mockDbSet = new Mock<DbSet<Location>>();

            _mockContext = new Mock<TrackMyPathContext>();
            _mockContext.Setup(m => m.Locations).Returns(_mockDbSet.Object);
        }
    }
}
