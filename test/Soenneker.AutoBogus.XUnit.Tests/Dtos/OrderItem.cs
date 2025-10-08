﻿using System;
using System.Collections.Generic;

namespace Soenneker.AutoBogus.XUnit.Tests.Dtos;

public sealed class OrderItem
{
    public OrderItem(Product product)
    {
        Product = product;
    }

    public Product Product { get; }
    public Quantity Quantity { get; set; }  
    public IDictionary<int, decimal> Discounts { get; set; }
    public TimeOnly MostEffectiveAt { get; set; }
    public DateOnly MostEffectiveOn { get; set; }
}