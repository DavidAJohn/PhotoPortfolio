﻿using PhotoPortfolio.Shared.Models.Prodigi.Common;

namespace PhotoPortfolio.Shared.Models.Prodigi.Products;

public class ProductDetails
{
    public string Sku { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProductDimensions ProductDimensions { get; set; } = null!;
    public List<ProdigiAttribute> Attributes { get; set; } = null!;
    public List<bool> PrintAreas { get; set; } = null!;
    public List<Variant> Variants { get; set; } = null!;
}