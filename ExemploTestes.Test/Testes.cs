using ExemploTestes.Models;
using ExemploTestes.Services;
using ExemploTestes.Test.Mocks;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ExemploTestes.Test
{
    [Collection("TestCollection")]
    public class Testes : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly HttpClient _httpClient;
        private readonly HttpClient _httpClientService;
        private HttpClient _httpClientServiceUnit;
        private readonly JsonSerializerOptions _options;
        private readonly ViaCepService _viaCepService;
        private ViaCepService _viaCepServiceUnit;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

        public Testes(WebApplicationFactory<Startup> factory)
        {
            _httpClient = factory.CreateClient();
            _httpClientService = new HttpClient();
            _viaCepService = new ViaCepService(_httpClientService);
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            _options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                IgnoreReadOnlyProperties = true
            };
        }

        [Fact]
        public async Task TesteIntegracao_Erro()
        {
            var response = await _httpClient.GetAsync("/api/endereco/72000000");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TesteIntegracao_Ok()
        {
            var response = await _httpClient.GetAsync("/api/endereco/70701000");

            var obj = JsonSerializer.Deserialize<Endereco>(await response.Content.ReadAsStringAsync(), _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(obj);
            Assert.True(obj is Endereco);
        }

        [Theory]
        [InlineData("70701000")]
        public async Task TesteIntegracao_Ok_Varias_Opcoes_NaLinha(string cep)
        {
            var response = await _httpClient.GetAsync($"/api/endereco/{cep}");

            var obj = JsonSerializer.Deserialize<Endereco>(await response.Content.ReadAsStringAsync(), _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(obj);
            Assert.True(obj is Endereco);
        }


        [Theory]
        [MemberData(nameof(GeradorDadosCep.OpcoesCep), MemberType = typeof(GeradorDadosCep))]
        public async Task TesteIntegracao_Ok_Varias_Opcoes_Mockadas(string cep)
        {
            var response = await _httpClient.GetAsync($"/api/endereco/{cep}");

            var obj = JsonSerializer.Deserialize<Endereco>(await response.Content.ReadAsStringAsync(), _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(obj);
            Assert.True(obj is Endereco);
        }

        [Fact]
        public async Task TesteIntegracao_Service_Erro()
        {
            var endereco = await _viaCepService.BuscaCep("72000000");

            Assert.Null(endereco);
        }

        [Fact]
        public async Task TesteIntegracao_Service_Ok()
        {
            var endereco = await _viaCepService.BuscaCep("70701000");

            Assert.NotNull(endereco);
            Assert.True(endereco is Endereco);
        }

        [Fact]
        public async Task TesteUnitario_Service_Ok()
        {
            _httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new Endereco() {
                Bairro = "Bairro Teste",
                Cep = "72000000",
                Complemento = "Complemento",
                Ddd = "61",
                Gia = "",
                Ibge = "",
                Localidade = "Localidade",
                Logradouro = "Logradouro",
                Siafi = "",
                Uf = "DF"
                }))
            });

            _httpClientServiceUnit = new HttpClient(_httpMessageHandlerMock.Object);
            _viaCepServiceUnit = new ViaCepService(_httpClientServiceUnit);

            var endereco = await _viaCepServiceUnit.BuscaCep("70701000");

            Assert.NotNull(endereco);
            Assert.True(endereco is Endereco);
        }
    }
}
