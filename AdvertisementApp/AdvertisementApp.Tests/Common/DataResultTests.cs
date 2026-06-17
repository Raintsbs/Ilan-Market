using AdvertisementApp.Common.Result;

namespace AdvertisementApp.Tests.Common
{
    public class DataResultTests
    {
        [Fact]
        public void Ok_ShouldSetSuccessAndData()
        {
            var result = DataResult<string>.Ok("test", "mesaj");
            Assert.True(result.Success);
            Assert.Equal("test", result.Data);
            Assert.Equal("mesaj", result.Message);
        }

        [Fact]
        public void Fail_ShouldSetSuccessFalse()
        {
            var result = DataResult<int>.Fail("hata");
            Assert.False(result.Success);
            Assert.Equal("hata", result.Message);
        }
    }
}
