using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.ServiceContracts;
using Core.Services;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.Services
{
    public class SubscriptionServiceTests
    {
        private readonly Mock<ISubscriptionRepository> _subscriptionRepositoryMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly SubscriptionService _subscriptionService;

        public SubscriptionServiceTests()
        {
            _subscriptionRepositoryMock = new Mock<ISubscriptionRepository>();
            _cacheServiceMock = new Mock<ICacheService>();

            _subscriptionService = new SubscriptionService(
                _subscriptionRepositoryMock.Object,
                _cacheServiceMock.Object);
        }

        [Fact]
        public async Task GetSubscriptionAsync_WhenCacheHit_ReturnsFromCacheWithoutHittingDatabase()
        {
            var tenantId = Guid.NewGuid();
            var cacheKey = $"subscription:{tenantId}";

            var cachedDto = new SubscriptionCacheDto
            {
                PlanTier = "Pro",
                MaxEmployees = 25,
                GitHubEnabled = true
            };

            // فاكر ده بالظبط "المكتب اللي في الغرفة"؟ الكاش موجود فيه القيمة بالفعل
            _cacheServiceMock
                .Setup(c => c.GetAsync<SubscriptionCacheDto>(cacheKey))
                .ReturnsAsync(cachedDto);

            var result = await _subscriptionService.GetSubscriptionAsync(tenantId);

            result.Should().BeEquivalentTo(cachedDto);

            // التأكيد الأهم: الداتابيز ما اتلمستش خالص، لأن الكاش كان كفاية
            _subscriptionRepositoryMock.Verify(r => r.GetByTenantIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetSubscriptionAsync_WhenCacheMiss_FetchesFromDatabaseAndCachesResult()
        {
            var tenantId = Guid.NewGuid();
            var cacheKey = $"subscription:{tenantId}";

            var subscriptionFromDb = new Subscription
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PlanTier = "Free",
                MaxEmployees = 5,
                GitHubEnabled = false
            };

            // فاكر "المكتبة البعيدة"؟ الكاش فاضي (null)، فالكود لازم يروح للداتابيز
            _cacheServiceMock
                .Setup(c => c.GetAsync<SubscriptionCacheDto>(cacheKey))
                .ReturnsAsync((SubscriptionCacheDto?)null);

            _subscriptionRepositoryMock
                .Setup(r => r.GetByTenantIdAsync(tenantId))
                .ReturnsAsync(subscriptionFromDb);

            var result = await _subscriptionService.GetSubscriptionAsync(tenantId);

            result.PlanTier.Should().Be("Free");
            result.MaxEmployees.Should().Be(5);

            // نتأكد إن الداتابيز فعلاً اتلمست (لأن الكاش كان فاضي)
            _subscriptionRepositoryMock.Verify(r => r.GetByTenantIdAsync(tenantId), Times.Once);

            // ونتأكد إن النتيجة اتخزنت في الكاش للمرة الجاية (فاكر "حط نسخة على مكتبك"؟)
            _cacheServiceMock.Verify(
                c => c.SetAsync(cacheKey, It.IsAny<SubscriptionCacheDto>(), It.IsAny<TimeSpan>()),
                Times.Once);
        }

        [Fact]
        public async Task GetSubscriptionAsync_WhenSubscriptionNotFoundInDatabase_ThrowsInvalidOperationException()
        {
            var tenantId = Guid.NewGuid();

            _cacheServiceMock
                .Setup(c => c.GetAsync<SubscriptionCacheDto>(It.IsAny<string>()))
                .ReturnsAsync((SubscriptionCacheDto?)null);

            _subscriptionRepositoryMock
                .Setup(r => r.GetByTenantIdAsync(tenantId))
                .ReturnsAsync((Subscription?)null);

            var act = async () => await _subscriptionService.GetSubscriptionAsync(tenantId);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("الاشتراك غير موجود.");
        }

        [Fact]
        public async Task InvalidateCacheAsync_RemovesCorrectCacheKey()
        {
            // فاكر ده أهم استخدام حقيقي شفناه بعنينا؟ لما رقّينا الباقة، الكاش القديم لازم يتمسح فوراً
            var tenantId = Guid.NewGuid();
            var expectedCacheKey = $"subscription:{tenantId}";

            await _subscriptionService.InvalidateCacheAsync(tenantId);

            _cacheServiceMock.Verify(c => c.RemoveAsync(expectedCacheKey), Times.Once);
        }
    }
}
