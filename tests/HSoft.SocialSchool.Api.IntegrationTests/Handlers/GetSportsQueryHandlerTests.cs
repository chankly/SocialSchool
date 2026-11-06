using HSoft.SocialSchool.Application.Queries.Sports;
using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Domain.Entities.Sports;
using HSoft.SocialSchool.Domain.Primitives;
using Xunit;

namespace HSoft.SocialSchool.Api.IntegrationTests.Handlers
{
    /// <summary>
    /// El catálogo del formulario: publica lo que devuelve el repositorio como DTO, sin sacar la entidad de
    /// dominio de la capa de aplicación.
    /// </summary>
    public class GetSportsQueryHandlerTests
    {
        [Fact]
        public async Task Devuelve_los_deportes_con_su_identificador_y_su_nombre()
        {
            var skate = Sport.Create(Guid.NewGuid(), "Skate");
            var surf = Sport.Create(Guid.NewGuid(), "Surf");

            var result = await new GetSportsQueryHandler(new FakeSportRepository([skate, surf]))
                .Handle(new GetSportsQuery(), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Collection(
                result.Value,
                sport =>
                {
                    Assert.Equal(skate.Id, sport.Id);
                    Assert.Equal("Skate", sport.Name);
                },
                sport =>
                {
                    Assert.Equal(surf.Id, sport.Id);
                    Assert.Equal("Surf", sport.Name);
                });
        }

        [Fact]
        public async Task Un_catalogo_vacio_devuelve_una_lista_vacia()
        {
            var result = await new GetSportsQueryHandler(new FakeSportRepository([]))
                .Handle(new GetSportsQuery(), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value);
        }

        /// <summary>Los métodos que la consulta no usa fallan si se llaman, para que el test lo delate.</summary>
        private sealed class FakeSportRepository(IReadOnlyList<Sport> sports)
            : IBaseRepository<Sport>, ISportRepository
        {
            public Task<Sport?> GetById(Guid id) => throw new NotSupportedException();

            public Task<Sport?> GetByIdReadOnly(Guid id, CancellationToken cancellationToken = default) =>
                throw new NotSupportedException();

            public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
                throw new NotSupportedException();

            public Task Add(Sport entity) => throw new NotSupportedException();

            public Task AddRange(IReadOnlyList<Sport> entities) => throw new NotSupportedException();

            public Task AddRange<TEntity>(
                IReadOnlyList<TEntity> entities,
                CancellationToken cancellationToken = default) where TEntity : class =>
                throw new NotSupportedException();

            public Task Update(Sport entity) => throw new NotSupportedException();

            public Task Delete(Sport entity) => throw new NotSupportedException();

            public Task Save(Sport entity) => throw new NotSupportedException();

            public Task Save(IReadOnlyList<Sport> entities) => throw new NotSupportedException();

            public Task<IReadOnlyList<Sport>> GetActiveReadOnlyAsync(
                CancellationToken cancellationToken = default) =>
                Task.FromResult(sports);

            public Task<IReadOnlyList<Sport>> GetByIdsAsync(
                IReadOnlyList<Guid> sportIds,
                CancellationToken cancellationToken = default) =>
                throw new NotSupportedException();
        }
    }
}
