namespace HSoft.SocialSchool.Domain.Enumerations
{
    /// <summary>
    /// Tipo de una cuenta. Es inmutable: se fija al crear la cuenta y no puede cambiar.
    /// </summary>
    public enum AccountType
    {
        /// <summary>Deportista: practica deportes, declara maniobras y recibe puntuaciones.</summary>
        Athlete = 0,

        /// <summary>Escuela de deporte: asociada a un único deporte, ofrece servicios y puntúa a sus asistentes.</summary>
        School = 1
    }
}
