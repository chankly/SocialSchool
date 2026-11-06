using HSoft.SocialSchool.Application.Models.Schools;
using HSoft.SocialSchool.Domain.Entities.Schools;
using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Application.Repositories
{
    /// <summary>Contrato de persistencia del agregado <see cref="School"/>.</summary>
    public interface ISchoolRepository : IBaseRepository<School>
    {
        /// <summary>
        /// Devuelve el directorio de escuelas: solo cuentas de escuela activas o pendientes de validación,
        /// ordenadas por nombre y como máximo cincuenta. Los filtros son opcionales y la comparación de texto
        /// no distingue mayúsculas. El resultado es una proyección porque el nombre del deporte y el estado
        /// de la cuenta no viven en la escuela.
        /// </summary>
        /// <param name="sportId">Deporte por el que filtrar, si se indica.</param>
        /// <param name="city">Ciudad exacta por la que filtrar, si se indica.</param>
        /// <param name="search">Texto que debe contener el nombre, si se indica.</param>
        /// <param name="cancellationToken">Token de cancelación de la petición.</param>
        Task<IReadOnlyList<SchoolDirectoryItem>> GetDirectoryReadOnlyAsync(
            Guid? sportId,
            string? city,
            string? search,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Devuelve la escuela con sus servicios, o <see langword="null"/> si no existe. Los servicios van
        /// en la misma consulta porque el lazy loading está desactivado.
        /// </summary>
        /// <param name="id">Identificador de la escuela, que comparte con su cuenta.</param>
        /// <param name="cancellationToken">Token de cancelación de la petición.</param>
        Task<School?> GetByIdWithServicesReadOnlyAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Devuelve las escuelas cuyo identificador está en la lista, sin rastrear. Sirve para resolver la
        /// ciudad de las cuentas invitables sin cargar sus agregados completos.
        /// </summary>
        /// <param name="schoolIds">Identificadores que se quieren resolver.</param>
        /// <param name="cancellationToken">Token de cancelación de la petición.</param>
        Task<IReadOnlyList<School>> GetByIdsReadOnlyAsync(
            IReadOnlyList<Guid> schoolIds,
            CancellationToken cancellationToken = default);
    }
}
