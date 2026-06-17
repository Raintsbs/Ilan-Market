using AdvertisementApp.Common.Models;

namespace AdvertisementApp.Tests.Common
{
    public class ApiResponseTests
    {
        [Fact]
        public void Ok_ShouldSetSuccessTrue()
        {
            var response = ApiResponse<string>.Ok("data", "ok");
            Assert.True(response.Success);
            Assert.Equal("data", response.Data);
        }

        [Fact]
        public void Fail_ShouldSetSuccessFalse()
        {
            var response = ApiResponse<int>.Fail("hata");
            Assert.False(response.Success);
            Assert.Equal("hata", response.Message);
        }
    }
}
