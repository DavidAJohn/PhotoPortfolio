using PhotoPortfolio.Shared.Entities;

namespace PhotoPortfolio.Shared.Models;

public class ModalConfirmResult
{
    public Photo Photo { get; set; } = null!;
    public LightboxConfirmOptions Button { get; set; } = null!;
}
