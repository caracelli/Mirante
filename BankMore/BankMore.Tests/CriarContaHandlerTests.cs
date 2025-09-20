using Xunit;
using Moq;
using BankMore.Application.Handlers;
using BankMore.Application.Commands;
using BankMore.Data;
using BankMore.Domain.Models;
using System.Threading;

namespace BankMore.Tests
{
    public class CriarContaHandlerTests
    {
        [Fact]
        public async Task CriarConta_Calls_Repository()
        {
            var repoMock = new Mock<IContaRepository>();
            repoMock.Setup(x => x.InsertContaAsync(It.IsAny<ContaCorrente>())).Returns(Task.CompletedTask);

            var handler = new CriarContaHandler(repoMock.Object);
            var cmd = new CriarContaCommand("12345678901", "Fulano", "senha");

            var res = await handler.Handle(cmd, CancellationToken.None);

            Assert.NotNull(res);
            repoMock.Verify(r => r.InsertContaAsync(It.IsAny<ContaCorrente>()), Times.Once);
        }
    }
}
