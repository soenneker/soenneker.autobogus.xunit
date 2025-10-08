﻿using System;
using System.Collections.Generic;

namespace Soenneker.AutoBogus.XUnit.Tests.Dtos;

public sealed class Order
{
    public DateTime Timestamp;

    public int Id { get; set; }

    public ICalculator Calculator { get; }

    public Guid? Code { get; set; }

    public Status Status { get; set; }

    public DiscountBase[] Discounts { get; set; }

    public IEnumerable<OrderItem> Items { get; set; }

    public DateTimeOffset DateCreated { get; set; }

    public ICollection<string> Comments { get; set; }

    public Order(int id, ICalculator calculator)
    {
        Id = id;
        Calculator = calculator;
    }
}