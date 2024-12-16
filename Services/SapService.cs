using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using sbm.Server.Interfaces;
using sbm.Shared.Sap;
using System.Net.Http.Headers;
using System.Text;

namespace sbm.Server.Services
{
    public class SapService : ISapService
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ConectionRequest _conection;

        public SapService(IMapper mapper, IConfiguration configuration, IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _configuration = configuration;
            _clientFactory = clientFactory;
            _httpContextAccessor = httpContextAccessor;
            _conection = new ConectionRequest()
            {
                Password = _configuration.GetSection("SapService:Passap").Get<string>(),
                UserName = _configuration.GetSection("SapService:Usap").Get<string>()
            };
        }

        public async Task<List<TSbmEntity>> RetrieveCatalogInformationAsync<TSapEntity, TSbmEntity>(string endpointPath, bool isCatalogVersion = false) where TSapEntity : class where TSbmEntity : Data.Interfaces.IEntity
        {
            var sapEntities = await RetrieveCatalogInformationAsync<TSapEntity>(endpointPath, isCatalogVersion)
                .ConfigureAwait(true);
            var sbmEntities = _mapper.Map<List<TSbmEntity>>(sapEntities);
            return sbmEntities;
        }

        public async Task<List<TSapEntity>> RetrieveCatalogInformationAsync<TSapEntity>(string endpointPath, bool isCatalogVersion = false)
        {
            var response = await CallSapApiAsync<List<TSapEntity>>(endpointPath, null, isCatalogVersion)
                .ConfigureAwait(true);
            return response;
        }

        #region Private Methods

        /// <summary>
        /// Llama al API de SAP especificando el endpoint y su método HTTP. Si el método HTTP no es indicado, por defecto será DELETE.
        /// </summary>
        /// <param name="relativeUri">Ruta relativa del endpoint.</param>
        /// <param name="httpMethod">Método HTTP (GET, POST, PATCH ó DELETE).</param>
        /// <returns></returns>
        public async Task CallSapApiAsync(string relativeUri, HttpMethod httpMethod = null, bool isCatalogVersion = false)
        {
            await CallSapApiAsync<object, object>(relativeUri, httpMethod ?? HttpMethod.Delete, isCatalogVersion).ConfigureAwait(true);
        }

        /// <summary>
        /// Llama al API de SAP especificando el endpoint y su método HTTP. Si el método HTTP no es indicado, por defecto será GET.
        /// </summary>
        /// <typeparam name="TOutput">Tipo de objeto a devolver.</typeparam>
        /// <param name="relativeUri">Ruta relativa del endpoint.</param>
        /// <param name="httpMethod">Método HTTP (GET, POST, PATCH ó DELETE).</param>
        /// <returns>Un objeto de tipo <typeparamref name="TOutput"/>.</returns>
        public async Task<TOutput> CallSapApiAsync<TOutput>(string relativeUri, HttpMethod httpMethod = null, bool isCatalogVersion = false)
            where TOutput : class
        {
            var response = await CallSapApiAsync<object, TOutput>(relativeUri, httpMethod ?? HttpMethod.Get, isCatalogVersion).ConfigureAwait(true);
            return response;
        }

        /// <summary>
        /// Llama al API de SAP especificando el endpoint, su método HTTP y el cuerpo de la solicitud.
        /// </summary>
        /// <typeparam name="TInput">Tipo de objeto del cuerpo que será serializado en la solicitud.</typeparam>
        /// <param name="relativeUri">Ruta relativa del endpoint.</param>
        /// <param name="httpMethod">Método HTTP (GET, POST, PATCH ó DELETE).</param>
        /// <param name="body">Un objeto de tipo <typeparamref name="TInput"/> que contiene el mensaje será serializado en la solicitud.</param>
        /// <returns></returns>
        public async Task CallSapApiAsync<TInput>(string relativeUri, HttpMethod httpMethod, TInput body, bool isCatalogVersion = false)
            where TInput : class
        {
            await CallSapApiAsync<TInput, object>(relativeUri, httpMethod, isCatalogVersion, body).ConfigureAwait(true);
        }

        /// <summary>
        /// Llama al API de SAP especificando el endpoint, su método HTTP y el cuerpo de la solicitud.
        /// </summary>
        /// <typeparam name="TInput">Tipo de objeto del cuerpo que será serializado en la solicitud.</typeparam>
        /// <typeparam name="TOutput">Tipo de objeto a devolver.</typeparam>
        /// <param name="relativeUri">Ruta relativa del endpoint.</param>
        /// <param name="httpMethod">Método HTTP (GET, POST, PATCH ó DELETE).</param>
        /// <param name="body">Un objeto de tipo <typeparamref name="TInput"/> que contiene el mensaje será serializado en la solicitud.</param>
        /// <param name="isCatalogVersion">Valida si es un catalogo de versiones que contiene el mensaje será serializado en la solicitud.</param>
        /// <returns>Un objeto de tipo <typeparamref name="TOutput"/>.</returns>
        /// <exception cref="HttpException"></exception>
        public async Task<TOutput> CallSapApiAsync<TInput, TOutput>(string relativeUri, HttpMethod httpMethod, bool isCatalogVersion, TInput body = null)
            where TInput : class where TOutput : class
        {
            try
            {
                var client = _clientFactory.CreateClient("SapService");

                #region Iniciar Sesión
                var endpointUriIni = new Uri(EndPointPath.CreateConnection, UriKind.Relative);
                var jsonRequestIni = JsonConvert.SerializeObject(_conection);

                using var contentIni = new StringContent(jsonRequestIni, Encoding.UTF8, "application/json");
                using var requestMessageIni = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = endpointUriIni,
                    Content = contentIni
                };
                using var responseMessageIni = await client.SendAsync(requestMessageIni).ConfigureAwait(true);
                if (!responseMessageIni.IsSuccessStatusCode)
                {
                    throw new Exception("SAP: No autorizado", new Exception());
                }
                var dataIni = await responseMessageIni.Content.ReadAsStringAsync().ConfigureAwait(true);
                var responseIni = JsonConvert.DeserializeObject<ConectionResponse>(dataIni ?? string.Empty);
                #endregion

                #region Request URI
                var endpointUri = new Uri(relativeUri, UriKind.Relative);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", responseIni.token.token);
                #endregion

                var jsonRequest = JsonConvert.SerializeObject(body);
                using var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                using var requestMessage = new HttpRequestMessage
                {
                    Method = httpMethod,
                    RequestUri = endpointUri,
                    Content = content
                };
                using var responseMessage = await client.SendAsync(requestMessage).ConfigureAwait(true);

                if (!responseMessage.IsSuccessStatusCode)
                {
                    throw new Exception("SAP: Error al obtener los datos", new Exception());
                }
                var data = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(true);

                if (isCatalogVersion)
                {
                    var responseCatalog = JsonConvert.DeserializeObject<TOutput>(data ?? string.Empty);
                    return responseCatalog;
                }
                else
                {
                    var response = JsonConvert.DeserializeObject<ResponseBase<TOutput>>(data ?? string.Empty);
                    return response.Value;
                }

            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo conectar a SAP", new Exception());
            }
        }

        #endregion

    }
}
