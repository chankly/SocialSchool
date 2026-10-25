namespace HSoft.SocialSchool.Web.Components.Shared;

/// <summary>Matiz visual de avisos, etiquetas y estados.</summary>
public enum UiTone
{
    /// <summary>Neutro.</summary>
    Neutral = 0,

    /// <summary>Color de marca.</summary>
    Brand = 1,

    /// <summary>Éxito o cuenta activa.</summary>
    Success = 2,

    /// <summary>Advertencia o pendiente de validación.</summary>
    Warning = 3,

    /// <summary>Error o cuenta eliminada.</summary>
    Danger = 4,

    /// <summary>Información.</summary>
    Info = 5
}

/// <summary>Conversión del matiz a la clase CSS correspondiente.</summary>
public static class UiToneExtensions
{
    /// <summary>Devuelve el sufijo de clase del matiz.</summary>
    public static string ToCss(this UiTone tone) => tone switch
    {
        UiTone.Brand => "brand",
        UiTone.Success => "success",
        UiTone.Warning => "warning",
        UiTone.Danger => "danger",
        UiTone.Info => "info",
        _ => "neutral"
    };
}
