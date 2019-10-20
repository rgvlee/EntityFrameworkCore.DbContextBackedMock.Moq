using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace EntityFrameworkCore.DbContextBackedMock.Moq.Tests
{
    [DebuggerDisplay("{nameof(Id)}: {Id}")]
    public class TestEntity1
    {
        public TestEntity1() { }

        public TestEntity1(Guid id)
        {
            Id = id;
        }

        [Key] public Guid Id { get; set; }
    }
}