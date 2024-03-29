﻿using System;
using System.Diagnostics;

namespace EntityFrameworkCore.DbContextBackedMock.Moq.Tests
{
    [DebuggerDisplay("{nameof(Id)}: {Id}")]
    public class TestEntity2
    {
        public TestEntity2() { }

        public TestEntity2(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
    }
}